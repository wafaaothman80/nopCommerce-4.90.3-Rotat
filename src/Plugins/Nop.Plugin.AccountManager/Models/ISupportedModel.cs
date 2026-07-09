using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.AccountManager.Models;

/// <summary>
/// Represents a model which supports access control list (ACL)
/// </summary>
public partial interface ISupportedModel
{
    #region Properties

    /// <summary>
    /// Gets or sets identifiers of the selected customer roles
    /// </summary>
    IList<int> SelectedRigionIds { get; set; }

    /// <summary>
    /// Gets or sets items for the all available customer roles
    /// </summary>
    IList<SelectListItem> AvailableAccountManagerRigions { get; set; }

    #endregion
}