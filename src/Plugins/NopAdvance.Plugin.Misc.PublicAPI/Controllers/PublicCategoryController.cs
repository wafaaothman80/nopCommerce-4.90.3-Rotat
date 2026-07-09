// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopCommerce Public RESTful API Plugin by NopAdvance team             *
// *    Copyright (c) NopAdvance LLP. All Rights Reserved.                   *
// *                                                                         *
// ***************************************************************************
// *                                                                         *
// *    This software is licensed for use under the terms accepted during    *
// *    the purchase of this product. A non-exclusive, non-transferable      *
// *    right is granted to use this product on the website for which it was *
// *    licensed.                                                            *
// *                                                                         *
// *    Companies purchasing this product for their customers are permitted, *
// *    provided the use complies with the terms outlined in the EULA:       *
// *    https://store.nopadvance.com/eula.                                   *
// *                                                                         *
// *    You may not reverse engineer, decompile, modify, or distribute this  *
// *    software without explicit permission from NopAdvance LLP. Any        *
// *    violation will result in the termination of your license and may     *
// *    lead to legal action.                                                *
// *                                                                         *
// ***************************************************************************
// *    Contact: contact@nopadvance.com                                      *
// *    Website: https://nopadvance.com                                      *
// ***************************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Category methods
/// </summary>
[ApiController]
[Route("api/public/category")]
public partial class PublicCategoryController : BaseAPIController
{
    #region Fields

    private readonly ICatalogModelFactory _catalogModelFactory;
    private readonly ICommonModelFactory _commonModelFactory;

    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;
    private readonly ICategoryService _categoryService;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWebHelper _webHelper;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public PublicCategoryController(
        ICatalogModelFactory catalogModelFactory,
        ICommonModelFactory commonModelFactory,
        IAclService aclService,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService,
        ICategoryService categoryService,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        IWebHelper webHelper,
        IStoreContext storeContext,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService)
    {
        _catalogModelFactory = catalogModelFactory;
        _commonModelFactory = commonModelFactory;

        _aclService = aclService;
        _storeMappingService = storeMappingService;
        _permissionService = permissionService;
        _categoryService = categoryService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _webHelper = webHelper;
        _storeContext = storeContext;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _urlRecordService = urlRecordService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<string> GetCategorySeNameAsync(Category category)
    {
        if (category == null)
            return string.Empty;

        var language = await _workContext.GetWorkingLanguageAsync();
        // ✅ الصحيح في nop: slug من UrlRecord
        return await _urlRecordService.GetSeNameAsync(category, language.Id, returnDefaultValue: true);
    }

    protected virtual async Task<bool> CheckCategoryAvailabilityAsync(Category category)
    {
        if (category is null)
            return false;

        var isAvailable = true;

        if (category.Deleted)
            isAvailable = false;

        var notAvailable =
            //published?
            !category.Published ||
            //ACL (access control list)
            !await _aclService.AuthorizeAsync(category) ||
            //Store mapping
            !await _storeMappingService.AuthorizeAsync(category);

        // allow admin preview
        var hasAdminAccess =
            await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) &&
            await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE);

        if (notAvailable && !hasAdminAccess)
            isAvailable = false;

        return isAvailable;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get categories to be displayed on home page
    /// </summary>
    [HttpGet("homepage")]
    [ProducesResponseType(typeof(IList<CategoryModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetHomepageCategories()
    {
        var model = await _catalogModelFactory.PrepareHomepageCategoryModelsAsync();
        return Ok(model);
    }

    /// <summary>
    /// Get main top menu

    [HttpGet("topmenu")]
    [ProducesResponseType(typeof(TopMenuResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetTopMenu()
    {
        var response = new TopMenuResponse();

        // Root categories
        var roots = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(0);

        foreach (var root in roots)
        {
            if (!await CheckCategoryAvailabilityAsync(root))
                continue;

            var rootDto = new CategoryNodeDto
            {
                Id = root.Id,
                Name = root.Name,
                SeName = await GetCategorySeNameAsync(root),
            };

          
            var children = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(root.Id);
            foreach (var child in children)
            {
                if (!await CheckCategoryAvailabilityAsync(child))
                    continue;

                rootDto.Children.Add(new CategoryNodeDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    SeName = await GetCategorySeNameAsync(child),
                });
            }

            response.Categories.Add(rootDto);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get specific category (CategoryModel includes paging/filtering command)
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    [HttpGet("{categoryId:int}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CategoryModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCategory(int categoryId, [FromQuery] CatalogRequest request)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);

        if (!await CheckCategoryAvailabilityAsync(category))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(category)));

        // 'Continue shopping' URL
        await _genericAttributeService.SaveAttributeAsync(
            await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.LastContinueShoppingPageAttribute,
            _webHelper.GetThisPageUrl(false),
            (await _storeContext.GetCurrentStoreAsync()).Id);

        // activity log
        await _customerActivityService.InsertActivityAsync(
            "PublicStore.ViewCategory",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"), category.Name),
            category);

        // model
        var command = new CatalogProductsCommand();

        if (request != null)
        {
            command = new CatalogProductsCommand
            {
                Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                Specs = request.SpecificationOptionIds ?? new List<int>(),
                Ms = request.ManufacturerIds ?? new List<int>(),
                OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        }

        var model = await _catalogModelFactory.PrepareCategoryModelAsync(category, command);

        return Ok(model);
    }

    /// <summary>
    /// Get category products only
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    [HttpGet("{categoryId:int}/products")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CatalogProductsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCategoryProducts(int categoryId, [FromQuery] CatalogRequest request)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);

        if (!await CheckCategoryAvailabilityAsync(category))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(category)));

        var command = new CatalogProductsCommand();

        if (request != null)
        {
            command = new CatalogProductsCommand
            {
                Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                Specs = request.SpecificationOptionIds ?? new List<int>(),
                Ms = request.ManufacturerIds ?? new List<int>(),
                OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        }

        var model = await _catalogModelFactory.PrepareCategoryProductsModelAsync(category, command);

        return Ok(model);
    }

    /// <summary>
    /// Get all root categories
    /// </summary>
    [HttpGet("root")]
    [ProducesResponseType(typeof(IList<CategorySimpleModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetRootCategories()
    {
     
        var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(0);

      
        var result = new List<CategorySimpleModel>();

        foreach (var c in categories)
        {
            if (!await CheckCategoryAvailabilityAsync(c))
                continue;

            result.Add(new CategorySimpleModel
            {
                Id = c.Id,
                Name = c.Name,
           
                SeName = await GetCategorySeNameAsync(c)
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all sub categories of the specified category
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    [HttpGet("{categoryId:int}/sub")]
    [ProducesResponseType(typeof(IList<CategorySimpleModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSubCategories(int categoryId)
    {
        var parent = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (!await CheckCategoryAvailabilityAsync(parent))
            return Ok(new List<CategorySimpleModel>());

        var children = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(categoryId);

        var result = new List<CategorySimpleModel>();

        foreach (var c in children)
        {
            if (!await CheckCategoryAvailabilityAsync(c))
                continue;

            result.Add(new CategorySimpleModel
            {
                Id = c.Id,
                Name = c.Name,
               
                SeName = await GetCategorySeNameAsync(c)
            });
        }

        return Ok(result);
    }

    #endregion
}
