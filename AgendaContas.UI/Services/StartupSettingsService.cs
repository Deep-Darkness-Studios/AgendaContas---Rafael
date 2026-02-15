using System.Text.Json;

namespace AgendaContas.UI.Services;

public static class SplashModes
{
    public const string Rapido = "Rapido";
    public const string Padrao = "Padrao";
    public const string Apresentacao = "Apresentacao";

    public static readonly string[] All =
    {
        Rapido,
        Padrao,
        Apresentacao
    };
}

public sealed class StartupSettings
{
    public string SplashMode { get; set; } = SplashModes.Apresentacao;
    public int? SplashDurationSeconds { get; set; }
}

public static class StartupSettingsService
{
    private const string SettingsFileName = "startup-settings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static StartupSettings Load()
    {
        var path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return new StartupSettings();
        }

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<StartupSettings>(json, JsonOptions);
            return Normalize(loaded);
        }
        catch
        {
            return new StartupSettings();
        }
    }

    public static void Save(StartupSettings settings)
    {
        var normalized = Normalize(settings);
        var path = GetSettingsPath();
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static string GetSettingsPath()
    {
        return Path.Combine(AppPaths.GetAppDataDirectory(), SettingsFileName);
    }

    public static StartupSettings Normalize(StartupSettings? settings)
    {
        var safe = settings ?? new StartupSettings();
        var mode = safe.SplashMode?.Trim();
        if (!SplashModes.All.Contains(mode, StringComparer.OrdinalIgnoreCase))
        {
            mode = SplashModes.Apresentacao;
        }

        var duration = safe.SplashDurationSeconds;
        if (duration.HasValue)
        {
            duration = Math.Clamp(duration.Value, 3, 120);
        }

        return new StartupSettings
        {
            SplashMode = mode!,
            SplashDurationSeconds = duration
        };
    }
}
