using System.Collections.Generic;
using System.Linq;
using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;

namespace MSSql.Vault.Dapper.DirectAccess
{
    internal class VaultFileFinderOld
    {
        private readonly Dictionary<long, FileIteration> _fileIterations;
        private readonly Dictionary<long, FileMaster> _fileMasters;
        private readonly Dictionary<long, FileResource> _fileResources;
        private readonly Dictionary<long, Folder> _folders;

        ILookup<long, FileResource> fileResourcesByMasterId;
        ILookup<long, FileIteration> fileIterationsByResourceId;
        Dictionary<string, Folder> foldersByPath;
        ILookup<long, FileMaster> fileMastersByFolder;


        public VaultFileFinderOld(Dictionary<long, FileIteration> fileIterations,
                               Dictionary<long, FileMaster> fileMasters,
                               Dictionary<long, FileResource> fileResources,
                               Dictionary<long, Folder> folders)
        {
            _fileIterations = fileIterations;
            _fileMasters = fileMasters;
            _fileResources = fileResources;
            _folders = folders;

            fileMastersByFolder = _fileMasters.Values.ToLookup(fm => fm.FolderId, fm => fm);

            // make sure to use a case insensitive dictionary
            foldersByPath = _folders.Values.ToDictionary(f => f.VaultPath, f => f);

            fileResourcesByMasterId = _fileResources.Values.ToLookup(fr => fr.FileMasterId, fr => fr);
            fileIterationsByResourceId = _fileIterations.Values.ToLookup(fi => fi.ResourceId, fi => fi);
        }

        public IEnumerable<FileIteration> GetIterationsByPath(string path)
        {


            var vaultFolderPath = GetFolderPath(path);
            var fileName = GetFileName(path);

            var folder = foldersByPath[vaultFolderPath];
            var fileMasters = fileMastersByFolder[folder.FolderID];

            var fileResources = new List<FileResource>();

            foreach (var fileMaster in fileMasters)
            {
                var frs = fileResourcesByMasterId[fileMaster.FileMasterID];
                fileResources.AddRange(frs);
            }

            var fileIterationsInFolder = new List<FileIteration>();

            foreach (var fileResource in fileResources)
            {
                var fis = fileIterationsByResourceId[fileResource.ResourceId];
                fileIterationsInFolder.AddRange(fis);
            }


            var fileIterations = fileIterationsInFolder.Where(fi => fi.FileName == fileName);


            return fileIterations;
        }

        private string GetFolderPath(string path)
        {
            var lastIndexOfForwardSlash = path.LastIndexOf('/');
            return path.Substring(0,  lastIndexOfForwardSlash );
        }

        private string GetFileName(string path)
        {
            var lastIndexOfForwardSlash = path.LastIndexOf('/');
            return path.Substring(lastIndexOfForwardSlash + 1);
        }
    }
}