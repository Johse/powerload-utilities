using System;
using System.Collections.Generic;
using Dapper;

namespace IDB.Core.DapperHelper
{
    public class DapperEntity
    {
        public readonly string TableName;
        public readonly string IdColumn;
        private readonly List<Tuple<string, string, object>> _tuples;

        public DapperEntity(string tableName, string idColumn)
        {
            TableName = tableName;
            IdColumn = idColumn;
            _tuples = new List<Tuple<string, string, object>>();
        }

        public void Add(string columnName, object value)
        {
            _tuples.Add(new Tuple<string, string, object>(columnName, columnName.Replace(" ", "_"), value));
        }

        public DynamicParameters DynamicParameters
        {
            get
            {
                _tuples.Sort((x, y) => string.Compare(y.Item1, x.Item1, StringComparison.Ordinal));
                var dynamicParameters = new DynamicParameters();
                for (int i = 0; i < _tuples.Count; i++)
                    dynamicParameters.Add(_tuples[i].Item2, _tuples[i].Item3);

                return dynamicParameters;
            }
        }

        public IEnumerable<ColumnAndParam> ColumnsAndParams
        {
            get
            {
                _tuples.Sort((x, y) => string.Compare(y.Item1, x.Item1, StringComparison.Ordinal));
                var mapping = new List<ColumnAndParam>();
                for (int i = 0; i < _tuples.Count; i++)
                    mapping.Add(new ColumnAndParam(_tuples[i].Item1, _tuples[i].Item2));

                return mapping;
            }
        }
    }
}
