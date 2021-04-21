using System;

namespace IDB.DbTargetVault
{
    public class TargetVaultFile
    {
        public string Folder { get; set; }
        public string FileName { get; set; }
        public long FileIterationId { get; set; }
        public long FileMasterId { get; set; }
        public DateTime CreateDate { get; set; }
        public int Checksum { get; set; }
    }
}
