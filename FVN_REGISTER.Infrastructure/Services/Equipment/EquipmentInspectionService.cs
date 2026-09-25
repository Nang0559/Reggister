using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Models.Actions;
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using NPOI.SS.UserModel;
using FVN_REGISTER.Application.Interfaces.Users;

namespace FVN_REGISTER.Infrastructure.Services.Equipment;

public sealed class EquipmentInspectionService : IEquipmentInspectionService
{
    private readonly FVNWEBAPPContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;
    private readonly IEmailService _email;
    private readonly IActionItemWriter _actionWriter;
    private readonly IActionItemService _actionService;
    private readonly INotificationService _notification;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EquipmentInspectionService> _logger;

    public EquipmentInspectionService(
        FVNWEBAPPContext db,
        ICurrentUserService currentUser,
        IAuthorizationService authorization,
        IEmailService email,
        IActionItemWriter actionWriter,
        IActionItemService actionService,
        INotificationService notification,
        IWebHostEnvironment environment,
        ILogger<EquipmentInspectionService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _authorization = authorization;
        _email = email;
        _actionWriter = actionWriter;
        _actionService = actionService;
        _notification = notification;
        _environment = environment;
        _logger = logger;
    }

    public async Task<ServiceResult<List<EquipmentAssetDto>>> GetRegisteredAssetsAsync(string? deptCode, CancellationToken ct = default)
    {
        var user = RequireUser();
        var canManage = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct);
        var canReport = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionReport, ct);
        if (!canManage && !canReport)
            return ServiceResult<List<EquipmentAssetDto>>.Fail("Bạn không có quyền xem danh sách thiết bị phục vụ checklist.");

        var q = _db.EquipmentAssets.AsNoTracking().Where(x => x.IsActive == true);
        if (!string.IsNullOrWhiteSpace(deptCode)) q = q.Where(x => x.DeptCode == deptCode.Trim().ToUpperInvariant());
        var rows = await q.OrderBy(x => x.DeptCode).ThenBy(x => x.EquipmentCode).Take(2000).ToListAsync(ct);
        return ServiceResult<List<EquipmentAssetDto>>.Ok(rows.Select(x => new EquipmentAssetDto
        {
            Id=x.Id, EquipmentCode=x.EquipmentCode, EquipmentName=x.EquipmentName, Specification=x.Specification,
            SerialNumber=x.SerialNumber, AssetCode=x.AssetCode, PurchasePrice=x.PurchasePrice, PurchaseDate=x.PurchaseDate,
            ExpectedDepreciationDate=x.ExpectedDepreciationDate, DeptCode=x.DeptCode, Location=x.Location,
            QrToken=x.QrToken, IsQrActive=x.IsQrActive, Note=x.Note
        }).ToList());
    }

    public async Task<ServiceResult<List<EquipmentInspectionTemplateDto>>> GetTemplatesAsync(string? deptCode, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct) &&
            !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionReport, ct))
            return ServiceResult<List<EquipmentInspectionTemplateDto>>.Fail("Bạn không có quyền xem cấu hình checklist.");

        var query = _db.Set<F03EquipmentInspectionTemplate>().AsNoTracking().Where(x => x.IsActive != false);
        if (!string.IsNullOrWhiteSpace(deptCode)) query = query.Where(x => x.DeptCode == deptCode.Trim().ToUpperInvariant());

        var rows = await query.Include(x => x.Items).OrderBy(x => x.DeptCode).ThenBy(x => x.TemplateName).ThenByDescending(x => x.Version).ToListAsync(ct);
        return ServiceResult<List<EquipmentInspectionTemplateDto>>.Ok(rows.Select(MapTemplate).ToList());
    }

    public async Task<ServiceResult<EquipmentInspectionTemplateDto>> GetTemplateAsync(int id, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct) &&
            !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionExecute, ct))
            return ServiceResult<EquipmentInspectionTemplateDto>.Fail("Bạn không có quyền xem checklist.");

        var row = await _db.Set<F03EquipmentInspectionTemplate>().AsNoTracking().Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive != false, ct);
        return row == null
            ? ServiceResult<EquipmentInspectionTemplateDto>.Fail("Không tìm thấy checklist.")
            : ServiceResult<EquipmentInspectionTemplateDto>.Ok(MapTemplate(row));
    }

    public async Task<ServiceResult<EquipmentInspectionTemplateImportResultDto>> ImportTemplateExcelAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct))
            return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail("Bạn không có quyền import checklist.");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not ".xls" and not ".xlsx" and not ".xlsm")
            return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail("Checklist chỉ hỗ trợ Excel .xls, .xlsx hoặc .xlsm.");
        if (content == null || !content.CanRead)
            return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail("File Excel không hợp lệ.");

        await using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        ms.Position = 0;
        IWorkbook workbook;
        try { workbook = WorkbookFactory.Create(ms); }
        catch (Exception ex) { return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail($"Không đọc được Excel checklist: {ex.Message}"); }

        using (workbook)
        {
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null) return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail("Excel không có sheet.");
            var formatter = new DataFormatter();
            var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            var headerIndex = sheet.FirstRowNum;
            while (headerIndex <= sheet.LastRowNum && sheet.GetRow(headerIndex) == null) headerIndex++;
            if (headerIndex > sheet.LastRowNum) return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail("Excel không có dữ liệu.");
            var header = sheet.GetRow(headerIndex);
            var headers = Enumerable.Range(0, header.LastCellNum).Select(i => NormalizeInspectionExcelKey(GetCellText(header.GetCell(i), formatter, evaluator))).ToList();
            var requiredHeaders = new[] { "templatecode", "templatename", "deptcode", "frequency", "itemcode", "itemlabel", "inputtype", "displayorder" };
            var missing = requiredHeaders.Where(x => !headers.Contains(x)).ToList();
            if (missing.Count > 0) return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail($"Thiếu cột checklist bắt buộc: {string.Join(", ", missing)}.");

            var rows = new List<Dictionary<string,string?>>();
            for (var ri = headerIndex + 1; ri <= sheet.LastRowNum; ri++)
            {
                var row = sheet.GetRow(ri); if (row == null) continue;
                var data = new Dictionary<string,string?>(StringComparer.OrdinalIgnoreCase);
                for (var ci = 0; ci < headers.Count; ci++) data[headers[ci]] = GetCellText(row.GetCell(ci), formatter, evaluator).Trim();
                if (data.Values.All(string.IsNullOrWhiteSpace)) continue;
                rows.Add(data);
            }
            if (rows.Count == 0) return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail("Excel chưa có hạng mục checklist.");

            string V(Dictionary<string,string?> d, params string[] keys) => keys.Select(NormalizeInspectionExcelKey).Select(k => d.TryGetValue(k, out var v) ? v : null).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "";
            var templateCode = V(rows[0], "TemplateCode", "Mã checklist").Trim().ToUpperInvariant();
            var templateName = V(rows[0], "TemplateName", "Tên checklist");
            var deptCode = V(rows[0], "DeptCode", "Bộ phận", "DepartmentCode").Trim().ToUpperInvariant();
            var frequency = V(rows[0], "Frequency", "Chu kỳ").Trim();
            var description = V(rows[0], "Description", "Mô tả");
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(templateCode)) errors.Add("Thiếu TemplateCode.");
            if (string.IsNullOrWhiteSpace(templateName)) errors.Add("Thiếu TemplateName.");
            if (string.IsNullOrWhiteSpace(deptCode)) errors.Add("Thiếu DeptCode.");
            if (string.IsNullOrWhiteSpace(frequency) || !new[] {"Daily","Weekly","Monthly","Quarterly","Yearly"}.Contains(frequency,StringComparer.OrdinalIgnoreCase))
                errors.Add("Frequency không hợp lệ: Daily/Weekly/Monthly/Quarterly/Yearly.");
            if (string.IsNullOrWhiteSpace(deptCode) || !await _db.Employees.AsNoTracking().AnyAsync(e => e.DeptCode == deptCode && e.IsActive != false, ct))
                errors.Add($"Bộ phận '{deptCode}' không tồn tại hoặc không có nhân viên hoạt động.");
            var items = new List<EquipmentInspectionItemDto>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (data, index) in rows.Select((x,i)=>(x,i+2)))
            {
                var code=V(data,"ItemCode","Mã hạng mục").Trim().ToUpperInvariant();
                var label=V(data,"ItemLabel","Hạng mục","Nội dung");
                var input=V(data,"InputType","Kiểu").Trim();
                var orderText=V(data,"DisplayOrder","STT");
                if (string.IsNullOrWhiteSpace(code)) errors.Add($"Dòng {index}: thiếu ItemCode.");
                else if (!seen.Add(code)) errors.Add($"Dòng {index}: ItemCode '{code}' bị trùng.");
                if (string.IsNullOrWhiteSpace(label)) errors.Add($"Dòng {index}: thiếu ItemLabel.");
                if (!new[] {"PassFail","YesNo","Number","Text","Select"}.Contains(input,StringComparer.OrdinalIgnoreCase)) errors.Add($"Dòng {index}: InputType '{input}' không hợp lệ.");
                if (!int.TryParse(orderText,out var order)) errors.Add($"Dòng {index}: DisplayOrder phải là số.");
                var required=ParseInspectionBool(V(data,"IsRequired","Required","Bắt buộc"));
                var image=ParseInspectionBool(V(data,"RequireImage","Ảnh","Bắt buộc ảnh"));
                var minImages=int.TryParse(V(data,"MinImages","Ảnh tối thiểu"),out var mi)?mi:0;
                var maxImages=int.TryParse(V(data,"MaxImages","Ảnh tối đa"),out var ma)?ma:3;
                decimal? min=decimal.TryParse(V(data,"MinValue","Min"),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var mn)?mn:null;
                decimal? max=decimal.TryParse(V(data,"MaxValue","Max"),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var mx)?mx:null;
                if (image && minImages < 1) errors.Add($"Dòng {index}: RequireImage=true phải có MinImages >= 1.");
                if (input.Equals("Number",StringComparison.OrdinalIgnoreCase) && min.HasValue && max.HasValue && min > max) errors.Add($"Dòng {index}: MinValue không được lớn hơn MaxValue.");
                items.Add(new EquipmentInspectionItemDto { ItemCode=code, ItemLabel=label, InputType=input, IsRequired=required, RequireImage=image, MinImages=Math.Max(0,minImages), MaxImages=Math.Max(Math.Max(1,maxImages),minImages), MinValue=min, MaxValue=max, Unit=V(data,"Unit","Đơn vị"), OptionsJson=V(data,"OptionsJson","Options","Danh sách"), DisplayOrder=order });
            }
            if (errors.Count > 0) return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail(string.Join(" ", errors.Take(20)));

            var saved=await SaveTemplateAsync(new EquipmentInspectionTemplateUpsertRequest { TemplateCode=templateCode,TemplateName=templateName,DeptCode=deptCode,Frequency=frequency,Status="Draft",Description=description,Items=items },ct);
            if (!saved.IsSuccess || saved.Data == null) return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Fail(saved.Message ?? "Không thể lưu checklist từ Excel.");
            return ServiceResult<EquipmentInspectionTemplateImportResultDto>.Ok(new EquipmentInspectionTemplateImportResultDto { TemplateId=saved.Data.Id,TemplateCode=saved.Data.TemplateCode,TemplateName=saved.Data.TemplateName,ItemCount=items.Count }, "Đã kiểm tra và import checklist Excel thành công ở trạng thái Draft.");
        }
    }

    private static string NormalizeInspectionExcelKey(string value)
        => new string((value ?? "").Trim().Normalize(System.Text.NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).Select(char.ToLowerInvariant).Where(char.IsLetterOrDigit).ToArray());

    private static string GetCellText(ICell? cell, DataFormatter formatter, IFormulaEvaluator evaluator)
        => cell == null ? "" : formatter.FormatCellValue(cell, evaluator);

    private static bool ParseInspectionBool(string value)
        => value.Trim().ToLowerInvariant() is "true" or "1" or "yes" or "y" or "co" or "có" or "x";

    public async Task<ServiceResult<EquipmentInspectionTemplateDto>> SaveTemplateAsync(EquipmentInspectionTemplateUpsertRequest request, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct))
            return ServiceResult<EquipmentInspectionTemplateDto>.Fail("Bạn không có quyền quản lý checklist.");

        var dept = request.DeptCode.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(dept) || string.IsNullOrWhiteSpace(request.TemplateName))
            return ServiceResult<EquipmentInspectionTemplateDto>.Fail("DepartmentCode và tên checklist là bắt buộc.");

        var frequencies = new[] { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly" };
        if (!frequencies.Contains(request.Frequency, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionTemplateDto>.Fail("Chu kỳ checklist không hợp lệ.");

        F03EquipmentInspectionTemplate entity;
        if (request.Id.HasValue)
        {
            entity = await _db.Set<F03EquipmentInspectionTemplate>().Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && x.IsActive != false, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy checklist.");

            // Published versions are immutable. Clone is the supported way to change them.
            if (string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase))
                return ServiceResult<EquipmentInspectionTemplateDto>.Fail("Checklist Active đã phát hành không được sửa. Hãy Nhân bản để tạo version mới.");

            entity.TemplateName = request.TemplateName.Trim();
            entity.Description = request.Description?.Trim();
            entity.Frequency = request.Frequency;
            entity.Status = request.Status;
            entity.DeptCode = dept;
            entity.ModifiedBy = user.UserId;
            entity.ModifiedAt = DateTime.Now;
            entity.LastModifiedSource = "EQUIPMENT_INSPECTION_TEMPLATE";
            foreach (var old in entity.Items) old.IsActive = false;
            entity.Items.Clear();
        }
        else
        {
            var code = request.TemplateCode.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code)) code = $"EQI-{Guid.NewGuid():N}"[..12];
            entity = new F03EquipmentInspectionTemplate
            {
                TemplateCode = code,
                TemplateName = request.TemplateName.Trim(),
                DeptCode = dept,
                Frequency = request.Frequency,
                Version = 1,
                Status = request.Status,
                Description = request.Description?.Trim(),
                CreatedBy = user.UserId
            };
            await _db.Set<F03EquipmentInspectionTemplate>().AddAsync(entity, ct);
        }

        var itemCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in request.Items.OrderBy(x => x.DisplayOrder))
        {
            var code = string.IsNullOrWhiteSpace(item.ItemCode) ? $"ITEM-{item.DisplayOrder + 1:000}" : item.ItemCode.Trim().ToUpperInvariant();
            if (!itemCodes.Add(code)) return ServiceResult<EquipmentInspectionTemplateDto>.Fail($"Hạng mục {code} bị trùng.");
            if (item.RequireImage && item.MinImages < 1) item.MinImages = 1;
            entity.Items.Add(new F03EquipmentInspectionItem
            {
                ItemCode = code,
                ItemLabel = item.ItemLabel.Trim(),
                InputType = item.InputType,
                IsRequired = item.IsRequired,
                RequireImage = item.RequireImage,
                MinImages = item.MinImages,
                MaxImages = Math.Max(item.MaxImages, item.MinImages),
                MinValue = item.MinValue,
                MaxValue = item.MaxValue,
                Unit = item.Unit?.Trim(),
                OptionsJson = item.OptionsJson,
                DisplayOrder = item.DisplayOrder,
                CreatedBy = user.UserId
            });
        }

        if (string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            await _db.Set<F03EquipmentInspectionTemplate>()
                .Where(x => x.IsActive != false && x.DeptCode == entity.DeptCode && x.TemplateCode == entity.TemplateCode && x.Id != entity.Id && x.Status == "Active")
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Inactive").SetProperty(x => x.ModifiedAt, DateTime.Now).SetProperty(x => x.LastModifiedSource, "EQUIPMENT_INSPECTION_PUBLISH"), ct);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult<EquipmentInspectionTemplateDto>.Ok(MapTemplate(entity), "Đã lưu checklist.");
    }

    public async Task<ServiceResult<EquipmentInspectionTemplateDto>> CloneTemplateAsync(int id, string? name, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct))
            return ServiceResult<EquipmentInspectionTemplateDto>.Fail("Bạn không có quyền nhân bản checklist.");

        var source = await _db.Set<F03EquipmentInspectionTemplate>().AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive != false, ct);
        if (source == null) return ServiceResult<EquipmentInspectionTemplateDto>.Fail("Không tìm thấy checklist nguồn.");

        var nextVersion = (await _db.Set<F03EquipmentInspectionTemplate>()
            .Where(x => x.DeptCode == source.DeptCode && x.TemplateCode == source.TemplateCode)
            .MaxAsync(x => (int?)x.Version, ct) ?? source.Version) + 1;

        var clone = new F03EquipmentInspectionTemplate
        {
            TemplateCode = source.TemplateCode,
            TemplateName = string.IsNullOrWhiteSpace(name) ? $"{source.TemplateName} v{nextVersion}" : name.Trim(),
            DeptCode = source.DeptCode,
            Frequency = source.Frequency,
            Version = nextVersion,
            Status = "Draft",
            Description = source.Description,
            CreatedBy = user.UserId
        };
        foreach (var item in source.Items.Where(x => x.IsActive != false).OrderBy(x => x.DisplayOrder))
            clone.Items.Add(new F03EquipmentInspectionItem
            {
                ItemCode=item.ItemCode, ItemLabel=item.ItemLabel, InputType=item.InputType, IsRequired=item.IsRequired,
                RequireImage=item.RequireImage, MinImages=item.MinImages, MaxImages=item.MaxImages,
                MinValue=item.MinValue, MaxValue=item.MaxValue, Unit=item.Unit, OptionsJson=item.OptionsJson,
                DisplayOrder=item.DisplayOrder, CreatedBy=user.UserId
            });
        await _db.Set<F03EquipmentInspectionTemplate>().AddAsync(clone, ct);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<EquipmentInspectionTemplateDto>.Ok(MapTemplate(clone), $"Đã nhân bản thành version {nextVersion} ở trạng thái Draft.");
    }

    public async Task<ServiceResult<EquipmentInspectionAssignmentDto>> AssignAsync(EquipmentInspectionAssignmentRequest request, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct))
            return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Bạn không có quyền phân công checklist.");

        var asset = await _db.EquipmentAssets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.EquipmentId && x.IsActive == true, ct);
        if (asset == null) return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Chỉ thiết bị đã đăng ký và đang Active mới được gán checklist.");

        var template = await _db.Set<F03EquipmentInspectionTemplate>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId && x.IsActive != false && x.Status == "Active", ct);
        if (template == null) return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Checklist phải là version Active.");
        if (!string.Equals(template.DeptCode, asset.DeptCode, StringComparison.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("DepartmentCode của checklist không khớp DepartmentCode của thiết bị.");

        var inspector = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == request.InspectorEmployeeCode && x.IsActive != false, ct);
        var approver = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == request.ApproverEmployeeCode && x.IsActive != false, ct);
        if (inspector == null) return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Người check không tồn tại hoặc không còn hoạt động.");
        if (approver == null) return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Người approve không tồn tại hoặc không còn hoạt động.");

        var frequency = request.Frequency.Trim();
        if (!new[] { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly" }.Contains(frequency, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Chu kỳ không hợp lệ.");
        if (!string.Equals(frequency, template.Frequency, StringComparison.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionAssignmentDto>.Fail($"Chu kỳ phân công phải trùng với checklist ({template.Frequency}).");

        var exists = await _db.Set<F03EquipmentInspectionAssignment>().AnyAsync(x =>
            x.IsActive != false && x.EquipmentId == asset.Id && x.TemplateId == template.Id &&
            x.InspectorEmployeeCode == inspector.EmployeeCode && x.EffectiveFrom == request.EffectiveFrom.Date &&
            (x.EffectiveTo == null || x.EffectiveTo >= DateTime.Today), ct);
        if (exists) return ServiceResult<EquipmentInspectionAssignmentDto>.Fail("Thiết bị đã có phân công checklist tương tự.");

        var entity = new F03EquipmentInspectionAssignment
        {
            EquipmentId = asset.Id, TemplateId = template.Id, Frequency = frequency,
            DueTime = request.DueTime, ReminderHoursBefore = Math.Clamp(request.ReminderHoursBefore, 0, 168),
            ScheduleDayOfWeek = request.ScheduleDayOfWeek, ScheduleDayOfMonth = request.ScheduleDayOfMonth,
            ScheduleMonth = request.ScheduleMonth, InspectorEmployeeCode = inspector.EmployeeCode,
            ApproverEmployeeCode = approver.EmployeeCode, EffectiveFrom = request.EffectiveFrom.Date,
            EffectiveTo = request.EffectiveTo?.Date, CreatedBy = user.UserId
        };
        await _db.Set<F03EquipmentInspectionAssignment>().AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<EquipmentInspectionAssignmentDto>.Ok(MapAssignment(entity, asset, template), "Đã gán checklist cho thiết bị.");
    }

    public async Task<ServiceResult<List<EquipmentInspectionAssignmentDto>>> GetAssignmentsAsync(int? equipmentId, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct) &&
            !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionReport, ct))
            return ServiceResult<List<EquipmentInspectionAssignmentDto>>.Fail("Bạn không có quyền xem phân công checklist.");

        var q = _db.Set<F03EquipmentInspectionAssignment>().AsNoTracking().Where(x => x.IsActive != false)
            .Include(x => x.Equipment).Include(x => x.Template);
        if (equipmentId.HasValue) q = q.Where(x => x.EquipmentId == equipmentId.Value);
        var rows = await q.OrderBy(x => x.Equipment!.EquipmentCode).ThenBy(x => x.DueTime).ToListAsync(ct);
        return ServiceResult<List<EquipmentInspectionAssignmentDto>>.Ok(rows.Select(x => MapAssignment(x, x.Equipment!, x.Template!)).ToList());
    }

    public async Task<ServiceResult<List<EquipmentInspectionTaskDto>>> GetMyTasksAsync(bool includeCompleted, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionExecute, ct) &&
            !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionApprove, ct))
            return ServiceResult<List<EquipmentInspectionTaskDto>>.Fail("Bạn không có quyền xem nhiệm vụ kiểm tra.");

        var code = user.EmployeeCode ?? "";
        var q = _db.Set<F03EquipmentInspectionTask>().AsNoTracking()
            .Include(x => x.Equipment).Include(x => x.Template)
             .Include(x => x.ItemResults)
            .Where(x => x.IsActive != false && (x.InspectorEmployeeCode == code || x.ApproverEmployeeCode == code));
        if (!includeCompleted) q = q.Where(x => x.Status != "Approved");
        var rows = await q.OrderBy(x => x.DueAt).Take(500).ToListAsync(ct);
        return ServiceResult<List<EquipmentInspectionTaskDto>>.Ok(rows.Select(MapTask).ToList());
    }

    public async Task<ServiceResult<EquipmentInspectionTaskDto>> GetTaskAsync(int id, CancellationToken ct = default)
    {
        var task = await LoadTask(id, ct);
        if (task == null) return ServiceResult<EquipmentInspectionTaskDto>.Fail("Không tìm thấy nhiệm vụ kiểm tra.");
        await EnsureTaskAccess(task, ct);
        return ServiceResult<EquipmentInspectionTaskDto>.Ok(MapTask(task));
    }

    public async Task<ServiceResult<EquipmentInspectionTaskDto>> SubmitAsync(EquipmentInspectionSubmitRequest request, CancellationToken ct = default)
    {
        var task = await LoadTask(request.TaskId, ct);
        if (task == null) return ServiceResult<EquipmentInspectionTaskDto>.Fail("Không tìm thấy nhiệm vụ kiểm tra.");
        var user = RequireUser();
        if (!string.Equals(task.InspectorEmployeeCode, user.EmployeeCode, StringComparison.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionTaskDto>.Fail("Bạn không phải người được phân công kiểm tra.");
        if (task.Status is "Approved" or "Cancelled")
            return ServiceResult<EquipmentInspectionTaskDto>.Fail("Checklist đã được phê duyệt/kết thúc và bị khóa, không thể sửa đổi.");

        var items = task.Template!.Items.Where(x => x.IsActive != false).OrderBy(x => x.DisplayOrder).ToList();
        foreach (var item in items)
        {
            var submitted = request.Items.FirstOrDefault(x => x.ItemId == item.Id);
            if (item.IsRequired && submitted == null)
                return ServiceResult<EquipmentInspectionTaskDto>.Fail($"Chưa nhập hạng mục: {item.ItemLabel}");
            if (submitted == null) continue;
            if (item.RequireImage)
            {
                var resultId = submitted.Id > 0 ? submitted.Id : task.ItemResults.FirstOrDefault(x => x.ItemId == submitted.ItemId)?.Id;
                var imageCount = resultId.HasValue ? await _db.Set<F03EquipmentInspectionEvidence>().CountAsync(x => x.TaskId == task.Id && x.ItemResultId == resultId.Value && x.IsActive != false, ct) : 0;
                if (imageCount < item.MinImages)
                    return ServiceResult<EquipmentInspectionTaskDto>.Fail($"Hạng mục '{item.ItemLabel}' yêu cầu tối thiểu {item.MinImages} hình ảnh.");
            }
            if (item.InputType.Equals("Number", StringComparison.OrdinalIgnoreCase) && submitted.ValueNumber.HasValue)
            {
                if (item.MinValue.HasValue && submitted.ValueNumber.Value < item.MinValue.Value ||
                    item.MaxValue.HasValue && submitted.ValueNumber.Value > item.MaxValue.Value)
                    submitted.Passed = false;
            }
        }

        foreach (var dto in request.Items)
        {
            var result = task.ItemResults.FirstOrDefault(x => x.ItemId == dto.ItemId);
            if (result == null)
            {
                result = new F03EquipmentInspectionItemResult { TaskId = task.Id, ItemId = dto.ItemId, CreatedBy = user.UserId };
                task.ItemResults.Add(result);
            }
            result.ValueText = dto.ValueText;
            result.ValueNumber = dto.ValueNumber;
            result.Passed = dto.Passed;
            result.Note = dto.Note;
            result.ModifiedBy = user.UserId;
            result.ModifiedAt = DateTime.Now;
        }

        task.Result = request.Result ?? (task.ItemResults.Any(x => x.Passed == false) ? "Failed" : "Passed");
        task.Status = "WaitingApproval";
        task.SubmittedAt = DateTime.Now;
        if (!task.ActionId.HasValue)
            await EnsureTaskActionAsync(task, ct);
        if (task.ActionId.HasValue)
            await _actionService.CompleteAsync(user.EmployeeCode ?? string.Empty, user.UserId, task.ActionId.Value, ct);
        var approvalActionId = await EnsureApproverActionAsync(task, ct);
        if (approvalActionId.HasValue)
        {
            var approverUser = await _db.Set<F03User>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == task.ApproverEmployeeCode && x.IsActive != false, ct);
            if (approverUser != null)
                await _notification.CreateAsync(new CreateNotificationDto
                {
                    UserId = approverUser.Id,
                    EmployeeCode = task.ApproverEmployeeCode,
                    Module = RequestModule.Equipment,
                    RelatedRequestId = task.Id,
                    Action = NotificationAction.Pending,
                    Title = $"Checklist thiết bị chờ duyệt {task.Equipment?.EquipmentCode}",
                    Body = $"Kết quả {task.Template?.TemplateName} đã được gửi. Vui lòng phê duyệt.",
                    ActionUrl = $"/equipment/inspection/{task.Id}",
                    ActionId = approvalActionId,
                    NotificationType = "EQUIPMENT_INSPECTION_APPROVAL_REQUEST"
                }, ct);
        }
        task.StartedAt ??= task.SubmittedAt;
        task.ModifiedBy = user.UserId;
        await _db.SaveChangesAsync(ct);

        var approver = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == task.ApproverEmployeeCode, ct);
        if (!string.IsNullOrWhiteSpace(approver?.EmailAddress))
        {
            await _email.QueueEmail(approver.EmailAddress, "EQUIPMENT_INSPECTION_APPROVAL_REQUEST", new
            {
                EquipmentCode = task.Equipment?.EquipmentCode,
                EquipmentName = task.Equipment?.EquipmentName,
                ChecklistName = task.Template?.TemplateName,
                ScheduledDate = task.ScheduledDate.ToString("dd/MM/yyyy"),
                InspectorEmployeeCode = task.InspectorEmployeeCode
            }, ct);
        }
        return ServiceResult<EquipmentInspectionTaskDto>.Ok(MapTask(task), "Đã gửi kết quả kiểm tra chờ phê duyệt.");
    }

    public async Task<ServiceResult<EquipmentInspectionTaskDto>> ApproveAsync(int id, CancellationToken ct = default)
    {
        var task = await LoadTask(id, ct);
        if (task == null) return ServiceResult<EquipmentInspectionTaskDto>.Fail("Không tìm thấy nhiệm vụ.");
        var user = RequireUser();
        if (!string.Equals(task.ApproverEmployeeCode, user.EmployeeCode, StringComparison.OrdinalIgnoreCase) &&
            !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionApprove, ct))
            return ServiceResult<EquipmentInspectionTaskDto>.Fail("Bạn không có quyền phê duyệt nhiệm vụ này.");
        if (task.Status == "Approved")
            return ServiceResult<EquipmentInspectionTaskDto>.Fail("Checklist đã được phê duyệt và bị khóa, không thể phê duyệt lại.");
        if (task.Status != "WaitingApproval") return ServiceResult<EquipmentInspectionTaskDto>.Fail("Nhiệm vụ chưa ở trạng thái chờ phê duyệt.");
        task.Status = "Approved"; task.ApprovedAt = DateTime.Now; task.ModifiedBy = user.UserId;
        await _db.SaveChangesAsync(ct);
        if (task.ActionId.HasValue)
            await _actionService.CompleteAsync(user.EmployeeCode ?? string.Empty, user.UserId, task.ActionId.Value, ct);
        await NotifyInspectorAsync(task, "EQUIPMENT_INSPECTION_APPROVED", ct);
        return ServiceResult<EquipmentInspectionTaskDto>.Ok(MapTask(task), "Đã phê duyệt checklist.");
    }

    public async Task<ServiceResult<EquipmentInspectionTaskDto>> RejectAsync(int id, string reason, CancellationToken ct = default)
    {
        var task = await LoadTask(id, ct);
        if (task == null) return ServiceResult<EquipmentInspectionTaskDto>.Fail("Không tìm thấy nhiệm vụ.");
        var user = RequireUser();
        if (!string.Equals(task.ApproverEmployeeCode, user.EmployeeCode, StringComparison.OrdinalIgnoreCase) &&
            !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionApprove, ct))
            return ServiceResult<EquipmentInspectionTaskDto>.Fail("Bạn không có quyền từ chối nhiệm vụ này.");
        if (task.Status == "Approved")
            return ServiceResult<EquipmentInspectionTaskDto>.Fail("Checklist đã được phê duyệt và bị khóa, không thể thay đổi.");
        if (string.IsNullOrWhiteSpace(reason)) return ServiceResult<EquipmentInspectionTaskDto>.Fail("Lý do từ chối là bắt buộc.");
        task.Status = "Rejected"; task.RejectedAt = DateTime.Now; task.RejectReason = reason.Trim(); task.ModifiedBy = user.UserId;
        await _db.SaveChangesAsync(ct);
        if (task.ApprovalActionId.HasValue)
            await _actionService.CompleteAsync(user.EmployeeCode ?? string.Empty, user.UserId, task.ApprovalActionId.Value, ct);
        task.ApprovalActionId = null;
        task.ActionId = null;
        await EnsureTaskActionAsync(task, ct);
        await NotifyInspectorAsync(task, "EQUIPMENT_INSPECTION_REJECTED", ct);
        return ServiceResult<EquipmentInspectionTaskDto>.Ok(MapTask(task), "Đã từ chối checklist.");
    }

    public async Task<ServiceResult<EquipmentInspectionDashboardDto>> DashboardAsync(DateTime from, DateTime to, string? deptCode, CancellationToken ct = default)
    {
        var user = RequireUser();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionReport, ct))
            return ServiceResult<EquipmentInspectionDashboardDto>.Fail("Bạn không có quyền xem báo cáo checklist.");

        var q = _db.Set<F03EquipmentInspectionTask>().AsNoTracking().Include(x => x.Equipment).Include(x => x.Template)
            .Where(x => x.IsActive != false && x.ScheduledDate >= from.Date && x.ScheduledDate <= to.Date);
        if (!string.IsNullOrWhiteSpace(deptCode)) q = q.Where(x => x.Equipment!.DeptCode == deptCode.Trim().ToUpperInvariant());
        var rows = await q.OrderByDescending(x => x.ScheduledDate).ThenBy(x => x.Equipment!.EquipmentCode).Take(5000).ToListAsync(ct);
        var total = rows.Count;
        var completed = rows.Count(x => x.Status == "Approved");
        var pending = rows.Count(x => x.Status is "Scheduled" or "InProgress" or "WaitingApproval" or "Rejected");
        var overdue = rows.Count(x => x.Status == "Overdue");
        var failed = rows.Count(x => x.Result == "Failed");
        var approved = rows.Count(x => x.Status == "Approved");
        var passed = rows.Count(x => x.Status == "Approved" && !string.Equals(x.Result, "Failed", StringComparison.OrdinalIgnoreCase));
        return ServiceResult<EquipmentInspectionDashboardDto>.Ok(new EquipmentInspectionDashboardDto
        {
            Total = total, Completed = completed, Pending = pending, Overdue = overdue, Failed = failed,
            CompletionRate = total == 0 ? 0 : Math.Round(completed * 100m / total, 2),
            PassRate = approved == 0 ? 0 : Math.Round(passed * 100m / approved, 2),
            Rows = rows.Select(x => new EquipmentInspectionReportRowDto
            {
                TaskId = x.Id,
                DeptCode = x.Equipment?.DeptCode ?? "", EquipmentCode = x.Equipment?.EquipmentCode ?? "",
                EquipmentName = x.Equipment?.EquipmentName ?? "", TemplateName = x.Template?.TemplateName ?? "",
                InspectorEmployeeCode = x.InspectorEmployeeCode, Status = x.Status, Result = x.Result,
                ScheduledDate = x.ScheduledDate, DueAt = x.DueAt, SubmittedAt = x.SubmittedAt, ApprovedAt = x.ApprovedAt
            }).ToList()
        });
    }

    public async Task<ServiceResult<EquipmentInspectionEvidenceDto>> AddEvidenceAsync(int taskId, int? itemResultId, int? itemId, string fileName, string contentType, Stream content, CancellationToken ct = default)
    {
        var task = await LoadTask(taskId, ct);
        if (task == null) return ServiceResult<EquipmentInspectionEvidenceDto>.Fail("Không tìm thấy nhiệm vụ.");
        await EnsureTaskAccess(task, ct);
        if (string.Equals(task.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionEvidenceDto>.Fail("Checklist đã được phê duyệt và bị khóa hoàn toàn, không thể thêm/sửa/xóa hình ảnh.");
        if (task.Status is "Cancelled")
            return ServiceResult<EquipmentInspectionEvidenceDto>.Fail("Checklist đã kết thúc và không thể thay đổi.");
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<EquipmentInspectionEvidenceDto>.Fail("Chỉ cho phép hình ảnh.");
        if (content.Length > 10 * 1024 * 1024) return ServiceResult<EquipmentInspectionEvidenceDto>.Fail("Hình ảnh tối đa 10 MB.");
        if (itemResultId.HasValue)
        {
            var itemResult = await _db.Set<F03EquipmentInspectionItemResult>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == itemResultId.Value && x.TaskId == taskId, ct);
            if (itemResult == null) return ServiceResult<EquipmentInspectionEvidenceDto>.Fail("Kết quả hạng mục không hợp lệ.");
            var item = task.Template?.Items.FirstOrDefault(x => x.Id == itemResult.ItemId);
            if (item != null)
            {
                var count = await _db.Set<F03EquipmentInspectionEvidence>().CountAsync(x => x.TaskId == taskId && x.ItemResultId == itemResultId.Value && x.IsActive != false, ct);
                if (count >= item.MaxImages) return ServiceResult<EquipmentInspectionEvidenceDto>.Fail($"Hạng mục chỉ cho phép tối đa {item.MaxImages} hình ảnh.");
            }
        }

        var folder = Path.Combine(_environment.ContentRootPath, "App_Data", "equipment-inspections", taskId.ToString());
        Directory.CreateDirectory(folder);
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
        var stored = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var path = Path.Combine(folder, stored);
        await using (var output = File.Create(path)) await content.CopyToAsync(output, ct);

        var entity = new F03EquipmentInspectionEvidence
        {
            TaskId = taskId, ItemResultId = itemResultId, FileName = Path.GetFileName(fileName),
            ContentType = contentType, FileSize = content.Length,
            StoragePath = Path.Combine("App_Data", "equipment-inspections", taskId.ToString(), stored),
            CreatedBy = RequireUser().UserId
        };
        await _db.Set<F03EquipmentInspectionEvidence>().AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<EquipmentInspectionEvidenceDto>.Ok(new EquipmentInspectionEvidenceDto
        {
            Id = entity.Id, ItemResultId = itemResultId, FileName = entity.FileName, ContentType = entity.ContentType,
            FileSize = entity.FileSize, Url = $"/api/equipment-inspections/evidence/{entity.Id}"
        });
    }

    public async Task<ServiceResult<(string ContentType, Stream Content, string FileName)>> OpenEvidenceAsync(int evidenceId, CancellationToken ct = default)
    {
        var evidence = await _db.Set<F03EquipmentInspectionEvidence>().AsNoTracking().Include(x => x.Task)
            .FirstOrDefaultAsync(x => x.Id == evidenceId && x.IsActive != false, ct);
        if (evidence == null) return ServiceResult<(string, Stream, string)>.Fail("Không tìm thấy hình ảnh.");
        await EnsureTaskAccess(evidence.Task!, ct);
        var path = Path.Combine(_environment.ContentRootPath, evidence.StoragePath);
        if (!File.Exists(path)) return ServiceResult<(string, Stream, string)>.Fail("File hình ảnh không còn tồn tại.");
        return ServiceResult<(string, Stream, string)>.Ok((evidence.ContentType, File.OpenRead(path), evidence.FileName));
    }

    public async Task GenerateScheduledTasksAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var assignments = await _db.Set<F03EquipmentInspectionAssignment>()
            .Include(x => x.Equipment).Include(x => x.Template)
            .Where(x => x.IsActive != false && x.Equipment!.IsActive == true && x.Template!.IsActive != false && x.Template.Status == "Active"
                && x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToListAsync(ct);

        foreach (var a in assignments)
        {
            if (!MatchesSchedule(a, today)) continue;
            var due = today.Add(a.DueTime);
            var exists = await _db.Set<F03EquipmentInspectionTask>().AnyAsync(x =>
                x.IsActive != false && x.AssignmentId == a.Id && x.ScheduledDate == today, ct);
            if (exists) continue;
            var task = new F03EquipmentInspectionTask
            {
                EquipmentId = a.EquipmentId, TemplateId = a.TemplateId, AssignmentId = a.Id,
                ScheduledDate = today, DueAt = due, Status = "Scheduled",
                InspectorEmployeeCode = a.InspectorEmployeeCode, ApproverEmployeeCode = a.ApproverEmployeeCode,
                CreatedBy = 0
            };
            await _db.Set<F03EquipmentInspectionTask>().AddAsync(task, ct);
        }
        await _db.SaveChangesAsync(ct);

        // Every generated inspection task becomes one shared Action Center item.
        // Dashboard/Work Inbox/Calendar can therefore use the same ActionId.
        var scheduledTasks = await _db.Set<F03EquipmentInspectionTask>()
            .Where(x => x.IsActive != false && x.ScheduledDate == today &&
                        x.ActionId == null &&
                        x.Status == "Scheduled")
            .ToListAsync(ct);
        foreach (var scheduled in scheduledTasks)
            await EnsureTaskActionAsync(scheduled, ct);

        var reminders = await _db.Set<F03EquipmentInspectionTask>()
            .Include(x => x.Equipment).Include(x => x.Template)
            .Where(x => x.IsActive != false && (x.Status == "Scheduled" || x.Status == "InProgress")
                && x.ReminderSentAt == null && x.DueAt > DateTime.Now)
            .ToListAsync(ct);

        foreach (var task in reminders)
        {
            var assignment = assignments.FirstOrDefault(x => x.Id == task.AssignmentId);
            if (assignment == null) continue;
            var remindAt = task.DueAt.AddHours(-assignment.ReminderHoursBefore);
            if (remindAt > DateTime.Now) continue;
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode, ct);
            if (employee == null || string.IsNullOrWhiteSpace(employee.EmailAddress)) continue;
            await _email.QueueEmail(employee.EmailAddress, "EQUIPMENT_INSPECTION_REMINDER", new
            {
                InspectorName = employee.EmployeeName, EquipmentCode = task.Equipment?.EquipmentCode,
                EquipmentName = task.Equipment?.EquipmentName, ChecklistName = task.Template?.TemplateName,
                DueAt = task.DueAt.ToString("dd/MM/yyyy HH:mm")
            }, ct);
            if (task.ActionId.HasValue)
            {
                var userAccount = await _db.Set<F03User>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode && x.IsActive != false, ct);
                if (userAccount != null)
                    await _notification.CreateAsync(new CreateNotificationDto
                    {
                        UserId = userAccount.Id,
                        EmployeeCode = employee.EmployeeCode,
                        Module = RequestModule.Equipment,
                        RelatedRequestId = task.Id,
                        Action = NotificationAction.Reminder,
                        Title = $"Nhắc kiểm tra thiết bị {task.Equipment?.EquipmentCode}",
                        Body = $"Checklist {task.Template?.TemplateName} đến hạn {task.DueAt:dd/MM/yyyy HH:mm}.",
                        ActionUrl = $"/equipment/inspection/{task.Id}",
                        IsHighPriority = false,
                        ActionId = task.ActionId,
                        NotificationType = "EQUIPMENT_INSPECTION_REMINDER"
                    }, ct);
            }
            task.ReminderSentAt = DateTime.Now;
        }

        var overdue = await _db.Set<F03EquipmentInspectionTask>()
            .Include(x => x.Equipment).Include(x => x.Template)
            .Where(x => x.IsActive != false && (x.Status == "Scheduled" || x.Status == "InProgress")
                && x.DueAt < DateTime.Now && x.OverdueReminderSentAt == null)
            .ToListAsync(ct);
        foreach (var task in overdue)
        {
            task.Status = "Overdue";
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode, ct);
            if (employee != null && !string.IsNullOrWhiteSpace(employee.EmailAddress))
            {
                await _email.QueueEmail(employee.EmailAddress, "EQUIPMENT_INSPECTION_OVERDUE", new
                {
                    InspectorName = employee.EmployeeName, EquipmentCode = task.Equipment?.EquipmentCode,
                    EquipmentName = task.Equipment?.EquipmentName, ChecklistName = task.Template?.TemplateName,
                    DueAt = task.DueAt.ToString("dd/MM/yyyy HH:mm")
                }, ct);
                if (task.ActionId.HasValue)
                {
                    var userAccount = await _db.Set<F03User>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode && x.IsActive != false, ct);
                    if (userAccount != null)
                        await _notification.CreateAsync(new CreateNotificationDto
                        {
                            UserId = userAccount.Id,
                            EmployeeCode = employee.EmployeeCode,
                            Module = RequestModule.Equipment,
                            RelatedRequestId = task.Id,
                            Action = NotificationAction.Escalated,
                            Title = $"Checklist thiết bị quá hạn {task.Equipment?.EquipmentCode}",
                            Body = $"Checklist {task.Template?.TemplateName} đã quá hạn {task.DueAt:dd/MM/yyyy HH:mm}.",
                            ActionUrl = $"/equipment/inspection/{task.Id}",
                            IsHighPriority = true,
                            ActionId = task.ActionId,
                            NotificationType = "EQUIPMENT_INSPECTION_OVERDUE"
                        }, ct);
                }
            }
            task.OverdueReminderSentAt = DateTime.Now;
        }
        if (overdue.Count > 0 || reminders.Count > 0) await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureTaskActionAsync(F03EquipmentInspectionTask task, CancellationToken ct)
    {
        if (task.ActionId.HasValue) return;
        var inspector = await _db.Set<F03Employee>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode && x.IsActive != false, ct);
        if (inspector == null) return;
        var account = await _db.Set<F03User>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == inspector.EmployeeCode && x.IsActive != false, ct);
        var actionId = await _actionWriter.EnsureOpenAsync(new ActionItemDraft(
            "Equipment", task.Id.ToString(), inspector.Id, inspector.Id, account?.Id,
            DateOnly.FromDateTime(task.ScheduledDate), "EQUIPMENT_INSPECTION_CHECK",
            $"Kiểm tra thiết bị {task.Equipment?.EquipmentCode}",
            $"Checklist {task.Template?.TemplateName} — hạn {task.DueAt:dd/MM/yyyy HH:mm}.",
            1, 100, task.DueAt, $"/equipment/inspection/{task.Id}", task.Equipment?.EquipmentCode,
            System.Text.Json.JsonSerializer.Serialize(new { task.Id, task.EquipmentId, task.TemplateId }), "EQUIPMENT_INSPECTION",
            task.EquipmentId.ToString(), null));
        task.ActionId = actionId;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Guid?> EnsureApproverActionAsync(F03EquipmentInspectionTask task, CancellationToken ct)
    {
        var approver = await _db.Set<F03Employee>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == task.ApproverEmployeeCode && x.IsActive != false, ct);
        if (approver == null) return null;
        if (task.ApprovalActionId.HasValue) return task.ApprovalActionId.Value;
        var account = await _db.Set<F03User>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == approver.EmployeeCode && x.IsActive != false, ct);
        var actionId = await _actionWriter.EnsureOpenAsync(new ActionItemDraft(
            "Equipment", task.Id.ToString(), approver.Id, approver.Id, account?.Id,
            DateOnly.FromDateTime(task.ScheduledDate), "EQUIPMENT_INSPECTION_APPROVE",
            $"Phê duyệt checklist {task.Equipment?.EquipmentCode}",
            $"Kết quả kiểm tra {task.Template?.TemplateName} đang chờ phê duyệt.",
            1, 100, null, $"/equipment/inspection/{task.Id}", task.Equipment?.EquipmentCode,
            System.Text.Json.JsonSerializer.Serialize(new { task.Id, task.EquipmentId, task.TemplateId }), "EQUIPMENT_INSPECTION",
            task.EquipmentId.ToString(), null));
        task.ApprovalActionId = actionId;
        await _db.SaveChangesAsync(ct);
        return actionId;
    }

    private async Task NotifyInspectorInAppAsync(F03EquipmentInspectionTask task, NotificationAction action, string body, CancellationToken ct)
    {
        var account = await _db.Set<F03User>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode && x.IsActive != false, ct);
        if (account == null) return;
        await _notification.CreateAsync(new CreateNotificationDto
        {
            UserId = account.Id,
            EmployeeCode = task.InspectorEmployeeCode,
            Module = RequestModule.Equipment,
            RelatedRequestId = task.Id,
            Action = action,
            Title = action == NotificationAction.Rejected ? "Checklist thiết bị cần kiểm tra lại" : "Checklist thiết bị đã được phê duyệt",
            Body = body,
            ActionUrl = $"/equipment/inspection/{task.Id}",
            ActionId = task.ActionId,
            NotificationType = $"EQUIPMENT_INSPECTION_{action.ToString().ToUpperInvariant()}"
        }, ct);
    }

    private async Task<F03EquipmentInspectionTask?> LoadTask(int id, CancellationToken ct) =>
        await _db.Set<F03EquipmentInspectionTask>().Include(x => x.Equipment).Include(x => x.Template).ThenInclude(x => x!.Items)
            .Include(x => x.ItemResults).Include(x => x.Evidence)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive != false, ct);

    private async Task EnsureTaskAccess(F03EquipmentInspectionTask task, CancellationToken ct)
    {
        var user = RequireUser();
        if (task.InspectorEmployeeCode == user.EmployeeCode || task.ApproverEmployeeCode == user.EmployeeCode) return;
        if (await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionReport, ct)) return;
        throw new UnauthorizedAccessException("Bạn không có quyền truy cập checklist này.");
    }

    private async Task NotifyInspectorAsync(F03EquipmentInspectionTask task, string template, CancellationToken ct)
    {
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == task.InspectorEmployeeCode, ct);
        if (employee == null || string.IsNullOrWhiteSpace(employee.EmailAddress)) return;
        await _email.QueueEmail(employee.EmailAddress, template, new
        {
            InspectorName = employee.EmployeeName, EquipmentCode = task.Equipment?.EquipmentCode,
            EquipmentName = task.Equipment?.EquipmentName, ChecklistName = task.Template?.TemplateName,
            DueAt = task.DueAt.ToString("dd/MM/yyyy HH:mm"), RejectReason = task.RejectReason
        }, ct);
    }

    private static bool MatchesSchedule(F03EquipmentInspectionAssignment a, DateTime date)
    {
        return a.Frequency switch
        {
            "Daily" => true,
            "Weekly" => !a.ScheduleDayOfWeek.HasValue || a.ScheduleDayOfWeek.Value == (int)date.DayOfWeek,
            "Monthly" => !a.ScheduleDayOfMonth.HasValue || a.ScheduleDayOfMonth.Value == date.Day,
            "Quarterly" => (!a.ScheduleDayOfMonth.HasValue || a.ScheduleDayOfMonth.Value == date.Day) &&
                           (!a.ScheduleMonth.HasValue || ((date.Month - 1) % 3) == ((a.ScheduleMonth.Value - 1) % 3)),
            "Yearly" => (!a.ScheduleDayOfMonth.HasValue || a.ScheduleDayOfMonth.Value == date.Day) &&
                        (!a.ScheduleMonth.HasValue || a.ScheduleMonth.Value == date.Month),
            _ => false
        };
    }

    private FVN_REGISTER.Contract.Dtos.Authentication.UserIdentityDto RequireUser() =>
        _currentUser.GetCurrentUser() ?? throw new UnauthorizedAccessException("Phiên đăng nhập hết hạn.");

    private static EquipmentInspectionTemplateDto MapTemplate(F03EquipmentInspectionTemplate x) => new()
    {
        Id=x.Id, TemplateCode=x.TemplateCode, TemplateName=x.TemplateName, DeptCode=x.DeptCode, Frequency=x.Frequency,
        Version=x.Version, Status=x.Status, Description=x.Description,
        Items=x.Items.Where(i=>i.IsActive!=false).OrderBy(i=>i.DisplayOrder).Select(i=>new EquipmentInspectionItemDto
        {
            Id=i.Id, ItemCode=i.ItemCode, ItemLabel=i.ItemLabel, InputType=i.InputType, IsRequired=i.IsRequired,
            RequireImage=i.RequireImage, MinImages=i.MinImages, MaxImages=i.MaxImages, MinValue=i.MinValue, MaxValue=i.MaxValue,
            Unit=i.Unit, OptionsJson=i.OptionsJson, DisplayOrder=i.DisplayOrder
        }).ToList()
    };

    private static EquipmentInspectionAssignmentDto MapAssignment(F03EquipmentInspectionAssignment x, F03EquipmentAsset asset, F03EquipmentInspectionTemplate template) => new()
    {
        Id=x.Id, EquipmentId=x.EquipmentId, TemplateId=x.TemplateId, Frequency=x.Frequency, DueTime=x.DueTime,
        ReminderHoursBefore=x.ReminderHoursBefore, ScheduleDayOfWeek=x.ScheduleDayOfWeek, ScheduleDayOfMonth=x.ScheduleDayOfMonth,
        ScheduleMonth=x.ScheduleMonth, InspectorEmployeeCode=x.InspectorEmployeeCode, ApproverEmployeeCode=x.ApproverEmployeeCode,
        EffectiveFrom=x.EffectiveFrom, EffectiveTo=x.EffectiveTo, EquipmentCode=asset.EquipmentCode, EquipmentName=asset.EquipmentName, TemplateName=template.TemplateName
    };

    private static EquipmentInspectionTaskDto MapTask(F03EquipmentInspectionTask x) => new()
    {
        Id=x.Id, EquipmentId=x.EquipmentId, EquipmentCode=x.Equipment?.EquipmentCode ?? "", EquipmentName=x.Equipment?.EquipmentName ?? "",
        AssetCode=x.Equipment?.AssetCode ?? "", DeptCode=x.Equipment?.DeptCode ?? "", TemplateId=x.TemplateId,
        TemplateName=x.Template?.TemplateName ?? "", TemplateVersion=x.Template?.Version ?? 0, Status=x.Status, Result=x.Result,
        ScheduledDate=x.ScheduledDate, DueAt=x.DueAt, InspectorEmployeeCode=x.InspectorEmployeeCode, ApproverEmployeeCode=x.ApproverEmployeeCode,
        Items=x.Template?.Items.Where(i=>i.IsActive!=false).OrderBy(i=>i.DisplayOrder).Select(i=>{
            var r=x.ItemResults.FirstOrDefault(v=>v.ItemId==i.Id);
            return new EquipmentInspectionItemResultDto { Id=r?.Id??0, ItemId=i.Id, ItemCode=i.ItemCode, ItemLabel=i.ItemLabel, InputType=i.InputType,
                IsRequired=i.IsRequired, RequireImage=i.RequireImage, MinImages=i.MinImages, MaxImages=i.MaxImages, MinValue=i.MinValue, MaxValue=i.MaxValue,
                Unit=i.Unit, OptionsJson=i.OptionsJson, ValueText=r?.ValueText, ValueNumber=r?.ValueNumber, Passed=r?.Passed, Note=r?.Note,
                Evidence=x.Evidence.Where(e=>e.ItemResultId==r?.Id).Select(e=>new EquipmentInspectionEvidenceDto{Id=e.Id,ItemResultId=e.ItemResultId,FileName=e.FileName,ContentType=e.ContentType,FileSize=e.FileSize,Url=$"/api/equipment-inspections/evidence/{e.Id}"}).ToList()
            };
        }).ToList() ?? new(),
        Evidence=x.Evidence.Select(e=>new EquipmentInspectionEvidenceDto{Id=e.Id,ItemResultId=e.ItemResultId,FileName=e.FileName,ContentType=e.ContentType,FileSize=e.FileSize,Url=$"/api/equipment-inspections/evidence/{e.Id}"}).ToList()
    };

    private static EquipmentInspectionTemplateDto MapTemplateWithItems(F03EquipmentInspectionTemplate x) => MapTemplate(x);
}
