using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;



namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    // ------------------ Contracts ------------------

    public interface IRefreshTokenStore
    {
        Task SaveAsync(Guid userId, string refreshToken, DateTimeOffset expiresAt, bool rotate);
        Task<Guid?> ResolveUserIdByRefreshTokenAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
    }

    // If you already have a users repo, make it implement this.
    public interface IUserLookup
    {
        Task<AppUser?> FindByIdAsync(Guid userId);
    }

    // ------------------ Service ------------------

    public sealed class TokenService
    {
        private readonly byte[] _signingKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IRefreshTokenStore _refreshStore;
        private readonly IUserLookup _userLookup;

        public TokenService(IConfiguration config, IRefreshTokenStore refreshStore, IUserLookup userLookup)
        {
            // Support both appsettings and environment variables
            var signingKey = config["Jwt:SigningKey"]
                              ?? Environment.GetEnvironmentVariable("JWT__SIGNINGKEY");

            _issuer = config["Jwt:Issuer"]
                     ?? Environment.GetEnvironmentVariable("JWT__ISSUER")
                     ?? "https://api.example.com";

            _audience = config["Jwt:Audience"]
                     ?? Environment.GetEnvironmentVariable("JWT__AUDIENCE")
                     ?? "your-api";

            if (string.IsNullOrWhiteSpace(signingKey))
                throw new ArgumentNullException("Jwt:SigningKey",
                    "JWT signing key is missing. Set Jwt:SigningKey in appsettings or JWT__SIGNINGKEY env var.");

            if (signingKey.Length < 48) // encourage strong keys
                throw new ArgumentException("Jwt:SigningKey must be long and random (>=48 chars).");

            _signingKey = Encoding.UTF8.GetBytes(signingKey);
            _refreshStore = refreshStore;
            _userLookup = userLookup;
        }

        public (string token, int expiresInSeconds) IssueAccessToken(AppUser user)
        {
            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(10); // short-lived access token

            // Use fully-qualified Claim to avoid any namespace collisions
            var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new System.Security.Claims.Claim("uid", user.Id.ToString())
        // add roles/permissions as needed
    };

            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(_signingKey),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expires.UtcDateTime,
                signingCredentials: creds);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwt);
            return (token, (int)(expires - now).TotalSeconds);
        }


        public async Task<string> IssueRefreshTokenAsync(AppUser user, bool rotate = true)
        {
            // Strong random token
            var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            await _refreshStore.SaveAsync(user.Id, refresh, DateTimeOffset.UtcNow.AddDays(45), rotate);
            return refresh;
        }

        public async Task<AppUser?> ResolveUserByRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var userId = await _refreshStore.ResolveUserIdByRefreshTokenAsync(refreshToken);
            if (userId is null)
                return null;

            return await _userLookup.FindByIdAsync(userId.Value);
        }

        public Task InvalidateRefreshTokenAsync(string refreshToken)
            => _refreshStore.RevokeAsync(refreshToken);
    }

    // ------------------ In-memory store ------------------

    public sealed class RefreshTokenStore : IRefreshTokenStore
    {
        private sealed class Entry
        {
            public Guid UserId { get; init; }
            public string Token { get; init; } = default!;
            public DateTimeOffset ExpiresAt { get; init; }
            public bool Rotate { get; init; }
            public bool Revoked { get; set; }
        }

        // Keyed by token for O(1) lookups/revocations.
        private static readonly ConcurrentDictionary<string, Entry> _byToken = new();

        public Task SaveAsync(Guid userId, string refreshToken, DateTimeOffset expiresAt, bool rotate)
        {
            var e = new Entry
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = expiresAt,
                Rotate = rotate,
                Revoked = false
            };
            _byToken[refreshToken] = e;

            // Opportunistic purge of expired tokens (non-blocking)
            var now = DateTimeOffset.UtcNow;
            foreach (var kv in _byToken.ToArray())
                if (kv.Value.ExpiresAt <= now || kv.Value.Revoked)
                    _byToken.TryRemove(kv.Key, out _);

            return Task.CompletedTask;
        }

        public Task<Guid?> ResolveUserIdByRefreshTokenAsync(string refreshToken)
        {
            if (_byToken.TryGetValue(refreshToken, out var e))
            {
                var now = DateTimeOffset.UtcNow;
                if (!e.Revoked && e.ExpiresAt > now)
                    return Task.FromResult<Guid?>(e.UserId);
            }
            return Task.FromResult<Guid?>(null);
        }

        public Task RevokeAsync(string refreshToken)
        {
            if (_byToken.TryGetValue(refreshToken, out var e))
                e.Revoked = true;

            return Task.CompletedTask;
        }
    }

    // ------------------ Model ------------------

    public sealed class AppUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string? GoogleSub { get; set; }
        public string? PictureUrl { get; set; }
        public bool EmailVerified { get; set; }
    }
}
