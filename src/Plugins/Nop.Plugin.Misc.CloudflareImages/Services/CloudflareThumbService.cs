using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.CloudflareImages.Services;

/// <summary>
/// Thumb service that routes product pictures through Cloudflare Images CDN
/// and falls back to the standard local thumb service for all other picture types
/// (banners, news, logo, slider, categories, manufacturers, etc.).
/// </summary>
public partial class CloudflareThumbService : IThumbService
{
    #region Fields

    private readonly CloudflareImagesSettings _cloudflareImagesSettings;
    private readonly CloudflareImagesHttpClient _cloudflareImagesHttpClient;
    private readonly INopFileProvider _fileProvider;
    private readonly IRepository<Domain.CloudflareImages> _cloudflareImagesRepository;
    private readonly IRepository<ProductPicture> _productPictureMappingRepository;
    private readonly ThumbService _localThumbService;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public CloudflareThumbService(CloudflareImagesSettings cloudflareImagesSettings,
        CloudflareImagesHttpClient cloudflareImagesHttpClient,
        INopFileProvider fileProvider,
        IRepository<Domain.CloudflareImages> cloudflareImagesRepository,
        IRepository<ProductPicture> productPictureMappingRepository,
        ThumbService localThumbService,
        ILogger logger)
    {
        _cloudflareImagesSettings = cloudflareImagesSettings;
        _cloudflareImagesHttpClient = cloudflareImagesHttpClient;
        _fileProvider = fileProvider;
        _cloudflareImagesRepository = cloudflareImagesRepository;
        _productPictureMappingRepository = productPictureMappingRepository;
        _localThumbService = localThumbService;
        _logger = logger;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Parses the picture ID from a NopCommerce thumb filename.
    /// Format: "{pictureId:0000000}_{seoFilename}_{size}.{ext}"
    /// Returns null when the filename does not follow this pattern.
    /// </summary>
    private static int? ParsePictureIdFromThumbFileName(string thumbFileName)
    {
        if (string.IsNullOrEmpty(thumbFileName))
            return null;

        var name = Path.GetFileNameWithoutExtension(thumbFileName);
        var underscoreIndex = name.IndexOf('_');
        var prefix = underscoreIndex > 0 ? name[..underscoreIndex] : name;

        return int.TryParse(prefix, out var id) ? id : null;
    }

    /// <summary>
    /// Returns true only when the given picture ID exists in Product_Picture_Mapping.
    /// All other picture types (logo, slider, banner, news, category, etc.) return false.
    /// </summary>
    private async Task<bool> IsProductPictureAsync(int pictureId)
    {
        return await _productPictureMappingRepository.Table
            .AnyAsync(m => m.PictureId == pictureId);
    }

    /// <summary>
    /// Parses picture ID from filename and checks Product_Picture_Mapping.
    /// Returns null if the filename cannot be parsed.
    /// </summary>
    private async Task<bool> IsProductPictureByFileNameAsync(string thumbFileName)
    {
        var pictureId = ParsePictureIdFromThumbFileName(thumbFileName);
        if (pictureId == null)
            return false;

        return await IsProductPictureAsync(pictureId.Value);
    }

    private string BuildCloudflareUrl(string imageId)
    {
        var url = _cloudflareImagesSettings.DeliveryUrl
            .Replace(CloudflareImagesDefaults.ImageIdPattern, imageId)
            .Replace(CloudflareImagesDefaults.VariantNamePattern, "public");

        // Sign the URL when a signing key is configured (required for images with
        // requireSignedURLs=true; harmless for unprotected images). No expiry ->
        // permanent, cache-friendly signature over the pathname only.
        var key = _cloudflareImagesSettings.SigningKey;
        if (string.IsNullOrEmpty(key))
            return url;

        // Signature is computed over "/<account_hash>/<image_id>/<variant>" only.
        // On custom domains (images.rotat.com) the URL contains a /cdn-cgi/imagedelivery
        // prefix that must NOT be part of the signed string.
        var path = new Uri(url).AbsolutePath;
        const string customDomainPrefix = "/cdn-cgi/imagedelivery";
        if (path.StartsWith(customDomainPrefix, StringComparison.OrdinalIgnoreCase))
            path = path[customDomainPrefix.Length..];

        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
        var sig = Convert.ToHexString(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path))).ToLowerInvariant();

        return $"{url}?sig={sig}";
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task<string> GetThumbLocalPathAsync(string pictureUrl)
    {
        if (string.IsNullOrEmpty(pictureUrl))
            return string.Empty;

        var thumbFileName = _fileProvider.GetFileName(pictureUrl);

        if (!await IsProductPictureByFileNameAsync(thumbFileName))
            return await _localThumbService.GetThumbLocalPathAsync(pictureUrl);

        return await GetThumbLocalPathByFileNameAsync(thumbFileName);
    }

    /// <inheritdoc />
    public async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName)
    {
        var pictureId = ParsePictureIdFromThumbFileName(thumbFileName);

        if (pictureId == null || !await IsProductPictureAsync(pictureId.Value))
            return await _localThumbService.GeneratedThumbExistsAsync(thumbFilePath, thumbFileName);

        // Cloudflare Images serves all sizes from a single uploaded image via variants,
        // so "exists" means ANY record exists for this PictureId — not an exact filename match.
        return await _cloudflareImagesRepository.Table
            .AnyAsync(i => i.PictureId == pictureId.Value);
    }

    /// <summary>
    /// Saves the thumb to Cloudflare Images for product pictures only.
    /// Non-product pictures (logo, slider, banner, news, category, etc.)
    /// are saved to the local file system via the standard thumb service.
    /// </summary>
    public async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
    {
        var pictureId = ParsePictureIdFromThumbFileName(thumbFileName);

        // Cannot determine picture type — save locally to be safe
        if (pictureId == null)
        {
            await _localThumbService.SaveThumbAsync(thumbFilePath, thumbFileName, mimeType, binary);
            return;
        }

        var isProductPicture = await IsProductPictureAsync(pictureId.Value);

        // Non-product picture → save to local file system, not Cloudflare
        if (!isProductPicture)
        {
            await _localThumbService.SaveThumbAsync(thumbFilePath, thumbFileName, mimeType, binary);
            return;
        }

        // Already uploaded under another size — Cloudflare variants serve every size
        // from this single image, so skip re-uploading.
        if (await _cloudflareImagesRepository.Table.AnyAsync(i => i.PictureId == pictureId.Value))
            return;

        // Product picture → upload to Cloudflare Images
        try
        {
            var dataContent = new MultipartFormDataContent
            {
                { new StreamContent(new MemoryStream(binary)), "file", thumbFileName }
            };

            var response = await _cloudflareImagesHttpClient.SaveThumbAsync(dataContent);

            if (response is not { Success: true })
            {
                await _logger.ErrorAsync(
                    $"[CloudflareImages] FAILED to upload thumb '{thumbFileName}' " +
                    $"(PictureId={pictureId}): Cloudflare returned unsuccessful response.");
                return;
            }

            await _cloudflareImagesRepository.InsertAsync(new Domain.CloudflareImages
            {
                PictureId = pictureId,
                ImageId = response.Result.Id,
                ThumbFileName = thumbFileName
            }, false);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(
                $"[CloudflareImages] FAILED to upload thumb '{thumbFileName}' " +
                $"(PictureId={pictureId}): {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<string> GetThumbLocalPathByFileNameAsync(string thumbFileName)
    {
        var pictureId = ParsePictureIdFromThumbFileName(thumbFileName);

        // Non-product picture → return local URL
        if (pictureId == null || !await IsProductPictureAsync(pictureId.Value))
            return await _localThumbService.GetThumbLocalPathByFileNameAsync(thumbFileName);

        // Product picture → return Cloudflare URL if uploaded, otherwise null (local fallback).
        // Match by PictureId, not exact filename: a single upload (any size) serves every
        // size via Cloudflare's "public" variant.
        var image = await _cloudflareImagesRepository.Table
            .FirstOrDefaultAsync(i => i.PictureId == pictureId.Value);

        if (image == null)
            return null;

        return BuildCloudflareUrl(image.ImageId);
    }

    /// <inheritdoc />
    public async Task<string> GetThumbUrlAsync(string thumbFileName, string storeLocation = null)
    {
        // Non-product picture → return local URL
        if (!await IsProductPictureByFileNameAsync(thumbFileName))
            return await _localThumbService.GetThumbUrlAsync(thumbFileName, storeLocation);

        return await GetThumbLocalPathByFileNameAsync(thumbFileName);
    }

    /// <inheritdoc />
    public async Task DeletePictureThumbsAsync(Picture picture)
    {
        // Nop also calls this when a picture is merely RENAMED (SeoFilename change on
        // product save in admin). In that case the product mapping still exists - keep
        // the Cloudflare images, otherwise they are lost forever (no local binary
        // remains to regenerate them). Real deletions remove Product_Picture_Mapping first.
        if (await IsProductPictureAsync(picture.Id))
        {
            await _localThumbService.DeletePictureThumbsAsync(picture);
            return;
        }

        // Delete from Cloudflare Images if it was a product picture
        var cfItems = await _cloudflareImagesRepository.Table
            .Where(i => i.PictureId == picture.Id)
            .ToListAsync();

        if (cfItems.Any())
        {
            foreach (var item in cfItems)
                await _cloudflareImagesHttpClient.DeleteThumbAsync(item.ImageId);

            await _cloudflareImagesRepository.DeleteAsync(cfItems, false);
        }

        // Also delete local thumbs (covers non-product pictures and any local fallbacks)
        await _localThumbService.DeletePictureThumbsAsync(picture);
    }

    #endregion
}
