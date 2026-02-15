namespace AgendaContas.UI.Services;

public static class AppPaths
{
    public static string GetAppDataDirectory()
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AgendaContas");

        Directory.CreateDirectory(baseDir);
        return baseDir;
    }

    public static string GetDatabasePath()
    {
        var baseDir = GetAppDataDirectory();

        var targetPath = Path.Combine(baseDir, "agenda.db");
        TryMigrateLegacyDatabase(targetPath);
        return targetPath;
    }

    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }

    public static string GetAttachmentsDirectory()
    {
        var attachmentsDir = Path.Combine(GetAppDataDirectory(), "Anexos");
        Directory.CreateDirectory(attachmentsDir);
        return attachmentsDir;
    }

    private static void TryMigrateLegacyDatabase(string targetPath)
    {
        if (File.Exists(targetPath))
        {
            return;
        }

        var candidates = new[]
        {
            Path.Combine(Environment.CurrentDirectory, "agenda.db"),
            Path.Combine(AppContext.BaseDirectory, "agenda.db"),
            Path.Combine(Directory.GetCurrentDirectory(), "agenda.db")
        };

        var legacyPath = candidates
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(File.Exists);

        if (string.IsNullOrWhiteSpace(legacyPath) ||
            string.Equals(legacyPath, Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            File.Copy(legacyPath, targetPath, overwrite: false);
        }
        catch
        {
            // Se não for possível copiar, o app cria um banco novo no destino padrão.
        }
    }
}
