USE [master]
GO

/****** Object:  Database [Load]  Script Date: 28.04.2021 ******/

-- to change the name of the target database from [Load] to something else
-- edit this script and change all
-- [Load] to [SomethingElse]
-- 'Load.mdf' to 'SomethingElse.mdf'
-- 'Load_Log.ldf' to 'SomethingElse_Log.lff'
-- N''Load'' to N''SomethingElse''
-- N''Load_log'' to N''SomethingElse_log''

DECLARE @MdfPath NVARCHAR(1024) = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(1024)) + 'Load.mdf';
DECLARE @LdfPath NVARCHAR(1024) = CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS NVARCHAR(1024)) + 'Load_Log.ldf';

DECLARE @CreateDBSQL NVARCHAR(1024) = 'CREATE DATABASE [Load] CONTAINMENT = NONE ON  PRIMARY 
( NAME = N''Load'', FILENAME = ' + quotename(@MdfPath) + ', SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N''Load_log'', FILENAME = ' + quotename(@LdfPath) + ', SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )'

EXEC (@CreateDBSQL)
GO


ALTER DATABASE [Load] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Load].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Load] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Load] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Load] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Load] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Load] SET ARITHABORT OFF 
GO
ALTER DATABASE [Load] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Load] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Load] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Load] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Load] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Load] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Load] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Load] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Load] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Load] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Load] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Load] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Load] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Load] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Load] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Load] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Load] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Load] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Load] SET  MULTI_USER 
GO
ALTER DATABASE [Load] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Load] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Load] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Load] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Load] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Load] SET QUERY_STORE = OFF
GO
USE [Load]
GO
/****** Object:  Table [dbo].[CustomObjectCustomObjectLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomObjectCustomObjectLinks](
	[ParentCustomObjectID] [bigint] NOT NULL,
	[ChildCustomObjectID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_CustomObjectCustomObjectLinks] PRIMARY KEY CLUSTERED 
(
	[ParentCustomObjectID] ASC,
	[ChildCustomObjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomObjectFileLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomObjectFileLinks](
	[ParentCustomObjectID] [bigint] NOT NULL,
	[ChildFileID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_CustomObjectFileLinks] PRIMARY KEY CLUSTERED 
(
	[ParentCustomObjectID] ASC,
	[ChildFileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomObjectFolderLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomObjectFolderLinks](
	[ParentCustomObjectID] [bigint] NOT NULL,
	[ChildFolderID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_CustomObjectFolderLinks] PRIMARY KEY CLUSTERED 
(
	[ParentCustomObjectID] ASC,
	[ChildFolderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomObjectItemLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomObjectItemLinks](
	[ParentCustomObjectID] [bigint] NOT NULL,
	[ChildItemID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_CustomObjectItemLinks] PRIMARY KEY CLUSTERED 
(
	[ParentCustomObjectID] ASC,
	[ChildItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomObjects]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomObjects](
	[CustomObjectID] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomObjectDefinition] [nvarchar](50) NOT NULL,
	[CustomObjectName] [nvarchar](260) NOT NULL,
	[Category] [nvarchar](max) NOT NULL,
	[CreateUser] [nvarchar](max) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LifecycleState] [nvarchar](max) NULL,
	[LifecycleDefinition] [nvarchar](max) NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
	[UDP_Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_CustomObjects] PRIMARY KEY CLUSTERED 
(
	[CustomObjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FileFileRelations]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileFileRelations](
	[ParentFileID] [bigint] NOT NULL,
	[ChildFileID] [bigint] NOT NULL,
	[IsAttachment] [bit] NOT NULL,
	[IsDependency] [bit] NOT NULL,
	[NeedsResolution] [bit] NULL,
	[Source] [nvarchar](20) NULL,
	[RefId] [nvarchar](256) NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
 CONSTRAINT [PK_FileFileRelations] PRIMARY KEY CLUSTERED 
(
	[ParentFileID] ASC,
	[ChildFileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Files]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Files](
	[LocalFullFileName] [nvarchar](256) NULL,
	[LocalFileChecksum] [int] NULL,
	[FileID] [bigint] IDENTITY(1,1) NOT NULL,
	[FolderID] [bigint] NOT NULL,
	[FileName] [nvarchar](260) NOT NULL,
	[Category] [nvarchar](max) NULL,
	[Classification] [nvarchar](max) NULL,
	[RevisionLabel] [nvarchar](5) NULL,
	[RevisionDefinition] [nvarchar](max) NULL,
	[Version] [int] NULL,
	[LifecycleState] [nvarchar](max) NULL,
	[LifecycleDefinition] [nvarchar](max) NULL,
	[Comment] [nvarchar](max) NULL,
	[CreateUser] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL,
	[ContentSource] [nvarchar](20) NULL,
	[IsHidden] [bit] NULL,
	[IsExcluded] [bit] NULL,
	[VaultChecksum] [int] NULL,
	[VaultCreateDate] [datetime] NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
	[UDP_Part Number] [nvarchar](max) NULL,
	[UDP_Title] [nvarchar](max) NULL,
	[UDP_Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED 
(
	[FileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FolderCustomObjectLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FolderCustomObjectLinks](
	[ParentFolderID] [bigint] NOT NULL,
	[ChildCustomObjectID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_FolderCustomObjectLinks] PRIMARY KEY CLUSTERED 
(
	[ParentFolderID] ASC,
	[ChildCustomObjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FolderFileLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FolderFileLinks](
	[ParentFolderID] [bigint] NOT NULL,
	[ChildFileID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_FolderFileLinks] PRIMARY KEY CLUSTERED 
(
	[ParentFolderID] ASC,
	[ChildFileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FolderFolderLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FolderFolderLinks](
	[ParentFolderID] [bigint] NOT NULL,
	[ChildFolderID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_FolderFolderLinks] PRIMARY KEY CLUSTERED 
(
	[ParentFolderID] ASC,
	[ChildFolderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FolderItemLinks]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FolderItemLinks](
	[ParentFolderID] [bigint] NOT NULL,
	[ChildItemID] [bigint] NOT NULL,
	[Tag] [sql_variant] NULL,
 CONSTRAINT [PK_FolderItemLinks] PRIMARY KEY CLUSTERED 
(
	[ParentFolderID] ASC,
	[ChildItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Folders]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Folders](
	[FolderID] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentFolderID] [bigint] NULL,
	[FolderName] [nvarchar](260) NOT NULL,
	[Path] [nvarchar](max) NULL,
	[IsLibrary] [bit] NULL,
	[Category] [nvarchar](max) NULL,
	[LifecycleState] [nvarchar](max) NULL,
	[LifecycleDefinition] [nvarchar](max) NULL,
	[CreateUser] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
	[UDP_Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Foders] PRIMARY KEY CLUSTERED 
(
	[FolderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ItemFileRelations]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemFileRelations](
	[ItemID] [bigint] NOT NULL,
	[FileID] [bigint] NOT NULL,
	[IsAttachment] [bit] NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
 CONSTRAINT [PK_ItemFileRelations] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC,
	[FileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ItemItemRelations]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemItemRelations](
	[ParentItemID] [bigint] NOT NULL,
	[ChildItemID] [bigint] NOT NULL,
	[Position] [int] NOT NULL,
	[Quantity] [decimal](18, 5) NOT NULL,
	[Unit] [nvarchar](50) NULL,
	[LinkType] [nvarchar](50) NULL,
	[InstanceCount] [int] NULL,
	[UnitSize] [decimal](18, 5) NULL,
	[CAD] [bit] NOT NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
 CONSTRAINT [PK_ItemItemRelations] PRIMARY KEY CLUSTERED 
(
	[ParentItemID] ASC,
	[ChildItemID] ASC,
	[Position] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Items]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Items](
	[ItemID] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemNumber] [nvarchar](50) NOT NULL,
	[Category] [nvarchar](max) NULL,
	[RevisionLabel] [nvarchar](5) NULL,
	[RevisionDefinition] [nvarchar](max) NULL,
	[Version] [int] NULL,
	[LifecycleState] [nvarchar](max) NULL,
	[LifecycleDefinition] [nvarchar](max) NULL,
	[Comment] [nvarchar](max) NULL,
	[CreateUser] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL,
	[Title] [nvarchar](max) NULL,
	[Unit] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[BomStructure] [nvarchar](max) NULL,
	[Tag] [sql_variant] NULL,
	[Validation_Comment] [nvarchar](max) NULL,
	[Validation_Status] [nvarchar](20) NULL,
	[UDP_Effectivity] [nvarchar](max) NULL,
 CONSTRAINT [PK_Items] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TargetVaultCategories]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetVaultCategories](
	[EntityClassId] [nvarchar](50) NOT NULL,
	[Category] [nvarchar](256) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TargetVaultFiles]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetVaultFiles](
	[Folder] [nvarchar](1000) NOT NULL,
	[FileName] [nvarchar](256) NOT NULL,
	[FileIterationId] [bigint] NOT NULL,
	[FileMasterId] [bigint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Checksum] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TargetVaultLifeCycles]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetVaultLifeCycles](
	[EntityClassId] [nvarchar](50) NOT NULL,
	[LifeCycleDefinition] [nvarchar](256) NOT NULL,
	[LifeCycleState] [nvarchar](256) NOT NULL,
	[IsObsoleteState] [bit] NOT NULL,
	[IsReleasedState] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TargetVaultProperties]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetVaultProperties](
	[EntityClassId] [nvarchar](50) NOT NULL,
	[PropertyName] [nvarchar](60) NOT NULL,
	[DataType] [nvarchar](60) NOT NULL,
	[IsSystem] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TargetVaultRevisions]    Script Date: 29.04.2021 08:33:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetVaultRevisions](
	[RevisionDefinition] [nvarchar](256) NOT NULL,
	[PrimarySequence] [nvarchar](256) NOT NULL,
	[RevisionLabel] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TargetVaultUsers]    Script Date: 04.05.2021 09:35:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetVaultUsers](
	[UserName] [nvarchar](255) NOT NULL,
	[SuperUser] [bit] NOT NULL,
	[AuthType] [int] NOT NULL,
	[Active] [bit] NOT NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CustomObjects]    Script Date: 29.04.2021 08:33:32 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomObjects] ON [dbo].[CustomObjects]
(
	[CustomObjectDefinition] ASC,
	[CustomObjectName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Files]    Script Date: 29.04.2021 08:33:32 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Files] ON [dbo].[Files]
(
	[FileName] ASC,
	[FolderID] ASC,
	[RevisionLabel] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Items]    Script Date: 29.04.2021 08:33:32 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Items] ON [dbo].[Items]
(
	[ItemNumber] ASC,
	[RevisionLabel] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CustomObjectCustomObjectLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectCustomObjectLinks_CustomObjects_Child] FOREIGN KEY([ChildCustomObjectID])
REFERENCES [dbo].[CustomObjects] ([CustomObjectID])
GO
ALTER TABLE [dbo].[CustomObjectCustomObjectLinks] CHECK CONSTRAINT [FK_CustomObjectCustomObjectLinks_CustomObjects_Child]
GO
ALTER TABLE [dbo].[CustomObjectCustomObjectLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectCustomObjectLinks_CustomObjects_Parent] FOREIGN KEY([ParentCustomObjectID])
REFERENCES [dbo].[CustomObjects] ([CustomObjectID])
GO
ALTER TABLE [dbo].[CustomObjectCustomObjectLinks] CHECK CONSTRAINT [FK_CustomObjectCustomObjectLinks_CustomObjects_Parent]
GO
ALTER TABLE [dbo].[CustomObjectFileLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectFileLinks_CustomObjects] FOREIGN KEY([ParentCustomObjectID])
REFERENCES [dbo].[CustomObjects] ([CustomObjectID])
GO
ALTER TABLE [dbo].[CustomObjectFileLinks] CHECK CONSTRAINT [FK_CustomObjectFileLinks_CustomObjects]
GO
ALTER TABLE [dbo].[CustomObjectFileLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectFileLinks_Files] FOREIGN KEY([ChildFileID])
REFERENCES [dbo].[Files] ([FileID])
GO
ALTER TABLE [dbo].[CustomObjectFileLinks] CHECK CONSTRAINT [FK_CustomObjectFileLinks_Files]
GO
ALTER TABLE [dbo].[CustomObjectFolderLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectFolderLinks_CustomObjects] FOREIGN KEY([ParentCustomObjectID])
REFERENCES [dbo].[CustomObjects] ([CustomObjectID])
GO
ALTER TABLE [dbo].[CustomObjectFolderLinks] CHECK CONSTRAINT [FK_CustomObjectFolderLinks_CustomObjects]
GO
ALTER TABLE [dbo].[CustomObjectFolderLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectFolderLinks_Folders] FOREIGN KEY([ChildFolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[CustomObjectFolderLinks] CHECK CONSTRAINT [FK_CustomObjectFolderLinks_Folders]
GO
ALTER TABLE [dbo].[CustomObjectItemLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectItemLinks_CustomObjects] FOREIGN KEY([ParentCustomObjectID])
REFERENCES [dbo].[CustomObjects] ([CustomObjectID])
GO
ALTER TABLE [dbo].[CustomObjectItemLinks] CHECK CONSTRAINT [FK_CustomObjectItemLinks_CustomObjects]
GO
ALTER TABLE [dbo].[CustomObjectItemLinks]  WITH CHECK ADD  CONSTRAINT [FK_CustomObjectItemLinks_Items] FOREIGN KEY([ChildItemID])
REFERENCES [dbo].[Items] ([ItemID])
GO
ALTER TABLE [dbo].[CustomObjectItemLinks] CHECK CONSTRAINT [FK_CustomObjectItemLinks_Items]
GO
ALTER TABLE [dbo].[FileFileRelations]  WITH CHECK ADD  CONSTRAINT [FK_FileFileRelations_FileFileRelations_Child] FOREIGN KEY([ChildFileID])
REFERENCES [dbo].[Files] ([FileID])
GO
ALTER TABLE [dbo].[FileFileRelations] CHECK CONSTRAINT [FK_FileFileRelations_FileFileRelations_Child]
GO
ALTER TABLE [dbo].[FileFileRelations]  WITH CHECK ADD  CONSTRAINT [FK_FileFileRelations_FileFileRelations_Parent] FOREIGN KEY([ParentFileID])
REFERENCES [dbo].[Files] ([FileID])
GO
ALTER TABLE [dbo].[FileFileRelations] CHECK CONSTRAINT [FK_FileFileRelations_FileFileRelations_Parent]
GO
ALTER TABLE [dbo].[Files]  WITH CHECK ADD  CONSTRAINT [FK_Files_Files] FOREIGN KEY([FileID])
REFERENCES [dbo].[Files] ([FileID])
GO
ALTER TABLE [dbo].[Files] CHECK CONSTRAINT [FK_Files_Files]
GO
ALTER TABLE [dbo].[Files]  WITH CHECK ADD  CONSTRAINT [FK_Files_Folders] FOREIGN KEY([FolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[Files] CHECK CONSTRAINT [FK_Files_Folders]
GO
ALTER TABLE [dbo].[FolderCustomObjectLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderCustomObjectLinks_CustomObjects] FOREIGN KEY([ChildCustomObjectID])
REFERENCES [dbo].[CustomObjects] ([CustomObjectID])
GO
ALTER TABLE [dbo].[FolderCustomObjectLinks] CHECK CONSTRAINT [FK_FolderCustomObjectLinks_CustomObjects]
GO
ALTER TABLE [dbo].[FolderCustomObjectLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderCustomObjectLinks_Folders] FOREIGN KEY([ParentFolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[FolderCustomObjectLinks] CHECK CONSTRAINT [FK_FolderCustomObjectLinks_Folders]
GO
ALTER TABLE [dbo].[FolderFileLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderFileLinks_Files] FOREIGN KEY([ChildFileID])
REFERENCES [dbo].[Files] ([FileID])
GO
ALTER TABLE [dbo].[FolderFileLinks] CHECK CONSTRAINT [FK_FolderFileLinks_Files]
GO
ALTER TABLE [dbo].[FolderFileLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderFileLinks_Folders] FOREIGN KEY([ParentFolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[FolderFileLinks] CHECK CONSTRAINT [FK_FolderFileLinks_Folders]
GO
ALTER TABLE [dbo].[FolderFolderLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderFolderLinks_Folders_Child] FOREIGN KEY([ChildFolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[FolderFolderLinks] CHECK CONSTRAINT [FK_FolderFolderLinks_Folders_Child]
GO
ALTER TABLE [dbo].[FolderFolderLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderFolderLinks_Folders_Parent] FOREIGN KEY([ParentFolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[FolderFolderLinks] CHECK CONSTRAINT [FK_FolderFolderLinks_Folders_Parent]
GO
ALTER TABLE [dbo].[FolderItemLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderItemLinks_Folders] FOREIGN KEY([ParentFolderID])
REFERENCES [dbo].[Folders] ([FolderID])
GO
ALTER TABLE [dbo].[FolderItemLinks] CHECK CONSTRAINT [FK_FolderItemLinks_Folders]
GO
ALTER TABLE [dbo].[FolderItemLinks]  WITH CHECK ADD  CONSTRAINT [FK_FolderItemLinks_Items] FOREIGN KEY([ChildItemID])
REFERENCES [dbo].[Items] ([ItemID])
GO
ALTER TABLE [dbo].[FolderItemLinks] CHECK CONSTRAINT [FK_FolderItemLinks_Items]
GO
ALTER TABLE [dbo].[ItemFileRelations]  WITH CHECK ADD  CONSTRAINT [FK_ItemFileRelations_Files] FOREIGN KEY([FileID])
REFERENCES [dbo].[Files] ([FileID])
GO
ALTER TABLE [dbo].[ItemFileRelations] CHECK CONSTRAINT [FK_ItemFileRelations_Files]
GO
ALTER TABLE [dbo].[ItemFileRelations]  WITH CHECK ADD  CONSTRAINT [FK_ItemFileRelations_Items] FOREIGN KEY([ItemID])
REFERENCES [dbo].[Items] ([ItemID])
GO
ALTER TABLE [dbo].[ItemFileRelations] CHECK CONSTRAINT [FK_ItemFileRelations_Items]
GO
ALTER TABLE [dbo].[ItemItemRelations]  WITH CHECK ADD  CONSTRAINT [FK_ItemItemRelations_Items_Child] FOREIGN KEY([ChildItemID])
REFERENCES [dbo].[Items] ([ItemID])
GO
ALTER TABLE [dbo].[ItemItemRelations] CHECK CONSTRAINT [FK_ItemItemRelations_Items_Child]
GO
ALTER TABLE [dbo].[ItemItemRelations]  WITH CHECK ADD  CONSTRAINT [FK_ItemItemRelations_Items_Parent] FOREIGN KEY([ParentItemID])
REFERENCES [dbo].[Items] ([ItemID])
GO
ALTER TABLE [dbo].[ItemItemRelations] CHECK CONSTRAINT [FK_ItemItemRelations_Items_Parent]
GO
USE [master]
GO
ALTER DATABASE [Load] SET  READ_WRITE 
GO
