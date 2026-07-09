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
    public class CategoryFactorsController : BasePluginController
    {
        #region Fields
        private readonly ICategoryFactorsService _categoryFactorsService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryFactorsModelFactory _categoryFactorsModelFactory;
        protected readonly IWorkContext _workContext;
        protected readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public CategoryFactorsController(
            ICategoryFactorsService categoryFactorsService,
            IPermissionService permissionService,
            ICategoryFactorsModelFactory categoryFactorsModelFactory, IWorkContext workContext, ICustomerService customerService)
        {
            _categoryFactorsService = categoryFactorsService;
            _permissionService = permissionService;
            _categoryFactorsModelFactory = categoryFactorsModelFactory;
            _workContext = workContext;
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
            var model = await _categoryFactorsModelFactory.PrepareCategoryFactorsSearchModelAsync(new CategoryFactorsSearchModel());

            return View("~/Plugins/Nop.Plugin.Factors/Views/CategoryFactors/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CategoryFactorsSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var CurrentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            if (!CurrentCustomerRoleIds.Contains(1))
                return Json(new { error = "Access denied." });

            //prepare model
            var model = await _categoryFactorsModelFactory.PrepareCategoryFactorsListModelAsync(searchModel);

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

                var categoryFactor = await _categoryFactorsService.GetByIdAsync(id);
                if (categoryFactor == null)
                {
                    return Json(new { success = false, message = $"Record with ID {id} not found." });
                }

                categoryFactor.Factor = factor;
                await _categoryFactorsService.UpdateAsync(categoryFactor);

                return Json(new { success = true, message = "Factor updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the factor." });
            }
        }

       
    }
}