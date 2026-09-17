using FVN_REGISTER.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Extensions
{
    // Enums/ValidationStatusExtensions.cs
    public static class ValidationStatusExtensions
    {
        public static RowStatus ToRowStatus(this ValidationStatus status) => status switch
        {
            ValidationStatus.Valid => RowStatus.Success,
            ValidationStatus.Warning => RowStatus.Warning,
            ValidationStatus.Invalid => RowStatus.Error,
            _ => RowStatus.Default
        };
    }
}
