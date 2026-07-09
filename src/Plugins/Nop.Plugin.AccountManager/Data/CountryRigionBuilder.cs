using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;

using Nop.Plugin.AccountManager.Domain;

namespace Nop.Plugin.AccountManager.Data;

public class CountryRigionBuilderBuilder : NopEntityBuilder<CountryRigionMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(CountryRigionMapping), nameof(CountryRigionMapping.RigionId)))
            .AsInt32().ForeignKey<Nop.Plugin.AccountManager.Domain.Rigion>().PrimaryKey()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(CountryRigionMapping), nameof(CountryRigionMapping.CountryId)))
            .AsInt32().ForeignKey<Country>().PrimaryKey();
        ;
    }
    


}