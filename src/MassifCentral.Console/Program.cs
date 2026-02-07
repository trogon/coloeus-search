using MassifCentral.Lib;
using MassifCentral.Lib.Utilities;

Logger.LogInfo($"Starting {Constants.ApplicationName} v{Constants.Version}");

try
{
    // Application logic goes here
    Logger.LogInfo("Application initialized successfully");
    Console.WriteLine("Welcome to MassifCentral!");
}
catch (Exception ex)
{
    Logger.LogError("An error occurred during application execution", ex);
    Environment.Exit(1);
}

Logger.LogInfo("Application completed successfully");
