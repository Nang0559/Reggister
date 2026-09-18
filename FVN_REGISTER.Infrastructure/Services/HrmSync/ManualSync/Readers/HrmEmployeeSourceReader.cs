namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Readers
{
    using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals;
    using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
    using Microsoft.Extensions.Configuration;

    public class HrmEmployeeSourceReader : SqlHrmSourceReaderBase<HrmEmployeeSourceRow>
        {
            public HrmEmployeeSourceReader(IConfiguration configuration) : base(configuration) { }

            // HRM dùng ngày sentinel '9990-12-31' để biểu thị "chưa nghỉ việc" thay vì NULL.
            // Chuẩn hóa về NULL ngay tại nguồn để tầng trên (Importer/SyncJob) chỉ cần
            // kiểm tra HasValue, không phải nhớ magic date rải rác nhiều nơi.
            protected override string Sql => @"EXEC dbo.usp_SyncHrmEmployeeSource;";
        }
}

