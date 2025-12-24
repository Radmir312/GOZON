using GOZON.Models;
using System;
using System.Data.SQLite;
using System.Windows;

namespace GOZON.Views.Main.Windows
{
    public partial class AddEditWarehouseWindow : Window
    {
        public Warehouse Warehouse { get; private set; }
        private bool IsEditMode => Warehouse.Id > 0;

        public AddEditWarehouseWindow(Warehouse warehouse = null)
        {
            InitializeComponent();

            Warehouse = warehouse ?? new Warehouse
            {
                Name = "",
                Location = "",
                TotalStock = 0
            };

            DataContext = Warehouse;
            Title = IsEditMode ? "Редактировать склад" : "Добавить склад";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Warehouse.Name))
            {
                MessageBox.Show("Введите название склада", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (Warehouse.Name.Length > 100)
            {
                MessageBox.Show("Название склада не должно превышать 100 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (Warehouse.Location?.Length > 200)
            {
                MessageBox.Show("Локация не должна превышать 200 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LocationTextBox.Focus();
                return;
            }

            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    if (IsEditMode)
                    {
                        // Обновление существующего склада
                        cmd.CommandText = @"
                            UPDATE Warehouses 
                            SET Name = @name, 
                                Location = @location
                            WHERE Id = @id";
                    }
                    else
                    {
                        // Добавление нового склада
                        cmd.CommandText = @"
                            INSERT INTO Warehouses (Name, Location)
                            VALUES (@name, @location);
                            SELECT last_insert_rowid();";
                    }

                    cmd.Parameters.AddWithValue("@name", Warehouse.Name.Trim());
                    cmd.Parameters.AddWithValue("@location", Warehouse.Location?.Trim() ?? string.Empty);

                    if (IsEditMode)
                    {
                        cmd.Parameters.AddWithValue("@id", Warehouse.Id);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var newId = cmd.ExecuteScalar();
                        Warehouse.Id = Convert.ToInt32(newId);
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
            {
                MessageBox.Show("Склад с таким названием уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NameTextBox.Focus();
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