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
using Nop.Core.Domain.Customers;
using Nop.Services.Security;
using NopAdvance.Plugin.Core.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

public static class PluginDefaults
{
    public const string SYSTEM_NAME = "NopAdvance.Plugin.PublicAPI";

    public const string PUBLIC_BASEURL_PREFIX = "/api";

    public const string PUBLIC_BASEURL = "api/[controller]/[action]";

    public const string REFRESH_TOKEN_MANAGEMENT_VIEWCOMPONENT = "NopAdvanceRefreshTokenManagement";

    public const string SWAGGER_BASE_PATH = "nopadvancepublicapi";

    public const string SWAGGER_TITLE = "nopCommerce Public API";

    public const string ROUTE_PREFIX = "api";

    public const string CONTENT_TYPE_APPLICATION_JSON = "application/json";
    
    public const string PLUGIN_VERSION = "1.14";

    public const string SCHEMA_MIGRATION = $"{SYSTEM_NAME} {PLUGIN_VERSION} base schema";

    public const string DATA_MIGRATION = $"{SYSTEM_NAME} {PLUGIN_VERSION}. Update Data";

    public static IList<PermissionConfig> AllConfigs => new List<PermissionConfig>{new (
                CoreDefaults.PERMISSION_RECORD_NAME_PREFIX_ADMIN + "Manage NopCommerce Public RESTful API",
                SYSTEM_NAME,
                CoreDefaults.PERMISSION_RECORD_CATEGORY,
                NopCustomerDefaults.AdministratorsRoleName)};
}

public static class SiteMapDefaults
{
    public const string MAIN_MENU_SYSTEM_NAME = "NopAdvance PublicAPI";

    public const string CONFIGURE_MENU_SYSTEM_NAME = MAIN_MENU_SYSTEM_NAME + " Configure";

    public const string ADMIN_PUBLIC_API_CONTROLLER_NAME = "NopAdvancePublicAPI";

    public const string ADMIN_CONFIGURE_CONTROLLER_NAME = "NopAdvancePublicAPIConfigure";

    public const string CONFIGURE_ACTION_NAME = "Configure";

    public const string APPLICATION_LIST_ACTION_NAME = "Applications";

    public const string TOKEN_LIST_ACTION_NAME = "Tokens";

    public const string HELP_MENU_SYSTEM_NAME = MAIN_MENU_SYSTEM_NAME + " Help";

    public const string CONFIGURATION_PAGE_URL = "Admin/" + ADMIN_CONFIGURE_CONTROLLER_NAME + "/" + CONFIGURE_ACTION_NAME;

    public const string APPLICATIONS_MENU_SYSTEM_NAME = MAIN_MENU_SYSTEM_NAME + " Applications";

    public const string TOKENS_MENU_SYSTEM_NAME = MAIN_MENU_SYSTEM_NAME + " Tokens";

    public const string DEBUG_LOG_LIST_ACTION_NAME = "DebugLogs";

    public const string DEBUG_LOG_MENU_SYSTEM_NAME = MAIN_MENU_SYSTEM_NAME + " Debug Log";
}

public static class LocaleResourceDefaults
{
    public const string CONFIGURE_RESTART_NOTE = PluginDefaults.SYSTEM_NAME + ".Configure.RestartNote";

    public const string CONFIGURE_ENABLE_API = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.EnableAPI";

    public const string CONFIGURE_ENABLE_SWAGGER = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.EnableSwagger";

    public const string CONFIGURE_IS_DEVELOPMENT = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.IsDevelopment";

    public const string CONFIGURE_SECRET_KEY = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.SecretKey";

    public const string CONFIGURE_SECURITY_ALGORITHM_TYPE = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.SecurityAlgorithmType";

    public const string CONFIGURE_ACCESS_TOKEN_EXPIRATION = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.AccessTokenExpiration";

    public const string CONFIGURE_REFRESH_TOKEN_EXPIRATION = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.RefreshTokenExpiration";

    public const string CONFIGURE_ENABLE_DEBUGGING = PluginDefaults.SYSTEM_NAME + ".Configure.Fields.EnableDebugging";

    public const string CONFIGURE_SECURITY_KEY_CHANGED = PluginDefaults.SYSTEM_NAME + ".Configure.SecurityKey.Changed";

    public const string LOGIN_USERNAMEOREMAIL_REQUIRED = PluginDefaults.SYSTEM_NAME + ".Login.UsernameOrEmail.Required";

    public const string LOGIN_PASSWORD_REQUIRED = PluginDefaults.SYSTEM_NAME + ".Login.Password.Required";

    public const string BASE_STORE_REQUIRED_MESSAGE = "Store identifier is required.";

    public const string APPLICATIONS_MENU = PluginDefaults.SYSTEM_NAME + ".Applications";

    public const string SEARCH_APPLICATION_NAME = PluginDefaults.SYSTEM_NAME + ".Applications.List.SearchApplicationName";

    public const string APPLICATION_NAME = PluginDefaults.SYSTEM_NAME + ".Applications.Fields.Name";

    public const string APPLICATION_STORE_ID = PluginDefaults.SYSTEM_NAME + ".Applications.Fields.Store";

    public const string API_KEY = PluginDefaults.SYSTEM_NAME + ".Applications.Fields.APIKey";

    public const string APPLICATION_ACTIVE = PluginDefaults.SYSTEM_NAME + ".Applications.Fields.Active";

    public const string DELETE_TOKENS = PluginDefaults.SYSTEM_NAME + ".Maintenance.DeleteTokens";

    public const string DELETE_TOKENS_TOTAL_DELETED = PluginDefaults.SYSTEM_NAME + ".Maintenance.DeleteTokens.TotalDeleted";

    public const string DEBUG_LOG_MENU = PluginDefaults.SYSTEM_NAME + ".DebugLog";

    public const string DEBUG_LOG_CUSTOMER_ID = DEBUG_LOG_MENU + ".Fields.Customer";

    public const string DEBUG_LOG_STORE_ID = DEBUG_LOG_MENU + ".Fields.Store";

    public const string DEBUG_LOG_STATUS_CODE = DEBUG_LOG_MENU + ".Fields.StatusCode";

    public const string DEBUG_LOG_METHOD = DEBUG_LOG_MENU + ".Fields.Method";

    public const string DEBUG_LOG_HEADERS = DEBUG_LOG_MENU + ".Fields.Headers";

    public const string DEBUG_LOG_REQUEST_BODY = DEBUG_LOG_MENU + ".Fields.RequestBody";

    public const string DEBUG_LOG_QUERY_STRING = DEBUG_LOG_MENU + ".Fields.QueryString";

    public const string DEBUG_LOG_RESPONSE_BODY = DEBUG_LOG_MENU + ".Fields.ResponseBody";

    public const string DEBUG_LOG_RESPONSE_TIME = DEBUG_LOG_MENU + ".Fields.ResponseTime";

    public const string DEBUG_LOG_PATH = DEBUG_LOG_MENU + ".Fields.Path";

    public const string DEBUG_LOG_CREATED_ON_UTC = DEBUG_LOG_MENU + ".Fields.CreatedOnUtc";

    public const string DEBUG_LOG_SEARCH_CREATED_ON_FROM = DEBUG_LOG_MENU + ".Search.Fields.CreatedOnFrom";

    public const string DEBUG_LOG_SEARCH_CREATED_ON_TO = DEBUG_LOG_MENU + ".Search.Fields.CreatedOnTo";

    public const string DEBUG_LOG_SEARCH_STORES = DEBUG_LOG_MENU + ".Search.Fields.Stores";

    public const string DEBUG_LOG_CLEARED = DEBUG_LOG_MENU + ".Cleared";

    public const string DEBUG_LOG_DELETED = DEBUG_LOG_MENU + ".Deleted";

    public const string DEBUG_LOG_VIEW_DETAILS = DEBUG_LOG_MENU + ".DebugLogViewDetails";

    public const string DEBUG_LOG_BACK_LIST = DEBUG_LOG_MENU + ".BackToList";

    public const string MAINTENANCE_TOKEN_INCLUDE_REVOKED = PluginDefaults.SYSTEM_NAME + ".Maintenance.Token.IncludeRevoked";

    public const string TOKENS_MENU = PluginDefaults.SYSTEM_NAME + ".Tokens";

    public const string TOKEN_LIST_SEARCH_APPLICATION = TOKENS_MENU + ".List.SearchApplication";

    public const string TOKEN_LIST_CUSTOMER_ROLES = TOKENS_MENU + ".List.CustomerRoles";

    public const string TOKEN_LIST_SEARCH_EMAIL = TOKENS_MENU + ".List.SearchEmail";

    public const string TOKEN_LIST_SEARCH_FIRSTNAME = TOKENS_MENU + ".List.SearchFirstname";

    public const string TOKEN_LIST_SEARCH_LASTNAME = TOKENS_MENU + ".List.SearchLastName";

    public const string TOKEN_REVOKE = TOKENS_MENU + ".Revoke";

    public const string TOKEN_REVOKE_SELECTED = TOKENS_MENU + ".RevokeSelected";

    public const string TOKEN_REVOKE_WARNING = TOKENS_MENU + ".Revoke.Warning";

    public const string TOKEN_REVOKE_SELECTED_WARNING = TOKENS_MENU + ".RevokeSelected.Warning";

    public const string TOKEN_APPLICATION = TOKENS_MENU + ".Fields.ApplicationName";

    public const string TOKEN_CUSTOMERROLES_NOCUSTOMERROLESAVAILABLE = TOKENS_MENU + ".CustomerRoles.NoCustomerRolesAvailable";

    public const string INVALID_LICENSE = PluginDefaults.SYSTEM_NAME + ".License.InValid";
}

public static class AuthenticationDefaults
{
    public const string API_KEY_NAME = "X-API-KEY";

    public const string AUTHORIZATION_KEY_NAME = "Authorization";

    public const string CLAIMS_CUSTOMER_ID = "CustomerId";

    public const string CURRENT_API_CUSTOMER = "APICustomer";

    public const string TOKEN_ERROR_MESSAGE = "TokenErrorMessage";
}

public static class MessageDefaults
{
    public const string DISABLED_FROM_SETTINGS = "Disabled from settings";

    public const string NOT_FOUND = "{0} not found";
    
    public const string INVALID_API_KEY = "Invalid Api Key";

    public const string PAYMENT_METHOD_REQUIRED = "Please select payment method";

    public const string PAYMENT_METHOD_NOT_FOUND = "Payment method not found";
}

public static class PaymentMethodDefaults
{
    //Standard
    public const string MANUAL = "Payments.Manual";
    
    public const string AUTHORIZE_NET = "Payments.AuthorizeNet";
    
    public const string CHECK_MONEY_ORDER = "Payments.CheckMoneyOrder";
    
    public const string BRAIN_TREE = "Payments.BrainTree";
    
    public const string PURCHASE_ORDER = "Payments.PurchaseOrder";

    //Redirection
    public const string PAY_PAL_STANDARD = "Payments.PayPalStandard";
    
    public const string SKRILL = "Payments.Skrill";
    
    public const string TWO_CHECKOUT = "Payments.TwoCheckout";
}
