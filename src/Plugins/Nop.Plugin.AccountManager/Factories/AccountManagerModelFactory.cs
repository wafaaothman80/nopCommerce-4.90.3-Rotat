using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Models;
using Nop.Plugin.AccountManager.Services;
using Nop.Services.Affiliates;
using Nop.Services.Attributes;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.AccountManager.Factories;

/// <summary>
/// Represents the customer model factory implementation
/// </summary>
public partial class AccountManagerModelFactory : IAccountManagerModelFactory
{
    #region Fields 
    protected readonly IAccountManagerService _accountManagerService;

    #endregion

    #region Ctor

    public AccountManagerModelFactory(IAccountManagerService accountManagerService)
    {
        _accountManagerService = accountManagerService;

    }

    #endregion

   

    #region Methods

    
    public virtual async Task<AccountManagerSearchModel> PrepareAccountManagerSearchModelAsync(AccountManagerSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        

     
        await _accountManagerService.PrepareModelAccountManagerRiginsAsync(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;


    }

   
    public virtual async Task<AccountManagerListModel> PrepareAccountManagerListModelAsync(AccountManagerSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        //get customers
        var AccountManagers = await _accountManagerService.GetAllAccountManagersAsync(rigionIds: searchModel.SelectedRigionIds.ToArray(),
            email: searchModel.SearchEmail,
            AccountManagerName: searchModel.SearchAccountManagerName, phone: searchModel.Phone,


            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new AccountManagerListModel().PrepareToGridAsync(searchModel, AccountManagers, () =>
        {
            return AccountManagers.SelectAwait(async accountManager =>
            {
                //fill in model values from the entity
                var accountManagerModel = accountManager.ToModel<AccountManagerModel>();

               
                accountManagerModel.AccountManagerName = accountManager.AccountManagerName;

                accountManagerModel.Email = accountManager.Email;
                accountManagerModel.Phone = accountManager.Phone;


                //fill in additional values (not existing in the entity)
                accountManagerModel.AccountManagerRigionNames = string.Join(", ",
                    (await _accountManagerService.GetAccountManagerRigionsAsync(accountManager)).Select(rigin => rigin.RigionName));
                

                return accountManagerModel;
            });
        });

        return model;
    }

  
    public virtual async Task<AccountManagerModel> PrepareAccountManagerModelAsync(AccountManagerModel model, Account_Manager account_Manager, bool excludeProperties = false)
    {
        if (account_Manager != null)
        {
           // fill in model values from the entity
            model ??= new AccountManagerModel();

            model.Customer_Id = account_Manager.Customer_Id;
            

            //whether to fill in some of properties
            //if (!excludeProperties)
            //{
            model.Id = account_Manager.Id;
                model.Email = account_Manager.Email;
                model.Phone = account_Manager.Phone;
                model.Active = account_Manager.Active;
                model.AccountManagerName = account_Manager.AccountManagerName;
                model.ERPAccountManagerId = account_Manager.ERPAccountManagerId;
                model.SelectedRigionIds = await _accountManagerService.GetRigionIdsAsync(account_Manager);


         //  }
         
           

        
        }
       

        

        //set default values for the new model
        if (account_Manager == null)
        {
            model.Active = true;
           
        }

       

        //prepare model customer roles
        await _accountManagerService.PrepareModelAccountManagerRiginsAsync(model);

        //prepare available time zones
     

      

        return model;
    }









    #endregion
}