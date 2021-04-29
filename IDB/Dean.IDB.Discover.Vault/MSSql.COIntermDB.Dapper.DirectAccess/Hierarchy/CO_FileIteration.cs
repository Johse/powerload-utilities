using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using BCPBuilderConfig;

using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;
using MSSql.COIntermDB.Dapper.DirectAccess.DbLink;
using MSSql.COIntermDB.Dapper.DirectAccess.DbRelation;


using bcpDevKit;
using bcpDevKit.Entities;
using bcpDevKit.Entities.Items;
using bcpDevKit.Entities.Vault;
using bcpDevKit.Entities.Configuration;
using bcpDevKit.Entities.General;


namespace MSSql.COIntermDB.Dapper.DirectAccess.Hierarchy
{
    // class to manage the original CO File record file iteration
    // relationships to file Revisions
    // relationship to its file Master
    public class CO_FileIteration
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_FileIteration));

        // manage the file Iteration record for this FileIteration
        public File mFileIterationRecord { get; private set; }

        public string mFileName { get; private set; }
        public int mFileId { get; private set; }
        public int mFolderId { get; private set; }

        // manage who owns this object
        public CO_FileRevision mCO_FileRevisionOwner { get; private set; }
        public CO_FileMaster mCO_FileMasterOwner { get; private set; }

        // TODO: properties that manage the relationships between CoFileIteration
        // NOTE: Deublin did not have any file to file relationships


        public List<CO_FileIteration> mChild_CO_FileIterations { get; private set; }
        public List<CO_FileIteration> mParent_CO_FileIterations { get; private set; }
        public Dictionary<int, FileFileRelation> mFileFileRelationDictByChildId { get; private set; }

        // manage the Item to File relationships
        public List<CO_ItemIteration> mParent_CO_ItemIterations { get; private set; }

        // manage the VaultBCP content for building the data loader package
        public FileIteration mBcpFileIteration { get; set; }




        // constructor
        public CO_FileIteration(File iterationRecord)
        {
            // assign the properties
            mFileIterationRecord = iterationRecord;
            mFileName = iterationRecord.FileName;
            mFileId = iterationRecord.FileID;
            mFolderId = iterationRecord.FolderID;

            mChild_CO_FileIterations = new List<CO_FileIteration>();
            mParent_CO_FileIterations = new List<CO_FileIteration>();
            mFileFileRelationDictByChildId = new Dictionary<int, FileFileRelation>();

            mParent_CO_ItemIterations = new List<CO_ItemIteration>();
        }

        // method to build the CoFileIterations
        public static bool BuildFileIterations(Dictionary<int, File> iterationsDict, ref Dictionary<int, CO_FileIteration> coFileIterationDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildBCPFileIterations(): Start");

            bool bSuccess = false;

            // iterate each of these and build the file Iterations
            List<CO_FileIteration> cofList = new List<CO_FileIteration>();
            foreach (File fileIteration in iterationsDict.Values)
            {
                // create the new CoFileIteration
                CO_FileIteration cof = new CO_FileIteration(fileIteration);

                cofList.Add(cof);
            }

            // create the dictionary
            coFileIterationDict = cofList.ToDictionary(cof => cof.mFileId, cof => cof);

            // log total time taken
            Logger.Debug(string.Format("BuildBCPFileIterations(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to set FileIteration relationships
        public static void AssignFileIterationRelationships(Dictionary<int, CO_FileIteration> co_FileIterationDict, List<FileFileRelation> fileFileRelationList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignFileIterationRelationships(): Start");

            // build lookup for parents grouping of FileFileRelation
            ILookup<int, FileFileRelation> associationParentLookup = fileFileRelationList.ToLookup(ass => ass.ParentFileID, ass => ass);

            // iterate through each, and assign relationships
            int nAssigned = 0;
            int nParents = 0;
            foreach (IGrouping<int, FileFileRelation> groupItem in associationParentLookup)
            {
                // groupItem.Key = ParentFileID<int>
                // groupItem is IEnumerable<FileFileRelation>

                nParents++;

                // get the parent CO_FileIteration
                CO_FileIteration parentCO_FileIteration = co_FileIterationDict[groupItem.Key];

                // iterate over the associations and get children
                foreach (FileFileRelation ffr in groupItem)
                {
                    nAssigned++;

                    // get the child CO_FileIteration
                    CO_FileIteration childCO_FileIteration = co_FileIterationDict[ffr.ChildFileID];

                    // add the child to the list, and add the parent to the child
                    parentCO_FileIteration.mChild_CO_FileIterations.Add(childCO_FileIteration);
                    childCO_FileIteration.mParent_CO_FileIterations.Add(parentCO_FileIteration);

                    // add the Association record
                    parentCO_FileIteration.mFileFileRelationDictByChildId.Add(ffr.ChildFileID, ffr);
                }
            }

            // log information
            Logger.Debug(string.Format("Parents/Children: {0:n0}/{0:n0}", nParents, nAssigned));

            // output a warning if no parents or children were assigned
            if ((nParents == 0) || (nAssigned == 0))
            {
                Logger.Debug("!!!Warning: No file parents or children were assigned");
            }

            // log total time taken
            Logger.Debug(string.Format("AssignFileIterationRelationships(): End {0}", swp.ElapsedTimeString()));

        }





        // set the CO_FileRevision as owner of this object
        public void SetCO_FileRevisionOwner(CO_FileRevision co_FileRevision)
        {
            mCO_FileRevisionOwner = co_FileRevision;
        }

        // set the CO_FileMaster as owner of this object
        public void SetCO_FileMasterOwner(CO_FileMaster co_FileMaster)
        {
            mCO_FileMasterOwner = co_FileMaster;
        }


    }
}
