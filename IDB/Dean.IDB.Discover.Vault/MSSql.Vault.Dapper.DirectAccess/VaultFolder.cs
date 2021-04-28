using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;

using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;

using log4net;
using BCPBuilderConfig;


namespace MSSql.Vault.Dapper.DirectAccess
{
    public class VaultFolder
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultFolder));

        // manage the Folder table record for this folder
        public Folder m_FolderRecord { get; private set; }
        public CategoryDef m_CategoryDef { get; private set; }

        // manage the folder relationships
        // manage the parents and children
        public VaultFolder m_ParentVaultFolder { get; private set; }
        public List<VaultFolder> m_ChildFolders { get; private set; }
        public List<VaultFolder> m_RolledUpFolders { get; private set; }

        public int m_FolderDepth { get; private set; }
        public int m_MaxFolderChildDepth { get; private set; }

        // constructor
        public VaultFolder(Folder folderRecord, CategoryDef categoryDef)
        {
            // assign the properties
            m_FolderRecord = folderRecord;
            m_CategoryDef = categoryDef;

            m_FolderDepth = 0;
            m_MaxFolderChildDepth = 0;

            m_ChildFolders = new List<VaultFolder>();

            m_RolledUpFolders = new List<VaultFolder>();
        }

        // method to build the hierarchy of ProjectWiseFolder objects
        public static bool SetupFolderHierarchies(  Dictionary<long, Folder> _folder_Dict,
                                                    Dictionary<long, CategoryDef> categoryDefs_Dict,
                                                    Dictionary<long, CategoryOnEntity> categoriesOnEntitiesForFolders_Dict,
                                                    ref VaultFolder rootDocumentsFolder,
                                                    ref Dictionary<long, VaultFolder> vaultFolderDictByFolderID,
                                                    ref Dictionary<string, VaultFolder> vaultFolderDictByVaultPath)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("SetupFolderHierarchies: Start");

            bool bSuccess = false;

            // create dictionaries for the folders based on their FolderID and VaultPath
            vaultFolderDictByFolderID = new Dictionary<long, VaultFolder>();
            vaultFolderDictByVaultPath = new Dictionary<string, VaultFolder>(StringComparer.CurrentCultureIgnoreCase);

            // iterate through the given projects list and build the folders
            foreach (var kvp in _folder_Dict)
            {
                // get the CategoryDef for the folder
                // may not be defined for the Folder
                CategoryDef categoryDef = null;
                if (categoriesOnEntitiesForFolders_Dict.ContainsKey(kvp.Value.FolderID))
                {
                    categoryDef = categoryDefs_Dict[categoriesOnEntitiesForFolders_Dict[kvp.Value.FolderID].CategoryDefId];
                }

                VaultFolder vf = new VaultFolder(kvp.Value, categoryDef);

                // add it to the dictionaries
                vaultFolderDictByFolderID.Add(kvp.Key, vf);
                vaultFolderDictByVaultPath.Add(vf.m_FolderRecord.VaultPath, vf);

                // check to see if it is the root documents folder
                if (kvp.Value.FolderID == 1)
                {
                    rootDocumentsFolder = vf;
                }

            }

            // the [Folder] table points the child back to the parent with the [ParentFolderID] element
            // use this to setup the hierarchies
            foreach (var kvp in vaultFolderDictByFolderID)
            {
                // if the folder has a parent, set the parent, and add it as a child
                if (kvp.Value.m_FolderRecord.ParentFolderId.HasValue)
                {
                    VaultFolder childVF = kvp.Value;
                    VaultFolder parentVF = vaultFolderDictByFolderID[kvp.Value.m_FolderRecord.ParentFolderId.Value];

                    // set the parent
                    childVF.m_ParentVaultFolder = parentVF;

                    // set the child
                    parentVF.m_ChildFolders.Add(childVF);
                }
            }

            // iterate through the root Documents folder and set all childrens mFullVaultFolderPath
            rootDocumentsFolder.SetupChildren();

            // get the max folder depth
            Logger.Debug(string.Format("Number of VaultFolders: {0:n0}", vaultFolderDictByFolderID.Count()));
            Logger.Debug(string.Format("m_MaxFolderChildDepth: {0:n0}", rootDocumentsFolder.m_MaxFolderChildDepth));


            // log total time taken
            Logger.Debug(string.Format("SetupFolderHierarchies(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to iterate children and setup max depth and rolled up children
        protected void SetupChildren()
        {
            // add the children to the rolled up children
            this.m_RolledUpFolders.AddRange(this.m_ChildFolders);

            foreach (VaultFolder childVF in this.m_ChildFolders)
            {
                // set the child mFullVaultFolderPath
                childVF.m_FolderDepth = this.m_FolderDepth + 1;

                // iterate through the children of this child
                childVF.SetupChildren();

                // set the max depth
                this.m_MaxFolderChildDepth = Math.Max(this.m_MaxFolderChildDepth, childVF.m_MaxFolderChildDepth + 1);

                // set the rolled up children
                this.m_RolledUpFolders.AddRange(childVF.m_RolledUpFolders);
            }
        }

    }
}
