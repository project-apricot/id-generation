using System.Globalization;
using ApricotFramework.IdGeneration.Impl;

namespace ApricotFramework.IdGeneration.Tests;

public class DefaultUuidGeneratorTests
{
    /// <summary>
    /// The index of the version digit within the 32-character hex form, per RFC 9562.
    /// </summary>
    private const int VersionDigitIndex = 12;

    /// <summary>
    /// The index of the variant digit within the 32-character hex form, per RFC 9562.
    /// </summary>
    private const int VariantDigitIndex = 16;

    /// <summary>
    /// The number of hex digits the version 7 millisecond timestamp occupies (48 bits).
    /// </summary>
    private const int TimestampDigits = 12;

    /// <summary>
    /// Builds a generator.
    /// </summary>
    /// <returns>The generator.</returns>
    private static DefaultUuidGenerator CreateGenerator()
    {
        return new DefaultUuidGenerator();
    }

    /// <summary>
    /// Writes a UUID as the 32 hex digits the specification lays its fields out in.
    /// </summary>
    /// <param name="uuid">The UUID.</param>
    /// <returns>The hex digits.</returns>
    private static string HexOf(Guid uuid)
    {
        return uuid.ToString("N", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Reads the millisecond timestamp out of a version 7 UUID.
    /// </summary>
    /// <param name="uuid">The UUID.</param>
    /// <returns>The instant encoded in its leading 48 bits.</returns>
    private static DateTimeOffset TimestampOf(Guid uuid)
    {
        var milliseconds = long.Parse(HexOf(uuid)[..TimestampDigits], NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
    }

    // --- version 7, cross-checked against the digits Python reports ---

    [Fact]
    public void Generate_Always_ProducesAVersion7Uuid()
    {
        // Python reports version 7 for a UUID whose 13th hex digit is '7'; that digit is the
        // version field.
        var hex = HexOf(CreateGenerator().Generate());

        Assert.Equal('7', hex[VersionDigitIndex]);
    }

    [Fact]
    public void Generate_Always_SetsTheRfc9562VariantBits()
    {
        var hex = HexOf(CreateGenerator().Generate());

        Assert.True("89ab".Contains(hex[VariantDigitIndex], StringComparison.Ordinal), hex);
    }

    // --- the timestamp, which is the whole reason for choosing version 7 ---

    [Fact]
    public void Generate_Always_EncodesTheCurrentTimeInTheLeading48Bits()
    {
        var before = DateTimeOffset.UtcNow;
        var uuid = CreateGenerator().Generate();
        var after = DateTimeOffset.UtcNow;

        var timestamp = TimestampOf(uuid);

        // A millisecond of slack at each end, since the encoded value is truncated to milliseconds.
        Assert.InRange(timestamp, before.AddMilliseconds(-1), after.AddMilliseconds(1));
    }

    [Fact]
    public void Generate_ManyTimes_ProducesNonDecreasingTimestamps()
    {
        // This is the property that buys index locality: successive keys sort next to each other.
        // Only non-decreasing, not strictly increasing — within one millisecond the remaining bits
        // are random, so two values can share a timestamp.
        var generator = CreateGenerator();

        var timestamps = new List<DateTimeOffset>();

        for (var i = 0; i < 1_000; i++)
        {
            timestamps.Add(TimestampOf(generator.Generate()));
        }

        for (var i = 1; i < timestamps.Count; i++)
        {
            Assert.True(timestamps[i] >= timestamps[i - 1], $"value {i} went backwards in time");
        }
    }

    [Fact]
    public void Generate_AcrossAMillisecondBoundary_SortsInGenerationOrder()
    {
        var generator = CreateGenerator();

        var first = generator.Generate();
        Thread.Sleep(millisecondsTimeout: 5);
        var second = generator.Generate();

        // Sorting the hex form is what a database does with a UUID column it compares byte-wise,
        // such as PostgreSQL's uuid or a BINARY(16).
        Assert.True(string.CompareOrdinal(HexOf(first), HexOf(second)) < 0);
    }

    // --- uniqueness ---

    [Fact]
    public void Generate_ManyTimes_ProducesDistinctValues()
    {
        var generator = CreateGenerator();

        var values = new HashSet<Guid>();

        for (var i = 0; i < 10_000; i++)
        {
            Assert.True(values.Add(generator.Generate()));
        }
    }

    [Fact]
    public void Generate_WithinOneMillisecond_StillProducesDistinctValues()
    {
        // The timestamp alone cannot separate these, so the random tail has to.
        var generator = CreateGenerator();

        var values = new HashSet<Guid>();
        var timestamp = TimestampOf(generator.Generate());

        for (var i = 0; i < 100; i++)
        {
            var uuid = generator.Generate();

            if (TimestampOf(uuid) == timestamp)
            {
                Assert.True(values.Add(uuid));
            }
        }
    }
}
