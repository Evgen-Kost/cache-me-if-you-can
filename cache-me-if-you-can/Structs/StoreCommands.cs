namespace cache_me_if_you_can.Structs;

/// <summary>
/// Defines the set of supported command types for the cache store protocol.
/// </summary>
/// <remarks>
/// All commands are case-sensitive and must be in uppercase format.
/// These string constants are used for parsing and validating client requests.
/// </remarks>
public static class StoreCommands
{
    /// <summary>
    /// Retrieves the value associated with a specified key.
    /// </summary>
    /// <value>"GET"</value>
    /// <remarks>
    /// Usage: GET key
    /// Returns the stored value or (null) if the key does not exist.
    /// </remarks>
    public const string Get = "GET";
    
    /// <summary>
    /// Stores a key-value pair in the cache.
    /// </summary>
    /// <value>"SET"</value>
    /// <remarks>
    /// Usage: SET key value
    /// If the key already exists, its value is updated.
    /// </remarks>
    public const string Set = "SET";
    
    /// <summary>
    /// Deletes a key and its associated value from the cache.
    /// </summary>
    /// <value>"DEL"</value>
    /// <remarks>
    /// Usage: DEL key
    /// No error is returned if the key does not exist.
    /// </remarks>
    public const string Del = "DEL";
}
