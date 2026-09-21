using Microsoft.Extensions.Caching.Memory;

namespace WebPageSublimation.Security;

/// <summary>
/// Evita consultar la base de datos por cada recurso de una página autenticada.
/// Las operaciones que cambian el estado de una cuenta invalidan la entrada de inmediato.
/// </summary>
public sealed class ActiveUserSessionCache(IMemoryCache cache)
{
    private static readonly TimeSpan ActiveLifetime = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan InactiveLifetime = TimeSpan.FromSeconds(5);

    public async Task<bool> IsActiveAsync(string userId, Func<Task<bool>> load)
    {
        var key = GetKey(userId);
        if (cache.TryGetValue(key, out bool active)) return active;

        active = await load();
        cache.Set(key, active, active ? ActiveLifetime : InactiveLifetime);
        return active;
    }

    public void Invalidate(string userId) => cache.Remove(GetKey(userId));

    private static string GetKey(string userId) => $"active-user:{userId}";
}
