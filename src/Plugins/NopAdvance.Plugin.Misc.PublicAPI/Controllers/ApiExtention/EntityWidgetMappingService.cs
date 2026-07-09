using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;

public class EntityWidgetMappingService : IEntityWidgetMappingService
{
    private readonly IRepository<SS_MAP_EntityWidgetMapping> _mappingRepository;


    public EntityWidgetMappingService(IRepository<SS_MAP_EntityWidgetMapping> mappingRepository)
    {
        _mappingRepository = mappingRepository;
    }



    public Task DeleteMappingAsync(SS_MAP_EntityWidgetMapping mapping)
    {
        throw new System.NotImplementedException();
    }

    public async Task<IList<SS_MAP_EntityWidgetMapping>> GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync(int EntityType, int entityId)
    {
        return await  _mappingRepository.Table
             .Where(mapping => mapping.EntityType == EntityType && mapping.EntityId == entityId)
             .ToListAsync();
    }

    public Task<IList<SS_MAP_EntityWidgetMapping>> GetMappingsByEntityAsync(string entityName, int entityId)
    {
        throw new System.NotImplementedException();
    }

    public Task InsertMappingAsync(SS_MAP_EntityWidgetMapping mapping)
    {
        throw new System.NotImplementedException();
    }
}




