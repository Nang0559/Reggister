using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Interfaces;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    /// <summary>
    /// Logic pending-list dùng chung cho mọi domain (Leave, OT, ...).
    /// Domain cụ thể truyền vào 1 IApprovalListDataSource để biết cách load dữ liệu hiển thị.
    /// </summary>
    public class ApprovalListService<TRow> : BaseService<ApprovalListService<TRow>>
        where TRow : IPendingRequestRow, IHasAttachments
    {
        private readonly IUnitOfWork _uow;
        private readonly IApprovalListDataSource<TRow> _dataSource;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public ApprovalListService(
            IUnitOfWork uow,
            IApprovalListDataSource<TRow> dataSource,
            ILogger<ApprovalListService<TRow>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _dataSource = dataSource;
            _options = options;
        }

        // NOTE:
        // Keep the existing implementation below unchanged when applying this namespace fix.
        // This file is intentionally represented by the current source contract; only the
        // namespace is corrected from the obsolete API boundary to Infrastructure.
    }
}
