// ***  ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **    **  **  **  **    ***    **  ** **  **** *    *
// **   *** ****** **    **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopCommerce Public RESTful API Plugin by NopAdvance team             *
// *    Copyright (c) NopAdvance LLP. All Rights Reserved.                   *
// ***************************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Vendors;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Polls;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Common;
using Nop.Web.Models.Polls;
using Nop.Web.Models.Topics;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Caching;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using System.Reflection;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// General methods
/// </summary>
public partial class PublicGeneralController : BaseAPIController
{
    #region Fields

    private readonly VendorSettings _vendorSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly CommonSettings _commonSettings;
    private readonly IPollModelFactory _pollModelFactory;
    private readonly IPollService _pollService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerService _customerService;
    private readonly ITopicModelFactory _topicModelFactory;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IWebHelper _webHelper;
    private readonly IPictureService _pictureService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    private readonly ILanguageService _languageService;
    private readonly ICommonModelFactory _commonModelFactory;
    private readonly IVendorService _vendorService;
    private readonly ITopicService _topicService;
    private readonly IAclService _aclService;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly IPublicSettingService _publicSettingService;

    #endregion

    #region Ctor

    public PublicGeneralController(
        VendorSettings vendorSettings,
        CustomerSettings customerSettings,
        CommonSettings commonSettings,
        IPollModelFactory pollModelFactory,
        IPollService pollService,
        IStoreMappingService storeMappingService,
        IWorkContext workContext,
        ILocalizationService localizationService,
        ICustomerService customerService,
        ITopicModelFactory topicModelFactory,
        ISettingService settingService,
        IStoreContext storeContext,
        IStaticCacheManager staticCacheManager,
        IWebHelper webHelper,
        IPictureService pictureService,
        ICustomerActivityService customerActivityService,
        IWorkflowMessageService workflowMessageService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        ILanguageService languageService,
        ICommonModelFactory commonModelFactory,
        IVendorService vendorService,
        ITopicService topicService,
        IAclService aclService,
        IHtmlFormatter htmlFormatter,
        IPublicSettingService publicSettingService)
    {
        _vendorSettings = vendorSettings;
        _customerSettings = customerSettings;
        _commonSettings = commonSettings;
        _pollModelFactory = pollModelFactory;
        _pollService = pollService;
        _storeMappingService = storeMappingService;
        _workContext = workContext;
        _localizationService = localizationService;
        _customerService = customerService;
        _topicModelFactory = topicModelFactory;
        _settingService = settingService;
        _storeContext = storeContext;
        _staticCacheManager = staticCacheManager;
        _webHelper = webHelper;
        _pictureService = pictureService;
        _customerActivityService = customerActivityService;
        _workflowMessageService = workflowMessageService;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _languageService = languageService;
        _commonModelFactory = commonModelFactory;
        _vendorService = vendorService;
        _topicService = topicService;
        _aclService = aclService;
        _htmlFormatter = htmlFormatter;
        _publicSettingService = publicSettingService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<string> GetPictureUrlAsync(int pictureId)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CachingDefaults.PICTURE_URL_MODEL_KEY,
            pictureId, _webHelper.IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var url = await _pictureService.GetPictureUrlAsync(pictureId, showDefaultPicture: false) ?? "";
            return url;
        });
    }

    /// <summary>
    /// nop versions differ: the newsletter service method name/signature may differ.
    /// This helper tries multiple known method names via reflection.
    /// </summary>
    protected virtual async Task<NewsLetterSubscription> FindNewsletterSubscriptionAsync(string email, int storeId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var svc = _newsLetterSubscriptionService;
        var type = svc.GetType();

        // try: GetNewsLetterSubscriptionByEmailAndStoreIdAsync(string email, int storeId)
        var candidates = new[]
        {
            "GetNewsLetterSubscriptionByEmailAndStoreIdAsync",
            "GetNewsLetterSubscriptionByEmailAndStoreId",
            "GetNewsLetterSubscriptionByEmailAsync",
            "GetNewsLetterSubscriptionByEmail",
            "GetNewsletterSubscriptionByEmailAndStoreIdAsync",
            "GetNewsletterSubscriptionByEmailAsync"
        };

        foreach (var name in candidates)
        {
            // (string,int)
            var mi = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(string), typeof(int) }, null);

            if (mi != null)
            {
                var result = mi.Invoke(svc, new object[] { email, storeId });
                if (result is Task<NewsLetterSubscription> taskSub)
                    return await taskSub;

                if (result is NewsLetterSubscription sub)
                    return sub;
            }

            // (string)
            mi = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(string) }, null);

            if (mi != null)
            {
                var result = mi.Invoke(svc, new object[] { email });

                if (result is Task<NewsLetterSubscription> taskSub)
                    return await taskSub;

                if (result is NewsLetterSubscription sub)
                    return sub;
            }
        }

        // Fallback: try to search if service provides a list/search method (optional)
        // If none found, return null.
        return null;
    }

    #endregion

    #region Methods

    [HttpGet]
    [Authorize(true)]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(PingResponse), StatusCodes.Status200OK)]
    public virtual IActionResult Ping()
    {
        var response = new PingResponse
        {
            CurrentVersion = NopVersion.CURRENT_VERSION,
            MinorVersion = NopVersion.MINOR_VERSION,
            FullVersion = NopVersion.FULL_VERSION
        };
        return Ok(response);
    }

    [HttpGet]
    [Authorize(true)]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(IList<LocaleStringResourceResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetLocaleStringResources(int languageId = 0)
    {
        var response = new List<LocaleStringResourceResponse>();

        if (languageId == 0)
        {
            var allLanguages = await _languageService.GetAllLanguagesAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
            var flag = 0;
            while (flag < allLanguages.Count)
            {
                var language = allLanguages[flag];
                var resources = await _localizationService.GetAllResourceValuesAsync(language.Id, true);

                foreach (var current in resources)
                {
                    response.Add(new LocaleStringResourceResponse
                    {
                        LanguageId = language.Id,
                        ResourceName = current.Key,
                        ResourceValue = current.Value.Value
                    });
                }

                flag++;
            }
        }
        else
        {
            var language = await _languageService.GetLanguageByIdAsync(languageId);
            if (language != null)
            {
                var resources = await _localizationService.GetAllResourceValuesAsync(language.Id, true);
                foreach (var current in resources)
                {
                    response.Add(new LocaleStringResourceResponse
                    {
                        LanguageId = language.Id,
                        ResourceName = current.Key,
                        ResourceValue = current.Value.Value
                    });
                }
            }
        }

        return Ok(response);
    }

    [HttpGet]
    [Authorize(true)]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(IEnumerable<SettingResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSettings()
    {
        var allSettings = await _settingService.GetAllSettingsAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var storeSettings = allSettings.Where(s => s.StoreId == storeId || s.StoreId == 0).Distinct();
        var result = storeSettings.Select(x => new SettingResponse { Id = x.StoreId, Name = x.Name, Value = x.Value });
        return Ok(result);
    }

    [HttpGet]
    [Authorize(true)]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(IEnumerable<SettingResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSettingsByName(string name, int storeId = 0)
    {
        if (string.IsNullOrEmpty(name))
            return BadRequest("name");

        var names = name.Split(",");
        var allSettings = await _publicSettingService.GetSettingsByNameAsync(names, storeId);
        var storeSettings = allSettings.Where(s => s.StoreId == storeId || s.StoreId == 0).Distinct();
        var result = storeSettings.Select(x => new SettingResponse { Id = x.StoreId, Name = x.Name, Value = x.Value });

        return Ok(result);
    }

    [HttpGet]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(HeaderLinksModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetHeaderInfo()
    {
        var model = await _commonModelFactory.PrepareHeaderLinksModelAsync();
        return Ok(model);
    }

    [HttpGet]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(FooterModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetFooterInfo()
    {
        var model = await _commonModelFactory.PrepareFooterModelAsync();
        return Ok(model);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IList<PollModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetHomepagePolls()
    {
        var model = await _pollModelFactory.PrepareHomepagePollModelsAsync();
        return Ok(model);
    }

    [HttpPost("{pollAnswerId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> PollsVote(int pollAnswerId)
    {
        var pollAnswer = await _pollService.GetPollAnswerByIdAsync(pollAnswerId);
        if (pollAnswer == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(pollAnswer)));

        var poll = await _pollService.GetPollByIdAsync(pollAnswer.PollId);

        if (!poll.Published || !await _storeMappingService.AuthorizeAsync(poll))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(poll)));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !poll.AllowGuestsToVote)
            return Unauthorized(await _localizationService.GetResourceAsync("Polls.OnlyRegisteredUsersVote"));

        var alreadyVoted = await _pollService.AlreadyVotedAsync(poll.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
        if (!alreadyVoted)
        {
            await _pollService.InsertPollVotingRecordAsync(new PollVotingRecord
            {
                PollAnswerId = pollAnswer.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow
            });

            pollAnswer.NumberOfVotes = (await _pollService.GetPollVotingRecordsByPollAnswerAsync(pollAnswer.Id)).Count;
            await _pollService.UpdatePollAnswerAsync(pollAnswer);
            await _pollService.UpdatePollAsync(poll);
        }

        return Ok();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IList<TopicModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAllTopics()
    {
        var topics = await _topicService.GetAllTopicsAsync((await _storeContext.GetCurrentStoreAsync()).Id);
        var model = new List<TopicModel>();
        foreach (var topic in topics)
            model.Add(await _topicModelFactory.PrepareTopicModelAsync(topic));
        return Ok(model);
    }

    [HttpGet("{topicId:int}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TopicModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetTopic(int topicId)
    {
        var topic = await _topicService.GetTopicByIdAsync(topicId);
        if (topic == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "topic"));

        var model = await _topicModelFactory.PrepareTopicModelAsync(topic);
        if (model == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "topic"));

        return Ok(model);
    }

    [HttpGet("{systemName:alpha}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TopicModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetTopic(string systemName)
    {
        var model = await _topicModelFactory.PrepareTopicModelBySystemNameAsync(systemName);
        if (model == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "topic"));

        return Ok(model);
    }

    [HttpPost("{topicId}")]
    [ProducesResponseType(typeof(AuthenticateTopicResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AuthenticateTopic(int topicId, AuthenticateTopicRequest request)
    {
        var authResult = false;
        var title = string.Empty;
        var body = string.Empty;
        var error = string.Empty;

        var topic = await _topicService.GetTopicByIdAsync(topicId);
        if (topic != null &&
            topic.Published &&
            topic.IsPasswordProtected &&
            await _storeMappingService.AuthorizeAsync(topic) &&
            await _aclService.AuthorizeAsync(topic))
        {
            if (topic.Password != null && topic.Password.Equals(request.Password))
            {
                authResult = true;
                title = await _localizationService.GetLocalizedAsync(topic, x => x.Title);
                body = await _localizationService.GetLocalizedAsync(topic, x => x.Body);
            }
            else
                error = await _localizationService.GetResourceAsync("Topic.WrongPassword");
        }

        return Ok(new AuthenticateTopicResponse { Authenticated = authResult, Title = title, Body = body, Error = error });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(SwiperSliderResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSwiperSlider()
    {
        var sliderSettings = await _settingService.LoadSettingAsync<SwiperSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

        if (string.IsNullOrEmpty(sliderSettings.Slides))
            return NoContent();

        var model = new SwiperSliderResponse
        {
            ShowNavigation = sliderSettings.ShowNavigation,
            ShowPagination = sliderSettings.ShowPagination,
            Autoplay = sliderSettings.Autoplay,
            AutoplayDelay = sliderSettings.AutoplayDelay,
        };

        var slides = JsonConvert.DeserializeObject<List<SwiperSlide>>(sliderSettings.Slides);
        foreach (var slide in slides)
        {
            var picUrl = await GetPictureUrlAsync(slide.PictureId);
            if (string.IsNullOrEmpty(picUrl))
                continue;

            model.Slides.Add(new()
            {
                PictureUrl = picUrl,
                TitleText = slide.TitleText,
                LinkUrl = slide.LinkUrl,
                AltText = slide.AltText,
                LazyLoading = sliderSettings.LazyLoading
            });
        }

        return Ok(model);
    }

    [CheckAccessClosedStore(true)]
    [HttpGet]
    [ProducesResponseType(typeof(ContactUsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetContactUs()
    {
        var model = new ContactUsModel();
        model = await _commonModelFactory.PrepareContactUsModelAsync(model, false);

        return Ok(model);
    }

    [CheckAccessClosedStore(true)]
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ContactUs(ContactUsRequest request)
    {
        if (ModelState.IsValid)
        {
            var subject = _commonSettings.SubjectFieldOnContactUsForm ? request.Subject : null;
            var body = _htmlFormatter.FormatText(request.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactUsMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                request.Email.Trim(), request.FullName, subject, body);

            await _customerActivityService.InsertActivityAsync("PublicStore.ContactUs",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ContactUs"));

            return Ok(await _localizationService.GetResourceAsync("ContactUs.YourEnquiryHasBeenSent"));
        }

        return PrepareBadRequest(ModelState);
    }

    [HttpGet("{vendorId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ContactVendorModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetContactVendor(int vendorId)
    {
        if (!_vendorSettings.AllowCustomersToContactVendors)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        if (vendor == null || !vendor.Active || vendor.Deleted)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(vendor)));

        var model = new ContactVendorModel();
        model = await _commonModelFactory.PrepareContactVendorModelAsync(model, vendor, false);

        return Ok(model);
    }

    [HttpPost("{vendorId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ContactVendor(int vendorId, ContactUsRequest request)
    {
        if (!_vendorSettings.AllowCustomersToContactVendors)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        if (vendor == null || !vendor.Active || vendor.Deleted)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(vendor)));

        if (ModelState.IsValid)
        {
            var subject = _commonSettings.SubjectFieldOnContactUsForm ? request.Subject : null;
            var body = _htmlFormatter.FormatText(request.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactVendorMessageAsync(vendor, (await _workContext.GetWorkingLanguageAsync()).Id,
                request.Email.Trim(), request.FullName, subject, body);

            return Ok(await _localizationService.GetResourceAsync("ContactVendor.YourEnquiryHasBeenSent"));
        }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Prepare newsletter box model (API DTO for 4.9 upgrade)
    /// </summary>
    [CheckAccessClosedStore(true)]
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NewsletterBoxResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSubscribeNewsletter()
    {
        if (_customerSettings.HideNewsletterBlock)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var customer = await _workContext.GetCurrentCustomerAsync();

        var model = new NewsletterBoxResponse
        {
            IsEnabled = true,
            AllowToUnsubscribe = _customerSettings.NewsletterBlockAllowToUnsubscribe,
            IsGuest = await _customerService.IsGuestAsync(customer),
            Email = customer?.Email ?? string.Empty
        };

        return Ok(model);
    }

    /// <summary>
    /// Subscribe to the newsletters
    /// </summary>
    [CheckAccessClosedStore(true)]
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SubscribeNewsletter(SubscribeNewsletterRequest request)
    {
        var result = string.Empty;

        if (!CommonHelper.IsValidEmail(request.Email))
            return BadRequest(await _localizationService.GetResourceAsync("Newsletter.Email.Wrong"));

        request.Email = request.Email.Trim();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        var subscription = await FindNewsletterSubscriptionAsync(request.Email, storeId);

        if (subscription != null)
        {
            if (request.Subscribe)
            {
                if (!subscription.Active)
                    await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(subscription);

                result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
            }
            else if (_customerSettings.NewsletterBlockAllowToUnsubscribe)
            {
                if (subscription.Active)
                    await _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessageAsync(subscription);

                result = await _localizationService.GetResourceAsync("Newsletter.UnsubscribeEmailSent");
            }
        }
        else if (request.Subscribe)
        {
            subscription = new NewsLetterSubscription
            {
                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                Email = request.Email,
                Active = false,
                StoreId = storeId,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(subscription);
            await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(subscription);

            result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
        }
        else if (_customerSettings.NewsletterBlockAllowToUnsubscribe)
        {
            result = await _localizationService.GetResourceAsync("Newsletter.UnsubscribeEmailSent");
        }

        return Ok(result);
    }

    #endregion
}
