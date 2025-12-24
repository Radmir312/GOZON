using GOZON.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GOZON.MainView
{
    public partial class SettingsPage : Page
    {
        private Dictionary<int, string> originalValues = new Dictionary<int, string>();

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var settings = new List<Settings>();

                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            Id,
                            Category,
                            Name,
                            Value,
                            Description
                        FROM Settings
                        ORDER BY Category, Name";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var setting = new Settings
                            {
                                Id = reader.GetInt32(0),
                                Category = reader.GetString(1),
                                Name = reader.GetString(2),
                                Value = reader.GetString(3),
                                Description = reader.GetString(4)
                            };

                            settings.Add(setting);
                            originalValues[setting.Id] = setting.Value;
                        }
                    }
                }

                SettingsGrid.ItemsSource = settings;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки настроек: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void SettingsGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header.ToString() == "Значение")
            {
                var setting = e.Row.Item as Settings;
                if (setting != null)
                {
                    originalValues[setting.Id] = setting.Value;
                }
            }
        }

        private void SettingsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Column.Header.ToString() == "Значение")
            {
                var setting = e.Row.Item as Settings;
                if (setting != null)
                {
                    // Можно добавить валидацию значения здесь
                    SaveSetting(setting);
                }
            }
        }

        private void SaveSetting(Settings setting)
        {
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Settings 
                        SET Value = @Value
                        WHERE Id = @Id";

                    cmd.Parameters.AddWithValue("@Value", setting.Value);
                    cmd.Parameters.AddWithValue("@Id", setting.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        originalValues[setting.Id] = setting.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настройки '{setting.Name}': " + ex.Message);
            }
        }

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Settings setting in SettingsGrid.Items)
                {
                    SaveSetting(setting);
                }

                MessageBox.Show("Все настройки сохранены");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения настроек: " + ex.Message);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Сбросить все изменения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                LoadSettings();
            }
        }
    }
}