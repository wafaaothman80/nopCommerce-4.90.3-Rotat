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
public partial record AccountManagerSearchModel : BaseSearchModel, ISupportedModel
{
    #region Ctor

    public AccountManagerSearchModel()
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
    public string SearchEmail { get; set; }

    [NopResourceDisplayName("Admin.AccountManager.List.AccountManagerName")]
    public string SearchAccountManagerName { get; set; }

    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.AccountManager.List.Phone")]
    public string Phone { get; set; }
   


    public int Customer_Id { get; set; }


    #endregion
}
