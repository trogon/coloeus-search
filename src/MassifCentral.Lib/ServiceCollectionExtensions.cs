using Microsoft.Extensions.DependencyInjection;
using MassifCentral.Lib.Logging;
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
    /// Includes infrastructure services and registers core logging and utility services with appropriate lifetimes.
    /// Note: ILogger is registered in Program.cs via SerilogLoggerAdapter after Serilog initialization.
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
        // Register correlation ID enricher as singleton
        // Used for distributed tracing across service boundaries
        services.AddSingleton<CorrelationIdEnricher>();

        // Note: ILogger (SerilogLoggerAdapter) is registered in Program.cs after Serilog initialization
        // This ensures proper logging context throughout the application

        // TODO: Register repository services
        // services.AddScoped<IUserRepository, UserRepository>();

        // TODO: Register use case services
        // services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();

        // TODO: Register application service interfaces
        // services.AddScoped<IApplicationService, ApplicationService>();

        return services;
    }
}

