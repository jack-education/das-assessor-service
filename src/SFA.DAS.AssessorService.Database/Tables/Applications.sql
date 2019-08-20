﻿CREATE TABLE [dbo].[Applications]
(
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[OrganisationId] [uniqueidentifier] NOT NULL,
	[ApplicationStatus] [nvarchar](20) NOT NULL,
	[ApplicationData] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[DeletedAt] [datetime2](7) NULL,
	[DeletedBy] [nvarchar](256) NULL
 CONSTRAINT [PK_Applications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [Applications_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Organisations_OrganisationId] FOREIGN KEY([OrganisationId])
REFERENCES [dbo].[Organisations] ([Id])
GO

ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Organisations_OrganisationId]
GO