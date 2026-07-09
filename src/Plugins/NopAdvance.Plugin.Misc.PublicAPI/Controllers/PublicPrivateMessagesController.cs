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
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Factories;
using Nop.Web.Models.PrivateMessages;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Private message methods
/// </summary>
public partial class PublicPrivateMessagesController : BaseAPIController
{
    #region Fields

    private readonly ForumSettings _forumSettings;
    private readonly IPrivateMessagesModelFactory _privateMessagesModelFactory;
    private readonly IForumService _forumService;
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public PublicPrivateMessagesController(ForumSettings forumSettings,
        IPrivateMessagesModelFactory privateMessagesModelFactory,
        IForumService forumService,
        IWorkContext workContext,
        ICustomerService customerService,
        IStoreContext storeContext,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService)
    {
        _forumSettings = forumSettings;
        _privateMessagesModelFactory = privateMessagesModelFactory;
        _forumService = forumService;
        _workContext = workContext;
        _customerService = customerService;
        _storeContext = storeContext;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get specific private message
    /// </summary>
    /// <param name="privateMessageId">The private message identifier</param>
    [HttpGet("{privateMessageId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PrivateMessageModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetMessage(int privateMessageId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
        if (pm != null)
        {
            if (pm.ToCustomerId != (await _workContext.GetCurrentCustomerAsync()).Id && pm.FromCustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, "customer"));

            if (!pm.IsRead && pm.ToCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
            {
                pm.IsRead = true;
                await _forumService.UpdatePrivateMessageAsync(pm);
            }
        }
        else
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(pm)));

        var model = await _privateMessagesModelFactory.PreparePrivateMessageModelAsync(pm);
        return Ok(model);
    }

    /// <summary>
    /// Get all inbox messages
    /// </summary>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PrivateMessageListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetInboxMessages(int? pageNumber)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _privateMessagesModelFactory.PrepareInboxModelAsync(pageNumber ?? 0, "inbox");
        return Ok(model);
    }

    /// <summary>
    /// Get all sent messages
    /// </summary>
    /// <param name="pageNumber">Page number of the pager</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PrivateMessageListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSentMessages(int? pageNumber)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _privateMessagesModelFactory.PrepareSentModelAsync(pageNumber ?? 0, "sent");
        return Ok(model);
    }

    /// <summary>
    /// Delete inbox messages (selected)
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PrivateMessageListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteInboxMessages(InboxPrivateMessagesRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        foreach (var privateMessageId in request.PrivateMessageIds)
        {
            var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
            if (pm != null)
                if (pm.ToCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                {
                    pm.IsDeletedByRecipient = true;
                    await _forumService.UpdatePrivateMessageAsync(pm);
                }
        }

        if (request.PrepareInboxItems)
        {
            var model = await _privateMessagesModelFactory.PrepareInboxModelAsync(0, "inbox");
            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Mark messages as unread (selected)
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PrivateMessageListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> MarkAsUnread(InboxPrivateMessagesRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        foreach (var privateMessageId in request.PrivateMessageIds)
        {
            var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
            if (pm != null)
                if (pm.ToCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                {
                    pm.IsRead = false;
                    await _forumService.UpdatePrivateMessageAsync(pm);
                }
        }
        if (request.PrepareInboxItems)
        {
            var model = await _privateMessagesModelFactory.PrepareInboxModelAsync(0, "inbox");
            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Delete sent messages (selected)
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PrivateMessageListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteSentMessages(SentPrivateMessagesRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        foreach (var privateMessageId in request.PrivateMessageIds)
        {
            var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
            if (pm != null)
                if (pm.FromCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                {
                    pm.IsDeletedByAuthor = true;
                    await _forumService.UpdatePrivateMessageAsync(pm);
                }
        }
        if (request.PrepareSentItems)
        {
            var model = await _privateMessagesModelFactory.PrepareSentModelAsync(0, "sent");
            return Ok(model);
        }
        return Ok();
    }

    /// <summary>
    /// Prepare send message model
    /// </summary>
    /// <param name="toCustomerId">The customer identifer</param>
    /// <param name="replyToMessageId">The message identifier(if is reply)</param>
    [HttpGet("{toCustomerId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SendPrivateMessageModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSendMessage(int toCustomerId, int? replyToMessageId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var customerTo = await _customerService.GetCustomerByIdAsync(toCustomerId);
        if (customerTo == null || await _customerService.IsGuestAsync(customerTo))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "ToCustomer"));

        PrivateMessage replyToPM = null;
        if (replyToMessageId.HasValue)
            //reply to a previous PM
            replyToPM = await _forumService.GetPrivateMessageByIdAsync(replyToMessageId.Value);

        var model = await _privateMessagesModelFactory.PrepareSendPrivateMessageModelAsync(customerTo, replyToPM);
        return Ok(model);
    }

    /// <summary>
    /// Send private message
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SendMessage(SendPrivateMessageRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        Customer toCustomer;
        var replyToPM = await _forumService.GetPrivateMessageByIdAsync(request.ReplyToMessageId);
        if (replyToPM != null)
            //reply to a previous PM
            if (replyToPM.ToCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id || replyToPM.FromCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                //Reply to already sent PM (by current customer) should not be sent to yourself
                toCustomer = await _customerService.GetCustomerByIdAsync(replyToPM.FromCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id
                    ? replyToPM.ToCustomerId
                    : replyToPM.FromCustomerId);
            else
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, "Private message"));
        else
            //first PM
            toCustomer = await _customerService.GetCustomerByIdAsync(request.ToCustomerId);

        if (toCustomer == null || await _customerService.IsGuestAsync(toCustomer))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(toCustomer)));

        if (ModelState.IsValid)
            try
            {
                var subject = request.Subject;
                if (_forumSettings.PMSubjectMaxLength > 0 && subject.Length > _forumSettings.PMSubjectMaxLength)
                    subject = subject[0.._forumSettings.PMSubjectMaxLength];

                var text = request.Message;
                if (_forumSettings.PMTextMaxLength > 0 && text.Length > _forumSettings.PMTextMaxLength)
                    text = text[0.._forumSettings.PMTextMaxLength];

                var nowUtc = DateTime.UtcNow;

                var privateMessage = new PrivateMessage
                {
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    ToCustomerId = toCustomer.Id,
                    FromCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Subject = subject,
                    Text = text,
                    IsDeletedByAuthor = false,
                    IsDeletedByRecipient = false,
                    IsRead = false,
                    CreatedOnUtc = nowUtc
                };

                await _forumService.InsertPrivateMessageAsync(privateMessage);

                //activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.SendPM",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.SendPM"), toCustomer.Email), toCustomer);

                return Ok(privateMessage.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Delete a private message
    /// </summary>
    /// <param name="privateMessageId">The message identifier</param>
    [HttpDelete("{privateMessageId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteMessage(int privateMessageId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_forumSettings.AllowPrivateMessages)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
        if (pm == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "PrivateMessage"));

        if (pm.FromCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
        {
            pm.IsDeletedByAuthor = true;
            await _forumService.UpdatePrivateMessageAsync(pm);
        }

        if (pm.ToCustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
        {
            pm.IsDeletedByRecipient = true;
            await _forumService.UpdatePrivateMessageAsync(pm);
        }

        return Ok();
    }

    #endregion
}
