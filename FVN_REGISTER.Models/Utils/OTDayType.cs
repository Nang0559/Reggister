using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class OTDayType
    {
        public const string Normal = "Normal";   // Ngày thường → max 4h
        public const string Weekend = "Weekend";  // Cuối tuần → max 12h + bắt buộc GM
        public const string Holiday = "Holiday";  // Ngày lễ → max 12h + bắt buộc GM
        public const string Tet = "Tet";      // Tết → max 12h + bắt buộc GM

        public static decimal MaxHours(string dayType) => dayType switch
        {
            Normal => 4m,
            Weekend => 12m,
            Holiday => 12m,
            Tet => 12m,
            _ => 4m
        };

        public static bool RequiresGM(string dayType) =>
            dayType is Weekend or Holiday or Tet;
    }
}
