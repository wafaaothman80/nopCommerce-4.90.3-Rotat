    using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Models;


namespace Nop.Plugin.AccountManager.Mapper;

/// <summary>
/// Represents AutoMapper configuration for plugin models
/// </summary>
public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    #region Ctor

    public MapperConfiguration()
    {
        CreateMap<Account_Manager, AccountManagerModel>();

                          //.ForMember(model => model., options => options.Ignore())
                          //.ForMember(model => model.EndExecute, options => options.Ignore());

        CreateMap<AccountManagerModel, Account_Manager>();


        CreateMap<Nop.Plugin.AccountManager.Domain.Rigion, RigionModel>();
        CreateMap<RigionModel, Nop.Plugin.AccountManager.Domain.Rigion>();


    }

    #endregion

    #region Properties

    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order => 1;

    #endregion
}