using System;
using System.Collections.Generic;

namespace IDB.Core.Data.Unique
{
    public class UniqueFile
    {
        public string FileName { get; }
        public long FolderId { get; }
        public DateTime CreateDate { get; }

        public UniqueFile(dynamic file)
        {
            FileName = file.FileName.ToUpper();
            FolderId = file.FolderID;
            CreateDate = file.CreateDate;
        }

        public UniqueFile(string fileName, long folderId, DateTime createDate)
        {
            FileName = fileName.ToUpper();
            FolderId = folderId;
            CreateDate = createDate;
        }
    }

    public class UniqueFileComparer : IEqualityComparer<UniqueFile>
    {
        public bool Equals(UniqueFile x, UniqueFile y)
        {
            if (x == null || y == null) return false;
            if (object.ReferenceEquals(x, y)) return true;
            return x.FileName == y.FileName && x.FolderId == y.FolderId && x.CreateDate.ToString("G") == y.CreateDate.ToString("G");
        }

        public int GetHashCode(UniqueFile obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.FileName.GetHashCode();
                hash = hash * 23 + obj.FolderId.GetHashCode();
                hash = hash * 23 + obj.CreateDate.ToString("G").GetHashCode();
                return hash;
            }
        }
    }
}
