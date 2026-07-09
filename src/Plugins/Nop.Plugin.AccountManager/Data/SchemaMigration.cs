using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.AccountManager.Domain;

namespace Nop.Plugin.AccountManager.Data;

[NopMigration("2024/11/10 09:40:55:1687541", "Nop.Plugin.AccountManager and Rigions base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration

{
    
    public override void Up()
    {
        Create.TableFor<Account_Manager>();
        Create.TableFor<Nop.Plugin.AccountManager.Domain.Rigion>();
        Create.TableFor<AccountManagerRigionMapping>();
        Create.TableFor<CountryRigionMapping>();
    }
   
}