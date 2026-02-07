using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassifCentral.Lib;
using MassifCentral.Lib.Logging;
using MassifCentral.Lib.Utilities;
using Serilog;

// Initialize Serilog logger before anything else
Log.Logger = SerilogConfiguration.GetConfiguration(
    applicationName: Constants.ApplicationName)
    .CreateLogger();

try
{
    // Build host with dependency injection and Serilog configuration
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog() // Use Serilog for all framework and application logging
        .ConfigureServices((context, services) =>
        {
            // Register all MassifCentral application services
            services.AddMassifCentralServices();

            // Register Serilog adapter implementing custom ILogger interface
            services.AddSingleton<MassifCentral.Lib.Utilities.ILogger>(sp =>
            {
                var serilogLogger = Log.ForContext<Program>();
                return new SerilogLoggerAdapter(serilogLogger);
            });

            // Register correlation ID enricher
            services.AddSingleton<CorrelationIdEnricher>();
        })
        .Build();

    // Resolve logger from the service provider
    var logger = host.Services.GetRequiredService<MassifCentral.Lib.Utilities.ILogger>();
    
    logger.LogInfo("Starting {ApplicationName} v{Version}", 
        Constants.ApplicationName, 
        Constants.Version);

    // Application logic goes here
    logger.LogInfo("Application initialized successfully");
    Console.WriteLine("Welcome to MassifCentral!");

    logger.LogInfo("Application completed successfully");

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    // Ensure all logs are flushed before application shutdown
    Log.CloseAndFlush();
}

