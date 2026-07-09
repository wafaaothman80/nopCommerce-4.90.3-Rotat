using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Models;

namespace Nop.Plugin.AccountManager.Services;

/// <summary>
/// Customer service interface
/// </summary>
public partial interface IRigionService
{
    #region Rigions
    //Task<IDictionary<int, Rigion>> GetAllAccountManagerRigionDictionaryAsync();
  
    Task PrepareModelCountryRigionAsync<TModel>(TModel model) where TModel : ISupportedRigionModel;
    Task<IPagedList<Nop.Plugin.AccountManager.Domain.Rigion>> GetAllRigionsAsync(int[] countryIds = null,
         string RigionName = null,

         int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);


    Task<Nop.Plugin.AccountManager.Domain.Rigion> GetRigionByNameAsync(string name);


    Task DeleteRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion);


    Task<Nop.Plugin.AccountManager.Domain.Rigion> GetRigionByIdAsync(int rigionId);




    Task<IList<Nop.Plugin.AccountManager.Domain.Rigion>> GetRigionsByIdsAsync(int[] rigionIds);



    Task InsertRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion);

    Task UpdateRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion);




    #endregion

    #region AccountManager Rigion


    Task AddrigionMappingAsync(CountryRigionMapping roleMapping);

    Task RemoverigionMappingAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, Country country);


    Task<List<int>> GetCountryIdsAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false);

    Task<IList<Country>> GetCountriesAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false);
    Task<int[]> GetCountryRigionsIdsAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false);
    Task<IList<Country>> GetCountryRigionsAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false);

  
    Task RemoveCountryRigionMappingAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, Country country);
    IList<Country> GetAllCountriessAsync(bool showHidden = false);



        #endregion




    }