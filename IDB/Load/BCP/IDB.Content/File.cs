using System;

namespace IDB.Load.BCP.IDB.Content
{
    class File
    {
        private string _name;
        private int _id;
        private string _localPath;
        private DateTime _date;
        private string _iterationId;
        internal string Name { set { _name = value; } get { return _name; } }
        internal string LocalPath { set { _localPath = value; } get { return _localPath; } }
        internal int Id { set { _id = value; } get { return _id; } }
        internal string IterationId { set { _iterationId = value; } get { return _iterationId; } }
        internal DateTime Date { set { _date = value; } get { return _date; } }
      internal  File(int fileId,string fileName, string fileLocalPath, DateTime createdDate,string fileIterationId) 
        {
            _iterationId = fileIterationId;
            _id = fileId;
            _name = fileName;
            _localPath = fileLocalPath;
            _date = createdDate;
        }
    }
}
