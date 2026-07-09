using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.CloudflareImages.Services;

/// <summary>
/// Scheduled task that uploads existing product pictures to Cloudflare Images in batches.
/// Only pictures linked via Product_Picture_Mapping are processed; all others are ignored.
/// Run from Admin → System → Schedule Tasks.
/// </summary>
public class CloudflareImagesSyncTask : IScheduleTask
{
    #region Constants

    public const string TaskType =
        "Nop.Plugin.Misc.CloudflareImages.Services.CloudflareImagesSyncTask, Nop.Plugin.Misc.CloudflareImages";

    public const string TaskName = "Sync product pictures to Cloudflare Images";

    // Keep each run well under Cloudflare's 100s proxy timeout.
    // The scheduler runs this task repeatedly, so all pictures are processed over multiple runs.
    private const int DefaultBatchSize = 25;

    #endregion

    #region Fields

    private readonly CloudflareImagesSettings _settings;
    private readonly CloudflareImagesHttpClient _httpClient;
    private readonly IRepository<Domain.CloudflareImages> _cfRepository;
    private readonly IRepository<ProductPicture> _productPictureMappingRepository;
    private readonly IRepository<Picture> _pictureRepository;
    private readonly IPictureService _pictureService;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public CloudflareImagesSyncTask(
        CloudflareImagesSettings settings,
        CloudflareImagesHttpClient httpClient,
        IRepository<Domain.CloudflareImages> cfRepository,
        IRepository<ProductPicture> productPictureMappingRepository,
        IRepository<Picture> pictureRepository,
        IPictureService pictureService,
        ILogger logger)
    {
        _settings = settings;
        _httpClient = httpClient;
        _cfRepository = cfRepository;
        _productPictureMappingRepository = productPictureMappingRepository;
        _pictureRepository = pictureRepository;
        _pictureService = pictureService;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes one batch run: fetches up to <see cref="DefaultBatchSize"/> product pictures
    /// that are not yet in the CloudflareImages table and uploads them.
    /// </summary>
    public async Task ExecuteAsync()
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.AccessToken))
        {
            await _logger.InformationAsync(
                "[CloudflareImages] Sync task skipped: plugin is disabled or AccessToken is not configured.");
            return;
        }

        // Single query: pictures that are in Product_Picture_Mapping AND not yet in CloudflareImages.
        // Executed entirely in SQL — no large lists loaded into memory.
        var pictures = await _pictureRepository.Table
            .Where(p =>
                _productPictureMappingRepository.Table.Any(m => m.PictureId == p.Id) &&
                !_cfRepository.Table.Any(c => c.PictureId == p.Id))
            .Take(DefaultBatchSize)
            .ToListAsync();

        if (pictures.Count == 0)
        {
            await _logger.InformationAsync(
                "[CloudflareImages] Sync task: no pending product pictures — all are already uploaded or none exist.");
            return;
        }

        await _logger.InformationAsync(
            $"[CloudflareImages] Sync task starting — {pictures.Count} product picture(s) to process " +
            $"(batch limit: {DefaultBatchSize}).");

        int processed = 0;
        int failed = 0;

        foreach (var picture in pictures)
        {
            try
            {
                await ProcessPictureAsync(picture);
                processed++;
            }
            catch (Exception ex)
            {
                failed++;
                await _logger.ErrorAsync(
                    $"[CloudflareImages] FAILED PictureId={picture.Id} " +
                    $"(SeoFilename='{picture.SeoFilename}'): {ex.Message}", ex);
            }
        }

        await _logger.InformationAsync(
            $"[CloudflareImages] Sync task complete — Processed: {processed}, Failed: {failed}.");

        if (failed > 0)
            await _logger.WarningAsync(
                $"[CloudflareImages] {failed} picture(s) failed. " +
                "Search the log for '[CloudflareImages] FAILED PictureId=' to review each error.");
    }

    private async Task ProcessPictureAsync(Picture picture)
    {
        // Load the original picture binary
        var binary = await _pictureService.LoadPictureBinaryAsync(picture);

        if (binary == null || binary.Length == 0)
            throw new InvalidOperationException(
                $"[CloudflareImages] PictureId={picture.Id} has no binary data — " +
                "check that the picture file exists on disk or in the database.");

        // Derive a stable filename: {pictureId:0000000}_{seoFilename}.{ext}
        string extension = picture.MimeType switch
        {
            "image/jpeg" or "image/pjpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".bin"
        };
        var fileName = $"{picture.Id:0000000}_{picture.SeoFilename ?? "image"}{extension}";

        var dataContent = new MultipartFormDataContent
        {
            { new StreamContent(new MemoryStream(binary)), "file", fileName }
        };

        var response = await _httpClient.SaveThumbAsync(dataContent);

        if (response == null)
            throw new HttpRequestException(
                $"[CloudflareImages] Cloudflare API returned null for PictureId={picture.Id}. " +
                "Verify AccountId and AccessToken in plugin settings.");

        if (!response.Success)
        {
            var errors = string.Join("; ", response.Errors.Select(e => $"[{e.Code}] {e.Message}"));
            throw new HttpRequestException(
                $"[CloudflareImages] Cloudflare rejected upload for PictureId={picture.Id}: {errors}");
        }

        if (string.IsNullOrEmpty(response.Result?.Id))
            throw new InvalidOperationException(
                $"[CloudflareImages] Cloudflare returned empty image ID for PictureId={picture.Id}.");

        await _cfRepository.InsertAsync(new Domain.CloudflareImages
        {
            PictureId = picture.Id,
            ThumbFileName = fileName,
            ImageId = response.Result.Id
        }, false);

        await _logger.InformationAsync(
            $"[CloudflareImages] Uploaded PictureId={picture.Id} → CF ImageId={response.Result.Id}");
    }

    #endregion
}
