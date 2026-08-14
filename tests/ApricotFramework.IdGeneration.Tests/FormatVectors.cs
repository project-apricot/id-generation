namespace ApricotFramework.IdGeneration.Tests;

/// <summary>
/// Known-answer vectors pinning the exact text form this library writes.
/// </summary>
/// <remarks>
/// Every expected value here was computed independently of this library, in Python:
/// <c>uuid.UUID(canonical).hex</c> for the UUID part, and <c>version</c> / <c>variant</c> for the
/// bit-level checks. That is what makes them worth having — a round-trip test only proves this
/// library agrees with itself, while these prove it agrees with the UUID specification.
/// <para>
/// Consumers persist these identifiers, so a failure here means the stored-identifier contract
/// broke. It is not a value to update without a major version.
/// </para>
/// </remarks>
internal static class FormatVectors
{
    /// <summary>
    /// A version 4 UUID in canonical hyphenated form.
    /// </summary>
    public const string Version4Canonical = "3f2a4c1e-8b7d-40f9-a1c2-6e5d90b3f748";

    /// <summary>
    /// <see cref="Version4Canonical"/> as Python's <c>uuid.UUID(...).hex</c> reports it.
    /// </summary>
    public const string Version4Hex = "3f2a4c1e8b7d40f9a1c26e5d90b3f748";

    /// <summary>
    /// A version 7 UUID in canonical hyphenated form.
    /// </summary>
    public const string Version7Canonical = "0199a4c1-7e3f-7c2b-b9d4-1a2b3c4d5e6f";

    /// <summary>
    /// <see cref="Version7Canonical"/> as Python's <c>uuid.UUID(...).hex</c> reports it.
    /// </summary>
    public const string Version7Hex = "0199a4c17e3f7c2bb9d41a2b3c4d5e6f";

    /// <summary>
    /// The all-zero UUID, whose text form has no non-zero digit to hide a formatting mistake behind.
    /// </summary>
    public const string EmptyCanonical = "00000000-0000-0000-0000-000000000000";

    /// <summary>
    /// <see cref="EmptyCanonical"/> as Python's <c>uuid.UUID(...).hex</c> reports it.
    /// </summary>
    public const string EmptyHex = "00000000000000000000000000000000";

    /// <summary>
    /// The all-ones UUID, which catches a formatter that upper-cases hex.
    /// </summary>
    public const string MaxCanonical = "ffffffff-ffff-ffff-ffff-ffffffffffff";

    /// <summary>
    /// <see cref="MaxCanonical"/> as Python's <c>uuid.UUID(...).hex</c> reports it.
    /// </summary>
    public const string MaxHex = "ffffffffffffffffffffffffffffffff";

    /// <summary>
    /// An identifier of exactly the shape the pre-migration library wrote, with the UUID generated
    /// outside this library. It must still read back, since a store full of these exists.
    /// </summary>
    public const string LegacyUserId = "usr-0ce4800f7f7e49c48bf97f4e35bc9b1e";

    /// <summary>
    /// A pre-migration identifier whose prefix itself contains the separator, which is the case
    /// that would break a parser searching for the first separator instead of the fixed-length
    /// UUID part.
    /// </summary>
    public const string LegacyOrderLineId = "ord-line-6489433150c24f2b9802740aed309403";

    /// <summary>
    /// A pre-migration identifier with the shortest prefix that can exist.
    /// </summary>
    public const string LegacySingleCharPrefixId = "a-3f1b5df9d2e349af83d0c65033d4a0c1";
}
