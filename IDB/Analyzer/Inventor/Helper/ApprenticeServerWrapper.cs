using System;
using System.Collections.Generic;
using IDB.Analyzer.Common;
using IDB.Analyzer.Common.Db;
using Inventor;
using log4net;

namespace IDB.Analyzer.Inventor.Helper
{
    public class ApprenticeServerWrapper
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBAnalyzeInventor");

        private static ApprenticeServerWrapper _instance;
        public static ApprenticeServerWrapper Instance => _instance ?? (_instance = new ApprenticeServerWrapper());

        private ApprenticeServerComponent InvApp { get; set; }
        private ApprenticeServerDocument InvDoc { get; set; }

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

        public void SetProjectFile(string invProjectFile)
        {
            try
            {
                var invProject = InvApp.DesignProjectManager.ActiveDesignProject;
                if (string.IsNullOrEmpty(invProjectFile))
                {
                    Log.Info("Inventor project file not specified. Using active project file!");
                }
                else if (!System.IO.File.Exists(invProjectFile))
                {
                    Log.WarnFormat("Specified Inventor project file '{0}' does not exist. Using active project file!", invProjectFile);
                }
                else
                {
                    invProject = InvApp.DesignProjectManager.DesignProjects.AddExisting(invProjectFile);
                    invProject.Activate(false);
                    Log.InfoFormat("Successfully set Inventor project file '{0}'!", invProjectFile);
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
                Console.WriteLine("Failed to set specified Inventor project file '{0}': {1}", invProjectFile, ex.Message);
                Log.ErrorFormat("Failed to set Inventor project file '{0}'. Using active project file!", invProjectFile);
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
                Console.WriteLine("ApprenticeServerComponent.IsInventorFileFailed(): Failed to check if file is an Inventor file '{0}': {1}", filename, ex.Message);
                Log.Error($"ApprenticeServerComponent.IsInventorFileFailed(): Failed to check if file is an Inventor file '{filename}'", ex);
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

        public void CollectReferenceInformation(long parentFileId, Dictionary<string, FileFileRelation> fileRelations, List<string> missingReferences, List<string> unknownReferences)
        {
            if (InvDoc == null)
                throw new Exception("CollectReferenceInformation(): No document open in Apprentice");

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

        public void CollectOleReferenceInformation(long parentFileId, Dictionary<string, FileFileRelation> fileRelations, List<string> missingOleReferences, List<string> unknownReferences)
        {
            if (InvDoc == null)
                throw new Exception("CollectOleReferenceInformation(): No document open in Apprentice");

            var alreadyHandledReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ReferencedOLEFileDescriptor oleFileDescriptor in InvDoc.ReferencedOLEFileDescriptors)
            {
                var fullFilename = oleFileDescriptor.FullFileName;
                var logicalName = oleFileDescriptor.LogicalName;

                if (alreadyHandledReferences.Contains(fullFilename)) continue; // maybe there are multiple references to the file (not sure if this can happen).
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
                    else
                        unknownReferences.Add(fullFilename);
                }
                else
                    missingOleReferences.Add(fullFilename);
            }
        }
    }
}
