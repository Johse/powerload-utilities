using System.Collections.Generic;
using System.Linq;
using MSSql.DAO.VaultDirect.VaultDbEntities;

namespace MSSql.DAO.VaultDirect
{
    internal class ChildFinder
    {
        private readonly List<FileAssociationExtended> _associationsExtended;
        private readonly Dictionary<long, FileIterationExtended> _fileIterExtended;
        ILookup<long, FileAssociationExtended> _associationsByFromId;
        ILookup<long, FileAssociationExtended> _associationsByToId;
        ILookup<long, FileAssociationExtended> _associationsByFromMasterId;
        ILookup<long, FileAssociationExtended> _associationsByToMasterId;



        public ChildFinder(List<FileAssociationExtended> associationsExtended, Dictionary<long, FileIterationExtended> fileIterExtended)
        {
            _associationsExtended = associationsExtended;
            _fileIterExtended = fileIterExtended;

            _associationsByFromId = associationsExtended.ToLookup(a => a.FromId, a => a);
            _associationsByToId = associationsExtended.ToLookup(a => a.ToId, a => a);
            _associationsByFromMasterId = associationsExtended.ToLookup(a => a.FromMasterID, a => a);
            _associationsByToMasterId = associationsExtended.ToLookup(a => a.ToMasterID, a => a);
        }

        public IEnumerable<FileIterationExtended> FindChildren(FileIterationExtended fileIterExtended)
        {
            return FindChildren(fileIterExtended.FileIterationId);
        }

        public IEnumerable<FileIterationExtended> FindChildren(long fileIterationId)
        {
            var childIterationIds = _associationsByFromId[fileIterationId].Select(assoc => assoc.ToId);

            return childIterationIds.Select(childIterationId => _fileIterExtended[childIterationId]);
        }

        public IEnumerable<FileIterationExtended> FindParents(FileIterationExtended fileIterExtended)
        {
            return FindParents(fileIterExtended.FileIterationId);
        }

        public IEnumerable<FileIterationExtended> FindParents(long fileIterationId)
        {
            var parentIterationIds = _associationsByToId[fileIterationId].Select(assoc => assoc.FromId);

            // because we did not crawl back up the tree to get ALL parents of files, we may run into a parent
            // that does not exist in the set of files we are looking at
            return parentIterationIds.Where(parentIterationId => _fileIterExtended.ContainsKey(parentIterationId)).Select(parentIterationId => _fileIterExtended[parentIterationId]);
        }
    }
}