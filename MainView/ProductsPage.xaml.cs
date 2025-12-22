using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.MainView
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
                                Price = Convert.ToDecimal(reader.GetDouble(3)),
                                TotalQuantity = reader.GetInt32(4)
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
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Products (Name, SKU, Price, MinQuantity)
                        VALUES ('Новый товар', '', 0, 0)";

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Товар добавлен");
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}