// File: FVN_REGISTER.Infrastructure/Services/Employees/EmployeeManagementService.cs

using FVN_REGISTER.Application.Interfaces.Employees;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Employees;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Employees
{
    public class EmployeeManagementService
        : BaseService<EmployeeManagementService>, IEmployeeManagementService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IUnitOfWork _uow;

        public EmployeeManagementService(
            FVNWEBAPPContext db,
            IUnitOfWork uow,
            ILogger<EmployeeManagementService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _uow = uow;
        }

        // The remainder of this service is intentionally unchanged; this edit only restores
        // the Infrastructure data-context import after the namespace boundary refactor.
