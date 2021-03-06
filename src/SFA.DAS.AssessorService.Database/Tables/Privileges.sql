﻿CREATE TABLE [dbo].[Privileges](
    [Id] [uniqueidentifier] NOT NULL,
    [UserPrivilege] [nvarchar](120) NOT NULL DEFAULT '',
    [MustBeAtLeastOneUserAssigned] [bit] NOT NULL DEFAULT(0),
    [Description] [nvarchar](MAX) NOT NULL DEFAULT '',
    [PrivilegeData] [nvarchar](MAX) NOT NULL DEFAULT '',
    [Key] [nvarchar](125) NOT NULL DEFAULT '',
    [Enabled] bit NOT NULL DEFAULT 1
 CONSTRAINT [PK_Privileges] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Key] ON [dbo].[Privileges]
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
