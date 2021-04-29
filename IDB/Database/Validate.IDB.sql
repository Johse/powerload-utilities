-- COOLORANGE sample SQL scripts for validating the powerLoad database prior to exporting it to a BCP-package
-- The scripts fill the field 'Validation_Status' with either 'ERROR' or 'WARN'  
-- The scripts fill the field 'Validation_Comment' with a short hint and adds the already existing content
-- COOLORANGE assumes no responibility for completeness
-- Extend the SQL scripts to your needs

-- Creation of fields for Validation Results if not exist
--IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Validation_Comment' AND [object_id] = OBJECT_ID(N'Folders')) BEGIN ALTER TABLE Folders ADD Validation_Comment varchar(max), Validation_Status varchar(20) END; 
--IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Validation_Comment' AND [object_id] = OBJECT_ID(N'Files')) BEGIN ALTER TABLE Files ADD Validation_Comment varchar(max), Validation_Status varchar(20) END;
--IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Validation_Comment' AND [object_id] = OBJECT_ID(N'FileFileRelations')) BEGIN ALTER TABLE FileFileRelations ADD Validation_Comment varchar(max), Validation_Status varchar(20) END;

-- Deleting all values in 'Validation_Comment' and 'Validation_Status' to initialize the validation
UPDATE Files SET Validation_Comment = Null where Validation_Comment is not Null
UPDATE Files SET Validation_Status = Null where Validation_Status is not Null
UPDATE Folders SET Validation_Comment = Null where Validation_Comment is not Null
UPDATE Folders SET Validation_Status = Null where Validation_Status is not Null
UPDATE FileFileRelations SET Validation_Comment = Null where Validation_Comment is not Null
UPDATE FileFileRelations SET Validation_Status = Null where Validation_Status is not Null

-- Validations and Updating 
-- Folder validations
-- Duplicate folders
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Duplicate folder; ', Validation_Comment) where Path in (select Path  from folders GROUP BY FolderName , Path  Having count(FolderName)>1);
-- Mandatory folder data
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Folder data is missing; ', Validation_Comment) where FolderID in(select FolderID from Folders where FolderName is null Or  Category is null or CreateUser is null Or CreateDate is null or FolderID in (select FolderID from Folders where Path IS NULL and ParentFolderID IS NULL)) ;
-- Folder endless loop
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('An endless loop; ', Validation_Comment) where FolderId in(select p.FolderId from Folders c join Folders p on c.ParentFolderID=p.FolderID where p.ParentFolderID=c.FolderID);
-- Category does not exist  in target Vault
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Invalid Category; ', Validation_Comment) where Category not in (SELECT Category FROM TargetVaultCategories where EntityClassID = 'FLDR')
-- LifecycleDefinition does not exist  in target Vault
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Invalid LifecycleDefinition; ', Validation_Comment) where LifecycleDefinition not in (SELECT LifeCycleDefinition FROM TargetVaultLifeCycles where EntityClassID = 'FLDR')
-- LifecycleState does not exist in target Vault for assigned LifecycleDefinition
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Invalid LifecycleState; ', Validation_Comment) where CONCAT(LifecycleDefinition, LifecycleState) not in (SELECT CONCAT(LifeCycleDefinition, LifeCycleState) FROM TargetVaultLifeCycles where EntityClassID = 'FLDR') and LifecycleDefinition is not Null
-- Identifying UDPs that are not present in target Vault
Declare @FLDR_column_name nvarchar(100)
Declare @Select_Statement_FLDR nvarchar(200)
Declare c Cursor For SELECT column_name from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Folders' and LEFT(column_name,4) = 'UDP_' and SUBSTRING(column_name, 5, LEN(column_name) - 4) not in (SELECT PropertyName FROM TargetVaultProperties where EntityClassID = 'FLDR')
Open c
Fetch next from c into @FLDR_column_name
While @@FETCH_STATUS=0 Begin
       Select @Select_Statement_FLDR = 'UPDATE Folders SET Validation_Status = ''ERROR'', Validation_Comment = CONCAT (''Invalid UDP Name: '+ @FLDR_column_name +'; '', Validation_Comment) where [' + @FLDR_column_name + '] is not null'
	   EXEC sp_executeSQL @Select_Statement_FLDR
       Fetch next from c into @FLDR_column_name
End
Close c
Deallocate c

-- File validations
-- Mandatory files data
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment =CONCAT ('File data is missing; ', Validation_Comment) where FileID in (SELECT FileID FROM Files where LocalFullFileName is Null OR FileName is Null OR  Category is null OR Classification is Null OR Version is Null OR CreateUser is Null OR CreateDate is Null);
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment=CONCAT ('CreateDate is not in ascending order; ', Validation_Comment) where FileId in (select f.FileID from Files f join Files fm on f.FileName=fm.FileName where f.Version < fm.Version and f.CreateDate> fm.CreateDate);
-- Full filename is longer than 260 characters
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Full filename is longer than 260 characters: ', LEN (CONCAT (FO.Path, '/', Fi.FileName)) , '; ', Fi.Validation_Comment) from Files FI
INNER JOIN Folders FO on FO.FolderID = FI.FolderID where LEN (CONCAT (FO.Path, '/', Fi.FileName)) > 260;
-- Category does not exist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Invalid Category. ', Validation_Comment) where Category not in (SELECT Category FROM TargetVaultCategories where EntityClassID = 'FILE')
-- LifecycleDefinition does not exist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Invalid LifecycleDefinition. ', Validation_Comment) where LifecycleDefinition not in (SELECT LifeCycleDefinition FROM TargetVaultLifeCycles where EntityClassID = 'FILE')
-- LifecycleState does not exist in target Vault for assigned LifecycleDefinition
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment =  CONCAT ('Invalid LifecycleState. ', Validation_Comment) where CONCAT (LifecycleDefinition, LifecycleState) not in (SELECT CONCAT(LifeCycleDefinition, LifeCycleState) FROM TargetVaultLifeCycles where EntityClassID = 'FILE')
-- RevisionDefinition does not exsist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT ('Invalid RevisionDefinition', Validation_Comment) where RevisionDefinition not in (SELECT RevisionDefinition FROM TargetVaultRevisions) 
-- Identifying UDPs that are used, but not present in target Vault 
Declare @FILE_column_name nvarchar(100)
Declare @Select_Statement_FILE nvarchar(200)
Declare c Cursor For SELECT column_name from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Files' and LEFT(column_name,4) = 'UDP_' and SUBSTRING(column_name, 5, LEN(column_name) - 4) not in (SELECT PropertyName FROM TargetVaultProperties where EntityClassID = 'FILE')
Open c
Fetch next from c into @FILE_column_name
While @@FETCH_STATUS=0 Begin
       Select @Select_Statement_FILE = 'UPDATE Files SET Validation_Status = ''ERROR'', Validation_Comment = CONCAT (''Invalid UDP Name: '+ @FILE_column_name +'; '', Validation_Comment) where [' + @FILE_column_name + '] is not null'
	   EXEC sp_executeSQL @Select_Statement_FILE
       Fetch next from c into @FILE_column_name
End
Close c
Deallocate c

-- FileFileRelations validations
-- FileFileRelation must be either Dependency or Attachment, not same value
UPDATE FileFileRelations SET Validation_Status = 'ERROR', Validation_Comment= CONCAT ('Relation`s logic is false; ',Validation_Comment) where IsAttachment=IsDependency;
-- RefId is not filled, but IsDependency is 'true'
UPDATE FileFileRelations SET Validation_Status = 'ERROR', Validation_Comment= CONCAT ('RefId is not found; ',Validation_Comment) where ParentFileID in (select ParentFileID from FileFileRelations where IsDependency=1 and RefId is NULL) and ChildFileID in (select ChildFileID from FileFileRelations where IsDependency=1 and RefId is NULL);


-- Uncomment next line, if 'Enforce Unique Filenames' in Vault is activated to check which filenames already exist in target Vault
-- UPDATE Files SET Validation_Status = 'ERROR',Validation_Comment = CONCAT('File exsists in target Vault; ', Validation_Comment) where FileName in (SELECT FileName From TargetVaultFiles)