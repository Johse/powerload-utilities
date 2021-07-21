using System;
using System.Collections.Generic;
using IDB.Analyzer.Common;
using IDB.Analyzer.Common.Db;
using Inventor;
using log4net;
using System.IO;
using System.Xml.Serialization;

namespace IDB.Analyzer.Inventor.Helper
{
    public class ApprenticeServerWrapper
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBAnalyzeInventor");

        private static ApprenticeServerWrapper _instance;
        public static ApprenticeServerWrapper Instance => _instance ?? (_instance = new ApprenticeServerWrapper());

        private ApprenticeServerComponent InvApp { get; set; }
        private ApprenticeServerDocument InvDoc { get; set; }

        private bool CreateNewFileRelationships { get; set; }

        private ApprenticeServerWrapper()
        {
            InvApp = new ApprenticeServerComponent();
        }

        private HashSet<string> _libraryFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Close()
        {
            if (InvApp != null)
                InvApp.Close();
        }

        public void SetCreateNewFileRelationships(bool bValue)
        {
            CreateNewFileRelationships = bValue;
        }

        public void SetProjectFile(string invProjectFile)
        {
            try
            {
                var invProject = InvApp.DesignProjectManager.ActiveDesignProject;
                if (string.IsNullOrEmpty(invProjectFile))
                {
                    Log.Info("Inventor project childFileRecord not specified. Using active project childFileRecord!");
                }
                else if (!System.IO.File.Exists(invProjectFile))
                {
                    Log.WarnFormat("Specified Inventor project childFileRecord '{0}' does not exist. Using active project childFileRecord!", invProjectFile);
                }
                else
                {
                    invProject = InvApp.DesignProjectManager.DesignProjects.AddExisting(invProjectFile);
                    invProject.Activate(false);
                    Log.InfoFormat("Successfully set Inventor project childFileRecord '{0}'!", invProjectFile);
                }

                var invProjectFolder = System.IO.Path.GetDirectoryName(invProject.FullFileName);
                char[] charsToTrim = { '\\', '/' };
                _libraryFolders.Add(invProject.ContentCenterPath.TrimEnd(charsToTrim));
                foreach (ProjectPath libpath in invProject.LibraryPaths)
                {
                    var p = libpath.Path.StartsWith(".")
                        ? invProjectFolder + libpath.Path.TrimStart('.')
                        : libpath.Path;
                    _libraryFolders.Add(p.TrimEnd(charsToTrim));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to set specified Inventor project childFileRecord '{0}': {1}", invProjectFile, ex.Message);
                Log.ErrorFormat("Failed to set Inventor project childFileRecord '{0}'. Using active project childFileRecord!", invProjectFile);
            }
        }

        public bool IsInventorFile(string filename)
        {
            try
            {
                if (filename.ToLower().EndsWith(".iam") || filename.ToLower().EndsWith(".ipt") || filename.ToLower().EndsWith(".idw") || filename.ToLower().EndsWith(".ipn"))
                    return true;
                if (filename.ToLower().EndsWith("dwg") && InvApp.FileManager.IsInventorDWG(filename))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ApprenticeServerComponent.IsInventorFileFailed(): Failed to check if childFileRecord is an Inventor childFileRecord '{0}': {1}", filename, ex.Message);
                Log.Error($"ApprenticeServerComponent.IsInventorFileFailed(): Failed to check if childFileRecord is an Inventor childFileRecord '{filename}'", ex);
                return false;
            }
        }

        public bool IsLibraryFile(string filename, out string libraryFolder)
        {
            libraryFolder = null;
            foreach (var libFolder in _libraryFolders)
            {
                if (!filename.StartsWith(libFolder))
                    continue;
                libraryFolder = libFolder;
                return true;
            }
            return false;
        }

        public bool OpenDocument(string filename)
        {
            try
            {
                InvDoc = InvApp.Open(filename);
                return (InvDoc != null);
            }
            catch (Exception ex)
            {
                var msg = $"\r\nOpen ERROR!File {filename} could not be opened";
                Console.WriteLine(msg);
                Log.Error($"File {filename} could not be opened", ex);
                return false;
            }
        }

        public void CloseDocument()
        {
            if (InvDoc != null)
                InvDoc.Close();
        }

        public void CollectReferenceInformationByFullFilePath(long parentFileId, Dictionary<string, FileFileRelation> fileRelations, List<string> missingReferences, List<string> unknownReferences)
        {
            if (InvDoc == null)
                throw new Exception("CollectReferenceInformationByFullFilePath(): No document open in Apprentice");

            // use ReferenceInfo to determine the refId (needed for Vault BCP export)
            var refInfo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var dbRevId = string.Empty;
            var lastSavedLocation = string.Empty;
            object indices;
            object oldPaths;
            object currentPaths;
            InvDoc._GetReferenceInfo(out dbRevId, out lastSavedLocation, out indices, out oldPaths, out currentPaths, true);

            var indicesA = indices as int[];
            var oldPathsA = oldPaths as string[];
            var currentPathsA = currentPaths as string[];
            if (currentPathsA != null && oldPathsA != null && indicesA != null &&
                currentPathsA.Length == oldPathsA.Length && currentPathsA.Length == indicesA.Length)
            {
                for (var i = 0; i < currentPathsA.Length; i++)
                    refInfo.Add((currentPathsA[i] == null || currentPathsA[i] == string.Empty) ? oldPathsA[i] : currentPathsA[i], indicesA[i]);
            }

            var alreadyHandledReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DocumentDescriptor docDescriptor in InvDoc.ReferencedDocumentDescriptors)
            {
                var fullFilename = docDescriptor.ReferencedFileDescriptor.FullFileName;

                if (alreadyHandledReferences.Contains(fullFilename)) continue; // an IDW can have multiple references to the same IAM when LODs are used 
                alreadyHandledReferences.Add(fullFilename);

                var refId = (refInfo.ContainsKey(fullFilename) ? refInfo[fullFilename] : -1).ToString();
                if (!docDescriptor.ReferenceMissing)
                {
                    if (fileRelations.TryGetValue(fullFilename, out var fileRelation))
                    {
                        fileRelation.NeedsResolution = true;
                        fileRelation.IsAttachment = false;
                        fileRelation.IsDependency = true;
                        fileRelation.Source = "INVENTOR";
                        fileRelation.RefId = refId;
                    }
                    else if (DataHandler.Instance.FilesByOrigName.TryGetValue(fullFilename, out var file))
                    {
                        if (CreateNewFileRelationships)
                        {
                            fileRelation = new FileFileRelation
                            {
                                ParentFileID = parentFileId,
                                ChildFileID = file.FileID,
                                NeedsResolution = true,
                                IsAttachment = false,
                                IsDependency = true,
                                Source = "INVENTOR",
                                RefId = refId
                            };
                            DataHandler.Instance.NewFileRelations.Add(fileRelation);
                        }
                    }
                    else
                        unknownReferences.Add(fullFilename);
                }
                else
                {
                    if (!docDescriptor.ReferenceSuppressed || !System.IO.File.Exists(fullFilename))
                        missingReferences.Add(fullFilename);
                }
            }
        }

        public void CollectReferenceInformationByFilename(Common.Db.File parentFileRecord, Dictionary<string, FileFileRelation> fileRelations,
                                                            List<string> addedReferences, List<string> unknownReferences, ref ProcessingStatistics processStatistics)
        {
            if (InvDoc == null)
                throw new Exception("CollectReferenceInformationByFilename(): No document open in Apprentice");

            // use ReferenceInfo to determine the refId (needed for Vault BCP export)
            var refInfo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var dbRevId = string.Empty;
            var lastSavedLocation = string.Empty;
            object indices;
            object oldPaths;
            object currentPaths;
            InvDoc._GetReferenceInfo(out dbRevId, out lastSavedLocation, out indices, out oldPaths, out currentPaths, true);

            var indicesA = indices as int[];
            var oldPathsA = oldPaths as string[];
            var currentPathsA = currentPaths as string[];

            // NOTE: we are using just the filename for comparison
            if (currentPathsA != null && oldPathsA != null && indicesA != null &&
                currentPathsA.Length == oldPathsA.Length && currentPathsA.Length == indicesA.Length)
            {
                for (var i = 0; i < currentPathsA.Length; i++)
                    refInfo.Add((currentPathsA[i] == null || currentPathsA[i] == string.Empty) ? System.IO.Path.GetFileName(oldPathsA[i]) : System.IO.Path.GetFileName(currentPathsA[i]), indicesA[i]);
            }

            var alreadyHandledReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DocumentDescriptor docDescriptor in InvDoc.ReferencedDocumentDescriptors)
            {
                var filename = System.IO.Path.GetFileName(docDescriptor.ReferencedFileDescriptor.FullFileName);

                if (alreadyHandledReferences.Contains(filename))
                {
                    // an IDW can have multiple references to the same IAM when LODs are used 
                    // TODO: should we set the IDBAnalyzed of all records that match that filename
                    // or should we expect only one record to exist?
                    continue; 
                }

                alreadyHandledReferences.Add(filename);

                var refId = (refInfo.ContainsKey(filename) ? refInfo[filename] : -1).ToString();

                // we don't care that docDescriptor.ReferenceMissing if we know about the childFileRecord in the IDB dataset
                if (fileRelations.TryGetValue(filename, out var fileRelation))
                {
                    // check to see the integrity of the record and set flags
                    // identify that that fileRelation has been processed
                    CheckRelationAndSetFlags(parentFileRecord, fileRelation, false, ref processStatistics);

                    fileRelation.NeedsResolution = true;
                    fileRelation.IsAttachment = false;
                    fileRelation.IsDependency = true;
                    fileRelation.Source = "INVENTOR";
                    fileRelation.RefId = refId;
                }
                else if (DataHandler.Instance.FilesByOrigName.TryGetValue(filename, out var childFileRecord))
                {
                    if (CreateNewFileRelationships)
                    {
                        fileRelation = new FileFileRelation
                        {
                            ParentFileID = parentFileRecord.FileID,
                            ChildFileID = childFileRecord.FileID,
                            NeedsResolution = true,
                            IsAttachment = false,
                            IsDependency = true,
                            Source = "INVENTOR",
                            RefId = refId,
                            IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileRelation.IDBAnalyzeNotes, "Dependency Relation added by IDBAnalyzeInventor"),
                            IDBAnalyzed = true
                        };

                        DataHandler.Instance.NewFileRelations.Add(fileRelation);

                        // set the parentFileRecord.IDBAnalyzeNotes and IDBAnalyzeNotes.
                        parentFileRecord.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(parentFileRecord.IDBAnalyzeNotes, string.Format("Dependency Relation added by IDBAnalyzeInventor: {0}", filename));
                        parentFileRecord.HasRelationshipIssues = true;
                    }

                    addedReferences.Add(filename);
                }
                else
                {
                    // set the record for the database
                    parentFileRecord.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(parentFileRecord.IDBAnalyzeNotes, string.Format("Unknown Ref: {0}", filename));
                    parentFileRecord.HasRelationshipIssues = true;

                    unknownReferences.Add(filename);
                }
            }
        }

        // check integrity of the record
        private void CheckRelationAndSetFlags(Common.Db.File parentFileRecord, FileFileRelation fileRelation, bool bIsOleReference, ref ProcessingStatistics processStatistics)
        {
            // check to see the integrity of the record and set flags
            // identify that that fileRelation has been processed
            fileRelation.IDBAnalyzed = true;

            if (fileRelation.IsAttachment)
            {
                processStatistics.IsAttatchmentModified++;

                parentFileRecord.HasRelationshipIssues = true;
                fileRelation.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileRelation.IDBAnalyzeNotes, "Original Set As Attachment, but is: " + (bIsOleReference ? "OleRef" : "Dependency"));
            }
            else if (bIsOleReference && (fileRelation.IsDependency == true))
            {
                processStatistics.IsDependencyModified++;

                parentFileRecord.HasRelationshipIssues = true;
                fileRelation.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileRelation.IDBAnalyzeNotes, "Original Set As Dependency but is: OleRef");
            }
            else if (!bIsOleReference && (fileRelation.IsDependency == false))
            {
                processStatistics.IsOLEModified++;

                parentFileRecord.HasRelationshipIssues = true;
                fileRelation.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileRelation.IDBAnalyzeNotes, "Original Set As OleRef but is: Dependency");
            }

        }


        public void CollectOleReferenceInformationByFullFilePath(long parentFileId, Dictionary<string, FileFileRelation> fileRelations, List<string> missingOleReferences, List<string> unknownReferences)
        {
            if (InvDoc == null)
                throw new Exception("CollectOleReferenceInformationByFullFilePath(): No document open in Apprentice");

            var alreadyHandledReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ReferencedOLEFileDescriptor oleFileDescriptor in InvDoc.ReferencedOLEFileDescriptors)
            {
                var fullFilename = oleFileDescriptor.FullFileName;
                var logicalName = oleFileDescriptor.LogicalName;

                if (alreadyHandledReferences.Contains(fullFilename)) continue; // maybe there are multiple references to the childFileRecord (not sure if this can happen).
                alreadyHandledReferences.Add(fullFilename);

                int idx;
                if (string.IsNullOrEmpty(logicalName) || (idx = logicalName.IndexOf(':', 0)) == -1 ||
                    !int.TryParse(logicalName.Substring(0, logicalName.IndexOf(':', 0)), out idx))
                {
                    Log.ErrorFormat($"Could not determine OLE index of {fullFilename}, LogicalName = {logicalName}");
                    missingOleReferences.Add(fullFilename);
                    continue;
                }

                if (oleFileDescriptor.ReferenceStatus != ReferenceStatusEnum.kMissingReference)
                {
                    if (fileRelations.TryGetValue(fullFilename, out var fileRelation))
                    {
                        fileRelation.NeedsResolution = true;
                        fileRelation.IsAttachment = false;
                        fileRelation.IsDependency = true;
                        fileRelation.Source = "INVENTOR";
                        fileRelation.RefId = $"Foreign:{idx}";
                    }
                    else if (DataHandler.Instance.FilesByOrigName.TryGetValue(fullFilename, out var file))
                    {
                        if (CreateNewFileRelationships)
                        {
                            fileRelation = new FileFileRelation
                            {
                                ParentFileID = parentFileId,
                                ChildFileID = file.FileID,
                                NeedsResolution = true,
                                IsAttachment = false,
                                IsDependency = true,
                                Source = "INVENTOR",
                                RefId = $"Foreign:{idx}"
                            };
                            DataHandler.Instance.NewFileRelations.Add(fileRelation);
                        }
                    }
                    else
                        unknownReferences.Add(fullFilename);
                }
                else
                    missingOleReferences.Add(fullFilename);
            }
        }

        public void CollectOleReferenceInformationByFilename(Common.Db.File parentFileRecord, Dictionary<string, FileFileRelation> fileRelations,
                                                                List<string> oleIndexIssuesReferences, List<string> addedOleReferences, List<string> unknownReferences, ref ProcessingStatistics processStatistics)
        {
            if (InvDoc == null)
                throw new Exception("CollectOleReferenceInformationByFilename(): No document open in Apprentice");

            var alreadyHandledReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ReferencedOLEFileDescriptor oleFileDescriptor in InvDoc.ReferencedOLEFileDescriptors)
            {
                // NOTE: we are using just the filename for comparison
                var filename = System.IO.Path.GetFileName(oleFileDescriptor.FullFileName);
                var logicalName = oleFileDescriptor.LogicalName;

                if (alreadyHandledReferences.Contains(filename))
                {
                    // maybe there are multiple references to the childFileRecord (not sure if this can happen).
                    continue; 
                }

                alreadyHandledReferences.Add(filename);

                int idx;
                if (string.IsNullOrEmpty(logicalName) || (idx = logicalName.IndexOf(':', 0)) == -1 ||
                    !int.TryParse(logicalName.Substring(0, logicalName.IndexOf(':', 0)), out idx))
                {
                    // create the error string
                    string sErrorString = $"Could not determine OLE index of {filename}, LogicalName = {logicalName}";
                    Log.ErrorFormat(sErrorString);
                    oleIndexIssuesReferences.Add(filename);

                    // set the parentFileRecord.IDBAnalyzeNotes and IDBAnalyzeNotes.HasRelationshipIssues
                    parentFileRecord.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(parentFileRecord.IDBAnalyzeNotes, sErrorString);
                    parentFileRecord.HasRelationshipIssues = true;

                    continue;
                }

                // we don't care that docDescriptor.ReferenceMissing if we know about the childFileRecord in the IDB dataset
                if (fileRelations.TryGetValue(filename, out var fileRelation))
                {
                    // check to see the integrity of the record and set flags
                    // identify that that fileRelation has been processed
                    CheckRelationAndSetFlags(parentFileRecord, fileRelation, true, ref processStatistics);

                    fileRelation.NeedsResolution = true;
                    fileRelation.IsAttachment = false;
                    fileRelation.IsDependency = false;
                    fileRelation.Source = "INVENTOR";
                    fileRelation.RefId = $"Foreign:{idx}";
                }
                else if (DataHandler.Instance.FilesByOrigName.TryGetValue(filename, out var file))
                {
                    if (CreateNewFileRelationships)
                    {
                        fileRelation = new FileFileRelation
                        {
                            ParentFileID = parentFileRecord.FileID,
                            ChildFileID = file.FileID,
                            NeedsResolution = true,
                            IsAttachment = false,
                            IsDependency = true,
                            Source = "INVENTOR",
                            RefId = $"Foreign:{idx}",
                            IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileRelation.IDBAnalyzeNotes, "OLE Relation added by IDBAnalyzeInventor"),
                            IDBAnalyzed = true
                        };
                        DataHandler.Instance.NewFileRelations.Add(fileRelation);

                        // set the parentFileRecord.IDBAnalyzeNotes and IDBAnalyzeNotes.HasRelationshipIssues
                        parentFileRecord.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(parentFileRecord.IDBAnalyzeNotes, string.Format("OLE Relation added by IDBAnalyzeInventor: {0}", filename));
                        parentFileRecord.HasRelationshipIssues = true;
                    }

                    addedOleReferences.Add(filename);
                }
                else
                {
                    // set the record for the database
                    parentFileRecord.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(parentFileRecord.IDBAnalyzeNotes, string.Format("Unknown OLE Ref: {0}", filename));
                    parentFileRecord.HasRelationshipIssues = true;

                    unknownReferences.Add(filename);
                }
            }
        }

        // method to create new or add string to existing string
        // add Environment.NewLine
        public static string AddOrNewComment(string sExistingString, string sNewComment)
        {
            string sResultsString = "";
            if (string.IsNullOrWhiteSpace(sExistingString))
            {
                sResultsString = sNewComment;
            }
            else
            {
                sResultsString = sExistingString + System.Environment.NewLine + sNewComment;
            }

            return (sResultsString);
        }
    }

    // object that handles issues and reports to the users on the display at the end of processing
    public class ProcessingStatistics
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBAnalyzeInventor");

        [XmlElement]
        public long NumberToProcess { get; set; }

        [XmlElement]
        public long TotalProcessed { get; set; }

        [XmlElement]
        public long TotalInventor { get; set; }


        [XmlElement]
        public long MissingLocalFiles { get; set; }

        [XmlElement]
        public bool UserCanceledProcess { get; set; }

        [XmlElement]
        public long InventorFailedToOpen { get; set; }

        [XmlElement]
        public long ExceptionsProcessed { get; set; }


        [XmlElement]
        public long NumberWithMissingReferences { get; set; }

        [XmlElement]
        public long NumberWithAddedReferences { get; set; }

        [XmlElement]
        public long NumberWithOLEIndexIssueReferences { get; set; }

        [XmlElement]
        public long NumberWithMissingOLEReferences { get; set; }

        [XmlElement]
        public long NumberWithAddedOLEReferences { get; set; }

        [XmlElement]
        public long NonInventorFiles { get; set; }

        [XmlElement]
        public long IsAttatchmentModified { get; set; }

        [XmlElement]
        public long IsDependencyModified { get; set; }

        [XmlElement]
        public long IsOLEModified { get; set; }

        // method to log and report the statistics
        public void LogAndReportStatistics()
        {
            string sOutputString = System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NumberToProcess", this.NumberToProcess);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "TotalProcessed", this.TotalProcessed);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "TotalInventor", this.TotalInventor);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "MissingLocalFiles", this.MissingLocalFiles);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "UserCanceledProcess", this.UserCanceledProcess);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "InventorFailedToOpen", this.InventorFailedToOpen);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "ExceptionsProcessed", this.ExceptionsProcessed);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NumberWithMissingReferences", this.NumberWithMissingReferences);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NumberWithAddedReferences", this.NumberWithAddedReferences);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NumberWithOLEIndexIssueReferences", this.NumberWithOLEIndexIssueReferences);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NumberWithMissingOLEReferences", this.NumberWithMissingOLEReferences);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NumberWithAddedOLEReferences", this.NumberWithAddedOLEReferences);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "NonInventorFiles", this.NonInventorFiles);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "IsAttatchmentModified", this.IsAttatchmentModified);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "IsDependencyModified", this.IsDependencyModified);
            sOutputString += System.Environment.NewLine + string.Format("{0}\t{1:n0}", "IsOLEModified", this.IsOLEModified);

            Console.WriteLine(sOutputString);
            Log.Info(sOutputString);
        }

    }
}
