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
            GenerateMovementsReport();
        }

        private void GenerateMovementsReport()
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка генерации отчёта: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GenerateMovementsReport();
        }
    }
}