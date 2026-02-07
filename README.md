# MassifCentral

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)

A .NET 10 console application with a shared library for scalable development.

## Project Structure

The solution follows .NET best practices with the following structure:

```
src/
├── MassifCentral.Console/      - Console application entry point
└── MassifCentral.Lib/          - Shared library with reusable code
    ├── Models/                 - Domain entities and base classes
    ├── Utilities/              - Helper utilities and services
    └── Constants.cs            - Application constants

tests/
└── MassifCentral.Tests/        - Unit tests (xUnit)

docs/
├── REQUIREMENTS.md             - Project requirements document
└── DESIGN.md                   - Architecture and design document
```

## Features

### MassifCentral.Lib - Shared Library

#### Constants
Application-wide constants including version and application name.

#### BaseEntity
Abstract base class for all domain entities providing:
- Unique identifier (Guid)
- Creation and modification timestamps (UTC)
- Active status flag

#### Logger
Logging utility with support for Info, Warning, and Error levels with timestamps.

## Quick Start

### Building the Project

```bash
dotnet build
```

### Running the Application

```bash
dotnet run --project src/MassifCentral.Console
```

### Running Tests

```bash
dotnet test
```

## NuGet Packages

### Library

Package: Trogon.MassifCentral.Lib

```bash
dotnet add package Trogon.MassifCentral.Lib
```

### Dotnet Tool

Package: Trogon.MassifCentral
Command: tmcfind

```bash
dotnet tool install -g Trogon.MassifCentral
```

## Documentation

- **[Requirements Document](docs/REQUIREMENTS.md)** - Comprehensive functional and non-functional requirements
- **[Design Document](docs/DESIGN.md)** - Architecture overview, component design, and patterns

## Requirements

- .NET 10 SDK or later
- Visual Studio Code or Visual Studio 2022

## License

MIT. See [LICENSE](LICENSE).
