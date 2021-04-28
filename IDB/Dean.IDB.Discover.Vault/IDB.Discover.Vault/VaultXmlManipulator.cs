using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using BCPBuilderConfig;

using VBXS = VaultBCPXSDSerialization;
using MCDD = MSSql.COIntermDB.Dapper.DirectAccess;
using MVDD = MSSql.Vault.Dapper.DirectAccess;


namespace IDB.Discover.Vault
{
    // class that can open a "Vault.xml" file and manipulate, remove, adjust, insert
    // content in to fit requirements
    // this should be used post "IDB.Translate.BCP"
    public class VaultXmlManipulator
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultXmlManipulator));


        public static void ModifyVaultXmlFile(string sVaultXMLFile, List<CO_IDB_VaultFileMaster> co_IDB_VaultFileMaster_List)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // log total time taken
            Logger.Debug(string.Format("ModifyVaultXmlFile(): start \"{0}\"", sVaultXMLFile));

            // read the Vault.xml
            // assumed that sSourceVaultXml is the source Vault with security, behaviours, and files
            Logger.Debug("Load Vault.xml: start");
            VBXS.Vault sourceVault = VBXS.VaultBCPSerialization.GetVault(sVaultXMLFile);
            Logger.Debug(string.Format("Loaded Vault.xml: End {0}", swp.LapTimeString()));

            // delete the Security and Behaviours section
            sourceVault.Security = null;
            sourceVault.Behaviors = null;

            // get all the files
            // this generates a dictionary with all the Vault files, Key = full Vault file path (StringComparer.CurrentCultureIgnoreCase)
            Dictionary<string, VBXS.File> sourceFileDictionaryByFullPath = GetSourceVaultXmlFileDictionary(sourceVault);

            // get the files that are in the Vault by full vault file path
            // this is from the IDB
            List<CO_IDB_VaultFileMaster> filesMatchingFullPath = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => (coivfm.mVaultFileMaster != null)).ToList();
            List<CO_IDB_VaultFileMaster> filesWithDiffLocalVaultChecksum = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => ((coivfm.mVaultFileMaster != null) && !coivfm.mChecksumsMatch)).ToList();

            // iterate through each of the file masters (from IDB and from target Vault) from CO_IDB_VaultFileMaster.Combine_CO_IDB_And_VaultCache
            foreach (CO_IDB_VaultFileMaster civfm in filesMatchingFullPath)
            {
                // many assumptions here, as defined throughout code base
                // only one version in IDB
                // only one version in target Vault
                // the filesMatchingFullPath will all need to be visited
                // filesWithDiffLocalVaultChecksum will need to have a FileIterationRef pointing to the original version
                // AND will need the new FileIteration from the IDB and as it exists in the Vault.xml file
                // the remaining files in filesMatchingFullPath whose Local and Vault checksum match will be
                // converted to FileIterationRef objects

                // get the Vault.xml file represented by the CO_IDB_VaultFileMaster
                VBXS.File vaultXmlFile = sourceFileDictionaryByFullPath[civfm.mFullVaultFilePath];

                // adjust the  VBXS.File based on the results in the CO_IDB_VaultFileMaster
                AdjustVBXSFile(civfm, vaultXmlFile);
            }


            // serialize the Vault.xml back out to the drive
            Logger.Debug("VBXS.VaultBCPSerialization.WriteVault: start");
            VBXS.VaultBCPSerialization.WriteVault(sourceVault, Path.GetDirectoryName(sVaultXMLFile) + "\\VaultUpdate.xml");
            Logger.Debug(string.Format("VBXS.VaultBCPSerialization.WriteVault: End {0}", swp.LapTimeString()));


            // log total time taken
            Logger.Debug(string.Format("ModifyVaultXmlFile(): End {0}", swp.ElapsedTimeString()));
        }

        // adjust the  VBXS.File based on the results in the CO_IDB_VaultFileMaster
        protected static void AdjustVBXSFile(CO_IDB_VaultFileMaster civfm, VBXS.File vaultXmlFile)
        {
            // we are either going to convert the FileIteration to a FileIterationRef
            // OR create the FileIterationRef and insert it before the FileIteration within the same FileRevision

            // get the last revision
            VBXS.FileRevision fileRevision = vaultXmlFile.Revision.Last();

            // get the lastest VBXS.FileRevisionIteration
            VBXS.FileRevisionIteration fileRevIter = (VBXS.FileRevisionIteration)fileRevision.Items[0];

            // create a new object list to work with
            List<object> objectList = new List<object>();

            // get the IDB record for the file
            MCDD.DbEntity.File idbFileRecord = civfm.mCO_FileMaster.LatestCO_FileIteration.mFileIterationRecord;

            // check to see if the checksums are the same
            if (civfm.mChecksumsMatch)
            {
                // when the checksums match, we are getting rid of the original VBXS.FileRevisionIteration
                // therefore, the Id MUST be he same as the VBXS.FileRevisionIteration so that parents
                // referencing this will get the correct ID
                VBXS.FileRevisionIterationRef fileRevIterRef = new VBXS.FileRevisionIterationRef();
                fileRevIterRef.Checksum = idbFileRecord.VaultFileCheckSum;
                fileRevIterRef.CreateDate = idbFileRecord.EntityCreateDate;
                fileRevIterRef.Id = fileRevIter.Id;

                // add the VBXS.FileRevisionIterationRef to the objectList
                objectList.Add(fileRevIterRef);
            }
            else
            {
                // need to keep the original FileRevisionIteration
                // but insert a new FileRevisionIterationRef before it

                // when the checksums don't match, we are keeping the original VBXS.FileRevisionIteration
                // therefore, the Id CANNOT be the same as VBXS.FileRevisionIteration because parents
                // still need to reference that VBXS.FileRevisionIteration from the Vault.xml file
                VBXS.FileRevisionIterationRef fileRevIterRef = new VBXS.FileRevisionIterationRef();
                fileRevIterRef.Checksum = idbFileRecord.VaultFileCheckSum;
                fileRevIterRef.CreateDate = idbFileRecord.EntityCreateDate;
                fileRevIterRef.Id = fileRevIter.Id + "Ref"; // add something that makes it different

                // add the VBXS.FileRevisionIterationRef to the objectList
                objectList.Add(fileRevIterRef);

                // add the original VBXS.FileRevisionIteration to the objectList
                objectList.Add(fileRevIter);
            }

            // add the new object list to the VBXS.FileRevision
            fileRevision.Items = objectList.ToArray();
        }


        #region Vault.xml methods to get objects from file


        // get the iterations
        protected static List<object> GetFileIterationsAsObjects(VBXS.File vbxsFile)
        {
            return (vbxsFile.Revision.SelectMany(rev => rev.Items).ToList());
        }

        // get the iterations as VBXS.FileRevisionIteration(s)
        protected static List<VBXS.FileRevisionIteration> GetFileIterations(VBXS.File vbxsFile)
        {
            return (vbxsFile.Revision.SelectMany(rev => rev.Items).Select(fri => (VBXS.FileRevisionIteration)fri).ToList());
        }


        // get last iteration as a VBXS.FileRevisionIterationRef
        protected static VBXS.FileRevisionIterationRef GetLastFileIterationRef(VBXS.File vbxsFile)
        {
            List<object> fileIterations = GetFileIterationsAsObjects(vbxsFile);
            return ((VBXS.FileRevisionIterationRef)fileIterations.Last());
        }

        // get first iteration as a VBXS.FileRevisionIteration
        protected static VBXS.FileRevisionIteration GetFirstFileIteration(VBXS.File vbxsFile)
        {
            List<object> fileIterations = GetFileIterationsAsObjects(vbxsFile);
            return ((VBXS.FileRevisionIteration)fileIterations.First());
        }


        // get last iteration as a VBXS.FileRevisionIteration
        protected static VBXS.FileRevisionIteration GetLastFileIteration(VBXS.File vbxsFile)
        {
            List<object> fileIterations = GetFileIterationsAsObjects(vbxsFile);
            return ((VBXS.FileRevisionIteration)fileIterations.Last());
        }

        // get last VBXS.FileRevisionIteration.Id or VBXS.FileRevisionIterationRef.Id
        protected static string GetLastFileIterationOrIterationRefId(VBXS.File vbxsFile)
        {
            List<object> fileIterations = GetFileIterationsAsObjects(vbxsFile);

            string id = "";
            object lastIteration = fileIterations.Last();
            if (lastIteration.GetType() == typeof(VBXS.FileRevisionIteration))
            {
                id = ((VBXS.FileRevisionIteration)lastIteration).Id;
            }
            else
            {
                id = ((VBXS.FileRevisionIterationRef)lastIteration).Id;
            }

            return (id);
        }



        // get specific iteration as a VBXS.FileRevisionIteration with Id
        protected static VBXS.FileRevisionIteration GetFileIterationWithId(VBXS.File vbxsFile, string sId)
        {
            List<object> fileIterations = GetFileIterationsAsObjects(vbxsFile);
            VBXS.FileRevisionIteration friWithId = fileIterations.Select(obj => (VBXS.FileRevisionIteration)obj).Where(fri => string.Compare(fri.Id, sId, true) == 0).First();
            return (friWithId);
        }



        // strip the iterations that did not ever load so that the file is left ONLY with references
        protected static void StripNonLoadedIterations(IEnumerable<VBXS.File> vbxsFiles)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // log total time taken
            Logger.Debug("StripNonLoadedIterations(): start");

            // visit each of the files and remove ANY non vaulted FileRevisionIterations
            int nFound = 0;
            foreach (VBXS.File vbxsFile in vbxsFiles)
            {
                if (vbxsFile.Name == "RFD101306.ipt")
                {
                }

                // iterate through and remove FileIterations and leave only FileIterationRefs
                if (StripNonLoadedIterations(vbxsFile))
                {
                    nFound++;
                }
            }

            // log total time taken
            Logger.Debug(string.Format("StripNonLoadedIterations(): End {0}", swp.ElapsedTimeString()));
        }

        // strip the iterations that did not ever load so that the file is left ONLY with references
        protected static bool StripNonLoadedIterations(VBXS.File vbxsFile)
        {
            // since the VBXS.File vbxsFile comes from a PSP DTU export, there may be more Revision nodes than there are revisions
            // this is because PSP DTU exports with a one to one revision and file vbxsIteration, rather than the norm
            // where there could be many file iterations to one revision in a normal "Vault.xml" Revision node
            // therefore, we are goint to nible through the iterations and match them up
            // since we want to keep the original SourceVault.Revision structure, we will iterate over those revisions
            // and keep a count of the iterations
            bool bOldAndNew = false;

            // get all of the FileRevisionIteration or FileRevisionIterationRef objects
            // identify if any are FileRevisionIteration
            List<VBXS.FileRevisionIteration> fileIterations = vbxsFile.Revision.SelectMany(rev => rev.Items)
                                                                        .Where(obj => obj.GetType() == typeof(VBXS.FileRevisionIteration))
                                                                        .Select(obj => (VBXS.FileRevisionIteration)obj).ToList();

            // if there are any, we know we need to remove them
            if (fileIterations.Any())
            {
                bOldAndNew = true;

                // create a new revision collection
                VBXS.FileRevisionCollection newFileRevisionCollection = new VBXS.FileRevisionCollection();

                foreach (VBXS.FileRevision fileRevision in vbxsFile.Revision)
                {
                    Debug.Assert(fileRevision.Items.Count() == 1);

                    // if it is an iteration ref, add it to the list
                    if (fileRevision.Items.First().GetType() == typeof(VBXS.FileRevisionIterationRef))
                    {
                        newFileRevisionCollection.Add(fileRevision);
                    }
                }

                // take on the newFileRevisionCollection
                vbxsFile.Revision = newFileRevisionCollection;
            }

            return (bOldAndNew);
        }




        // build dictionaries of VBXS.File by iteration id
        protected static Dictionary<string, VBXS.File> BuildVBXSFileDictionaryByIterationRefId(IEnumerable<VBXS.File> files)
        {
            Dictionary<string, VBXS.File> vbxsFileByFileRevIterId = new Dictionary<string, VBXS.File>(StringComparer.CurrentCultureIgnoreCase);

            // iterate over each Revision and file and get the VBXS.FileRevisionIteration.Id
            foreach (VBXS.File file in files)
            {
                // get all of the FileRevisionIteration
                List<object> fileIterations = GetFileIterationsAsObjects(file);

                // add them to the dictionary
                foreach (object fri in fileIterations)
                {
                    string id = "";
                    if (fri.GetType() == typeof(VBXS.FileRevisionIteration))
                    {
                        id = ((VBXS.FileRevisionIteration)fri).Id;
                    }
                    else
                    {
                        id = ((VBXS.FileRevisionIterationRef)fri).Id;
                    }

                    vbxsFileByFileRevIterId.Add(id, file);
                }
            }

            return (vbxsFileByFileRevIterId);
        }

        // remove files from the SourceVault that where not loaded
        protected static void RemoveFilesFromSourceVaultNotInVault(VBXS.Vault sourceVault,
                                                                    List<string> filePathsNotVaulted,
                                                                    Dictionary<string, VBXS.File> vbxsFileDictionaryByFullVaultPath,
                                                                    out List<VBXS.File> vbxsFilesNotVaulted)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // log total time taken
            Logger.Debug("RemoveFilesFromSourceVaultNotInVault(): start");

            // get the unique vault paths of all the files not Vaulted
            List<string> vaultFoldersWithFilesToRemove = filePathsNotVaulted.Select(st => Path.GetDirectoryName(st).Replace("\\", "/")).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();

            // build a dictionary of the Vault folders so we can visit them very fast
            Dictionary<string, VBXS.Folder> vaultFolderDictionary = BuildVaultFolderDictionary(sourceVault);

            // create a list to hold the files we have removed
            vbxsFilesNotVaulted = filePathsNotVaulted.Select(vfp => vbxsFileDictionaryByFullVaultPath[vfp]).ToList();

            // go through each of the folders and remove the files that
            // were not vaulted in the Full load, delta load, or the load missing files runs
            int nRemoved = 0;
            foreach (string sVaultFolderPath in vaultFoldersWithFilesToRemove)
            {
                VBXS.Folder folder = vaultFolderDictionary[sVaultFolderPath];

                // create a collection of files
                VBXS.FileCollection newFileCollection = new VBXS.FileCollection();
                foreach (VBXS.File file in folder.File)
                {
                    // add the file to the new container
                    // so we retain those files that do not need to be removed
                    if (!vbxsFilesNotVaulted.Contains(file))
                    {
                        newFileCollection.Add(file);
                    }
                    else
                    {
                        nRemoved++;
                    }
                }

                // assign the folder the new file collection
                folder.File = newFileCollection;
            }

            // verify we removed the right number of files
            Debug.Assert(nRemoved == vbxsFilesNotVaulted.Count() && vbxsFilesNotVaulted.Count() == filePathsNotVaulted.Count());

            // log total time taken
            Logger.Debug(string.Format("RemoveFilesFromSourceVaultNotInVault(): End {0}", swp.ElapsedTimeString()));
        }


        // visit the folders and build a dictionary for quick access
        private static Dictionary<string, VBXS.Folder> BuildVaultFolderDictionary(VBXS.Vault sourceVault)
        {
            Dictionary<string, VBXS.Folder> vaultFolderDictionary = new Dictionary<string, VBXS.Folder>(StringComparer.CurrentCultureIgnoreCase);

            // iterate over the folders and capture a reference to the folders in question
            // we are assuming that sourceVault.Root.File list is empty
            string sCurrentVaultPath = "$";
            foreach (VBXS.Folder childFolder in sourceVault.Root.Folder)
            {
                BuildVaultFolderDictionary(childFolder, sCurrentVaultPath, ref vaultFolderDictionary);
            }

            return (vaultFolderDictionary);
        }

        // visit the folders and build a dictionary for quick access
        private static void BuildVaultFolderDictionary(VBXS.Folder currentFolder, string sParentVaultPath, ref Dictionary<string, VBXS.Folder> vaultFolderDictionary)
        {
            // build the vault path
            string sCurrentVaultPath = sParentVaultPath + "/" + currentFolder.Name;

            // add the folder to the dictionary
            vaultFolderDictionary.Add(sCurrentVaultPath, currentFolder);

            // iterate over the folders and capture a reference to the folders in question
            foreach (VBXS.Folder childFolder in currentFolder.Folder1)
            {
                BuildVaultFolderDictionary(childFolder, sCurrentVaultPath, ref vaultFolderDictionary);
            }
        }


        // get a dictionary of the full vault path and pointer to the VBXS.File from the VBXS.Vault sourceVault
        protected static Dictionary<string, VBXS.File> GetSourceVaultXmlFileDictionary(VBXS.Vault sourceVault)
        {
            Dictionary<string, VBXS.File> vbxsFileDictionaryByFullVaultPath = new Dictionary<string, VBXS.File>(StringComparer.CurrentCultureIgnoreCase);

            // add the root folders files
            PopulateSourceVaultXmlFileDictionary(sourceVault.Root.File, "$", ref vbxsFileDictionaryByFullVaultPath);

            // iterate through all of the folders, and add to the dictionary
            PopulateSourceVaultXmlFileDictionary(sourceVault.Root.Folder, "$", ref vbxsFileDictionaryByFullVaultPath);

            // return the built dictionary
            return (vbxsFileDictionaryByFullVaultPath);
        }

        // method to add the files from a Vault.Folder.FileCollection to the dictionary
        protected static void PopulateSourceVaultXmlFileDictionary(VBXS.FileCollection fileCollection, string sParentPath, ref Dictionary<string, VBXS.File> vbxsFileDictionaryByFullVaultPath)
        {
            // add to the dictionary
            foreach (VBXS.File file in fileCollection)
            {
                vbxsFileDictionaryByFullVaultPath.Add(sParentPath + "/" + file.Name, file);
            }
        }

        // method to iterate all of the folders and add the files from a Vault.Folder.FileCollection to the dictionary
        protected static void PopulateSourceVaultXmlFileDictionary(VBXS.FolderCollection folderCollection, string sParentPath, ref Dictionary<string, VBXS.File> vbxsFileDictionaryByFullVaultPath)
        {
            // add to the dictionary
            foreach (VBXS.Folder folder in folderCollection)
            {
                // add this folders children
                string sChildFolderPath = sParentPath + "/" + folder.Name;
                PopulateSourceVaultXmlFileDictionary(folder.File, sChildFolderPath, ref vbxsFileDictionaryByFullVaultPath);

                // iterate the child folders
                PopulateSourceVaultXmlFileDictionary(folder.Folder1, sChildFolderPath, ref vbxsFileDictionaryByFullVaultPath);
            }
        }

        #endregion Vault.xml methods to get objects from file

    }
}
