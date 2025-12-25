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
    public partial class AddDeliveryWindow : Window
    {
        private int selectedProductId;
        private int selectedSupplierId;
        private int selectedWarehouseId;
        private int quantity;
        private Dictionary<int, int> productStock = new Dictionary<int, int>();

        public AddDeliveryWindow()
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


                    cmd.CommandText = "SELECT ProductId, SUM(Quantity) FROM Stock GROUP BY ProductId";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productStock[reader.GetInt32(0)] = reader.GetInt32(1);
                        }
                    }


                    var suppliers = new List<Supplier>();
                    cmd.CommandText = "SELECT Id, Name FROM Suppliers ORDER BY Name";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(new Supplier
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }

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
                        SupplierComboBox.ItemsSource = suppliers;
                        WarehouseComboBox.ItemsSource = warehouses;

                        if (products.Count > 0) ProductComboBox.SelectedIndex = 0;
                        if (suppliers.Count > 0) SupplierComboBox.SelectedIndex = 0;
                        if (warehouses.Count > 0) WarehouseComboBox.SelectedIndex = 0;
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
            if (ProductComboBox.SelectedItem is Product selectedProduct)
            {
                selectedProductId = selectedProduct.Id;

                if (productStock.TryGetValue(selectedProduct.Id, out int stock))
                {
                    CurrentStockTextBlock.Text = stock.ToString();
                }
                else
                {
                    CurrentStockTextBlock.Text = "0";
                }
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ProductComboBox.Focus();
                return;
            }

            if (SupplierComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SupplierComboBox.Focus();
                return;
            }

            if (WarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                WarehouseComboBox.Focus();
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
            selectedSupplierId = ((Supplier)SupplierComboBox.SelectedItem).Id;
            selectedWarehouseId = ((Warehouse)WarehouseComboBox.SelectedItem).Id;

            try
            {
                using (var conn = Database.Open())
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int userId = 1; 
                        int deliveryId = 0;

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT INTO Movements 
                                (ProductId, SupplierId, ToWarehouseId, Quantity, MovementType, UserId)
                                VALUES (@productId, @supplierId, @warehouseId, @quantity, 'IN', @userId);
                                SELECT last_insert_rowid();";

                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@supplierId", selectedSupplierId);
                            cmd.Parameters.AddWithValue("@warehouseId", selectedWarehouseId);
                            cmd.Parameters.AddWithValue("@quantity", quantity);
                            cmd.Parameters.AddWithValue("@userId", userId);

                            deliveryId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT OR REPLACE INTO Stock (ProductId, WarehouseId, Quantity)
                                VALUES (
                                    @productId, 
                                    @warehouseId, 
                                    COALESCE(
                                        (SELECT Quantity FROM Stock 
                                         WHERE ProductId = @productId AND WarehouseId = @warehouseId), 
                                        0
                                    ) + @quantity
                                )";

                            cmd.Parameters.AddWithValue("@productId", selectedProductId);
                            cmd.Parameters.AddWithValue("@warehouseId", selectedWarehouseId);
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
                MessageBox.Show($"Ошибка сохранения поставки: {ex.Message}", "Ошибка",
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