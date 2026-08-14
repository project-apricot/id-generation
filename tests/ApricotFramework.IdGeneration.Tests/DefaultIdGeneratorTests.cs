using ApricotFramework.IdGeneration.Impl;

namespace ApricotFramework.IdGeneration.Tests;

public class DefaultIdGeneratorTests
{
    /// <summary>
    /// The index of the version digit within the 32-character UUID part, per RFC 9562.
    /// </summary>
    private const int VersionDigitIndex = 12;

    /// <summary>
    /// The index of the variant digit within the 32-character UUID part, per RFC 9562.
    /// </summary>
    private const int VariantDigitIndex = 16;

    /// <summary>
    /// Builds a generator.
    /// </summary>
    /// <returns>The generator.</returns>
    private static DefaultIdGenerator CreateGenerator()
    {
        return new DefaultIdGenerator();
    }

    /// <summary>
    /// Takes the UUID part of an identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The 32 hex characters after the last separator.</returns>
    private static string UuidPartOf(string id)
    {
        return id[^PrefixedId.UuidLength..];
    }

    // --- the written shape, which consumers persist ---

    [Fact]
    public void Generate_Always_WritesThePrefixThenTheSeparatorThenTheUuid()
    {
        var id = CreateGenerator().Generate("usr");

        Assert.StartsWith("usr-", id, StringComparison.Ordinal);
        Assert.Equal("usr-".Length + PrefixedId.UuidLength, id.Length);
    }

    [Fact]
    public void Generate_Always_WritesLowercaseHexWithNoSeparatorsInTheUuidPart()
    {
        var uuidPart = UuidPartOf(CreateGenerator().Generate("usr"));

        Assert.True(uuidPart.All(char.IsAsciiHexDigitLower), uuidPart);
    }

    [Fact]
    public void Generate_Always_ProducesAnIdentifierThatReadsBack()
    {
        // Write and read bounds have to be symmetric: anything this can produce, TryParse must
        // accept, or a value could be stored that is never recoverable.
        var id = CreateGenerator().Generate("usr");

        Assert.True(PrefixedId.TryParse(id, out var parsed));
        Assert.Equal("usr", parsed.Prefix);
        Assert.Equal(id, parsed.ToString());
    }

    [Fact]
    public void Generate_ManyTimes_ProducesDistinctIdentifiers()
    {
        var generator = CreateGenerator();

        var ids = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < 10_000; i++)
        {
            Assert.True(ids.Add(generator.Generate("usr")));
        }
    }

    // --- version 4, cross-checked against the digits Python reports ---

    [Fact]
    public void Generate_Always_ProducesAVersion4Uuid()
    {
        // Python reports version 4 for a UUID whose 13th hex digit is '4'; that digit is the
        // version field. Randomness is the point here — a version 7 value would carry the
        // millisecond the identifier was created in, and these identifiers are handed out.
        var uuidPart = UuidPartOf(CreateGenerator().Generate("usr"));

        Assert.Equal('4', uuidPart[VersionDigitIndex]);
    }

    [Fact]
    public void Generate_Always_SetsTheRfc9562VariantBits()
    {
        var uuidPart = UuidPartOf(CreateGenerator().Generate("usr"));

        Assert.True("89ab".Contains(uuidPart[VariantDigitIndex], StringComparison.Ordinal), uuidPart);
    }

    [Fact]
    public void Generate_Always_LeavesTheTimestampFieldRandom()
    {
        // The first 48 bits of a version 7 UUID are a millisecond timestamp. If these were version
        // 7 by accident, successive values would share that prefix; random ones will not.
        var generator = CreateGenerator();

        var leadingDigits = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < 100; i++)
        {
            leadingDigits.Add(UuidPartOf(generator.Generate("usr"))[..12]);
        }

        Assert.Equal(100, leadingDigits.Count);
    }

    // --- prefixes ---

    [Theory]
    [InlineData("usr")]
    [InlineData("a")]
    [InlineData("ord-line")]
    [InlineData("пользователь")]
    [InlineData("USR")]
    public void Generate_UnusualPrefix_StillReadsBack(string prefix)
    {
        var id = CreateGenerator().Generate(prefix);

        Assert.True(PrefixedId.TryParse(id, out var parsed));
        Assert.Equal(prefix, parsed.Prefix);
    }

    [Fact]
    public void Generate_VeryLongPrefix_StillReadsBack()
    {
        var prefix = new string('p', 5_000);

        var id = CreateGenerator().Generate(prefix);

        Assert.True(PrefixedId.TryParse(id, out var parsed));
        Assert.Equal(prefix, parsed.Prefix);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Generate_MissingPrefix_Throws(string? prefix)
    {
        // The pre-migration library produced "-3f2a…" here, an identifier with no prefix that also
        // fails to parse. Rejecting it turns a silent bad write into an immediate error.
        Assert.ThrowsAny<ArgumentException>(() => CreateGenerator().Generate(prefix!));
    }

    [Fact]
    public void Generate_NullPrefix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CreateGenerator().Generate(null!));
    }
}
