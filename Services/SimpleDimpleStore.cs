namespace cache_me_if_you_can.Services;

public class SimpleDimpleStore
{
    private readonly Dictionary<string, byte[]> _cache = new();

    void Set(string key, byte[] value)
    {
        _cache[key] = value;
    }

    byte[]? Get(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }

    void Delete(string key)
    {
        _cache.Remove(key);
    }
}