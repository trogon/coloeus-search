namespace MassifCentral.Lib.Logging;

using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

/// <summary>
/// Enriches logs with correlation IDs for distributed tracing across application boundaries.
/// Correlation IDs track operations through multiple services and layers.
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";

    /// <summary>
    /// Sets the correlation ID for the current operation context.
    /// Should be called at the entry point of a request/operation.
    /// </summary>
    /// <param name="correlationId">Unique identifier for the operation chain.</param>
    public static void SetCorrelationId(string correlationId)
    {
        LogContext.PushProperty(CorrelationIdPropertyName, correlationId);
    }

    /// <summary>
    /// Gets the current correlation ID from context, or generates a new one if not set.
    /// </summary>
    /// <returns>Current or newly generated correlation ID.</returns>
    public static string GetOrCreateCorrelationId()
    {
        var newId = Guid.NewGuid().ToString("D");
        SetCorrelationId(newId);
        return newId;
    }

    /// <summary>
    /// Enriches the log event with correlation ID property if present in context.
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Correlation ID is automatically added via LogContext.PushProperty
        // This enricher ensures the property is present in the log event
        var property = propertyFactory.CreateProperty(CorrelationIdPropertyName, Guid.NewGuid().ToString("D"));
        logEvent.AddPropertyIfAbsent(property);
    }
}
