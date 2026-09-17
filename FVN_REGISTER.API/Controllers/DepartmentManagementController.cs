using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/departments")]
    public class DepartmentManagementController : BaseApiController
    {
        private readonly IDepartmentManagementService _departmentService;

        public DepartmentManagementController(
            IDepartmentManagementService departmentService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<DepartmentManagementController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _departmentService = departmentService;
        }

        // GET api/admin/departments/tree
        // The current application contract exposes a flat master-data DTO.
        // Keep the route for compatibility until a dedicated hierarchy DTO is introduced.
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(CancellationToken ct)
        {
            var result = await _departmentService.GetAllAsync(ct);
            await LogActionAsync("Xem cây quản lý Bộ phận");
            return HandleResult(result);
        }

        // GET api/admin/departments?isActive=true
        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] bool? isActive,
            CancellationToken ct)
        {
            var result = await _departmentService.GetFilteredAsync(isActive, ct);
            return HandleResult(result);
        }

        // GET api/admin/departments/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _departmentService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/departments
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] DepartmentUpsertDto model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _departmentService.CreateAsync(
                model, UserInfo.UserId, ct);

            await LogActionAsync($"Thêm bộ phận: {model.DeptCode}");
            return HandleResult(result);
        }

        // PUT api/admin/departments/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] DepartmentUpsertDto model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            model.Id = id;
            var result = await _departmentService.UpdateAsync(
                model, UserInfo.UserId, ct);

            await LogActionAsync($"Cập nhật bộ phận: {model.DeptCode}");
            return HandleResult(result);
        }

        // PATCH api/admin/departments/5/toggle
        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _departmentService.ToggleActiveAsync(
                id, UserInfo.UserId, ct);

            await LogActionAsync($"Đổi trạng thái bộ phận ID: {id}");
            return HandleResult(result);
        }

        // DELETE api/admin/departments/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _departmentService.DeleteAsync(id, ct);
            await LogActionAsync($"Xóa bộ phận ID: {id}");
            return HandleResult(result);
        }
    }
}
