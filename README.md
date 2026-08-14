# ApricotFramework.IdGeneration

[![NuGet](https://img.shields.io/nuget/v/ApricotFramework.IdGeneration.svg?label=ApricotFramework.IdGeneration)](https://www.nuget.org/packages/ApricotFramework.IdGeneration/)
[![NuGet](https://img.shields.io/nuget/v/ApricotFramework.IdGeneration.AspNetCore.svg?label=ApricotFramework.IdGeneration.AspNetCore)](https://www.nuget.org/packages/ApricotFramework.IdGeneration.AspNetCore/)
[![CI](https://github.com/project-apricot/id-generation/actions/workflows/ci.yml/badge.svg)](https://github.com/project-apricot/id-generation/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://github.com/project-apricot/id-generation/blob/main/LICENSE)

Identifier generation for .NET: self-describing prefixed strings such as `usr-3f2a…` for identifiers
you hand out, and time-ordered UUIDv7 `Guid` values for keys the database owns.

`ApricotFramework.IdGeneration` is the **zero-dependency** core.

## Install

```bash
dotnet add package ApricotFramework.IdGeneration
dotnet add package ApricotFramework.IdGeneration.AspNetCore
```

## Usage

```csharp
using ApricotFramework.IdGeneration.AspNetCore.Extensions;

builder.Services.AddIdGeneration();
```

```csharp
using ApricotFramework.IdGeneration;

// A prefixed string identifier: random, so it reveals nothing about when it was made.
string userId = idGenerator.Generate("usr");        // "usr-3f2a4c1e8b7d40f9a1c26e5d90b3f748"

// Read one back. Malformed input is a false, not an exception.
if (PrefixedId.TryParse(userId, out var parsed))
{
    Console.WriteLine($"{parsed.Prefix} / {parsed.Uuid}");
}

// A key for a native uuid column: UUIDv7, so successive rows sort together in the index.
Guid key = uuidGenerator.Generate();
```

The prefixed string form, `{prefix}-{32 lowercase hex characters}`, is what consumers persist, so it
is a compatibility contract and will not change without a major version. The two generators differ
deliberately: string identifiers are UUIDv4 because they travel in URLs and payloads, while a UUIDv7
carries the millisecond it was created in — excellent for index locality, wrong for something you
hand to a client.

Full documentation: <https://projectapricot.dev>
