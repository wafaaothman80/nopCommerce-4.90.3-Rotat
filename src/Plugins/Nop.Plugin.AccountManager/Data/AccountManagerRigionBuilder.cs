using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;

using Nop.Plugin.AccountManager.Domain;

namespace Nop.Plugin.AccountManager.Data;

public class AccountManagerRigionBuilder : NopEntityBuilder<AccountManagerRigionMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(AccountManagerRigionMapping), nameof(AccountManagerRigionMapping.RigionId)))
            .AsInt32().ForeignKey<Nop.Plugin.AccountManager.Domain.Rigion>().PrimaryKey()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(AccountManagerRigionMapping), nameof(AccountManagerRigionMapping.AccountManagerId)))
            .AsInt32().ForeignKey<Account_Manager>().PrimaryKey();
        ;
    }
    


}