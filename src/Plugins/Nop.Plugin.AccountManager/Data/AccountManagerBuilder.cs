using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

using Nop.Plugin.AccountManager.Domain;

namespace Nop.Plugin.AccountManager.Data;

public class AccountManagerBuilder : NopEntityBuilder<Account_Manager>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Account_Manager.AccountManagerName))
             .AsString(400)
            
            .WithColumn(nameof(Account_Manager.Customer_Id))
            .AsInt32()
            .WithColumn(nameof(Account_Manager.ManagerStartDate))
            .AsDateTime()
            .WithColumn(nameof(Account_Manager.Active))
            .AsBoolean()
            .WithColumn(nameof(Account_Manager.Deleted))
            .AsBoolean()
             .WithColumn(nameof(Account_Manager.Email))
             .AsString(100)
              .WithColumn(nameof(Account_Manager.Phone))
             .AsString(100)
             .WithColumn(nameof(Account_Manager.ERPAccountManagerId))
             .AsInt32().WithDefaultValue(0)
            ;
    }
}