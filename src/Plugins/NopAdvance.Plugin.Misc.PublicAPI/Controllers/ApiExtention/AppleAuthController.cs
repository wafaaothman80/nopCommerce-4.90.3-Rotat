using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    [Route("api/apple-auth")]
    [ApiController]
    public class AppleAuthController : BaseAPIController
    {
        #region Fields

        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly INotificationService _notificationService;
        private readonly IAPIService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IAppleAuthenticationService _appleAuthenticationService;
        private readonly AppleExternalAuthSettings _appleSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public AppleAuthController(
         IExternalAuthenticationService externalAuthenticationService,
         ILogger logger,
         ICustomerService customerService,
         IAuthenticationService authenticationService,
         ICustomerRegistrationService customerRegistrationService,
         INotificationService notificationService,
         IAPIService apiService,
         IHttpContextAccessor httpContextAccessor,
         IWorkContext workContext,
         IAppleAuthenticationService appleAuthenticationService,  IShoppingCartService shoppingCartService,
         IEventPublisher eventPublisher,
       ICustomerActivityService customerActivityService,
        ILocalizationService localizationService, IPermissionService permissionService)
        {
            _externalAuthenticationService = externalAuthenticationService;
            _logger = logger;
            _customerService = customerService;
            _authenticationService = authenticationService;
            _customerRegistrationService = customerRegistrationService;
            _notificationService = notificationService;
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _appleAuthenticationService = appleAuthenticationService;
            _shoppingCartService = shoppingCartService;
            _eventPublisher = eventPublisher;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _permissionService = permissionService;
             
            // Remove this line: _appleSettings = new AppleExternalAuthSettings();
        }

        #endregion

        #region Methods

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateWithApple([FromBody] AppleAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Apple ID token is required"
                    });
                }

                // Validate the token using your Apple authentication service
                var isValid = await _appleAuthenticationService.ValidateTokenAsync(request.IdToken, _appleSettings);
                if (!isValid)
                {
                    await _logger.WarningAsync("Apple Auth: Invalid ID token received");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid Apple ID token"
                    });
                }

                // Extract user information using your Apple authentication service
                var userInfo = await _appleAuthenticationService.ExtractUserInfoFromTokenAsync(request.IdToken, _appleSettings);
                if (userInfo == null || string.IsNullOrEmpty(userInfo.UserId))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to extract user information from Apple token"
                    });
                }

                // Handle user object if provided (contains name information)
                //if (!string.IsNullOrEmpty(request.User))
                //{
                //    userInfo = ParseAppleUserObject(request.User, userInfo);
                //}

                // Check if user exists, if not create one
                var customer = await _customerService.GetCustomerByEmailAsync(userInfo.Email)
                    ?? await CreateCustomerFromAppleInfo(userInfo);

                if (customer == null)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to create or find customer"
                    });
                }

                // Create claims from Apple user info
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserId ?? string.Empty),
                    new Claim(ClaimTypes.Email, userInfo.Email ?? string.Empty),
                    // Keep Apple's "email_verified" claim name (use boolean type)
                    new Claim("email_verified", userInfo.EmailVerified ? "true" : "false", ClaimValueTypes.Boolean)
                };

                // add given / surname if present
                if (!string.IsNullOrEmpty(userInfo.FirstName))
                    claims.Add(new Claim(ClaimTypes.GivenName, userInfo.FirstName));

                if (!string.IsNullOrEmpty(userInfo.LastName))
                    claims.Add(new Claim(ClaimTypes.Surname, userInfo.LastName));

                // add display name when available
                if (!string.IsNullOrEmpty(userInfo.FullName))
                    claims.Add(new Claim(ClaimTypes.Name, userInfo.FullName));

                // Convert Claims to ExternalAuthenticationClaim
                var externalClaims = claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList();

                // Create external authentication parameters
                var parameters = new ExternalAuthenticationParameters
                {
                    ProviderSystemName = "ExternalAuth.Apple",
                    AccessToken = request.IdToken,
                    Email = userInfo.Email,
                    ExternalIdentifier = userInfo.UserId,
                    ExternalDisplayIdentifier = userInfo.Email,
                    Claims = externalClaims
                };

                // Associate external account with customer
                await _externalAuthenticationService.AssociateExternalAccountWithUserAsync(customer, parameters);

                // Sign in the customer
                await _authenticationService.SignInAsync(customer, true);

                await _logger.InformationAsync($"User {customer.Email} successfully logged in via Apple");

                var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
                if (applicationId == 0)
                {
                    await _logger.ErrorAsync("Application ID could not be determined. Invalid API key.");
                    return BadRequest(new { Error = MessageDefaults.INVALID_API_KEY });
                }

                await _logger.InformationAsync($"Generating API tokens for customer ID: {customer.Id}");

                // Use your own method instead of trying to access the private one
             //   var loginResponse = await SignInCustomerAsync(customer, applicationId);

                //if (loginResponse.Success)
                //{
                   


                   
                    var authResponse = await _apiService.GenerateTokensAsync(customer, applicationId);
                    if (authResponse == null)
                    {
                        await _logger.ErrorAsync($"Apple Auth: Failed to generate tokens for customer ID: {customer.Id}");
                        return BadRequest(new ApiResponse
                        {
                            Success = false,
                            Message = "Failed to generate authentication tokens"
                        });
                    }
                return Ok(await SignInCustomerAsync(customer, applicationId));
                //return Ok(new ApiResponse<AuthenticationResponse>
                //    {
                //        Success = true,
                //        Data = authResponse,
                //        Message = "Authentication successful"
                //    });
                //}
                //else
                //{
                //    return BadRequest(new ApiResponse<LoginResponse>
                //    {
                //        Success = false,
                //        Data = loginResponse,
                //        Message = loginResponse.Message
                //    });
                //}
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Apple Auth: Authentication failed", ex);
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

        //[HttpPost("register")]
        //public async Task<IActionResult> RegisterWithApple([FromBody] AppleAuthRequest request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(request.IdToken))
        //        {
        //            return BadRequest(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "Apple ID token is required"
        //            });
        //        }

        //        // Validate the token using your Apple authentication service
        //        var isValid = await _appleAuthenticationService.ValidateTokenAsync(request.IdToken, _appleSettings);
        //        if (!isValid)
        //        {
        //            return BadRequest(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "Invalid Apple ID token"
        //            });
        //        }

        //        // Extract user information using your Apple authentication service
        //        var userInfo = await _appleAuthenticationService.ExtractUserInfoFromTokenAsync(request.IdToken, _appleSettings);
        //        if (userInfo == null || string.IsNullOrEmpty(userInfo.UserId))
        //        {
        //            return BadRequest(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "Failed to extract user information"
        //            });
        //        }

        //        // Check if user already exists
        //        var existingCustomer = await _customerService.GetCustomerByEmailAsync(userInfo.Email);
        //        if (existingCustomer != null)
        //        {
        //            return BadRequest(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "User already exists with this email"
        //            });
        //        }

        //        // Create new customer
        //        var customer = await CreateCustomerFromAppleInfo(userInfo);
        //        if (customer == null)
        //        {
        //            return BadRequest(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "Failed to create customer"
        //            });
        //        }

        //        // Create claims
        //        var claims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
        //            new Claim(ClaimTypes.Email, userInfo.Email ?? "")
        //        };

        //        // Convert Claims to ExternalAuthenticationClaim
        //        var externalClaims = claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList();

        //        var parameters = new ExternalAuthenticationParameters
        //        {
        //            ProviderSystemName = "ExternalAuth.Apple",
        //            AccessToken = request.IdToken,
        //            Email = userInfo.Email,
        //            ExternalIdentifier = userInfo.UserId,
        //            ExternalDisplayIdentifier = userInfo.Email,
        //            Claims = externalClaims
        //        };

        //        // Associate external account with customer
        //        await _externalAuthenticationService.AssociateExternalAccountWithUserAsync(customer, parameters);

        //        // Sign in the customer
        //        await _authenticationService.SignInAsync(customer, true);

        //        await _logger.InformationAsync($"Apple Auth: Successful registration for user {userInfo.Email}");

        //        var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
        //        if (applicationId == 0)
        //        {
        //            await _logger.ErrorAsync("Application ID could not be determined. Invalid API key.");
        //            return BadRequest(new { Error = MessageDefaults.INVALID_API_KEY });
        //        }

        //        var loginResponse = await SignInCustomerAsync(customer, applicationId);

        //        if (loginResponse.Success)
        //        {
        //            return Ok(new ApiResponse<LoginResponse>
        //            {
        //                Success = true,
        //                Data = loginResponse,
        //                Message = "Registration successful"
        //            });
        //        }
        //        else
        //        {
        //            return BadRequest(new ApiResponse<LoginResponse>
        //            {
        //                Success = false,
        //                Data = loginResponse,
        //                Message = loginResponse.Message
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.ErrorAsync("Apple Auth: Registration failed", ex);
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = "Registration failed",
        //            Error = ex.Message
        //        });
        //    }
        //}

        //[HttpPost("logout")]
        //public async Task<IActionResult> Logout()
        //{
        //    try
        //    {
        //        await _authenticationService.SignOutAsync();

        //        return Ok(new ApiResponse
        //        {
        //            Success = true,
        //            Message = "Logged out successfully"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.ErrorAsync("Apple Auth: Logout failed", ex);
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = "Logout failed",
        //            Error = ex.Message
        //        });
        //    }
        //}

        //[HttpGet("user-info")]
        //public async Task<IActionResult> GetUserInfo()
        //{
        //    try
        //    {
        //        var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
        //        if (customer == null)
        //        {
        //            return Unauthorized(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "User not authenticated"
        //            });
        //        }

        //        return Ok(new ApiResponse<CustomerInfoResponse>
        //        {
        //            Success = true,
        //            Data = new CustomerInfoResponse
        //            {
        //                Email = customer.Email,
        //                FirstName = customer.FirstName,
        //                LastName = customer.LastName,
        //                Username = customer.Username
        //            },
        //            Message = "User info retrieved successfully"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.ErrorAsync("Apple Auth: Failed to get user info", ex);
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = "Failed to get user info",
        //            Error = ex.Message
        //        });
        //    }
        //}

        #endregion

        #region Private Methods



        private AppleUserInfo ParseAppleUserObject(string userJson, AppleUserInfo existingInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(userJson))
                    return existingInfo;

                // Apple sends user information as a JSON string in the initial response
                var userData = System.Text.Json.JsonSerializer.Deserialize<AppleUserData>(userJson);

                if (userData?.Name != null)
                {
                    // Update with the complete user information from the user object
                    existingInfo.FirstName = userData.Name.FirstName ?? existingInfo.FirstName;
                    existingInfo.LastName = userData.Name.LastName ?? existingInfo.LastName;
                }

                return existingInfo;
            }
            catch (Exception ex)
            {
                _logger.ErrorAsync("Failed to parse Apple user object", ex).Wait();
                return existingInfo;
            }
        }

        private async Task<Customer> CreateCustomerFromAppleInfo(AppleUserInfo userInfo)
        {
            try
            {
                // Create customer
                var customer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = userInfo.Email,
                    Username = userInfo.Email, // Use email as username
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow
                };

                // Add customer
                await _customerService.InsertCustomerAsync(customer);

                // Set first and last name if available
                if (!string.IsNullOrEmpty(userInfo.FirstName))
                    customer.FirstName = userInfo.FirstName;

                if (!string.IsNullOrEmpty(userInfo.LastName))
                    customer.LastName = userInfo.LastName;

                // Update customer
                await _customerService.UpdateCustomerAsync(customer);

                // Add to registered role
                var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
                if (registeredRole != null)
                {
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
                    {
                        CustomerId = customer.Id,
                        CustomerRoleId = registeredRole.Id
                    });
                }

                await _logger.InformationAsync($"Apple Auth: Created new customer for {userInfo.Email}");

                return customer;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Apple Auth: Failed to create customer for {userInfo.Email}", ex);
                return null;
            }
        }

        private DateTime? GetDateTimeFromUnix(string unixTimestamp)
        {
            if (string.IsNullOrEmpty(unixTimestamp) || !long.TryParse(unixTimestamp, out long seconds))
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }

        #endregion

        #region Request/Response Models

        public class AppleAuthRequest
        {
            public string IdToken { get; set; }
          //  public string AccessToken { get; set; }
            public string AuthorizationCode { get; set; }
         //   public string User { get; set; } // Contains user object with name information
          //  public string ReturnUrl { get; set; }
         //   public int ApplicationId { get; set; }
        }

        public class AppleAuthResponse
        {
            public string Email { get; set; }
            public string AppleUserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ReturnUrl { get; set; }
            public bool IsEmailVerified { get; set; }
        }

        public class CustomerInfoResponse
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Username { get; set; }
        }

        public class AppleUserInfo
        {
            // Basic user information
            public string UserId { get; set; }
            public string Email { get; set; }
            public bool EmailVerified { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            // Token information
            public string Issuer { get; set; }
            public string Audience { get; set; }
            public DateTime? IssuedAt { get; set; }
            public DateTime? Expiration { get; set; }

            // Authentication context
            public DateTime? AuthTime { get; set; }
            public string Nonce { get; set; }

            // Computed properties
            public string FullName => $"{FirstName} {LastName}".Trim();
            public bool IsTokenValid => Expiration.HasValue && Expiration.Value > DateTime.UtcNow;
        }

        public class AppleUserData
        {
            public AppleUserName Name { get; set; }
            public string Email { get; set; }
        }

        public class AppleUserName
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        //public class LoginResponse
        //{
        //    public bool Success { get; set; }
        //    public string Email { get; set; }
        //    public string FirstName { get; set; }
        //    public string LastName { get; set; }
        //    public string Username { get; set; }
        //    public int ApplicationId { get; set; }
        //    public string Message { get; set; }
        //    public string Error { get; set; }
        //}

        public class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Error { get; set; }
        }

        public class ApiResponse<T> : ApiResponse
        {
            public T Data { get; set; }
        }

        #endregion
    }
}