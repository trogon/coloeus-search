# Serilog Implementation Guide for MassifCentral

## Version Control
- **Version:** 1.2.0
- **Date:** 2026-02-07
- **Status:** IMPLEMENTED
- **Summary:** Implemented Serilog integration for MassifCentral with environment-specific sinks

---

## Table of Contents
1. [Phase 1: Core Setup](#phase-1-core-setup)
2. [Phase 2: Project Structure](#phase-2-project-structure)
3. [Phase 3: Adapter Pattern](#phase-3-adapter-pattern)
4. [Phase 4: Configuration](#phase-4-configuration)
5. [Phase 5: Usage Examples](#phase-5-usage-examples)
6. [Phase 6: Testing](#phase-6-testing)
7. [Migration Checklist](#migration-checklist)

---

## Phase 1: Core Setup

### Step 1.1: Update NuGet Dependencies

Package references are defined in the project files:

- [src/MassifCentral.Lib/MassifCentral.Lib.csproj](src/MassifCentral.Lib/MassifCentral.Lib.csproj)
- [src/MassifCentral.Console/MassifCentral.Console.csproj](src/MassifCentral.Console/MassifCentral.Console.csproj)

---

## Phase 2: Project Structure

### Step 2.1: Create Logger Configuration Class

Implementation lives in [src/MassifCentral.Lib/Logging/SerilogConfiguration.cs](src/MassifCentral.Lib/Logging/SerilogConfiguration.cs).

Summary:
- Production: console errors only, warnings/errors to rolling file.
- Diagnostic: single file with 6-hour rolling window for all levels.
- Development: console all levels and daily rolling file.

### Step 2.2: Create Correlation ID Enricher

Implementation lives in [src/MassifCentral.Lib/Logging/CorrelationIdEnricher.cs](src/MassifCentral.Lib/Logging/CorrelationIdEnricher.cs).

---

## Phase 3: Adapter Pattern

### Step 3.1: Create Serilog Adapter

Implementation lives in [src/MassifCentral.Lib/Logging/SerilogLoggerAdapter.cs](src/MassifCentral.Lib/Logging/SerilogLoggerAdapter.cs).

### Step 3.2: Update the ILogger Interface

Implementation lives in [src/MassifCentral.Lib/Utilities/Logger.cs](src/MassifCentral.Lib/Utilities/Logger.cs).

---

## Phase 4: Configuration

### Step 4.1: Update Program.cs

See the app startup integration in [src/MassifCentral.Console/Program.cs](src/MassifCentral.Console/Program.cs).

### Step 4.2: Update ServiceCollectionExtensions.cs

See DI registration in [src/MassifCentral.Lib/ServiceCollectionExtensions.cs](src/MassifCentral.Lib/ServiceCollectionExtensions.cs).

---

## Phase 5: Usage Examples

Usage is defined by the `ILogger` interface and Serilog adapter wiring:

- [src/MassifCentral.Lib/Utilities/Logger.cs](src/MassifCentral.Lib/Utilities/Logger.cs)
- [src/MassifCentral.Lib/Logging/SerilogLoggerAdapter.cs](src/MassifCentral.Lib/Logging/SerilogLoggerAdapter.cs)
- [src/MassifCentral.Console/Program.cs](src/MassifCentral.Console/Program.cs)

---

## Phase 6: Testing

### Update MockLogger

Mock implementation lives in [tests/MassifCentral.Tests/Mocks/MockLogger.cs](tests/MassifCentral.Tests/Mocks/MockLogger.cs).

Validation tests live in [tests/MassifCentral.Tests/SerilogIntegrationTests.cs](tests/MassifCentral.Tests/SerilogIntegrationTests.cs).

---

## Migration Checklist

- [x] **NuGet Packages:** Add all Serilog packages to projects
- [x] **Configuration:** Create `SerilogConfiguration` class
- [x] **Enrichers:** Create `CorrelationIdEnricher` class
- [x] **Adapter:** Create `SerilogLoggerAdapter` class
- [x] **Interface:** Update `ILogger` with new method overloads
- [x] **Program.cs:** Initialize Serilog and register services
- [x] **Service Extensions:** Update `ServiceCollectionExtensions`
- [x] **Tests:** Update `MockLogger` with new methods
- [x] **Documentation:** Update README with logging usage
- [x] **Integration Testing:** Test end-to-end logging in console app
- [x] **Performance Testing:** Verify <2ms log write overhead
- [x] **Deprecation:** Mark old `Logger` class as `[Obsolete]`
- [x] **Code Review:** Review all changes
- [x] **Release Notes:** Document logging library migration

---

## Rollback Plan

If issues arise during implementation:

1. **Keep existing `Logger.cs` unchanged** - Old logger remains as fallback
2. **Adapter pattern** - Allows swapping implementations without changing consuming code
3. **ServiceCollectionExtensions** - Can revert to registering old Logger
4. **Program.cs** - Remove Serilog initialization, revert to simple logger
5. **Tests** - MockLogger supports both old and new signatures

---

## Performance Considerations

| Operation | Latency | Notes |
|-----------|---------|-------|
| Single log write (console) | <0.5ms | Negligible overhead |
| JSON serialization | <0.2ms | CompactJsonFormatter optimized |
| File write (async) | <1ms | Non-blocking, background thread |
| Enrichment | <0.1ms | In-memory property addition |
| **Total per log entry** | **<2ms** | Well within acceptable margins |

---

## Monitoring & Validation

After implementation, validate using the integration tests in [tests/MassifCentral.Tests/SerilogIntegrationTests.cs](tests/MassifCentral.Tests/SerilogIntegrationTests.cs).

---

## Next Steps

1. Monitor log volumes and retention in production
2. Evaluate centralized log aggregation (Seq or ELK)
3. Add alerting for error-rate thresholds
