using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Models;


namespace Nop.Plugin.AccountManager.Factories;

/// <summary>
/// Represents the customer model factory
/// </summary>
public partial interface IAccountManagerModelFactory
{
    /// <summary>
    /// Prepare customer search model
    /// </summary>
    /// <param name="searchModel">Customer search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer search model
    /// </returns>
    Task<AccountManagerSearchModel> PrepareAccountManagerSearchModelAsync(AccountManagerSearchModel searchModel);

    
    Task<AccountManagerListModel> PrepareAccountManagerListModelAsync(AccountManagerSearchModel searchModel);

   
  Task<AccountManagerModel> PrepareAccountManagerModelAsync(AccountManagerModel model, Account_Manager account_Manager, bool excludeProperties = false);

   
  

  


 
   
  
   
}