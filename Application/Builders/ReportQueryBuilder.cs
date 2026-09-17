


using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Builders
{
    public class ReportQueryBuilder
    {
        private readonly ReportQueryDto _query = new();

        public ReportQueryBuilder(ReportType type)
        {
            _query.Type = type;
            // Thiết lập giá trị mặc định cho tất cả báo cáo
            _query.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _query.ToDate = DateTime.Now;
        }

        public ReportQueryBuilder ForDept(string deptCode)
        {
            _query.DeptCode = deptCode;
            return this;
        }

        public ReportQueryBuilder ForEmployee(string empCode)
        {
            _query.EmployeeCode = empCode;
            return this;
        }

        public ReportQueryBuilder ForYear(int year)
        {
            _query.WorkYear = year;
            return this;
        }

        public ReportQueryBuilder WithPagination(int page, int size)
        {
            _query.PageNumber = page;
            _query.PageSize = size;
            return this;
        }

        public ReportQueryBuilder EnableExport(string format)
        {
            _query.ExportFormat = format;
            return this;
        }

        public ReportQueryDto Build() => _query;
    }
}
