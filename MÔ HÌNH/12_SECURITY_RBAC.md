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
