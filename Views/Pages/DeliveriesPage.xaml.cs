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

        private void DeleteDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (DeliveriesGrid.SelectedItem == null)
            {
                MessageBox.Show("Выбери поставку, умник.",
                    "Нечего удалять",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var delivery = (Delivery)DeliveriesGrid.SelectedItem;

            var result = MessageBox.Show(
                $"Удалить поставку ID {delivery.Id}?\nОтмены не будет, жизнь боль.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Movements WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", delivery.Id);

                    cmd.ExecuteNonQuery();
                }

                LoadDeliveries();

                MessageBox.Show("Поставка удалена.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить поставку:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
                MessageBox.Show("Поставка успешно добавлена",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadDeliveries();
            }
        }
    }
}