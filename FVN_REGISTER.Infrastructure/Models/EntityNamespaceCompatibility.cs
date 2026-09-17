// Infrastructure-wide imports for canonical domain and application namespaces.
// No compatibility type aliases are used here.

global using FVN_REGISTER.Infrastructure.Models.Data;

global using FVN_REGISTER.Core.Entities.Common;
global using FVN_REGISTER.Core.Entities.Approvers;
global using FVN_REGISTER.Core.Entities.HR;
global using FVN_REGISTER.Core.Entities.Leaves;
global using FVN_REGISTER.Core.Entities.OT;
global using FVN_REGISTER.Core.Entities.Security;
global using FVN_REGISTER.Core.Entities.Trips;

global using FVN_REGISTER.Application.Models.Subjects;
global using FVN_REGISTER.Application.Interfaces.Approvals;
global using FVN_REGISTER.Application.Interfaces.Histories;

global using Microsoft.Extensions.DependencyInjection;
