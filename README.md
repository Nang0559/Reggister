├── FVN_REGISTER
│   ├── FVN_REGISTER
│   │   ├── Components
│   │   │   └── _Imports.razor
│   │   ├── Properties
│   │   │   └── launchSettings.json
│   │   ├── Services
│   │   │   └── SecureStorageTokenService .cs
│   │   ├── wwwroot
│   │   │   ├── app.css
│   │   │   └── index.html
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── FVN_REGISTER.UI.csproj
│   │   ├── FVN_REGISTER.UI.csproj.user
│   │   ├── MainPage.xaml
│   │   ├── MainPage.xaml.cs
│   │   └── MauiProgram.cs
│   ├── FVN_REGISTER.Shared
│   │   ├── Components
│   │   │   └── DashboardOverview.razor
│   │   ├── Handlers
│   │   │   ├── AuthHeaderHandler.cs
│   │   │   ├── AuthorizedHttpClient.cs
│   │   │   └── IHttpClientWithAuth.cs
│   │   ├── Layout
│   │   │   ├── EmptyLayout.razor
│   │   │   ├── MainLayout.razor
│   │   │   ├── MainLayout.razor.css
│   │   │   ├── MainLayout2.razor
│   │   │   └── NavMenu.razor
│   │   ├── Pages
│   │   │   ├── ChangePassword.razor
│   │   │   ├── Home.razor
│   │   │   ├── Login.razor
│   │   │   ├── RedirectToLogin.razor
│   │   │   └── UserProfile.razor
│   │   ├── Services
│   │   │   ├── Dashboards
│   │   │   │   ├── DashboardClientService.cs
│   │   │   │   └── IDashboardClientService.cs
│   │   │   └── Users
│   │   │       ├── AuthClientService.cs
│   │   │       ├── CurrentUserClientService.cs
│   │   │       ├── IAuthClientService.cs
│   │   │       └── ICurrentUserClientService.cs
│   │   ├── Utils
│   │   │   ├── Helpers
│   │   │   │   ├── AppBase.cs
│   │   │   │   ├── AppComponentBase.cs
│   │   │   │   ├── AppLayoutBase.cs
│   │   │   │   └── JwtParser.cs
│   │   │   ├── CustomAuthStateProvider.cs
│   │   │   ├── ITokenStorage.cs
│   │   │   └── UserPermissionCodes.cs
│   │   ├── wwwroot
│   │   │   └── app.css
│   │   ├── _Imports.razor
│   │   ├── FVN_REGISTER.Shared.csproj
│   │   └── Routes.razor
│   └── FVN_REGISTER.Web
│       ├── Components
│       │   ├── Pages
│       │   │   └── Error.razor
│       │   ├── _Imports.razor
│       │   └── App.razor
│       ├── Properties
│       │   └── launchSettings.json
│       ├── Services
│       │   └── LocalStorageTokenService.cs
│       ├── Utils
│       ├── appsettings.Development.json
│       ├── appsettings.json
│       ├── FVN_REGISTER.Web.csproj
│       ├── FVN_REGISTER.Web.csproj.user
│       └── Program.cs
├── FVN_REGISTER.API
│   ├── Attributes
│   │   └── AppAuthorizeAttribute.cs
│   ├── Controllers
│   │   ├── AuthController.cs
│   │   ├── BaseApiController.cs
│   │   ├── DashboardController.cs
│   │   └── LeaveDaysController.cs
│   ├── Middlewares
│   │   └── JwtMiddleware.cs
│   ├── Properties
│   │   └── launchSettings.json
│   ├── Repositories
│   │   └── LeaveRepository.cs
│   ├── Services
│   │   ├── Auths
│   │   │   ├── AuditService.cs
│   │   │   └── AuthService.cs
│   │   ├── Companies
│   │   │   └── CompanyHolidayService.cs
│   │   ├── Emails
│   │   │   ├── EmailBackgroundWorker.cs
│   │   │   └── EmailService.cs
│   │   ├── Leaves
│   │   │   ├── EscalationRuleService.cs
│   │   │   ├── LeaveEscalationService.cs
│   │   │   ├── LeaveNotificationService.cs
│   │   │   ├── LeaveQueryService.cs
│   │   │   ├── LeaveService.cs
│   │   │   ├── LeaveValidator.cs
│   │   │   └── WorkingDayService.cs
│   │   ├── Statics
│   │   │   ├── DashboardService.cs
│   │   │   └── StatisticsService.cs
│   │   └── Users
│   │       ├── CurrentUserService.cs
│   │       ├── UserLogService.cs
│   │       └── UserService.cs
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── FVN_REGISTER.API.csproj
│   ├── FVN_REGISTER.API.csproj.user
│   ├── FVN_REGISTER.API.http
│   ├── Program.cs
│   └── WeatherForecast.cs
├── FVN_REGISTER.Core
│   ├── Config
│   │   └── AuthDebugOptions.cs
│   ├── Helper
│   ├── Logging
│   │   └── LoggerExtensions.cs
│   ├── Repositories
│   │   ├── IBaseRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Services
│   │   └── BaseService.cs
│   └── FVN_REGISTER.Core.csproj
├── FVN_REGISTER.Infrastructure
│   ├── Repositories
│   │   ├── BaseRepository.cs
│   │   └── UnitOfWork.cs
│   └── FVN_REGISTER.Infrastructure.csproj
├── FVN_REGISTER.Models
│   ├── Dtos
│   │   ├── AbsenceWarningDto.cs
│   │   ├── LeaveBalanceDto.cs
│   │   ├── LeaveDetailDto.cs
│   │   ├── PendingRequestDto.cs
│   │   ├── RecentLeaveRequestDto.cs
│   │   └── WidgetCounterDto.cs
│   ├── Interfaces
│   │   ├── Auths
│   │   │   ├── IAuditService.cs
│   │   │   └── IAuthService.cs
│   │   ├── Companies
│   │   │   └── ICompanyHolidayService.cs
│   │   ├── Emails
│   │   │   └── IEmailService.cs
│   │   ├── Infrastructure
│   │   │   └── IJwtService.cs
│   │   ├── Leaves
│   │   │   ├── IEscalationRuleService.cs
│   │   │   ├── ILeaveEscalationService.cs
│   │   │   ├── ILeaveNotificationService.cs
│   │   │   ├── ILeaveQueryService.cs
│   │   │   ├── ILeaveService.cs
│   │   │   ├── ILeaveValidator.cs
│   │   │   └── IWorkingDayService.cs
│   │   ├── Repositores
│   │   │   ├── ApiResponse.cs
│   │   │   ├── ILeaveRepository.cs
│   │   │   ├── PagedResponse.cs
│   │   │   └── QueryOptionsDto.cs
│   │   ├── Statics
│   │   │   ├── IDashboardService.cs
│   │   │   └── IStatisticsService.cs
│   │   └── Users
│   │       ├── ICurrentUserService.cs
│   │       ├── IUserLogService.cs
│   │       └── IUserService.cs
│   ├── Maps
│   │   ├── LeaveMapper.cs
│   │   └── MappingProfile.cs
│   ├── Models
│   │   ├── AuditLog.cs
│   │   ├── CompanyHoliday.cs
│   │   ├── DebugTable.cs
│   │   ├── EmailLog.cs
│   │   ├── EmailQueue.cs
│   │   ├── EmailTemplate.cs
│   │   ├── EmployeeAttendanceView.cs
│   │   ├── EscalationLog.cs
│   │   ├── EscalationRule.cs
│   │   ├── F03cv.cs
│   │   ├── F03department.cs
│   │   ├── F03emailProfile.cs
│   │   ├── F03employee.cs
│   │   ├── F03employeeTmp.cs
│   │   ├── F03function.cs
│   │   ├── F03gender.cs
│   │   ├── F03leaveDay.cs
│   │   ├── F03leaveDayDetail.cs
│   │   ├── F03leaveDaysApprover.cs
│   │   ├── F03leaveDaysAttachment.cs
│   │   ├── F03leaveType.cs
│   │   ├── F03levelApprove.cs
│   │   ├── F03permission.cs
│   │   ├── F03registerType.cs
│   │   ├── F03tongPhep.cs
│   │   ├── F03user.cs
│   │   ├── F03userFunction.cs
│   │   ├── F03userLog.cs
│   │   ├── F03workYear.cs
│   │   ├── FVNWEBAPPContext.cs
│   │   ├── TmpPhepton.cs
│   │   ├── VEmployeeApprover.cs
│   │   ├── VF03employee.cs
│   │   ├── VF03leaveDay.cs
│   │   ├── VF03leaveDayDetail.cs
│   │   ├── VF03leaveDays1.cs
│   │   ├── VF03leaveDaysApprover.cs
│   │   ├── VF03leaveDaysIntermediate.cs
│   │   ├── VF03leaveType.cs
│   │   ├── VF03phepTon.cs
│   │   ├── VF03phepTon1.cs
│   │   ├── VF03user.cs
│   │   ├── VF03users1.cs
│   │   ├── VwCurrentlyPresentEmployee.cs
│   │   └── VwShiftCheckInOut.cs
│   ├── Responses
│   │   └── DashboardResponse.cs
│   ├── Utils
│   │   ├── AuthConstants.cs
│   │   ├── EmailTemplateEngine.cs
│   │   ├── EncryptUtils.cs
│   │   ├── FileService.cs
│   │   ├── IFileService.cs
│   │   ├── INetworkService.cs
│   │   ├── IUtilityService.cs
│   │   ├── LeaveStatus.cs
│   │   ├── LeaveStatusHelper.cs
│   │   ├── NetworkService.cs
│   │   ├── PaginationResult.cs
│   │   ├── ServiceResult.cs
│   │   ├── StaticString.cs
│   │   ├── TimeRange.cs
│   │   ├── UserRole.cs
│   │   └── UtilityService.cs
│   ├── ViewModels
│   │   ├── ApprovalCounts.cs
│   │   ├── ApprovalLevelViewModel.cs
│   │   ├── ApproveRequest.cs
│   │   ├── ApproverViewModel.cs
│   │   ├── CancelRequestModel.cs
│   │   ├── ChangePasswordViewModel.cs
│   │   ├── CombinedHolidaysViewModel.cs
│   │   ├── CreateLeaveDetailModel.cs
│   │   ├── CreateLeaveRequestModel.cs
│   │   ├── CurrentUser.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── DepartmentViewModel.cs
│   │   ├── ExtendedProps.cs
│   │   ├── HolidayViewModel.cs
│   │   ├── LeaveBalanceViewModel.cs
│   │   ├── LeaveDaysViewModel.cs
│   │   ├── LeaveDetailsViewModel.cs
│   │   ├── LeaveDetailViewModel.cs
│   │   ├── LeaveFormViewModel.cs
│   │   ├── LeaveStatisticsViewModel.cs
│   │   ├── LeaveTypeViewModel.cs
│   │   ├── LoginViewModel.cs
│   │   ├── PendingApprovalGroup.cs
│   │   ├── PendingSummaryViewModel.cs
│   │   ├── UpdateProfileRequest.cs
│   │   ├── UserAccountViewModel.cs
│   │   ├── UserSessionDto.cs
│   │   └── WorkYearStatsViewModel.cs
│   ├── FVN_REGISTER.Contract.csproj
│   └── PaginationResult.cs
├── .gitignore
├── FVN_REGISTER.sln
└── FVN_REGISTER.slnLaunch.user

## Work Calendar

Leave, OT and Trip share one Work Calendar. It aggregates company holidays plus the current user's Leave/OT/Trip events without duplicating business state.

- API: `GET /api/calendar`
- Availability: `GET /api/calendar/availability`
- UI component: `WorkCalendar`
- Design: `MÔ HÌNH/18_WORK_CALENDAR.md`
- SQL: `SQL/19_WorkCalendar.sql`

Company holidays are informational calendar data; final Leave/OT/Trip authorization and validation remain in each module.