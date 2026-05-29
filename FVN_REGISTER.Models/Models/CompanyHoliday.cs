using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class CompanyHoliday
{
    public int Id { get; set; }

    public DateTime? HolidayDate { get; set; }

    public string? Description { get; set; }

    public int? Year { get; set; }

    public int? TinhPhep { get; set; }
}
