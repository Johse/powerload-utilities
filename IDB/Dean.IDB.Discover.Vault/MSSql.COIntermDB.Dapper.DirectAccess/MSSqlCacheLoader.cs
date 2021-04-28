using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;
using Dapper;

namespace MSSql.COIntermDB.Dapper.DirectAccess
{
    public class MSSqlCacheLoader
    {
        private string _connectionString;

        public SqlConnectionStringBuilder SqlConnStrBuilder;

        public MSSqlCacheLoader(string connectionString)
        {
            _connectionString = connectionString;
            SqlConnStrBuilder = new SqlConnectionStringBuilder(_connectionString);
        }


        // TODO: optimize throughput by identifying the Enumerable parameters and putting them into a temporary table
        // sql bulk copy them to the table, and allow multiple of these types
        // make sure the table goes away after use
        // in this manner can then do SQL analysis on LARGE parameterized sets, and on mulitple parameters
        public List<T> LoadEntities<T>(string selectQuery = null, object param = null) where T : IDbEntity, new()
        {
            var tempT = new T();
            selectQuery = selectQuery ?? tempT.GetSelectString();

            List<T> vaultEntityList = null;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                // check to see if param is a single parameter
                // and that parameter is IEnumerable
                // if so, we need to not call with more than 2000 elements, more than 2000 will cause a SQL exception
                // don't know what to do if multiple parameters are passed
                // Such as with Param = "" OR Param = ""
                // use reflection to identify it if is an anonymous property/value parameters list
                IEnumerable<T> entities = null;
                PropertyInfo propertyInfo;
                if ((param == null) ||
                        !IsAnonymousSinglePropertyValueOfIEnumerable(param, "TempIDTable", conn, out propertyInfo))
                {
                    // run the query without parameters, or with multiple parameters (hoping that they don't exceed 2000)
                    entities = param == null ? conn.Query<T>(selectQuery, commandTimeout: 600) : conn.Query<T>(selectQuery, param, commandTimeout: 300);
                }
                else
                {
                    // get this set of results
                    // the query MUST use @parm as the parameter string in the query
                    // therefore, replace @parm with "(SELECT ID FROM ##TempIDTable)"
                    string sNewQuery = selectQuery.Replace("@parm", "(SELECT ID FROM ##TempIDTable)");
                    entities = conn.Query<T>(sNewQuery, commandTimeout: 600);
                }

                // create the dictionary
                if (entities != null)
                {
                    vaultEntityList = entities.ToList();
                }
            }

            return vaultEntityList;
        }

        // TODO: optimize throughput by identifying the Enumerable parameters and putting them into a temporary table
        // sql bulk copy them to the table, and allow multiple of these types
        // make sure the table goes away after use
        // in this manner can then do SQL analysis on LARGE parameterized sets, and on mulitple parameters
        public Dictionary<int, T> LoadEntityDictionary<T>(string selectQuery = null, object param = null) where T : IDbEntityWithID, new()
        {
            Dictionary<int, T> vaultEntityDictionary = null;

            List<T> vaultEntityList = LoadEntities<T>(selectQuery, param);

            // create the dictionary
            if (vaultEntityList != null)
            {
                vaultEntityDictionary = vaultEntityList.ToDictionary(en => en.GetId(), en => en);

                // add the zero entity if GetNullEntity returns a value
                var tempT = new T();
                var defaultEntity = tempT.GetNullEntity();
                if (defaultEntity != null)
                {
                    vaultEntityDictionary.Add(0, (T)defaultEntity);
                }
            }

            return vaultEntityDictionary;
        }

        // TODO: optimize throughput by identifying the Enumerable parameters and putting them into a temporary table
        // sql bulk copy them to the table, and allow multiple of these types
        // make sure the table goes away after use
        // in this manner can then do SQL analysis on LARGE parameterized sets, and on mulitple parameters
        public List<T> LoadEntitiesWithUDP<T>(string selectQuery = null, object param = null) where T : IDbEntityWithIDAndUDPs, new()
        {
            var tempT = new T();
            selectQuery = selectQuery ?? tempT.GetSelectString();

            List<T> vaultEntityList = null;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                // check to see if param is a single parameter
                // and that parameter is IEnumerable
                // if so, we need to not call with more than 2000 elements, more than 2000 will cause a SQL exception
                // don't know what to do if multiple parameters are passed
                // Such as with Param = "" OR Param = ""
                // use reflection to identify it if is an anonymous property/value parameters list
                IEnumerable<T> entities = null;
                PropertyInfo propertyInfo;
                if ((param == null) ||
                        !IsAnonymousSinglePropertyValueOfIEnumerable(param, "TempIDTable", conn, out propertyInfo))
                {
                    // run the query without parameters, or with multiple parameters (hoping that they don't exceed 2000)
                    // entities = param == null ? conn.Query<T>(selectQuery, commandTimeout: 600) : conn.Query<T>(selectQuery, param, commandTimeout: 300);

                    entities = param == null ? conn.Query(selectQuery, commandTimeout: 600).Select(x => (T)MSSqlCacheLoader.ConvertToIDbEntityWithIDAndUDPs<T>(x)) :
                                                    conn.Query<T>(selectQuery, param, commandTimeout: 300).Select(x => (T)MSSqlCacheLoader.ConvertToIDbEntityWithIDAndUDPs<T>(x));

                }
                else
                {
                    // get this set of results
                    // the query MUST use @parm as the parameter string in the query
                    // therefore, replace @parm with "(SELECT ID FROM ##TempIDTable)"
                    string sNewQuery = selectQuery.Replace("@parm", "(SELECT ID FROM ##TempIDTable)");
                    entities = conn.Query<T>(sNewQuery, commandTimeout: 600).Select(x => (T)MSSqlCacheLoader.ConvertToIDbEntityWithIDAndUDPs<T>(x));
                }

                // create the dictionary
                if (entities != null)
                {
                    vaultEntityList = entities.ToList();
                }
            }

            return vaultEntityList;
        }

        // method that takes a table input and allows for n-number of UDP_ properties as individual columns
        public static T ConvertToIDbEntityWithIDAndUDPs<T>(dynamic p) where T : IDbEntityWithIDAndUDPs, new()
        {
            var entity = new T { UserDefinedProperties = new Dictionary<string, object>() };

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var element in p)
            {
                var property = properties.SingleOrDefault(x => x.Name == element.Key);
                if (property != default(PropertyInfo))
                    property.SetValue(entity, element.Value);

                if (element.Key.StartsWith("UDP_"))
                    entity.UserDefinedProperties.Add(element.Key.Replace("UDP_", "").ToString(), element.Value);
            }

            return entity;
        }




        // TODO: optimize throughput by identifying the Enumerable parameters and putting them into a temporary table
        // sql bulk copy them to the table, and allow multiple of these types
        // make sure the table goes away after use
        // in this manner can then do SQL analysis on LARGE parameterized sets, and on mulitple parameters
        public Dictionary<int, T> LoadEntitiesWithUDPAsDictionary<T>(string selectQuery = null, object param = null) where T : IDbEntityWithIDAndUDPs, new()
        {
            Dictionary<int, T> vaultEntityDictionary = null;

            List<T> vaultEntityList = LoadEntitiesWithUDP<T>(selectQuery, param);

            // create the dictionary
            if (vaultEntityList != null)
            {
                vaultEntityDictionary = vaultEntityList.ToDictionary(en => en.GetId(), en => en);

                // add the zero entity if GetNullEntity returns a value
                var tempT = new T();
                var defaultEntity = tempT.GetNullEntity();
                if (defaultEntity != null)
                {
                    vaultEntityDictionary.Add(0, (T)defaultEntity);
                }
            }

            return vaultEntityDictionary;
        }


        // method to check if it IsAnonymousType 
        protected static bool IsAnonymousType(Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        // method to check for IsAnonymousType, has a named property, IEnumerable value
        // return false if not
        // return true and populate temporary table with content
        // TODO: don't create the temporary table if the number of elements is reasonably small
        protected static bool IsAnonymousSinglePropertyValueOfIEnumerable(object param, string sTempTableName, SqlConnection sqlConnection, out PropertyInfo propertyInfo)
        {
            bool IsAnonymousEtc = false;

            // set the output values
            propertyInfo = null;
            if ((param != null) && IsAnonymousType(param.GetType()))
            {
                // get the property info
                PropertyInfo[] pi = param.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                // check for 1 property/value pair, name has content, and value is an IEnumerable
                if ((pi.Count() == 1) && (pi[0].Name != null) && (pi[0].Name.Length > 0))
                {
                    // set the property info
                    propertyInfo = pi[0];

                    // check to see if the parameter value is IEnumerable<object>
                    object propValue = propertyInfo.GetValue(param, null);
                    Type type = propValue.GetType();
                    if (type.HasElementType)
                    {
                        Type baseType = type.GetElementType();

                        // only doing our long arrays for now
                        if (baseType == typeof(long))
                        {
                            // Create a datatable with one column, long/bigint ID
                            DataTable tempDataTable = new DataTable(sTempTableName);

                            // Create Column 1: ID
                            DataColumn idColumn = new DataColumn();
                            idColumn.DataType = Type.GetType("System.Int64");
                            idColumn.ColumnName = "ID";
                            tempDataTable.Columns.Add(idColumn);

                            // populate the data table
                            foreach (long nVal in ((long[])propValue))
                            {
                                DataRow row = tempDataTable.NewRow();
                                row[0] = nVal;
                                tempDataTable.Rows.Add(row);
                            }

                            // for throughput, set the AcceptChanges
                            tempDataTable.AcceptChanges();

                            // create the temporary table
                            SqlCommand cmd = new SqlCommand("create table ##" + sTempTableName + "(ID bigint)", sqlConnection);
                            cmd.ExecuteNonQuery();

                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection))
                            {
                                bulkCopy.DestinationTableName = "##" + sTempTableName;
                                bulkCopy.WriteToServer(tempDataTable);
                            }

                            IsAnonymousEtc = true;
                        }

                    }
                }
            }

            return (IsAnonymousEtc);
        }



        // method to check for IsAnonymousType, has a named property, IEnumerable value
        // return false if not
        // return true and populate property name, and List<List<object>> of properties
        // so that they don't exceed 2000 elements
        protected static bool IsAnonymousSinglePropertyValueOfIEnumerable(object param, out PropertyInfo propertyInfo, out List<List<long>> valueLists)
        {
            bool IsAnonymousEtc = false;

            // set the output values
            propertyInfo = null;
            valueLists = null;
            if ((param != null) && IsAnonymousType(param.GetType()))
            {
                // get the property info
                PropertyInfo[] pi = param.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                // check for 1 property/value pair, name has content, and value is an IEnumerable
                if ((pi.Count() == 1) && (pi[0].Name != null) && (pi[0].Name.Length > 0))
                {
                    // set the property info
                    propertyInfo = pi[0];

                    // check to see if the parameter value is IEnumerable<object>
                    object propValue = propertyInfo.GetValue(param, null);
                    Type type = propValue.GetType();
                    if (type.HasElementType)
                    {
                        Type baseType = type.GetElementType();

                        // only doing our long arrays for now
                        if (baseType == typeof(long))
                        {
                            // iterate through and create separate holders
                            List<long> longList = new List<long>();
                            valueLists = new List<List<long>>();
                            foreach (long nVal in ((long[])propValue))
                            {
                                longList.Add(nVal);

                                // create a new list after 2000
                                if (longList.Count() >= 2000)
                                {
                                    valueLists.Add(longList);
                                    longList = new List<long>();
                                }
                            }

                            // add it to the list
                            if (longList.Count > 0)
                            {
                                valueLists.Add(longList);
                            }

                            IsAnonymousEtc = true;
                        }

                    }
                }
            }

            return (IsAnonymousEtc);
        }


        public T LoadSingleVaultEntity<T>(string selectQuery, object paramaters) where T : IDbEntity, new()
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var entities = conn.Query<T>(selectQuery, paramaters, commandTimeout: 600);

                var entity = entities.Single();
                return entity;
            }
        }

        #region SQL verify columns, add columns, etc

        // check to see if a column in the table exists
        public bool TableColumnExists<T>(string sColumnName) where T : IDbEntity, new()
        {
            bool columnExists = false;

            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var tempT = new T();
                string selectQuery = string.Format("SELECT COUNT(*) from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", tempT.GetTableName(), sColumnName);
                int count = (int)conn.ExecuteScalar(selectQuery, commandTimeout: 600);

                if (count > 0)
                {
                    columnExists = true;
                }
            }

            return (columnExists);
        }

        // check to see if a column in the table exists
        public void AddTableColumn<T>(string sColumnName, string dbType = "NVARCHAR(MAX)") where T : IDbEntity, new()
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var tempT = new T();
                string alterQuery = string.Format("ALTER TABLE {0} ADD [{1}] {2}", tempT.GetTableName(), sColumnName, dbType);
                conn.ExecuteScalar(alterQuery, commandTimeout: 600);
            }
        }


        #endregion SQL verify columns, add columns, etc

        #region SQL insert and update single entities

        // method to insert entity into a table
        // Dapper.Contrip project has IDbConnection extension methods, but we are not going to use it for
        // inserting IEnumerable<T> list of objects
        public int InsertDbEntity<T>(string insertQuery, T ent) where T : IDbEntity
        {
            int nInserted = 0;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                nInserted = conn.Execute(insertQuery, ent);
            }

            return (nInserted);
        }

        // method to insert entity into a table
        // Dapper.Contrip project has IDbConnection extension methods, but we are not going to use it for
        // inserting IEnumerable<T> list of objects
        // we'll build the query and return the Identity built
        public int InsertDbEntityReturnIdentiyOLD<T>(T ent) where T : IDbEntity, new()
        {
            int nIdentity = 0;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var tempT = new T();
                string insertQuery = string.Format("INSERT INTO [{0}] ([Stuff]) VALUES (@Stuff); SELECT CAST(SCOPE_IDENTITY() as int)", tempT.GetTableName());

                nIdentity = conn.Query<int>(insertQuery, ent).Single();
            }

            return (nIdentity);
        }

        // method to insert entity into a table
        // Dapper.Contrip project has IDbConnection extension methods, but we are not going to use it for
        // inserting IEnumerable<T> list of objects
        // we'll build the query and return the Identity built
        public int InsertDbEntityReturnIdentiy<T>(string insertQuery, T ent) where T : IDbEntity
        {
            int nIdentity = 0;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                insertQuery += "; SELECT CAST(SCOPE_IDENTITY() as int)";

                nIdentity = conn.Query<int>(insertQuery, ent).Single();
            }

            return (nIdentity);
        }



        // method to update entity already in a table
        // Dapper.Contrip project has IDbConnection extension methods, but we are not going to use it for
        // inserting IEnumerable<T> list of objects
        public int UpdateDbEntity<T>(string updateQuery, T ent) where T : IDbEntity
        {
            int nUpdated = 0;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                nUpdated = conn.Execute(updateQuery, ent);
            }

            return (nUpdated);
        }

        // method to update record in a table given the query and parameters
        public int UpdateDbRecord(string updateQuery, DynamicParameters param) 
        {
            int nUpdated = 0;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                nUpdated = conn.Execute(updateQuery, param);
            }

            return (nUpdated);
        }


        #endregion SQL insert and update single entities


        #region SQL database statistics

        // no longer assumes that the name of the class is the same as the SQL table name
        public int GetNumberOfRows<T>() where T : IDbEntity, new()
        {
            var tempT = new T();

            return (GetNumberOfRows(tempT.GetTableName()));
        }

        // get number of rows for the table
        public int GetNumberOfRows(string sTableName)
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                string selectQuery = string.Format("SELECT COUNT(*) FROM [dbo].[{0}]", sTableName);
                int count = (int)conn.ExecuteScalar(selectQuery, commandTimeout: 600);

                return (count);
            }
        }

        // no longer assumes that the name of the class is the same as the SQL table name
        public int GetNumberOfColumns<T>() where T : IDbEntity, new()
        {
            var tempT = new T();

            return (GetNumberOfColumns(tempT.GetTableName()));
        }


        // get number of columns for the table
        public int GetNumberOfColumns(string sTableName)
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                string selectQuery = string.Format("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", sTableName);
                int count = (int)conn.ExecuteScalar(selectQuery, commandTimeout: 600);

                return (count);
            }
        }



        // call "sp_spaceused" stored procedure to get size of table
        public SpaceUsedTableInfo GetTableSizeInfo(string sTableName)
        {
            // returns the following
            // name             rows    reserved    data        index_size  unused
            // FileIteration    1549448 705736 KB   480328 KB   224960 KB   448 KB

            SpaceUsedTableInfo suti = null;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var param = new DynamicParameters();
                param.Add("@oneresultset", dbType: DbType.Int32, value: 1, direction: ParameterDirection.Input);
                param.Add("@objname", dbType: DbType.String, value: sTableName, direction: ParameterDirection.Input);

                try
                {
                    var result = conn.Query<SpaceUsedTableInfo>("sp_spaceused", param, commandType: CommandType.StoredProcedure);

                    // set the return value
                    if (result.Count() > 0)
                    {
                        suti = result.First();
                        suti.Convert();

                        // get the number of columns
                        suti.numColumns = GetNumberOfColumns(sTableName);
                    }
                }
                catch (Exception exc) { }
            }

            return (suti);
        }

        // call "sp_spaceused" stored procedure to get size of table
        public List<SpaceUsedTableInfo> GetTableSizeInfo(IEnumerable<string> sTableNames)
        {
            List<SpaceUsedTableInfo> sutiList = new List<SpaceUsedTableInfo>();

            // iterate through the list and get the table information
            foreach (string sTableName in sTableNames)
            {
                sutiList.Add(GetTableSizeInfo(sTableName));
            }

            return (sutiList);
        }




        // call "sp_spaceused" stored procedure to get size of database
        public SpaceUsedDatabaseInfo GetDatabaseSizeInfo()
        {
            // returns the following
            // database_name    database_size       unallocated space   reserved        data            index_size      unused
            // HTS_Vault        65358.00 MB         38089.47 MB         27914784 KB     21359064 KB     6016496 KB      539224 KB

            SpaceUsedDatabaseInfo sudi = null;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_spaceused", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@oneresultset", 1));

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    // iterate through results
                    while (rdr.Read())
                    {
                        sudi = new SpaceUsedDatabaseInfo();
                        sudi.database_name = (string)rdr["database_name"];
                        sudi.database_size = (string)rdr["database_size"];
                        sudi.unallocated_space = (string)rdr["unallocated space"];
                        sudi.reserved = (string)rdr["reserved"];
                        sudi.data = (string)rdr["data"];
                        sudi.index_size = (string)rdr["index_size"];
                        sudi.unused = (string)rdr["unused"];

                        sudi.Convert();
                    }
                }
            }

            return (sudi);
        }

        #endregion SQL database statistics

    }

    // class used to return information on space used by a table
    // when querying database with the "sp_spaceused" stored procedure
    public class SpaceUsedTableInfo
    {
        public string name { get; set; } // nvarchar, not null
        public long rows { get; set; } // bigint, not null
        public long rowsAfterPurge { get; set; } // bigint, not null
        public string reserved { get; set; } // nvarchar, not null
        public string data { get; set; } // nvarchar, not null
        public string dataAfterPurge { get; set; } // nvarchar, not null
        public string index_size { get; set; } // nvarchar, not null
        public string unused { get; set; } // nvarchar, not null
        public long numColumns { get; set; } // bigint, not in result, queried with another method

        // method to convert string/size to string/size with commas
        public static string ConvertStringSizeOLD(string stringsize)
        {
            // split on the space
            string[] splits = stringsize.Split(' ');

            try
            {
                long val = long.Parse(splits[0]);
                stringsize = string.Format("{0:n0} {1}", val, splits[1]);
            }
            catch (Exception exc)
            {
                // probably was a double
                float val = float.Parse(splits[0]);
                stringsize = string.Format("{0:n} {1}", val, splits[1]);
            }


            return (stringsize);
        }

        // method to convert string/size to string/size with commas
        public static string ConvertStringSize(string stringsize)
        {
            object[] returnVals = GetSizeAndUnits(stringsize);

            string returnString;
            if (returnVals[0].GetType() == typeof(long))
            {
                returnString = string.Format("{0:n0} {1}", (long)returnVals[0], returnVals[1]);
            }
            else
            {
                returnString = string.Format("{0:n} {1}", (float)returnVals[0], returnVals[1]);
            }

            return (returnString);
        }


        public static object[] GetSizeAndUnits(string stringsize)
        {
            // split on the space
            string[] splits = stringsize.Split(' ');

            List<object> objectList = new List<object>();

            try
            {
                long val = long.Parse(splits[0]);
                objectList.Add(val);
                objectList.Add(splits[1]);
            }
            catch (Exception exc)
            {
                // probably was a double
                float val = float.Parse(splits[0]);
                objectList.Add(val);
                objectList.Add(splits[1]);
            }


            return (objectList.ToArray());
        }

        // convert all of the strings with sizes to numbers with commas for easy reading
        public void Convert()
        {
            reserved = ConvertStringSize(reserved);
            data = ConvertStringSize(data);
            index_size = ConvertStringSize(index_size);
            unused = ConvertStringSize(unused);
        }

        // method to set the rowsAfter purge and calculate the dataAfterPurge
        public void SetRowsAfterPurge(long rap)
        {
            // set the rows
            rowsAfterPurge = rap;

            // get the percentage
            float perc = ((float)rowsAfterPurge) / (float)rows;

            // get the size and units of the data
            object[] returnVals = GetSizeAndUnits(data);

            if (returnVals[0].GetType() == typeof(long))
            {
                dataAfterPurge = string.Format("{0:n0} {1}", (long)((long)returnVals[0] * perc), returnVals[1]);
            }
            else
            {
                dataAfterPurge = string.Format("{0:n} {1}", (float)((float)returnVals[0] * perc), returnVals[1]);
            }
        }

    }


    // class used to return information on space used by a table
    // when querying database with the "sp_spaceused" stored procedure
    public class SpaceUsedDatabaseInfo
    {
        // database_name    database_size       unallocated space   reserved        data            index_size      unused
        // HTS_Vault        65358.00 MB         38089.47 MB         27914784 KB     21359064 KB     6016496 KB      539224 KB

        public string database_name { get; set; } // nvarchar, not null
        public string database_size { get; set; } // nvarchar, not null
        public string unallocated_space { get; set; } // nvarchar, not null
        public string reserved { get; set; } // nvarchar, not null
        public string data { get; set; } // nvarchar, not null
        public string index_size { get; set; } // nvarchar, not null
        public string unused { get; set; } // nvarchar, not null

        // convert all of the strings with sizes to numbers with commas for easy reading
        public void Convert()
        {
            database_size = SpaceUsedTableInfo.ConvertStringSize(database_size);
            unallocated_space = SpaceUsedTableInfo.ConvertStringSize(unallocated_space);
            reserved = SpaceUsedTableInfo.ConvertStringSize(reserved);
            data = SpaceUsedTableInfo.ConvertStringSize(data);
            index_size = SpaceUsedTableInfo.ConvertStringSize(index_size);
            unused = SpaceUsedTableInfo.ConvertStringSize(unused);
        }
    }

}
