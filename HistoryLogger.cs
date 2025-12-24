using System;
using System.Data.SQLite;

namespace GOZON
{
    public static class HistoryLogger
    {
        public static void Log(string entity, int entityId, string action, string description)
        {
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO History (Entity, EntityId, Action, NewValue, UserId, CreatedAt)
                        VALUES (@entity, @entityId, @action, @description, @userId, CURRENT_TIMESTAMP)";

                    cmd.Parameters.AddWithValue("@entity", entity);
                    cmd.Parameters.AddWithValue("@entityId", entityId);
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@userId", SessionManager.CurrentUserId);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // В реальном приложении здесь должно быть нормальное логирование ошибок
                Console.WriteLine($"Ошибка при логировании в историю: {ex.Message}");
            }
        }

        public static void Log(string entity, int entityId, string action, string oldValue, string newValue)
        {
            try
            {
                using (var conn = Database.Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO History (Entity, EntityId, Action, OldValue, NewValue, UserId, CreatedAt)
                        VALUES (@entity, @entityId, @action, @oldValue, @newValue, @userId, CURRENT_TIMESTAMP)";

                    cmd.Parameters.AddWithValue("@entity", entity);
                    cmd.Parameters.AddWithValue("@entityId", entityId);
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@oldValue", oldValue ?? string.Empty);
                    cmd.Parameters.AddWithValue("@newValue", newValue ?? string.Empty);
                    cmd.Parameters.AddWithValue("@userId", SessionManager.CurrentUserId);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при логировании в историю: {ex.Message}");
            }
        }
    }
}