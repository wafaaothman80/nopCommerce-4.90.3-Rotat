namespace Nop.Plugin.Misc.CloudflareImages.Models;

public class ProtectRequest
{
    /// <summary>
    /// Optional: limit protection to a single product. Omit to protect ALL unprotected images.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Max images to process in this call (default 100). Loop until remaining = 0.
    /// </summary>
    public int? BatchSize { get; set; }
}
