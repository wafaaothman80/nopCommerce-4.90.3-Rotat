using Nop.Core;

namespace Nop.Plugin.Misc.CloudflareImages.Domain;

public class CloudflareImagesImportLog : BaseEntity
{
    public int? ProductId { get; set; }
    public string OrgName { get; set; }
    public string ImageRole { get; set; }
    public string CloudflareImageId { get; set; }
    public string Status { get; set; }
    public string ErrorMessage { get; set; }
    public int ImagesDeleted { get; set; }
    public int ImagesInserted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}
