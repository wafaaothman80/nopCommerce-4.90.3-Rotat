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
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Tax;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Models.Common;
using Nop.Web.Models.Directory;
using Nop.Web.Models.Sitemap;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Factories;
using NopAdvance.Plugin.Misc.PublicAPI.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Common methods
/// </summary>
public partial class PublicCommonController : BaseAPIController
{
    #region Fields

    private readonly ILanguageService _languageService;
    private readonly IWorkContext _workContext;
    private readonly ICommonModelFactory _commonModelFactory;
    private readonly IAPICommonModelFactory _apiCommonModelFactory;
    private readonly ICurrencyService _currencyService;
    private readonly ICountryModelFactory _countryModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly SitemapSettings _sitemapSettings;
    private readonly ISitemapModelFactory _sitemapModelFactory;

    #endregion

    #region Ctor

    public PublicCommonController(ILanguageService languageService,
        IWorkContext workContext,
        ICommonModelFactory commonModelFactory,
        IAPICommonModelFactory apiCommonModelFactory,
        ICurrencyService currencyService,
        ICountryModelFactory countryModelFactory,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        SitemapSettings sitemapSettings,
        ISitemapModelFactory sitemapModelFactory)
    {
        _languageService = languageService;
        _workContext = workContext;
        _commonModelFactory = commonModelFactory;
        _apiCommonModelFactory = apiCommonModelFactory;
        _currencyService = currencyService;
        _countryModelFactory = countryModelFactory;
        _localizationService = localizationService;
        _urlRecordService = urlRecordService;
        _sitemapSettings = sitemapSettings;
        _sitemapModelFactory = sitemapModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get all states of specific country
    /// </summary>
    /// <param name="countryId">The country identifier</param>
    /// <param name="addSelectStateItem">Add select state item? (the first item)</param>
    [HttpGet("{countryId}")]
    [ProducesResponseType(typeof(IList<StateProvinceModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetStates(string countryId, bool addSelectStateItem = true)
    {
        var model = await _countryModelFactory.GetStatesByCountryIdAsync(int.Parse(countryId), addSelectStateItem);

        return Ok(model);
    }

    /// <summary>
    /// Get all languages
    /// </summary>
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [HttpGet]
    [Authorize(true)]
    [ProducesResponseType(typeof(LanguageResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetLanguages()
    {
        var model = await _apiCommonModelFactory.PrepareLanguageSelectorModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Set working language
    /// </summary>
    /// <param name="languageId">The language identifier</param>
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [HttpPost("{languageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SetLanguage(int languageId)
    {
        var language = await _languageService.GetLanguageByIdAsync(languageId);
        if (!language?.Published ?? false)
            language = await _workContext.GetWorkingLanguageAsync();

        await _workContext.SetWorkingLanguageAsync(language);

        return Ok();
    }

    /// <summary>
    /// Get all currencies
    /// </summary>
    [CheckAccessPublicStore(true)]
    [HttpGet]
    [ProducesResponseType(typeof(CurrencySelectorModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCurrencies()
    {
        var model = await _commonModelFactory.PrepareCurrencySelectorModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Set working currency
    /// </summary>
    /// <param name="currencyId">The currency identifier</param>
    [CheckAccessPublicStore(true)]
    [HttpPost("{currencyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SetCurrency(int currencyId)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);
        if (currency != null)
            await _workContext.SetWorkingCurrencyAsync(currency);
        return Ok();
    }

    /// <summary>
    /// Get all tax types
    /// </summary>
    [CheckAccessPublicStore(true)]
    [HttpGet]
    [ProducesResponseType(typeof(TaxTypeSelectorResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetTaxTypes()
    {
        var response = new TaxTypeSelectorResponse
        {
            CurrentTaxType = await _workContext.GetTaxDisplayTypeAsync()
        };

        response.AvailableTaxTypes.Add(new TaxTypeResponse
        {
            Id = (int)TaxDisplayType.IncludingTax,
            Name = TaxDisplayType.IncludingTax,
            DisplayText = await _localizationService.GetResourceAsync("Tax.Inclusive")
        });

        response.AvailableTaxTypes.Add(new TaxTypeResponse
        {
            Id = (int)TaxDisplayType.ExcludingTax,
            Name = TaxDisplayType.ExcludingTax,
            DisplayText = await _localizationService.GetResourceAsync("Tax.Exclusive")
        });

        return Ok(response);
    }

    /// <summary>
    /// Set tax display type
    /// </summary>
    /// <param name="taxType">TaxDisplayType</param>
    [CheckAccessPublicStore(true)]
    [HttpPost("{taxType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SetTaxType(TaxDisplayType taxType)
    {
        await _workContext.SetTaxDisplayTypeAsync(taxType);
        return Ok();
    }

    /// <summary>
    /// Find entity
    /// </summary>
    /// <param name="slug">Slug</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the found URL record
    /// </returns>
    [CheckAccessPublicStore(true)]
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UrlRecordResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetEntityBySlug(string slug)
    {
        var urlRecord = await _urlRecordService.GetBySlugAsync(slug);

        if (urlRecord == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(urlRecord)));

        var response = new UrlRecordResponse
        {
            Id = urlRecord.Id,
            EntityId = urlRecord.EntityId,
            EntityName = urlRecord.EntityName,
            Slug = urlRecord.Slug,
            IsActive = urlRecord.IsActive,
            LanguageId = urlRecord.LanguageId,
        };

        return Ok(response);
    }

    /// <summary>
    /// Prepare the sitemap model
    /// </summary>
    /// <param name="pageNumber">page number</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the sitemap model
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SitemapModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSitemap([FromQuery] int pageNumber)
    {
        if (!_sitemapSettings.SitemapEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var sitemapPageModel = new SitemapPageModel
        {
            PageNumber = pageNumber,
        };

        var model = await _sitemapModelFactory.PrepareSitemapModelAsync(sitemapPageModel);

        return Ok(model);
    }

    #endregion
}
