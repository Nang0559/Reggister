

namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows
{
    
        /// <summary>DTO thô 1-1 với cột nguồn HRM.[dbo].[tblChucVu].</summary>
        public class HrmPositionSourceRow
        {
            public string PositionCode { get; set; } = string.Empty;   // CVMa
            public string PositionName { get; set; } = string.Empty;   // CVTen
            
        }
    
}
