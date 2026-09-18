using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.EquipmentImport;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Equipment;

public sealed class EquipmentImportService : IEquipmentImportService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;
    private readonly IAuditService _audit;

    public EquipmentImportService(IUnitOfWork uow, ICurrentUserService currentUser, IAuthorizationService authorization, IAuditService audit)
    { _uow = uow; _currentUser = currentUser; _authorization = authorization; _audit = audit; }

    public async Task<List<EquipmentFieldDefinitionDto>> GetFieldDefinitionsAsync(string deptCode, CancellationToken ct = default)
    {
        var user = RequireUser(); await EnsureScopeAsync(user, deptCode, ct);
        return await _uow.Repository<F03EquipmentFieldDefinition>().Query().AsNoTracking()
            .Where(x => x.DeptCode == deptCode && x.IsActive == true && x.IsActiveField)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.FieldLabel)
            .Select(x => new EquipmentFieldDefinitionDto { Id=x.Id, DeptCode=x.DeptCode, FieldKey=x.FieldKey, FieldLabel=x.FieldLabel, DataType=x.DataType, IsRequired=x.IsRequired, IsImportable=x.IsImportable, IsSearchable=x.IsSearchable, IsActiveField=x.IsActiveField, DisplayOrder=x.DisplayOrder, OptionsJson=x.OptionsJson }).ToListAsync(ct);
    }

    public async Task<EquipmentFieldDefinitionDto> SaveFieldDefinitionAsync(SaveEquipmentFieldDefinitionRequest request, CancellationToken ct = default)
    {
        var user=RequireUser(); await EnsureScopeAsync(user, request.DeptCode, ct);
        var dept=request.DeptCode.Trim(); var key=NormalizeKey(request.FieldKey);
        if(string.IsNullOrWhiteSpace(dept)||string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Bộ phận và mã trường là bắt buộc.");
        var types=new[]{"Text","Number","Date","Boolean","Choice"};
        if(!types.Contains(request.DataType,StringComparer.OrdinalIgnoreCase)) throw new ArgumentException("DataType không được hỗ trợ.");
        var e=await _uow.Repository<F03EquipmentFieldDefinition>().Query().FirstOrDefaultAsync(x=>x.DeptCode==dept&&x.FieldKey==key,ct);
        if(e==null){e=new F03EquipmentFieldDefinition{DeptCode=dept,FieldKey=key,CreatedBy=user.UserId};await _uow.Repository<F03EquipmentFieldDefinition>().AddAsync(e,ct);}
        e.FieldLabel=request.FieldLabel.Trim();e.DataType=request.DataType.Trim();e.IsRequired=request.IsRequired;e.IsImportable=request.IsImportable;e.IsSearchable=request.IsSearchable;e.IsActiveField=request.IsActiveField;e.DisplayOrder=request.DisplayOrder;e.OptionsJson=request.OptionsJson;e.ModifiedBy=user.UserId;e.ModifiedAt=DateTime.Now;
        await _uow.SaveChangesAsync(ct);
        return new EquipmentFieldDefinitionDto{Id=e.Id,DeptCode=e.DeptCode,FieldKey=e.FieldKey,FieldLabel=e.FieldLabel,DataType=e.DataType,IsRequired=e.IsRequired,IsImportable=e.IsImportable,IsSearchable=e.IsSearchable,IsActiveField=e.IsActiveField,DisplayOrder=e.DisplayOrder,OptionsJson=e.OptionsJson};
    }

    public async Task<EquipmentImportBatchDto> StageExcelAsync(string deptCode,string fileName,Stream content,CancellationToken ct=default)
    {
        var user=RequireUser();await EnsureScopeAsync(user,deptCode,ct);
        if(content==null||!content.CanRead) throw new ArgumentException("File Excel không hợp lệ.");
        if(!string.Equals(Path.GetExtension(fileName),".xlsx",StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Chỉ hỗ trợ Excel .xlsx.");
        if(content.CanSeek)content.Position=0;
        using var wb=new XLWorkbook(content);var ws=wb.Worksheets.FirstOrDefault()??throw new InvalidOperationException("File Excel không có sheet.");
        var header=ws.FirstRowUsed()??throw new InvalidOperationException("File Excel không có dữ liệu.");
        var first=header.RowNumber();var last=ws.LastRowUsed()?.RowNumber()??first;var lastCol=ws.LastColumnUsed()?.ColumnNumber()??0;
        if(lastCol==0||last<=first)throw new InvalidOperationException("Excel phải có tiêu đề và ít nhất một dòng dữ liệu.");
        var headers=Enumerable.Range(1,lastCol).Select(i=>ws.Cell(first,i).GetString().Trim()).ToList();
        if(headers.Any(string.IsNullOrWhiteSpace))throw new InvalidOperationException("Không được để trống tên cột Excel.");
        var defs=await _uow.Repository<F03EquipmentFieldDefinition>().Query().AsNoTracking().Where(x=>x.DeptCode==deptCode&&x.IsActive==true&&x.IsActiveField&&x.IsImportable).ToListAsync(ct);
        var batch=new F03EquipmentImportBatch{DeptCode=deptCode.Trim(),FileName=Path.GetFileName(fileName),Status="Staged",CreatedBy=user.UserId};
        await _uow.Repository<F03EquipmentImportBatch>().AddAsync(batch,ct);await _uow.SaveChangesAsync(ct);
        var rows=new List<F03EquipmentImportRow>();var valid=0;var invalid=0;
        for(var rowNo=first+1;rowNo<=last;rowNo++){ct.ThrowIfCancellationRequested();var data=new Dictionary<string,string?>(StringComparer.OrdinalIgnoreCase);for(var col=1;col<=headers.Count;col++)data[headers[col-1]]=ws.Cell(rowNo,col).GetFormattedString().Trim();if(data.Values.All(string.IsNullOrWhiteSpace))continue;var err=ValidateRow(data,defs);var status=err==null?"Valid":"Invalid";if(err==null)valid++;else invalid++;rows.Add(new F03EquipmentImportRow{BatchId=batch.Id,RowNumber=rowNo,RawJson=JsonSerializer.Serialize(data),Status=status,ErrorMessage=err,CreatedBy=user.UserId});}
        foreach(var row in rows)await _uow.Repository<F03EquipmentImportRow>().AddAsync(row,ct);
        batch.TotalRows=rows.Count;batch.ValidRows=valid;batch.InvalidRows=invalid;batch.Status=invalid==0?"Ready":"NeedsReview";await _uow.SaveChangesAsync(ct);await _audit.LogAction("EQUIPMENT_IMPORT_STAGED",user.UserId,$"BatchId={batch.Id}; DeptCode={batch.DeptCode}; FileName={batch.FileName}; Rows={batch.TotalRows}; Valid={batch.ValidRows}; Invalid={batch.InvalidRows}",ct:ct);return MapBatch(batch);
    }

    public async Task<EquipmentImportBatchDto?> GetBatchAsync(int batchId,CancellationToken ct=default)
    {
        var user=RequireUser();var b=await _uow.Repository<F03EquipmentImportBatch>().Query().AsNoTracking().FirstOrDefaultAsync(x=>x.Id==batchId&&x.IsActive==true,ct);if(b==null)return null;await EnsureScopeAsync(user,b.DeptCode,ct);return MapBatch(b);
    }

    public async Task<EquipmentImportCommitResultDto> CommitAsync(int batchId,CancellationToken ct=default)
    {
        var user=RequireUser();var b=await _uow.Repository<F03EquipmentImportBatch>().Query().FirstOrDefaultAsync(x=>x.Id==batchId&&x.IsActive==true,ct)??throw new KeyNotFoundException("Không tìm thấy lô import.");
        await EnsureScopeAsync(user,b.DeptCode,ct);if(b.Status=="Imported")return new(){BatchId=b.Id,ImportedRows=b.ImportedRows};if(b.Status!="Ready")throw new InvalidOperationException("Lô import chưa sẵn sàng. Hãy xử lý các dòng lỗi trước.");
        var rows=await _uow.Repository<F03EquipmentImportRow>().Query().Where(x=>x.BatchId==b.Id&&x.Status=="Valid").OrderBy(x=>x.RowNumber).ToListAsync(ct);
        var defs=await _uow.Repository<F03EquipmentFieldDefinition>().Query().AsNoTracking().Where(x=>x.DeptCode==b.DeptCode&&x.IsActive==true&&x.IsActiveField&&x.IsImportable).ToListAsync(ct);var imported=0;
        foreach(var row in rows){var data=JsonSerializer.Deserialize<Dictionary<string,string?>>(row.RawJson)??new();var code=GetValue(data,"EquipmentCode","Mã thiết bị","Mã TB","Mã tài sản","AssetCode");var name=GetValue(data,"EquipmentName","Tên thiết bị","Tên TB","Tên tài sản");if(string.IsNullOrWhiteSpace(code)||string.IsNullOrWhiteSpace(name)){row.Status="Skipped";row.ErrorMessage="Thiếu mã hoặc tên thiết bị.";continue;}var exists=await _uow.Repository<F03EquipmentAsset>().Query().FirstOrDefaultAsync(x=>x.EquipmentCode==code&&x.DeptCode==b.DeptCode,ct);if(exists!=null){row.Status="Skipped";row.ErrorMessage="Mã thiết bị đã tồn tại trong bộ phận.";continue;}
            var asset=new F03EquipmentAsset{EquipmentCode=code,EquipmentName=name,Specification=GetValue(data,"Specification","Thông số","Thông số kỹ thuật"),SerialNumber=GetValue(data,"SerialNumber","Serial","Số serial","S/N"),AssetCode=GetValue(data,"AssetCode","Mã tài sản"),PurchasePrice=ParseDecimal(GetValue(data,"PurchasePrice","Nguyên giá","Giá mua","Giá")),PurchaseDate=ParseDate(GetValue(data,"PurchaseDate","Ngày mua","Ngày nhập"))??DateTime.Today,ExpectedDepreciationDate=ParseDate(GetValue(data,"ExpectedDepreciationDate","Ngày khấu hao","Ngày hết khấu hao"))??DateTime.Today,DeptCode=b.DeptCode,Location=GetValue(data,"Location","Vị trí","Địa điểm"),QrToken=Convert.ToHexString(Guid.NewGuid().ToByteArray())+Guid.NewGuid().ToString("N"),IsQrActive=true,Note=GetValue(data,"Note","Ghi chú"),CustomDataJson=JsonSerializer.Serialize(BuildCustomData(data,defs)),CreatedBy=user.UserId};
            await _uow.Repository<F03EquipmentAsset>().AddAsync(asset,ct);await _uow.SaveChangesAsync(ct);row.AssetId=asset.Id;row.Status="Imported";imported=imported+1;
        }
        b.ImportedRows=imported;b.Status="Imported";b.CompletedAt=DateTime.Now;b.ModifiedBy=user.UserId;b.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);var skipped=rows.Count(x=>x.Status=="Skipped");await _audit.LogAction("EQUIPMENT_IMPORT_COMMITTED",user.UserId,$"BatchId={b.Id}; DeptCode={b.DeptCode}; Imported={imported}; Skipped={skipped}",ct:ct);return new(){BatchId=b.Id,ImportedRows=imported,SkippedRows=skipped};
    }

    private async Task EnsureScopeAsync(FVN_REGISTER.Contract.Dtos.Authentication.UserIdentityDto user,string deptCode,CancellationToken ct)
    {if(string.IsNullOrWhiteSpace(deptCode))throw new ArgumentException("Bộ phận là bắt buộc.");if(!await _authorization.CanAccessAsync(user,SecurityFunctionCodes.EquipmentImport,null,deptCode.Trim(),ct))throw new UnauthorizedAccessException("Bạn không có quyền import thiết bị cho bộ phận này.");}
    private FVN_REGISTER.Contract.Dtos.Authentication.UserIdentityDto RequireUser()=>_currentUser.GetCurrentUser()??throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
    private static EquipmentImportBatchDto MapBatch(F03EquipmentImportBatch x)=>new(){Id=x.Id,DeptCode=x.DeptCode,FileName=x.FileName,Status=x.Status,TotalRows=x.TotalRows,ValidRows=x.ValidRows,InvalidRows=x.InvalidRows,ImportedRows=x.ImportedRows};
    private static string? ValidateRow(Dictionary<string,string?> data,List<F03EquipmentFieldDefinition> defs){var code=GetValue(data,"EquipmentCode","Mã thiết bị","Mã TB","Mã tài sản","AssetCode");var name=GetValue(data,"EquipmentName","Tên thiết bị","Tên TB","Tên tài sản");if(string.IsNullOrWhiteSpace(code))return"Thiếu mã thiết bị.";if(string.IsNullOrWhiteSpace(name))return"Thiếu tên thiết bị.";foreach(var d in defs.Where(x=>x.IsRequired)){var v=GetValue(data,d.FieldKey,d.FieldLabel);if(string.IsNullOrWhiteSpace(v))return$"Thiếu trường bắt buộc: {d.FieldLabel}.";if(!ValidateType(v,d.DataType))return$"Sai kiểu dữ liệu: {d.FieldLabel} ({d.DataType}).";}return null;}
    private static bool ValidateType(string value,string type)=>type.ToLowerInvariant() switch{"number"=>decimal.TryParse(value,NumberStyles.Any,CultureInfo.InvariantCulture,out _)||decimal.TryParse(value,NumberStyles.Any,new CultureInfo("vi-VN"),out _),"date"=>ParseDate(value).HasValue,"boolean"=>bool.TryParse(value,out _)||value is "0" or "1" or "Có" or "Không" or "Yes" or "No",_=>true};
    private static Dictionary<string,string?> BuildCustomData(Dictionary<string,string?> source,List<F03EquipmentFieldDefinition> defs){var r=new Dictionary<string,string?>(StringComparer.OrdinalIgnoreCase);foreach(var p in source){var k=defs.FirstOrDefault(x=>NormalizeKey(x.FieldKey)==NormalizeKey(p.Key)||NormalizeKey(x.FieldLabel)==NormalizeKey(p.Key))?.FieldKey;if(!string.IsNullOrWhiteSpace(k))r[k]=p.Value;else if(!IsStandardColumn(p.Key))r[NormalizeKey(p.Key)]=p.Value;}return r;}
    private static bool IsStandardColumn(string v)=>new[]{"equipmentcode","equipmentname","specification","serialnumber","assetcode","purchaseprice","purchasedate","expecteddepreciationdate","deptcode","location","note","matb","mattb","matasan","tenthietbi","tentb","thongso","thongsokythuat","serial","soserial","sn","ngaymua","ngaynhap","ngaykhauhao","ngayhethauhao","nguyengia","giamua","gia","vitri","diadiem","ghichu"}.Contains(NormalizeKey(v));
    private static string? GetValue(Dictionary<string,string?> data,params string[] names){foreach(var n in names){var h=data.FirstOrDefault(x=>NormalizeKey(x.Key)==NormalizeKey(n));if(!string.IsNullOrWhiteSpace(h.Key)&&!string.IsNullOrWhiteSpace(h.Value))return h.Value.Trim();}return null;}
    private static decimal ParseDecimal(string? v){if(string.IsNullOrWhiteSpace(v))return 0;if(decimal.TryParse(v,NumberStyles.Any,CultureInfo.InvariantCulture,out var d))return d;return decimal.TryParse(v,NumberStyles.Any,new CultureInfo("vi-VN"),out d)?d:0;}
    private static DateTime? ParseDate(string? v){if(string.IsNullOrWhiteSpace(v))return null;var f=new[]{"dd/MM/yyyy","d/M/yyyy","yyyy-MM-dd","MM/dd/yyyy","M/d/yyyy"};if(DateTime.TryParseExact(v.Trim(),f,CultureInfo.InvariantCulture,DateTimeStyles.None,out var d))return d;return DateTime.TryParse(v,new CultureInfo("vi-VN"),DateTimeStyles.None,out d)?d:null;}
    private static string NormalizeKey(string v){var d=v.Trim().Normalize(NormalizationForm.FormD);var sb=new StringBuilder(d.Length);foreach(var c in d)if(System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)!=System.Globalization.UnicodeCategory.NonSpacingMark)sb.Append(char.ToLowerInvariant(c));return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC),@"[^a-z0-9]+","");}
}