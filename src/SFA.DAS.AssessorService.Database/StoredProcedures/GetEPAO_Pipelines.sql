CREATE PROCEDURE [GetEPAO_Pipelines]
    -- Add the parameters for the stored procedure here
      @EPAOID NVARCHAR(12)
AS
BEGIN    
-- check for all pipeline for EPAO 

WITH Data_CTE 
AS
(
    SELECT StdCode, Title, 
    COUNT(*) Pipeline, EstimateDate
    FROM (
    -- The active records from ilr
    SELECT il.Uln, il.StdCode, LearnStartDate, PlannedEndDate,il.UpdatedAt
	,CASE WHEN PlannedEndDate > GETDATE() THEN EOMONTH(PlannedEndDate) ELSE EOMONTH(DATEADD(month, std.Duration, LearnStartDate)) END AS EstimateDate , Title
    FROM Ilrs il
    JOIN OrganisationStandard os ON il.EpaOrgId = os.EndPointAssessorOrganisationId AND il.StdCode = os.StandardCode AND 
         os.[Status] <> 'Deleted' AND (os.[EffectiveTo] IS NULL OR os.[EffectiveTo] >= GETDATE())
    JOIN (SELECT StandardId, Title, CONVERT(numeric,JSON_VALUE([StandardData],'$.Duration')) Duration FROM [dbo].[StandardCollation]) std ON std.StandardId = il.StdCode
    -- ignore already created certs
    LEFT JOIN (SELECT DISTINCT Uln, StandardCode FROM [Certificates] c1 ) ce ON ce.Uln = il.Uln AND ce.StandardCode = il.StdCode
    WHERE ce.Uln IS NULL
	AND il.CompletionStatus IN (1,2) AND il.EpaOrgId = @EPAOID 
	-- limit Pipeline to where Ilr is completed or most recent active submission is no more than 6 months ago
 	AND ( (FundingModel = 36 and CompletionStatus = 1 AND ISNULL(il.UpdatedAt,il.CreatedAt) >= dateadd(month,-6,GETDATE()) ) OR CompletionStatus = 2 OR FundingModel != 36) 
	-- limit Pipeline to where the Planned End Date (or Estimated End Date) is no more than 3 months in the past.
   	AND (CASE WHEN PlannedEndDate > GETDATE() THEN EOMONTH(PlannedEndDate) ELSE EOMONTH(DATEADD(month, std.Duration, LearnStartDate)) END) >= dateadd(month,-3,GETDATE())
    ) ab0
    GROUP BY StdCode,EstimateDate,Title
), 
Count_CTE 
AS 
(
   SELECT COUNT(*) AS TotalRows FROM Data_CTE
)
SELECT *
FROM Data_CTE
CROSS JOIN Count_CTE
ORDER BY Title,EstimateDate

END
GO