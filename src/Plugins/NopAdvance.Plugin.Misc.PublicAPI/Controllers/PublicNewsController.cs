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
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Web.Factories;
using Nop.Web.Models.News;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// News methods
/// </summary>
public partial class PublicNewsController : BaseAPIController
{
    #region Fields

    private readonly NewsSettings _newsSettings;
    private readonly LocalizationSettings _localizationSettings;
    private readonly INewsModelFactory _newsModelFactory;
    private readonly INewsService _newsService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;

    #endregion

    #region Ctor

    public PublicNewsController(NewsSettings newsSettings,
        LocalizationSettings localizationSettings,
        INewsModelFactory newsModelFactory,
        INewsService newsService,
        ICustomerService customerService,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IWorkflowMessageService workflowMessageService,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher)
    {
        _newsSettings = newsSettings;
        _localizationSettings = localizationSettings;
        _newsModelFactory = newsModelFactory;
        _newsService = newsService;
        _customerService = customerService;
        _workContext = workContext;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _workflowMessageService = workflowMessageService;
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get news to be displayed on home page
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HomepageNewsItemsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetHomepageNews()
    {
        if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _newsModelFactory.PrepareHomepageNewsItemsModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Get all news of the current store
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NewsItemListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAllNews([FromQuery] BasePageableRequest request)
    {
        if (!_newsSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var command = new NewsPagingFilteringModel
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        var model = await _newsModelFactory.PrepareNewsItemListModelAsync(command);
        return Ok(model);
    }

    /// <summary>
    /// Get specific news item
    /// </summary>
    /// <param name="newsItemId">The news item identifier</param>
    [HttpGet("{newsItemId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(NewsItemModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetNewsItem(int newsItemId)
    {
        if (!_newsSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);
        if (newsItem == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(newsItem)));

        var model = new NewsItemModel();
        model = await _newsModelFactory.PrepareNewsItemModelAsync(model, newsItem, true);

        return Ok(model);
    }

    /// <summary>
    /// Add a new news item comment
    /// </summary>
    /// <param name="newsItemId">The news item identifier</param>
    [HttpPost("{newsItemId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddNewsComment(int newsItemId, NewsCommentRequest request)
    {
        if (!_newsSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);
        if (newsItem == null || !newsItem.Published || !newsItem.AllowComments)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(newsItem)));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("News.Comments.OnlyRegisteredUsersLeaveComments"));

        if (ModelState.IsValid)
        {
            var comment = new NewsComment
            {
                NewsItemId = newsItem.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                CommentTitle = request.CommentTitle,
                CommentText = request.CommentText,
                IsApproved = !_newsSettings.NewsCommentsMustBeApproved,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _newsService.InsertNewsCommentAsync(comment);

            //notify a store owner;
            if (_newsSettings.NotifyAboutNewNewsComments)
                await _workflowMessageService.SendNewsCommentStoreOwnerNotificationMessageAsync(comment, _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddNewsComment",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddNewsComment"), comment);

            //raise event
            if (comment.IsApproved)
                await _eventPublisher.PublishAsync(new NewsCommentApprovedEvent(comment));

            var message = comment.IsApproved
                ? await _localizationService.GetResourceAsync("News.Comments.SuccessfullyAdded")
                : await _localizationService.GetResourceAsync("News.Comments.SeeAfterApproving");

            return Ok(message);
        }

        return PrepareBadRequest(ModelState);
    }

    #endregion
}
