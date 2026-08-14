namespace ApricotFramework.IdGeneration.Tests;

public class PrefixedIdTests
{
    // --- the written form, against vectors computed outside this library ---

    [Theory]
    [InlineData(FormatVectors.Version4Canonical, FormatVectors.Version4Hex)]
    [InlineData(FormatVectors.Version7Canonical, FormatVectors.Version7Hex)]
    [InlineData(FormatVectors.EmptyCanonical, FormatVectors.EmptyHex)]
    [InlineData(FormatVectors.MaxCanonical, FormatVectors.MaxHex)]
    public void ToString_KnownUuid_MatchesTheIndependentlyComputedForm(string canonical, string hex)
    {
        var id = new PrefixedId("usr", Guid.Parse(canonical));

        Assert.Equal($"usr-{hex}", id.ToString());
    }

    [Fact]
    public void ToString_UppercaseInput_WritesLowercase()
    {
        var id = new PrefixedId("usr", Guid.Parse(FormatVectors.MaxCanonical.ToUpperInvariant()));

        Assert.Equal($"usr-{FormatVectors.MaxHex}", id.ToString());
    }

    [Fact]
    public void ToString_UuidPart_IsAlwaysTheDeclaredLength()
    {
        var id = new PrefixedId("usr", Guid.Parse(FormatVectors.EmptyCanonical));

        Assert.Equal(PrefixedId.UuidLength, id.ToString().Length - "usr-".Length);
    }

    // --- construction ---

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_BlankPrefix_ThrowsArgumentException(string prefix)
    {
        // An identifier with no prefix would read back as a parse failure, so it must not be
        // constructible in the first place.
        Assert.Throws<ArgumentException>(() => new PrefixedId(prefix, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_NullPrefix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PrefixedId(null!, Guid.NewGuid()));
    }

    // --- reading identifiers back ---

    [Theory]
    [InlineData(FormatVectors.LegacyUserId, "usr")]
    [InlineData(FormatVectors.LegacyOrderLineId, "ord-line")]
    [InlineData(FormatVectors.LegacySingleCharPrefixId, "a")]
    public void TryParse_IdentifierWrittenByThePreMigrationLibrary_RecoversThePrefix(string value, string expectedPrefix)
    {
        Assert.True(PrefixedId.TryParse(value, out var parsed));
        Assert.Equal(expectedPrefix, parsed.Prefix);
    }

    [Fact]
    public void TryParse_IdentifierWrittenByThePreMigrationLibrary_RecoversTheUuid()
    {
        Assert.True(PrefixedId.TryParse(FormatVectors.LegacyUserId, out var parsed));

        Assert.Equal(Guid.Parse("0ce4800f-7f7e-49c4-8bf9-7f4e35bc9b1e"), parsed.Uuid);
    }

    [Theory]
    [InlineData("usr")]
    [InlineData("ord-line")]
    [InlineData("a")]
    [InlineData("пользователь")]
    [InlineData("USR")]
    [InlineData("with space")]
    [InlineData("dots.and_underscores")]
    [InlineData(FormatVectors.Version4Hex)]
    [InlineData(FormatVectors.LegacyUserId)]
    public void TryParse_WhateverThePrefixContains_RoundTrips(string prefix)
    {
        var uuid = Guid.Parse(FormatVectors.Version4Canonical);
        var written = new PrefixedId(prefix, uuid).ToString();

        Assert.True(PrefixedId.TryParse(written, out var parsed));
        Assert.Equal(prefix, parsed.Prefix);
        Assert.Equal(uuid, parsed.Uuid);
        Assert.Equal(written, parsed.ToString());
    }

    [Fact]
    public void TryParse_UppercaseUuidPart_IsAccepted()
    {
        // The read side widens where the write side cannot: an identifier upper-cased somewhere in
        // transit still has to resolve to the same row.
        Assert.True(PrefixedId.TryParse($"usr-{FormatVectors.Version4Hex.ToUpperInvariant()}", out var parsed));

        Assert.Equal(Guid.Parse(FormatVectors.Version4Canonical), parsed.Uuid);
    }

    [Fact]
    public void TryParse_UppercaseUuidPart_NormalisesToLowercaseWhenWrittenBack()
    {
        Assert.True(PrefixedId.TryParse($"usr-{FormatVectors.Version4Hex.ToUpperInvariant()}", out var parsed));

        Assert.Equal($"usr-{FormatVectors.Version4Hex}", parsed.ToString());
    }

    // --- malformed input is a negative result, never an exception ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("usr")]
    [InlineData("usr-")]
    [InlineData("-3f2a4c1e8b7d40f9a1c26e5d90b3f748")]
    [InlineData(" -3f2a4c1e8b7d40f9a1c26e5d90b3f748")]
    [InlineData("usr3f2a4c1e8b7d40f9a1c26e5d90b3f748")]
    [InlineData("usr-3f2a4c1e8b7d40f9a1c26e5d90b3f74")]
    [InlineData("usr-3f2a4c1e8b7d40f9a1c26e5d90b3f7488")]
    [InlineData("usr-3f2a4c1e-8b7d-40f9-a1c2-6e5d90b3f748")]
    [InlineData("usr-3f2a4c1e8b7d40f9a1c26e5d90b3f74g")]
    [InlineData("usr-{3f2a4c1e8b7d40f9a1c26e5d90b3f748}")]
    [InlineData("usr_3f2a4c1e8b7d40f9a1c26e5d90b3f748")]
    [InlineData("3f2a4c1e8b7d40f9a1c26e5d90b3f748")]
    public void TryParse_MalformedValue_ReturnsFalse(string? value)
    {
        Assert.False(PrefixedId.TryParse(value, out var parsed));
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParse_HugeValue_ReturnsFalseWithoutScanningIt()
    {
        // The separator's position is known from the length, so an absurd input is rejected in
        // constant time rather than searched.
        var huge = new string('x', 1_000_000);

        Assert.False(PrefixedId.TryParse(huge, out _));
    }

    [Fact]
    public void TryParse_HugePrefix_RoundTrips()
    {
        var prefix = new string('x', 5_000);
        var written = new PrefixedId(prefix, Guid.Parse(FormatVectors.Version4Canonical)).ToString();

        Assert.True(PrefixedId.TryParse(written, out var parsed));
        Assert.Equal(prefix, parsed.Prefix);
    }

    // --- value semantics ---

    [Fact]
    public void Equals_SamePrefixAndUuid_AreEqual()
    {
        var uuid = Guid.Parse(FormatVectors.Version4Canonical);

        Assert.Equal(new PrefixedId("usr", uuid), new PrefixedId("usr", uuid));
    }

    [Fact]
    public void Equals_DifferentPrefix_AreNotEqual()
    {
        var uuid = Guid.Parse(FormatVectors.Version4Canonical);

        Assert.NotEqual(new PrefixedId("usr", uuid), new PrefixedId("org", uuid));
    }
}
