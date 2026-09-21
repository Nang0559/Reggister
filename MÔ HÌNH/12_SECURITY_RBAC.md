# Security & Authorization — FVN_REGISTER

## 1. Ownership

HRM is the source of truth for HR master data. FVN_REGISTER owns application security.

HR synchronization may update employee identity/master fields and may resolve a default role through F03HrmUserRoleRules. It must not overwrite FVN role/function assignments, passwords, lockout state, sessions, or security settings.

## 2. Authorization model

User
- primary/legacy role: F03Users.PermissionCode
- normalized roles: F03UserRoles -> F03Roles -> F03RoleFunctions -> F03Functions
- legacy direct grants: F03UserFunctions

F03UserFunctions remains effective during migration. New code should use IAuthorizationService and normalized role/function permissions.

## 3. Capability model

Each F03Function is a server-enforced capability with:
- ModuleCode
- ActionCode
- ScopeCode
- stable FunctionCode

Examples:
- Leave: View/Create/Edit/Cancel/Approve/Export
- OT: View/Create/Edit/Cancel/Approve/Reconcile/Export
- Trip: View/Create/Edit/Cancel/Approve
- Equipment: View/Create/Edit/Repair/Approve
- UserManagement: View/Create/Edit/Lock/ResetPassword/AssignPermission
- HrmSync: ViewStatus/Sync/Review/Retry
- Security: View/ManageRoles/ManageFunctions/Audit
- Dashboard: View

## 4. Server-side enforcement

Authentication -> Current User -> IAuthorizationService -> Effective Permissions -> Controller/Orchestrator -> Business Service -> Data Scope.

Hiding a button in Blazor is not authorization. API endpoints enforce the capability.

## 5. HRM boundary

HRM
  -> Employee / Department / Position / Leave Type / Shift / Schedule
  -> FVN_REGISTER local masters

User provisioning is separate:
F03Employee
  -> optional auto-provision -> F03User
  -> FVN UI manual create -> F03User

HRM must never become the source of truth for FVN authorization.

## 6. Security Center

Route: /admin/security

The UI provides:
- user search
- role assignment
- effective permission view
- role/function catalog
- server-side persistence

Effective permissions are read from the server, not inferred only from JWT claims.

## 7. Migration compatibility

Existing objects are retained:
- F03Permissions.PermissionCode
- F03Users.PermissionCode
- F03UserFunctions

Normalized RBAC adds:
- F03Roles
- F03RoleFunctions
- F03UserRoles
- F03Functions.ModuleCode
- F03Functions.ActionCode
- F03Functions.ScopeCode

## 8. Rules

1. Never check numeric role codes inside business services.
2. Prefer IAuthorizationService.HasAsync(user, SecurityFunctionCodes.X).
3. Never trust client-side visibility as authorization.
4. HRM synchronization must not overwrite FVN security data.
5. Role/function changes are security-sensitive and must invalidate stale sessions where appropriate.
6. Data scope is separate from action permission.

## Phase 2 — Role → Function/Action Matrix

Security Center now exposes the normalized RBAC matrix through:

- `GET /api/security/roles`
- `GET /api/security/functions`
- `PUT /api/security/roles/{roleCode}/functions`
- `PUT /api/security/users/{userId}/roles`
- `GET /api/security/users/{userId}/permissions`

The Security Center UI has two operational views:

1. **Users & Effective Permission** — assign multiple roles to a user and inspect the effective capability set.
2. **Role → Function / Action** — edit the role/function matrix directly. Changes are stored in `F03RoleFunctions` and therefore affect every user carrying that role.

The server remains authoritative: the UI is only an administration surface. Role changes revoke the affected user's sessions.

## Phase 2 — API capability enforcement

The following API surfaces now resolve effective permissions from the database before invoking the business service:

| API surface | View | Create | Edit | Cancel | Approve | Other |
|---|---|---|---|---|---|---|
| Leave | `Leave.View` | `Leave.Create` | — | `Leave.Cancel` | `Leave.Approve` | pending = Approve |
| OT | `OT.View` | `OT.Create` | `OT.Edit` | `OT.Cancel` | `OT.Approve` | reconciliation/export reserved |
| Trip | `Trip.View` | `Trip.Create` | `Trip.Edit` | — | reserved | — |
| Equipment | `Equipment.View` | `Equipment.Create` | `Equipment.Edit` | — | reserved | `Equipment.Repair` |
| HRM Sync | `HrmSync.ViewStatus` | — | — | — | — | run = `HrmSync.Sync`, review = `HrmSync.Review` |

`[Authorize]` remains the authentication boundary. Capability checks are the application authorization boundary. A valid JWT alone does not grant module access.

### Important

`ScopeCode` is still metadata for the next authorization phase. For example, `Leave.View=Own` and `Leave.Approve=Department` describe intended data scope, but query-level scope filtering must be implemented separately and must not be inferred from the action permission alone.

## Phase 3 — Capability + Data Scope

Phase 3 makes `ScopeCode` executable authorization metadata.

The effective authorization decision is:

```text
User
  -> Function/Action capability
  -> Effective Scope
       -> Own
       -> Employee
       -> Department
       -> All
  -> query filter
```

### Scope semantics

| ScopeCode | Meaning | Request data example |
|---|---|---|
| `Own` | only records belonging to the authenticated employee | `EmployeeCode == current.EmployeeCode` |
| `Employee` | explicitly selected employee scope; currently equivalent to Own for request data | selected/current employee |
| `Department` | records belonging to the authenticated employee's department | `DeptCode == current.DeptCode` |
| `All` | no department/employee restriction | unrestricted, subject to business filters |

Scope is **not** inferred from role names or `IsAdmin`. An administrator only receives the scope granted by the effective function mapping.

### Enforcement boundary

`IAuthorizationService.GetScopeAsync` resolves the broadest effective scope across all active roles and legacy direct grants:

```text
All > Department > Employee > Own > None
```

`IAuthorizationService.CanAccessAsync` evaluates a target employee/department against that scope.

Phase 3 applies query filtering to Leave, OT, Trip and Equipment data paths. A caller with `Own` cannot broaden a query by supplying another department code; a caller with `Department` is constrained to their own department; only `All` can intentionally use an arbitrary department filter.

### Important security rule

Action permission and data scope are independent:

- `Approve` does not imply `All`.
- `View=Department` does not grant `Edit=Department`.
- Each function/action has its own `ScopeCode`.
- Client-side filtering is never considered authorization.

### Migration

Legacy `F03UserFunctions` remain effective. When an old direct grant has no scope metadata, Phase 3 treats it as `Own` rather than expanding access.

The existing HRM → FVN master synchronization boundary is unchanged: HRM remains read-only source-of-truth for HR master data; FVN remains owner of authorization and security state.


## Equipment — user-specific capability

Equipment uses the same authorization pipeline but keeps module and import permission separate:

| FunctionCode | Capability | Purpose |
|---|---|---|
| 2301 | Equipment.View | module, QR and lookup |
| 2302 | Equipment.Create | create registration |
| 2303 | Equipment.Edit | edit/submit request |
| 2304 | Equipment.Repair | repair request |
| 2305 | Equipment.Approve | approval |
| 2306 | Equipment.Import | Excel import |
| 2307 | Equipment.Export | export |
| 2308 | Equipment.Cancel | cancel request |

Admin can grant these capabilities directly to an individual user from **User Management -> Edit permission**. The UI exposes two separate Equipment controls: **Sổ quản lý thiết bị** (2301) and **Import Excel thiết bị** (2306).

Therefore a user can use Equipment without being allowed to import Excel. The import API enforces 2306 and department scope server-side; hiding the button is only UX.

900 / EquipmentModule remains a legacy marker and is not an action-capability bypass.
