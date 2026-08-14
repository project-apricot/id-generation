namespace ApricotFramework.IdGeneration.Impl;

/// <summary>
/// Generates prefixed identifiers from random UUIDs (version 4).
/// </summary>
/// <remarks>
/// Version 4 rather than version 7, deliberately: these identifiers travel in URLs, API payloads
/// and support conversations, and a version 7 UUID would carry the millisecond it was created in.
/// Random identifiers reveal nothing. For keys the database owns — where being time-ordered pays
/// for itself in index locality — use <see cref="DefaultUuidGenerator"/> instead.
/// <para>
/// Stateless and thread-safe, so one instance can serve a whole application.
/// </para>
/// </remarks>
public sealed class DefaultIdGenerator : IIdGenerator
{
    /// <inheritdoc />
    public string Generate(string prefix)
    {
        return new PrefixedId(prefix, Guid.NewGuid()).ToString();
    }
}
