namespace IDB.Core.DapperHelper
{
    public class ColumnAndParam
    {
        public string ColumnName { get; set; }
        public string ParamName { get; set; }

        public ColumnAndParam(string columnName, string paramName)
        {
            ColumnName = columnName;
            ParamName = paramName;
        }
    }
}
