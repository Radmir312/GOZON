using System;
using System.Data.SQLite;
using System.IO;

namespace GOZON
{
    public static class Database
    {
        private static string connectionString;

        public static SQLiteConnection Open()
        {
            var conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
        }

        public static void Init()
        {
            string folder = Path.Combine(AppContext.BaseDirectory, @"..\..\..\Database");
            folder = Path.GetFullPath(folder);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string dbPath = Path.Combine(folder, "warehouse.db");
            connectionString = $@"Data Source={dbPath};Version=3;";

            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Email TEXT,
                    Role TEXT DEFAULT 'User',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Warehouses (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Location TEXT,
                    Capacity INTEGER
                );

                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SKU TEXT UNIQUE,
                    Description TEXT,
                    Price REAL NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 0,
                    WarehouseId INTEGER,
                    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id)
                );

                CREATE TABLE IF NOT EXISTS Suppliers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Contact TEXT,
                    Email TEXT,
                    Phone TEXT,
                    Address TEXT
                );

                CREATE TABLE IF NOT EXISTS Deliveries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    SupplierId INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    DeliveryDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id),
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
                );

                CREATE TABLE IF NOT EXISTS Actions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    ActionType TEXT,
                    Description TEXT,
                    ActionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS Reports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Content TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy INTEGER,
                    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS History (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Entity TEXT,
                    EntityId INTEGER,
                    UserId INTEGER,
                    Change TEXT,
                    ChangeDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Key TEXT NOT NULL UNIQUE,
                    Value TEXT
                );
                ";

                cmd.ExecuteNonQuery();
            }
        }
    }
}
