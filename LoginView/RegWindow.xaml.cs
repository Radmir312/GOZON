using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON
{
    public partial class RegWindow : Window
    {
        public RegWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполни все поля!");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            string passwordHash = PasswordHelper.Hash(password);

            using (var conn = Database.Open())
            using (var cmd = conn.CreateCommand())
            {
                // проверка логина
                cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                cmd.Parameters.AddWithValue("@login", login);

                long exists = (long)cmd.ExecuteScalar();
                if (exists > 0)
                {
                    MessageBox.Show("Логин уже занят");
                    return;
                }

                // регистрация
                cmd.CommandText = @"
                    INSERT INTO Users (Login, PasswordHash, FullName, Email)
                    VALUES (@login, @passwordHash, @fullName, @email)
                ";

                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd.Parameters.AddWithValue("@fullName", fullName);
                cmd.Parameters.AddWithValue("@email", email);

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Пользователь зарегистрирован успешно!");
                    Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Ошибка БД: " + ex.Message);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
