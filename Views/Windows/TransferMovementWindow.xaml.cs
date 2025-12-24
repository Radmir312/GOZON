using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GOZON.Views.Main.Windows
{
    public partial class TransferMovementWindow : Window
    {
        private int selectedProductId;
        private int fromWarehouseId;
        private int toWarehouseId;
        private int quantity;
        private Dictionary<int, Dictionary<int, int>> warehouseStock = new Dictionary<int, Dictionary<int, int>>();

        public TransferMovementWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    // Загружаем товары
                    var products = new List<Product>();
                    cmd.CommandText = "SELECT Id, Name, SKU FROM Products ORDER BY Name";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                SKU = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            });
                        }
                    }

                    // Загружаем текущие остатки по складам
                    cmd.CommandText = "SELECT ProductId, WarehouseId, Quantity FROM Stock";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int productId = reader.GetInt32(0);
                            int warehouseId = reader.GetInt32(1);
                            int stock = reader.GetInt32(2);

                            if (!warehouseStock.ContainsKey(productId))
                                warehouseStock[productId] = new Dictionary<int, int>();

                            warehouseStock[productId][warehouseId] = stock;
                        }
                    }

                    // Загружаем склады
                    var warehouses = new List<Warehouse>();
                    cmd.CommandText = "SELECT Id, Name FROM Warehouses ORDER BY Name";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            warehouses.Add(new Warehouse
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductComboBox.ItemsSource = products;
                        FromWarehouseComboBox.ItemsSource = warehouses;
                        ToWarehouseComboBox.ItemsSource = warehouses;

                        if (products.Count > 0) ProductComboBox.SelectedIndex = 0;
                        if (warehouses.Count > 0)
                        {
                            FromWarehouseComboBox.SelectedIndex = 0;
                            ToWarehouseComboBox.SelectedIndex = warehouses.Count > 1 ? 1 : 0;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStockInfo();
        }

        private void FromWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStockInfo();
        }

        private void UpdateStockInfo()
        {
            if (ProductComboBox.SelectedItem is Product selectedProduct &&
                FromWarehouseComboBox.SelectedItem is Warehouse selectedWarehouse)
            {
                selectedProductId = selectedProduct.Id;
                fromWarehouseId = selectedWarehouse.Id;

                int stock = 0;
                if (warehouseStock.ContainsKey(selectedProductId) &&
                    warehouseStock[selectedProductId].ContainsKey(fromWarehouseId))
                {
                    stock = warehouseStock[selectedProductId][fromWarehouseId];
                }

                CurrentStockTextBlock.Text = stock.ToString();

                // Меняем цвет если мало товара
                if (stock < 10)
                    CurrentStockTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                else if (stock < 50)
                    CurrentStockTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                else
                    CurrentStockTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ProductComboBox.Focus();
                return;
            }

            if (FromWarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад-источник", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FromWarehouseComboBox.Focus();
                return;
            }

            if (ToWarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад-назначение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ToWarehouseComboBox.Focus();
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество (больше 0)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            selectedProductId = ((Product)ProductComboBox.SelectedItem).Id;
            fromWarehouseId = ((Warehouse)FromWarehouseComboBox.SelectedItem).Id;
            toWarehouseId = ((Warehouse)ToWarehouseComboBox.SelectedItem).Id;

            // Проверяем что склады разные
            if (fromWarehouseId == toWarehouseId)
            {
                MessageBox.Show("Склад-источник и склад-назначение должны быть разными",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка наличия достаточного количества товара
            int availableStock = 0;
            if (warehouseStock.ContainsKey(selectedProductId) &&
                warehouseStock[selectedProductId].ContainsKey(fromWarehouseId))
            {
                availableStock = warehouseStock[selectedProductId][fromWarehouseId];
            }

            if (quantity > availableStock)
            {
                MessageBox.Show($"Недостаточно товара на складе-источнике. Доступно: {availableStock}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                return;
            }

            try
            {
                using (var conn = Database.Open())
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int userId = 1; // Заглушка - нужно будет заменить на реального пользователя

                        // 1. Добавляем движение товара (перемещение)
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT INTO Movements 
                                (ProductId, FromWarehouseId, ToWarehouseId, Quantity, MovementType, UserId)
                                VALUES (@productId, @fromWarehouseId, @toWarehouseId, @quantity, 'MOVE', @userId);
                                SELECT last_insert_rowid();";

                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@fromWarehouseId", fromWarehouseId);
                            cmd.Parameters.AddWithValue("@toWarehouseId", toWarehouseId);
                            cmd.Parameters.AddWithValue("@quantity", quantity);
                            cmd.Parameters.AddWithValue("@userId", userId);

                            cmd.ExecuteScalar();
                        }

                        // 2. Уменьшаем остатки на складе-источнике
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                UPDATE Stock 
                                SET Quantity = Quantity - @quantity
                                WHERE ProductId = @productId AND WarehouseId = @fromWarehouseId";

                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@fromWarehouseId", fromWarehouseId);
                            cmd.Parameters.AddWithValue("@quantity", quantity);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                // Создаем запись если её не было (но не должно быть такого случая)
                                cmd.CommandText = @"
                                    INSERT INTO Stock (ProductId, WarehouseId, Quantity)
                                    VALUES (@productId, @fromWarehouseId, -@quantity)";
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Увеличиваем остатки на складе-назначении
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT INTO Stock (ProductId, WarehouseId, Quantity)
                                VALUES (@productId, @toWarehouseId, @quantity)
                                ON CONFLICT(ProductId, WarehouseId) 
                                DO UPDATE SET Quantity = Quantity + @quantity";

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@toWarehouseId", toWarehouseId);
                            cmd.Parameters.AddWithValue("@quantity", quantity);

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        DialogResult = true;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения перемещения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}