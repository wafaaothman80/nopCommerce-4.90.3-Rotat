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
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Search methods
/// </summary>
public partial class PublicSearchController : BaseAPIController
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly MediaSettings _mediaSettings;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWebHelper _webHelper;
    private readonly ICatalogModelFactory _catalogModelFactory;

    #endregion

    #region Ctor

    public PublicSearchController(CatalogSettings catalogSettings,
        MediaSettings mediaSettings,
        IProductService productService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IProductModelFactory productModelFactory,
        IGenericAttributeService genericAttributeService,
        IWebHelper webHelper,
        ICatalogModelFactory catalogModelFactory)
    {
        _catalogSettings = catalogSettings;
        _productService = productService;
        _storeContext = storeContext;
        _workContext = workContext;
        _productModelFactory = productModelFactory;
        _mediaSettings = mediaSettings;
        _genericAttributeService = genericAttributeService;
        _webHelper = webHelper;
        _catalogModelFactory = catalogModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get search term auto complete response
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    [HttpGet("{searchTerm}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<SearchTermAutoCompleteResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSearchTermAutoComplete(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term can't be empty");

        searchTerm = searchTerm.Trim();

        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < _catalogSettings.ProductSearchTermMinimumLength)
            return BadRequest("Search term's minimum length should be " + _catalogSettings.ProductSearchTermMinimumLength);

        //products
        var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
            _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

        var products = await _productService.SearchProductsAsync(0,
            storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            keywords: searchTerm,
            languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
            visibleIndividuallyOnly: true,
            pageSize: productNumber);

        var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && products.TotalCount > productNumber;

        var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete,
            _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();

        var storeUrl = _webHelper.GetStoreLocation().TrimEnd('/');
        var result = (from p in models
                      select new SearchTermAutoCompleteResponse
                      {
                          ProductId = p.Id,
                          Label = p.Name,
                          ProductURL = storeUrl + Url.RouteUrl("Product", new { p.SeName }),
                          ProductPictureURL = p.PictureModels.FirstOrDefault()?.ImageUrl,
                          ShowLinktoResultSearch = showLinkToResultSearch
                      }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Prepare search model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SearchModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSearch([FromQuery] SearchProductsRequest request)
    {
        //'Continue shopping' URL
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.LastContinueShoppingPageAttribute,
            _webHelper.GetThisPageUrl(true),
            (await _storeContext.GetCurrentStoreAsync()).Id);

        // model
        var model = new SearchModel
        {
            q = request.q,
            cid = request.cid,
            isc = request.isc,
            mid = request.mid,
            vid = request.vid,
            sid = request.sid,
            advs = request.advs,
            asv = request.asv,
        };

        //command
        var command = new CatalogProductsCommand
        {
            Price = request.Price,
            Specs = request.SpecificationOptionIds,
            Ms = request.ManufacturerIds,
            OrderBy = request.OrderBy != null ? (int)request.OrderBy : null,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        model = await _catalogModelFactory.PrepareSearchModelAsync(model, command);

        return Ok(model);
    }

    /// <summary>
    /// Search products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CatalogProductsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SearchProducts([FromQuery] SearchProductsRequest request)
    {
        // model
        var searchModel = new SearchModel
        {
            q = request.q,
            cid = request.cid,
            isc = request.isc,
            mid = request.mid,
            vid = request.vid,
            sid = request.sid,
            advs = request.advs,
            asv = request.asv,
        };

        //command
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

        var model = await _catalogModelFactory.PrepareSearchProductsModelAsync(searchModel, command);
        return Ok(model);
    }

    #endregion
}
