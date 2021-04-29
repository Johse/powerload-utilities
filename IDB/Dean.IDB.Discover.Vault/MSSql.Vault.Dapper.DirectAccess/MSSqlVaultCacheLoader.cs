using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dapper;

using log4net;
using BCPBuilderConfig;


namespace MSSql.Vault.Dapper.DirectAccess
{
    public class MSSqlVaultCacheLoader
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(MSSqlVaultCacheLoader));


        private string _connectionString;

        public MSSqlVaultCacheLoader(string connectionString)
        {
            _connectionString = connectionString;
        }

        // template method to combine dictionaries and make sure they don't have duplicates
        static public Dictionary<long, T> CombineDictionaries<T>(Dictionary<long, T> dictionary1, Dictionary<long, T> dictionary2) where T : IVaultDbEntity
        {
            Dictionary<long, T> resultantDictionary = null;
            try
            {
                // check to see the purmutations
                if ((dictionary1 != null) && ((dictionary2 == null) || dictionary2.Count() == 0))
                {
                    resultantDictionary = dictionary1;
                }
                else if (((dictionary1 == null) || (dictionary1.Count() == 0)) && (dictionary2 != null))
                {
                    resultantDictionary = dictionary2;
                }
                else
                {
                    resultantDictionary = dictionary1.Concat(dictionary2).GroupBy(kvp => kvp.Key, kvp => kvp.Value).ToDictionary(g => g.Key, g => g.Last());
                }
            }
            catch (Exception exc)
            {
                Logger.Debug("Error", exc);

                // rethrow the exception
                throw (exc);
            }

            // return the combined dictionary
            return (resultantDictionary);
        }

        static public Dictionary<long, T> CombineDictionariesOld<T>(Dictionary<long, T> dictionary1, Dictionary<long, T> dictionary2) where T : IVaultDbEntity
        {
            // check to see the purmutations
            Dictionary<long, T> resultantDictionary = null;

            try
            {
                if ((dictionary1 != null) && (dictionary2 == null))
                {
                    resultantDictionary = dictionary1;
                }
                else if ((dictionary1 == null) && (dictionary2 != null))
                {
                    resultantDictionary = dictionary2;
                }
                else
                {
                    // combine them and check for multiples
                    List<T> combinedLists = dictionary1.Values.Union(dictionary2.Values).ToList();

                    // union does not remove duplicates unless we supply a comparison operator on the objects
                    // resultantDictionary = combinedLists.ToDictionary(idx => idx.GetId(), idx => idx);
                    resultantDictionary = combinedLists.GroupBy(idx => idx.GetId()).ToDictionary(g => g.Key, g => g.First());
                }
            }
            catch (Exception exc)
            {
                Logger.Debug("Error", exc);

                // rethrow the exception
                throw (exc);
            }

            // return the combined dictionary
            return (resultantDictionary);
        }



        // TODO: optimize throughput by identifying the Enumerable parameters and putting them into a temporary table
        // sql bulk copy them to the table, and allow multiple of these types
        // make sure the table goes away after use
        // in this manner can then do SQL analysis on LARGE parameterized sets, and on mulitple parameters
        public List<T> LoadVaultEntities<T>(string selectQuery = null, object param = null) where T : IVaultDbEntity, new()
        {
            var tempT = new T();
            selectQuery = selectQuery ?? tempT.GetSelectString();

            List<T> vaultEntityList = null;
            using (var conn = new SqlConnection())
            {
                try
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
                catch (Exception exc)
                {
                    Logger.Debug("Error", exc);

                    // rethrow the exception
                    throw (exc);
                }
            }

            return vaultEntityList;
        }

        // TODO: optimize throughput by identifying the Enumerable parameters and putting them into a temporary table
        // sql bulk copy them to the table, and allow multiple of these types
        // make sure the table goes away after use
        // in this manner can then do SQL analysis on LARGE parameterized sets, and on mulitple parameters
        public Dictionary<long, T> LoadVaultEntityDictionary<T>(string selectQuery = null, object param = null) where T : IVaultDbEntity, new()
        {
            Dictionary<long, T> vaultEntityDictionary = null;

            List<T> vaultEntityList = LoadVaultEntities<T>(selectQuery, param);

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

            try
            {
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
            }
            catch (Exception exc)
            {
                Logger.Debug("Error", exc);

                // rethrow the exception
                throw (exc);
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

            try
            {
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
            }
            catch (Exception exc)
            {
                Logger.Debug("Error", exc);

                // rethrow the exception
                throw (exc);
            }


            return (IsAnonymousEtc);
        }


        public T LoadSingleVaultEntity<T>(string selectQuery, object paramaters) where T : IVaultDbEntity, new()
        {
            using (var conn = new SqlConnection())
            {
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    var entities = conn.Query<T>(selectQuery, paramaters, commandTimeout: 600);

                    T entity = entities.Single();
                    return entity;
                }
                catch (Exception exc)
                {
                    Logger.Debug("Error", exc);

                    // rethrow the exception
                    throw (exc);
                }
            }

        }


        // this assumes that the name of the class is the same as the SQL table name
        public int GetTableCount<T>() where T : IVaultDbEntity
        {
            using (var conn = new SqlConnection())
            {
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    string selectQuery = string.Format("SELECT COUNT(*) FROM [dbo].[{0}]", typeof(T).Name);
                    int count = (int)conn.ExecuteScalar(selectQuery, commandTimeout: 600);

                    return (count);
                }
                catch (Exception exc)
                {
                    Logger.Debug("Error", exc);

                    // rethrow the exception
                    throw (exc);
                }
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
                    }
                }
                catch (Exception exc)
                {
                    Logger.Debug("Error", exc);

                    // rethrow the exception
                    throw (exc);
                }
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
                try
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
                catch (Exception exc)
                {
                    Logger.Debug("Error", exc);

                    // rethrow the exception
                    throw (exc);
                }
            }

            return (sudi);
        }

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
