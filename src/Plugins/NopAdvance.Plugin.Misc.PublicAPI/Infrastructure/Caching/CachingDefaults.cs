using Nop.Core.Caching;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Caching;

public static class CachingDefaults
{
    public const string CACHE_KEY_PREFIX = "NopAdvance.publicapi.";

    /// <summary>
    /// {0} : api key
    /// </summary>
    public static CacheKey APIApplicationByAPIKeyCacheKey
        => new CacheKey($"{CACHE_KEY_PREFIX}application.byapikey-{{0}}");

    /// <summary>
    /// {0} : application id
    /// {1} : customer id
    /// {2} : token id
    /// </summary>
    public static CacheKey APIRefreshTokenCacheKey
        => new CacheKey($"{CACHE_KEY_PREFIX}refreshtoken-{{0}}-{{1}}-{{2}}");

    /// <summary>
    /// Prefix/pattern for removing refresh token caches (use with RemoveByPrefix / RemoveByPattern)
    /// {0} : application id
    /// </summary>
    public static string APIRefreshTokenPrefix
        => $"{CACHE_KEY_PREFIX}refreshtoken-";

    /// <summary>
    /// {0} : picture id
    /// {1} : connection type (http/https)
    /// </summary>
    public static CacheKey PICTURE_URL_MODEL_KEY
        => new CacheKey("Nop.plugins.widgets.nivoslider.pictureurl-{0}-{1}");

    /// <summary>
    /// Prefix/pattern for removing picture url caches
    /// </summary>
    public const string PICTURE_URL_PATTERN_KEY = "Nop.plugins.widgets.nivoslider";
}
