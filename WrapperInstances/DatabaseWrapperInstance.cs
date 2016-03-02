using System;
using System.IO;
using System.Linq.Expressions;

using Aragas.Core.Wrappers;

using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.XamarinAndroid;

namespace PokeD.Server.Android.WrapperInstances
{
    /// <summary>
    /// SQL Database, only Primitive Types.
    /// </summary>
    public class SQLiteDatabase : IDatabase
    {
        public string FileExtension => ".sqlite3";

        private SQLiteConnection Connection { get; set; }


        public IDatabase CreateDB(string databaseName)
        {
			Connection = new SQLiteConnection(new SQLitePlatformAndroid(), CombinePath(databaseName));

            return this;
        }
        
        public void CreateTable<T>() where T : DatabaseTable, new()
        {
			Connection.CreateTable<T>(CreateFlags.ImplicitPK | CreateFlags.AutoIncPK);
        }

        public void Insert<T>(T obj) where T : DatabaseTable, new()
        {
            Connection.Insert(obj);
        }

        public void Update<T>(T obj) where T : DatabaseTable, new()
        {
            Connection.Update(obj);
        }

        public T Find<T>(Expression<Func<T, bool>> predicate) where T : DatabaseTable, new() => Connection.Find(predicate);


        private string CombinePath(string fileName) { return Path.Combine(FileSystemWrapper.DatabaseFolder.Path, fileName + FileExtension); }
    }
}
