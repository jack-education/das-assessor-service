﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using SFA.DAS.AssessorService.Data;
using System;

namespace SFA.DAS.AssessorService.Data.Migrations
{
    [DbContext(typeof(AssessorDbContext))]
    [Migration("20180213125233_AddSOftDelete")]
    partial class AddSOftDelete
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.Certificate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AchievementDate");

                    b.Property<string>("AchievementOutcome");

                    b.Property<string>("ContactAddLine1");

                    b.Property<string>("ContactAddLine2");

                    b.Property<string>("ContactAddLine3");

                    b.Property<string>("ContactAddLine4");

                    b.Property<string>("ContactName");

                    b.Property<string>("ContactOrganisation");

                    b.Property<string>("ContactPostcode");

                    b.Property<string>("CourseOption");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<Guid>("CreatedBy");

                    b.Property<DateTime>("DeletedAt");

                    b.Property<Guid>("DeletedBy");

                    b.Property<int>("EndPointAssessorCertificateId");

                    b.Property<int>("EndPointAssessorContactId");

                    b.Property<string>("EndPointAssessorOrganisationId");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("LearnerDateofBirth");

                    b.Property<string>("LearnerFamilyName");

                    b.Property<string>("LearnerGivenNames");

                    b.Property<string>("LearnerSex");

                    b.Property<DateTime>("LearningStartDate");

                    b.Property<Guid>("OrganisationId");

                    b.Property<string>("OverallGrade");

                    b.Property<int>("ProviderUKPRN");

                    b.Property<string>("Registration");

                    b.Property<int>("StandardCode");

                    b.Property<int>("StandardLevel");

                    b.Property<string>("StandardName");

                    b.Property<DateTime>("StandardPublicationDate");

                    b.Property<string>("Status");

                    b.Property<int>("ULN");

                    b.Property<DateTime>("UpdatedAt");

                    b.Property<Guid>("UpdatedBY");

                    b.HasKey("Id");

                    b.HasIndex("OrganisationId");

                    b.ToTable("Certificates");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.CertificateLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action");

                    b.Property<Guid>("CertificateId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime>("DeletedAt");

                    b.Property<int>("EndPointAssessorCertificateId");

                    b.Property<DateTime>("EventTime");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Status");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("CertificateId");

                    b.ToTable("CertificateLogs");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.Contact", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContactEmail");

                    b.Property<string>("ContactName");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime>("DeletedAt");

                    b.Property<int>("EndPointAssessorContactId");

                    b.Property<string>("EndPointAssessorOrganisationId");

                    b.Property<int>("EndPointAssessorUKPRN");

                    b.Property<bool>("IsDeleted");

                    b.Property<Guid>("OrganisationId");

                    b.Property<string>("Status");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("OrganisationId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.Organisation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime>("DeletedAt");

                    b.Property<string>("EndPointAssessorName");

                    b.Property<string>("EndPointAssessorOrganisationId");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Status");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("Organisations");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.Certificate", b =>
                {
                    b.HasOne("SFA.DAS.AssessorService.Data.Entitites.Organisation", "Organisation")
                        .WithMany("Certificates")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.CertificateLog", b =>
                {
                    b.HasOne("SFA.DAS.AssessorService.Data.Entitites.Certificate", "Certificate")
                        .WithMany("CertificateLogs")
                        .HasForeignKey("CertificateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Data.Entitites.Contact", b =>
                {
                    b.HasOne("SFA.DAS.AssessorService.Data.Entitites.Organisation", "Organisation")
                        .WithMany("Contacts")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
