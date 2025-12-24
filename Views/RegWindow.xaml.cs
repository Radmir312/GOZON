using System;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GOZON
{
    public partial class RegWindow : Window
    {
        public RegWindow()
        {
            InitializeComponent();
            // Устанавливаем фокус на первое поле при открытии окна
            Loaded += (s, e) => FullNameTextBox.Focus();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Собираем данные из полей
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Валидация обязательных полей
            if (!ValidateRequiredFields(fullName, login, password, confirmPassword))
                return;

            // Валидация формата данных
            if (!ValidateDataFormat(fullName, login, password))
                return;

            // Валидация email (если указан)
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
                // проверка уникальности логина
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

                // проверка уникальности email (если указан)
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

                // регистрация
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
                // Более точная, но не слишком сложная проверка email
                var pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

                // Проверяем общую структуру
                if (!Regex.IsMatch(email, pattern))
                    return false;

                // Проверяем длину (максимум 254 символа по стандарту)
                if (email.Length > 254)
                    return false;

                // Проверяем, что есть @ и она не первая и не последняя
                int atIndex = email.IndexOf('@');
                if (atIndex <= 0 || atIndex == email.Length - 1)
                    return false;

                string localPart = email.Substring(0, atIndex);
                string domainPart = email.Substring(atIndex + 1);

                // Проверяем локальную часть
                if (localPart.Length > 64) // RFC ограничение
                    return false;

                // Не может начинаться или заканчиваться точкой
                if (localPart.StartsWith(".") || localPart.EndsWith("."))
                    return false;

                // Не может содержать две точки подряд
                if (localPart.Contains(".."))
                    return false;

                // Проверяем доменную часть
                if (domainPart.Length > 253)
                    return false;

                // Должна содержать точку
                if (!domainPart.Contains("."))
                    return false;

                // Не может начинаться или заканчиваться точкой
                if (domainPart.StartsWith(".") || domainPart.EndsWith("."))
                    return false;

                // Не может содержать две точки подряд
                if (domainPart.Contains(".."))
                    return false;

                // Верхнеуровневый домен должен быть минимум 2 символа
                string tld = domainPart.Substring(domainPart.LastIndexOf('.') + 1);
                if (tld.Length < 2)
                    return false;

                // Проверяем допустимые символы в домене
                if (!Regex.IsMatch(domainPart, @"^[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$"))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Дополнительная валидация в реальном времени для email
        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EmailTextBox.Text.Length > 100)
            {
                EmailTextBox.Text = EmailTextBox.Text.Substring(0, 100);
                EmailTextBox.CaretIndex = 100;
            }

            // Опционально: можно добавить цветовую индикацию валидности
            string email = EmailTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                // Можно менять цвет рамки или фона в зависимости от валидности
                // Это улучшает UX
                bool isValid = IsValidEmail(email);
                EmailTextBox.BorderBrush = isValid ?
                    System.Windows.Media.Brushes.Green :
                    System.Windows.Media.Brushes.OrangeRed;
            }
            else
            {
                // Сброс цвета, если поле пустое
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