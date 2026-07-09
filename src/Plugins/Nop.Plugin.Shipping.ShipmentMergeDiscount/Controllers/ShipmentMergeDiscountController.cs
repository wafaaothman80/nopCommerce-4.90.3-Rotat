using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Shipping.ShipmentMergeDiscount.Models;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class ShipmentMergeDiscountController : BasePluginController
{
    #region Fields

    private readonly IDiscountService _discountService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public ShipmentMergeDiscountController(
        IDiscountService discountService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _discountService = discountService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Discount-requirement configuration (Admin → Discounts → Requirements)

    /// <summary>
    /// Renders the configuration widget shown inside the admin Discounts edit page
    /// when an admin adds this requirement rule to a discount.
    /// The requirement is automatic (no per-requirement settings), so the page just
    /// confirms the rule is active and shows a summary of current global settings.
    /// </summary>
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        // Verify the discount exists.
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        if (discount == null)
            return Content("Discount not found.");

        // Verify the requirement exists when editing an existing one.
        if (discountRequirementId.HasValue)
        {
            var req = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value);
            if (req == null)
                return Content("Discount requirement not found.");
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeScope);

        var model = new RequirementModel
        {
            DiscountId = discountId,
            RequirementId = discountRequirementId ?? 0,
            PluginEnabled = settings.Enabled,
            MinimumEligibleOrders = settings.MinimumEligibleOrders,
            UsePercentage = settings.UsePercentage,
            DiscountPercentage = settings.DiscountPercentage,
            DiscountAmount = settings.DiscountAmount
        };

        // This prefix allows multiple requirements of the same type on one discount form.
        ViewData.TemplateInfo.HtmlFieldPrefix =
            $"ShipmentMergeDiscount{discountRequirementId ?? 0}";

        return View("~/Plugins/Shipping.ShipmentMergeDiscount/Views/Configure.cshtml", model);
    }

    /// <summary>
    /// Saves (or creates) the discount requirement. Since the requirement has no
    /// per-requirement settings, we only ensure the DiscountRequirement record exists.
    /// Returns JSON so the admin discount page can handle the response inline.
    /// </summary>
    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        if (!ModelState.IsValid)
            return Ok(new { Errors = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

        var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
        if (discount == null)
            return Ok(new { Errors = new[] { "Discount not found." } });

        var requirement = model.RequirementId > 0
            ? await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId)
            : null;

        if (requirement == null)
        {
            requirement = new DiscountRequirement
            {
                DiscountId = discount.Id,
                DiscountRequirementRuleSystemName = ShipmentMergeDiscountDefaults.SystemName
            };

            await _discountService.InsertDiscountRequirementAsync(requirement);
        }

        return Ok(new { NewRequirementId = requirement.Id });
    }

    #endregion

    #region Global plugin settings (Plugin Manager → Configure)

    /// <summary>
    /// Renders the global plugin settings page accessible from the Plugin Manager.
    /// </summary>
    public async Task<IActionResult> Settings()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeScope);

        var model = new ConfigurationModel
        {
            ActiveStoreScopeConfiguration = storeScope,
            Enabled = settings.Enabled,
            MinimumEligibleOrders = settings.MinimumEligibleOrders,
            UsePercentage = settings.UsePercentage,
            DiscountPercentage = settings.DiscountPercentage,
            DiscountAmount = settings.DiscountAmount,
            MaxDiscountAmount = settings.MaxDiscountAmount,
            StrictAddressMatch = settings.StrictAddressMatch
        };

        if (storeScope > 0)
        {
            model.Enabled_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.Enabled, storeScope);
            model.MinimumEligibleOrders_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.MinimumEligibleOrders, storeScope);
            model.UsePercentage_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.UsePercentage, storeScope);
            model.DiscountPercentage_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.DiscountPercentage, storeScope);
            model.DiscountAmount_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.DiscountAmount, storeScope);
            model.MaxDiscountAmount_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.MaxDiscountAmount, storeScope);
            model.StrictAddressMatch_OverrideForStore =
                await _settingService.SettingExistsAsync(settings, x => x.StrictAddressMatch, storeScope);
        }

        return View("~/Plugins/Shipping.ShipmentMergeDiscount/Views/Settings.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Settings(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Settings();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeScope);

        settings.Enabled = model.Enabled;
        settings.MinimumEligibleOrders = model.MinimumEligibleOrders;
        settings.UsePercentage = model.UsePercentage;
        settings.DiscountPercentage = model.DiscountPercentage;
        settings.DiscountAmount = model.DiscountAmount;
        settings.MaxDiscountAmount = model.MaxDiscountAmount;
        settings.StrictAddressMatch = model.StrictAddressMatch;

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MinimumEligibleOrders, model.MinimumEligibleOrders_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UsePercentage, model.UsePercentage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DiscountPercentage, model.DiscountPercentage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DiscountAmount, model.DiscountAmount_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MaxDiscountAmount, model.MaxDiscountAmount_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.StrictAddressMatch, model.StrictAddressMatch_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Shipping.ShipmentMergeDiscount.Saved"));

        return await Settings();
    }

    #endregion
}
