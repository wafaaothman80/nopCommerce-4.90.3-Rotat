using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.CloudflareImages.Domain;

namespace Nop.Plugin.Misc.CloudflareImages.Data;

public class CloudflareImagesImportLogBuilder : NopEntityBuilder<CloudflareImagesImportLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CloudflareImagesImportLog.ProductId)).AsInt32().Nullable().Indexed()
            .WithColumn(nameof(CloudflareImagesImportLog.OrgName)).AsString(500).NotNullable()
            .WithColumn(nameof(CloudflareImagesImportLog.ImageRole)).AsString(50).Nullable()
            .WithColumn(nameof(CloudflareImagesImportLog.CloudflareImageId)).AsString(500).Nullable()
            .WithColumn(nameof(CloudflareImagesImportLog.Status)).AsString(100).NotNullable().Indexed()
            .WithColumn(nameof(CloudflareImagesImportLog.ErrorMessage)).AsString(2000).Nullable()
            .WithColumn(nameof(CloudflareImagesImportLog.ImagesDeleted)).AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(CloudflareImagesImportLog.ImagesInserted)).AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(CloudflareImagesImportLog.StartedAt)).AsDateTime2().NotNullable()
            .WithColumn(nameof(CloudflareImagesImportLog.FinishedAt)).AsDateTime2().NotNullable();
    }
}
