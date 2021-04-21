namespace IDB.Data.DbTargetVault
{
    public class TargetVaultProperty
    {
        public string EntityClassId { get; set; }
        public string PropertyName { get; set; }
        public string DataType { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
    }
}