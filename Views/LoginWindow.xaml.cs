using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GOZON.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполни поля логина и пароля.");
                return;
            }

            string passwordHash = PasswordHelper.Hash(password);

            using (var conn = Database.Open())
            using (var cmd = conn.CreateCommand())
            {
                // ИЗМЕНЕНИЕ: Запрашиваем ВСЕ данные пользователя, а не только COUNT
                cmd.CommandText = @"
                    SELECT Id, Login, FullName, Role, PasswordHash
                    FROM Users
                    WHERE Login = @login AND PasswordHash = @passwordHash
                ";

                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // ВАЖНО: Инициализируем сессию с данными пользователя!
                        int userId = Convert.ToInt32(reader["Id"]);
                        string userLogin = reader["Login"].ToString();
                        string fullName = reader["FullName"].ToString();
                        string role = reader["Role"].ToString();

                        SessionManager.InitializeSession(userId, userLogin, fullName, role);

                        // Покажем отладочное сообщение
                        MessageBox.Show($"Вход выполнен!\nЛогин: {userLogin}\nФИО: {fullName}", "Успех");

                        Dashboard dashboard = new Dashboard();
                        dashboard.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль.");
                    }
                }
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            new RegWindow().Show();
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Image_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate_1(object sender, RequestNavigateEventArgs e)
        {

        }
    }
}
