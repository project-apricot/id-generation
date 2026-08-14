namespace ApricotFramework.IdGeneration;

/// <summary>
/// Generates prefixed string identifiers, such as <c>usr-3f2a…</c>.
/// </summary>
/// <remarks>
/// The prefix says what the identifier denotes, which makes an identifier self-describing wherever
/// it turns up — a log line, a URL, a support ticket. See <see cref="PrefixedId"/> for the exact
/// format and for reading one back.
/// </remarks>
public interface IIdGenerator
{
    /// <summary>
    /// Generates a new identifier carrying the given prefix.
    /// </summary>
    /// <param name="prefix">
    /// The prefix identifying what the identifier denotes, such as <c>usr</c>. It may contain the
    /// separator; that stays unambiguous, because the part after it has a fixed length.
    /// </param>
    /// <returns>The identifier, as <c>{prefix}-{32 lowercase hex characters}</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="prefix"/> is empty or whitespace. An identifier without a prefix would
    /// defeat the point, and it would not read back.
    /// </exception>
    string Generate(string prefix);
}
