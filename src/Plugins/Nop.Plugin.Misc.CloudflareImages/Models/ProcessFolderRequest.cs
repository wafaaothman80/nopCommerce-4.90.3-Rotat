namespace Nop.Plugin.Misc.CloudflareImages.Models;

public class ProcessFolderRequest
{
    /// <summary>
    /// Optional mapping of non-numeric filenames to product IDs.
    /// Key = filename without extension (e.g. "CB-37425625"), Value = NopCommerce ProductId (e.g. 30722)
    /// </summary>
    public Dictionary<string, int> FileMap { get; set; }

    /// <summary>
    /// Max number of PRODUCTS to process in this call (for batching large folders).
    /// 0 or null = process all. Recommended 50 for very large folders.
    /// </summary>
    public int? MaxProducts { get; set; }
}
