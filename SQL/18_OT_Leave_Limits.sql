USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    OT policy baseline.
    Weekly is intentionally NOT seeded because the repository does not contain
    a confirmed company-wide weekly cap. Configure it explicitly when the
    internal policy defines one.

    LimitHours is the canonical value consumed by OTBalance/OTValidator.
    LimitValue remains a compatibility field for older data.
*/

INSERT dbo.F03OTLimitRules
    (IsActive, CreatedBy, LimitType, LimitValue, PositionCode, DeptCode, LimitHours, Description)
SELECT 1, 0, v.LimitType, v.LimitHours, NULL, NULL, v.LimitHours, v.Description
FROM (VALUES
    (N'Monthly', CAST(40.00 AS decimal(5,2)), N'OT chuẩn tối đa 40 giờ/tháng'),
    (N'Yearly',  CAST(200.00 AS decimal(5,2)), N'OT chuẩn tối đa 200 giờ/năm'),
    (N'Special', CAST(300.00 AS decimal(5,2)), N'Ngưỡng tối đa năm theo policy đặc biệt')
) v(LimitType, LimitHours, Description)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.F03OTLimitRules r
    WHERE r.IsActive = 1
      AND r.LimitType = v.LimitType
      AND r.PositionCode IS NULL
      AND r.DeptCode IS NULL
);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_OTLimitRule_ActiveScope'
      AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
)
BEGIN
    CREATE UNIQUE INDEX UX_OTLimitRule_ActiveScope
        ON dbo.F03OTLimitRules(LimitType, DeptCode, PositionCode)
        WHERE IsActive = 1;
END;
GO

PRINT N'OT limit baseline installed: Monthly=40h, Yearly=200h, Special=300h; Weekly remains configurable.';
GO
