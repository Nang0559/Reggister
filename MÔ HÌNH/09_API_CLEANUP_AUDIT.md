# 09 — API Architecture Cleanup Audit

## Scope

`FVN_REGISTER.API` is kept as the HTTP/composition-root boundary over the agreed architecture:

```text
HTTP / JWT / routing
        ↓
API Controllers
        ↓
Application interfaces / orchestrators
        ↓
Infrastructure implementations
        ↓
Core repositories / EF / database
```

Controllers do not access EF entities, `DbContext`, or Infrastructure services directly for business operations. Infrastructure references remain confined to the composition root (`Program.cs`) for dependency registration and hosting integration.

## Cleanup completed

- Removed the legacy `AppAuthorizeAttribute` based on `HttpContext.Items`.
- Removed the unused legacy `JwtMiddleware`; JWT validation is owned by ASP.NET Core JwtBearer authentication.
- Removed `AutoMapper` from `BaseApiController` and all API controllers; controllers no longer carry an unused mapping dependency.
- Moved the authentication diagnostic `test-auth` endpoint from the shared base controller into `AuthController`.
- Removed the legacy `Contract.ViewModels` dependency from `DashboardController`.
- `ReportController` now delegates to `IReportDispatcher`; report-type selection is no longer performed by the controller.
- `OTSyncController` uses constructor injection for `IWebHostEnvironment` instead of `RequestServices` service location.
- Removed `UsePathBase("/api")` because API controllers already own the `/api/...` route prefix; this prevents a duplicated `/api/api/...` route base.
- Moved endpoint-specific request models from controllers into `Contract.Requests`:
  - authentication refresh/logout;
  - history cancellation;
  - leave approval/cancellation;
  - OT approval/cancellation/hour validation;
  - user password reset;
  - email batch operations.
- Invalid history request kinds are rejected instead of silently being treated as Leave.
- `LeaveCalendarController` is query-only and no longer injects an unused command service.

## Composition root

`Program.cs` remains the only API location that wires Infrastructure implementations, EF, SignalR, background workers and authentication. Application contracts remain the dependency direction used by controllers.

## Verification

The repository was edited directly on `main`. A local clean `dotnet build` has not been claimed because this execution environment cannot clone the repository from GitHub. The API cleanup was performed against the current `main` source and existing Application/Contract/Infrastructure contracts; CI/build verification remains the final external verification step.
