# FVN_REGISTER — Hướng dẫn cấu hình môi trường

## 1. Nguyên tắc

Không commit JWT secret, SQL password hoặc Production connection string vào Git.
Development dùng .NET User Secrets. Production dùng Environment Variables hoặc Secret Manager.

## 2. Máy Development mới

Chạy từ thư mục repository:

    .\scripts\Setup-DevelopmentSecrets.ps1

Kiểm tra:

    dotnet user-secrets list --project .\FVN_REGISTER.API\FVN_REGISTER.API.csproj

Phải có:

    ConnectionStrings:DefaultConnection = ...
    Jwt:Audience = FVNRGTUI
    Jwt:Issuer = FVNRGTApi
    Jwt:SecretKey = ...

Nếu cấu hình thủ công:

    dotnet user-secrets set "Jwt:SecretKey" "<random-secret-at-least-32-bytes>" --project .\FVN_REGISTER.API\FVN_REGISTER.API.csproj
    dotnet user-secrets set "Jwt:Issuer" "FVNRGTApi" --project .\FVN_REGISTER.API\FVN_REGISTER.API.csproj
    dotnet user-secrets set "Jwt:Audience" "FVNRGTUI" --project .\FVN_REGISTER.API\FVN_REGISTER.API.csproj
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=<sql-host>,<port>;Initial Catalog=FVN_REGISTER;User ID=<user>;Password=<password>;TrustServerCertificate=True;MultipleActiveResultSets=True" --project .\FVN_REGISTER.API\FVN_REGISTER.API.csproj

Khuyến nghị dùng IP/hostname + port, ví dụ `Data Source=192.168.200.10,1433`, khi biết port cố định. Cách này không phụ thuộc SQL Server Browser để phân giải named instance.

Kiểm tra TCP:

    Test-NetConnection 192.168.200.10 -Port 1433

`TcpTestSucceeded : True` chỉ xác nhận TCP tới port, không xác nhận database/user/password.

## 3. Đổi sang máy Development khác

User Secrets nằm trên từng máy và không nằm trong Git. Khi đổi máy:

1. Clone repository.
2. Cài .NET SDK đúng version.
3. Chạy `scripts\Setup-DevelopmentSecrets.ps1`.
4. Nhập connection string của SQL Server mà máy mới được phép truy cập.
5. Kiểm tra bằng `dotnet user-secrets list`.
6. Build và chạy API.

Không copy secret vào appsettings.json và không commit file chứa secret.

## 4. EF Core / Migration

Design-time factory đọc appsettings.json, appsettings.Development.json, Environment Variables và User Secrets của FVN_REGISTER.API.

    dotnet ef dbcontext info --project .\FVN_REGISTER.Infrastructure --startup-project .\FVN_REGISTER.API
    dotnet ef migrations list --project .\FVN_REGISTER.Infrastructure --startup-project .\FVN_REGISTER.API

## 5. Production / Public Server

Không dùng User Secrets làm cơ chế triển khai Production.

Cấu hình:

    ConnectionStrings__DefaultConnection
    Jwt__SecretKey
    Jwt__Issuer
    Jwt__Audience

Ví dụ:

    ConnectionStrings__DefaultConnection=Data Source=<server>,<port>;Initial Catalog=FVN_REGISTER;User ID=<user>;Password=<password>;TrustServerCertificate=True;MultipleActiveResultSets=True
    Jwt__SecretKey=<production-random-secret-at-least-32-bytes>
    Jwt__Issuer=FVNRGTApi
    Jwt__Audience=FVNRGTUI

Trên Windows Server/IIS có thể cấu hình Environment Variables ở mức Machine hoặc IIS. Sau khi thay đổi phải restart Application Pool/ứng dụng.

Trên Linux/container nên cấp secrets qua Secret Manager, Docker/Kubernetes Secrets hoặc CI/CD secret store; không ghi secret vào image/source code.

## 6. ConnectionString khi đổi SQL Server

Nếu SQL Server dùng port cố định, ưu tiên:

    Data Source=<IP-or-hostname>,<PORT>;

Ví dụ:

    Data Source=192.168.200.10,1433;

Nếu dùng named instance:

    Data Source=192.168.200.10\WEBAPPDB;

thì phải bảo đảm instance được phân giải từ máy chạy API, thường liên quan SQL Server Browser và network configuration.

## 7. JWT Secret

JWT secret phải:
- Không để trong Git.
- Không để trong appsettings.json Production.
- Khác nhau giữa Development và Production.
- Tối thiểu 32 bytes theo validation hiện tại.
- Được rotate nếu bị lộ.

Issuer mặc định: `FVNRGTApi`.
Audience mặc định: `FVNRGTUI`.

## 8. Checklist trước khi public

- [ ] ConnectionStrings__DefaultConnection đã được cấu hình.
- [ ] Máy API kết nối được đúng SQL Server port.
- [ ] Không có SQL password trong Git.
- [ ] Production có JWT secret riêng và đủ tối thiểu 32 bytes.
- [ ] Jwt__Issuer và Jwt__Audience đúng.
- [ ] Restart API/IIS sau khi thay đổi secrets.
- [ ] Database/migration được triển khai theo quy trình Production.

## 9. Xử lý lỗi

### ConnectionString property has not been initialized

Ứng dụng chưa nhận `ConnectionStrings:DefaultConnection`. Kiểm tra User Secrets hoặc `ConnectionStrings__DefaultConnection`.

### error: 26 - Error Locating Server/Instance Specified

Thường liên quan named instance, SQL Server Browser hoặc port. Nếu biết port cố định, dùng `Data Source=<IP>,<PORT>` và kiểm tra `Test-NetConnection`.

### Login failed for user

TCP đã tới SQL Server nhưng authentication hoặc permission không đúng. Kiểm tra username, password, authentication mode và quyền database.

### Cannot open database

SQL Server đã nhận kết nối nhưng database không tồn tại hoặc user không có quyền.

## 10. Nếu secret đã bị lộ

Không chỉ xóa secret khỏi Git. Phải thay/rotate JWT secret hoặc SQL password thực tế, cập nhật secret store và restart ứng dụng.