using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.MainView
{
    public partial class SuppliersPage : Page
    {
        public SuppliersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                var Suppliers = new List<Suppliers>();

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
                            Suppliers.Add(new Suppliers
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Location = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                TotalStock = reader.GetInt32(3)
                            });
                        }
                    }
                }

                SuppliersGrid.ItemsSource = Suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки складов: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
        }

        private void AddSuppliers_Click(object sender, RoutedEventArgs e)
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
                    LoadSuppliers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления склада: " + ex.Message);
            }
        }
    }
}
