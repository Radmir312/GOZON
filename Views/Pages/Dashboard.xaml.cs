using GOZON.Views.Main.Windows;
using GOZON.Views.Pages;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace GOZON
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
            MainFrame.Navigate(new ProductsPage()); // стартовая страница
            Manager.MainFrame = MainFrame;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                switch (btn.Content.ToString())
                {
                    case "Товары":
                        MainFrame.Navigate(new ProductsPage());
                        break;
                    case "Склады":
                        MainFrame.Navigate(new WarehousesPage());
                        break;
                    case "Поставки":
                        MainFrame.Navigate(new DeliveriesPage()); 
                        break;
                    case "Поставщики":
                        MainFrame.Navigate(new SuppliersPage()); 
                        break;
                    case "Действия":
                        MainFrame.Navigate(new MovementsPage());
                        break;
                    case "Отчёты":
                        MainFrame.Navigate(new ReportsPage());
                        break;
                    case "История":
                        MainFrame.Navigate(new HistoryPage());
                        break;
                    case "Настройки":
                        MainFrame.Navigate(new SettingsPage());
                        break;
                    default:
                        MessageBox.Show("Страница не найдена");
                        break;
                }
            }
        }
    }
}