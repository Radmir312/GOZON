using System;
using System.Data.SQLite;
using System.IO;

namespace GOZON
{
    public static class Database
    {
        public static string DbPath = Path.Combine(
        Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
        "warehouse.db"
        );

        public static SQLiteConnection Open()
        {
            var conn = new SQLiteConnection($"Data Source={DbPath};Version=3;");
            conn.Open();
            return conn;
        }
    }
}
