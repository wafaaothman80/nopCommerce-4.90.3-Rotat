using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Models;

namespace Nop.Plugin.AccountManager.Services;

/// <summary>
/// Customer service interface
/// </summary>
public partial interface IAccountManagerService
{
    #region AccountManagers
   //Task<IDictionary<int, Rigion>> GetAllAccountManagerRigionDictionaryAsync();
    Task PrepareModelAccountManagerRiginsAsync<TModel>(TModel model) where TModel : ISupportedModel;

    Task<IPagedList<Account_Manager>> GetAllAccountManagersAsync(int[] rigionIds = null,
        string email = null, string AccountManagerName = null, string phone = null,

        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);





    Task DeleteAccountManagerAsync(Account_Manager account_Manager);


    Task<Account_Manager> GetAccountManagerByIdAsync(int account_ManagerId);

    

    Task<IList<Account_Manager>> GetAccountManagersByIdsAsync(int[] accountManagerIds);



    Task InsertAccountManagerAsync(Account_Manager account_Manager);

    Task UpdateAccountManagerAsync(Account_Manager account_Manager);




    #endregion

    #region AccountManager Rigion


    Task AddrigionMappingAsync(AccountManagerRigionMapping roleMapping);

    Task RemoveRigionMappingAsync(Account_Manager accountManager, Nop.Plugin.AccountManager.Domain.Rigion rigion);

    /// <summary>
    /// Delete a customer role
    /// </summary>
    /// <param name="Rigion">Customer role</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion);



     Task<Nop.Plugin.AccountManager.Domain.Rigion> GetRigionByIdAsync(int rigionId);

    Task<int[]> GetAccountManagerRigionsIdsAsync(Account_Manager account_Manager, bool showHidden = false);

    Task<IList<Nop.Plugin.AccountManager.Domain.Rigion>> GetAccountManagerRigionsAsync(Account_Manager account_Manager, bool showHidden = false);
    Task<List<int>> GetRigionIdsAsync(Account_Manager account_Manager, bool showHidden = false);
  //  Task<IDictionary<int, Rigion>> GetAllRigionsDictionaryAsync();
    Task<IList<Nop.Plugin.AccountManager.Domain.Rigion>> GetRigionsAsync(Account_Manager account_Manager, bool showHidden = false);

    
    IList<Nop.Plugin.AccountManager.Domain.Rigion> GetAllRigionsAsync(bool showHidden = false);

   
    Task InsertRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion);


    Task UpdateRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion);
    Task RemoverigionMappingAsync(Account_Manager accountManager, Nop.Plugin.AccountManager.Domain.Rigion rigion);
    Task<Account_Manager> GetAccountManagerByEmailAsync(string email);

    /// <summary>
    /// Finds a nopCommerce customer by their ERPCustomerId and upserts an AccountManager_CustomerMapping row.
    /// </summary>
    Task UpsertAccountManagerCustomerMappingByERPCustomerIdAsync(int erpCustomerId, int accountManagerPluginId);

    #endregion




}