namespace ApricotFramework.IdGeneration.Impl;

/// <summary>
/// Generates time-ordered UUIDs (version 7).
/// </summary>
/// <remarks>
/// A version 7 UUID begins with a 48-bit millisecond timestamp, so values generated over time sort
/// close to each other. Inserts then land at the end of an index instead of scattering across it,
/// which is why this is the current recommendation for a database-generated key.
/// <para>
/// The trade is that the creation time is recoverable from the value, so do not use one where that
/// would leak something — an identifier handed to a client, for instance. Use
/// <see cref="DefaultIdGenerator"/> there.
/// </para>
/// <para>
/// Stateless and thread-safe, so one instance can serve a whole application.
/// </para>
/// </remarks>
public sealed class DefaultUuidGenerator : IUuidGenerator
{
    /// <inheritdoc />
    public Guid Generate()
    {
        return Guid.CreateVersion7();
    }
}
