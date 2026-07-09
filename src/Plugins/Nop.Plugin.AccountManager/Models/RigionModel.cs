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
public partial record RigionModel : BaseNopEntityModel, ISupportedRigionModel
{
    
    #region Ctor

    public RigionModel()
    {
        SelectedCountryIds = new List<int>();
        AvailableRigionCountries = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.SelectedCountryIds")]
    public IList<int> SelectedCountryIds { get; set; }

    public IList<SelectListItem> AvailableRigionCountries { get; set; }

 
    [NopResourceDisplayName("Admin.AccountManager.List.RigionName")]
    public string RigionName { get; set; }



    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.DisplayOrder")]
    public int DisplayOrder { get; set; }
    [NopResourceDisplayName("Admin.RigionAddedDate")]
    public DateTime RigionAddedDate { get; set; }
   
    public string RigionCountriesNames { get; set; }

    #endregion
}
