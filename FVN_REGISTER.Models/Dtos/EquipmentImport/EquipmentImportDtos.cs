namespace FVN_REGISTER.Contract.Dtos.EquipmentImport;

public sealed class EquipmentFieldDefinitionDto
{
    public int Id { get; set; }
    public int SchemaId { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsImportable { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsActiveField { get; set; }
    public int DisplayOrder { get; set; }
    public int? MaxLength { get; set; }
    public string? DefaultValue { get; set; }
    public string? OptionsJson { get; set; }
}

public sealed class SaveEquipmentFieldDefinitionRequest
{
    public string DeptCode { get; set; } = string.Empty;
    public int? SchemaId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsImportable { get; set; } = true;
    public bool IsSearchable { get; set; }
    public bool IsActiveField { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? MaxLength { get; set; }
    public string? DefaultValue { get; set; }
    public string? OptionsJson { get; set; }
}

public sealed class EquipmentImportBatchDto
{
    public int Id { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int ImportedRows { get; set; }
    public bool AssignToEmployee { get; set; }
    public List<EquipmentImportRowDto> Rows { get; set; } = new();
}

public sealed class EquipmentImportRowDto
{
    public int RowNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public sealed class EquipmentImportCommitResultDto
{
    public int BatchId { get; set; }
    public int ImportedRows { get; set; }
    public int SkippedRows { get; set; }
}
