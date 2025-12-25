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
    public partial class OutgoingMovementWindow : Window
    {
        private int selectedProductId;
        private int selectedWarehouseId;
        private int quantity;
        private string reason;
        private Dictionary<int, Dictionary<int, int>> warehouseStock = new Dictionary<int, Dictionary<int, int>>();

        public OutgoingMovementWindow()
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

                    // Загружаем остатки товаров
                    cmd.CommandText = "SELECT ProductId, WarehouseId, Quantity FROM Stock";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int productId = reader.GetInt32(0);
                            int warehouseId = reader.GetInt32(1);
                            int quantity = reader.GetInt32(2);

                            if (!warehouseStock.ContainsKey(productId))
                            {
                                warehouseStock[productId] = new Dictionary<int, int>();
                            }

                            warehouseStock[productId][warehouseId] = quantity;
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductComboBox.ItemsSource = products;
                        WarehouseComboBox.ItemsSource = warehouses;

                        if (products.Count > 0) ProductComboBox.SelectedIndex = 0;
                        if (warehouses.Count > 0) WarehouseComboBox.SelectedIndex = 0;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStockInfo();
        }

        private void WarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStockInfo();
        }

        private void UpdateStockInfo()
        {
            if (ProductComboBox.SelectedItem is Product selectedProduct &&
                WarehouseComboBox.SelectedItem is Warehouse selectedWarehouse)
            {
                selectedProductId = selectedProduct.Id;
                selectedWarehouseId = selectedWarehouse.Id;

                int stock = 0;
                if (warehouseStock.ContainsKey(selectedProductId) &&
                    warehouseStock[selectedProductId].ContainsKey(selectedWarehouseId))
                {
                    stock = warehouseStock[selectedProductId][selectedWarehouseId];
                }

                Dispatcher.Invoke(() =>
                {
                    CurrentStockTextBlock.Text = stock.ToString();

                    // Меняем цвет если мало товара
                    if (stock < 10)
                        CurrentStockTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    else if (stock < 50)
                        CurrentStockTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                    else
                        CurrentStockTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                });
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
                MessageBox.Show("Выберите товар",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                ProductComboBox.Focus();
                return;
            }

            if (WarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                WarehouseComboBox.Focus();
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество (больше 0)",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            selectedProductId = ((Product)ProductComboBox.SelectedItem).Id;
            selectedWarehouseId = ((Warehouse)WarehouseComboBox.SelectedItem).Id;
            reason = ReasonTextBox.Text.Trim();

            // Проверка наличия достаточного количества товара
            int availableStock = 0;
            if (warehouseStock.ContainsKey(selectedProductId) &&
                warehouseStock[selectedProductId].ContainsKey(selectedWarehouseId))
            {
                availableStock = warehouseStock[selectedProductId][selectedWarehouseId];
            }

            if (quantity > availableStock)
            {
                MessageBox.Show($"Недостаточно товара на складе. Доступно: {availableStock}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

                        // 1. Добавляем движение товара (отгрузка)
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT INTO Movements 
                                (ProductId, FromWarehouseId, Quantity, MovementType, UserId)
                                VALUES (@productId, @warehouseId, @quantity, 'OUT', @userId);
                                SELECT last_insert_rowid();";

                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@warehouseId", selectedWarehouseId);
                            cmd.Parameters.AddWithValue("@quantity", quantity);
                            cmd.Parameters.AddWithValue("@userId", userId);

                            int movementId = Convert.ToInt32(cmd.ExecuteScalar());

                            // Добавляем причину в историю если указана
                            if (!string.IsNullOrWhiteSpace(reason))
                            {
                                cmd.CommandText = @"
                                    INSERT INTO History 
                                    (Entity, EntityId, Action, NewValue, UserId)
                                    VALUES ('Movement', @movementId, 'OUTGOING', @reason, @userId)";

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@movementId", movementId);
                                cmd.Parameters.AddWithValue("@reason", $"Причина: {reason}");
                                cmd.Parameters.AddWithValue("@userId", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 2. Уменьшаем остатки на складе
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                UPDATE Stock 
                                SET Quantity = Quantity - @quantity
                                WHERE ProductId = @productId AND WarehouseId = @warehouseId";

                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@warehouseId", selectedWarehouseId);
                            cmd.Parameters.AddWithValue("@quantity", quantity);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                // Если записи не было, создаем с отрицательным количеством?
                                // Или просто сообщаем об ошибке
                                throw new Exception("Не удалось обновить остатки товара.");
                            }
                        }

                        // Обновляем локальный словарь остатков
                        if (warehouseStock.ContainsKey(selectedProductId) &&
                            warehouseStock[selectedProductId].ContainsKey(selectedWarehouseId))
                        {
                            warehouseStock[selectedProductId][selectedWarehouseId] -= quantity;
                        }

                        transaction.Commit();
                        DialogResult = true;
                        Close();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения отгрузки: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}