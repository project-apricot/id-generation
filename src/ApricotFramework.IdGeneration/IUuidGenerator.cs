namespace ApricotFramework.IdGeneration;

/// <summary>
/// Generates UUIDs as <see cref="Guid"/> values, for storing in a native UUID column rather than
/// as text.
/// </summary>
/// <remarks>
/// A native column costs 16 bytes against 33 or more for the string form, which matters most in
/// indexes, where a narrower key means more rows per page. Use this for keys the database owns, and
/// <see cref="IIdGenerator"/> for identifiers people and other systems see.
/// </remarks>
public interface IUuidGenerator
{
    /// <summary>
    /// Generates a new UUID.
    /// </summary>
    /// <returns>The UUID.</returns>
    Guid Generate();
}
