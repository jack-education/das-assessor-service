﻿using Dapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models.Apply;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ApplyTypes;
using SFA.DAS.AssessorService.Data.DapperTypeHandlers;
using SFA.DAS.AssessorService.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Data.Apply
{
    public class ApplyRepository : IApplyRepository
    {
        // NOTE: Should the Financial Section move; then these need to be updated
        private const int FINANCIAL_SEQUENCE = 1;
        private const int FINANCIAL_SECTION = 3;

        private readonly IWebConfiguration _configuration;
        private readonly ILogger<ApplyRepository> _logger;

        public ApplyRepository(IWebConfiguration configuration, ILogger<ApplyRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;

            SqlMapper.AddTypeHandler(typeof(ApplyData), new ApplyDataHandler());
            SqlMapper.AddTypeHandler(typeof(FinancialGrade), new FinancialGradeHandler());
            SqlMapper.AddTypeHandler(typeof(FinancialEvidence), new FinancialEvidenceHandler());
        }

        public async Task<List<Domain.Entities.Apply>> GetUserApplications(Guid userId)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection.QueryAsync<Domain.Entities.Apply>(@"SELECT a.* FROM Contacts c
                                                    INNER JOIN Apply a ON a.OrganisationId = c.OrganisationId
                                                    WHERE c.Id = @userId AND a.CreatedBy = @userId", new { userId })).ToList();
            }
        }

        public async Task<List<Domain.Entities.Apply>> GetOrganisationApplications(Guid userId)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection.QueryAsync<Domain.Entities.Apply>(@"SELECT a.* FROM Contacts c
                                                    INNER JOIN Apply a ON a.OrganisationId = c.OrganisationId
                                                    WHERE c.Id = @userId", new { userId })).ToList();
            }
        }

        public async Task<Domain.Entities.Apply> GetApplication(Guid applicationId)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                var application = await connection.QuerySingleOrDefaultAsync<Domain.Entities.Apply>(@"SELECT * FROM Apply WHERE Id = @applicationId", new { applicationId });

                return application;
            }
        }

        public async Task<Guid> CreateApplication(Domain.Entities.Apply apply)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return await connection.QuerySingleAsync<Guid>(
                    @"INSERT INTO Apply (ApplicationId, OrganisationId ,ApplicationStatus, ApplyData, StandardCode, ReviewStatus, FinancialReviewStatus, CreatedBy, CreatedAt)
                                        OUTPUT INSERTED.[Id] 
                                        VALUES (@ApplicationId, @OrganisationId, @ApplicationStatus, @ApplyData, @StandardCode, @ReviewStatus, @FinancialReviewStatus, @CreatedBy, GETUTCDATE())",
                    new { apply.ApplicationId, apply.OrganisationId, apply.ApplicationStatus, apply.ApplyData, apply.StandardCode, apply.ReviewStatus, apply.FinancialReviewStatus, apply.CreatedBy });
            }
        }

        public async Task<bool> CanSubmitApplication(Guid applicationId)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                // Prevent submission if Sequence 1 is required and another user has submitted theirs
                var otherAppsInProgress = await connection.QueryAsync<Domain.Entities.Apply>(@"
                                                        SELECT a.*
                                                        FROM Apply a
                                                        INNER JOIN Organisations o ON o.Id = a.OrganisationId
														INNER JOIN Contacts con ON a.OrganisationId = con.OrganisationID
                                                        CROSS APPLY OPENJSON(a.ApplyData,'$.Sequences') WITH (SequenceNo INT, IsActive BIT, NotRequired BIT, Status VARCHAR(20)) sequence
                                                        WHERE a.OrganisationId = (SELECT OrganisationId FROM Apply WHERE Id = @applicationId)
														AND a.CreatedBy <> (SELECT CreatedBy FROM Apply WHERE Id = @applicationId)
                                                        AND a.ApplicationStatus NOT IN (@applicationStatusApproved, @applicationStatusApprovedDeclined)
                                                        AND sequence.NotRequired = 0 AND sequence.SequenceNo = 1
                                                        AND sequence.Status IN (@applicationSequenceStatusSubmitted, @applicationSequenceStatusResubmitted)",
                                                        new
                                                        {
                                                            applicationId,
                                                            applicationStatusApproved = ApplicationStatus.Approved,
                                                            applicationStatusApprovedDeclined = ApplicationStatus.Declined,
                                                            applicationSequenceStatusSubmitted = ApplicationSequenceStatus.Submitted,
                                                            applicationSequenceStatusResubmitted = ApplicationSequenceStatus.Resubmitted
                                                        });

                return !otherAppsInProgress.Any();
            }
        }

        public async Task SubmitApplicationSequence(Domain.Entities.Apply apply)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                await connection.ExecuteAsync(@"UPDATE Apply
                                                SET  ApplicationStatus = @ApplicationStatus, ApplyData = @ApplyData, StandardCode = @StandardCode, ReviewStatus = @ReviewStatus, FinancialReviewStatus = @FinancialReviewStatus, UpdatedBy = @UpdatedBy, UpdatedAt = GETUTCDATE() 
                                                WHERE  (Apply.Id = @Id)",
                                                new { apply.ApplicationStatus, apply.ApplyData, apply.StandardCode, apply.ReviewStatus, apply.FinancialReviewStatus, apply.Id, apply.UpdatedBy});
            }
        }

        public async Task<bool> UpdateStandardData(Guid id, int standardCode, string referenceNumber, string standardName)
        {
            var application = await GetApplication(id);
            var applyData = application?.ApplyData;

            if(application != null && applyData != null)
            {
                application.StandardCode = standardCode;

                if (applyData.Apply == null)
                {
                    applyData.Apply = new ApplyTypes.Apply();
                }

                applyData.Apply.StandardCode = standardCode;
                applyData.Apply.StandardReference = referenceNumber;
                applyData.Apply.StandardName = standardName;

                using (var connection = new SqlConnection(_configuration.SqlConnectionString))
                {
                    await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplyData = @ApplyData, StandardCode = @StandardCode
                                                    WHERE  Id = @Id",
                                                    new { application.Id, application.ApplyData, application.StandardCode });
                }

                return true;
            }

            return false;
        }

        public async Task StartFinancialReview(Guid id, string reviewer)
        {
            var application = await GetApplication(id);
            var applyData = application?.ApplyData;
            var sequence = applyData?.Sequences.SingleOrDefault(seq => seq.SequenceNo == FINANCIAL_SEQUENCE);
            var section = sequence?.Sections.SingleOrDefault(sec => sec.SectionNo == FINANCIAL_SECTION);

            if (application != null && section != null && sequence?.IsActive == true && application.FinancialReviewStatus == FinancialReviewStatus.New)
            {
                application.FinancialReviewStatus = FinancialReviewStatus.InProgress;
                application.UpdatedBy = reviewer;
                application.UpdatedAt = DateTime.UtcNow;

                section.Status = ApplicationSectionStatus.InProgress;
                section.ReviewedBy = reviewer;
                section.ReviewStartDate = DateTime.UtcNow;

                using (var connection = new SqlConnection(_configuration.SqlConnectionString))
                {
                    await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplyData = @ApplyData, FinancialReviewStatus = @FinancialReviewStatus, UpdatedBy = @UpdatedBy, UpdatedAt = GETUTCDATE() 
                                                    WHERE Apply.Id = @Id",
                                                    new { application.Id, application.ApplyData, application.FinancialReviewStatus, application.UpdatedBy });
                }
            }
        }

        public async Task ReturnFinancialReview(Guid id, FinancialGrade financialGrade)
        {
            if (financialGrade != null)
            {
                var application = await GetApplication(id);
                var applyData = application?.ApplyData;
                var sequence = applyData?.Sequences.SingleOrDefault(seq => seq.SequenceNo == FINANCIAL_SEQUENCE);
                var section = sequence?.Sections.SingleOrDefault(sec => sec.SectionNo == FINANCIAL_SECTION);

                if (application != null && section != null && sequence?.IsActive == true && application.FinancialReviewStatus == FinancialReviewStatus.InProgress)
                {
                    var financialReviewStatus = (financialGrade.SelectedGrade == FinancialApplicationSelectedGrade.Inadequate) ? FinancialReviewStatus.Rejected : FinancialReviewStatus.Graded;

                    application.FinancialReviewStatus = financialReviewStatus;
                    application.FinancialGrade = financialGrade;
                    application.UpdatedBy = financialGrade.GradedBy;
                    application.UpdatedAt = DateTime.UtcNow;

                    section.Status = ApplicationSectionStatus.Graded;
                    section.ReviewedBy = financialGrade.GradedBy;
                    section.ReviewStartDate = DateTime.UtcNow;

                    using (var connection = new SqlConnection(_configuration.SqlConnectionString))
                    {
                        await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplyData = @ApplyData, FinancialGrade = @FinancialGrade, FinancialReviewStatus = @FinancialReviewStatus, UpdatedBy = @UpdatedBy, UpdatedAt = GETUTCDATE()  
                                                    WHERE Apply.Id = @Id",
                                                        new { application.Id, application.ApplyData, application.FinancialGrade, application.FinancialReviewStatus, application.UpdatedBy });
                    }
                }
            }
            else
            {
                _logger.LogError("FinancialGrade is null therefore failed to update Apply table.");
            }
        }

        public async Task StartApplicationSectionReview(Guid id, int sequenceNo, int sectionNo, string reviewer)
        {
            var application = await GetApplication(id);
            var applyData = application?.ApplyData;
            var sequence = applyData?.Sequences.SingleOrDefault(seq => seq.SequenceNo == sequenceNo);
            var section = sequence?.Sections.SingleOrDefault(sec => sec.SectionNo == sectionNo);

            if (application != null && section != null && sequence?.IsActive == true && section.Status != ApplicationSectionStatus.Evaluated)
            {
                application.ReviewStatus = ApplicationReviewStatus.InProgress;
                application.UpdatedBy = reviewer;
                application.UpdatedAt = DateTime.UtcNow;

                section.Status = ApplicationSectionStatus.InProgress;
                section.ReviewedBy = reviewer;
                section.ReviewStartDate = DateTime.UtcNow;

                using (var connection = new SqlConnection(_configuration.SqlConnectionString))
                {
                    await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplyData = @ApplyData, ReviewStatus = @ReviewStatus, UpdatedBy = @UpdatedBy, UpdatedAt = GETUTCDATE() 
                                                    WHERE Apply.Id = @Id",
                                                    new { application.Id, application.ApplyData, application.ReviewStatus, application.UpdatedBy });
                }
            }
        }

        public async Task EvaluateApplicationSection(Guid id, int sequenceNo, int sectionNo, bool isSectionComplete, string evaluatedBy)
        {
            var application = await GetApplication(id);
            var applyData = application?.ApplyData;
            var sequence = applyData?.Sequences.SingleOrDefault(seq => seq.SequenceNo == sequenceNo);
            var section = sequence?.Sections.SingleOrDefault(sec => sec.SectionNo == sectionNo);

            if (application != null && section != null && sequence?.IsActive == true)
            {
                application.UpdatedBy = evaluatedBy;
                application.UpdatedAt = DateTime.UtcNow;

                if (isSectionComplete)
                {
                    section.Status = ApplicationSectionStatus.Evaluated;
                    section.EvaluatedDate = DateTime.UtcNow;
                    section.EvaluatedBy = evaluatedBy;

                    if(section.ReviewedBy != section.EvaluatedBy)
                    {
                        // Note: If it's a different person who has evaluated from the person who started the review then update the review information!
                        section.ReviewStartDate = section.EvaluatedDate;
                        section.ReviewedBy = section.EvaluatedBy;
                    }
                }
                else if (sequence.SequenceNo == FINANCIAL_SEQUENCE && section.SectionNo == FINANCIAL_SECTION)
                {
                    section.Status = ApplicationSectionStatus.Graded;
                    section.EvaluatedDate = null;
                    section.EvaluatedBy = null;

                    if(application.FinancialGrade != null)
                    {
                        section.ReviewStartDate = application.FinancialGrade.GradedDateTime;
                        section.ReviewedBy = application.FinancialGrade.GradedBy;
                    }
                }
                else
                {
                    section.Status = ApplicationSectionStatus.InProgress;
                    section.EvaluatedDate = null;
                    section.EvaluatedBy = null;
                }

                using (var connection = new SqlConnection(_configuration.SqlConnectionString))
                {
                    await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplyData = @ApplyData, UpdatedBy = @UpdatedBy, UpdatedAt = GETUTCDATE() 
                                                    WHERE  (Apply.Id = @Id)",
                                                    new { application.Id, application.ApplyData, application.UpdatedBy });
                }
            }
        }

        public async Task ReturnApplicationSequence(Guid id, int sequenceNo, string sequenceStatus, string returnedBy)
        {
            var application = await GetApplication(id);
            var applyData = application?.ApplyData;
            var sequence = applyData?.Sequences.SingleOrDefault(seq => seq.SequenceNo == sequenceNo);
            var nextSequence = applyData?.Sequences.Where(seq => seq.SequenceNo > sequenceNo && !seq.NotRequired).OrderBy(seq => seq.SequenceNo).FirstOrDefault();

            if (application != null && applyData != null && sequence !=null)
            { 
                application.UpdatedBy = returnedBy;
                application.UpdatedAt = DateTime.UtcNow;
                sequence.Status = sequenceStatus;
                sequence.ApprovedBy = returnedBy;
                sequence.ApprovedDate = DateTime.UtcNow;

                switch (sequenceStatus)
                {
                    case ApplicationSequenceStatus.FeedbackAdded:
                        application.ReviewStatus = ApplicationReviewStatus.HasFeedback;
                        application.ApplicationStatus = ApplicationStatus.FeedbackAdded;
                        if (sequenceNo == 1)
                        {
                            applyData.Apply.InitSubmissionFeedbackAddedDate = DateTime.UtcNow;
                        }
                        else if (sequenceNo == 2)
                        {
                            applyData.Apply.StandardSubmissionFeedbackAddedDate = DateTime.UtcNow;
                        }
                        break;
                    case ApplicationSequenceStatus.Declined:
                        application.ReviewStatus = ApplicationReviewStatus.Declined;
                        application.ApplicationStatus = ApplicationStatus.Declined;
                        if (sequenceNo == 1)
                        {
                            applyData.Apply.InitSubmissionClosedDate = DateTime.UtcNow;
                        }
                        else if (sequenceNo == 2)
                        {
                            applyData.Apply.StandardSubmissionClosedDate = DateTime.UtcNow;
                        }
                        break;
                    case ApplicationSequenceStatus.Approved:
                        application.ReviewStatus = ApplicationReviewStatus.Approved;
                        if (sequenceNo == 1)
                        {
                            applyData.Apply.InitSubmissionClosedDate = DateTime.UtcNow;
                        }
                        else if (sequenceNo == 2)
                        {
                            applyData.Apply.StandardSubmissionClosedDate = DateTime.UtcNow;
                        }

                        if(nextSequence != null)
                        {
                            sequence.IsActive = false;
                            nextSequence.IsActive = true;
                            application.ApplicationStatus = ApplicationStatus.InProgress;
                        }
                        else
                        {
                            application.ApplicationStatus = ApplicationStatus.Approved;

                            // Delete any related applications if this one was an initial application
                            // (i.e all sequences are required, section 1 & 2 are required, hence not on EPAO Register)
                            var sequenceOneSections = applyData.Sequences.Where(seq => seq.SequenceNo == 1).SelectMany(seq => seq.Sections);
                            var initialSections = sequenceOneSections.Where(sec => sec.SectionNo == 1 || sec.SectionNo == 2);

                            bool initialSectionsRequired = initialSections.All(sec => !sec.NotRequired);
                            bool allSequencesRequired = applyData.Sequences.All(seq => !seq.NotRequired);

                            if (allSequencesRequired && initialSectionsRequired)
                            {
                                await RejectAllRelatedApplications(application.Id, application.UpdatedBy);
                            }
                        }
                        break;
                }

                using (var connection = new SqlConnection(_configuration.SqlConnectionString))
                {
                    await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplicationStatus = @ApplicationStatus, ReviewStatus = @ReviewStatus, ApplyData = @ApplyData, UpdatedBy = @UpdatedBy, UpdatedAt = GETUTCDATE() 
                                                    WHERE  (Apply.Id = @Id)",
                                                    new { application.Id, application.ApplicationStatus, application.ReviewStatus, application.ApplyData, application.UpdatedBy });
                }
            }
        }

        private async Task RejectAllRelatedApplications(Guid applicationId, string deletedBy)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                var inProgressRelatedApplications = await connection.QueryAsync<Domain.Entities.Apply>(@"SELECT * FROM Apply a
                                                                                                         WHERE a.OrganisationId = (SELECT OrganisationId FROM Apply WHERE Id = @applicationId)
                                                                                                         AND a.Id <> @applicationId
                                                                                                         AND a.ApplicationStatus NOT IN (@approvedStatus, @rejectedStatus)",
                                                                                                         new { applicationId, approvedStatus = ApplicationStatus.Approved, rejectedStatus = ApplicationStatus.Declined });

                foreach (var application in inProgressRelatedApplications)
                {
                    application.ApplicationStatus = ApplicationStatus.Declined;
                    application.ReviewStatus = ApplicationReviewStatus.Deleted;
                    application.DeletedBy = deletedBy;

                    foreach (var sequence in application.ApplyData?.Sequences)
                    {
                        sequence.IsActive = false;
                        sequence.Status = ApplicationSequenceStatus.Declined;
                    }

                    await connection.ExecuteAsync(@"UPDATE Apply
                                                    SET  ApplicationStatus = @ApplicationStatus, ReviewStatus = @ReviewStatus, ApplyData = @ApplyData, DeletedBy = @UpdatedBy, DeletedAt = GETUTCDATE() 
                                                    WHERE  (Apply.Id = @Id)",
                                                    new { application.Id, application.ApplicationStatus, application.ReviewStatus, application.ApplyData, application.DeletedBy });
                }
            }
        }

        public async Task<int> GetNextAppReferenceSequence()
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection.QueryAsync<int>(@"SELECT NEXT VALUE FOR AppRefSequence")).FirstOrDefault();

            }
        }

        public async Task<List<ApplicationSummaryItem>> GetOpenApplications(int sequenceNo)
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection
                    .QueryAsync<ApplicationSummaryItem>(
                        @"SELECT
                            ApplicationId,
                            SequenceNo,
                            OrganisationName,
                            ApplicationType,
                            StandardName,
                            StandardCode,
                            SubmittedDate,
                            SubmissionCount,
                            ApplicationStatus,
                            ReviewStatus,
                            FinancialStatus,
                            FinancialGrade
                        FROM ApplicationSummary 
                        WHERE ApplicationStatus IN (@applicationStatusSubmitted, @applicationStatusResubmitted)
                        AND SequenceNo = @SequenceNo",
                        new
                        {
                            SequenceNo = sequenceNo,
                            applicationStatusSubmitted = ApplicationStatus.Submitted,
                            applicationStatusResubmitted = ApplicationStatus.Resubmitted,
                        })).ToList();
            }
        }

        public async Task<List<ApplicationSummaryItem>> GetFeedbackAddedApplications()
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection
                    .QueryAsync<ApplicationSummaryItem>(
                        @"SELECT
                            ApplicationId,
                            SequenceNo,
                            OrganisationName,
                            ApplicationType,
                            StandardName,
                            StandardCode,
                            FeedbackAddedDate,
                            SubmissionCount,
                            ApplicationStatus,
                            ReviewStatus,
                            FinancialStatus,
                            FinancialGrade
                        FROM ApplicationSummary 
                        WHERE ApplicationStatus IN (@applicationStatusFeedbackAdded)",
                        new
                        {
                            applicationStatusFeedbackAdded = ApplicationStatus.FeedbackAdded,
                        })).ToList();
            }
        }

        public async Task<List<ApplicationSummaryItem>> GetClosedApplications()
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection
                    .QueryAsync<ApplicationSummaryItem>(
                        @"SELECT 
                            ap1.id AS ApplicationId,
                            seq.SequenceNo AS SequenceNo,
                            org.EndPointAssessorName AS OrganisationName,
                            CASE WHEN seq.SequenceNo = 1 THEN 'Midpoint'
		                         WHEN seq.SequenceNo = 2 THEN 'Standard'
		                         ELSE 'Unknown'
	                        END As ApplicationType,
                            CASE WHEN seq.SequenceNo = 1 THEN NULL
		                         ELSE JSON_VALUE(ap1.Applydata, '$.Apply.StandardName')
                            END As StandardName,
                            CASE WHEN seq.SequenceNo = 1 THEN NULL
		                         ELSE JSON_VALUE(ap1.Applydata, '$.Apply.StandardCode')
                            END As StandardCode,
                            CASE WHEN seq.SequenceNo = 1 THEN JSON_VALUE(ap1.Applydata, '$.Apply.InitSubmissionClosedDate')
		                         WHEN seq.SequenceNo = 2 THEN JSON_VALUE(ap1.Applydata, '$.Apply.StandardSubmissionClosedDate')
		                         ELSE NULL
	                        END As ClosedDate,
                            CASE WHEN seq.SequenceNo = 1 THEN JSON_VALUE(ap1.Applydata, '$.Apply.InitSubmissionsCount')
		                         WHEN seq.SequenceNo = 2 THEN JSON_VALUE(ap1.Applydata, '$.Apply.StandardSubmissionsCount')
		                         ELSE 0
	                        END As SubmissionCount,
                            ap1.ApplicationStatus As ApplicationStatus,
                            ap1.ReviewStatus As ReviewStatus,
                            ap1.FinancialReviewStatus As FinancialStatus,
                            JSON_VALUE(ap1.FinancialGrade,'$.SelectedGrade') AS FinancialGrade,
                            seq.Status As SequenceStatus
	                     FROM Apply ap1
					        INNER JOIN Organisations org ON ap1.OrganisationId = org.Id
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences') WITH (SequenceNo INT, Status VARCHAR(20), NotRequired BIT) seq
	                     WHERE seq.Status IN (@sequenceStatusApproved, @sequenceStatusDeclined) AND seq.NotRequired = 0
                            AND ap1.DeletedAt IS NULL
                            AND ap1.ApplicationStatus <> @applicationStatusDeclined
                            AND ap1.ReviewStatus <> @applicationReviewStatusDeleted",
                        new
                        {
                            sequenceStatusApproved = ApplicationSequenceStatus.Approved,
                            sequenceStatusDeclined = ApplicationSequenceStatus.Declined,
                            applicationStatusDeclined = ApplicationStatus.Declined,
                            applicationReviewStatusDeleted = ApplicationReviewStatus.Deleted
                        })).ToList();


                //This has been commented out as Alan seems to think we'll be able to move back to this at a later date
                //return (await connection
                //    .QueryAsync<ApplicationSummaryItem>(
                //        @"SELECT
                //            ApplicationId,
                //            SequenceNo,
                //            OrganisationName,
                //            ApplicationType,
                //            StandardName,
                //            StandardCode,
                //            ClosedDate,
                //            SubmissionCount,
                //            ApplicationStatus,
                //            ReviewStatus,
                //            FinancialStatus,
                //            FinancialGrade
                //        FROM ApplicationSummary 
                //        WHERE ApplicationStatus IN (@applicationStatusApproved, @applicationStatusDeclined)",
                //        new
                //        {
                //            applicationStatusApproved = ApplicationStatus.Approved,
                //            applicationStatusDeclined = ApplicationStatus.Declined,
                //        })).ToList();
            }
        }

        public async Task<List<FinancialApplicationSummaryItem>> GetOpenFinancialApplications()
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection
                    .QueryAsync<FinancialApplicationSummaryItem>(
                        @"SELECT
                            ap1.Id AS ApplicationId,
                            sequence.SequenceNo AS SequenceNo,
                            section.SectionNo AS SectionNo, 
                            org.EndPointAssessorName AS OrganisationName, 
                            apply.SubmittedDate AS SubmittedDate,
                            apply.SubmissionCount AS SubmissionCount,
                            ap1.ApplicationStatus AS ApplicationStatus,
                            ap1.ReviewStatus AS ReviewStatus,
                            ap1.FinancialReviewStatus AS FinancialStatus
                        FROM Apply ap1
                        INNER JOIN Organisations org ON ap1.OrganisationId = org.Id
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences') WITH (SequenceNo INT, IsActive BIT, Status VARCHAR(20)) sequence
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences[0].Sections') WITH (SectionNo INT, Status VARCHAR(20)) section
                            CROSS APPLY OPENJSON(ApplyData,'$.Apply') WITH (SubmittedDate VARCHAR(30) '$.LatestInitSubmissionDate', SubmissionCount INT '$.InitSubmissionsCount') apply
                        WHERE sequence.SequenceNo = 1 AND section.SectionNo = 3 AND sequence.IsActive = 1
                            AND ap1.FinancialReviewStatus IN (@financialReviewStatusNew, @financialReviewStatusInProgress)
                            AND ap1.ApplicationStatus IN (@applicationStatusSubmitted, @applicationStatusResubmitted)
                            AND ap1.DeletedAt IS NULL",
                        new
                        {
                            financialReviewStatusNew = FinancialReviewStatus.New,
                            financialReviewStatusInProgress = FinancialReviewStatus.InProgress,
                            applicationStatusSubmitted = ApplicationStatus.Submitted,
                            applicationStatusResubmitted = ApplicationStatus.Resubmitted,
                        })).ToList();
            }
        }

        public async Task<List<FinancialApplicationSummaryItem>> GetFeedbackAddedFinancialApplications()
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection
                    .QueryAsync<FinancialApplicationSummaryItem>(
                        @"SELECT
                            ap1.Id AS ApplicationId,
                            sequence.SequenceNo AS SequenceNo,
                            section.SectionNo AS SectionNo, 
                            org.EndPointAssessorName AS OrganisationName,
                            apply.SubmittedDate AS SubmittedDate,
                            apply.SubmissionCount AS SubmissionCount,
                            ISNULL(section.FeedbackDate, JSON_VALUE(ap1.FinancialGrade, '$.GradedDateTime')) As FeedbackAddedDate,
                            ap1.ApplicationStatus AS ApplicationStatus,
                            ap1.ReviewStatus AS ReviewStatus,
                            ap1.FinancialReviewStatus AS FinancialStatus,
	                        ap1.FinancialGrade As Grade
                        FROM Apply ap1
                        INNER JOIN Organisations org ON ap1.OrganisationId = org.Id
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences') WITH (SequenceNo INT, IsActive BIT, Status VARCHAR(20)) sequence
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences[0].Sections') WITH (SectionNo INT, Status VARCHAR(20), FeedbackDate VARCHAR(30) '$.Feedback.FeedbackDate') section
                            CROSS APPLY OPENJSON(ApplyData,'$.Apply') WITH (SubmittedDate VARCHAR(30) '$.LatestInitSubmissionDate', SubmissionCount INT '$.InitSubmissionsCount') apply
                        WHERE sequence.SequenceNo = 1 AND section.SectionNo = 3 AND sequence.IsActive = 1
                            AND ap1.FinancialReviewStatus = @financialReviewStatusRejected
                            AND ap1.ApplicationStatus IN (@applicationStatusSubmitted, @applicationStatusResubmitted, @applicationStatusFeedbackAdded)
                            AND ap1.DeletedAt IS NULL",
                        new
                        {
                            financialReviewStatusRejected = FinancialReviewStatus.Rejected,
                            applicationStatusSubmitted = ApplicationStatus.Submitted,
                            applicationStatusResubmitted = ApplicationStatus.Resubmitted,
                            applicationStatusFeedbackAdded = ApplicationStatus.FeedbackAdded,
                        })).ToList();
            }
        }

        public async Task<List<FinancialApplicationSummaryItem>> GetClosedFinancialApplications()
        {
            using (var connection = new SqlConnection(_configuration.SqlConnectionString))
            {
                return (await connection
                    .QueryAsync<FinancialApplicationSummaryItem>(
                        @"SELECT
                            ap1.Id AS ApplicationId,
                            sequence.SequenceNo AS SequenceNo,
                            section.SectionNo AS SectionNo, 
                            org.EndPointAssessorName AS OrganisationName,
                            apply.ClosedDate AS ClosedDate,
                            apply.SubmissionCount AS SubmissionCount,
                            ap1.ApplicationStatus AS ApplicationStatus,
                            ap1.ReviewStatus AS ReviewStatus,
                            ap1.FinancialReviewStatus AS FinancialStatus,
	                        ap1.FinancialGrade As Grade
                        FROM Apply ap1
                        INNER JOIN Organisations org ON ap1.OrganisationId = org.Id
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences') WITH (SequenceNo INT, Status VARCHAR(20)) sequence
                            CROSS APPLY OPENJSON(ApplyData,'$.Sequences[0].Sections') WITH (SectionNo INT, Status VARCHAR(20), NotRequired BIT) section
                            CROSS APPLY OPENJSON(ApplyData,'$.Apply') WITH (ClosedDate VARCHAR(30) '$.InitSubmissionClosedDate', SubmissionCount INT '$.InitSubmissionsCount') apply
                        WHERE sequence.SequenceNo = 1 AND section.SectionNo = 3 AND section.NotRequired = 0
                            AND ap1.FinancialReviewStatus IN (@financialReviewStatusGraded, @financialReviewStatusApproved) -- NOTE: Not showing Exempt
                            AND ap1.DeletedAt IS NULL",
                        new
                        {
                            financialReviewStatusGraded = FinancialReviewStatus.Graded,
                            financialReviewStatusApproved = FinancialReviewStatus.Approved                            
                        })).ToList();
            }
        }

    }
}
