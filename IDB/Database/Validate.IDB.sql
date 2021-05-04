-- COOLORANGE sample SQL scripts for validating the powerLoad database prior to exporting it to a BCP-package
-- The scripts fill the field 'Validation_Status' with either 'ERROR' or 'WARN'
-- The scripts fill the field 'Validation_Comment' with a short hint AND adds the already existing content
-- COOLORANGE assumes no responibility for completeness
-- Extend the SQL scripts to your needs

-- Creation of fields for Validation Results if not exist
--IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Validation_Comment' AND [object_id] = OBJECT_ID(N'Folders')) BEGIN ALTER TABLE Folders ADD Validation_Comment varchar(max), Validation_Status varchar(20) END; 
--IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Validation_Comment' AND [object_id] = OBJECT_ID(N'Files')) BEGIN ALTER TABLE Files ADD Validation_Comment varchar(max), Validation_Status varchar(20) END;
--IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Validation_Comment' AND [object_id] = OBJECT_ID(N'FileFileRelations')) BEGIN ALTER TABLE FileFileRelations ADD Validation_Comment varchar(max), Validation_Status varchar(20) END;

-- Deleting all values in 'Validation_Comment' AND 'Validation_Status' to initialize the validation
PRINT 'Initializing fields...'
UPDATE Files SET Validation_Comment = NULL WHERE Validation_Comment IS NOT NULL
UPDATE Files SET Validation_Status = NULL WHERE Validation_Status IS NOT NULL
UPDATE Folders SET Validation_Comment = NULL WHERE Validation_Comment IS NOT NULL
UPDATE Folders SET Validation_Status = NULL WHERE Validation_Status IS NOT NULL
UPDATE FileFileRelations SET Validation_Comment = NULL WHERE Validation_Comment IS NOT NULL
UPDATE FileFileRelations SET Validation_Status = NULL WHERE Validation_Status IS NOT NULL

-- Folder validations
-- User does not exist in target Vault
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('User ''', CreateUser, ''' does not exist in Vault; ', Validation_Comment) WHERE CreateUser NOT IN (SELECT UserName FROM TargetVaultUsers)
-- Duplicate folders
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Duplicate folder; ', Validation_Comment) WHERE Path IN (SELECT Path FROM folders GROUP BY FolderName, Path HAVING COUNT(FolderName) > 1)
-- Mandatory folder data
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Folder data missing; ', Validation_Comment) WHERE FolderID IN (SELECT FolderID FROM Folders WHERE FolderName IS NULL OR Category IS NULL OR CreateUser IS NULL OR CreateDate IS NULL OR FolderID IN (SELECT FolderID FROM Folders WHERE Path IS NULL AND ParentFolderID IS NULL))
-- Folder endless loop
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Infinite nested folders; ', Validation_Comment) WHERE FolderId IN (SELECT p.FolderId FROM Folders c JOIN Folders p on c.ParentFolderID = p.FolderID WHERE p.ParentFolderID = c.FolderID)
-- Category does not exist in target Vault
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid Category; ', Validation_Comment) WHERE Category NOT IN (SELECT Category FROM TargetVaultCategories WHERE EntityClassID = 'FLDR')
-- LifecycleDefinition does not exist in target Vault
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid Lifecycle Definition; ', Validation_Comment) WHERE LifecycleDefinition NOT IN (SELECT LifeCycleDefinition FROM TargetVaultLifeCycles WHERE EntityClassID = 'FLDR')
-- LifecycleState does not exist in target Vault for assigned LifecycleDefinition
UPDATE Folders SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid LifecycleState; ', Validation_Comment) WHERE CONCAT(LifecycleDefinition, LifecycleState) NOT IN (SELECT CONCAT(LifeCycleDefinition, LifeCycleState) FROM TargetVaultLifeCycles WHERE EntityClassID = 'FLDR') AND LifecycleDefinition IS NOT NULL
-- Identifying UDPs that are not present in target Vault
DECLARE @FLDR_column_name nvarchar(100)
DECLARE @FLDR_Statement nvarchar(400)
DECLARE c CURSOR FOR SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Folders' AND LEFT(column_name,4) = 'UDP_' AND SUBSTRING(column_name, 5, LEN(column_name) - 4) NOT IN (SELECT PropertyName FROM TargetVaultProperties WHERE EntityClassID = 'FLDR')
OPEN c
FETCH NEXT FROM c INTO @FLDR_column_name
WHILE @@FETCH_STATUS = 0 Begin
    SELECT @FLDR_Statement = 'UPDATE Folders SET Validation_Status = ''ERROR'', Validation_Comment = CONCAT(''Invalid UDP Name: ' + @FLDR_column_name + '; '', Validation_Comment) WHERE [' + @FLDR_column_name + '] IS NOT NULL'
    EXEC sp_executeSQL @FLDR_Statement
    FETCH NEXT FROM c INTO @FLDR_column_name
END
CLOSE c
DEALLOCATE c

-- File validations
-- User does not exist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('User ''', CreateUser, ''' does not exist in Vault; ', Validation_Comment) WHERE CreateUser NOT IN (SELECT UserName FROM TargetVaultUsers)
-- Mandatory files data
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('File data is missing; ', Validation_Comment) WHERE FileID IN (SELECT FileID FROM Files WHERE LocalFullFileName IS NULL OR FileName IS NULL OR Category IS NULL OR Classification IS NULL OR Version IS NULL OR CreateUser IS NULL OR CreateDate IS NULL)
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('CreateDate not in ascending order; ', Validation_Comment) WHERE FileId IN (SELECT f.FileID FROM Files f JOIN Files fm on f.FileName = fm.FileName WHERE f.Version < fm.Version AND f.CreateDate > fm.CreateDate)
-- Full filename is longer than 260 characters
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Full filename is longer than 260 characters: ', LEN (CONCAT(fo.Path, '/', f.FileName)), '; ', f.Validation_Comment) FROM Files f INNER JOIN Folders fo on fo.FolderID = f.FolderID WHERE LEN (CONCAT(fo.Path, '/', f.FileName)) > 260
-- Category does not exist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid Category; ', Validation_Comment) WHERE Category NOT IN (SELECT Category FROM TargetVaultCategories WHERE EntityClassID = 'FILE')
-- LifecycleDefinition does not exist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid LifecycleDefinition; ', Validation_Comment) WHERE LifecycleDefinition NOT IN (SELECT LifeCycleDefinition FROM TargetVaultLifeCycles WHERE EntityClassID = 'FILE')
-- LifecycleState does not exist in target Vault for assigned LifecycleDefinition
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid LifecycleState; ', Validation_Comment) WHERE CONCAT(LifecycleDefinition, LifecycleState) NOT IN (SELECT CONCAT(LifeCycleDefinition, LifeCycleState) FROM TargetVaultLifeCycles WHERE EntityClassID = 'FILE')
-- RevisionDefinition does not exsist in target Vault
UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Invalid RevisionDefinition; ', Validation_Comment) WHERE RevisionDefinition NOT IN (SELECT RevisionDefinition FROM TargetVaultRevisions) 
-- Identifying UDPs that are used, but not present in target Vault 
DECLARE @FILE_column_name nvarchar(100)
DECLARE @FILE_Statement nvarchar(400)
DECLARE c CURSOR FOR SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Files' AND LEFT(column_name,4) = 'UDP_' AND SUBSTRING(column_name, 5, LEN(column_name) - 4) NOT IN (SELECT PropertyName FROM TargetVaultProperties WHERE EntityClassID = 'FILE')
OPEN c
FETCH NEXT FROM c INTO @FILE_column_name
WHILE @@FETCH_STATUS = 0 Begin
    SELECT @FILE_Statement = 'UPDATE Files SET Validation_Status = ''ERROR'', Validation_Comment = CONCAT(''Invalid UDP Name: ' + @FILE_column_name + '; '', Validation_Comment) WHERE [' + @FILE_column_name + '] IS NOT NULL'
    EXEC sp_executeSQL @FILE_Statement
    FETCH NEXT FROM c INTO @FILE_column_name
END
CLOSE c
DEALLOCATE c

-- FileFileRelation validations
-- FileFileRelation must be either Dependency or Attachment, not same value
UPDATE FileFileRelations SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('Relation must be either ''Attachment'' or ''Dependency''; ', Validation_Comment) WHERE IsAttachment = IsDependency
-- RefId IS NOT filled, but IsDependency is 'true'
UPDATE FileFileRelations SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('RefId not found; ', Validation_Comment) WHERE ParentFileID IN (SELECT ParentFileID FROM FileFileRelations WHERE IsDependency = 1 AND RefId IS NULL) AND ChildFileID IN (SELECT ChildFileID FROM FileFileRelations WHERE IsDependency = 1 AND RefId IS NULL)


-- Uncomment next line, if 'Enforce Unique Filenames' in Vault is activated to check which filenames already exist in target Vault
-- UPDATE Files SET Validation_Status = 'ERROR', Validation_Comment = CONCAT('File exsists in target Vault; ', Validation_Comment) WHERE FileName IN (SELECT FileName FROM TargetVaultFiles)