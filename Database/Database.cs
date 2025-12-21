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
                    -- Пользователи
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Login TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        Email TEXT,
                        Role TEXT DEFAULT 'User',
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    -- Склады
                    CREATE TABLE IF NOT EXISTS Warehouses (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Location TEXT
                    );

                    -- Товары
                    CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        SKU TEXT UNIQUE,
                        Description TEXT,
                        Price REAL NOT NULL,
                        MinQuantity INTEGER DEFAULT 0
                    );

                    -- Остатки по складам
                    CREATE TABLE IF NOT EXISTS Stock (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductId INTEGER NOT NULL,
                        WarehouseId INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL DEFAULT 0,
                        UNIQUE(ProductId, WarehouseId),
                        FOREIGN KEY (ProductId) REFERENCES Products(Id),
                        FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id)
                    );

                    -- Поставщики
                    CREATE TABLE IF NOT EXISTS Suppliers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        ContactPerson TEXT,
                        Phone TEXT,
                        Email TEXT,
                        Address TEXT
                    );

                    -- Движения товаров
                    CREATE TABLE IF NOT EXISTS Movements (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductId INTEGER NOT NULL,
                        FromWarehouseId INTEGER,
                        ToWarehouseId INTEGER,
                        SupplierId INTEGER,
                        Quantity INTEGER NOT NULL,
                        MovementType TEXT NOT NULL, -- IN, OUT, MOVE
                        UserId INTEGER,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

                        FOREIGN KEY (ProductId) REFERENCES Products(Id),
                        FOREIGN KEY (FromWarehouseId) REFERENCES Warehouses(Id),
                        FOREIGN KEY (ToWarehouseId) REFERENCES Warehouses(Id),
                        FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );

                    -- История изменений
                    CREATE TABLE IF NOT EXISTS History (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Entity TEXT NOT NULL,
                        EntityId INTEGER NOT NULL,
                        Action TEXT NOT NULL,
                        OldValue TEXT,
                        NewValue TEXT,
                        UserId INTEGER,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );
                    ";

                cmd.ExecuteNonQuery();
            }
        }
    }
}
