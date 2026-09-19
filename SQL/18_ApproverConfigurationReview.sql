/*
  HRM -> Approver review. HRM never overwrites F03Approvers.
*/
IF COL_LENGTH('dbo.F03SyncReviewFlags','CurrentApproverId') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD CurrentApproverId INT NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','OldDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD OldDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','OldPositionCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD OldPositionCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','NewDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD NewDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','NewPositionCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD NewPositionCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','CurrentApproverCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD CurrentApproverCode NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','CurrentLevel') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD CurrentLevel INT NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','CurrentRoleName') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD CurrentRoleName NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','CurrentApproveForDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD CurrentApproveForDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','SuggestedApproverCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD SuggestedApproverCode NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','SuggestedLevel') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD SuggestedLevel INT NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','SuggestedRoleName') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD SuggestedRoleName NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','SuggestedApproveForDeptCode') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD SuggestedApproveForDeptCode NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.F03SyncReviewFlags','Decision') IS NULL ALTER TABLE dbo.F03SyncReviewFlags ADD Decision NVARCHAR(50) NULL;
