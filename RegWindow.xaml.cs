using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

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
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;
            string fullName = FullNameTextBox.Text;
            string email = EmailTextBox.Text;

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName)) /*||
                string.IsNullOrWhiteSpace(email));*/
            {
                MessageBox.Show("Заполни все поля, инвалид");
                return;
            }

            using (var connection = Database.Open())
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Users (Login, Password, FullName, Email)
                    VALUES (@login, @password, @fullName, @email);
                ";

                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@fullName", fullName);
                cmd.Parameters.AddWithValue("@email", email);

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Пользователь зарегистрирован, аккаунт удалить нельзя, мы будем закидывать вас спам расылками каждый час, живи теперь с этим");
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
