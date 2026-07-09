using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Plugin.Misc.CloudflareImages.Domain;
using Nop.Plugin.Misc.CloudflareImages.Models;
using Nop.Plugin.Misc.CloudflareImages.Services;
using Nop.Services.Configuration;
using CFImages = Nop.Plugin.Misc.CloudflareImages.Domain.CloudflareImages;

namespace Nop.Plugin.Misc.CloudflareImages.Controllers;

[IgnoreAntiforgeryToken]
public class CloudflareImagesImportController : Controller
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };

    // Prevents two ProcessFolder requests from running concurrently (causes duplicate
    // uploads and "file not found" when one request moves files the other is reading).
    private static readonly SemaphoreSlim _processFolderLock = new(1, 1);

    private readonly ISettingService _settingService;
    private readonly IRepository<Picture> _pictureRepository;
    private readonly IRepository<PictureBinary> _pictureBinaryRepository;
    private readonly IRepository<ProductPicture> _productPictureRepository;
    private readonly IRepository<CFImages> _cfRepository;
    private readonly IRepository<CloudflareImagesImportLog> _logRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly CloudflareImagesHttpClient _cloudflareHttpClient;
    private readonly Nop.Services.Seo.IUrlRecordService _urlRecordService;

    public CloudflareImagesImportController(
        ISettingService settingService,
        IRepository<Picture> pictureRepository,
        IRepository<PictureBinary> pictureBinaryRepository,
        IRepository<ProductPicture> productPictureRepository,
        IRepository<CFImages> cfRepository,
        IRepository<CloudflareImagesImportLog> logRepository,
        IRepository<Product> productRepository,
        CloudflareImagesHttpClient cloudflareHttpClient,
        Nop.Services.Seo.IUrlRecordService urlRecordService)
    {
        _urlRecordService = urlRecordService;
        _settingService = settingService;
        _pictureRepository = pictureRepository;
        _pictureBinaryRepository = pictureBinaryRepository;
        _productPictureRepository = productPictureRepository;
        _cfRepository = cfRepository;
        _logRepository = logRepository;
        _productRepository = productRepository;
        _cloudflareHttpClient = cloudflareHttpClient;
    }

    // GET /cfimport/status
    [HttpGet]
    public async Task<IActionResult> Status()
    {
        if (!await IsAuthorizedAsync())
            return Json(new { error = "Unauthorized" });

        var logs = _logRepository.Table;

        // Count CloudflareImages records still pending upload (planned paths contain "/")
        var pendingUpload = await _cfRepository.Table
            .CountAsync(c => c.ImageId.Contains("/"));

        return Json(new
        {
            totalProducts       = await logs.Select(l => l.ProductId).Distinct().CountAsync(),
            succeeded           = await logs.CountAsync(l => l.Status == "Success"),
            failed              = await logs.CountAsync(l => l.Status != "Success"),
            totalImagesImported = await logs.Where(l => l.Status == "Success").SumAsync(l => (int?)l.ImagesInserted) ?? 0,
            lastUpdated         = await logs.MaxAsync(l => (DateTime?)l.FinishedAt),
            pendingCloudflareUpload = pendingUpload
        });
    }

    // POST /cfimport/process-batch
    [HttpPost]
    public async Task<IActionResult> ProcessBatch([FromBody] ImportBatchRequest request)
    {
        if (!await IsAuthorizedAsync())
            return Json(new { error = "Unauthorized" });

        if (request?.Products == null || request.Products.Count == 0)
            return Json(new { error = "No products in request." });

        var results = new List<ProductImportResult>();
        var totalOk = 0; var totalFail = 0; var totalImages = 0;

        foreach (var productRequest in request.Products)
        {
            var result = await ReplaceProductImagesAsync(productRequest);
            results.Add(result);

            if (result.Status == "Success")
            {
                totalOk++;
                totalImages += result.ImagesInserted;
            }
            else if (result.Status != "Skipped")
                totalFail++;
        }

        return Json(new
        {
            productsReceived  = request.Products.Count,
            productsSucceeded = totalOk,
            productsFailed    = totalFail,
            imagesImported    = totalImages,
            results
        });
    }

    // POST /cfimport/sync-cloudflare?batchSize=50
    // Uploads pending images (those with planned ImageIds containing "/") to Cloudflare
    [HttpPost]
    public async Task<IActionResult> SyncCloudflare(int batchSize = 50)
    {
        if (!await IsAuthorizedAsync())
            return Json(new { error = "Unauthorized" });

        var settings = await _settingService.LoadSettingAsync<CloudflareImagesSettings>();
        var folder = settings?.ImportImagesFolderPath?.TrimEnd('\\', '/');

        if (string.IsNullOrEmpty(folder))
            return Json(new { error = "ImportImagesFolderPath not configured." });

        // Find pending records: planned paths contain "/"
        var pending = await _cfRepository.Table
            .Where(c => c.ImageId.Contains("/"))
            .OrderBy(c => c.Id)
            .Take(batchSize)
            .ToListAsync();

        var totalRemaining = await _cfRepository.Table.CountAsync(c => c.ImageId.Contains("/"));

        var uploaded = 0; var failed = 0;
        var errors = new List<string>();

        foreach (var ci in pending)
        {
            var filePath = ImageExtensions
                .Select(ext => System.IO.Path.Combine(folder, ci.ThumbFileName + ext))
                .FirstOrDefault(System.IO.File.Exists);

            if (filePath == null)
            {
                errors.Add($"File not found: {ci.ThumbFileName}");
                failed++;
                continue;
            }

            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var ext = System.IO.Path.GetExtension(filePath).ToLower();
                var mimeType = ext is ".jpg" or ".jpeg" ? "image/jpeg"
                             : ext == ".png"  ? "image/png"
                             : ext == ".webp" ? "image/webp"
                             : "image/jpeg";

                var stream  = new System.IO.MemoryStream(fileBytes);
                var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

                var form = new MultipartFormDataContent
                {
                    { content, "file", ci.ThumbFileName + ext }
                };

                var response = await _cloudflareHttpClient.SaveThumbAsync(form);

                if (response?.Success == true && !string.IsNullOrEmpty(response.Result?.Id))
                {
                    ci.ImageId = response.Result.Id;
                    await _cfRepository.UpdateAsync(ci, false);
                    uploaded++;
                }
                else
                {
                    errors.Add($"Cloudflare rejected: {ci.ThumbFileName}");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{ci.ThumbFileName}: {ex.Message}");
                failed++;
            }
        }

        return Json(new
        {
            batchProcessed = pending.Count,
            uploaded,
            failed,
            remaining = totalRemaining - uploaded,
            errors
        });
    }

    // POST /cfimport/process-folder
    // Scans images-new folder, groups files by productId (filename prefix),
    // replaces product images and uploads to Cloudflare.
    // Accepts optional body: { "fileMap": { "CB-37425625": 30722, "OTHER-CODE": 12345 } }
    [HttpPost]
    public async Task<IActionResult> ProcessFolder([FromBody] ProcessFolderRequest request = null)
    {
        if (!await IsAuthorizedAsync())
            return Json(new { error = "Unauthorized" });

        // Reject overlapping calls instead of queuing them: the client script retries
        // on timeout, and a queued duplicate would reprocess the same products.
        if (!await _processFolderLock.WaitAsync(0))
            return Json(new { error = "Busy", message = "A previous process-folder call is still running. Retry later." });

        try
        {
            return await ProcessFolderCoreAsync(request);
        }
        finally
        {
            _processFolderLock.Release();
        }
    }

    private async Task<IActionResult> ProcessFolderCoreAsync(ProcessFolderRequest request)
    {
        var settings = await _settingService.LoadSettingAsync<CloudflareImagesSettings>();
        var folder = settings?.ImportImagesFolderPath?.TrimEnd('\\', '/');

        if (string.IsNullOrEmpty(folder) || !System.IO.Directory.Exists(folder))
            return Json(new { error = $"Folder not found: {folder}" });

        // Build lookup: orgName (lowercase) → productId from provided fileMap
        var fileMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (request?.FileMap != null)
            foreach (var kv in request.FileMap)
                fileMap[kv.Key] = kv.Value;

        // Discover image files and group by productId
        var allFilePaths = System.IO.Directory.GetFiles(folder)
            .Where(f => ImageExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()))
            .ToList();

        var files = new List<(int ProductId, int DisplayOrder, string FilePath)>();
        var skipped = new List<string>();

        foreach (var filePath in allFilePaths)
        {
            var parsed = ParseImageFile(filePath);
            if (parsed.HasValue)
            {
                files.Add(parsed.Value);
                continue;
            }

            var orgName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            // Trailing "-NN" (1-2 digits) = image index, e.g. CB-32017-03 → DisplayOrder 3.
            // Long trailing numbers (CB-37425625) are part of the product code, not an index.
            var displayOrder = 0;
            var lastDash = orgName.LastIndexOf('-');
            if (lastDash > 0)
            {
                var tail = orgName[(lastDash + 1)..];
                if (tail.Length <= 2 && int.TryParse(tail, out var idx))
                    displayOrder = idx;
            }

            // 1st: check provided fileMap (try full name, then base name without the -NN index)
            var baseName = displayOrder > 0 ? orgName[..lastDash] : orgName;
            if (fileMap.TryGetValue(orgName, out var mappedProductId) ||
                fileMap.TryGetValue(baseName, out mappedProductId))
            {
                files.Add((mappedProductId, displayOrder, filePath));
                continue;
            }

            // 2nd: look up via Picture.SeoFilename (works if process-batch was previously run)
            var seoName = orgName.ToLowerInvariant();
            var seoBase = baseName.ToLowerInvariant();
            var picture = await _pictureRepository.Table
                .FirstOrDefaultAsync(p => p.SeoFilename == seoName || p.SeoFilename == seoBase);

            if (picture != null)
            {
                var mapping = await _productPictureRepository.Table
                    .FirstOrDefaultAsync(m => m.PictureId == picture.Id);
                if (mapping != null)
                {
                    files.Add((mapping.ProductId, displayOrder, filePath));
                    continue;
                }
            }

            // 3rd: look up via CloudflareImages.ThumbFileName (fallback)
            var cf = await _cfRepository.Table
                .FirstOrDefaultAsync(c => c.ThumbFileName == orgName || c.ThumbFileName == baseName);
            if (cf?.PictureId != null)
            {
                var mapping = await _productPictureRepository.Table
                    .FirstOrDefaultAsync(m => m.PictureId == cf.PictureId.Value);
                if (mapping != null)
                {
                    files.Add((mapping.ProductId, displayOrder, filePath));
                    continue;
                }
            }

            skipped.Add(orgName);
        }

        files = files.OrderBy(x => x.ProductId).ThenBy(x => x.DisplayOrder).ToList();

        if (!files.Any())
            return Json(new { error = "No image files found in folder." });

        var allGroups = files.GroupBy(f => f.ProductId).ToList();
        var totalProductsInFolder = allGroups.Count;

        // Batching: only take up to MaxProducts groups this call.
        var maxProducts = request?.MaxProducts ?? 0;
        var groups = maxProducts > 0 ? allGroups.Take(maxProducts).ToList() : allGroups;

        // Processed files are moved here so subsequent calls don't re-scan them.
        var processedFolder = System.IO.Path.Combine(folder, "_processed");
        System.IO.Directory.CreateDirectory(processedFolder);

        var results = new List<object>();
        var totalOk = 0; var totalFail = 0; var totalImages = 0;

        foreach (var group in groups)
        {
            var productId = group.Key;
            var productFiles = group.ToList();
            var startedAt = DateTime.UtcNow;

            try
            {
                // Verify product exists
                var product = await _productRepository.Table
                    .FirstOrDefaultAsync(p => p.Id == productId && !p.Deleted);
                if (product == null)
                {
                    results.Add(new { productId, status = "ProductNotFound", uploaded = 0 });
                    totalFail++;
                    await _logRepository.InsertAsync(new CloudflareImagesImportLog
                    {
                        ProductId      = productId,
                        OrgName        = string.Join("; ", productFiles.Select(f => System.IO.Path.GetFileNameWithoutExtension(f.FilePath))),
                        Status         = "ProductNotFound",
                        ImagesDeleted  = 0,
                        ImagesInserted = 0,
                        StartedAt      = startedAt,
                        FinishedAt     = DateTime.UtcNow
                    }, false);
                    continue;
                }

                // Delete existing pictures for this product
                var existingMappings = await _productPictureRepository.Table
                    .Where(m => m.ProductId == productId).ToListAsync();
                var existingPictureIds = existingMappings.Select(m => m.PictureId).ToList();
                var deletedCount = existingPictureIds.Count;

                if (existingPictureIds.Any())
                {
                    var cfToDelete = await _cfRepository.Table
                        .Where(c => c.PictureId.HasValue && existingPictureIds.Contains(c.PictureId.Value))
                        .ToListAsync();
                    if (cfToDelete.Any()) await _cfRepository.DeleteAsync(cfToDelete, false);

                    var binToDelete = await _pictureBinaryRepository.Table
                        .Where(b => existingPictureIds.Contains(b.PictureId)).ToListAsync();
                    if (binToDelete.Any()) await _pictureBinaryRepository.DeleteAsync(binToDelete, false);

                    await _productPictureRepository.DeleteAsync(existingMappings, false);

                    var picturesToDelete = await _pictureRepository.Table
                        .Where(p => existingPictureIds.Contains(p.Id)).ToListAsync();
                    if (picturesToDelete.Any()) await _pictureRepository.DeleteAsync(picturesToDelete, false);
                }

                // Upload each file to Cloudflare and insert records
                var uploaded = 0;
                var uploadErrors = new List<string>();
                var uploadedCfIds = new List<string>();
                var uploadedRoles = new List<string>();

                // SeoFilename MUST match what admin product-save will set
                // (ProductController.UpdatePictureSeoNamesAsync), otherwise saving the product
                // in admin "renames" pictures -> thumbs deleted -> Cloudflare images deleted
                // with no binary left to regenerate them.
                var productSeoName = await _urlRecordService.GetSeNameAsync(product.Name, true, false);

                // Upload all files of this product to Cloudflare IN PARALLEL (max 6 at a time),
                // then insert DB records sequentially (repositories are not thread-safe).
                var uploadThrottle = new SemaphoreSlim(6);
                var uploadTasks = productFiles.Select(async imgFile =>
                {
                    if (!System.IO.File.Exists(imgFile.FilePath))
                        return (imgFile, cfId: (string)null, mimeType: (string)null,
                                error: $"File missing (already processed?): {System.IO.Path.GetFileName(imgFile.FilePath)}");

                    var ext = System.IO.Path.GetExtension(imgFile.FilePath).ToLower();
                    var mime = ext is ".jpg" or ".jpeg" ? "image/jpeg"
                             : ext == ".png"  ? "image/png"
                             : ext == ".webp" ? "image/webp"
                             : "image/jpeg";

                    await uploadThrottle.WaitAsync();
                    try
                    {
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(imgFile.FilePath);
                        var content = new StreamContent(new System.IO.MemoryStream(fileBytes));
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
                        var form = new MultipartFormDataContent
                        {
                            { content, "file", System.IO.Path.GetFileName(imgFile.FilePath) }
                        };

                        var response = await _cloudflareHttpClient.SaveThumbAsync(form);

                        if (response?.Success != true || string.IsNullOrEmpty(response.Result?.Id))
                            return (imgFile, cfId: null, mimeType: mime,
                                    error: $"Cloudflare upload failed: {System.IO.Path.GetFileName(imgFile.FilePath)}");

                        return (imgFile, cfId: response.Result.Id, mimeType: mime, error: (string)null);
                    }
                    catch (Exception upEx)
                    {
                        return (imgFile, cfId: null, mimeType: mime,
                                error: $"{System.IO.Path.GetFileName(imgFile.FilePath)}: {upEx.Message}");
                    }
                    finally
                    {
                        uploadThrottle.Release();
                    }
                }).ToList();

                var uploadResults = await Task.WhenAll(uploadTasks);

                // Sequential DB inserts, in DisplayOrder order
                foreach (var up in uploadResults.OrderBy(u => u.imgFile.DisplayOrder))
                {
                    if (up.error != null)
                    {
                        uploadErrors.Add(up.error);
                        continue;
                    }

                    var picture = new Picture
                    {
                        MimeType     = up.mimeType,
                        SeoFilename  = productSeoName,
                        AltAttribute = string.Empty,
                        IsNew        = false,
                        VirtualPath  = null
                    };
                    await _pictureRepository.InsertAsync(picture, false);

                    await _productPictureRepository.InsertAsync(new ProductPicture
                    {
                        ProductId    = productId,
                        PictureId    = picture.Id,
                        DisplayOrder = up.imgFile.DisplayOrder
                    }, false);

                    await _cfRepository.InsertAsync(new CFImages
                    {
                        PictureId     = picture.Id,
                        ImageId       = up.cfId,
                        ThumbFileName = System.IO.Path.GetFileNameWithoutExtension(up.imgFile.FilePath)
                    }, false);

                    uploadedCfIds.Add(up.cfId);
                    uploadedRoles.Add(up.imgFile.DisplayOrder == 0 ? "main" : $"additional-{up.imgFile.DisplayOrder:00}");
                    uploaded++;
                }

                var orgNames = string.Join("; ", productFiles.Select(f => System.IO.Path.GetFileNameWithoutExtension(f.FilePath)));
                var cfIdsJoined = string.Join("; ", uploadedCfIds);
                var rolesJoined = string.Join("; ", uploadedRoles);

                totalImages += uploaded;

                if (uploadErrors.Any())
                {
                    results.Add(new { productId, status = "PartialSuccess", uploaded, errors = uploadErrors });
                    totalFail++;
                    await _logRepository.InsertAsync(new CloudflareImagesImportLog
                    {
                        ProductId         = productId,
                        OrgName           = orgNames,
                        ImageRole         = rolesJoined,
                        CloudflareImageId = cfIdsJoined,
                        Status            = "PartialSuccess",
                        ErrorMessage      = string.Join("; ", uploadErrors),
                        ImagesDeleted     = deletedCount,
                        ImagesInserted    = uploaded,
                        StartedAt         = startedAt,
                        FinishedAt        = DateTime.UtcNow
                    }, false);
                }
                else
                {
                    results.Add(new { productId, status = "Success", uploaded });
                    totalOk++;
                    await _logRepository.InsertAsync(new CloudflareImagesImportLog
                    {
                        ProductId         = productId,
                        OrgName           = orgNames,
                        ImageRole         = rolesJoined,
                        CloudflareImageId = cfIdsJoined,
                        Status            = "Success",
                        ImagesDeleted     = deletedCount,
                        ImagesInserted    = uploaded,
                        StartedAt         = startedAt,
                        FinishedAt        = DateTime.UtcNow
                    }, false);

                    // Move processed files out so subsequent batches don't re-scan them.
                    foreach (var imgFile in productFiles)
                    {
                        try
                        {
                            var dest = System.IO.Path.Combine(processedFolder, System.IO.Path.GetFileName(imgFile.FilePath));
                            if (System.IO.File.Exists(dest)) System.IO.File.Delete(dest);
                            System.IO.File.Move(imgFile.FilePath, dest);
                        }
                        catch { /* leave file in place if move fails; it'll retry next run */ }
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add(new { productId, status = "Error", error = ex.Message });
                totalFail++;
                await _logRepository.InsertAsync(new CloudflareImagesImportLog
                {
                    ProductId      = productId,
                    OrgName        = string.Join("; ", productFiles.Select(f => System.IO.Path.GetFileNameWithoutExtension(f.FilePath))),
                    Status         = "Error",
                    ErrorMessage   = ex.Message,
                    ImagesDeleted  = 0,
                    ImagesInserted = 0,
                    StartedAt      = startedAt,
                    FinishedAt     = DateTime.UtcNow
                }, false);
            }
        }

        // Products still waiting = total distinct products in folder minus the ones fully moved out.
        // (Only successful products are moved; failed/partial stay for retry.)
        var remaining = totalProductsInFolder - totalOk;

        return Json(new
        {
            productsInFolder  = totalProductsInFolder,
            productsProcessed = groups.Count,
            productsSucceeded = totalOk,
            productsFailed    = totalFail,
            imagesUploaded    = totalImages,
            remaining,
            skippedFiles      = skipped,
            results
        });
    }

    // GET /cfimport/check/{productId}
    [HttpGet]
    public async Task<IActionResult> Check(int productId)
    {
        if (!await IsAuthorizedAsync())
            return Json(new { error = "Unauthorized" });

        var imported = await _logRepository.Table
            .AnyAsync(l => l.ProductId == productId && l.Status == "Success");

        return Json(new { imported, productId });
    }

    // POST /cfimport/protect
    // Applies requireSignedURLs + allowed_variant metadata to CloudflareImages records.
    // No image files needed - works purely from the DB + Cloudflare API.
    // Body (all optional): { "productId": 53836, "batchSize": 100 }
    //   - productId omitted => processes ALL unprotected images across every product.
    // Cloudflare re-keys each image; the new id is written back to CloudflareImages.ImageId.
    private static readonly SemaphoreSlim _protectLock = new(1, 1);

    [HttpPost]
    public async Task<IActionResult> Protect([FromBody] ProtectRequest request = null)
    {
        if (!await IsAuthorizedAsync())
            return Json(new { error = "Unauthorized" });

        // Reject overlapping calls: the client retries after proxy timeouts (524) while
        // the previous batch may still be running; concurrent batches would re-key the
        // same images twice and leave orphan copies on Cloudflare.
        if (!await _protectLock.WaitAsync(0))
            return Json(new { error = "Busy", message = "A previous protect batch is still running. Retry later." });

        try
        {
            return await ProtectCoreAsync(request);
        }
        finally
        {
            _protectLock.Release();
        }
    }

    private async Task<IActionResult> ProtectCoreAsync(ProtectRequest request)
    {
        var batchSize = request?.BatchSize is > 0 ? request.BatchSize.Value : 100;

        var query = _cfRepository.Table.Where(c => !c.IsProtected);
        if (request?.ProductId is > 0)
        {
            var pictureIds = _productPictureRepository.Table
                .Where(m => m.ProductId == request.ProductId.Value)
                .Select(m => m.PictureId);
            query = query.Where(c => c.PictureId.HasValue && pictureIds.Contains(c.PictureId.Value));
        }

        var totalRemainingBefore = await query.CountAsync();
        var batch = await query.OrderBy(c => c.Id).Take(batchSize).ToListAsync();

        var ok = 0; var fail = 0;
        var errors = new List<string>();

        foreach (var cf in batch)
        {
            try
            {
                var resp = await _cloudflareHttpClient.SetSignedUrlAsync(cf.ImageId, "public");
                if (resp is { Success: true } && !string.IsNullOrEmpty(resp.Result?.Id))
                {
                    cf.ImageId = resp.Result.Id; // Cloudflare re-keyed the image
                    cf.IsProtected = true;
                    await _cfRepository.UpdateAsync(cf, false);
                    ok++;
                }
                else
                {
                    fail++;
                    errors.Add($"{cf.ThumbFileName} ({cf.ImageId}): unsuccessful response");
                }
            }
            catch (Exception ex)
            {
                fail++;
                errors.Add($"{cf.ThumbFileName} ({cf.ImageId}): {ex.Message}");
            }
        }

        return Json(new
        {
            protectedNow = ok,
            failed       = fail,
            remaining    = totalRemainingBefore - ok,
            errors
        });
    }

    // -------------------------------------------------------------------------

    private async Task<ProductImportResult> ReplaceProductImagesAsync(ReplaceProductImagesRequest request)
    {
        var started = DateTime.UtcNow;

        var alreadyDone = await _logRepository.Table
            .AnyAsync(l => l.ProductId == request.ProductId && l.Status == "Success");
        if (alreadyDone)
            return new ProductImportResult { ProductId = request.ProductId, Status = "Skipped" };

        try
        {
            var productExists = await _productRepository.Table
                .AnyAsync(p => p.Id == request.ProductId && !p.Deleted);
            if (!productExists)
            {
                await LogAsync(request.ProductId, "ProductNotFound", $"Product {request.ProductId} not found.", 0, 0, started);
                return new ProductImportResult { ProductId = request.ProductId, Status = "ProductNotFound", Error = $"Product {request.ProductId} not found." };
            }

            var settings = await _settingService.LoadSettingAsync<CloudflareImagesSettings>();
            var folder = settings?.ImportImagesFolderPath?.TrimEnd('\\', '/');
            if (!string.IsNullOrEmpty(folder))
            {
                var missing = request.Images
                    .Where(img => !ImageExtensions.Any(ext =>
                        System.IO.File.Exists(System.IO.Path.Combine(folder, img.OrgName + ext))))
                    .Select(img => img.OrgName).ToList();

                if (missing.Any())
                {
                    var err = $"Missing files: {string.Join(", ", missing)}";
                    await LogAsync(request.ProductId, "FileNotFound", err, 0, 0, started);
                    return new ProductImportResult { ProductId = request.ProductId, Status = "FileNotFound", Error = err };
                }
            }

            var existingMappings = await _productPictureRepository.Table
                .Where(m => m.ProductId == request.ProductId).ToListAsync();

            var existingPictureIds = existingMappings.Select(m => m.PictureId).ToList();
            var deleted = 0;

            if (existingPictureIds.Any())
            {
                var cfToDelete = await _cfRepository.Table
                    .Where(c => c.PictureId.HasValue && existingPictureIds.Contains(c.PictureId.Value))
                    .ToListAsync();
                if (cfToDelete.Any()) await _cfRepository.DeleteAsync(cfToDelete, false);

                var binToDelete = await _pictureBinaryRepository.Table
                    .Where(b => existingPictureIds.Contains(b.PictureId)).ToListAsync();
                if (binToDelete.Any()) await _pictureBinaryRepository.DeleteAsync(binToDelete, false);

                await _productPictureRepository.DeleteAsync(existingMappings, false);

                var picturesToDelete = await _pictureRepository.Table
                    .Where(p => existingPictureIds.Contains(p.Id)).ToListAsync();
                if (picturesToDelete.Any()) await _pictureRepository.DeleteAsync(picturesToDelete, false);

                deleted = existingPictureIds.Count;
            }

            var inserted = 0;
            foreach (var img in request.Images)
            {
                var picture = new Picture
                {
                    MimeType     = "image/jpeg",
                    SeoFilename  = SanitizeSeoFilename(img.ProductNameForUrl ?? img.OrgName),
                    AltAttribute = img.AltText ?? string.Empty,
                    IsNew        = false,
                    VirtualPath  = null
                };
                await _pictureRepository.InsertAsync(picture, false);

                await _productPictureRepository.InsertAsync(new ProductPicture
                {
                    ProductId    = request.ProductId,
                    PictureId    = picture.Id,
                    DisplayOrder = ParseDisplayOrder(img.ImageRole)
                }, false);

                // Store planned CloudflareImageId — will be replaced by real ID via /cfimport/sync-cloudflare
                await _cfRepository.InsertAsync(new CFImages
                {
                    PictureId     = picture.Id,
                    ImageId       = img.CloudflareImageId ?? $"pending/{picture.Id}",
                    ThumbFileName = img.OrgName ?? string.Empty
                }, false);

                inserted++;
            }

            await LogAsync(request.ProductId, "Success", null, deleted, inserted, started);
            return new ProductImportResult { ProductId = request.ProductId, Status = "Success", ImagesDeleted = deleted, ImagesInserted = inserted };
        }
        catch (Exception ex)
        {
            await LogAsync(request.ProductId, "DatabaseError", ex.Message, 0, 0, started);
            return new ProductImportResult { ProductId = request.ProductId, Status = "DatabaseError", Error = ex.Message };
        }
    }

    private async Task LogAsync(int productId, string status, string error, int deleted, int inserted, DateTime started)
    {
        await _logRepository.InsertAsync(new CloudflareImagesImportLog
        {
            ProductId      = productId,
            OrgName        = $"ProductId={productId}",
            Status         = status,
            ErrorMessage   = error,
            ImagesDeleted  = deleted,
            ImagesInserted = inserted,
            StartedAt      = started,
            FinishedAt     = DateTime.UtcNow
        }, false);
    }

    private async Task<bool> IsAuthorizedAsync()
    {
        var settings = await _settingService.LoadSettingAsync<CloudflareImagesSettings>();
        var key = settings?.ImportApiKey;
        if (string.IsNullOrEmpty(key)) return false;
        Request.Headers.TryGetValue("X-Api-Key", out var provided);
        return string.Equals(provided.ToString(), key, StringComparison.Ordinal);
    }

    private static (int ProductId, int DisplayOrder, string FilePath)? ParseImageFile(string filePath)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
        var parts = name.Split('-');

        if (!int.TryParse(parts[0], out var productId))
            return null; // skip non-numeric prefixes like CB-37425625

        var displayOrder = 0;
        if (parts.Length > 1 && int.TryParse(parts[^1], out var idx))
            displayOrder = idx;

        return (productId, displayOrder, filePath);
    }

    private static int ParseDisplayOrder(string imageRole)
    {
        if (string.IsNullOrEmpty(imageRole) || imageRole == "main") return 0;
        return int.TryParse(imageRole, out var n) ? n : 0;
    }

    private static string SanitizeSeoFilename(string input)
    {
        return new string(input.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
    }
}
