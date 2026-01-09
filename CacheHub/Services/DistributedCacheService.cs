using CacheHub.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace CacheHub.Services
{
    public class DistributedCacheService(IDistributedCache distributedCache) : IDistributedCacheService
    {
        private readonly IDistributedCache _distributedCache = distributedCache;
        private static readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

        /// <summary>
        /// Asynchronously retrieves a cached value by key and deserializes it to the specified reference type.
        /// </summary>
        /// <remarks>If the cached value is not found or cannot be deserialized to the specified type, the
        /// method returns null. The operation is performed asynchronously and may be canceled using the provided
        /// cancellation token.</remarks>
        /// <typeparam name="T">The reference type to which the cached value will be deserialized.</typeparam>
        /// <param name="key">The key used to locate the cached value. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A deserialized object of type T if the key exists in the cache and the value can be deserialized; otherwise,
        /// null.</returns>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class
        {
            string? cacheValue = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (cacheValue is null)
            {
                return null;
            }

            T? value = JsonConvert.DeserializeObject<T>(cacheValue);

            return value;
        }

        /// <summary>
        /// Retrieves a cached value associated with the specified key, or creates, caches, and returns a new value
        /// using the provided asynchronous factory if the key is not found.
        /// </summary>
        /// <remarks>If the value is not present in the cache, the method invokes the factory delegate to
        /// create it, stores the result in the cache, and then returns it. Subsequent calls with the same key will
        /// return the cached value until it is evicted or expires. The factory delegate is only invoked if the value is
        /// not already cached.</remarks>
        /// <typeparam name="T">The type of the value to retrieve or create. Must be a reference type.</typeparam>
        /// <param name="key">The cache key used to identify the value. Cannot be null or empty.</param>
        /// <param name="factory">An asynchronous delegate that produces the value to cache if it does not already exist. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the cached or newly created
        /// value of type T, or null if the factory returns null.</returns>
        public async Task<T?> GetAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken) where T : class
        {
            T? cachedValue = await GetAsync<T>(key, cancellationToken);

            if (cachedValue is not null)
            {
                return cachedValue;
            }

            cachedValue = await factory();

            await SetAsync<T>(key, cachedValue, cancellationToken);

            return cachedValue;
        }

        /// <summary>
        /// Asynchronously stores the specified value in the distributed cache under the given key.
        /// </summary>
        /// <remarks>The value is serialized to JSON before being stored. If the operation is cancelled
        /// via the provided token, the cache may not be updated.</remarks>
        /// <typeparam name="T">The type of the value to store in the cache. Must be a reference type.</typeparam>
        /// <param name="key">The cache key under which the value will be stored. Cannot be null.</param>
        /// <param name="value">The value to store in the cache. Must not be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous cache set operation.</returns>
        public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken) where T : class
        {
            string cacheValue = JsonConvert.SerializeObject(value);

            await _distributedCache.SetStringAsync(key, cacheValue, cancellationToken);

            _cacheKeys.TryAdd(key, false);
        }

        /// <summary>
        /// Asynchronously removes the cache entry associated with the specified key from both the distributed cache and
        /// the local cache key store.
        /// </summary>
        /// <param name="key">The key identifying the cache entry to remove. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the remove operation.</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);

            bool isRemoved = _cacheKeys.TryRemove(key, out _);

            return isRemoved;
        }

        /// <summary>
        /// Asynchronously removes all cache entries whose keys begin with the specified prefix.
        /// </summary>
        /// <remarks>This method removes all matching entries in parallel. If no keys match the specified
        /// prefix, no entries are removed.</remarks>
        /// <param name="prefix">The prefix to match against cache entry keys. All keys that start with this value will be removed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the remove operation.</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken)
        {
            IEnumerable<Task> enumarable = _cacheKeys.Keys
                .Where(key => key.StartsWith(prefix))
                .Select(key => RemoveAsync(key, cancellationToken));

            await Task.WhenAll(enumarable);
        }


    }
}
