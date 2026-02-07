# Compatibility Analysis: Clean Architecture and SOLID

## Version Control
- **Version:** 1.0.0
- **Last Updated:** 2026-02-07
- **Change Summary:** Initial creation — analysis, examples, wrong usages, recommendations

---

## Summary

Short answer: Clean Architecture and SOLID are compatible and complementary. Clean Architecture defines high-level boundaries and dependency direction; SOLID provides principles for designing the classes and interfaces inside those boundaries.

## Definitions

- SOLID — five object-oriented design principles:
  - Single Responsibility Principle (SRP)
  - Open/Closed Principle (OCP)
  - Liskov Substitution Principle (LSP)
  - Interface Segregation Principle (ISP)
  - Dependency Inversion Principle (DIP)
- Clean Architecture — layered architectural style (entities, use-cases/interactors, interfaces/adapters, frameworks) with a dependency rule: source code dependencies point inward toward the business core.

## Why they're compatible

- They operate at different levels: SOLID targets class/module design; Clean targets system structure and dependencies between layers.
- DIP directly supports Clean's dependency direction: depend on abstractions inwards rather than on concrete outer-layer implementations.
- SRP, ISP and OCP help keep each layer focused, small, and extensible without leaking concerns across boundaries.

## Mapping SOLID → Clean

- SRP: keep entities and use-cases narrowly focused inside the core layers.
- OCP: extend outer adapters without changing core business rules; prefer strategy/adapter patterns at boundaries.
- LSP: design substitute-able domain abstractions so different outer implementations can replace each other without surprise.
- ISP: define thin, intention-revealing interfaces for layer boundaries rather than fat all-purpose interfaces.
- DIP: core depends on interfaces; implementations live in outer layers and are injected at the composition root.

## Correct (concise C# example)

Core layers define ports (interfaces), entities, and use-cases that depend only on abstractions.

```csharp
// Core/Ports/IUserRepository.cs
public interface IUserRepository
{
    User? GetById(Guid id);
}

// Core/Entities/User.cs
public record User(Guid Id, string Name);

// Core/UseCases/GetUser.cs
public class GetUser
{
    private readonly IUserRepository _repo;
    public GetUser(IUserRepository repo) => _repo = repo;
    public User? Execute(Guid id) => _repo.GetById(id);
}
```

Infra/outer layer implements the interface and lives outside the core:

```csharp
// Infra/Adapters/SqlUserRepository.cs
public class SqlUserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public SqlUserRepository(AppDbContext db) => _db = db;
    public User? GetById(Guid id)
    {
        var e = _db.Users.Find(id);
        return e == null ? null : new User(e.Id, e.Name);
    }
}
```

Composition root (wiring):

```csharp
var db = new AppDbContext(...);
IUserRepository repo = new SqlUserRepository(db);
var getUser = new GetUser(repo);
var user = getUser.Execute(someId);
```

Why this is good: core types depend only on `IUserRepository` (abstraction). Implementations are outer concerns injected at the composition root. This satisfies Clean's dependency rule and SOLID (notably DIP and SRP).

## Wrong usages (common anti-patterns)

### 1. Core depends on concrete infra (breaks Clean + DIP)

```csharp
// BAD: Core/UseCases/GetUser.cs depends on concrete SQL repo
public class GetUser
{
    private readonly SqlUserRepository _repo; // concrete infra type in core — BAD
    public GetUser(SqlUserRepository repo) => _repo = repo;
    public User? Execute(Guid id) => _repo.GetById(id);
}
```

**Problem:** Core cannot be tested or reused without the infra; business rules are coupled to persistence.

### 2. Service locator / static access (hidden dependencies, hard to test)

```csharp
// BAD: hidden dependency via global/service locator
public class GetUser
{
    public User? Execute(Guid id)
    {
        return ServiceLocator.Current.UserRepo.GetById(id); // hidden dependency
    }
}
```

**Problem:** Dependencies are implicit, testing requires global setup, and the dependency graph is obscured — violates explicit wiring and DIP intent.

### 3. Premature/over-abstraction (too many interfaces)

**Problem:** Creating an interface for every tiny class increases indirection and maintenance cost. Prefer interfaces where there are multiple implementations, unstable concrete dependencies, or testing benefits.

## Practical recommendations

- Use Clean Architecture to define layer boundaries and the allowed direction of dependencies.
- Apply SOLID within layers: use SRP for focused classes, DIP for inward dependencies, ISP for thin boundary interfaces, OCP for stable extension points, and LSP where substitutability matters.
- Inject concrete implementations at the composition root; keep the core framework-agnostic and testable.
- Avoid service-locator and static singletons for cross-layer dependencies.
- Avoid premature abstraction: introduce interfaces when they solve a concrete need (multiple implementations, testing, or stable boundary).

## Example minimal project layout

```
Core/
  ├── Entities/
  ├── Ports/
  └── UseCases/
Infra/
  ├── Adapters/
  └── Persistence/
Api/ (or UI)
CompositionRoot/Program.cs
```

Place interfaces in the `Core/Ports` area and implementations in `Infra/Adapters`. Wire everything in `CompositionRoot/Program.cs`.
