using FVN_REGISTER.Application.Interfaces.PublicForms;
using FVN_REGISTER.Contract.Dtos.PublicForms;
using FVN_REGISTER.Contract.Requests.PublicForms;
using FVN_REGISTER.Core.Entities.PublicForms;
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
            .Include(x => x.Audiences).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<List<PublicFormDto>> GetAvailableAsync(string employeeCode, string? deptCode, string? positionCode, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var rows = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Where(x => x.IsActive && x.Status == "Published"
                && (!x.StartAt.HasValue || x.StartAt <= now)
                && (!x.EndAt.HasValue || x.EndAt >= now))
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .Include(x => x.Audiences).ToListAsync(ct);
        return rows.Where(x => Matches(x, employeeCode, deptCode, positionCode)).Select(Map).ToList();
    }

    public async Task<PublicFormDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var x = await _uow.Repository<F03PublicForm>().Query().AsNoTracking()
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .Include(x => x.Audiences).FirstOrDefaultAsync(x => x.Id == id, ct);
        return x == null ? null : Map(x);
    }

    public async Task<Contract.Responses.ServiceResult<PublicFormDto>> CreateAsync(SavePublicFormRequest request, int actorUserId, CancellationToken ct = default)
    {
        Validate(request);
        var exists = await _uow.Repository<F03PublicForm>().Query().AnyAsync(x => x.FormCode == request.FormCode.Trim(), ct);
        if (exists) return Contract.Responses.ServiceResult<PublicFormDto>.Fail("Mã biểu mẫu đã tồn tại.");
        var entity = BuildEntity(request, actorUserId);
        await _uow.Repository<F03PublicForm>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Contract.Responses.ServiceResult<PublicFormDto>.Ok(Map(entity));
    }

    public async Task<Contract.Responses.ServiceResult<PublicFormDto>> UpdateAsync(int id, SavePublicFormRequest request, int actorUserId, CancellationToken ct = default)
    {
        Validate(request);
        var entity = await _uow.Repository<F03PublicForm>().Query().Include(x=>x.Questions).ThenInclude(x=>x.Options).Include(x=>x.Audiences).FirstOrDefaultAsync(x=>x.Id==id,ct);
        if (entity == null) return Contract.Responses.ServiceResult<PublicFormDto>.Fail("Không tìm thấy biểu mẫu.");
        if (entity.Status == "Published") return Contract.Responses.ServiceResult<PublicFormDto>.Fail("Không sửa trực tiếp biểu mẫu đã Publish.");
        entity.FormCode=request.FormCode.Trim(); entity.Title=request.Title.Trim(); entity.Description=request.Description?.Trim(); entity.CategoryCode=request.CategoryCode?.Trim();
        entity.StartAt=request.StartAt; entity.EndAt=request.EndAt; entity.AllowMultipleSubmit=request.AllowMultipleSubmit; entity.RequireApproval=request.RequireApproval; entity.MaxSubmissions=request.MaxSubmissions;
        entity.ModifiedBy=actorUserId; entity.ModifiedAt=DateTime.Now;
        entity.Questions.Clear(); entity.Audiences.Clear();
        AddChildren(entity,request);
        await _uow.SaveChangesAsync(ct);
        return Contract.Responses.ServiceResult<PublicFormDto>.Ok(Map(entity));
    }

    public async Task<Contract.Responses.ServiceResult> PublishAsync(int id,int actorUserId,CancellationToken ct=default)
    {
        var e=await _uow.Repository<F03PublicForm>().Query().Include(x=>x.Questions).Include(x=>x.Audiences).FirstOrDefaultAsync(x=>x.Id==id,ct);
        if(e==null)return Contract.Responses.ServiceResult.Fail("Không tìm thấy biểu mẫu.");
        if(e.Status=="Archived")return Contract.Responses.ServiceResult.Fail("Biểu mẫu đã Archive.");
        if(e.Questions.Count==0)return Contract.Responses.ServiceResult.Fail("Biểu mẫu phải có ít nhất một câu hỏi.");
        if(e.Audiences.Count==0)return Contract.Responses.ServiceResult.Fail("Biểu mẫu phải có đối tượng đăng ký.");
        e.Status="Published";e.PublishedAt=DateTime.Now;e.ModifiedBy=actorUserId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);return Contract.Responses.ServiceResult.Ok();
    }

    public async Task<Contract.Responses.ServiceResult> CloseAsync(int id,int actorUserId,CancellationToken ct=default)
    {
        var e=await _uow.Repository<F03PublicForm>().Query().FirstOrDefaultAsync(x=>x.Id==id,ct);
        if(e==null)return Contract.Responses.ServiceResult.Fail("Không tìm thấy biểu mẫu.");
        e.Status="Closed";e.ClosedAt=DateTime.Now;e.ModifiedBy=actorUserId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);return Contract.Responses.ServiceResult.Ok();
    }

    public async Task<Contract.Responses.ServiceResult<int>> SubmitAsync(int formId,string employeeCode,string? deptCode,string? positionCode,IReadOnlyCollection<Contract.Requests.PublicForms.PublicFormAnswerRequest> answers,CancellationToken ct=default)
    {
        var now=DateTime.Now;
        var form=await _uow.Repository<F03PublicForm>().Query()
            .Include(x=>x.Questions).ThenInclude(x=>x.Options)
            .Include(x=>x.Audiences)
            .FirstOrDefaultAsync(x=>x.Id==formId,ct);
        if(form==null) return Contract.Responses.ServiceResult<int>.Fail("Không tìm thấy biểu mẫu.");
        if(form.Status!="Published" || (form.StartAt.HasValue&&form.StartAt>now) || (form.EndAt.HasValue&&form.EndAt<now))
            return Contract.Responses.ServiceResult<int>.Fail("Biểu mẫu không còn nhận đăng ký.");
        if(!Matches(form,employeeCode,deptCode,positionCode))
            return Contract.Responses.ServiceResult<int>.Fail("Bạn không thuộc đối tượng được phép đăng ký.");
        if(!form.AllowMultipleSubmit && await _uow.Repository<F03PublicFormSubmission>().Query().AnyAsync(x=>x.FormId==formId&&x.EmployeeCode==employeeCode&&x.Status!="Cancelled",ct))
            return Contract.Responses.ServiceResult<int>.Fail("Bạn đã đăng ký biểu mẫu này.");
        var required=form.Questions.Where(q=>q.IsRequired&&q.IsActive).Select(q=>q.Id).ToHashSet();
        var provided=answers.Select(x=>x.QuestionId).ToHashSet();
        if(required.Any(id=>!provided.Contains(id))) return Contract.Responses.ServiceResult<int>.Fail("Vui lòng hoàn tất các câu hỏi bắt buộc.");
        foreach(var a in answers)
        {
            var q=form.Questions.FirstOrDefault(x=>x.Id==a.QuestionId&&x.IsActive);
            if(q==null) return Contract.Responses.ServiceResult<int>.Fail("Có câu hỏi không hợp lệ.");
            if((q.QuestionType=="SingleChoice"||q.QuestionType=="MultiChoice") && !string.IsNullOrWhiteSpace(a.JsonValue))
            {
                var selected=a.JsonValue.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
                if(selected.Any(v=>!q.Options.Any(o=>o.IsActive&&o.OptionCode==v))) return Contract.Responses.ServiceResult<int>.Fail($"Lựa chọn không hợp lệ cho {q.QuestionCode}.");
            }
        }
        var sub=new F03PublicFormSubmission{FormId=formId,EmployeeCode=employeeCode,SubmittedAt=now,Status="Submitted",FormVersion=form.Version};
        foreach(var a in answers) sub.Answers.Add(new F03PublicFormAnswer{QuestionId=a.QuestionId,TextValue=a.TextValue,NumberValue=a.NumberValue,DateValue=a.DateValue,BoolValue=a.BoolValue,JsonValue=a.JsonValue});
        await _uow.Repository<F03PublicFormSubmission>().AddAsync(sub,ct);
        await _uow.SaveChangesAsync(ct);
        return Contract.Responses.ServiceResult<int>.Ok(sub.Id);
    }

    private static bool Matches(F03PublicForm x,string employeeCode,string? deptCode,string? positionCode)
        => x.Audiences.Any(a => a.IsActive && a.ScopeType=="AllCompany"
            || a.IsActive && a.ScopeType=="Employee" && string.Equals(a.ScopeValue,employeeCode,StringComparison.OrdinalIgnoreCase)
            || a.IsActive && a.ScopeType=="Department" && !string.IsNullOrWhiteSpace(deptCode) && string.Equals(a.ScopeValue,deptCode,StringComparison.OrdinalIgnoreCase)
            || a.IsActive && a.ScopeType=="Position" && !string.IsNullOrWhiteSpace(positionCode) && string.Equals(a.ScopeValue,positionCode,StringComparison.OrdinalIgnoreCase));

    private static void Validate(SavePublicFormRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.FormCode)||string.IsNullOrWhiteSpace(r.Title)) throw new ArgumentException("Mã và tiêu đề biểu mẫu là bắt buộc.");
        if (r.EndAt.HasValue && r.StartAt.HasValue && r.EndAt < r.StartAt) throw new ArgumentException("EndAt phải lớn hơn hoặc bằng StartAt.");
        if (r.Questions.Count==0) throw new ArgumentException("Biểu mẫu phải có ít nhất một câu hỏi.");
        foreach(var q in r.Questions)
        {
            if(!QuestionTypes.Contains(q.QuestionType)) throw new ArgumentException($"QuestionType không hợp lệ: {q.QuestionType}");
            if((q.QuestionType.Equals("SingleChoice",StringComparison.OrdinalIgnoreCase)||q.QuestionType.Equals("MultiChoice",StringComparison.OrdinalIgnoreCase))&&!q.Options.Any())
                throw new ArgumentException($"Câu hỏi {q.QuestionCode} phải có lựa chọn.");
        }
        foreach(var a in r.Audiences)
        {
            if(!AudienceTypes.Contains(a.ScopeType)) throw new ArgumentException($"ScopeType không hợp lệ: {a.ScopeType}");
            if(!a.ScopeType.Equals("AllCompany",StringComparison.OrdinalIgnoreCase)&&string.IsNullOrWhiteSpace(a.ScopeValue))
                throw new ArgumentException($"ScopeValue bắt buộc cho {a.ScopeType}.");
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
    private static PublicFormDto Map(F03PublicForm x)=>new(){Id=x.Id,FormCode=x.FormCode,Title=x.Title,Description=x.Description,CategoryCode=x.CategoryCode,Status=x.Status,StartAt=x.StartAt,EndAt=x.EndAt,AllowMultipleSubmit=x.AllowMultipleSubmit,RequireApproval=x.RequireApproval,MaxSubmissions=x.MaxSubmissions,Version=x.Version,IsActive=x.IsActive,Questions=x.Questions.OrderBy(q=>q.Sequence).Select(q=>new PublicFormQuestionDto{Id=q.Id,QuestionCode=q.QuestionCode,QuestionText=q.QuestionText,QuestionType=q.QuestionType,HelpText=q.HelpText,Placeholder=q.Placeholder,IsRequired=q.IsRequired,Sequence=q.Sequence,Options=q.Options.OrderBy(o=>o.Sequence).Select(o=>new PublicFormOptionDto{Id=o.Id,OptionCode=o.OptionCode,OptionText=o.OptionText,Sequence=o.Sequence}).ToList()}).ToList(),Audiences=x.Audiences.Where(a=>a.IsActive).Select(a=>new PublicFormAudienceDto{Id=a.Id,ScopeType=a.ScopeType,ScopeValue=a.ScopeValue}).ToList()};
}