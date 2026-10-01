# ADR-001: Clean Architecture & Request Dispatcher Pattern

## Status
**Accepted** (2025-08-15)

## Context
In early iterations of the StoreProject backend, controller action methods were accumulating excessive responsibilities: parsing HTTP parameters, performing direct EF Core database queries, running business calculations (tax brackets, discount evaluations, inventory stock adjustments), executing audit logging, and constructing responses.

This tight coupling introduced several significant failure modes:
1. **Difficult Unit Testing**: Testing business rules required setting up full ASP.NET Core `HttpContext` and in-memory databases.
2. **Duplicated Business Logic**: Cross-cutting rules (e.g. validating item margins or inventory availability) were repeated across POS endpoints, purchase order receiving, and catalog updates.
3. **Leaked Domain Entities**: Internal EF Core entity models were exposed directly over the wire, risking accidental data exposure and breaking API contracts when database schemas evolved.

## Decision
We adopted **Clean Architecture** with a decoupled **Application Layer** using the **Request Dispatcher** pattern:

1. **Layer Separation**:
   - **`Store.Models`**: Pure domain entities, value objects, request/response DTOs, and permission keys. Zero dependencies on ASP.NET Core or EF Core.
   - **`Store.API/Application/`**: Application layer containing Commands, Queries, Handlers, Validators, and Ports.
   - **`Store.DbServices`**: Data persistence, EF Core DbContext, entity configurations, and repository implementations.
   - **`Store.API/Controllers/`**: Thin HTTP protocol adapters. Controllers only handle model binding, invoke `IRequestDispatcher`, and translate domain results to HTTP status codes (`200 OK`, `201 Created`, `400 BadRequest`, `404 NotFound`).

2. **Boundary Contracts**:
   - DTOs never expose internal entity types. Mapping is strictly enforced at the application boundary.
   - Read queries executed via `IUnitOfWork` must declare `.AsNoTracking()` for performance and memory optimization.
   - All handler calls accept a `CancellationToken` as their final parameter.

## Alternatives Considered
- **Direct Controller-to-Repository Calls**: Retaining business logic inside controllers and calling generic repositories directly. Rejected because it preserves controller bloat and hinders automated unit testing.
- **Full MediatR Library Dependency**: Adopting third-party MediatR. Rejected in favor of a lean, custom `IRequestDispatcher` implementation within `Store.API/Application/` to avoid unnecessary external package dependencies while retaining pipeline extensibility.

## Consequences
### Positive
- **High Testability**: Handlers are pure C# classes easily tested with lightweight unit test fixtures without mocking HTTP contexts.
- **Maintainable Controllers**: Controllers average under 100 lines of boilerplate dispatch logic.
- **Strict Boundaries**: Prevent accidental database tracking leaks and unintended entity mutations.

### Trade-offs
- Increased file count (commands, queries, and handlers are separate classes).
- Developers must follow the command/query separation rather than writing quick inline LINQ queries in controllers.
