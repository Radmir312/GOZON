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
                cmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Login = @login AND PasswordHash = @passwordHash
                ";

                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

                long count = (long)cmd.ExecuteScalar();

                if (count == 1)
                {
                    Dashboard dashboard = new Dashboard();
                    dashboard.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
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
