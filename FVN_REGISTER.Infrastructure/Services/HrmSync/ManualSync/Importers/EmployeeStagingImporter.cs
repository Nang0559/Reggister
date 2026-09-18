using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Services.HrmSync.ManualSync.BaseManuals;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
using Microsoft.Extensions.Logging;

using System.Linq.Expressions;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Importers
{
    public class EmployeeStagingImporter
     : HrmStagingImporterBase<HrmEmployeeSourceRow, F03StagingEmployee, F03Employee>
    {
        public override string EntityType => "Employee";

        public EmployeeStagingImporter(
            IHrmSourceReader<HrmEmployeeSourceRow> reader,
            IUnitOfWork uow,
            ILogger<EmployeeStagingImporter> logger)
            : base(reader, uow, logger)
        {
        }

        protected override string GetSourceKey(HrmEmployeeSourceRow row) => row.EmployeeCode;

        protected override Expression<Func<F03Employee, string>> GetEntityKeySelector()
            => x => x.EmployeeCode;

        protected override F03StagingEmployee MapToStaging(HrmEmployeeSourceRow row) => new()
        {
            EntityKey = row.EmployeeCode,
            EmployeeName = row.EmployeeName,
            DeptCode = row.DeptCode,
            PositionCode = row.PositionCode,
            EmailAddress = row.EmailAddress ?? "",
            PhoneNumber = row.PhoneNumber,
            BirthDate = row.BirthDate,
            GenderCode = row.GenderCode,
            FirstWorkingDate = row.FirstWorkingDate,
            EndWorkingDate = row.EndWorkingDate,
            TotalLeaveDays = row.TotalLeaveDays,
            EmployeeNo = row.EmployeeNo
            // LevelApprove KHÔNG map từ HRM — cấu hình nghiệp vụ thuần của FVN (ai được
            // duyệt cấp mấy). EmployeeHrmSyncJob.ApplyUpdate PHẢI bỏ qua field này khi
            // ghi đè, tương tự cách PositionHrmSyncJob bỏ qua IsApprove/IsAllowApprove.
        };

        protected override F03StagingEmployee BuildDeleteStaging(string entityKey) => new()
        {
            EntityKey = entityKey
        };
    }
}
