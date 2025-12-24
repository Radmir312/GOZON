using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.MainView
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

        private void AddSuppliers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address)
                        VALUES ('Новый поставщик', '', '', '', '')";

                    cmd.ExecuteNonQuery();
                }

                LoadSuppliers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления поставщика: " + ex.Message);
            }
        }
    }
}
