using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.AccountManager.Models;
public partial record AccountManagerModel : BaseNopEntityModel, ISupportedModel
{
    
    #region Ctor

    public AccountManagerModel()
    {
        SelectedRigionIds = new List<int>();
        AvailableAccountManagerRigions = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.AccountManager.SelectedRigionIds")]
    public IList<int> SelectedRigionIds { get; set; }

    public IList<SelectListItem> AvailableAccountManagerRigions { get; set; }

    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Admin.AccountManager.List.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Admin.AccountManager.List.AccountManagerName")]
    public string AccountManagerName { get; set; }

    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.AccountManager.List.Phone")]
    public string Phone { get; set; }
    [NopResourceDisplayName("Admin.AccountManager.ManagerStartDate")]
    public DateTime ManagerStartDate { get; set; }
    public int Customer_Id { get; set; }
    public string CustomerUserName { get; set; }
    [NopResourceDisplayName("Admin.AccountManager.AccountManagerRigionNames")]
    public string AccountManagerRigionNames { get; set; }

    [NopResourceDisplayName("Admin.AccountManager.ERPAccountManagerId")]
    public int ERPAccountManagerId { get; set; }

    #endregion
}
