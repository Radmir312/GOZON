using System;

namespace GOZON
{
    public static class SessionManager
    {
        // Статические свойства для хранения данных текущего пользователя
        public static int CurrentUserId { get; private set; }
        public static string CurrentUserLogin { get; private set; }
        public static string CurrentUserFullName { get; private set; }
        public static string CurrentUserRole { get; private set; }

        // Флаг, показывающий авторизован ли пользователь
        public static bool IsLoggedIn { get; private set; }

        // Инициализация сессии после успешного входа
        public static void InitializeSession(int userId, string login, string fullName, string role)
        {
            CurrentUserId = userId;
            CurrentUserLogin = login;
            CurrentUserFullName = fullName;
            CurrentUserRole = role;
            IsLoggedIn = true;
        }

        // Завершение сессии (выход)
        public static void Logout()
        {
            CurrentUserId = 0;
            CurrentUserLogin = string.Empty;
            CurrentUserFullName = string.Empty;
            CurrentUserRole = string.Empty;
            IsLoggedIn = false;
        }

        // Проверка прав пользователя (если нужно)
        public static bool HasRole(string role)
        {
            return IsLoggedIn && CurrentUserRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAdmin()
        {
            return HasRole("Admin") || HasRole("Administrator");
        }
    }
}