using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Main.Windows
{
    public partial class MovementsPage : Page
    {
        public MovementsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
            LoadMovements();
        }

        private void LoadFilters()
        {

            TypeFilter.Items.Add("Все");
            TypeFilter.Items.Add("Приход (IN)");
            TypeFilter.Items.Add("Отгрузка (OUT)");
            TypeFilter.Items.Add("Перемещение (MOVE)");
            TypeFilter.SelectedIndex = 0;


            WarehouseFilter.Items.Add("Все склады");
            using (var conn = Database.Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name FROM Warehouses ORDER BY Name";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        WarehouseFilter.Items.Add(reader.GetString(1));
                    }
                }
            }
            WarehouseFilter.SelectedIndex = 0;


            DateFromPicker.SelectedDate = DateTime.Now.AddDays(-30);
            DateToPicker.SelectedDate = DateTime.Now;
        }

        private void LoadMovements(string typeFilter = null, string warehouseFilter = null,
                                 DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                var movements = new List<Movement>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    string query = @"
                        SELECT 
                            m.Id,
                            p.Name as ProductName,
                            fw.Name as FromWarehouse,
                            tw.Name as ToWarehouse,
                            s.Name as SupplierName,
                            m.Quantity,
                            m.MovementType,
                            u.FullName as UserName,
                            m.CreatedAt
                        FROM Movements m
                        LEFT JOIN Products p ON m.ProductId = p.Id
                        LEFT JOIN Warehouses fw ON m.FromWarehouseId = fw.Id
                        LEFT JOIN Warehouses tw ON m.ToWarehouseId = tw.Id
                        LEFT JOIN Suppliers s ON m.SupplierId = s.Id
                        LEFT JOIN Users u ON m.UserId = u.Id
                        WHERE 1=1";

                    if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "Все")
                    {
                        string dbType = "";
                        if (typeFilter == "Приход (IN)") dbType = "IN";
                        else if (typeFilter == "Отгрузка (OUT)") dbType = "OUT";
                        else if (typeFilter == "Перемещение (MOVE)") dbType = "MOVE";

                        query += " AND m.MovementType = @MovementType";
                        cmd.Parameters.AddWithValue("@MovementType", dbType);
                    }

                    if (!string.IsNullOrEmpty(warehouseFilter) && warehouseFilter != "Все склады")
                    {
                        query += @" AND (fw.Name = @Warehouse OR tw.Name = @Warehouse)";
                        cmd.Parameters.AddWithValue("@Warehouse", warehouseFilter);
                    }

                    if (dateFrom != null)
                    {
                        query += " AND m.CreatedAt >= @DateFrom";
                        cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Value);
                    }

                    if (dateTo != null)
                    {
                        query += " AND m.CreatedAt <= @DateTo";
                        cmd.Parameters.AddWithValue("@DateTo", dateTo.Value.AddDays(1).AddSeconds(-1));
                    }

                    query += " ORDER BY m.CreatedAt DESC";

                    cmd.CommandText = query;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var movement = new Movement
                            {
                                Id = reader.GetInt32(0),
                                ProductName = reader.IsDBNull(1) ? "Неизвестно" : reader.GetString(1),
                                FromWarehouse = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                ToWarehouse = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                SupplierName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                Quantity = reader.GetInt32(5),
                                MovementType = reader.GetString(6),
                                UserName = reader.IsDBNull(7) ? "Неизвестно" : reader.GetString(7),
                                CreatedAt = reader.GetDateTime(8)
                            };


                            switch (movement.MovementType)
                            {
                                case "IN":
                                    movement.TypeDisplay = "Приход";
                                    break;
                                case "OUT":
                                    movement.TypeDisplay = "Отгрузка";
                                    break;
                                case "MOVE":
                                    movement.TypeDisplay = "Перемещение";
                                    break;
                                default:
                                    movement.TypeDisplay = movement.MovementType;
                                    break;
                            }

                            movements.Add(movement);
                        }
                    }
                }

                MovementsGrid.ItemsSource = movements;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки движений: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMovements();
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            string type = TypeFilter.SelectedItem?.ToString();
            string warehouse = WarehouseFilter.SelectedItem?.ToString();
            DateTime? dateFrom = DateFromPicker.SelectedDate;
            DateTime? dateTo = DateToPicker.SelectedDate;

            LoadMovements(type, warehouse, dateFrom, dateTo);
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            TypeFilter.SelectedIndex = 0;
            WarehouseFilter.SelectedIndex = 0;
            DateFromPicker.SelectedDate = DateTime.Now.AddDays(-30);
            DateToPicker.SelectedDate = DateTime.Now;
            LoadMovements();
        }

        private void CreateIncoming_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddDeliveryWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Приход товара успешно оформлен",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadMovements();
            }
        }

        private void CreateOutgoing_Click(object sender, RoutedEventArgs e)
        {
            var window = new OutgoingMovementWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Отгрузка товара успешно оформлена",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadMovements();
            }
        }

        private void CreateTransfer_Click(object sender, RoutedEventArgs e)
        {
            var window = new TransferMovementWindow();
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Перемещение товара успешно оформлено",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadMovements();
            }
        }
    }
}