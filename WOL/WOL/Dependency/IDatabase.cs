using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WOL.Models;

namespace WOL.Dependency
{
    public interface IDatabase
    {
        string GetDatabasePath();
    }
    public class DbInstance
    {
        string path;
        object lockObject = new object();
        public DbInstance(string path)
        {
            this.path = path;
        }
        public void CreateDatabase()
        {
            lock (lockObject)
            {
                var connection = new SQLiteConnection(path);
                {
                    if (TableExists<Machine>(connection))
                    {
                        return;
                    }
                    connection.CreateTable<Machine>();
                }
            }
        }
        public void insertUpdateData(Machine data)
        {
            lock (lockObject)
            {
                var db = new SQLiteConnection(path);
                db.InsertOrReplace(data);
            }

        }
        public void DeleteData(Machine data)
        {
            lock (lockObject)
            {
                var db = new SQLiteConnection(path);
                db.Delete(data);
            }

        }
        public List<Machine> findRecords()
        {
            lock (lockObject)
            {
                var db = new SQLiteConnection(path);
                if (TableExists<Machine>(db))
                {
                    var lstMachine = db.Table<Machine>().ToList();
                    return lstMachine;
                }
                return new List<Machine>();
            }
        }
        public bool TableExists<T>(SQLiteConnection connection)
        {
            lock (lockObject)
            {
                const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
                var cmd = connection.ExecuteScalar<string>(cmdText, typeof(T).Name);
                return cmd != null;
            }
        }
    }
}
