using FVN_REGISTER.Application.Interfaces.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FVN_REGISTER.API.Controllers;
[ApiController,Authorize,Route("api/payroll")]
public sealed class PayrollController : ControllerBase
{
 private readonly IPayrollInputService _service;
 public PayrollController(IPayrollInputService service)=>_service=service;
 [HttpPost("periods/{periodId:int}/prepare")]
 public async Task<IActionResult> Prepare(int periodId,CancellationToken ct)=>Ok(new { periodId,inputRows=await _service.PrepareAsync(periodId,ct) });
 [HttpPost("periods/{periodId:int}/lock")]
 public async Task<IActionResult> Lock(int periodId,CancellationToken ct){await _service.LockAsync(periodId,ct);return Ok(new {periodId,status="Locked"});}
}
