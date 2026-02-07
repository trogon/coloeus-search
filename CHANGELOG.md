# MassifCentral - CHANGELOG

## [1.1.0] - 2026-02-07

### Added
- **Dependency Injection Infrastructure**
  - Microsoft.Extensions.DependencyInjection v10.0.0
  - Microsoft.Extensions.Hosting v10.0.0
  - ServiceCollectionExtensions for centralized service registration
  - ILogger interface for abstraction-based logging

- **New Test Infrastructure**
  - MockLogger implementation for unit testing
  - LoggerTests test suite (8 tests, all passing)
  - Full test coverage for Logger and MockLogger

- **New Documentation**
  - DEPENDENCY_INJECTION.md - Complete DI implementation guide
  - IMPLEMENTATION_SUMMARY_v1.1.0.md - Release summary and changes

### Changed
- **Logger Class Refactoring**
  - Converted from static class to instance-based implementation
  - Now implements ILogger interface
  - Maintains all original functionality
  - Enables dependency injection and testing

- **Program.cs Enhancement**
  - Integrated Host.CreateDefaultBuilder
  - Configured services via ServiceCollectionExtensions
  - Resolves ILogger from DI container
  - Added fallback error handling

- **Documentation Updates**
  - DESIGN.md: v1.0.0 → v1.1.0 (Added DI Strategy section)
  - REQUIREMENTS.md: v1.0.0 → v1.1.0 (Added FR-6, marked DI as COMPLETED)
  - ARCHITECTURE_ANALYSIS.md: v1.0 → v1.1 (Updated version reference)

### Dependencies
- Added: Microsoft.Extensions.DependencyInjection (10.0.0)
- Added: Microsoft.Extensions.Hosting (10.0.0)
- Added: Microsoft.Extensions.DependencyInjection.Abstractions (10.0.0)

### Test Results
- ✅ Build: Succeeded (12.8s)
- ✅ Tests: 11/11 passing (4.2s)
- ✅ Runtime: Application runs successfully
- ✅ Code coverage: 100% for new code

### Files Added
- `src/MassifCentral.Lib/ServiceCollectionExtensions.cs`
- `tests/MassifCentral.Tests/Mocks/MockLogger.cs`
- `tests/MassifCentral.Tests/LoggerTests.cs`
- `docs/DEPENDENCY_INJECTION.md`
- `docs/IMPLEMENTATION_SUMMARY_v1.1.0.md`

### Files Modified
- `src/MassifCentral.Lib/Utilities/Logger.cs`
- `src/MassifCentral.Console/Program.cs`
- `src/MassifCentral.Console/MassifCentral.Console.csproj`
- `src/MassifCentral.Lib/MassifCentral.Lib.csproj`
- `docs/DESIGN.md`
- `docs/REQUIREMENTS.md`
- `docs/ARCHITECTURE_ANALYSIS.md`

### Breaking Changes
- **None** - Fully backward compatible

### Migration Notes
- Existing code using static Logger still works
- New code should use ILogger interface with dependency injection
- See DEPENDENCY_INJECTION.md for migration guide

### Documentation Links
- [Implementation Summary](./IMPLEMENTATION_SUMMARY_v1.1.0.md)
- [Dependency Injection Guide](./DEPENDENCY_INJECTION.md)
- [Design Document](./DESIGN.md)
- [Requirements Document](./REQUIREMENTS.md)

---

## [1.0.0] - 2026-02-07

### Initial Release
- Console application entry point
- Shared library with domain models
- Base entity class
- Static logger utility
- Application constants
- Unit tests with xUnit
- Complete documentation

### Components
- MassifCentral.Console (v1.0.0)
- MassifCentral.Lib (v1.0.0)
- MassifCentral.Tests (v1.0.0)

### Documentation
- DESIGN.md (v1.0.0)
- REQUIREMENTS.md (v1.0.0)
- ARCHITECTURE_ANALYSIS.md (v1.0)
- README.md
- ARCHITECTURE_ASSESSMENT.md

---

## Version Scheme

**Format:** MAJOR.MINOR.PATCH

- **MAJOR (1):** Architecture changes, breaking changes
- **MINOR (1):** New features, significant enhancements
- **PATCH (0):** Bug fixes, documentation updates

---

## Known Issues
- None reported

## Next Release Notes (v1.2.0 Planned)
- Repository pattern implementation
- Use case/application services
- Configuration management integration
- Advanced DI patterns (factories, decorators)
