using FluentMigrator;
using Nop.Core.Domain.Stores;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Mapping;

namespace Nop.Plugin.Recommendations.SimilarProducts.Migrations
{
    //[SkipMigrationOnUpdate]
    [NopMigration("2022/05/25 08:41:55:1687541", "Nop.Plugin.Recommendations.SimilarProducts schema NEW", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<FeaturesConfigurationRecord>();
            Create.TableFor<SimilarProductRecord>();

            string similarProductsTableName = nameof(SimilarProductRecord);
            new NameCompatibility().TableNames.TryGetValue(typeof(SimilarProductRecord), out similarProductsTableName);

            Create.Index($"IX_{similarProductsTableName}_ProductId_Similarity").OnTable(similarProductsTableName)
                .OnColumn(nameof(SimilarProductRecord.ProductId)).Ascending()
                .OnColumn(nameof(SimilarProductRecord.Similarity)).Descending()
                .WithOptions().NonClustered();
        }
    }
}
