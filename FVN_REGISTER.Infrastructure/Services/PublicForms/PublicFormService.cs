using FVN_REGISTER.Application.Interfaces.PublicForms;
using FVN_REGISTER.Contract.Dtos.PublicForms;
using FVN_REGISTER.Contract.Requests.PublicForms;
using FVN_REGISTER.Core.Entities.PublicForms;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.PublicForms;

public sealed class PublicFormService : IPublicFormService
{
    private static readonly HashSet<string> QuestionTypes = new(StringComparer.OrdinalIgnoreCase)
    { "Text","Textarea","Number","Date","Time","DateTime","SingleChoice","MultiChoice","YesNo","Department","Employee","File" };
    private static readonly HashSet<string> AudienceTypes = new(StringComparer.OrdinalIgnoreCase)
    { "AllCompany","Department","Position","Employee" };
    private readonly IUnitOfWork _uow;
    public PublicFormService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<PublicFormDto>> GetManageListAsync(CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .Include(x => x.Audiences).AsSplitQuery().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<List<PublicFormDto>> GetAvailableAsync(string employeeCode, string? deptCode, string? positionCode, CancellationToken ct = default)
    {
        var normalizedEmployeeCode = employeeCode.Trim();
        var normalizedDeptCode = deptCode?.Trim();
        var normalizedPositionCode = positionCode?.Trim();

        if (normalizedEmployeeCode.Length == 0)
            return new List<PublicFormDto>();

        var now = DateTime.Now;

        // Filter audience in SQL first. This avoids loading every published form
        // and then evaluating Matches() in memory.
        var query = _uow.Repository<F03PublicForm>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true
                && x.Status == "Published"
                && (!x.StartAt.HasValue || x.StartAt <= now)
                && (!x.EndAt.HasValue || x.EndAt >= now)
                && x.Audiences.Any(a => a.IsActive == true
                    && (a.ScopeType == "AllCompany"
                        || (a.ScopeType == "Employee" && a.ScopeValue == normalizedEmployeeCode)
                        || (a.ScopeType == "Department"
                            && !string.IsNullOrWhiteSpace(normalizedDeptCode)
                            && a.ScopeValue == normalizedDeptCode)
                        || (a.ScopeType == "Position"
                            && !string.IsNullOrWhiteSpace(normalizedPositionCode)
                            && a.ScopeValue == normalizedPositionCode))));

        var rows = await query
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Audiences)
            .AsSplitQuery()
            .ToListAsync(ct);

        return rows.Select(Map).ToList();
    }

    public async Task<List<PublicFormAudienceLookupDto>> GetAudienceDepartmentsAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<F03Department>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.DisplayPriority ?? int.MaxValue)
            .ThenBy(x => x.DeptCode)
            .Select(x => new PublicFormAudienceLookupDto
            {
                Code = x.DeptCode,
                Name = x.DeptName
            })
            .ToListAsync(ct);
    }

    public async Task<List<PublicFormAudienceLookupDto>> GetAudiencePositionsAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<F03Position>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.PositionCode)
            .Select(x => new PublicFormAudienceLookupDto
            {
                Code = x.PositionCode,
                Name = x.PositionName
            })
            .ToListAsync(ct);
    }

    public async Task<PublicFormAudienceEmployeePageDto> SearchAudienceEmployeesAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 50);

        var q = _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true
                && (!x.EndWorkingDate.HasValue || x.EndWorkingDate.Value.Date >= DateTime.Today));

        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            q = q.Where(x =>
                x.EmployeeCode.Contains(term) ||
                x.EmployeeName.Contains(term) ||
                x.DeptCode.Contains(term) ||
                x.PositionCode.Contains(term));
        }

        var total = await q.CountAsync(ct);
        var rows = await q
            .OrderBy(x => x.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PublicFormAudienceLookupDto
            {
                Code = x.EmployeeCode,
                Name = x.EmployeeName,
                Secondary = x.DeptCode + " · " + x.PositionCode
            })
            .ToListAsync(ct);

        return new PublicFormAudienceEmployeePageDto
        {
            Items = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PublicFormDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var x = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .Include(x => x.Audiences).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
        return x == null ? null : Map(x);
    }

    public async Task<ServiceResult<PublicFormDto>> CreateAsync(SavePublicFormRequest request, int actorUserId, CancellationToken ct = default)
    {
        await ValidateAsync(request, ct);
        var exists = await _uow.Repository<F03PublicForm>().Query().AnyAsync(x => x.FormCode == request.FormCode.Trim(), ct);
        if (exists) return ServiceResult<PublicFormDto>.Fail("Mã biểu mẫu đã tồn tại.");
        var entity = BuildEntity(request, actorUserId);
        await _uow.Repository<F03PublicForm>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<PublicFormDto>.Ok(Map(entity));
    }

    public async Task<ServiceResult<PublicFormDto>> UpdateAsync(int id, SavePublicFormRequest request, int actorUserId, CancellationToken ct = default)
    {
        await ValidateAsync(request, ct);
        var entity = await _uow.Repository<F03PublicForm>().Query().Include(x=>x.Questions).ThenInclude(x=>x.Options).Include(x=>x.Audiences).FirstOrDefaultAsync(x=>x.Id==id,ct);
        if (entity == null) return ServiceResult<PublicFormDto>.Fail("Không tìm thấy biểu mẫu.");
        if (entity.Status == "Published") return ServiceResult<PublicFormDto>.Fail("Không sửa trực tiếp biểu mẫu đã Publish.");
        entity.FormCode=request.FormCode.Trim(); entity.Title=request.Title.Trim(); entity.Description=request.Description?.Trim(); entity.CategoryCode=request.CategoryCode?.Trim();
        entity.StartAt=request.StartAt; entity.EndAt=request.EndAt; entity.AllowMultipleSubmit=request.AllowMultipleSubmit; entity.RequireApproval=request.RequireApproval; entity.MaxSubmissions=request.MaxSubmissions;
        entity.ModifiedBy=actorUserId; entity.ModifiedAt=DateTime.Now;
        entity.Questions.Clear(); entity.Audiences.Clear();
        AddChildren(entity,request);
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<PublicFormDto>.Ok(Map(entity));
    }

    public async Task<ServiceResult> PublishAsync(int id,int actorUserId,CancellationToken ct=default)
    {
        var e=await _uow.Repository<F03PublicForm>().Query().Include(x=>x.Questions).Include(x=>x.Audiences).FirstOrDefaultAsync(x=>x.Id==id,ct);
        if(e==null)return ServiceResult.Fail("Không tìm thấy biểu mẫu.");
        if(e.Status=="Archived")return ServiceResult.Fail("Biểu mẫu đã Archive.");
        if(e.Questions.Count==0)return ServiceResult.Fail("Biểu mẫu phải có ít nhất một câu hỏi.");
        if(e.Audiences.Count==0)return ServiceResult.Fail("Biểu mẫu phải có đối tượng đăng ký.");
        e.Status="Published";e.PublishedAt=DateTime.Now;e.ModifiedBy=actorUserId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CloseAsync(int id,int actorUserId,CancellationToken ct=default)
    {
        var e=await _uow.Repository<F03PublicForm>().Query().FirstOrDefaultAsync(x=>x.Id==id,ct);
        if(e==null)return ServiceResult.Fail("Không tìm thấy biểu mẫu.");
        e.Status="Closed";e.ClosedAt=DateTime.Now;e.ModifiedBy=actorUserId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> SubmitAsync(int formId,string employeeCode,string? deptCode,string? positionCode,IReadOnlyCollection<PublicFormAnswerRequest> answers,CancellationToken ct=default)
    {
        var now=DateTime.Now;
        var form=await _uow.Repository<F03PublicForm>().Query()
            .Include(x=>x.Questions).ThenInclude(x=>x.Options)
            .Include(x=>x.Audiences)
            .FirstOrDefaultAsync(x=>x.Id==formId,ct);
        if(form==null) return ServiceResult<int>.Fail("Không tìm thấy biểu mẫu.");
        if(form.Status!="Published" || (form.StartAt.HasValue&&form.StartAt>now) || (form.EndAt.HasValue&&form.EndAt<now))
            return ServiceResult<int>.Fail("Biểu mẫu không còn nhận đăng ký.");
        if(!Matches(form,employeeCode,deptCode,positionCode))
            return ServiceResult<int>.Fail("Bạn không thuộc đối tượng được phép đăng ký.");
        var activeSubmissionQuery=_uow.Repository<F03PublicFormSubmission>().Query().Where(x=>x.FormId==formId&&x.Status!="Cancelled");
        if(form.MaxSubmissions.HasValue && await activeSubmissionQuery.CountAsync(ct)>=form.MaxSubmissions.Value) return ServiceResult<int>.Fail("Biểu mẫu đã đủ số lượng đăng ký.");
        if(!form.AllowMultipleSubmit && await activeSubmissionQuery.AnyAsync(x=>x.EmployeeCode==employeeCode,ct))
            return ServiceResult<int>.Fail("Bạn đã đăng ký biểu mẫu này.");
        var required=form.Questions.Where(q=>q.IsRequired&&q.IsActive==true).Select(q=>q.Id).ToHashSet();
        var provided=answers.Select(x=>x.QuestionId).ToHashSet();
        if(required.Any(id=>!provided.Contains(id))) return ServiceResult<int>.Fail("Vui lòng hoàn tất các câu hỏi bắt buộc.");
        foreach(var a in answers)
        {
            var q=form.Questions.FirstOrDefault(x=>x.Id==a.QuestionId&&x.IsActive==true);
            if(q==null) return ServiceResult<int>.Fail("Có câu hỏi không hợp lệ.");
            if((q.QuestionType=="SingleChoice"||q.QuestionType=="MultiChoice") && !string.IsNullOrWhiteSpace(a.JsonValue))
            {
                var selected=a.JsonValue.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
                if(selected.Any(v=>!q.Options.Any(o=>o.IsActive==true&&o.OptionCode==v))) return ServiceResult<int>.Fail($"Lựa chọn không hợp lệ cho {q.QuestionCode}.");
            }
        }
        var sub=new F03PublicFormSubmission{FormId=formId,EmployeeCode=employeeCode,SubmittedAt=now,Status="Submitted",FormVersion=form.Version};
        foreach(var a in answers) sub.Answers.Add(new F03PublicFormAnswer{QuestionId=a.QuestionId,TextValue=a.TextValue,NumberValue=a.NumberValue,DateValue=a.DateValue,BoolValue=a.BoolValue,JsonValue=a.JsonValue});
        await _uow.Repository<F03PublicFormSubmission>().AddAsync(sub,ct);
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<int>.Ok(sub.Id);
    }

    private static bool Matches(F03PublicForm x,string employeeCode,string? deptCode,string? positionCode)
        => x.Audiences.Any(a => a.IsActive == true && a.ScopeType=="AllCompany"
            || a.IsActive==true && a.ScopeType=="Employee" && string.Equals(a.ScopeValue,employeeCode,StringComparison.OrdinalIgnoreCase)
            || a.IsActive==true && a.ScopeType=="Department" && !string.IsNullOrWhiteSpace(deptCode) && string.Equals(a.ScopeValue,deptCode,StringComparison.OrdinalIgnoreCase)
            || a.IsActive==true && a.ScopeType=="Position" && !string.IsNullOrWhiteSpace(positionCode) && string.Equals(a.ScopeValue,positionCode,StringComparison.OrdinalIgnoreCase));

    private async Task ValidateAsync(SavePublicFormRequest r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.FormCode) || string.IsNullOrWhiteSpace(r.Title))
            throw new ArgumentException("Mã và tiêu đề biểu mẫu là bắt buộc.");

        if (r.EndAt.HasValue && r.StartAt.HasValue && r.EndAt < r.StartAt)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

        if (r.Questions.Count == 0)
            throw new ArgumentException("Biểu mẫu phải có ít nhất một câu hỏi.");

        foreach (var q in r.Questions)
        {
            if (!QuestionTypes.Contains(q.QuestionType))
                throw new ArgumentException($"QuestionType không hợp lệ: {q.QuestionType}");

            if ((q.QuestionType.Equals("SingleChoice", StringComparison.OrdinalIgnoreCase)
                || q.QuestionType.Equals("MultiChoice", StringComparison.OrdinalIgnoreCase))
                && !q.Options.Any())
                throw new ArgumentException($"Câu hỏi {q.QuestionCode} phải có lựa chọn.");
        }

        if (r.Audiences.Count == 0)
            throw new ArgumentException("Biểu mẫu phải có ít nhất một đối tượng đăng ký.");

        foreach (var a in r.Audiences)
        {
            a.ScopeType = a.ScopeType.Trim();

            if (!AudienceTypes.Contains(a.ScopeType))
                throw new ArgumentException($"ScopeType không hợp lệ: {a.ScopeType}");

            if (a.ScopeType.Equals("AllCompany", StringComparison.OrdinalIgnoreCase))
            {
                a.ScopeValue = null;
                continue;
            }

            if (string.IsNullOrWhiteSpace(a.ScopeValue))
                throw new ArgumentException($"Chưa chọn {a.ScopeType}.");

            a.ScopeValue = a.ScopeValue.Trim();
        }

        var allCompanyCount = r.Audiences.Count(x =>
            x.ScopeType.Equals("AllCompany", StringComparison.OrdinalIgnoreCase));

        if (allCompanyCount > 0 && r.Audiences.Count != 1)
            throw new ArgumentException("Toàn công ty không được kết hợp với Phòng ban, Vị trí hoặc Nhân viên.");

        var nonAllTypes = r.Audiences
            .Where(x => !x.ScopeType.Equals("AllCompany", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.ScopeType)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (nonAllTypes.Count > 1)
            throw new ArgumentException("Chỉ được chọn một loại đối tượng: Phòng ban, Vị trí hoặc Nhân viên.");

        var grouped = r.Audiences
            .Where(x => !x.ScopeType.Equals("AllCompany", StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.ScopeType, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            var values = group.Select(x => x.ScopeValue!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (group.Key.Equals("Department", StringComparison.OrdinalIgnoreCase))
            {
                var found = await _uow.Repository<F03Department>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive == true && values.Contains(x.DeptCode))
                    .Select(x => x.DeptCode)
                    .ToListAsync(ct);

                var missing = values.Where(v => !found.Contains(v, StringComparer.OrdinalIgnoreCase)).ToList();
                if (missing.Count > 0)
                    throw new ArgumentException($"Phòng ban không tồn tại hoặc đã ngừng hoạt động: {string.Join(", ", missing)}");
            }
            else if (group.Key.Equals("Position", StringComparison.OrdinalIgnoreCase))
            {
                var found = await _uow.Repository<F03Position>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive == true && values.Contains(x.PositionCode))
                    .Select(x => x.PositionCode)
                    .ToListAsync(ct);

                var missing = values.Where(v => !found.Contains(v, StringComparer.OrdinalIgnoreCase)).ToList();
                if (missing.Count > 0)
                    throw new ArgumentException($"Vị trí không tồn tại hoặc đã ngừng hoạt động: {string.Join(", ", missing)}");
            }
            else if (group.Key.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.Today;
                var found = await _uow.Repository<F03Employee>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive == true
                        && (!x.EndWorkingDate.HasValue || x.EndWorkingDate.Value.Date >= today)
                        && values.Contains(x.EmployeeCode))
                    .Select(x => x.EmployeeCode)
                    .ToListAsync(ct);

                var missing = values.Where(v => !found.Contains(v, StringComparer.OrdinalIgnoreCase)).ToList();
                if (missing.Count > 0)
                    throw new ArgumentException($"Nhân viên không tồn tại hoặc đã nghỉ việc: {string.Join(", ", missing)}");
            }
        }
    }

    private static F03PublicForm BuildEntity(SavePublicFormRequest r,int actor)
    {
        var e=new F03PublicForm{FormCode=r.FormCode.Trim(),Title=r.Title.Trim(),Description=r.Description?.Trim(),CategoryCode=r.CategoryCode?.Trim(),StartAt=r.StartAt,EndAt=r.EndAt,AllowMultipleSubmit=r.AllowMultipleSubmit,RequireApproval=r.RequireApproval,MaxSubmissions=r.MaxSubmissions,CreatedBy=actor};
        AddChildren(e,r); return e;
    }
    private static void AddChildren(F03PublicForm e,SavePublicFormRequest r)
    {
        foreach(var q in r.Questions.OrderBy(x=>x.Sequence))
        {
            var qe=new F03PublicFormQuestion{FormId=e.Id,QuestionCode=q.QuestionCode.Trim(),QuestionText=q.QuestionText.Trim(),QuestionType=q.QuestionType.Trim(),HelpText=q.HelpText,Placeholder=q.Placeholder,IsRequired=q.IsRequired,Sequence=q.Sequence};
            foreach(var o in q.Options.OrderBy(x=>x.Sequence)) qe.Options.Add(new F03PublicFormQuestionOption{QuestionId=qe.Id,OptionCode=o.OptionCode.Trim(),OptionText=o.OptionText.Trim(),Sequence=o.Sequence});
            e.Questions.Add(qe);
        }
        foreach(var a in r.Audiences) e.Audiences.Add(new F03PublicFormAudience{FormId=e.Id,ScopeType=a.ScopeType.Trim(),ScopeValue=a.ScopeValue?.Trim()});
    }
    private static PublicFormDto Map(F03PublicForm x)=>new(){Id=x.Id,FormCode=x.FormCode,Title=x.Title,Description=x.Description,CategoryCode=x.CategoryCode,Status=x.Status,StartAt=x.StartAt,EndAt=x.EndAt,AllowMultipleSubmit=x.AllowMultipleSubmit,RequireApproval=x.RequireApproval,MaxSubmissions=x.MaxSubmissions,Version=x.Version,IsActive=x.IsActive,Questions=x.Questions.OrderBy(q=>q.Sequence).Select(q=>new PublicFormQuestionDto{Id=q.Id,QuestionCode=q.QuestionCode,QuestionText=q.QuestionText,QuestionType=q.QuestionType,HelpText=q.HelpText,Placeholder=q.Placeholder,IsRequired=q.IsRequired,Sequence=q.Sequence,Options=q.Options.OrderBy(o=>o.Sequence).Select(o=>new PublicFormOptionDto{Id=o.Id,OptionCode=o.OptionCode,OptionText=o.OptionText,Sequence=o.Sequence}).ToList()}).ToList(),Audiences=x.Audiences.Where(a=>a.IsActive==true).Select(a=>new PublicFormAudienceDto{Id=a.Id,ScopeType=a.ScopeType,ScopeValue=a.ScopeValue}).ToList()};
    public async Task<List<PublicFormDto>> GetSubmissionFormsAsync(CancellationToken ct = default)
        => await GetManageListAsync(ct);

    public async Task<ServiceResult<PublicFormSubmissionListDto>> GetSubmissionsAsync(int formId, PublicFormSubmissionQueryDto query, string scopeCode, CancellationToken ct = default)
    {
        var form = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.Id == formId, ct);
        if (form == null) return ServiceResult<PublicFormSubmissionListDto>.Fail("Không tìm thấy biểu mẫu.");

        var q = BuildSubmissionQuery(formId, query, scopeCode);
        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var rows = await q.OrderByDescending(x => x.SubmittedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Include(x => x.Answers).ToListAsync(ct);

        var employeeCodes = rows.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => employeeCodes.Contains(x.EmployeeCode)).ToDictionaryAsync(x => x.EmployeeCode, ct);

        return ServiceResult<PublicFormSubmissionListDto>.Ok(new PublicFormSubmissionListDto
        {
            FormId = form.Id, FormCode = form.FormCode, FormTitle = form.Title,
            TotalCount = total, Page = page, PageSize = pageSize,
            Questions = form.Questions.Where(x => x.IsActive == true).OrderBy(x => x.Sequence).Select(MapQuestion).ToList(),
            Items = rows.Select(x => MapSubmission(x, employees, form.Questions)).ToList()
        });
    }

    public async Task<ServiceResult<PublicFormSubmissionSummaryDto>> GetSubmissionSummaryAsync(int formId, PublicFormSubmissionQueryDto query, string scopeCode, CancellationToken ct = default)
    {
        var form = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Include(x => x.Questions).ThenInclude(x => x.Options).FirstOrDefaultAsync(x => x.Id == formId, ct);
        if (form == null) return ServiceResult<PublicFormSubmissionSummaryDto>.Fail("Không tìm thấy biểu mẫu.");

        var rows = await BuildSubmissionQuery(formId, query, scopeCode).AsNoTracking().Include(x => x.Answers).ToListAsync(ct);
        var employeeCodes = rows.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => employeeCodes.Contains(x.EmployeeCode)).ToDictionaryAsync(x => x.EmployeeCode, ct);

        var summary = new PublicFormSubmissionSummaryDto
        {
            FormId = formId,
            TotalSubmissions = rows.Count,
            ByDepartment = rows.GroupBy(x => employees.TryGetValue(x.EmployeeCode, out var e) ? e.DeptCode : string.Empty)
                .OrderByDescending(x => x.Count()).Select(x => new PublicFormDepartmentSummaryDto
                {
                    DeptCode = string.IsNullOrWhiteSpace(x.Key) ? "(Không xác định)" : x.Key, Count = x.Count()
                }).ToList()
        };

        foreach (var question in form.Questions.Where(x => x.IsActive == true).OrderBy(x => x.Sequence))
        {
            if (!string.Equals(question.QuestionType, "SingleChoice", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(question.QuestionType, "MultiChoice", StringComparison.OrdinalIgnoreCase)) continue;

            var counts = question.Options.Where(x => x.IsActive == true)
                .ToDictionary(x => x.OptionCode, _ => 0, StringComparer.OrdinalIgnoreCase);

            foreach (var submission in rows)
            {
                var answer = submission.Answers.FirstOrDefault(x => x.QuestionId == question.Id);
                foreach (var code in SplitChoices(answer?.JsonValue))
                    if (counts.ContainsKey(code)) counts[code]++;
            }

            summary.ByQuestion.Add(new PublicFormQuestionSummaryDto
            {
                QuestionId = question.Id, QuestionCode = question.QuestionCode, QuestionText = question.QuestionText, QuestionType = question.QuestionType,
                Choices = question.Options.Where(x => x.IsActive == true).OrderBy(x => x.Sequence).Select(x => new PublicFormChoiceSummaryDto
                {
                    OptionCode = x.OptionCode, OptionText = x.OptionText,
                    Count = counts.TryGetValue(x.OptionCode, out var n) ? n : 0
                }).ToList()
            });
        }

        return ServiceResult<PublicFormSubmissionSummaryDto>.Ok(summary);
    }

    public async Task<ServiceResult<byte[]>> ExportSubmissionsAsync(int formId, PublicFormSubmissionQueryDto query, string scopeCode, CancellationToken ct = default)
    {
        var form = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Include(x => x.Questions).ThenInclude(x => x.Options).FirstOrDefaultAsync(x => x.Id == formId, ct);
        if (form == null) return ServiceResult<byte[]>.Fail("Không tìm thấy biểu mẫu.");

        var rows = await BuildSubmissionQuery(formId, query, scopeCode).AsNoTracking().Include(x => x.Answers)
            .OrderByDescending(x => x.SubmittedAt).ToListAsync(ct);
        var employeeCodes = rows.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => employeeCodes.Contains(x.EmployeeCode)).ToDictionaryAsync(x => x.EmployeeCode, ct);
        var questions = form.Questions.Where(x => x.IsActive == true).OrderBy(x => x.Sequence).ToList();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add("Đăng ký");
        var headers = new List<string> { "Mã NV", "Họ tên", "Bộ phận", "Thời gian đăng ký", "Trạng thái" };
        headers.AddRange(questions.Select(x => x.QuestionText));
        for (var i = 0; i < headers.Count; i++) sheet.Cell(1, i + 1).Value = headers[i];

        for (var r = 0; r < rows.Count; r++)
        {
            var submission = rows[r];
            employees.TryGetValue(submission.EmployeeCode, out var employee);
            sheet.Cell(r + 2, 1).Value = submission.EmployeeCode;
            sheet.Cell(r + 2, 2).Value = employee?.EmployeeName ?? string.Empty;
            sheet.Cell(r + 2, 3).Value = employee?.DeptCode ?? string.Empty;
            sheet.Cell(r + 2, 4).Value = submission.SubmittedAt;
            sheet.Cell(r + 2, 4).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            sheet.Cell(r + 2, 5).Value = submission.Status;

            for (var q = 0; q < questions.Count; q++)
            {
                var answer = submission.Answers.FirstOrDefault(x => x.QuestionId == questions[q].Id);
                sheet.Cell(r + 2, q + 6).Value = FormatAnswer(answer, questions[q]);
            }
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return ServiceResult<byte[]>.Ok(stream.ToArray());
    }

    private IQueryable<F03PublicFormSubmission> BuildSubmissionQuery(int formId, PublicFormSubmissionQueryDto query, string scopeCode)
    {
        var q = _uow.Repository<F03PublicFormSubmission>().Query()
            .Where(x => x.FormId == formId && !x.IsCancelled);

        var isAll = string.Equals(scopeCode, AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase);
        var isDepartment = string.Equals(scopeCode, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase);
        if (!isAll && !(isDepartment && !string.IsNullOrWhiteSpace(query.DepartmentCode)))
            return q.Where(_ => false);

        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(x => x.Status == query.Status);
        if (query.FromDate.HasValue) q = q.Where(x => x.SubmittedAt >= query.FromDate.Value);
        if (query.ToDate.HasValue) q = q.Where(x => x.SubmittedAt < query.ToDate.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(query.DepartmentCode))
        {
            var employeeCodes = _uow.Repository<F03Employee>().Query()
                .Where(x => x.DeptCode == query.DepartmentCode).Select(x => x.EmployeeCode);
            q = q.Where(x => employeeCodes.Contains(x.EmployeeCode));
        }

        return q;
    }

    private static PublicFormQuestionDto MapQuestion(F03PublicFormQuestion q) => new()
    {
        Id = q.Id, QuestionCode = q.QuestionCode, QuestionText = q.QuestionText, QuestionType = q.QuestionType,
        HelpText = q.HelpText, Placeholder = q.Placeholder, IsRequired = q.IsRequired, Sequence = q.Sequence,
        Options = q.Options.Where(x => x.IsActive == true).OrderBy(x => x.Sequence).Select(x => new PublicFormOptionDto
        {
            Id = x.Id, OptionCode = x.OptionCode, OptionText = x.OptionText, Sequence = x.Sequence
        }).ToList()
    };

    private static PublicFormSubmissionRowDto MapSubmission(F03PublicFormSubmission submission, Dictionary<string, F03Employee> employees, IEnumerable<F03PublicFormQuestion> questions)
    {
        employees.TryGetValue(submission.EmployeeCode, out var employee);
        return new PublicFormSubmissionRowDto
        {
            SubmissionId = submission.Id, EmployeeCode = submission.EmployeeCode, EmployeeName = employee?.EmployeeName ?? string.Empty,
            DeptCode = employee?.DeptCode ?? string.Empty, SubmittedAt = submission.SubmittedAt, Status = submission.Status,
            Answers = questions.Where(x => x.IsActive == true).OrderBy(x => x.Sequence).Select(q => new PublicFormSubmissionAnswerDto
            {
                QuestionId = q.Id, QuestionCode = q.QuestionCode, QuestionText = q.QuestionText, QuestionType = q.QuestionType,
                Value = FormatAnswer(submission.Answers.FirstOrDefault(a => a.QuestionId == q.Id), q)
            }).ToList()
        };
    }

    private static string FormatAnswer(F03PublicFormAnswer? answer, F03PublicFormQuestion question)
    {
        if (answer == null) return string.Empty;
        if (string.Equals(question.QuestionType, "SingleChoice", StringComparison.OrdinalIgnoreCase)
            || string.Equals(question.QuestionType, "MultiChoice", StringComparison.OrdinalIgnoreCase))
        {
            var codes = SplitChoices(answer.JsonValue);
            var text = question.Options.Where(x => codes.Contains(x.OptionCode, StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => x.Sequence).Select(x => x.OptionText).ToList();
            return text.Count > 0 ? string.Join(", ", text) : string.Join(", ", codes);
        }

        if (answer.BoolValue.HasValue) return answer.BoolValue.Value ? "Có" : "Không";
        if (answer.NumberValue.HasValue) return answer.NumberValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (answer.DateValue.HasValue) return answer.DateValue.Value.ToString("dd/MM/yyyy HH:mm");
        return answer.TextValue ?? answer.JsonValue ?? string.Empty;
    }

    private static string[] SplitChoices(string? value) => string.IsNullOrWhiteSpace(value)
        ? Array.Empty<string>()
        : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

}
