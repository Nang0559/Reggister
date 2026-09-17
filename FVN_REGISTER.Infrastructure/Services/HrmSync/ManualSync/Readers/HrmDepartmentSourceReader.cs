using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
using Microsoft.Extensions.Configuration;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Readers
{
    public class HrmDepartmentSourceReader : SqlHrmSourceReaderBase<HrmDepartmentSourceRow>
    {
        public HrmDepartmentSourceReader(IConfiguration configuration) : base(configuration) { }

        protected override string Sql => @"
        SELECT
            DeptCode = BP.BPMa,
            DeptName = BP.BPTen
        FROM [HRM].[dbo].[tblBoPhan] BP
        WHERE ISNULL(BP.DLocked, 0) = 0";
    }
}
