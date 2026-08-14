using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ApricotFramework.IdGeneration;

/// <summary>
/// A prefixed identifier: a prefix, a separator, and a UUID written as 32 lowercase hex characters
/// — <c>usr-3f2a4c1e8b7d40f9a1c26e5d90b3f748</c>.
/// </summary>
/// <remarks>
/// This type owns the format, so the generator that writes an identifier and the parser that reads
/// one back cannot drift apart. Consumers persist these strings, so the format is a compatibility
/// contract and will not change without a major version.
/// <para>
/// The UUID part is always exactly <see cref="UuidLength"/> characters and never contains the
/// separator, which is what makes a prefix containing the separator safe: <c>ord-line-3f2a…</c>
/// reads back as the prefix <c>ord-line</c>. Nothing a caller can put in a prefix produces an
/// identifier that fails to parse.
/// </para>
/// </remarks>
public sealed record PrefixedId
{
    /// <summary>
    /// The number of characters the UUID part occupies.
    /// </summary>
    public const int UuidLength = 32;

    /// <summary>
    /// The character between the prefix and the UUID.
    /// </summary>
    private const char Separator = '-';

    /// <summary>
    /// The <see cref="Guid"/> format specifier for 32 hex characters with no separators or braces.
    /// </summary>
    private const string UuidFormat = "N";

    /// <summary>
    /// Creates a new prefixed identifier.
    /// </summary>
    /// <param name="prefix">The prefix identifying what the identifier denotes, such as <c>usr</c>.</param>
    /// <param name="uuid">The UUID part.</param>
    /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="prefix"/> is empty or whitespace.</exception>
    public PrefixedId(string prefix, Guid uuid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        this.Prefix = prefix;
        this.Uuid = uuid;
    }

    /// <summary>
    /// The prefix identifying what the identifier denotes.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// The UUID part.
    /// </summary>
    public Guid Uuid { get; }

    /// <summary>
    /// Writes the identifier in the form consumers persist.
    /// </summary>
    /// <returns>The identifier, as <c>{prefix}-{32 lowercase hex characters}</c>.</returns>
    public override string ToString()
    {
        return $"{this.Prefix}{Separator}{this.Uuid.ToString(UuidFormat, CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Reads an identifier back into its prefix and UUID.
    /// </summary>
    /// <param name="value">The identifier to read, as written by <see cref="ToString"/>.</param>
    /// <param name="result">The parsed identifier, or <see langword="null"/> when parsing fails.</param>
    /// <returns>Whether <paramref name="value"/> was an identifier in this format.</returns>
    /// <remarks>
    /// Malformed input is reported as a negative result rather than raised as an exception, because
    /// the input is typically a stored or externally supplied value rather than a programming
    /// mistake. Hex is accepted in either case, so an identifier upper-cased in transit still
    /// reads; <see cref="ToString"/> always writes lowercase.
    /// </remarks>
    public static bool TryParse(string? value, [NotNullWhen(true)] out PrefixedId? result)
    {
        result = null;

        if (value is null)
        {
            return false;
        }

        // The UUID part has a fixed length, so the separator's position is known without searching.
        // That is what keeps a prefix containing the separator unambiguous, and it means an
        // arbitrarily long input costs no more than a short one.
        var separatorIndex = value.Length - UuidLength - 1;

        if (separatorIndex < 1 || value[separatorIndex] != Separator)
        {
            return false;
        }

        if (!Guid.TryParseExact(value.AsSpan(separatorIndex + 1), UuidFormat, out var uuid))
        {
            return false;
        }

        var prefix = value[..separatorIndex];

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return false;
        }

        result = new PrefixedId(prefix, uuid);

        return true;
    }
}
