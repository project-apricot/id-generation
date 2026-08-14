using ApricotFramework.IdGeneration;
using ApricotFramework.IdGeneration.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// One call registers both generators as singletons. There is nothing to configure.
builder.Services.AddIdGeneration();

var app = builder.Build();

// The identifier to hand out and store as text: prefixed, and random so it reveals nothing.
app.MapGet("/ids/{prefix}", (string prefix, IIdGenerator generator) => Results.Ok(new
{
    Id = generator.Generate(prefix),
}));

// The identifier to store in a native UUID column. Time-ordered, so successive rows land together
// in the index instead of scattering across it.
app.MapGet("/uuids", (IUuidGenerator generator) =>
{
    var uuid = generator.Generate();

    return Results.Ok(new
    {
        Uuid = uuid,
        Hex = uuid.ToString("N"),
    });
});

// Reading an identifier back. A malformed value is a 400 rather than an exception, which is what
// TryParse returning false is for.
app.MapGet("/parse", (string value) =>
{
    return PrefixedId.TryParse(value, out var parsed)
        ? Results.Ok(new { parsed.Prefix, parsed.Uuid })
        : Results.BadRequest(new { Error = $"'{value}' is not a prefixed identifier." });
});

// Walks the whole story end to end, including identifiers written before the migration.
app.MapGet("/demo", (IIdGenerator ids, IUuidGenerator uuids) =>
{
    // Written by an earlier version of this library. The format has not changed, so it still reads.
    const string LegacyUserId = "usr-0ce4800f7f7e49c48bf97f4e35bc9b1e";

    // A prefix containing the separator, the case a naive parser gets wrong.
    const string LegacyOrderLineId = "ord-line-6489433150c24f2b9802740aed309403";

    return Results.Ok(new
    {
        Fresh = Read(ids.Generate("usr")),
        Legacy = Read(LegacyUserId),
        OrderLine = Read(LegacyOrderLineId),
        Malformed = Read("usr-not-a-uuid"),

        // Two UUIDs generated back to back. Their leading digits match, because those digits are
        // the millisecond both were created in — that shared prefix is the index locality.
        FirstUuid = uuids.Generate().ToString("N"),
        SecondUuid = uuids.Generate().ToString("N"),
    });
});

app.Run();

// Shows what reading an identifier back gives you, including when it fails.
static object Read(string value)
{
    return PrefixedId.TryParse(value, out var parsed)
        ? new { Value = value, Parsed = true, Prefix = (string?)parsed.Prefix, Uuid = (Guid?)parsed.Uuid }
        : new { Value = value, Parsed = false, Prefix = (string?)null, Uuid = (Guid?)null };
}
