using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nop.Services.Configuration;
using static NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention.AppleAuthController;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public interface IAppleAuthenticationService
    {
        string GenerateClientSecret(AppleExternalAuthSettings settings);
        Task<bool> ValidateTokenAsync(string idToken, AppleExternalAuthSettings settings);
        Task<AppleUserInfo> ExtractUserInfoFromTokenAsync(string idToken, AppleExternalAuthSettings settings);
    }

    public class AppleAuthenticationService : IAppleAuthenticationService
    {
        private readonly ISettingService _settingService;

        public AppleAuthenticationService(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public string GenerateClientSecret(AppleExternalAuthSettings settings)
        {
            var keyId = _settingService.GetSettingByKeyAsync<string>("signinwithapplesettings.Mobile.keyid");
            var TeamId = _settingService.GetSettingByKeyAsync<string>("signinwithapplesettings.Mobile.teamid");
            var ClientId = _settingService.GetSettingByKeyAsync<string>("signinwithapplesettings.Mobile.clientid");
            var PrivateKey = _settingService.GetSettingByKeyAsync<string>("signinwithapplesettings.Mobile.privatekey");

            var now = DateTime.UtcNow;
            var expiration = now.AddHours(1);

            var header = new { alg = "ES256", kid = keyId.Result };
            var payload = new
            {
                iss = TeamId.Result,
                iat = ToUnixTime(now),
                exp = ToUnixTime(expiration),
                aud = "https://appleid.apple.com",
                sub = ClientId.Result
            };

            var headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header));
            var payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            var headerBase64 = Base64UrlEncode(headerBytes);
            var payloadBase64 = Base64UrlEncode(payloadBytes);

            var dataToSign = $"{headerBase64}.{payloadBase64}";

            using var ecdsa = ECDsa.Create();
            var privateKeyBytes = Convert.FromBase64String(PrivateKey.Result);
            ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            var signature = ecdsa.SignData(Encoding.UTF8.GetBytes(dataToSign), HashAlgorithmName.SHA256);
            var signatureBase64 = Base64UrlEncode(signature);

            return $"{dataToSign}.{signatureBase64}";
        }

        public async Task<bool> ValidateTokenAsync(string idToken, AppleExternalAuthSettings settings)
        {
            var ClientId = await _settingService.GetSettingByKeyAsync<string>("signinwithapplesettings.Mobile.clientid");
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(idToken);

                // Validate token expiration
                if (token.ValidTo < DateTime.UtcNow)
                    return false;

                // Validate issuer
                if (token.Issuer != "https://appleid.apple.com")
                    return false;

                // Validate audience
                if (token.Audiences.FirstOrDefault() != ClientId)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AppleUserInfo> ExtractUserInfoFromTokenAsync(string idToken, AppleExternalAuthSettings settings)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(idToken))
                    return null;

                var token = handler.ReadJwtToken(idToken);
                var claims = token.Claims.ToList();

                // Extract user information from the token
                var userInfo = new AppleUserInfo
                {
                    UserId = claims.FirstOrDefault(x => x.Type == "sub")?.Value,
                    Email = claims.FirstOrDefault(x => x.Type == "email")?.Value,
                    EmailVerified = claims.FirstOrDefault(x => x.Type == "email_verified")?.Value == "true",
                    FirstName = claims.FirstOrDefault(x => x.Type == "firstName" || x.Type == "given_name")?.Value,
                    LastName = claims.FirstOrDefault(x => x.Type == "lastName" || x.Type == "family_name")?.Value,
                    Issuer = claims.FirstOrDefault(x => x.Type == "iss")?.Value,
                    Audience = claims.FirstOrDefault(x => x.Type == "aud")?.Value,
                    IssuedAt = GetDateTimeFromUnix(claims.FirstOrDefault(x => x.Type == "iat")?.Value),
                    Expiration = GetDateTimeFromUnix(claims.FirstOrDefault(x => x.Type == "exp")?.Value),
                    AuthTime = GetDateTimeFromUnix(claims.FirstOrDefault(x => x.Type == "auth_time")?.Value),
                    Nonce = claims.FirstOrDefault(x => x.Type == "nonce")?.Value
                };

                return userInfo;
            }
            catch (Exception ex)
            {
                // Log error here
                return null;
            }
        }

        private static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private DateTime? GetDateTimeFromUnix(string unixTimestamp)
        {
            if (string.IsNullOrEmpty(unixTimestamp) || !long.TryParse(unixTimestamp, out long seconds))
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }
    }
}