namespace Nop.Plugin.Misc.CloudflareImages.Models;

public class ImportBatchResponse
{
    public int ProductsReceived { get; set; }
    public int ProductsSucceeded { get; set; }
    public int ProductsFailed { get; set; }
    public int ImagesImported { get; set; }
    public List<ProductImportResult> Results { get; set; } = new();
}

public class ProductImportResult
{
    public int ProductId { get; set; }
    public string Status { get; set; }
    public int ImagesDeleted { get; set; }
    public int ImagesInserted { get; set; }
    public string Error { get; set; }
}

public class ImportStatusResponse
{
    public int TotalProducts { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int TotalImagesImported { get; set; }
    public DateTime? LastUpdated { get; set; }
}
