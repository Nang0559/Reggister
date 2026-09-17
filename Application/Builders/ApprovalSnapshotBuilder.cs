


using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Builders
{
    // Ví dụ về Builder cho Snapshot
    public sealed class ApprovalSnapshotBuilder
    {
        private readonly List<F03ApprovalStepSnapshot> _steps = new();

        public ApprovalSnapshotBuilder AddStep(int level, string code, string name, string role, bool required)
        {
            _steps.Add(new F03ApprovalStepSnapshot { Level = level, ApproverCode = code, ApproverName = name, RoleName = role, IsRequired = required });
            return this;
        }

        public F03ApprovalSnapshot Build(int requestId, RequestModule module) => new()
        {
            RequestId = requestId,
            RequestType = module,
            Steps = _steps.OrderBy(s => s.Level).ToList().AsReadOnly()
        };
    }
}
