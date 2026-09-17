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
    public class DepartmentStagingImporter
    : HrmStagingImporterBase<HrmDepartmentSourceRow, F03StagingDepartment, F03Department>
    {
        public override string EntityType => "Department";

        public DepartmentStagingImporter(
            IHrmSourceReader<HrmDepartmentSourceRow> reader,
            IUnitOfWork uow,
            ILogger<DepartmentStagingImporter> logger)
            : base(reader, uow, logger)
        {
        }

        protected override string GetSourceKey(HrmDepartmentSourceRow row)
            => row.DeptCode;

        protected override Expression<Func<F03Department, string>> GetEntityKeySelector()
            => x => x.DeptCode;

        protected override F03StagingDepartment MapToStaging(HrmDepartmentSourceRow row) => new()
        {
            EntityKey = row.DeptCode,
            DeptName = row.DeptName
        };

        protected override F03StagingDepartment BuildDeleteStaging(string entityKey) => new()
        {
            EntityKey = entityKey
        };
    }
}
