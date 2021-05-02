using System;
using System.Collections.Generic;

namespace IDB.Load.BCP
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
            return y != null && x != null && x.FileName == y.FileName && x.FolderId == y.FolderId && x.CreateDate.Ticks == y.CreateDate.Ticks;
        }

        public int GetHashCode(UniqueFile obj)
        {
            return $"{obj.FileName}{obj.FolderId}{obj.CreateDate.Ticks}".GetHashCode();
        }
    }
}
