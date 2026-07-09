using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.CloudflareImages.Data;

/// <summary>
/// Adds nullable PictureId column to CloudflareImages table so records can be
/// associated with a specific Picture row and filtered by product membership.
/// </summary>
[NopMigration("2025-06-30 12:00:00", "Misc.CloudflareImages add PictureId column", MigrationProcessType.Update)]
public class AddPictureIdMigration : AutoReversingMigration
{
    public override void Up()
    {
        var tableName = nameof(Domain.CloudflareImages);

        if (!Schema.Table(tableName).Column(nameof(Domain.CloudflareImages.PictureId)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(Domain.CloudflareImages.PictureId))
                .AsInt32().Nullable();

            Create.Index($"IX_{tableName}_PictureId")
                .OnTable(tableName)
                .OnColumn(nameof(Domain.CloudflareImages.PictureId))
                .Ascending();
        }
    }
}
