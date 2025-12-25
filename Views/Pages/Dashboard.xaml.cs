using GOZON.Views;
using GOZON.Views.Main.Windows;
using GOZON.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace GOZON
{

    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
            InitializeUserInfo();
            NavigateToPage(new ProductsPage());
        }

        private void InitializeUserInfo()
        {
            // Отображаем ФИО и логин пользователя
            UserFullNameLabel.Text = SessionManager.CurrentUserFullName;
            UserLoginLabel.Text = $"Логин: {SessionManager.CurrentUserLogin}";
        }

        private void NavigateToPage(Page page)
        {
            MainFrame.Navigate(page);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Логируем выход
            HistoryLogger.Log("User", SessionManager.CurrentUserId, "LOGOUT",
                $"Пользователь {SessionManager.CurrentUserLogin} вышел из системы");

            SessionManager.Logout();

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            switch (button.Content.ToString())
            {
                case "Товары":
                    NavigateToPage(new ProductsPage());
                    break;
                case "Поставки":
                    NavigateToPage(new DeliveriesPage());
                    break;
                case "Поставщики":
                    NavigateToPage(new SuppliersPage());
                    break;
                case "Склады":
                    NavigateToPage(new WarehousesPage());
                    break;
                case "Движения":
                    NavigateToPage(new MovementsPage());
                    break;
                case "Отчёты":
                    NavigateToPage(new ReportsPage());
                    break;
            }
        }
    }
}