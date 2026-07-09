// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopAdvance Core Plugin by NopAdvance team                            *
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
namespace NopAdvance.Plugin.Core.Infrastructure;

public class CoreDefaults
{
    public const string SYSTEM_NAME = "NopAdvance.Core";
    public const string CORE_PLUGIN_VERSION = "1.18";
    public const string ROOT_MENU_SYSTEM_NAME = "NopAdvance";
    public const string PLUGINS_MENU_SYSTEM_NAME = SYSTEM_NAME + ".Plugins";
    public const string THEME_PLUGINS_MENU_SYSTEM_NAME = SYSTEM_NAME + ".Themes";
    public const string DISCOUNT_RULES_PLUGINS_MENU_SYSTEM_NAME = SYSTEM_NAME + ".DiscountRules";
    public const string DEPENDENT_ERROR = "All NopAdvance plugins are dependent on this Core plugin. Please uninstall all others NopAdvance plugins in order to uninstall this plugin.";
    public const string PERMISSION_RECORD_CATEGORY = "NopAdvancePlugins";
    public const string PERMISSION_RECORD_NAME_PREFIX_ADMIN = "Admin area. NopAdvance - ";
}

internal class CorePluginDefaults
{
    public const string DEFAULT_THEME = "DefaultClean";
    public const string PLUGINS_SYSTEM_NAME_PREFIX = "nopadvance.plugin.";
    public const string THEME_PLUGINS_SYSTEM_NAME_PREFIX = "nopadvance.theme.";
    public const string DISCOUNT_RULES_PLUGINS_SYSTEM_NAME_PREFIX = "nopadvance.discountrules.";
    public const string MORE_PLUGINS_MENU_SYSTEM_NAME = CoreDefaults.SYSTEM_NAME + ".MorePlugins";
    public const string WIDGET_ZONE_SYSTEM_NAME = CoreDefaults.SYSTEM_NAME + ".WidgetZones";
    public const string MORE_PLUGINS_URL = "https://store.nopadvance.com/nopcommerce-plugins?utm_source=plugin-menu&utm_medium=help-menu&utm_campaign=more-plugins";

    public const string WIDGET_ZONE_CONTROLLER_NAME = "NopAdvanceCoreWidgetZone";
    public const string WIDGET_ZONE_MANAGE_ACTION_NAME = "Manage";

    public const string LICENSE_SYSTEM_NAME = CoreDefaults.SYSTEM_NAME + ".License";
    public const string LICENSE_CONTROLLER_NAME = "NopAdvanceLicensing";

    public const string LICENSE_INFO_SETTING = "licenseinfo";
    public const string LICENSE_KEY_SETTING = "licensekey";
    public const string ENABLED_KEY_SETTING = "enabled";
}

public class CoreIconClassDefaults
{
    public const string NOPADVANCE = "icon-nop-advance";
    public const string PLUG = "fas fa-plug";
    public const string THEME = "fas fa-paint-brush";
    public const string DISCOUNT_RULE = "fas fa-tags";
    public const string BINOCULARS = "fas fa-binoculars";
    public const string QUESTION = "far fa-question-circle";
    public const string DOT_CIRCLE = "far fa-dot-circle";
    public const string CIRCLE = "far fa-circle";
}

public class CoreLocaleResourceDefaults
{
    public const string PLUGIN_MENU = CoreDefaults.SYSTEM_NAME + ".Plugins";
    public const string THEME_MENU = CoreDefaults.SYSTEM_NAME + ".Themes";
    public const string DISCOUNT_RULES_MENU = CoreDefaults.SYSTEM_NAME + ".DiscountRules";
    public const string MORE_PLUGINS_MENU = CoreDefaults.SYSTEM_NAME + ".MorePlugins";
    public const string CONFIGURE_PAGE_TITLE = CoreDefaults.SYSTEM_NAME + ".Configure";
    public const string SETTINGS_PAGE_TITLE = CoreDefaults.SYSTEM_NAME + ".Settings";
    public const string CONTACT_SUPPORT = CoreDefaults.SYSTEM_NAME + ".ContactSupport";
    public const string CONTACT_SALES = CoreDefaults.SYSTEM_NAME + ".ContactSales";
    public const string HELP_MENU = CoreDefaults.SYSTEM_NAME + ".Help";

    public const string WIDGET_ZONES = CoreDefaults.SYSTEM_NAME + ".WidgetZones";
    public const string WIDGET_ZONES_TITILE = WIDGET_ZONES + ".Title";
    public const string WIDGET_ZONES_FIELD_NAMES = WIDGET_ZONES + ".Fields.WidgetZones";
    public const string WIDGET_ZONES_FIELD_NAMES_HINT = WIDGET_ZONES + ".Fields.WidgetZones.Hint";
    public const string WIDGET_ZONES_FIELD_NAMES_TEXT = WIDGET_ZONES + ".Fields.WidgetZones.Text";
    public const string WIDGET_ZONES_UPDATED = WIDGET_ZONES + ".Updated";

    public const string LICENSE = CoreDefaults.SYSTEM_NAME + ".License";
    public const string LICENSING_TITLE = CoreDefaults.SYSTEM_NAME + ".License.Title";
    public const string LICENSE_FIELD_LICENSEKEY = LICENSE + ".Fields.LicenseKey";

    public const string CONFIGURATION = CoreDefaults.SYSTEM_NAME + ".Plugin.Configure";
    public const string CONFIGURATION_ENABLED = CoreDefaults.SYSTEM_NAME + ".Plugin.Enabled";
}
