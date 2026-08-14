using ApricotFramework.IdGeneration.AspNetCore.Extensions;
using ApricotFramework.IdGeneration.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace ApricotFramework.IdGeneration.AspNetCore.Tests;

public class IdGenerationServiceCollectionExtensionsTests
{
    /// <summary>
    /// An identifier generator a caller might register instead of the built-in one.
    /// </summary>
    private sealed class FixedIdGenerator : IIdGenerator
    {
        /// <summary>
        /// The identifier this always returns.
        /// </summary>
        public const string FixedId = "fixed-00000000000000000000000000000000";

        /// <inheritdoc />
        public string Generate(string prefix)
        {
            return FixedId;
        }
    }

    /// <summary>
    /// A UUID generator a caller might register instead of the built-in one, standing in for a
    /// version 4 or database-ordered UUID.
    /// </summary>
    private sealed class FixedUuidGenerator : IUuidGenerator
    {
        /// <summary>
        /// The UUID this always returns.
        /// </summary>
        public static readonly Guid FixedUuid = Guid.Parse("3f2a4c1e-8b7d-40f9-a1c2-6e5d90b3f748");

        /// <inheritdoc />
        public Guid Generate()
        {
            return FixedUuid;
        }
    }

    /// <summary>
    /// Builds a provider over a service collection the caller has configured.
    /// </summary>
    /// <param name="configure">Configures the services, or null to only add id generation.</param>
    /// <returns>The provider. Scope validation is on, so a lifetime mistake fails the test.</returns>
    private static ServiceProvider CreateProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();

        configure?.Invoke(services);

        services.AddIdGeneration();

        return services.BuildServiceProvider(validateScopes: true);
    }

    // --- registration ---

    [Fact]
    public void AddIdGeneration_Always_ResolvesTheDefaultIdGenerator()
    {
        using var provider = CreateProvider();

        Assert.IsType<DefaultIdGenerator>(provider.GetRequiredService<IIdGenerator>());
    }

    [Fact]
    public void AddIdGeneration_Always_ResolvesTheDefaultUuidGenerator()
    {
        using var provider = CreateProvider();

        Assert.IsType<DefaultUuidGenerator>(provider.GetRequiredService<IUuidGenerator>());
    }

    [Fact]
    public void AddIdGeneration_Always_RegistersTheIdGeneratorAsASingleton()
    {
        using var provider = CreateProvider();

        Assert.Same(provider.GetRequiredService<IIdGenerator>(), provider.GetRequiredService<IIdGenerator>());
    }

    [Fact]
    public void AddIdGeneration_Always_RegistersTheUuidGeneratorAsASingleton()
    {
        using var provider = CreateProvider();

        Assert.Same(provider.GetRequiredService<IUuidGenerator>(), provider.GetRequiredService<IUuidGenerator>());
    }

    [Fact]
    public void AddIdGeneration_FromAScope_ResolvesTheSameInstanceAsTheRoot()
    {
        // A singleton generator must be usable from scoped code, which is where it is normally
        // consumed — a request handler.
        using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        Assert.Same(
            provider.GetRequiredService<IIdGenerator>(),
            scope.ServiceProvider.GetRequiredService<IIdGenerator>());
    }

    [Fact]
    public void AddIdGeneration_CalledTwice_RegistersOneOfEach()
    {
        var services = new ServiceCollection();

        services.AddIdGeneration();
        services.AddIdGeneration();

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IIdGenerator));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IUuidGenerator));
    }

    [Fact]
    public void AddIdGeneration_Always_AddsNothingElse()
    {
        // The package exists to register two services. Anything else here would be a dependency a
        // consumer did not ask for.
        var services = new ServiceCollection();

        services.AddIdGeneration();

        Assert.Equal(2, services.Count);
    }

    [Fact]
    public void AddIdGeneration_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(services.AddIdGeneration);
    }

    // --- the caller's own implementation wins ---

    [Fact]
    public void AddIdGeneration_AfterACustomIdGenerator_KeepsTheCustomOne()
    {
        using var provider = CreateProvider(services => services.AddSingleton<IIdGenerator, FixedIdGenerator>());

        Assert.Equal(FixedIdGenerator.FixedId, provider.GetRequiredService<IIdGenerator>().Generate("usr"));
    }

    [Fact]
    public void AddIdGeneration_AfterACustomUuidGenerator_KeepsTheCustomOne()
    {
        using var provider = CreateProvider(services => services.AddSingleton<IUuidGenerator, FixedUuidGenerator>());

        Assert.Equal(FixedUuidGenerator.FixedUuid, provider.GetRequiredService<IUuidGenerator>().Generate());
    }

    [Fact]
    public void AddIdGeneration_BeforeACustomIdGenerator_TheLastRegistrationWins()
    {
        // TryAdd claims the service, but the container resolves the last registration, so a caller
        // registering afterwards still overrides it. Verified rather than assumed.
        var services = new ServiceCollection();

        services.AddIdGeneration();
        services.AddSingleton<IIdGenerator, FixedIdGenerator>();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Equal(FixedIdGenerator.FixedId, provider.GetRequiredService<IIdGenerator>().Generate("usr"));
    }

    // --- what the resolved services actually produce ---

    [Fact]
    public void ResolvedIdGenerator_Generate_ProducesAPrefixedIdentifierThatReadsBack()
    {
        using var provider = CreateProvider();

        var id = provider.GetRequiredService<IIdGenerator>().Generate("usr");

        Assert.True(PrefixedId.TryParse(id, out var parsed));
        Assert.Equal("usr", parsed.Prefix);
    }

    [Fact]
    public void ResolvedUuidGenerator_Generate_ProducesDistinctValues()
    {
        using var provider = CreateProvider();

        var generator = provider.GetRequiredService<IUuidGenerator>();

        Assert.NotEqual(generator.Generate(), generator.Generate());
    }
}
