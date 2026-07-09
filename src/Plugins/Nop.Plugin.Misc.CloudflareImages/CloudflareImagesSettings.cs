using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.CloudflareImages;

/// <summary>
/// Represents Cloudflare Images configuration parameters
/// </summary>
public class CloudflareImagesSettings : ISettings
{
    /// <summary>
    /// Gets a value indicating whether we should use Cloudflare Images
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a period (in seconds) before the request times out
    /// </summary>
    public int? RequestTimeout { get; set; }

    /// <summary>
    /// Gets or sets the Cloudflare Images account ID
    /// </summary>
    public string AccountId { get; set; }

    /// <summary>
    /// Gets or sets the Cloudflare Images access token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets end point for Cloudflare Images
    /// </summary>
    public string DeliveryUrl { get; set; }

    /// <summary>
    /// API key required in X-Api-Key header for the import endpoints
    /// </summary>
    public string ImportApiKey { get; set; }

    /// <summary>
    /// Physical folder on the server where product images are stored
    /// </summary>
    public string ImportImagesFolderPath { get; set; }

    /// <summary>
    /// Cloudflare Images URL signing key (Images -> Keys). When set, delivery URLs
    /// are signed (?sig=...) so images with requireSignedURLs=true keep working.
    /// </summary>
    public string SigningKey { get; set; }
}