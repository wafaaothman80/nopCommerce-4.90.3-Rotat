using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Models;


namespace Nop.Plugin.AccountManager.Factories;

/// <summary>
/// Represents the customer model factory
/// </summary>
public partial interface IRigionModelFactory
{
  
    
    Task<RigionSearchModel> PrepareRigionSearchModelAsync(RigionSearchModel searchModel);


   
    Task<RigionListModel> PrepareRigionListModelAsync(RigionSearchModel searchModel);


 

    Task<RigionModel> PrepareRigionModelAsync(RigionModel model, Nop.Plugin.AccountManager.Domain.Rigion rigion, bool excludeProperties = false);










}