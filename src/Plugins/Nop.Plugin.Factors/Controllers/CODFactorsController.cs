using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Factors.Domain;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Factors.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class CODFactorsController : BasePluginController
    {
        #region Fields
        private readonly ICODFactorsService _codFactorsService;
        private readonly IWorkContext _workContext;
        private readonly ICountryService _countryService;
        private readonly IPermissionService _permissionService;
        private readonly ICODFactorsModelFactory _codFactorsModelFactory;
       
        protected readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public CODFactorsController(
            ICODFactorsService codFactorsService,
            IWorkContext workContext,
            ICountryService countryService,
            IPermissionService permissionService,
            ICODFactorsModelFactory codFactorsModelFactory,   ICustomerService customerService
            )
        {
            _codFactorsService = codFactorsService;
            _workContext = workContext;
            _countryService = countryService;
            _permissionService = permissionService;
            _codFactorsModelFactory = codFactorsModelFactory;
            _customerService = customerService;
        }
        #endregion

        public virtual async Task<IActionResult> List()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var CurrentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            if (!CurrentCustomerRoleIds.Contains(1))
                return AccessDeniedView();

            //prepare model
            var model = await _codFactorsModelFactory.PrepareCODFactorsSearchModelAsync(new CODFactorsSearchModel());

            return View("~/Plugins/Nop.Plugin.Factors/Views/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CODFactorsSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var CurrentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            if (!CurrentCustomerRoleIds.Contains(1))
                return Json(new { error = "Access denied." });

            //prepare model
            var model = await _codFactorsModelFactory.PrepareCODFactorsListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateFactor([FromForm] int id, [FromForm] decimal codFactor)
        {
            try
            {
               
               

                //if (id <= 0)
                //{
                //    System.Diagnostics.Debug.WriteLine($"Invalid ID received: {id}");
                //    return Json(new { success = false, message = $"Invalid ID: {id}. ID must be greater than 0." });
                //}

                var codFactors = await _codFactorsService.GetByIdAsync(id);
                if (codFactors == null)
                {
                    
                    return Json(new { success = false, message = $"Record with ID {id} not found." });
                }

                codFactors.CODFactor = codFactor;
                await _codFactorsService.UpdateAsync(codFactors);

               
                return Json(new { success = true, message = "Factor updated successfully" });
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = "An error occurred while updating the factor." });
            }
        }
       

     

      
    

    }
}