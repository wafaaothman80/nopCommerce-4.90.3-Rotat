using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Controllers;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public class GoogleMobileAuthController : BaseAPIController
    {
        #region Fields

        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger _logger;
        private readonly GoogleMobileAuthSettings _googleMobileSettings;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly Dictionary<string, string[]> _clientIds;
        private readonly IAPIService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        #endregion

        #region Ctor

        public GoogleMobileAuthController(
            IAuthenticationService authenticationService,
            ILogger logger,
            GoogleMobileAuthSettings googleMobileSettings,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService, IAPIService apiService, IHttpContextAccessor httpContextAccessor, IWorkContext workContext, IShoppingCartService shoppingCartService, IEventPublisher eventPublisher, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPermissionService permissionService)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _googleMobileSettings = googleMobileSettings;
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;

            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _shoppingCartService = shoppingCartService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            // Initialize client IDs in the main constructor
            _clientIds = new Dictionary<string, string[]>
            {
                ["android"] = new[]
     {
        "455994608888-4nrqgsqago1e0nrh864osu3ucievc2es.apps.googleusercontent.com",
        "455994608888-b6d3n1nd4j3o87lndtti6qeid3o6vqea.apps.googleusercontent.com",
        "455994608888-u5mqltvb46gohr9r8t2flp91thlhp1e6.apps.googleusercontent.com"
    },
                ["ios"] = new[]
     {
        "455994608888-jed553sam2ohifuv2ajh7f6lscujo5mm.apps.googleusercontent.com",
        "455994608888-u5mqltvb46gohr9r8t2flp91thlhp1e6.apps.googleusercontent.com"
    },
                ["web"] = new[]
     {
        "455994608888-u5mqltvb46gohr9r8t2flp91thlhp1e6.apps.googleusercontent.com"
    }
            };
            _workContext = workContext;
            _customerActivityService = customerActivityService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        //[HttpPost("login")]
        //[AllowAnonymous]
        //public virtual async Task<IActionResult> Login(GoogleLoginModel model)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(model?.IdToken))
        //            return BadRequest(new { success = false, message = "Invalid token" });

        //        if (string.IsNullOrEmpty(model.Platform))
        //            model.Platform = "android";

        //        await _logger.InformationAsync($"=== Starting Google Token Validation ===");
        //        await _logger.InformationAsync($"Platform: {model.Platform}");
        //        await _logger.InformationAsync($"Token length: {model.IdToken?.Length}");

        //        // First, let's debug the token contents
        //        var tokenDebugInfo = await DebugTokenContentsAsync(model.IdToken);
        //        if (tokenDebugInfo != null)
        //        {
        //            await _logger.InformationAsync($"Token Debug - Audience: {tokenDebugInfo.Audience}");
        //            await _logger.InformationAsync($"Token Debug - Issuer: {tokenDebugInfo.Issuer}");
        //            await _logger.InformationAsync($"Token Debug - Email: {tokenDebugInfo.Email}");
        //            await _logger.InformationAsync($"Token Debug - Expiration: {tokenDebugInfo.Expiration}");
        //            await _logger.InformationAsync($"Token Debug - Current UTC: {DateTime.UtcNow}");
        //            await _logger.InformationAsync($"Token Debug - Is Expired: {tokenDebugInfo.Expiration < DateTime.UtcNow}");
        //        }

        //        // Try validation
        //        var googleUser = await ValidateGoogleTokenWithFallbackAsync(model.IdToken, model.Platform);
        //        if (googleUser == null)
        //            return BadRequest(new { success = false, message = "All token validation methods failed" });

        //        if (!googleUser.EmailVerified)
        //            return BadRequest(new { success = false, message = "Email not verified by Google" });

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Login successful",
        //            user = new
        //            {
        //                googleUser.Email,
        //                googleUser.FullName,
        //                googleUser.Picture,
        //                googleUser.Subject
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.ErrorAsync($"Google mobile login error: {ex.Message}", ex);
        //        return BadRequest(new { success = false, message = $"Login failed: {ex.Message}" });
        //    }
        //}

        [HttpPost("login")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Login(GoogleLoginModel model)
        {
            try 
            {
                if (string.IsNullOrEmpty(model?.IdToken))
                    return BadRequest(new { success = false, message = "Invalid token" });

                if (string.IsNullOrEmpty(model.Platform))
                    model.Platform = "android";

                await _logger.InformationAsync($"=== Starting Google Token Validation ===");
                await _logger.InformationAsync($"Platform: {model.Platform}");
                await _logger.InformationAsync($"Token length: {model.IdToken?.Length}");

                // First, let's debug the token contents
                var tokenDebugInfo = await DebugTokenContentsAsync(model.IdToken);
                if (tokenDebugInfo != null)
                {
                    await _logger.InformationAsync($"Token Debug - Audience: {tokenDebugInfo.Audience}");
                    await _logger.InformationAsync($"Token Debug - Issuer: {tokenDebugInfo.Issuer}");
                    await _logger.InformationAsync($"Token Debug - Email: {tokenDebugInfo.Email}");
                    await _logger.InformationAsync($"Token Debug - Expiration: {tokenDebugInfo.Expiration}");
                    await _logger.InformationAsync($"Token Debug - Current UTC: {DateTime.UtcNow}");
                    await _logger.InformationAsync($"Token Debug - Is Expired: {tokenDebugInfo.Expiration < DateTime.UtcNow}");
                }

                // Try validation
                var googleUser = await ValidateGoogleTokenWithFallbackAsync(model.IdToken, model.Platform);
                if (googleUser == null)
                    return BadRequest(new { success = false, message = "All token validation methods failed" });

                if (!googleUser.EmailVerified)
                    return BadRequest(new { success = false, message = "Email not verified by Google" });

                // Find or create customer
                var customer = await FindOrCreateCustomerAsync(googleUser);
                if (customer == null)
                    return BadRequest(new { success = false, message = "Failed to create or find user account" });

                // Login the customer
                await _authenticationService.SignInAsync(customer, true);

                await _logger.InformationAsync($"User {customer.Email} successfully logged in via Google");

                var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
                if (applicationId == 0)
                {
                    _logger.Error("Application ID could not be determined. Invalid API key.");
                    return BadRequest(new ErrorResponse { Error = MessageDefaults.INVALID_API_KEY });
                }

               
                _logger.Information($"Generating API tokens for customer ID: {customer.Id}");
                return Ok(await SignInCustomerAsync(customer, applicationId));


                //return Ok(new
                //{
                //    success = true,
                //    message = "Login successful",
                //    user = new
                //    {
                //        customer.Id,
                //        customer.Email,
                //        customer.Username,
                //        googleUser.FullName,
                //        googleUser.Picture,
                //        googleUser.Subject,
                //        IsNewUser = customer.CreatedOnUtc > DateTime.UtcNow.AddMinutes(-5) // Rough check if user was just created
                //    }
                //});
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Google mobile login error: {ex.Message}", ex);
                return BadRequest(new { success = false, message = $"Login failed: {ex.Message}" });
            }
        }

        [HttpPost("signin-customer")]
        public virtual async Task<LoginResponse> SignInCustomerAsync(Customer customer, int applicationId)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var previousCustomerId = 0;
            if (!currentCustomer.IsSystemAccount && currentCustomer?.Id != customer.Id)
            {
                previousCustomerId = currentCustomer.Id;

                //migrate shopping cart
                await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

                await _workContext.SetCurrentCustomerAsync(customer);
            }

            //sign in new customer
            await _authenticationService.SignInAsync(customer, false);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

            if (previousCustomerId > 0)
            {
                var accessTokenId = _apiService.GetTokenId(_httpContextAccessor.HttpContext.Request.Headers[AuthenticationDefaults.AUTHORIZATION_KEY_NAME].FirstOrDefault());
                if (accessTokenId.HasValue)
                {
                    var refreshToken = await _apiService.GetAPIRefreshTokenAsync(applicationId, previousCustomerId, accessTokenId.Value);
                    if (refreshToken != null)
                        await _apiService.DeleteAPIRefreshTokenAsync(refreshToken);
                }
            }

            var tokens = await _apiService.GenerateTokensAsync(customer, applicationId);
            var response = new LoginResponse
            {
                CustomerId = tokens.CustomerId,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                IsImpersonationAllowed = await _permissionService.AuthorizeAsync("AllowCustomerImpersonation", customer),
            };

            return response;
        }



        private async Task<Customer> FindOrCreateCustomerAsync(GoogleUserInfo googleUser)
        {
            try
            {
                await _logger.InformationAsync($"Looking for customer with email: {googleUser.Email}");

                // Try to find existing customer by email
                var customer = await _customerService.GetCustomerByEmailAsync(googleUser.Email);

                if (customer != null)
                {
                    await _logger.InformationAsync($"Found existing customer: {customer.Email} (ID: {customer.Id})");
                    return customer;
                }

                await _logger.InformationAsync($"No existing customer found, creating new customer for: {googleUser.Email}");

                // Create new customer - FIRST save the customer without password
                var newCustomer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = googleUser.Email,
                    Username = googleUser.Email, // Using email as username
                    FirstName = googleUser.GivenName,
                    LastName = googleUser.FamilyName,
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = 1 // Use appropriate store ID
                };

                // FIRST: Insert the customer without password
                await _customerService.InsertCustomerAsync(newCustomer);

                await _logger.InformationAsync($"Customer record created with ID: {newCustomer.Id}");

                // SECOND: Add customer to registered role
                var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
                if (registeredRole != null)
                {
                    // Get existing role IDs for the customer
                    var existingRoleIds = await _customerService.GetCustomerRoleIdsAsync(newCustomer);

                    // Check if customer already has the registered role
                    if (!existingRoleIds.Contains(registeredRole.Id))
                    {
                        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
                        {
                            CustomerId = newCustomer.Id,
                            CustomerRoleId = registeredRole.Id
                        });
                        await _logger.InformationAsync($"Added customer to Registered role");
                    }
                    else
                    {
                        await _logger.InformationAsync($"Customer already has Registered role");
                    }
                }

                // THIRD: Create a random password for the customer
                var randomPassword = Guid.NewGuid().ToString();
                await _customerRegistrationService.ChangePasswordAsync(new ChangePasswordRequest(
                    googleUser.Email,
                    false,
                    PasswordFormat.Hashed,
                    randomPassword));

                await _logger.InformationAsync($"Successfully created new customer: {googleUser.Email} (ID: {newCustomer.Id})");

                return newCustomer;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error in FindOrCreateCustomerAsync for {googleUser.Email}: {ex.Message}", ex);
                throw;
            }
        }



        [HttpPost("debug-token")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> DebugToken(GoogleLoginModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model?.IdToken))
                    return BadRequest(new { success = false, message = "Invalid token" });

                var tokenInfo = await DebugTokenContentsAsync(model.IdToken);
                if (tokenInfo == null)
                    return BadRequest(new { success = false, message = "Cannot decode token" });

                var allClientIds = _clientIds.Values.SelectMany(x => x).ToList();
                var matchingClientIds = allClientIds.Where(id => id == tokenInfo.Audience).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Token debug information",
                    data = new
                    {
                        tokenInfo.Audience,
                        tokenInfo.Issuer,
                        tokenInfo.Email,
                        tokenInfo.Expiration,
                        tokenInfo.IssuedAt,
                        tokenInfo.Subject,
                        isExpired = tokenInfo.Expiration < DateTime.UtcNow,
                        matchesClientId = matchingClientIds.Any(),
                        matchingClientIds = matchingClientIds,
                        allConfiguredClientIds = allClientIds,
                        currentUtcTime = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Token debug error: {ex.Message}", ex);
                return BadRequest(new { success = false, message = $"Debug failed: {ex.Message}" });
            }
        }

        private async Task<TokenDebugInfo> DebugTokenContentsAsync(string idToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(idToken))
                {
                    await _logger.WarningAsync("Cannot read JWT token - invalid format");
                    return null;
                }

                var jwtToken = handler.ReadJwtToken(idToken);

                return new TokenDebugInfo
                {
                    Audience = string.Join(", ", jwtToken.Audiences),
                    Issuer = jwtToken.Issuer,
                    Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                    Expiration = jwtToken.ValidTo,
                    IssuedAt = jwtToken.IssuedAt,
                    Subject = jwtToken.Subject
                };
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error debugging token contents: {ex.Message}", ex);
                return null;
            }
        }

        private async Task<GoogleUserInfo> ValidateGoogleTokenWithFallbackAsync(string idToken, string platform)
        {
            // Approach: Manual JWT validation
            try
            {
                await _logger.InformationAsync("Attempting manual JWT validation...");
                return await ValidateManuallyAsync(idToken, platform);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Manual validation failed: {ex.Message}", ex);
            }

            return null;
        }

        private async Task<GoogleUserInfo> ValidateManuallyAsync(string idToken, string platform)
        {
            try
            {
                await _logger.InformationAsync("Starting manual validation...");

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(idToken))
                    throw new Exception("Cannot read JWT token - invalid format");

                var jwtToken = handler.ReadJwtToken(idToken);

                await _logger.InformationAsync($"JWT Token parsed successfully");
                await _logger.InformationAsync($"Audiences: {string.Join(", ", jwtToken.Audiences)}");
                await _logger.InformationAsync($"Issuer: {jwtToken.Issuer}");
                await _logger.InformationAsync($"Expiration: {jwtToken.ValidTo}");
                await _logger.InformationAsync($"Issued At: {jwtToken.IssuedAt}");

                // Extract claims manually
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var emailVerified = jwtToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;
                var picture = jwtToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
                var givenName = jwtToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
                var familyName = jwtToken.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
                var locale = jwtToken.Claims.FirstOrDefault(c => c.Type == "locale")?.Value;

                await _logger.InformationAsync($"Extracted claims - Email: {email}, Name: {name}");

                // Verify audience
                var audience = jwtToken.Audiences.FirstOrDefault();
                var allClientIds = _clientIds.Values.SelectMany(x => x);
                if (!allClientIds.Contains(audience))
                {
                    await _logger.WarningAsync($"Audience mismatch. Token audience: {audience}, Allowed: {string.Join(", ", allClientIds)}");
                    throw new Exception($"Audience '{audience}' not in trusted client IDs");
                }

                await _logger.InformationAsync("Audience validation passed");

                // Verify expiration
                var utcNow = DateTime.UtcNow;
                if (jwtToken.ValidTo < utcNow)
                {
                    await _logger.WarningAsync($"Token expired. Expires: {jwtToken.ValidTo}, Current: {utcNow}");
                    throw new Exception($"Token expired at {jwtToken.ValidTo}. Current time: {utcNow}");
                }

                await _logger.InformationAsync("Expiration validation passed");

                // Verify issuer
                var validIssuers = new[] { "https://accounts.google.com", "accounts.google.com" };
                if (!validIssuers.Contains(jwtToken.Issuer))
                {
                    await _logger.WarningAsync($"Issuer mismatch. Token issuer: {jwtToken.Issuer}, Expected: {string.Join(", ", validIssuers)}");
                    throw new Exception($"Untrusted issuer: {jwtToken.Issuer}");
                }

                await _logger.InformationAsync("Issuer validation passed");

                // Check if email is present
                if (string.IsNullOrEmpty(email))
                    throw new Exception("Email claim is missing from token");

                await _logger.InformationAsync("Manual validation SUCCESSFUL");

                return new GoogleUserInfo
                {
                    Subject = jwtToken.Subject,
                    Email = email,
                    EmailVerified = bool.TryParse(emailVerified, out bool verified) && verified,
                    GivenName = givenName,
                    FamilyName = familyName,
                    FullName = name,
                    Picture = picture,
                    Locale = locale
                };
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Manual validation failed: {ex.Message}", ex);
                throw new Exception($"Manual validation failed: {ex.Message}");
            }
        }

        #endregion

        #region Utility Classes

        public class GoogleUserInfo
        {
            public string Subject { get; set; }
            public string Email { get; set; }
            public bool EmailVerified { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string FullName { get; set; }
            public string Picture { get; set; }
            public string Locale { get; set; }
        }

        public class GoogleLoginModel
        {
            public string IdToken { get; set; }
            public string Platform { get; set; } = "android";
        }

        public class TokenDebugInfo
        {
            public string Audience { get; set; }
            public string Issuer { get; set; }
            public string Email { get; set; }
            public DateTime Expiration { get; set; }
            public DateTime? IssuedAt { get; set; }
            public string Subject { get; set; }
        }

        #endregion
    }
}