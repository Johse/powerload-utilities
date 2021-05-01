using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using IDB.Core.Data.Entity;
using IDB.Core.Extensions;

namespace IDB.Core.Test
{
    [TestClass]
    public class SqlTests
    {
        private string _connectionString = @"Server=(local)\AUTODESKVAULT;Database=Load;Trusted_Connection=True;";

        [TestMethod]
        public void FindSingleFolderTest()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "SELECT TOP 1 * FROM Folders WHERE Path = @Path";
                var folder = connection.SelectEntity<Folder>(sql, new { Path = "$/Designs/Projects/" });
            }
        }

        [TestMethod]
        public void FindMultipleFoldersTest()
        {
            using (var connection = new SqlConnection(_connectionString))

            {
                connection.Open();
                var sql = "SELECT * FROM Folders";
                var folders = connection.SelectEntities<Folder>(sql);

                Assert.IsTrue(folders.Any());
            }
        }

        [TestMethod]
        public void InsertAndUpdateFolderTest()
        {
            var folderName = Guid.NewGuid().ToString();
            var folder = new Folder
            {
                FolderName = folderName,
                Path = $"$/Designs/Projects/{folderName}",
                IsLibrary = false,
                Category = "Project",
                LifecycleDefinition = "Flexible Release Process",
                LifecycleState = "Released",
                CreateUser = "Administrator",
                CreateDate = DateTime.Now,
                UserDefinedProperties = new Dictionary<string, object>
                {
                    {"Description", "Unit Test"}
                }
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                folder.Insert(connection);
                Assert.IsTrue(folder.FolderID > 0);

                folder.Tag = "Unit Test";
                folder.Update(connection);
            }
        }

        [TestMethod]
        public void FindSingleFileTest()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "SELECT TOP 1 * FROM Files WHERE LocalFullFileName = @LocalFullFileName ORDER BY Version";
                var file = connection.SelectEntity<Data.Entity.File>(sql, new { LocalFullFileName = "$/Designs/Projects/Wheel/Wheel.ipt" });
            }
        }

        [TestMethod]
        public void FindMultipleFilesTest()
        {
            using (var connection = new SqlConnection(_connectionString))

            {
                connection.Open();
                var sql = "SELECT * FROM Files WHERE FileName LIKE @FileName";
                var files = connection.SelectEntities<Data.Entity.File>(sql, new { FileName = "%.ipt" });

                Assert.IsTrue(files.Any());
            }
        }

        [TestMethod]
        public void InsertAndUpdateFileTest()
        {
            var fileName = Guid.NewGuid() + ".ipt";
            var file = new Data.Entity.File
            {
                LocalFullFileName = $"$/Designs/Projects/Test/{fileName}",
                FileName = fileName,
                FolderID = 1,
                Version = 1,
                CreateDate = DateTime.Now,
                UserDefinedProperties = new Dictionary<string, object>
                {
                    {"Part Number", "000001"}, {"Description", "Wheel part"}, {"Title", "Christian"}
                }
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                file.Insert(connection);
                Assert.IsTrue(file.FileID > 0);

                file.Tag = "_321";
                file.Update(connection);
            }
        }
    }
}
