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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Boards;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Forum methods
/// </summary>
public partial class PublicForumController : BaseAPIController
{
    #region Fields

    private readonly ForumSettings _forumSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly IForumService _forumService;
    private readonly IForumModelFactory _forumModelFactory;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly ICustomerService _customerService;
    private readonly IProfileModelFactory _profileModelFactory;

    #endregion

    #region Ctor

    public PublicForumController(ForumSettings forumSettings,
        CustomerSettings customerSettings,
        IForumService forumService,
        IForumModelFactory forumModelFactory,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IWebHelper webHelper,
        ICustomerService customerService,
        IProfileModelFactory profileModelFactory)
    {
        _forumSettings = forumSettings;
        _customerSettings = customerSettings;
        _forumService = forumService;
        _forumModelFactory = forumModelFactory;
        _workContext = workContext;
        _localizationService = localizationService;
        _webHelper = webHelper;
        _customerService = customerService;
        _profileModelFactory = profileModelFactory;
    }

    #endregion

    #region Utilities

    protected virtual async Task<ForumGroupModel> PrepareForumGroupModelAsync(ForumGroup forumGroup, bool prepareForums)
    {
        var forumGroupModel = new ForumGroupModel
        {
            Id = forumGroup.Id,
            Name = forumGroup.Name,
            SeName = await _forumService.GetForumGroupSeNameAsync(forumGroup),
        };
        if (prepareForums)
        {
            var forums = await _forumService.GetAllForumsByGroupIdAsync(forumGroup.Id);
            foreach (var forum in forums)
            {
                var forumModel = await _forumModelFactory.PrepareForumRowModelAsync(forum);
                forumGroupModel.Forums.Add(forumModel);
            }
        }
        return forumGroupModel;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get all forums
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BoardsIndexModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetForums()
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PrepareBoardsIndexModelAsync();

        return Ok(model);
    }

    /// <summary>
    /// Get all active discussions
    /// </summary>
    /// <param name="forumId">The forum identifier</param>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ActiveDiscussionsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetActiveDiscussions(int forumId = 0, int pageNumber = 1)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PrepareActiveDiscussionsModelAsync(forumId, pageNumber);

        return Ok(model);
    }

    /// <summary>
    /// Get active discussions in small for forum's index page
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ActiveDiscussionsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetActiveDiscussionsSmall()
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PrepareActiveDiscussionsModelAsync();
        if (!model.ForumTopics.Any())
            return NoContent();

        return Ok(model);
    }

    /// <summary>
    /// Get specific forum group
    /// </summary>
    /// <param name="forumGroupId">The forum group identifier</param>
    [HttpGet("{forumGroupId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ForumGroupModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetForumGroup(int forumGroupId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumGroup = await _forumService.GetForumGroupByIdAsync(forumGroupId);
        if (forumGroup == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumGroup)));

        var model = await _forumModelFactory.PrepareForumGroupModelAsync(forumGroup);

        return Ok(model);
    }

    /// <summary>
    /// Get specific forum
    /// </summary>
    /// <param name="forumId">The forum identifier</param>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet("{forumId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ForumPageModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetForum(int forumId, int pageNumber = 1)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forum = await _forumService.GetForumByIdAsync(forumId);
        if (forum == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forum)));

        var model = await _forumModelFactory.PrepareForumPageModelAsync(forum, pageNumber);

        return Ok(model);
    }

    /// <summary>
    /// Watch/Unwatch forum
    /// </summary>
    /// <param name="forumId">The forum identifier</param>
    [HttpPost("{forumId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> WatchForum(int forumId)
    {
        if (!await _forumService.IsCustomerAllowedToSubscribeAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var watchTopic = await _localizationService.GetResourceAsync("Forum.WatchForum");
        var unwatchTopic = await _localizationService.GetResourceAsync("Forum.UnwatchForum");
        var returnText = watchTopic;

        var forum = await _forumService.GetForumByIdAsync(forumId);
        if (forum == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forum)));

        var forumSubscription = (await _forumService.GetAllSubscriptionsAsync((await _workContext.GetCurrentCustomerAsync()).Id,
            forum.Id, 0, 0, 1)).FirstOrDefault();

        if (forumSubscription == null)
        {
            forumSubscription = new ForumSubscription
            {
                SubscriptionGuid = Guid.NewGuid(),
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                ForumId = forum.Id,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _forumService.InsertSubscriptionAsync(forumSubscription);
            returnText = unwatchTopic;
        }
        else
            await _forumService.DeleteSubscriptionAsync(forumSubscription);

        return Ok(returnText);
    }

    /// <summary>
    /// Get specific forum topic
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet("{topicId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ForumTopicPageModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetTopic(int topicId, int pageNumber = 1)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        var model = await _forumModelFactory.PrepareForumTopicPageModelAsync(forumTopic, pageNumber);

        //update view count
        forumTopic.Views += 1;
        await _forumService.UpdateTopicAsync(forumTopic);

        return Ok(model);
    }

    /// <summary>
    /// Watch/Unwatch forum topic
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    [HttpPost("{topicId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> WatchTopic(int topicId)
    {
        if (!await _forumService.IsCustomerAllowedToSubscribeAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var watchTopic = await _localizationService.GetResourceAsync("Forum.WatchTopic");
        var unwatchTopic = await _localizationService.GetResourceAsync("Forum.UnwatchTopic");
        var returnText = watchTopic;

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        var forumSubscription = (await _forumService.GetAllSubscriptionsAsync((await _workContext.GetCurrentCustomerAsync()).Id,
            0, forumTopic.Id, 0, 1)).FirstOrDefault();

        if (forumSubscription == null)
        {
            forumSubscription = new ForumSubscription
            {
                SubscriptionGuid = Guid.NewGuid(),
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                TopicId = forumTopic.Id,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _forumService.InsertSubscriptionAsync(forumSubscription);
            returnText = unwatchTopic;
        }
        else
            await _forumService.DeleteSubscriptionAsync(forumSubscription);

        return Ok(returnText);
    }

    /// <summary>
    /// Prepare the move topic model
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    [HttpGet("{topicId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TopicMoveModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetMoveTopic(int topicId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        var model = await _forumModelFactory.PrepareTopicMoveAsync(forumTopic);

        return Ok(model);
    }

    /// <summary>
    /// Move topic to another forum
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ForumTopicPageModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> MoveTopic(MoveTopicRequest request)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(request.TopicId);

        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        var newForumId = request.ForumId;
        var forum = await _forumService.GetForumByIdAsync(newForumId);

        if (forum != null && forumTopic.ForumId != newForumId)
            await _forumService.MoveTopicAsync(forumTopic.Id, newForumId);

        if (request.PrepareTopic)
        {
            var model = await _forumModelFactory.PrepareForumTopicPageModelAsync(forumTopic, 1);

            //update view count
            forumTopic.Views += 1;
            await _forumService.UpdateTopicAsync(forumTopic);

            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Delete topic
    /// </summary>
    /// <param name="topicId">The topic identifier</param>
    [HttpDelete("{topicId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteTopic(int topicId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        if (!await _forumService.IsCustomerAllowedToDeleteTopicAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
            return Unauthorized();

        await _forumService.DeleteTopicAsync(forumTopic);
        return Ok();

    }

    /// <summary>
    /// Prepare create topic model
    /// </summary>
    /// <param name="forumId">The forum identifier</param>
    [HttpGet("{forumId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EditForumTopicModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCreateTopic(int forumId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forum = await _forumService.GetForumByIdAsync(forumId);
        if (forum == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forum)));

        if (await _forumService.IsCustomerAllowedToCreateTopicAsync(await _workContext.GetCurrentCustomerAsync(), forum) == false)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = new EditForumTopicModel();
        await _forumModelFactory.PrepareTopicCreateModelAsync(forum, model);
        return Ok(model);
    }

    /// <summary>
    /// Create a new forum topic
    /// </summary>
    /// <param name="forumId">The forum identifier</param>
    [HttpPost("{forumId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CreateTopic(int forumId, EditTopicRequest request)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forum = await _forumService.GetForumByIdAsync(forumId);
        if (forum == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forum)));

        if (ModelState.IsValid)
            try
            {
                if (!await _forumService.IsCustomerAllowedToCreateTopicAsync(await _workContext.GetCurrentCustomerAsync(), forum))
                    return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

                var subject = request.Subject;
                var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
                if (maxSubjectLength > 0 && subject.Length > maxSubjectLength)
                    subject = subject[0..maxSubjectLength];

                var text = request.Text;
                var maxPostLength = _forumSettings.PostMaxLength;
                if (maxPostLength > 0 && text.Length > maxPostLength)
                    text = text[0..maxPostLength];

                var topicType = ForumTopicType.Normal;
                var ipAddress = _webHelper.GetCurrentIpAddress();
                var nowUtc = DateTime.UtcNow;

                if (await _forumService.IsCustomerAllowedToSetTopicPriorityAsync(await _workContext.GetCurrentCustomerAsync()))
                    topicType = (ForumTopicType)Enum.ToObject(typeof(ForumTopicType), request.TopicTypeId);

                //forum topic
                var forumTopic = new ForumTopic
                {
                    ForumId = forum.Id,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    TopicTypeId = (int)topicType,
                    Subject = subject,
                    CreatedOnUtc = nowUtc,
                    UpdatedOnUtc = nowUtc
                };
                await _forumService.InsertTopicAsync(forumTopic, true);

                //forum post
                var forumPost = new ForumPost
                {
                    TopicId = forumTopic.Id,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Text = text,
                    IPAddress = ipAddress,
                    CreatedOnUtc = nowUtc,
                    UpdatedOnUtc = nowUtc
                };
                await _forumService.InsertPostAsync(forumPost, false);

                //update forum topic
                forumTopic.NumPosts = 1;
                forumTopic.LastPostId = forumPost.Id;
                forumTopic.LastPostCustomerId = forumPost.CustomerId;
                forumTopic.LastPostTime = forumPost.CreatedOnUtc;
                forumTopic.UpdatedOnUtc = nowUtc;
                await _forumService.UpdateTopicAsync(forumTopic);

                //subscription                
                if (await _forumService.IsCustomerAllowedToSubscribeAsync(await _workContext.GetCurrentCustomerAsync()))
                    if (request.Subscribed)
                    {
                        var forumSubscription = new ForumSubscription
                        {
                            SubscriptionGuid = Guid.NewGuid(),
                            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                            TopicId = forumTopic.Id,
                            CreatedOnUtc = nowUtc
                        };

                        await _forumService.InsertSubscriptionAsync(forumSubscription);
                    }

                return Ok(forumTopic.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Prepare edit topic model
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    [HttpGet("{topicId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EditForumTopicModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetUpdateTopic(int topicId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        if (!await _forumService.IsCustomerAllowedToEditTopicAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = new EditForumTopicModel();
        await _forumModelFactory.PrepareTopicEditModelAsync(forumTopic, model, false);

        return Ok(model);
    }

    /// <summary>
    /// Update the forum topic
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    [HttpPut("{topicId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdateTopic(int topicId, EditTopicRequest request)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);

        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        var forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
        if (forum == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forum)));

        if (ModelState.IsValid)
            try
            {
                if (!await _forumService.IsCustomerAllowedToEditTopicAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
                    return Unauthorized();

                var subject = request.Subject;
                var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
                if (maxSubjectLength > 0 && subject.Length > maxSubjectLength)
                    subject = subject[0..maxSubjectLength];

                var text = request.Text;
                var maxPostLength = _forumSettings.PostMaxLength;
                if (maxPostLength > 0 && text.Length > maxPostLength)
                    text = text[0..maxPostLength];

                var topicType = ForumTopicType.Normal;
                var ipAddress = _webHelper.GetCurrentIpAddress();
                var nowUtc = DateTime.UtcNow;

                if (await _forumService.IsCustomerAllowedToSetTopicPriorityAsync(await _workContext.GetCurrentCustomerAsync()))
                    topicType = (ForumTopicType)Enum.ToObject(typeof(ForumTopicType), request.TopicTypeId);

                //forum topic
                forumTopic.TopicTypeId = (int)topicType;
                forumTopic.Subject = subject;
                forumTopic.UpdatedOnUtc = nowUtc;
                await _forumService.UpdateTopicAsync(forumTopic);

                //forum post                
                var firstPost = await _forumService.GetFirstPostAsync(forumTopic);
                if (firstPost != null)
                {
                    firstPost.Text = text;
                    firstPost.UpdatedOnUtc = nowUtc;
                    await _forumService.UpdatePostAsync(firstPost);
                }
                else
                {
                    //error (not possible)
                    firstPost = new ForumPost
                    {
                        TopicId = forumTopic.Id,
                        CustomerId = forumTopic.CustomerId,
                        Text = text,
                        IPAddress = ipAddress,
                        UpdatedOnUtc = nowUtc
                    };

                    await _forumService.InsertPostAsync(firstPost, false);
                }

                //subscription
                if (await _forumService.IsCustomerAllowedToSubscribeAsync(await _workContext.GetCurrentCustomerAsync()))
                {
                    var forumSubscription = (await _forumService.GetAllSubscriptionsAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                        0, forumTopic.Id, 0, 1)).FirstOrDefault();
                    if (request.Subscribed)
                        if (forumSubscription == null)
                        {
                            forumSubscription = new ForumSubscription
                            {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                                TopicId = forumTopic.Id,
                                CreatedOnUtc = nowUtc
                            };

                            await _forumService.InsertSubscriptionAsync(forumSubscription);
                        }
                        else
                        if (forumSubscription != null)
                            await _forumService.DeleteSubscriptionAsync(forumSubscription);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Delete the post
    /// </summary>
    /// <param name="postId">The forum topic post identifier</param>
    [HttpDelete("{postId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeletePost(int postId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumPost = await _forumService.GetPostByIdAsync(postId);

        if (forumPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumPost)));

        if (!await _forumService.IsCustomerAllowedToDeletePostAsync(await _workContext.GetCurrentCustomerAsync(), forumPost))
            return Unauthorized();

        await _forumService.DeletePostAsync(forumPost);

        return Ok();
    }

    /// <summary>
    /// Prepare create post model
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    /// <param name="quote">Quoted post identifier (optional)</param>
    [HttpGet("{topicId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EditForumPostModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCreatePost(int topicId, int? quote)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        if (!await _forumService.IsCustomerAllowedToCreatePostAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PreparePostCreateModelAsync(forumTopic, quote, false);

        return Ok(model);
    }

    /// <summary>
    /// Create a new forum topic post
    /// </summary>
    /// <param name="topicId">The forum topic identifier</param>
    [HttpPost("{topicId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CreatePost(int topicId, EditForumPostRequest request)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumTopic = await _forumService.GetTopicByIdAsync(topicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        if (ModelState.IsValid)
            try
            {
                if (!await _forumService.IsCustomerAllowedToCreatePostAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
                    return Unauthorized();

                var text = request.Text;
                var maxPostLength = _forumSettings.PostMaxLength;
                if (maxPostLength > 0 && text.Length > maxPostLength)
                    text = text[0..maxPostLength];

                var ipAddress = _webHelper.GetCurrentIpAddress();

                var nowUtc = DateTime.UtcNow;

                var forumPost = new ForumPost
                {
                    TopicId = forumTopic.Id,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Text = text,
                    IPAddress = ipAddress,
                    CreatedOnUtc = nowUtc,
                    UpdatedOnUtc = nowUtc
                };
                await _forumService.InsertPostAsync(forumPost, true);

                //subscription
                if (await _forumService.IsCustomerAllowedToSubscribeAsync(await _workContext.GetCurrentCustomerAsync()))
                {
                    var forumSubscription = (await _forumService.GetAllSubscriptionsAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                        0, forumPost.TopicId, 0, 1)).FirstOrDefault();
                    if (request.Subscribed)
                        if (forumSubscription == null)
                        {
                            forumSubscription = new ForumSubscription
                            {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                                TopicId = forumPost.TopicId,
                                CreatedOnUtc = nowUtc
                            };

                            await _forumService.InsertSubscriptionAsync(forumSubscription);
                        }
                        else
                        if (forumSubscription != null)
                            await _forumService.DeleteSubscriptionAsync(forumSubscription);
                }

                return Ok(forumPost.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Prepare edit post model
    /// </summary>
    /// <param name="postId">The forum topic post identifier</param>
    [HttpGet("{postId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EditForumPostModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetUpdatePost(int postId)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumPost = await _forumService.GetPostByIdAsync(postId);
        if (forumPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumPost)));

        if (!await _forumService.IsCustomerAllowedToEditPostAsync(await _workContext.GetCurrentCustomerAsync(), forumPost))
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PreparePostEditModelAsync(forumPost, false);

        return Ok(model);
    }

    /// <summary>
    /// Update the post
    /// </summary>
    /// <param name="postId">The forum topic post identifier</param>
    [HttpPut("{postId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdatePost(int postId, EditForumPostRequest request)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumPost = await _forumService.GetPostByIdAsync(postId);
        if (forumPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumPost)));

        if (!await _forumService.IsCustomerAllowedToEditPostAsync(await _workContext.GetCurrentCustomerAsync(), forumPost))
            return Unauthorized();

        var forumTopic = await _forumService.GetTopicByIdAsync(forumPost.TopicId);
        if (forumTopic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumTopic)));

        var forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
        if (forum == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forum)));

        if (ModelState.IsValid)
            try
            {
                var nowUtc = DateTime.UtcNow;

                var text = request.Text;
                var maxPostLength = _forumSettings.PostMaxLength;
                if (maxPostLength > 0 && text.Length > maxPostLength)
                    text = text[0..maxPostLength];

                forumPost.UpdatedOnUtc = nowUtc;
                forumPost.Text = text;
                await _forumService.UpdatePostAsync(forumPost);

                //subscription
                if (await _forumService.IsCustomerAllowedToSubscribeAsync(await _workContext.GetCurrentCustomerAsync()))
                {
                    var forumSubscription = (await _forumService.GetAllSubscriptionsAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                        0, forumPost.TopicId, 0, 1)).FirstOrDefault();
                    if (request.Subscribed)
                        if (forumSubscription == null)
                        {
                            forumSubscription = new ForumSubscription
                            {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                                TopicId = forumPost.TopicId,
                                CreatedOnUtc = nowUtc
                            };
                            await _forumService.InsertSubscriptionAsync(forumSubscription);
                        }
                        else
                        if (forumSubscription != null)
                            await _forumService.DeleteSubscriptionAsync(forumSubscription);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Vote the post
    /// </summary>
    /// <param name="postId">The forum topic post identifier</param>
    /// <param name="isUp">Is up(true) or down(false)</param>
    [HttpPost("{postId}/{isUp}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> VotePost(int postId, bool isUp)
    {
        if (!_forumSettings.ForumsEnabled || !_forumSettings.AllowPostVoting)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var forumPost = await _forumService.GetPostByIdAsync(postId);
        if (forumPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumPost)));

        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return BadRequest(await _localizationService.GetResourceAsync("Forum.Votes.Login"));

        if ((await _workContext.GetCurrentCustomerAsync()).Id == forumPost.CustomerId)
            return BadRequest(await _localizationService.GetResourceAsync("Forum.Votes.OwnPost"));

        var forumPostVote = await _forumService.GetPostVoteAsync(postId, await _workContext.GetCurrentCustomerAsync());
        if (forumPostVote != null)
        {
            if (forumPostVote.IsUp && isUp || !forumPostVote.IsUp && !isUp)
                return BadRequest(await _localizationService.GetResourceAsync("Forum.Votes.AlreadyVoted"));

            await _forumService.DeletePostVoteAsync(forumPostVote);
            return Ok();
        }

        if (await _forumService.GetNumberOfPostVotesAsync(await _workContext.GetCurrentCustomerAsync(), DateTime.UtcNow.AddDays(-1)) >= _forumSettings.MaxVotesPerDay)
            return BadRequest(string.Format(await _localizationService.GetResourceAsync("Forum.Votes.MaxVotesReached"), _forumSettings.MaxVotesPerDay));


        await _forumService.InsertPostVoteAsync(new ForumPostVote
        {
            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
            ForumPostId = postId,
            IsUp = isUp,
            CreatedOnUtc = DateTime.UtcNow
        });

        return Ok();
    }

    /// <summary>
    /// Get last/latest post
    /// </summary>
    /// <param name="lastPostId">The forum topic post identifier</param>
    /// <param name="showTopic">Show topic?</param>
    [HttpGet("{lastPostId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LastPostModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLastPost(int lastPostId, bool showTopic)
    {
        var forumPost = await _forumService.GetPostByIdAsync(lastPostId);
        if (forumPost == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(forumPost)));

        var model = await _forumModelFactory.PrepareLastPostModelAsync(forumPost, showTopic);
        return Ok(model);
    }

    /// <summary>
    /// Search into forum
    /// </summary>
    /// <param name="searchterms">Search term</param>
    /// <param name="advs">Is advanced search?</param>
    /// <param name="forumId">The forum identifier</param>
    /// <param name="within">The search type</param>
    /// <param name="limitDays">Limit search to previous in days</param>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet("{searchterms}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SearchModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SearchForum([Required] string searchterms, string forumId, bool? advs,
        ForumSearchType within, string limitDays, int pageNumber = 1)
    {
        if (!_forumSettings.ForumsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PrepareSearchModelAsync(searchterms, advs, forumId, Convert.ToString((int)within), limitDays, pageNumber);

        return Ok(model);
    }

    /// <summary>
    /// Get customer forum subscriptions
    /// </summary>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerForumSubscriptionsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerForumSubscriptions(int? pageNumber)
    {
        if (!_forumSettings.AllowCustomersToManageSubscriptions)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _forumModelFactory.PrepareCustomerForumSubscriptionsModelAsync(pageNumber);

        return Ok(model);
    }

    /// <summary>
    /// Delete customer forum subscriptions
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteCustomerForumSubscriptions(CustomerForumSubscriptionRequest request)
    {
        if (!_forumSettings.AllowCustomersToManageSubscriptions)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        foreach (var forumSubscriptionId in request.ForumSubscriptionIds)
        {
            var forumSubscription = await _forumService.GetSubscriptionByIdAsync(forumSubscriptionId);
            if (forumSubscription != null && forumSubscription.CustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                await _forumService.DeleteSubscriptionAsync(forumSubscription);
        }
        return Ok();
    }

    /// <summary>
    /// Prepare customer profile model
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="pageNumber">Page number of the pager(optional)</param>
    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ForumCustomerProfileResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerProfile(int customerId, int? pageNumber)
    {
        if (!_customerSettings.AllowViewingProfiles)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(customer)));

        var response = new ForumCustomerProfileResponse
        {
            Profile = await _profileModelFactory.PrepareProfileIndexModelAsync(customer, pageNumber),
            ProfileInfo = await _profileModelFactory.PrepareProfileInfoModelAsync(customer)
        };

        if (response.Profile.ForumsEnabled)
            response.ProfilePosts = await _profileModelFactory.PrepareProfilePostsModelAsync(customer, response.Profile.PostsPage);

        return Ok(response);
    }

    #endregion
}
