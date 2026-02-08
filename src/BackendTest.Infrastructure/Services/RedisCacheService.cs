using BackendTest.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace BackendTest.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly IConnectionMultiplexer _redis = redis;

    /// <summary>
    /// Obtiene un valor del caché deserializado al tipo especificado
    /// </summary>
    /// <typeparam name="T">Tipo del objeto a obtener</typeparam>
    /// <param name="key">Clave del caché</param>
    /// <returns>El objeto deserializado o null si no existe</returns>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var cached = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cached))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(cached);
    }

    /// <summary>
    /// Guarda un valor en el caché con expiración opcional
    /// </summary>
    /// <typeparam name="T">Tipo del objeto a guardar</typeparam>
    /// <param name="key">Clave del caché</param>
    /// <param name="value">Valor a guardar</param>
    /// <param name="expiration">Tiempo de expiración opcional</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        var serialized = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serialized, options);
    }

    /// <summary>
    /// Elimina un valor del caché por su clave
    /// </summary>
    /// <param name="key">Clave del valor a eliminar</param>
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    /// <summary>
    /// Elimina todas las entradas del caché que coincidan con el patrón especificado usando SCAN
    /// </summary>
    /// <param name="pattern">Patrón de búsqueda (ejemplo: "movies:*")</param>
    /// <remarks>
    /// Usa el comando SCAN de Redis en lugar de KEYS para evitar bloquear el servidor en producción.
    /// Compatible con Redis Cluster.
    /// </remarks>
    public async Task RemoveByPatternAsync(string pattern)
    {
        var database = _redis.GetDatabase();
        var endpoints = _redis.GetEndPoints();

        if (endpoints.Length == 0)
        {
            return;
        }

        var server = _redis.GetServer(endpoints[0]);
        var keys = server.KeysAsync(pattern: pattern, pageSize: 100);

        await foreach (var key in keys)
        {
            await database.KeyDeleteAsync(key);
        }
    }
}
