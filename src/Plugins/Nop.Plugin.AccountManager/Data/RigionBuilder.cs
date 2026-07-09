using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

using Nop.Plugin.AccountManager.Domain;

namespace Nop.Plugin.AccountManager.Data;

public class RigionBuilder : NopEntityBuilder<Nop.Plugin.AccountManager.Domain.Rigion>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Nop.Plugin.AccountManager.Domain.Rigion.RigionName))
             .AsString(400)
            
            .WithColumn(nameof(Nop.Plugin.AccountManager.Domain.Rigion.RigionAddedDate))
            .AsDateTime()
           
            .WithColumn(nameof(Nop.Plugin.AccountManager.Domain.Rigion.Active))
            .AsBoolean()
 .WithColumn(nameof(Nop.Plugin.AccountManager.Domain.Rigion.DisplayOrder))
           .AsInt32 ();
    }



 


}