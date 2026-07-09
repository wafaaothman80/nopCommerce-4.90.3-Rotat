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
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Localization;
using Nop.Core.Events;
using Nop.Services.Blogs;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Factories;
using Nop.Web.Models.Blogs;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Blog post methods
/// </summary>
public partial class PublicBlogController : BaseAPIController
{
    #region Fields

    private readonly BlogSettings _blogSettings;
    private readonly LocalizationSettings _localizationSettings;
    private readonly IBlogModelFactory _blogModelFactory;
    private readonly IBlogService _blogService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;

    #endregion

    #region Ctor

    public PublicBlogController(BlogSettings blogSettings,
        LocalizationSettings localizationSettings,
        IBlogModelFactory blogModelFactory,
        IBlogService blogService,
        ICustomerService customerService,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IWorkflowMessageService workflowMessageService,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher)
    {
        _blogSettings = blogSettings;
        _localizationSettings = localizationSettings;
        _blogModelFactory = blogModelFactory;
        _blogService = blogService;
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
    /// Get all blog posts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BlogPostListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetBlogs([FromQuery] BlogRequest request)
    {
        if (!_blogSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var month = request.Year <= 0 ? null : request.Year + "-" + request.Month;

        var command = new BlogPagingFilteringModel
        {
            Month = month,
            Tag = request.Tag,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        var model = await _blogModelFactory.PrepareBlogPostListModelAsync(command);
        return Ok(model);
    }

    /// <summary>
    /// Get all blog post tags
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BlogPostTagListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetBlogTags()
    {
        if (!_blogSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _blogModelFactory.PrepareBlogPostTagListModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Get all blog post months
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IList<BlogPostYearModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetBlogMonths()
    {
        if (!_blogSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _blogModelFactory.PrepareBlogPostYearModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Get blog post
    /// </summary>
    /// <param name="blogPostId">The blog post identifier</param>
    [HttpGet("{blogPostId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BlogPostModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetBlogPost(int blogPostId)
    {
        if (!_blogSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId);
        if (blogPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(blogPost)));

        var model = new BlogPostModel();
        await _blogModelFactory.PrepareBlogPostModelAsync(model, blogPost, true);

        return Ok(model);
    }

    /// <summary>
    /// Add comment to a blog post
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddBlogComment(BlogCommentRequest request)
    {
        if (!_blogSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var blogPost = await _blogService.GetBlogPostByIdAsync(request.BlogPostId);
        if (blogPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(blogPost)));

        if (!blogPost.AllowComments)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_blogSettings.AllowNotRegisteredUsersToLeaveComments)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Blog.Comments.OnlyRegisteredUsersLeaveComments"));

        if (ModelState.IsValid)
        {
            var comment = new BlogComment
            {
                BlogPostId = blogPost.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                CommentText = request.CommentText,
                IsApproved = !_blogSettings.BlogCommentsMustBeApproved,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _blogService.InsertBlogCommentAsync(comment);

            //notify a store owner
            if (_blogSettings.NotifyAboutNewBlogComments)
                await _workflowMessageService.SendBlogCommentStoreOwnerNotificationMessageAsync(comment, _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddBlogComment",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddBlogComment"), comment);

            //raise event
            if (comment.IsApproved)
                await _eventPublisher.PublishAsync(new BlogCommentApprovedEvent(comment));

            var message = comment.IsApproved
                ? await _localizationService.GetResourceAsync("Blog.Comments.SuccessfullyAdded")
                : await _localizationService.GetResourceAsync("Blog.Comments.SeeAfterApproving");
            return Ok(message);
        }

        return PrepareBadRequest(ModelState);
    }

    #endregion
}
