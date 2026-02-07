using Microsoft.Extensions.DependencyInjection;
using MassifCentral.Lib.Utilities;

namespace MassifCentral.Lib;

/// <summary>
/// Extension methods for configuring MassifCentral services in the dependency injection container.
/// Provides centralized service registration to maintain clean startup code.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MassifCentral application services to the service collection.
    /// Registers core logging and utility services with appropriate lifetimes.
    /// </summary>
    /// <param name="services">The service collection to register services in.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// services.AddMassifCentralServices();
    /// var serviceProvider = services.BuildServiceProvider();
    /// </code>
    /// </example>
    public static IServiceCollection AddMassifCentralServices(
        this IServiceCollection services)
    {
        // Register core logging service as singleton
        // Logger is thread-safe and stateless, so singleton is appropriate
        services.AddSingleton<ILogger, Logger>();

        // TODO: Register repository services
        // services.AddScoped<IUserRepository, UserRepository>();

        // TODO: Register use case services
        // services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();

        // TODO: Register application service interfaces
        // services.AddScoped<IApplicationService, ApplicationService>();

        return services;
    }
}
