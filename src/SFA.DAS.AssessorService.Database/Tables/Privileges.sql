﻿CREATE TABLE [dbo].[Privileges](
	[Id] [uniqueidentifier] NOT NULL,
	[UserPrivilege] [nvarchar](120) NOT NULL DEFAULT '',
	[MustBeAtLeastOneUserAssigned] [uniqueidentifier] NOT NULL DEFAULT(0),
	[Description] [nvarchar(MAX)] NOT NULL DEFAULT ''
 CONSTRAINT [PK_Privileges] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
