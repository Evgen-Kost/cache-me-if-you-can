namespace cache_me_if_you_can.Services;

/// <summary>
/// SimpleStore
/// </summary>
public class SimpleDimpleStore : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, byte[]> _cache = new();
    private long _setCount;
    private long _getCount;
    private long _deleteCount;
    private bool _disposed;
    
    public void Set(string key, byte[] value)
    {
        try
        {
            _lock.EnterWriteLock();
            _cache[key] = value;
            
            Interlocked.Increment(ref _setCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public byte[]? Get(string key)
    {
        try
        {
            _lock.EnterReadLock();
            _cache.TryGetValue(key, out var value);
            Interlocked.Increment(ref _getCount);
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    public void Delete(string key)
    {
        try
        {
            _lock.EnterWriteLock();
            _cache.Remove(key);
            Interlocked.Increment(ref _deleteCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
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
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); 
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _lock.Dispose();
            _cache.Clear();
        }
            
        _disposed = true;
    }
}