using ApricotFramework.IdGeneration.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApricotFramework.IdGeneration.AspNetCore.Extensions;

/// <summary>
/// Registers the identifier generators in a service collection.
/// </summary>
public static class IdGenerationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the prefixed string identifier generator and the UUID generator.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for chaining.</returns>
    /// <remarks>
    /// Both are singletons, which the implementations allow because they hold no state. Both use
    /// <c>TryAdd</c>, so an implementation the caller registered first is left in place and calling
    /// this twice is harmless. There is nothing to configure: the string generator writes random
    /// (version 4) UUIDs and the UUID generator writes time-ordered (version 7) ones. To change
    /// either, register your own implementation of <see cref="IIdGenerator"/> or
    /// <see cref="IUuidGenerator"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddIdGeneration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IIdGenerator, DefaultIdGenerator>();
        services.TryAddSingleton<IUuidGenerator, DefaultUuidGenerator>();

        return services;
    }
}
