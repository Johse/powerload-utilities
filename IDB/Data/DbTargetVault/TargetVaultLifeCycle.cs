namespace IDB.Data.DbTargetVault
{
    public class TargetVaultLifeCycle
    {
        public string EntityClassId { get; set; }
        public string LifeCycleDefinition { get; set; }
        public string LifeCycleState { get; set; }
        public bool IsObsoleteState { get; set; }
        public bool IsReleasedState { get; set; }
    }
}