using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using IDB.Core.Extensions;
using File = IDB.Core.DbEntity.File;

namespace IDB.Core.Test
{
    [TestClass]
    public class UnitTest1
    {
        private string _connectionString = @"Server=(local)\AUTODESKVAULT;Database=Load;Trusted_Connection=True;";

        [TestMethod]
        public void FindSingleFileTest()
        {
            DynamicTypes.DynamicTypeInstance.Create(_connectionString);
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "SELECT TOP 1 * FROM Files WHERE LocalFullFileName = @LocalFullFileName ORDER BY Version";
                var file = connection.FindSingle<File>(sql, new { LocalFullFileName = "$/Designs/Projects/Wheel/Wheel.ipt" });

                Assert.IsFalse(file is null);

            }
        }

        [TestMethod]
        public void FindMultipleFilesTest()
        {
            DynamicTypes.DynamicTypeInstance.Create(_connectionString);
            using (var connection = new SqlConnection(_connectionString))

            {
                connection.Open();
                var sql = "SELECT * FROM Files WHERE FileName = @FileName";
                var files = connection.Find<File>(sql, new { FileName = "test1.ipt" });

                Assert.IsTrue(files.Any());
            }
        }

        [TestMethod]
        public void InsertAndUpdateFileTest()
        {
            var fileName = Guid.NewGuid() + ".ipt";
            var file = new File
            {
                LocalFullFileName = $"$/Designs/Projects/Test/{fileName}",
                FileName = fileName,
                FolderID = 2,
                Version = 1,
                CreateDate = DateTime.Now,
                UserDefinedProperties = new Dictionary<string, object>
                {
                    {"Part Number", "000001"}, {"Description", "Wheel part"}, {"Title", "Christian"}
                }
            };

            DynamicTypes.DynamicTypeInstance.Create(_connectionString);
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var insertResult = file.Insert(connection);
                Assert.IsTrue(insertResult);

                file.Tag = "_321";
                var updateResult = file.Update(connection);
                Assert.IsTrue(updateResult);
            }
        }
    }
}
