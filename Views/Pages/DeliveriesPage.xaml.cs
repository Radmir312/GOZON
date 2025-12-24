using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Main.Windows
{
    public partial class DeliveriesPage : Page
    {
        public DeliveriesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDeliveries();
        }

        private void LoadDeliveries()
        {
            try
            {
                var deliveries = new List<Delivery>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            m.Id,
                            p.Name,
                            s.Name,
                            w.Name,
                            m.Quantity,
                            m.CreatedAt
                        FROM Movements m
                        JOIN Products p ON p.Id = m.ProductId
                        JOIN Suppliers s ON s.Id = m.SupplierId
                        JOIN Warehouses w ON w.Id = m.ToWarehouseId
                        WHERE m.MovementType = 'IN'
                        ORDER BY m.CreatedAt DESC";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deliveries.Add(new Delivery
                            {
                                Id = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                SupplierName = reader.GetString(2),
                                WarehouseName = reader.GetString(3),
                                Quantity = reader.GetInt32(4),
                                Date = reader.GetDateTime(5).ToString("dd.MM.yyyy HH:mm")
                            });
                        }
                    }
                }

                DeliveriesGrid.ItemsSource = deliveries;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки поставок: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDeliveries();
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddDeliveryWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Поставка успешно добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDeliveries();
            }
        }
    }
}