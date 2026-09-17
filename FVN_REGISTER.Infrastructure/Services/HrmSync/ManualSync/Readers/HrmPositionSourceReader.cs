namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Readers
{
    using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals;
    using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
    using Microsoft.Extensions.Configuration;

    public class HrmPositionSourceReader : SqlHrmSourceReaderBase<HrmPositionSourceRow>
    {
        public HrmPositionSourceReader(IConfiguration configuration) : base(configuration) { }

        protected override string Sql => @"
            SELECT
                PositionCode = CV.CVMa,
                PositionName = CV.CVTen
               
            FROM [HRM].[dbo].[tblChucVu] CV
            WHERE ISNULL(CV.DLocked, 0) = 0";
    }
}