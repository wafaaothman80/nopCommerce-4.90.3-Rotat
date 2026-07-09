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
using Nop.Plugin.AccountManager.Factories;
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

namespace Nop.Plugin.Rigion.Factories;

/// <summary>
/// Represents the customer model factory implementation
/// </summary>
public partial class RigionModelFactory : IRigionModelFactory
{
    #region Fields 

    protected readonly IRigionService _rigionService;

    #endregion

    #region Ctor

    public RigionModelFactory(IRigionService rigionService)
    {
        _rigionService = rigionService;

    }

    #endregion

   

    #region Methods

    
    public virtual async Task<RigionSearchModel> PrepareRigionSearchModelAsync(RigionSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        

     
        await _rigionService.PrepareModelCountryRigionAsync(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;


    }

   
    public virtual async Task<RigionListModel> PrepareRigionListModelAsync(RigionSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        //get customers
        var Rigions = await _rigionService.GetAllRigionsAsync(countryIds: searchModel.SelectedCountryIds.ToArray(),
            RigionName: searchModel.RigionName,
           


            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new RigionListModel().PrepareToGridAsync(searchModel, Rigions, () =>
        {
            return Rigions.SelectAwait(async Rigion =>
            {
                //fill in model values from the entity
                var RigionModel = Rigion.ToModel<RigionModel>();

               
                RigionModel.RigionName = Rigion.RigionName;

              RigionModel.DisplayOrder = Rigion.DisplayOrder;


                //fill in additional values (not existing in the entity)
                RigionModel.RigionCountriesNames = string.Join(", ",
                    (await _rigionService.GetCountriesAsync(Rigion)).Select(rigin => rigin.Name));
                

                return RigionModel;
            });
        });

        return model;
    }

  
    public virtual async Task<RigionModel> PrepareRigionModelAsync(RigionModel model, Nop.Plugin.AccountManager.Domain.Rigion rigion, bool excludeProperties = false)
    {
        if (rigion != null)
        {
           // fill in model values from the entity
            model ??= new RigionModel();

            

            //whether to fill in some of properties
            //if (!excludeProperties)
            //{
            model.Id = rigion.Id;
              
                model.Active = rigion.Active;
                model.RigionName = rigion.RigionName;
            model.DisplayOrder = rigion.DisplayOrder;
                model.SelectedCountryIds = await _rigionService.GetCountryIdsAsync(rigion);


         //  }
         
           

        
        }
       

        

        //set default values for the new model
        if (rigion == null)
        {
            model.Active = true;
           
        }

       

        //prepare model customer roles
        await _rigionService.PrepareModelCountryRigionAsync(model);

        //prepare available time zones
     

      

        return model;
    }









    #endregion
}