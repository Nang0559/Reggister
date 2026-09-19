# HRM-compatible Attendance Calculation

FVN_REGISTER does not call the HRM UI and does not write HRM calculated tables.

The operation HRM → Tính giờ reads the HRM raw/configuration tables and executes a port of HRM.dbo.sphrmvn_TimeKeepingForStaff from SQL/HRM/HRMSP3.sql.

Output: dbo.F03HrmAttendanceCalculated and dbo.F03HrmOTActual. Each run creates a new CalculationBatchId. The report view selects the latest calculation for each employee/day.

Parity keeps shift/break handling, multiple swipes, day/night split, leave, privileges, thresholds, late/early and OT logic. HRM rounding is preserved as CEILING(ROUND(minutes / unit, 1)) * unit.

Validation must compare BCTGVao/BCTGVe, BCTGLamNgay/BCTGLamToi, BCTGThemNgay/BCTGThemToi, late/early, leave fields and BCTGQuyDinh against HRM.tblBaoCao for day/night/leave/OT/missing-swipe cases.
