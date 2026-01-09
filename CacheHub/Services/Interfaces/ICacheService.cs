namespace CacheHub.Services.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Asynchronously retrieves the value associated with the specified key, deserialized to the given reference
        /// type.
        /// </summary>
        /// <typeparam name="T">The reference type to which the stored value will be deserialized.</typeparam>
        /// <param name="key">The key identifying the value to retrieve. Cannot be null or empty.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value of type T
        /// if the key exists; otherwise, null.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
            where T : class;

        /// <summary>
        /// Asynchronously retrieves a cached value associated with the specified key, or creates and caches the value
        /// using the provided factory function if it does not exist.
        /// </summary>
        /// <remarks>If the value for the specified key is not present in the cache, the factory function
        /// is invoked to create and cache the value. Subsequent calls with the same key will return the cached value
        /// until it expires or is removed. This method is thread-safe and ensures that the factory function is only
        /// invoked once per key, even if called concurrently.</remarks>
        /// <typeparam name="T">The type of the value to retrieve or create. Must be a reference type.</typeparam>
        /// <param name="key">The key that identifies the cached value. Cannot be null or empty.</param>
        /// <param name="factory">A function that asynchronously produces the value to cache if it is not already present. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the cached value of type T if
        /// found, or the newly created value if not. Returns null if the factory function returns null.</returns>
        Task<T?> GetAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken)
            where T : class;

        /// <summary>
        /// Asynchronously sets the value associated with the specified key in the underlying store.
        /// </summary>
        /// <typeparam name="T">The type of the value to store. Must be a reference type.</typeparam>
        /// <param name="key">The key with which the specified value will be associated. Cannot be null or empty.</param>
        /// <param name="value">The value to store. Must not be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        Task SetAsync<T>(string key, T value, CancellationToken cancellationToken)
            where T : class;

        /// <summary>
        /// Asynchronously removes the value associated with the specified key, if it exists.
        /// </summary>
        /// <param name="key">The key of the item to remove. Cannot be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the remove operation.</param>
        /// <returns>A task that represents the asynchronous remove operation. The task result is <see langword="true"/> if the
        /// item was successfully removed; otherwise, <see langword="false"/>.</returns>
        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously removes all items whose keys begin with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to match against item keys. All items with keys starting with this value will be removed. Cannot
        /// be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the remove operation.</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken);
    }
}
