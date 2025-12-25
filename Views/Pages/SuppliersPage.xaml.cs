using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Main.Windows
{
    public partial class SuppliersPage : Page
    {
        public SuppliersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
            UpdateButtonsState();
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = new List<Supplier>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            Id,
                            Name,
                            ContactPerson,
                            Phone,
                            Email,
                            Address
                        FROM Suppliers
                        ORDER BY Name";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(new Supplier
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ContactPerson = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                Address = reader.IsDBNull(5) ? "" : reader.GetString(5)
                            });
                        }
                    }
                }

                SuppliersGrid.ItemsSource = suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки поставщиков: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
        }

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditSupplierWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Поставщик успешно добавлен",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
            }
        }

        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersGrid.SelectedItem is Supplier selectedSupplier)
            {
                var window = new AddEditSupplierWindow(selectedSupplier);
                if (window.ShowDialog() == true)
                {
                    MessageBox.Show("Поставщик успешно обновлен",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSuppliers();
                }
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersGrid.SelectedItem is Supplier selectedSupplier)
            {

                bool hasRelatedData = false;
                string message = "";

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {

                    cmd.CommandText = "SELECT COUNT(*) FROM Movements WHERE SupplierId = @id";
                    cmd.Parameters.AddWithValue("@id", selectedSupplier.Id);

                    var movementsCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (movementsCount > 0)
                    {
                        hasRelatedData = true;
                        message = "Невозможно удалить поставщика, так как с ним связаны поставки товаров. " +
                                "Удалите сначала все связанные поставки.";
                    }
                }

                if (hasRelatedData)
                {
                    MessageBox.Show(message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить поставщика '{selectedSupplier.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = Database.Open())
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "DELETE FROM Suppliers WHERE Id = @id";
                            cmd.Parameters.AddWithValue("@id", selectedSupplier.Id);
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Поставщик успешно удален",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadSuppliers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SuppliersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool isSelected = SuppliersGrid.SelectedItem != null;
            EditButton.IsEnabled = isSelected;
            DeleteButton.IsEnabled = isSelected;
        }
    }
}