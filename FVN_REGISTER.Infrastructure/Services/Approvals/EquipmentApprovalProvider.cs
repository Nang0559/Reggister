using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace FVN_REGISTER.Infrastructure.Services.Approvals;
public sealed class EquipmentApprovalProvider : BaseApprovalProvider<EquipmentRequestSubject, EquipmentApprovalProvider>, IApprovalProvider<EquipmentRequestSubject>