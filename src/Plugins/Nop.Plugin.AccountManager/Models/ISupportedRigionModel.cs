using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.AccountManager.Models;

/// <summary>
/// Represents a model which supports access control list (ACL)
/// </summary>
public partial interface ISupportedRigionModel
{
    #region Properties
    
    IList<int> SelectedCountryIds { get; set; }

    
    IList<SelectListItem> AvailableRigionCountries { get; set; }

    #endregion
}