using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Factors.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class BrandFactorsController : BasePluginController
    {
        #region Fields
        private readonly IBrandFactorsService _BrandFactorsService;
        private readonly IPermissionService _permissionService;
        private readonly IBrandFactorsModelFactory _BrandFactorsModelFactory;
        protected readonly IWorkContext _workContext;
        protected readonly ICustomerService _customerService;
        #endregion

        #region Ctor
        public BrandFactorsController(
            IBrandFactorsService BrandFactorsService,
            IPermissionService permissionService,
            IBrandFactorsModelFactory BrandFactorsModelFactory,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _BrandFactorsService = BrandFactorsService;
            _permissionService = permissionService;
            _BrandFactorsModelFactory = BrandFactorsModelFactory;
            _workContext = workContext;
            _customerService = customerService;
        }
        #endregion

        #region Helpers

        private async Task<bool> IsAdminAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var roleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            return roleIds.Contains(1);
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await IsAdminAsync())
                return AccessDeniedView();

            var model = await _BrandFactorsModelFactory.PrepareBrandFactorsSearchModelAsync(new BrandsFactorsSearchModel());
            return View("~/Plugins/Nop.Plugin.Factors/Views/BrandFactors/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(BrandsFactorsSearchModel searchModel)
        {
            if (!await IsAdminAsync())
                return Json(new { error = "Access denied." }); 

            var model = await _BrandFactorsModelFactory.PrepareBrandFactorsListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateFactor([FromForm] int id, [FromForm] decimal factor)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = $"Invalid ID: {id}. ID must be greater than 0." });

                var brandFactor = await _BrandFactorsService.GetByIdAsync(id);
                if (brandFactor == null)
                    return Json(new { success = false, message = $"Record with ID {id} not found." });

                brandFactor.Factor = factor;
                await _BrandFactorsService.UpdateAsync(brandFactor);
                return Json(new { success = true, message = "Factor updated successfully" });
            }
            catch (System.Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating the factor." });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminAsync())
                return AccessDeniedView();

            var brandFactor = await _BrandFactorsService.GetByIdAsync(id);
            if (brandFactor != null)
                await _BrandFactorsService.DeleteAsync(brandFactor);

            return RedirectToAction("List");
        }

        #endregion
    }
}