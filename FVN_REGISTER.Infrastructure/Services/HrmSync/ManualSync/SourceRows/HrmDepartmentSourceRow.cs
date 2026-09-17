using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows
{
    /// <summary>
    /// Dòng dữ liệu thô đọc từ nguồn HRM cho Department (Pipeline B — poll theo lịch).
    /// Được IHrmSourceReader&lt;HrmDepartmentSourceRow&gt; trả về, sau đó
    /// DepartmentStagingImporter.MapToStaging ánh xạ sang F03StagingDepartment.
    /// </summary>
    public class HrmDepartmentSourceRow
    {
        public string DeptCode { get; set; } = null!;
        public string DeptName { get; set; } = null!;
    }
}
