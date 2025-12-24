using GOZON.Models;
using GOZON.Views.Windows;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Pages
{
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            UpdateButtonsState();
        }

        private void LoadProducts()
        {
            try
            {
                var products = new List<Product>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            p.Id,
                            p.Name,
                            p.SKU,
                            p.Price,
                            p.MinQuantity,
                            p.Description,
                            COALESCE(SUM(s.Quantity), 0) as TotalQuantity
                        FROM Products p
                        LEFT JOIN Stock s ON p.Id = s.ProductId
                        GROUP BY p.Id
                        ORDER BY p.Name";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                SKU = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                MinQuantity = reader.GetInt32(4),
                                Description = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                TotalQuantity = reader.GetInt32(6)
                            });
                        }
                    }
                }

                ProductsGrid.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditProductWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Товар успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                var window = new AddEditProductWindow(selectedProduct);
                if (window.ShowDialog() == true)
                {
                    MessageBox.Show("Товар успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProducts();
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить товар '{selectedProduct.Name}'?",
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
                            // Сначала проверяем, есть ли остатки
                            cmd.CommandText = "SELECT COUNT(*) FROM Stock WHERE ProductId = @id";
                            cmd.Parameters.AddWithValue("@id", selectedProduct.Id);
                            var stockCount = Convert.ToInt32(cmd.ExecuteScalar());

                            if (stockCount > 0)
                            {
                                MessageBox.Show("Невозможно удалить товар, так как он числится на складе. Сначала удалите все остатки.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Проверяем, есть ли движения товара
                            cmd.CommandText = "SELECT COUNT(*) FROM Movements WHERE ProductId = @id";
                            var movementsCount = Convert.ToInt32(cmd.ExecuteScalar());

                            if (movementsCount > 0)
                            {
                                MessageBox.Show("Невозможно удалить товар, так как есть история его движений. Удалите сначала все связанные движения.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Удаляем товар
                            cmd.CommandText = "DELETE FROM Products WHERE Id = @id";
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Товар успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadProducts();
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

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool isSelected = ProductsGrid.SelectedItem != null;
            EditButton.IsEnabled = isSelected;
            DeleteButton.IsEnabled = isSelected;
        }
    }
}