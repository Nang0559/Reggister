# FVN_REGISTER.Application — Design & Architecture

## 1. Vai trò

`FVN_REGISTER.Application` là lớp **Application** trong kiến trúc phân lớp/clean architecture của hệ thống Register.

Application chứa:

- use-case orchestration;
- application services;
- business policies/rules ở cấp use case;
- application ports (interfaces) để Infrastructure cung cấp implementation;
- application-specific models/subjects/context;
- mapping thuần C# giữa domain và contract khi cần;
- cross-cutting abstraction mà Application thực sự cần, ví dụ logging abstraction và options abstraction.

Application **không sở hữu persistence implementation**, HTTP endpoint, UI component hoặc EF Core.

## 2. Dependency rule

Dependency phải đi từ lớp bên ngoài vào abstraction bên trong:

```text
Shared / UI
     ↓
    API
     ↓
Infrastructure
     ↓
Application
     ↓
Contract / Models
     ↓
   Core
```

Project references của Application:

```text
Application
 ├── FVN_REGISTER.Core
 └── FVN_REGISTER.Models (Contract)
```

Application không được reference:

- `FVN_REGISTER.Infrastructure`
- `FVN_REGISTER.API`
- `FVN_REGISTER.Shared`
- `FVN_REGISTER.Web`
- `FVN_REGISTER.UI`
- EF Core / DbContext / Entity Framework implementation
- ASP.NET Controller/HttpContext/MudBlazor

Infrastructure được phép implement các interface/ports được định nghĩa tại Application.

## 3. Phân chia trách nhiệm

### Core

Domain entities, enums, constants, domain extensions và các abstraction thuần domain.

### Contract / Models

DTO, request, response, API result và các type dùng để giao tiếp giữa Application với adapter bên ngoài.

### Application

Application orchestration và use case. Ví dụ:

- `LeaveOrchestrator`
- `OTOrchestrator`
- `ApprovalWorkflowOrchestrator<TSubject>`
- `DashboardOrchestrator`

Các class này điều phối interface, không truy cập trực tiếp database.

### Infrastructure

Implementation của persistence và external adapters:

- EF Core/DbContext;
- repository;
- approval provider/engine implementation;
- email/notification transport;
- HRM synchronization;
- file/database integrations.

### API

HTTP boundary: authentication, authorization, controllers/endpoints, DI composition root và HTTP-specific response mapping.

### Shared / UI

Client-side service và presentation/UI.

## 4. Application interfaces

Các interface dưới `Application/Interfaces` là **ports** của Application.

Ví dụ:

- `Interfaces/Approvals` — approval workflow ports;
- `Interfaces/Leaves` — leave use-case ports;
- `Interfaces/OT` — OT use-case ports;
- `Interfaces/Histories` — history ports;
- `Interfaces/Users` — current-user/user resolution ports;
- `Interfaces/Infrastructure` — external technical ports như JWT/approval snapshot khi Application cần chúng.

Tên thư mục `Interfaces/Infrastructure` không có nghĩa Application phụ thuộc Infrastructure. Ngược lại, Infrastructure phải implement các port này.

## 5. Result và DTO boundary

Application dùng:

```csharp
FVN_REGISTER.Contract.Utils.ServiceResult
FVN_REGISTER.Contract.Utils.PaginationResult<T>
```

Không đưa các result contract này trở lại `Core`.

Không tạo namespace compatibility/alias để che dependency sai. Nếu một type chuyển layer, mọi consumer phải được sửa sang namespace canonical.

## 6. Logging và configuration

Application chỉ dùng abstraction:

- `ILogger<T>`;
- `IOptionsMonitor<AuthDebugOptions>`.

Implementation/configuration binding được thực hiện ở composition root (API/host).

`AuthDebugOptions` thuộc Application vì đây là option phục vụ behavior của application services; Core không biết về Microsoft.Extensions.

## 7. Orchestrator rule

Orchestrator được phép:

1. validate input qua application port;
2. gọi application service;
3. gọi query service;
4. gọi approval workflow;
5. compose nhiều kết quả thành một use-case result;
6. log và xử lý lỗi ở application boundary.

Orchestrator không được:

- tạo `DbContext`;
- gọi EF Core trực tiếp;
- biết table/entity configuration;
- tạo `HttpClient` trực tiếp;
- trả về EF entity cho API/UI;
- import namespace của Infrastructure/API/UI.

## 8. Mapping

Mapping nghiệp vụ quan trọng được giữ explicit trong Application/Maps hoặc service phù hợp.

Không sử dụng một mapping framework chỉ để phục vụ vài mapping đơn giản. Mapping profile AutoMapper legacy đã được loại bỏ để tránh thêm dependency không cần thiết.

## 9. Compatibility policy

Không sử dụng:

- `GlobalUsings.Compatibility.cs`;
- namespace alias để giữ code cũ;
- duplicate type ở nhiều layer;
- `NamespaceCompatibility.cs`.

Khi architecture thay đổi, sửa consumer về namespace canonical thay vì tạo cầu nối tạm thời.

## 10. Quy tắc kiểm tra trước khi chuyển sang Infrastructure

Application được coi là sạch khi thỏa tất cả:

- chỉ reference Core + Contract;
- không có `FVN_REGISTER.Infrastructure.*`;
- không có `FVN_REGISTER.API.*`;
- không có `FVN_REGISTER.Shared.*`, `FVN_REGISTER.Web.*`, `FVN_REGISTER.UI.*`;
- không có `Core.Configurations` hoặc `Core.Logging` legacy;
- result types đến từ Contract;
- không còn compatibility aliases;
- build Application thành công độc lập sau khi Core và Contract build thành công.

Sau checkpoint này mới chuyển sang quét Infrastructure.

## Shared Work Calendar
Work Calendar is a read-only projection shared by Leave, OT and Trip. The application does not persist a duplicated calendar state.Sources: F03CompanyHolidays, VF03LeaveRequest, VF03OTRequest and F03TripRequests.The calendar is not a business-rule authority. Each module remains responsible for authorization and final validation.