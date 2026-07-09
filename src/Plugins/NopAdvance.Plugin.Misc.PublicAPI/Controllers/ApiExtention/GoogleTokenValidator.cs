using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Nop.Core;
using Nop.Services.Logging;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleUserInfo> ValidateTokenAsync(string idToken, string platform = "web");
    }

    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly GoogleMobileExternalAuthSettings _settings;
        private readonly ILogger _logger;

        public GoogleTokenValidator(GoogleMobileExternalAuthSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<GoogleUserInfo> ValidateTokenAsync(string idToken, string platform = "web")
        {
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings();

                // Set audience based on platform
                switch (platform.ToLower())
                {
                    case "android":
                        if (!string.IsNullOrEmpty(_settings.AndroidClientId))
                            validationSettings.Audience = new[] { _settings.AndroidClientId };
                        break;
                    case "ios":
                        if (!string.IsNullOrEmpty(_settings.IOSClientId))
                            validationSettings.Audience = new[] { _settings.IOSClientId };
                        break;
                    default: // web
                        if (!string.IsNullOrEmpty(_settings.ClientId))
                            validationSettings.Audience = new[] { _settings.ClientId };
                        break;
                }

                // Validate the token
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

                // Check if email is verified
                if (!payload.EmailVerified)
                {
                    await _logger.WarningAsync($"Google login attempted with unverified email: {payload.Email}");
                    return null;
                }

                return new GoogleUserInfo
                {
                    Subject = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName,
                    FullName = payload.Name,
                    Picture = payload.Picture,
                    Locale = payload.Locale
                };
            }
            catch (InvalidJwtException ex)
            {
                await _logger.ErrorAsync($"Google token validation failed for {platform}: {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Unexpected error during Google token validation: {ex.Message}", ex);
                return null;
            }
        }
    }

    public class GoogleUserInfo
    {
        public string Subject { get; set; }          // Unique Google user ID
        public string Email { get; set; }            // User's email address
        public bool EmailVerified { get; set; }      // Whether email is verified
        public string GivenName { get; set; }        // First name
        public string FamilyName { get; set; }       // Last name
        public string FullName { get; set; }         // Full name
        public string Picture { get; set; }          // Profile picture URL
        public string Locale { get; set; }           // Language/locale
    }
}