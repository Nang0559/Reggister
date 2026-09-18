USE [FVN_REGISTER];
GO
SELECT s.name AS [SchemaName],o.name AS [ObjectName],o.type_desc AS [Type]
FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
WHERE o.is_ms_shipped=0
ORDER BY s.name,o.type_desc,o.name;

SELECT fk.name,OBJECT_SCHEMA_NAME(fk.parent_object_id) ParentSchema,
       OBJECT_NAME(fk.parent_object_id) ParentTable,
       OBJECT_SCHEMA_NAME(fk.referenced_object_id) RefSchema,
       OBJECT_NAME(fk.referenced_object_id) RefTable
FROM sys.foreign_keys fk
ORDER BY fk.name;
GO
