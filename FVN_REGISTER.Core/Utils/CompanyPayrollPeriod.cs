namespace FVN_REGISTER.Core.Utils;

public readonly record struct CompanyPayrollPeriod(DateTime From, DateTime To)
{
    public int DayCount => (To.Date - From.Date).Days + 1;

    public string Display => $"{From:dd/MM/yyyy} → {To:dd/MM/yyyy}";

    public static CompanyPayrollPeriod For(DateTime anchor)
    {
        var date = anchor.Date;

        if (date.Day >= 21)
        {
            var from = new DateTime(date.Year, date.Month, 21);
            var to = from.AddMonths(1).AddDays(-1);
            return new CompanyPayrollPeriod(from, to);
        }

        var toDate = new DateTime(date.Year, date.Month, 20);
        var fromDate = toDate.AddMonths(-1).AddDays(1);
        return new CompanyPayrollPeriod(fromDate, toDate);
    }
}
