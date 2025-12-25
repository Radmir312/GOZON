using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Main.Windows
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
            UpdateButtonsState();
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
            var window = new AddEditWarehouseWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Склад успешно добавлен",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadWarehouses();
            }
        }

        private void EditWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (WarehousesGrid.SelectedItem is Warehouse selectedWarehouse)
            {
                var window = new AddEditWarehouseWindow(selectedWarehouse);
                if (window.ShowDialog() == true)
                {
                    MessageBox.Show("Склад успешно обновлен",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadWarehouses();
                }
            }
        }

        private void DeleteWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (WarehousesGrid.SelectedItem is Warehouse selectedWarehouse)
            {

                if (selectedWarehouse.TotalStock > 0)
                {
                    MessageBox.Show("Невозможно удалить склад, так как на нем есть товары. " +
                                  "Сначала переместите все товары на другие склады или списывайте их.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить склад '{selectedWarehouse.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = Database.Open())
                        using (var cmd = conn.CreateCommand())
                        {

                            cmd.CommandText = @"
                                SELECT COUNT(*) FROM Movements 
                                WHERE FromWarehouseId = @id OR ToWarehouseId = @id";
                            cmd.Parameters.AddWithValue("@id", selectedWarehouse.Id);

                            var movementsCount = Convert.ToInt32(cmd.ExecuteScalar());

                            if (movementsCount > 0)
                            {
                                MessageBox.Show("Невозможно удалить склад, так как есть история его движений. " +
                                              "Удалите сначала все связанные движения.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            cmd.CommandText = "DELETE FROM Stock WHERE WarehouseId = @id";
                            cmd.ExecuteNonQuery();

                            // Удаляем склад
                            cmd.CommandText = "DELETE FROM Warehouses WHERE Id = @id";
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Склад успешно удален",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadWarehouses();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void WarehousesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool isSelected = WarehousesGrid.SelectedItem != null;
            EditButton.IsEnabled = isSelected;
            DeleteButton.IsEnabled = isSelected;
        }
    }
}