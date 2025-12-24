using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.Views.Main.Windows
{
    public partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
            LoadHistory();
        }

        private void LoadFilters()
        {
            EntityTypeFilter.Items.Add("Все");
            EntityTypeFilter.Items.Add("Warehouse");
            EntityTypeFilter.Items.Add("Product");
            EntityTypeFilter.Items.Add("Stock");
            EntityTypeFilter.Items.Add("Supplier");
            EntityTypeFilter.Items.Add("Movement");
            EntityTypeFilter.Items.Add("User");
            EntityTypeFilter.SelectedIndex = 0;

            DateFromPicker.SelectedDate = DateTime.Now.AddDays(-7);
            DateToPicker.SelectedDate = DateTime.Now;
        }

        private void LoadHistory(DateTime? dateFrom = null, DateTime? dateTo = null, string entityType = null)
        {
            try
            {
                var history = new List<HistoryRecord>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    string query = @"
                        SELECT 
                            h.Id,
                            h.Entity,
                            h.EntityId,
                            h.Action,
                            h.OldValue,
                            h.NewValue,
                            u.FullName as UserName,
                            h.CreatedAt
                        FROM History h
                        LEFT JOIN Users u ON h.UserId = u.Id
                        WHERE 1=1";

                    if (dateFrom != null)
                    {
                        query += " AND h.CreatedAt >= @DateFrom";
                        cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Value);
                    }

                    if (dateTo != null)
                    {
                        query += " AND h.CreatedAt <= @DateTo";
                        cmd.Parameters.AddWithValue("@DateTo", dateTo.Value.AddDays(1).AddSeconds(-1));
                    }

                    if (entityType != null && entityType != "Все")
                    {
                        query += " AND h.Entity = @Entity";
                        cmd.Parameters.AddWithValue("@Entity", entityType);
                    }

                    query += " ORDER BY h.CreatedAt DESC";

                    cmd.CommandText = query;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(new HistoryRecord
                            {
                                Id = reader.GetInt32(0),
                                Entity = reader.GetString(1),
                                EntityId = reader.GetInt32(2),
                                Action = reader.GetString(3),
                                OldValue = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                NewValue = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                UserName = reader.IsDBNull(6) ? "Система" : reader.GetString(6),
                                CreatedAt = reader.GetDateTime(7)
                            });
                        }
                    }
                }

                HistoryGrid.ItemsSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            DateTime? dateFrom = DateFromPicker.SelectedDate;
            DateTime? dateTo = DateToPicker.SelectedDate;
            string entityType = EntityTypeFilter.SelectedItem?.ToString();

            LoadHistory(dateFrom, dateTo, entityType);
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            DateFromPicker.SelectedDate = DateTime.Now.AddDays(-7);
            DateToPicker.SelectedDate = DateTime.Now;
            EntityTypeFilter.SelectedIndex = 0;
            LoadHistory();
        }
    }
}