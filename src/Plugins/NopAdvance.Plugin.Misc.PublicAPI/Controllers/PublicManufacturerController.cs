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
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Manufacturer methods
/// </summary>
public partial class PublicManufacturerController : BaseAPIController
{
    #region Fields

    private readonly ICatalogModelFactory _catalogModelFactory;
    private readonly IManufacturerService _manufacturerService;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWebHelper _webHelper;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public PublicManufacturerController(ICatalogModelFactory catalogModelFactory,
        IManufacturerService manufacturerService,
        IAclService aclService,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        IWebHelper webHelper,
        IStoreContext storeContext,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService)
    {
        _catalogModelFactory = catalogModelFactory;
        _manufacturerService = manufacturerService;
        _aclService = aclService;
        _storeMappingService = storeMappingService;
        _permissionService = permissionService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _webHelper = webHelper;
        _storeContext = storeContext;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<bool> CheckManufacturerAvailabilityAsync(Manufacturer manufacturer)
    {
        var isAvailable = true;

        if (manufacturer == null || manufacturer.Deleted)
            isAvailable = false;

        var notAvailable =
            //published?
            !manufacturer.Published ||
            //ACL (access control list) 
            !await _aclService.AuthorizeAsync(manufacturer) ||
            //Store mapping
            !await _storeMappingService.AuthorizeAsync(manufacturer);
        //Check whether the current user has a "Manage categories" permission (usually a store owner)
        //We should allows him (her) to use "Preview" functionality
        var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE);
        if (notAvailable && !hasAdminAccess)
            isAvailable = false;

        return isAvailable;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare the manufacturer model
    /// </summary>
    /// <param name="manufacturerId">The manufacturer identifier</param>
    [HttpGet("{manufacturerId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ManufacturerModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetManufacturer(int manufacturerId, [FromQuery] CatalogRequest request)
    {
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

        if (!await CheckManufacturerAvailabilityAsync(manufacturer))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(manufacturer)));

        //'Continue shopping' URL
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.LastContinueShoppingPageAttribute,
            _webHelper.GetThisPageUrl(false),
            (await _storeContext.GetCurrentStoreAsync()).Id);

        //activity log
        await _customerActivityService.InsertActivityAsync("PublicStore.ViewManufacturer",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name), manufacturer);

        //model
        var command = new CatalogProductsCommand();
        if (request != null)
            command = new CatalogProductsCommand
            {
                Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                Specs = request.SpecificationOptionIds != null ? request.SpecificationOptionIds : new List<int>(),
                Ms = request.ManufacturerIds != null ? request.ManufacturerIds : new List<int>(),
                OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        var model = await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer, command);

        return Ok(model);
    }

    /// <summary>
    /// Get all manufacturers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ManufacturerModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetManufacturers()
    {
        var model = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();

        return Ok(model);
    }

    /// <summary>
    /// Get manufacturer products
    /// </summary>
    /// <param name="manufacturerId">The manufacturer identifier</param>
    [HttpGet("{manufacturerId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CatalogProductsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetManufacturerProducts(int manufacturerId, [FromQuery] CatalogRequest request)
    {
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

        if (!await CheckManufacturerAvailabilityAsync(manufacturer))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(manufacturer)));

        var command = new CatalogProductsCommand();
        if (request != null)
            command = new CatalogProductsCommand
            {
                Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                Specs = request.SpecificationOptionIds != null ? request.SpecificationOptionIds : new List<int>(),
                Ms = request.ManufacturerIds != null ? request.ManufacturerIds : new List<int>(),
                OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        var model = await _catalogModelFactory.PrepareManufacturerProductsModelAsync(manufacturer, command);

        return Ok(model);
    }

    #endregion
}
