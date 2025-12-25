using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Main.Windows
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateStockReport();
        }

        private void GenerateStockReport()
        {
            try
            {
                var reportData = new List<StockItem>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            s.Id,
                            p.Name as ProductName,
                            w.Name as WarehouseName,
                            s.Quantity,
                            p.SKU
                        FROM Stock s
                        JOIN Products p ON s.ProductId = p.Id
                        JOIN Warehouses w ON s.WarehouseId = w.Id
                        ORDER BY w.Name, p.Name";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reportData.Add(new StockItem
                            {
                                Id = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                WarehouseName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                SKU = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }

                ReportsGrid.ItemsSource = reportData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка генерации отчёта: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GenerateStockReport();
        }

        private void GenerateMovementsReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var movements = new List<Movement>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            m.Id,
                            p.Name as ProductName,
                            fw.Name as FromWarehouse,
                            tw.Name as ToWarehouse,
                            m.Quantity,
                            m.MovementType,
                            u.FullName as UserName,
                            m.CreatedAt
                        FROM Movements m
                        LEFT JOIN Products p ON m.ProductId = p.Id
                        LEFT JOIN Warehouses fw ON m.FromWarehouseId = fw.Id
                        LEFT JOIN Warehouses tw ON m.ToWarehouseId = tw.Id
                        LEFT JOIN Users u ON m.UserId = u.Id
                        WHERE m.CreatedAt >= date('now', '-30 days')
                        ORDER BY m.CreatedAt DESC";

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
                                Quantity = reader.GetInt32(4),
                                MovementType = reader.GetString(5),
                                UserName = reader.IsDBNull(6) ? "Неизвестно" : reader.GetString(6),
                                CreatedAt = reader.GetDateTime(7)
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

                ReportsGrid.ItemsSource = movements;
                MessageBox.Show("Отчёт по движениям за 30 дней сгенерирован");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка генерации отчёта: " + ex.Message);
            }
        }

        private void GenerateLowStockReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lowStock = new List<StockItem>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            s.Id,
                            p.Name as ProductName,
                            w.Name as WarehouseName,
                            s.Quantity,
                            p.SKU,
                            p.MinQuantity
                        FROM Stock s
                        JOIN Products p ON s.ProductId = p.Id
                        JOIN Warehouses w ON s.WarehouseId = w.Id
                        WHERE s.Quantity <= p.MinQuantity
                        ORDER BY s.Quantity ASC";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lowStock.Add(new StockItem
                            {
                                Id = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                WarehouseName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                SKU = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }

                ReportsGrid.ItemsSource = lowStock;
                MessageBox.Show($"Найдено {lowStock.Count} позиций с низкими остатками");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка генерации отчёта: " + ex.Message);
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт в Excel будет реализован позже");
        }
    }
}