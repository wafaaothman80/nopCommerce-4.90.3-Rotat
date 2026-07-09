    using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Factors.Domain;
using Nop.Plugin.Factors.Models;


namespace Nop.Plugin.Factors.Mapper;

/// <summary>
/// Represents AutoMapper configuration for plugin models
/// </summary>
public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    #region Ctor

    public MapperConfiguration()
    {
       

        CreateMap<CODFactors, CODFactorsModel>()
           .ForMember(dest => dest.CountryName, opt => opt.Ignore()); 

        CreateMap<CODFactorsModel, CODFactors>();






    }

    #endregion

    #region Properties

    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order => 166;

    #endregion
}