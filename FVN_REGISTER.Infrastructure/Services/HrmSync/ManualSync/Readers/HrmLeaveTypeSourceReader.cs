namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Readers
{
    using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals;
    using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
    using Microsoft.Extensions.Configuration;

    public class HrmLeaveTypeSourceReader : SqlHrmSourceReaderBase<HrmLeaveTypeSourceRow>
    {
        public HrmLeaveTypeSourceReader(IConfiguration configuration) : base(configuration) { }

        protected override string Sql => @"EXEC dbo.usp_SyncHrmLeaveTypeSource;";
    }
}