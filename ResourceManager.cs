using System.IO;
using System;

public class ResourceManager
{
    // Путь к папке с ресурсами
    private static readonly string BaseResourcesPath =
        Path.Combine(AppContext.BaseDirectory, @"..\..\..\Resources");

    public static readonly string ImagesPath = Path.Combine(BaseResourcesPath, "Images");
    public static readonly string DatabasePath = Path.Combine(BaseResourcesPath, "Database");

    static ResourceManager()
    {
        // Инициализация всех папок при первом обращении
        EnsureDirectories();
    }

    public static void EnsureDirectories()
    {
        // Создаем все необходимые папки
        var directories = new[] { ImagesPath, DatabasePath };

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }

    public static string GetDatabasePath()
    {
        return Path.Combine(DatabasePath, "warehouse.db");
    }

    public static string GetConnectionString()
    {
        string dbPath = GetDatabasePath();
        return $@"Data Source={dbPath};Version=3;";
    }

    public static string GetImagePath(string imageName)
    {
        return Path.Combine(ImagesPath, imageName);
    }
}