using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Companies;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels.Departments;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/departments")]
    public class DepartmentManagementController : BaseApiController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentManagementController(
            IDepartmentService departmentService,
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
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(CancellationToken ct)
        {
            var data = await _departmentService.GetTreeAsync(ct);
            await LogActionAsync("Xem cây quản lý Bộ phận");
            return Ok(ApiResponse<DepartmentTreeViewModel>.Ok(data));
        }

        // GET api/admin/departments?isActive=true
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] bool? isActive, CancellationToken ct)
        {
            var data = await _departmentService.GetListAsync(isActive, ct);
            return Ok(ApiResponse<List<DepartmentItemViewModel>>.Ok(data));
        }

        // GET api/admin/departments/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var data = await _departmentService.GetByIdAsync(id, ct);
            if (data == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bộ phận."));

            return Ok(ApiResponse<DepartmentEditViewModel>.Ok(data));
        }

        // POST api/admin/departments
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] DepartmentEditViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var result = await _departmentService.CreateAsync(model, UserInfo?.UserId ?? -1, ct);

            await LogActionAsync($"Thêm bộ phận: {model.DeptCode}");

            return HandleResult(result);
        }

        // PUT api/admin/departments
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] DepartmentEditViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var result = await _departmentService.UpdateAsync(model, UserInfo?.UserId ?? -1, ct);

            await LogActionAsync($"Cập nhật bộ phận: {model.DeptCode}");

            return HandleResult(result);
        }

        // PATCH api/admin/departments/5/toggle
        [HttpPatch("{id:int}/toggle")]
        public async Task<ActionResult> Toggle(int id, CancellationToken ct)
        {
            var result = await _departmentService.ToggleAsync(id, UserInfo?.UserId ?? -1, ct);

            await LogActionAsync($"Đổi trạng thái bộ phận ID: {id}");

            return HandleResult(result);
        }

        // DELETE api/admin/departments/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _departmentService.DeleteAsync(id, ct);

            await LogActionAsync($"Xóa bộ phận ID: {id}");

            return HandleResult(result);
        }
    
    }
}
