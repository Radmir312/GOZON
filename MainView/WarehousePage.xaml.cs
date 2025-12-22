using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.MainView
{
    public partial class WarehousesPage : Page
    {
        public WarehousesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            try
            {
                var warehouses = new List<Warehouse>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            w.Id,
                            w.Name,
                            w.Location,
                            COALESCE(SUM(s.Quantity), 0) as TotalStock
                        FROM Warehouses w
                        LEFT JOIN Stock s ON w.Id = s.WarehouseId
                        GROUP BY w.Id
                        ORDER BY w.Name";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            warehouses.Add(new Warehouse
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Location = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                TotalStock = reader.GetInt32(3)
                            });
                        }
                    }
                }

                WarehousesGrid.ItemsSource = warehouses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки складов: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }

        private void AddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Warehouses (Name, Location)
                        VALUES ('Новый склад', '')";

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Склад добавлен");
                    LoadWarehouses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления склада: " + ex.Message);
            }
        }
    }
}
