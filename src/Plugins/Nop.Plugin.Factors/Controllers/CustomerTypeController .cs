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
    public class CustomerTypeController : BasePluginController
    {
        #region Fields
        private readonly ICustomerTypeService _CustomerTypeService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerTypeModelFactory _CustomerTypeModelFactory;
        protected readonly IWorkContext _workContext;
        protected readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public CustomerTypeController(
            ICustomerTypeService CustomerTypeService,
            IPermissionService permissionService,
            ICustomerTypeModelFactory CustomerTypeModelFactory, IWorkContext workContext, ICustomerService customerService)
        {
            _CustomerTypeService = CustomerTypeService;
            _permissionService = permissionService;
            _CustomerTypeModelFactory = CustomerTypeModelFactory;
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
            var model = await _CustomerTypeModelFactory.PrepareCustomerTypeSearchModelAsync(new CustomerTypeSearchModel());

            return View("~/Plugins/Nop.Plugin.Factors/Views/CustomerType/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CustomerTypeSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var CurrentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            if (!CurrentCustomerRoleIds.Contains(1))
                return Json(new { error = "Access denied." });

            //prepare model
            var model = await _CustomerTypeModelFactory.PrepareCustomerTypeListModelAsync(searchModel);

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

                var BrandFactor = await _CustomerTypeService.GetByIdAsync(id);
                if (BrandFactor == null)
                {
                    return Json(new { success = false, message = $"Record with ID {id} not found." });
                }

                BrandFactor.Factor = factor;
                await _CustomerTypeService.UpdateAsync(BrandFactor);

                return Json(new { success = true, message = "Factor updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the factor." });
            }
        }

       
    }
}