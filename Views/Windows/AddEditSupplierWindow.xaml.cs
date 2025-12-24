using GOZON.Models;
using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;

namespace GOZON.Views.Main.Windows
{
    public partial class AddEditSupplierWindow : Window
    {
        public Supplier Supplier { get; private set; }
        private bool IsEditMode => Supplier.Id > 0;

        public AddEditSupplierWindow(Supplier supplier = null)
        {
            InitializeComponent();

            Supplier = supplier ?? new Supplier
            {
                Name = "",
                ContactPerson = "",
                Phone = "",
                Email = "",
                Address = ""
            };

            DataContext = Supplier;
            Title = IsEditMode ? "Редактировать поставщика" : "Добавить поставщика";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Supplier.Name))
            {
                MessageBox.Show("Введите название компании", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (Supplier.Name.Length > 100)
            {
                MessageBox.Show("Название компании не должно превышать 100 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Проверка email, если указан
            if (!string.IsNullOrWhiteSpace(Supplier.Email))
            {
                try
                {
                    var email = new System.Net.Mail.MailAddress(Supplier.Email);
                }
                catch
                {
                    MessageBox.Show("Введите корректный email адрес", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus();
                    return;
                }
            }

            // Проверка телефона, если указан
            if (!string.IsNullOrWhiteSpace(Supplier.Phone))
            {
                // Простая проверка на минимальную длину телефона
                if (Supplier.Phone.Length < 5)
                {
                    MessageBox.Show("Номер телефона слишком короткий", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PhoneTextBox.Focus();
                    return;
                }
            }

            // Проверка длины других полей
            if (Supplier.ContactPerson?.Length > 100)
            {
                MessageBox.Show("Имя контактного лица не должно превышать 100 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ContactPersonTextBox.Focus();
                return;
            }

            if (Supplier.Address?.Length > 200)
            {
                MessageBox.Show("Адрес не должен превышать 200 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AddressTextBox.Focus();
                return;
            }

            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    if (IsEditMode)
                    {
                        // Обновление существующего поставщика
                        cmd.CommandText = @"
                            UPDATE Suppliers 
                            SET Name = @name, 
                                ContactPerson = @contactPerson,
                                Phone = @phone,
                                Email = @email,
                                Address = @address
                            WHERE Id = @id";
                    }
                    else
                    {
                        // Добавление нового поставщика
                        cmd.CommandText = @"
                            INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address)
                            VALUES (@name, @contactPerson, @phone, @email, @address);
                            SELECT last_insert_rowid();";
                    }

                    cmd.Parameters.AddWithValue("@name", Supplier.Name.Trim());
                    cmd.Parameters.AddWithValue("@contactPerson", Supplier.ContactPerson?.Trim() ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", Supplier.Phone?.Trim() ?? string.Empty);
                    cmd.Parameters.AddWithValue("@email", Supplier.Email?.Trim() ?? string.Empty);
                    cmd.Parameters.AddWithValue("@address", Supplier.Address?.Trim() ?? string.Empty);

                    if (IsEditMode)
                    {
                        cmd.Parameters.AddWithValue("@id", Supplier.Id);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var newId = cmd.ExecuteScalar();
                        Supplier.Id = Convert.ToInt32(newId);
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
            {
                MessageBox.Show("Поставщик с таким названием уже существует", "Ошибка",
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