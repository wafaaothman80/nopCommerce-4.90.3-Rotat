using Nop.Core;

namespace Nop.Plugin.Misc.CloudflareImages.Domain;

/// <summary>
/// Represents a Cloudflare Images
/// </summary>
public class CloudflareImages : BaseEntity
{
    /// <summary>
    /// Gets or sets the Picture.Id this record belongs to (null for legacy rows)
    /// </summary>
    public int? PictureId { get; set; }

    /// <summary>
    /// Gets or sets the thumb file name
    /// </summary>
    public string ThumbFileName { get; set; }

    /// <summary>
    /// Gets or sets the cloudflare image identifier
    /// </summary>
    public string ImageId { get; set; }

    /// <summary>
    /// True once requireSignedURLs + allowed_variant metadata have been applied on Cloudflare.
    /// Used so bulk protection can resume without re-processing (and re-keying) done images.
    /// </summary>
    public bool IsProtected { get; set; }
}