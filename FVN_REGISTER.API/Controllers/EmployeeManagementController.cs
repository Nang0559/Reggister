using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Employees;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeManagementController : BaseApiController
    {
        private readonly IEmployeeManagementService _service;

        public EmployeeManagementController(
            IEmployeeManagementService service,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<EmployeeManagementController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _service = service;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(
            [FromQuery] string? searchTerm,
            [FromQuery] string? deptCode,
            CancellationToken ct)
        {
            var result = await _service.GetTreeAsync(searchTerm, deptCode, ct);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        [HttpGet("cv-list")]
        public async Task<IActionResult> GetCvList(CancellationToken ct)
        {
            var result = await _service.GetCvListAsync(ct);
            return HandleResult(result);
        }

        [HttpGet("ot-summary/{employeeCode}")]
        public async Task<IActionResult> GetOtSummary(
            string employeeCode,
            [FromQuery] int? year,
            CancellationToken ct)
        {
            var result = await _service.GetOtSummaryAsync(
                employeeCode, year ?? DateTime.Now.Year, ct);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] EmployeeFormViewModel model,
            CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _service.CreateAsync(model, UserInfo.UserId, ct);
            await LogActionAsync($"Thêm nhân viên: {model.EmployeeCode}");
            return HandleResult(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] EmployeeFormViewModel model,
            CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            model.Id = id;
            var result = await _service.UpdateAsync(model, UserInfo.UserId, ct);
            await LogActionAsync($"Sửa nhân viên: {model.EmployeeCode}");
            return HandleResult(result);
        }

        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _service.ToggleActiveAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _service.DeleteAsync(id, UserInfo.UserId, ct);
            await LogActionAsync($"Xóa nhân viên ID: {id}");
            return HandleResult(result);
        }
    }
}
