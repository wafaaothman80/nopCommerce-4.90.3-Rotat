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
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Vendors;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Vendor methods
/// </summary>
public partial class PublicVendorController : BaseAPIController
{
    #region Fields

    private readonly VendorSettings _vendorSettings;
    private readonly LocalizationSettings _localizationSettings;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IPictureService _pictureService;
    private readonly IAttributeParser<VendorAttribute, VendorAttributeValue> _vendorAttributeParser;
    private readonly IVendorService _vendorService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IAttributeService<VendorAttribute, VendorAttributeValue> _vendorAttributeService;
    private readonly IVendorModelFactory _vendorModelFactory;
    private readonly ICatalogModelFactory _catalogModelFactory;
    private readonly IWebHelper _webHelper;
    private readonly IStoreContext _storeContext;
    private readonly IHtmlFormatter _htmlFormatter;

    #endregion

    #region Ctor

    public PublicVendorController(VendorSettings vendorSettings,
        LocalizationSettings localizationSettings,
        ICustomerService customerService,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IPictureService pictureService,
        IAttributeParser<VendorAttribute, VendorAttributeValue> vendorAttributeParser,
        IVendorService vendorService,
        IUrlRecordService urlRecordService,
        IGenericAttributeService genericAttributeService,
        IWorkflowMessageService workflowMessageService,
        IAttributeService<VendorAttribute, VendorAttributeValue> vendorAttributeService,
        IVendorModelFactory vendorModelFactory,
        ICatalogModelFactory catalogModelFactory,
        IWebHelper webHelper,
        IStoreContext storeContext,
        IHtmlFormatter htmlFormatter)
    {
        _vendorSettings = vendorSettings;
        _localizationSettings = localizationSettings;
        _customerService = customerService;
        _workContext = workContext;
        _localizationService = localizationService;
        _pictureService = pictureService;
        _vendorAttributeParser = vendorAttributeParser;
        _vendorService = vendorService;
        _urlRecordService = urlRecordService;
        _genericAttributeService = genericAttributeService;
        _workflowMessageService = workflowMessageService;
        _vendorAttributeService = vendorAttributeService;
        _vendorModelFactory = vendorModelFactory;
        _catalogModelFactory = catalogModelFactory;
        _webHelper = webHelper;
        _storeContext = storeContext;
        _htmlFormatter = htmlFormatter;
    }

    #endregion

    #region Utilities

    protected virtual Task<bool> CheckVendorAvailabilityAsync(Vendor vendor)
    {
        var isAvailable = true;

        if (vendor == null || vendor.Deleted || !vendor.Active)
            isAvailable = false;

        return Task.FromResult(isAvailable);
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task UpdatePictureSeoNamesAsync(Vendor vendor)
    {
        var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
        if (picture != null)
            await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(vendor.Name));
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task<string> ParseVendorAttributesAsync(IFormCollection form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = "";
        var attributes = await _vendorAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = $"{NopVendorDefaults.VendorAttributePrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            )
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _vendorAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                            attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }
                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported vendor attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare vendor model
    /// </summary>
    /// <param name="vendorId">The vendor identifier</param>
    [HttpGet("{vendorId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(VendorModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetVendor(int vendorId, [FromQuery] CatalogRequest request)
    {
        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

        if (!await CheckVendorAvailabilityAsync(vendor))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(vendor)));

        //'Continue shopping' URL
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.LastContinueShoppingPageAttribute,
            _webHelper.GetThisPageUrl(false),
            (await _storeContext.GetCurrentStoreAsync()).Id);

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
        //model
        var model = await _catalogModelFactory.PrepareVendorModelAsync(vendor, command);

        return Ok(model);
    }

    /// <summary>
    /// Get vendor products
    /// </summary>
    /// <param name="vendorId">The vendor identifier</param>
    [HttpGet("{vendorId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CatalogProductsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetVendorProducts(int vendorId, [FromQuery] CatalogRequest request)
    {
        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

        if (!await CheckVendorAvailabilityAsync(vendor))
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(vendor)));

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
        var model = await _catalogModelFactory.PrepareVendorProductsModelAsync(vendor, command);

        return Ok(model);
    }

    /// <summary>
    /// Get all vendors
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IList<VendorModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetVendors()
    {
        //we don't allow viewing of vendors if "vendors" block is hidden
        if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _catalogModelFactory.PrepareVendorAllModelsAsync();
        return Ok(model);
    }

    /// <summary>
    /// Prepare apply for vendor model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplyVendorModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetApplyVendor()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = new ApplyVendorModel();
        model = await _vendorModelFactory.PrepareApplyVendorModelAsync(model, true, false, null);
        return Ok(model);
    }

    /// <summary>
    /// Submit apply for vendor request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ApplyVendor(VendorRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Vendors.ApplyAccount.IsAdmin"));

        var pictureId = 0;
        var (vendorPictureBinary, contentType, pictureFileLength) = PluginCommonHelper.ConvertBase64ToFile(request.AvatarBase64String, request.AvatarFileName);

        if (vendorPictureBinary != null)
            try
            {
                if (!contentType.StartsWith("image/") || contentType.StartsWith("image/svg"))
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Vendors.ApplyAccount.Picture.ErrorMessage"));
                else
                {
                    var picture = await _pictureService.InsertPictureAsync(vendorPictureBinary, contentType, null);

                    if (picture != null)
                        pictureId = picture.Id;
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Vendors.ApplyAccount.Picture.ErrorMessage"));
            }

        //vendor attributes
        var vendorAttributesXml = await ParseVendorAttributesAsync(new FormCollection(ConvertToFormCollection(request.VendorAttributes)));
        (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList()
            .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

        if (ModelState.IsValid)
        {
            var description = _htmlFormatter.FormatText(request.Description, false, false, true, false, false, false);
            //disabled by default
            var vendor = new Vendor
            {
                Name = request.Name,
                Email = request.Email,
                //some default settings
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                PictureId = pictureId,
                Description = description
            };
            await _vendorService.InsertVendorAsync(vendor);
            //search engine name (the same as vendor name)
            var seName = await _urlRecordService.ValidateSeNameAsync(vendor, vendor.Name, vendor.Name, true);
            await _urlRecordService.SaveSlugAsync(vendor, seName, 0);

            //associate to the current customer
            //but a store owner will have to manually add this customer role to "Vendors" role
            //if he wants to grant access to admin area
            (await _workContext.GetCurrentCustomerAsync()).VendorId = vendor.Id;
            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            //save vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

            //notify store owner here (email)
            await _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotificationAsync(await _workContext.GetCurrentCustomerAsync(),
                vendor, _localizationSettings.DefaultAdminLanguageId);

            return Ok(await _localizationService.GetResourceAsync("Vendors.ApplyAccount.Submitted"));
        }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Prepare vendor info model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(VendorInfoModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetInfo()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (await _workContext.GetCurrentVendorAsync() == null)
            return Unauthorized();

        if (!_vendorSettings.AllowVendorsToEditInfo)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = new VendorInfoModel();
        model = await _vendorModelFactory.PrepareVendorInfoModelAsync(model, false);
        return Ok(model);
    }

    /// <summary>
    /// Update vendor info
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdateInfo(VendorRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (await _workContext.GetCurrentVendorAsync() == null)
            return Unauthorized();

        if (!_vendorSettings.AllowVendorsToEditInfo)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        Picture picture = null;
        var (vendorPictureBinary, contentType, pictureFileLength) = PluginCommonHelper.ConvertBase64ToFile(request.AvatarBase64String, request.AvatarFileName);
        if (vendorPictureBinary != null)
            try
            {
                picture = await _pictureService.InsertPictureAsync(vendorPictureBinary, contentType, null);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.VendorInfo.Picture.ErrorMessage"));
            }

        var vendor = await _workContext.GetCurrentVendorAsync();
        var prevPicture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);

        //vendor attributes
        var form = new FormCollection(ConvertToFormCollection(request.VendorAttributes));
        var vendorAttributesXml = await ParseVendorAttributesAsync(form);
        (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList()
            .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

        if (ModelState.IsValid)
        {
            var description = _htmlFormatter.FormatText(request.Description, false, false, true, false, false, false);

            vendor.Name = request.Name;
            vendor.Email = request.Email;
            vendor.Description = description;

            if (picture != null)
            {
                vendor.PictureId = picture.Id;

                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            await _vendorService.UpdateVendorAsync(vendor);

            //save vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                await _workflowMessageService.SendVendorInformationChangeStoreOwnerNotificationAsync(vendor, _localizationSettings.DefaultAdminLanguageId);

            return Ok();
        }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Remove vendor picture
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeletePicture()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (await _workContext.GetCurrentVendorAsync() == null)
            return Unauthorized();

        if (!_vendorSettings.AllowVendorsToEditInfo)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var vendor = await _workContext.GetCurrentVendorAsync();
        var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);

        if (picture != null)
            await _pictureService.DeletePictureAsync(picture);

        vendor.PictureId = 0;
        await _vendorService.UpdateVendorAsync(vendor);

        //notifications
        if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
            await _workflowMessageService.SendVendorInformationChangeStoreOwnerNotificationAsync(vendor, _localizationSettings.DefaultAdminLanguageId);

        return Ok();
    }

    #endregion
}
