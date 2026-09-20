/*
  HRM -> Approver review. HRM never overwrites F03Approvers.
*/
IF COL_LENGTH('dbo.F03SyncReviewFlag','CurrentApproverId') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD CurrentApproverId INT NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','OldDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD OldDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','OldPositionCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD OldPositionCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','NewDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD NewDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','NewPositionCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD NewPositionCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','CurrentApproverCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD CurrentApproverCode NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','CurrentLevel') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD CurrentLevel INT NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','CurrentRoleName') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD CurrentRoleName NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','CurrentApproveForDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD CurrentApproveForDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','SuggestedApproverCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD SuggestedApproverCode NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','SuggestedLevel') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD SuggestedLevel INT NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','SuggestedRoleName') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD SuggestedRoleName NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','SuggestedApproveForDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD SuggestedApproveForDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlag','Decision') IS NULL ALTER TABLE dbo.F03SyncReviewFlag ADD Decision NVARCHAR(50) NULL;
