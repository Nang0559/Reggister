namespace FVN_REGISTER.Contract.Dtos.EquipmentImport;

public sealed class EquipmentSchemaDto
{
    public int Id { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<EquipmentFieldDefinitionDto> Fields { get; set; } = new();
}

public sealed class EquipmentSchemaSummaryDto
{
    public int Id { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int FieldCount { get; set; }
}

public sealed class EquipmentSchemaUpsertRequest
{
    public int? Id { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
}

public sealed class EquipmentSchemaCloneRequest
{
    public string? SchemaName { get; set; }
    public string Status { get; set; } = "Draft";
}
