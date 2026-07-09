namespace Nop.Plugin.Misc.CloudflareImages.Models;

public class ReplaceProductImagesRequest
{
    public int ProductId { get; set; }
    public List<ImportImageItem> Images { get; set; } = new();
}

public class ImportImageItem
{
    public string OrgName { get; set; }
    public string ProductNameForUrl { get; set; }
    public string ImageRole { get; set; }
    public string CloudflareImageId { get; set; }
    public string AltText { get; set; }
}

// Batch: multiple products in one call
public class ImportBatchRequest
{
    public List<ReplaceProductImagesRequest> Products { get; set; } = new();
}
