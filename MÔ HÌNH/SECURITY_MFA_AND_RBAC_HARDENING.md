# Security Hardening — RBAC + Explicit Per-User MFA

## Scope

2FA policy is explicitly assigned per account by SuperAdmin. The system does not infer MFA requirements from sensitive RBAC capabilities.

- `F03Users.TwoFactorRequired`: SuperAdmin policy.
- `F03Users.TwoFactorEnabled`: user enrollment state.
- Function `2407 UserManagement.ManageTwoFactor` is granted only to SuperAdmin (`RoleCode=1`).

## MFA flow

1. User submits username/password.
2. If `TwoFactorRequired=1`, the API does not issue an access token yet.
3. API creates a short-lived, single-use 2FA challenge.
4. If TOTP is not enrolled, the user receives a locally generated QR/otpauth URI and must confirm a valid TOTP code.
5. If TOTP is already enrolled, the user submits the 6-digit TOTP code.
6. Only after successful verification does the API issue a new JWT + refresh-token session.
7. Failed challenges are limited to five attempts and expire after ten minutes.
8. TOTP secrets are encrypted with ASP.NET Core Data Protection; challenge values are stored only as SHA-256 hashes.
9. Turning the policy off revokes target sessions but keeps the enrolled secret for future re-enablement.
10. Administrator reset clears the enrolled factor, revokes target sessions, and preserves `TwoFactorRequired`, so the next login requires re-enrollment.

## Administration

- `GET /api/security/users/2fa` — SuperAdmin-only list of account 2FA policy/enrollment status.
- `PUT /api/security/users/{userId}/2fa-required` — SuperAdmin-only enable/disable policy.
- `POST /api/security/users/{userId}/2fa/reset` — SuperAdmin-only factor reset; cannot target the current administrator.

## RBAC cleanup

- Dashboard now enforces Dashboard.View.
- Report Excel export requires the module-specific Export capability instead of View.
- Email template toggle requires EmailTemplate.Manage.
- Attendance employee feedback is a separate Attendance.Feedback capability.
- Unimplemented legacy capabilities are inactive until their corresponding business actions exist: OT.Reconcile, Equipment Cancel/Assign/Transfer/Return/Liquidate, and Security.ManageRoles.

## Operational note

ASP.NET Core Data Protection keys must be persisted/shared in production when multiple API instances are used. Otherwise a TOTP secret encrypted by one instance may not be decryptable by another instance after key-ring divergence.


## Chính sách 2FA explicit theo tài khoản

2FA không còn được hệ thống tự suy luận từ RBAC/capability nhạy cảm. SuperAdmin là người quyết định tài khoản nào phải dùng 2FA thông qua capability UserManagement.ManageTwoFactor (2407).

- F03Users.TwoFactorRequired: chính sách bắt buộc 2FA do SuperAdmin bật/tắt.
- F03Users.TwoFactorEnabled: trạng thái người dùng đã hoàn tất đăng ký Authenticator.
- Khi TwoFactorRequired=1 nhưng TwoFactorEnabled=0, lần đăng nhập kế tiếp dừng sau mật khẩu và yêu cầu đăng ký Authenticator.
- Khi SuperAdmin tắt yêu cầu 2FA, phiên đang hoạt động của tài khoản bị thu hồi; secret đã đăng ký không bị xóa để có thể sử dụng lại nếu chính sách được bật lại.
- Reset 2FA xóa secret/enrollment nhưng giữ nguyên TwoFactorRequired; người dùng sẽ phải đăng ký lại ở lần đăng nhập tiếp theo.
- Function 2407 chỉ được seed cho SuperAdmin (RoleCode=1).
- QR TOTP được tạo nội bộ bằng QRCoder; secret/URI không gửi qua dịch vụ QR bên thứ ba.

