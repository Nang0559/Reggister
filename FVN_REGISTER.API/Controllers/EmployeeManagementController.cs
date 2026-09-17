using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Employees;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    // FVN_REGISTER.API/Controllers/EmployeeManagementController.cs
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

        // GET api/employeemanagement/tree?searchTerm=xx&deptCode=yy
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(
            [FromQuery] string? searchTerm,
            [FromQuery] string? deptCode,
            CancellationToken ct)
        {
            var result = await _service.GetTreeAsync(searchTerm, deptCode, ct);
            return HandleResult(result);
        }

        // GET api/employeemanagement/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // GET api/employeemanagement/cv-list
        [HttpGet("cv-list")]
        public async Task<IActionResult> GetCvList(CancellationToken ct)
        {
            var result = await _service.GetCvListAsync(ct);
            return HandleResult(result);
        }

        // GET api/employeemanagement/ot-summary/{employeeCode}?year=2025
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

        // POST api/employeemanagement
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

        // PUT api/employeemanagement/{id}
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

        // PATCH api/employeemanagement/{id}/toggle
        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _service.ToggleActiveAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        // DELETE api/employeemanagement/{id}
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
