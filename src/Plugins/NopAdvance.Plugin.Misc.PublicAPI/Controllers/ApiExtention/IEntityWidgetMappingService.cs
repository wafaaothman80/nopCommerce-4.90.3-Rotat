using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;

public interface IEntityWidgetMappingService
{
    Task<IList<SS_MAP_EntityWidgetMapping>> GetMappingsByEntityAsync(string entityName, int entityId);
    Task InsertMappingAsync(SS_MAP_EntityWidgetMapping mapping);
    Task DeleteMappingAsync(SS_MAP_EntityWidgetMapping mapping);


    Task<IList<SS_MAP_EntityWidgetMapping>> GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync(int EntityType, int entityId);




}
