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
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using static Nop.Web.Controllers.BackInStockSubscriptionController;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Back in stock subscription methods
/// </summary>
public partial class PublicBackInStockSubscriptionController : BaseAPIController
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public PublicBackInStockSubscriptionController(CatalogSettings catalogSettings,
        CustomerSettings customerSettings,
        IBackInStockSubscriptionService backInStockSubscriptionService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IProductService productService,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IWorkContext workContext)
    {
        _catalogSettings = catalogSettings;
        _customerSettings = customerSettings;
        _backInStockSubscriptionService = backInStockSubscriptionService;
        _customerService = customerService;
        _localizationService = localizationService;
        _productService = productService;
        _storeContext = storeContext;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected virtual async Task<CustomerBackInStockSubscriptionsModel> GetCustomerSubscriptionsAsync(int? pageNumber)
    {
        var pageIndex = 0;
        if (pageNumber > 0)
            pageIndex = pageNumber.Value - 1;
        var pageSize = 10;

        var customer = await _workContext.GetCurrentCustomerAsync();
        var list = await _backInStockSubscriptionService.GetAllSubscriptionsByCustomerIdAsync(customer.Id,
            (await _storeContext.GetCurrentStoreAsync()).Id, pageIndex, pageSize);

        var model = new CustomerBackInStockSubscriptionsModel();

        foreach (var subscription in list)
        {
            var product = await _productService.GetProductByIdAsync(subscription.ProductId);

            if (product != null)
            {
                var subscriptionModel = new CustomerBackInStockSubscriptionsModel.BackInStockSubscriptionModel
                {
                    Id = subscription.Id,
                    ProductId = product.Id,
                    ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                };
                model.Subscriptions.Add(subscriptionModel);
            }
        }

        model.PagerModel = new PagerModel(_localizationService)
        {
            PageSize = list.PageSize,
            TotalRecords = list.TotalCount,
            PageIndex = list.PageIndex,
            ShowTotalSummary = false,
            RouteActionName = "CustomerBackInStockSubscriptions",
            UseRouteLinks = true,
            RouteValues = new BackInStockSubscriptionsRouteValues { PageNumber = pageIndex }
        };

        return model;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare back in stock subscription model
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BackInStockSubscribeModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSubscribe(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        var model = new BackInStockSubscribeModel
        {
            ProductId = product.Id,
            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            IsCurrentCustomerRegistered = await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()),
            MaximumBackInStockSubscriptions = _catalogSettings.MaximumBackInStockSubscriptions,
            CurrentNumberOfBackInStockSubscriptions = (await _backInStockSubscriptionService
            .GetAllSubscriptionsByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id, 0, 1))
            .TotalCount
        };
        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
            product.BackorderMode == BackorderMode.NoBackorders &&
            product.AllowBackInStockSubscriptions &&
            await _productService.GetTotalStockQuantityAsync(product) <= 0)
        {
            //out of stock
            model.SubscriptionAllowed = true;
            model.AlreadySubscribed = await _backInStockSubscriptionService
                .FindSubscriptionAsync((await _workContext.GetCurrentCustomerAsync()).Id, product.Id, (await _storeContext.GetCurrentStoreAsync()).Id) != null;
        }

        return Ok(model);
    }

    /// <summary>
    /// Subscribe to back in stock product
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Subscribe(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return BadRequest(await _localizationService.GetResourceAsync("BackInStockSubscriptions.OnlyRegistered"));

        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
            product.BackorderMode == BackorderMode.NoBackorders &&
            product.AllowBackInStockSubscriptions &&
            await _productService.GetTotalStockQuantityAsync(product) <= 0)
        {
            //out of stock
            var subscription = await _backInStockSubscriptionService
                .FindSubscriptionAsync((await _workContext.GetCurrentCustomerAsync()).Id, product.Id, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (subscription != null)
            {
                //subscription already exists
                //unsubscribe
                await _backInStockSubscriptionService.DeleteSubscriptionAsync(subscription);

                return Ok(await _localizationService.GetResourceAsync("BackInStockSubscriptions.Notification.Unsubscribed"));
            }

            //subscription does not exist
            //subscribe
            if ((await _backInStockSubscriptionService
                .GetAllSubscriptionsByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id, 0, 1))
                .TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                return BadRequest(string.Format(await _localizationService.GetResourceAsync("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions));
            subscription = new BackInStockSubscription
            {
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                ProductId = product.Id,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _backInStockSubscriptionService.InsertSubscriptionAsync(subscription);

            return Ok(await _localizationService.GetResourceAsync("BackInStockSubscriptions.Notification.Subscribed"));
        }

        //subscription not possible
        return BadRequest(await _localizationService.GetResourceAsync("BackInStockSubscriptions.NotAllowed"));
    }

    /// <summary>
    /// Get customer back in stock subscriptions
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerBackInStockSubscriptionsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerSubscriptions(int? pageNumber)
    {
        if (_customerSettings.HideBackInStockSubscriptionsTab)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        return Ok(await GetCustomerSubscriptionsAsync(pageNumber));
    }

    /// <summary>
    /// Opt out from back in stock subscriptions
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerBackInStockSubscriptionsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> RemoveCustomerSubscriptions(CustomerSubscriptionsRequest request)
    {
        if (_customerSettings.HideBackInStockSubscriptionsTab)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        foreach (var subscriptionId in request.SubscriptionIds)
        {
            var subscription = await _backInStockSubscriptionService.GetSubscriptionByIdAsync(subscriptionId);
            if (subscription != null && subscription.CustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                await _backInStockSubscriptionService.DeleteSubscriptionAsync(subscription);
        }

        if (request.PrepareCustomerSubscriptions)
            return Ok(await GetCustomerSubscriptionsAsync(null));

        return Ok();
    }

    #endregion
}
