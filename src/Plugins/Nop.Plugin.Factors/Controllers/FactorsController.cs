using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Factors.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class FactorsController : BasePluginController
    {
        #region Fields
        private readonly IFactorsService _FactorsService;
        private readonly IPermissionService _permissionService;
        private readonly IFactorsModelFactory _FactorsModelFactory;
        protected readonly IWorkContext _workContext;
        protected readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public FactorsController(
            IFactorsService FactorsService,
            IPermissionService permissionService,
            IFactorsModelFactory FactorsModelFactory, IWorkContext workContext, ICustomerService customerService)
        {
            _FactorsService = FactorsService;
            _permissionService = permissionService;
            _FactorsModelFactory = FactorsModelFactory;
            _workContext = workContext;
            _customerService = customerService;
        }
        #endregion

        public virtual async Task<IActionResult> List()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var CurrentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            if ( !CurrentCustomerRoleIds.Contains(1))
            
                return AccessDeniedView();

            //prepare model
            var model = await _FactorsModelFactory.PrepareFactorsSearchModelAsync(new FactorsSearchModel());

            return View("~/Plugins/Nop.Plugin.Factors/Views/Factors/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(FactorsSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var CurrentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            if (!CurrentCustomerRoleIds.Contains(1))
                return Json(new { error = "Access denied." });

            //prepare model
            var model = await _FactorsModelFactory.PrepareFactorsListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateFactor([FromForm] int id, [FromForm] decimal factor)
        {
            try
            {


                if (id <= 0)
                {
                    return Json(new { success = false, message = $"Invalid ID: {id}. ID must be greater than 0." });
                }

                var BrandFactor = await _FactorsService.GetByIdAsync(id);
                if (BrandFactor == null)
                {
                    return Json(new { success = false, message = $"Record with ID {id} not found." });
                }

                BrandFactor.Factor = factor;
                await _FactorsService.UpdateAsync(BrandFactor);

                return Json(new { success = true, message = "Factor updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the factor." });
            }
        }

       
    }
}