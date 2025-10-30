namespace cache_me_if_you_can.Services;

/// <summary>
/// Represents a thread-safe in-memory key-value store with basic operations and statistics tracking.
/// </summary>
/// <remarks>
/// This class uses <see cref="ReaderWriterLockSlim"/> to provide thread-safe access to the underlying dictionary.
/// All operations are atomic and statistics are updated using interlocked operations.
/// </remarks>
public class SimpleDimpleStore : IDisposable
{
    /// <summary>
    /// Reader-writer lock for thread-safe access to the cache.
    /// </summary>
    private readonly ReaderWriterLockSlim _lock = new();
    
    /// <summary>
    /// Internal dictionary storing key-value pairs.
    /// </summary>
    private readonly Dictionary<string, byte[]> _cache = new();
    
    /// <summary>
    /// Counter tracking the total number of Set operations.
    /// </summary>
    private long _setCount;
    
    /// <summary>
    /// Counter tracking the total number of Get operations.
    /// </summary>
    private long _getCount;
    
    /// <summary>
    /// Counter tracking the total number of Delete operations.
    /// </summary>
    private long _deleteCount;
    
    /// <summary>
    /// Flag indicating whether the object has been disposed.
    /// </summary>
    private bool _disposed;
    
    /// <summary>
    /// Stores a key-value pair in the cache. If the key already exists, its value is updated.
    /// </summary>
    /// <param name="key">The unique identifier for the cached item.</param>
    /// <param name="value">The byte array value to store.</param>
    /// <remarks>
    /// This operation acquires a write lock and is thread-safe. The operation counter is incremented atomically.
    /// </remarks>
    public void Set(string key, byte[] value)
    {
        try
        {
            _lock.EnterWriteLock();
            _cache[key] = value;
            
            // Increment the Set operation counter atomically
            _setCount++;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>
    /// The byte array value associated with the key, or <see langword="null"/> if the key does not exist.
    /// </returns>
    /// <remarks>
    /// This operation acquires a read lock and is thread-safe. The operation counter is incremented atomically.
    /// </remarks>
    public byte[]? Get(string key)
    {
        try
        {
            _lock.EnterReadLock();
            _cache.TryGetValue(key, out var value);
            
            // Increment the Get operation counter atomically
            _getCount++;
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    /// <summary>
    /// Removes the value associated with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the item to delete.</param>
    /// <remarks>
    /// This operation acquires a write lock and is thread-safe. The operation counter is incremented atomically.
    /// If the key does not exist, no exception is thrown.
    /// </remarks>
    public void Delete(string key)
    {
        try
        {
            _lock.EnterWriteLock();
            _cache.Remove(key);
            
            // Increment the Delete operation counter atomically
            _deleteCount++;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Retrieves statistics about cache operations.
    /// </summary>
    /// <returns>
    /// A tuple containing the total number of Set, Get, and Delete operations performed since the store was created.
    /// </returns>
    /// <remarks>
    /// This operation acquires a read lock to ensure consistent snapshot of all counters.
    /// </remarks>
    public (long setCount, long getCount, long deleteCount) GetStatistics()
    {
        try
        {
            _lock.EnterReadLock();
            return (_setCount, _getCount, _deleteCount);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    /// <summary>
    /// Releases all resources used by the <see cref="SimpleDimpleStore"/>.
    /// </summary>
    /// <remarks>
    /// This method disposes the reader-writer lock and clears the internal cache dictionary.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="SimpleDimpleStore"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        // Check if already disposed to prevent multiple disposal
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            _lock.Dispose();
            _cache.Clear();
        }
            
        _disposed = true;
    }
}
