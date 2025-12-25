using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace GOZON
{
    public partial class RegWindow : Window
    {
        public RegWindow()
        {
            InitializeComponent();

            Loaded += (s, e) => FullNameTextBox.Focus();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть ссылку: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;


            if (!ValidateRequiredFields(fullName, login, password, confirmPassword))
                return;


            if (!ValidateDataFormat(fullName, login, password))
                return;


            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Некорректный формат email адреса.\n" +
                        "Пример правильного email: user@example.com",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus();
                    EmailTextBox.SelectAll();
                    return;
                }
            }

            string passwordHash = PasswordHelper.Hash(password);

            using (var conn = Database.Open())
            using (var cmd = conn.CreateCommand())
            {

                cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                cmd.Parameters.AddWithValue("@login", login);

                long exists = (long)cmd.ExecuteScalar();
                if (exists > 0)
                {
                    MessageBox.Show("Логин уже занят. Пожалуйста, выберите другой логин.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoginTextBox.Focus();
                    LoginTextBox.SelectAll();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @email";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@email", email);

                    exists = (long)cmd.ExecuteScalar();
                    if (exists > 0)
                    {
                        MessageBox.Show("Email уже зарегистрирован. Используйте другой email или оставьте поле пустым.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        EmailTextBox.Focus();
                        EmailTextBox.SelectAll();
                        return;
                    }
                }


                cmd.CommandText = @"
                    INSERT INTO Users (Login, PasswordHash, FullName, Email, CreatedAt)
                    VALUES (@login, @passwordHash, @fullName, @email, datetime('now'))
                ";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd.Parameters.AddWithValue("@fullName", fullName);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(email) ? DBNull.Value : (object)email);

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Пользователь зарегистрирован успешно!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateRequiredFields(string fullName, string login, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Поле 'ФИО' обязательно для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Поле 'Логин' обязательно для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Поле 'Пароль' обязательно для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Поле 'Подтверждение пароля' обязательно для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return false;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают. Пожалуйста, введите пароль еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Clear();
                ConfirmPasswordBox.Clear();
                PasswordBox.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateDataFormat(string fullName, string login, string password)
        {
            if (!fullName.Contains(' ') || fullName.Length < 5)
            {
                MessageBox.Show("ФИО должно содержать имя и фамилию (минимум 5 символов с пробелом).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                FullNameTextBox.SelectAll();
                return false;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9_]{4,20}$"))
            {
                MessageBox.Show("Логин должен содержать от 4 до 20 символов (буквы, цифры и подчеркивание).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus();
                LoginTextBox.SelectAll();
                return false;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {

                var pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";


                if (!Regex.IsMatch(email, pattern))
                    return false;


                if (email.Length > 254)
                    return false;


                int atIndex = email.IndexOf('@');
                if (atIndex <= 0 || atIndex == email.Length - 1)
                    return false;

                string localPart = email.Substring(0, atIndex);
                string domainPart = email.Substring(atIndex + 1);


                if (localPart.Length > 64)
                    return false;


                if (localPart.StartsWith(".") || localPart.EndsWith("."))
                    return false;


                if (localPart.Contains(".."))
                    return false;


                if (domainPart.Length > 253)
                    return false;


                if (!domainPart.Contains("."))
                    return false;


                if (domainPart.StartsWith(".") || domainPart.EndsWith("."))
                    return false;


                if (domainPart.Contains(".."))
                    return false;


                string tld = domainPart.Substring(domainPart.LastIndexOf('.') + 1);
                if (tld.Length < 2)
                    return false;


                if (!Regex.IsMatch(domainPart, @"^[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$"))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EmailTextBox.Text.Length > 100)
            {
                EmailTextBox.Text = EmailTextBox.Text.Substring(0, 100);
                EmailTextBox.CaretIndex = 100;
            }


            string email = EmailTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(email))

                bool isValid = IsValidEmail(email);
                EmailTextBox.BorderBrush = isValid ?
                    System.Windows.Media.Brushes.Green :
                    System.Windows.Media.Brushes.OrangeRed;
            }
            else
            {

                EmailTextBox.ClearValue(BorderBrushProperty);
            }
        }

        private void FullNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FullNameTextBox.Text.Length > 100)
            {
                FullNameTextBox.Text = FullNameTextBox.Text.Substring(0, 100);
                FullNameTextBox.CaretIndex = 100;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                MoveToNextControl(sender as UIElement);
            }
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                MoveToNextControl(sender as UIElement);
            }
        }

        private void MoveToNextControl(UIElement currentControl)
        {
            var request = new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next);
            currentControl.MoveFocus(request);
        }
    }
}