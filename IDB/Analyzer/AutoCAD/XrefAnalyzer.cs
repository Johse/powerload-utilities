using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using IDB.Analyzer.AutoCAD.Properties;
using IDB.Analyzer.Common;
using IDB.Analyzer.Common.Db;
using IDB.Analyzer.Common.Helper;
using log4net;
using File = IDB.Analyzer.Common.Db.File;

namespace IDB.Analyzer.AutoCAD
{
    class XrefAnalyzer
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBAnalyzeAutoCAD");

        public void AnalyzeReferences()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nAnalyzing AutoCAD references!");
            Log.Info("Analyzing AutoCAD references ...");

            if (!string.IsNullOrEmpty(Settings.Default.DifferentLoadLocalFilestorePath) && string.IsNullOrEmpty(Settings.Default.FilestorePath))
            {
                Log.Error("Setting 'FilestorePath' is required if 'DifferentLoadLocalFilestorePath' is set");
                ed.WriteMessage("Setting 'FilestorePath' is required if 'DifferentLoadLocalFilestorePath' is set!");
                return;
            }

            var dwgFilesById = GetDwgFilesById();

            var counter = 1;
            var totalCount = dwgFilesById.Count;

            foreach (var fileEntry in dwgFilesById)
            {
                var lc = counter++;
                var filename = fileEntry.Value.LocalFullFileName.GetReplacedFilename();

                ed.WriteMessage($"\n{lc}/{totalCount}: Analyzing Xrefs for {filename}");
                Log.InfoFormat("Analyzing Xrefs for file '{0}': {1}/{2}", filename, lc, totalCount);

                if (!System.IO.File.Exists(filename))
                {
                    ed.WriteMessage(": File doesn't exist");
                    Log.InfoFormat("File ({0}) '{1}' doesn't exist!", lc, filename);
                    continue;
                }

                try
                {
                    var fileRelationsByName = new Dictionary<string, FileFileRelation>(StringComparer.OrdinalIgnoreCase);

                    if (DataHandler.Instance.FileRelationsById.TryGetValue(fileEntry.Key, out var fileRelations))
                    {
                        foreach (var fileRelation in fileRelations.Values)
                        {
                            if (DataHandler.Instance.FilesById.TryGetValue(fileRelation.ChildFileID, out var file))
                                fileRelationsByName.Add(file.LocalFullFileName.GetReplacedFilename(), fileRelation);
                        }
                    }

                    var db = new Database(false, true);
                    using (db)
                    {
                        db.ReadDwgFile(filename, FileShare.Read, true, null);
                        var missingReferences = new List<string>();
                        var unknownReferences = new List<string>();
                        CollectXrefsInformation(fileEntry.Key, db, fileRelationsByName, missingReferences, unknownReferences);

                        if (missingReferences.Any())
                        {
                            Log.ErrorFormat("File '{0}' has missing references!", filename);
                            foreach (var missingReference in missingReferences)
                                Log.ErrorFormat("Missing reference: {0}", missingReference);
                        }
                        if (unknownReferences.Any())
                        {
                            Log.ErrorFormat("File '{0}' has unknown references!", filename);
                            foreach (var unknownReference in unknownReferences)
                                Log.ErrorFormat("Unknown reference: {0}", unknownReference);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nError processing Xrefs for AutoCAD file '{filename}'");
                    Log.Error($"Error processing Xrefs for AutoCAD file '{filename}'", ex);
                }
            }
            ed.WriteMessage("\nAnalyzing AutoCAD Xrefs. Done!");
            Log.Info("Analyzing AutoCAD Xrefs. Done!");
        }

        private Dictionary<long, File> GetDwgFilesById()
        {
            return (from fileEntry in DataHandler.Instance.FilesById
                where IsAutoCADFile(fileEntry.Value.LocalFullFileName)
                select fileEntry).ToDictionary(fileEntry => fileEntry.Key, fileEntry => fileEntry.Value);
        }

        private bool IsAutoCADFile(string filename)
        {
            // todo - check if it's an Inventor DWG
            return filename.ToLower().EndsWith(".dwg");
        }


        private void CollectXrefsInformation(long parentFileId, Database db, Dictionary<string, FileFileRelation> fileRelations, List<string> missingReferences, List<string> unknownReferences)
        {
            using (var tx = db.TransactionManager.StartTransaction())
            {
                db.ResolveXrefs(true, false);
                var xg = db.GetHostDwgXrefGraph(true);

                for (var i = 0; i < xg.RootNode.NumOut; i++)
                {
                    var xrefNode = xg.RootNode.Out(i) as XrefGraphNode;
                    if (xrefNode == null || xrefNode.IsNested)
                        continue;

                    var bl = tx.GetObject(xrefNode.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                    if (bl == null)
                        continue;

                    var xrefFileName = (xrefNode.Database != null) ? xrefNode.Database.Filename : xrefNode.Name; ;
                    try
                    {
                        if (xrefNode.XrefStatus != XrefStatus.Resolved)
                        {
                            xrefFileName = bl.PathName;
                            missingReferences.Add(xrefFileName);
                        }
                        else
                        {
                            var refId = Convert.ToInt32(bl.Handle.ToString(), 16).ToString();
                            if (fileRelations.TryGetValue(xrefFileName, out var fileRelation))
                            {
                                fileRelation.NeedsResolution = true;
                                fileRelation.IsAttachment = false;
                                fileRelation.IsDependency = true;
                                fileRelation.Source = "AutoCAD";
                                fileRelation.RefId = refId;
                            }
                            else if (DataHandler.Instance.FilesByOrigName.TryGetValue(xrefFileName, out var file))
                            {

                                fileRelation = new FileFileRelation
                                {
                                    ParentFileID = parentFileId,
                                    ChildFileID = file.FileID,
                                    NeedsResolution = true,
                                    IsAttachment = false,
                                    IsDependency = true,
                                    Source = "AutoCAD",
                                    RefId = refId
                                };
                                DataHandler.Instance.NewFileRelations.Add(fileRelation);
                            }
                            else
                                unknownReferences.Add(xrefFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        var nodeInfo = string.IsNullOrEmpty(xrefFileName) ? i.ToString() : xrefFileName;
                        Log.Error($"Failed to add XRef node '{nodeInfo}'", ex);
                    }
                }
            }
        }
    }
}
