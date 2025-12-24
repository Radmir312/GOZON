using GOZON.Models;
using System;
using System.Windows;

namespace GOZON.Views.Windows
{
    public partial class AddEditProductWindow : Window
    {
        public Product Product { get; private set; }
        private bool IsEditMode => Product.Id > 0;

        public AddEditProductWindow(Product product = null)
        {
            InitializeComponent();

            Product = product ?? new Product
            {
                Name = "",
                SKU = "",
                Price = 0,
                MinQuantity = 0,
                Description = ""
            };

            DataContext = Product;
            Title = IsEditMode ? "Редактировать товар" : "Добавить товар";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                MessageBox.Show("Введите название товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }

            if (!int.TryParse(MinQuantityTextBox.Text, out int minQuantity) || minQuantity < 0)
            {
                MessageBox.Show("Введите корректное минимальное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MinQuantityTextBox.Focus();
                return;
            }

            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    if (IsEditMode)
                    {
                        // Обновление существующего товара
                        cmd.CommandText = @"
                            UPDATE Products 
                            SET Name = @name, 
                                SKU = @sku, 
                                Price = @price, 
                                MinQuantity = @minQuantity,
                                Description = @description
                            WHERE Id = @id";
                    }
                    else
                    {
                        // Добавление нового товара
                        cmd.CommandText = @"
                            INSERT INTO Products (Name, SKU, Price, MinQuantity, Description)
                            VALUES (@name, @sku, @price, @minQuantity, @description);
                            SELECT last_insert_rowid();";
                    }

                    cmd.Parameters.AddWithValue("@name", Product.Name);
                    cmd.Parameters.AddWithValue("@sku", Product.SKU ?? string.Empty);
                    cmd.Parameters.AddWithValue("@price", Product.Price);
                    cmd.Parameters.AddWithValue("@minQuantity", Product.MinQuantity);
                    cmd.Parameters.AddWithValue("@description", Product.Description ?? string.Empty);

                    if (IsEditMode)
                    {
                        cmd.Parameters.AddWithValue("@id", Product.Id);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var newId = cmd.ExecuteScalar();
                        Product.Id = Convert.ToInt32(newId);
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (System.Data.SQLite.SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
            {
                MessageBox.Show("Товар с таким артикулом уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SKUTextBox.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
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