using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.CloudflareImages.Domain;

namespace Nop.Plugin.Misc.CloudflareImages.Data;

[NopMigration("2026-07-01 10:00:00", "Misc.CloudflareImages add ImportLog table", MigrationProcessType.Update)]
public class AddImportLogTableMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(CloudflareImagesImportLog)).Exists())
            Create.TableFor<CloudflareImagesImportLog>();
    }
}
