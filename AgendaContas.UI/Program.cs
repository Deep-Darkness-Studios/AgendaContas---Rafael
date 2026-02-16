using AgendaContas.Data.Repositories;
using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Services;
using AgendaContas.UI.Forms;
using AgendaContas.UI.Services;
using System.Diagnostics;

namespace AgendaContas.UI;

static class Program
{
    private const string AppDisplayName = "DDS - Cofre Real";

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var startupSettings = StartupSettingsService.Load();

        StartupResult startupResult;
        try
        {
            startupResult = RunStartupWithSplash(startupSettings);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Falha ao inicializar o aplicativo:\n" + ex.Message,
                AppDisplayName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        using (var login = new LoginForm(startupResult.AuthService))
        {
            if (login.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainForm(startupResult.Repository, login.AuthenticatedUser));
            }
        }
    }

    private static StartupResult RunStartupWithSplash(StartupSettings startupSettings)
    {
        var splashProfile = ResolveSplashProfile(startupSettings);
        using var splash = new SplashForm(AppDisplayName, splashProfile.FadeInDurationMs, splashProfile.FadeOutDurationMs);
        splash.Show();
        Application.DoEvents();

        var minDuration = TimeSpan.FromSeconds(splashProfile.DurationSeconds);
        var displayWatch = Stopwatch.StartNew();

        var progress = new Progress<StartupProgress>(info =>
            splash.ReportProgress(info.Percent, info.Message));

        var initTask = Task.Run(() => InitializeApplication(progress));

        while (true)
        {
            if (initTask.IsFaulted || initTask.IsCanceled)
            {
                break;
            }

            var elapsed = displayWatch.Elapsed;
            if (initTask.IsCompletedSuccessfully)
            {
                var remaining = minDuration - elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    splash.ReportProgress(100, "Abrindo DDS - Cofre Real...");
                    break;
                }

                var remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
                splash.ReportProgress(
                    Math.Max(96, splash.CurrentProgress),
                    $"Carregamento concluído ({splashProfile.Mode}). Iniciando em {remainingSeconds}s...");
            }

            Application.DoEvents();
            Thread.Sleep(80);
        }

        Application.DoEvents();

        if (initTask.IsFaulted)
        {
            splash.FadeOutAndClose();
            throw initTask.Exception?.GetBaseException()
                  ?? new InvalidOperationException("Falha inesperada na inicialização.");
        }

        if (initTask.IsCanceled)
        {
            splash.FadeOutAndClose();
            throw new InvalidOperationException("Inicialização cancelada.");
        }

        splash.ReportProgress(100, "Abrindo DDS - Cofre Real...");
        splash.FadeOutAndClose();
        return initTask.GetAwaiter().GetResult();
    }

    private static StartupResult InitializeApplication(IProgress<StartupProgress> progress)
    {
        progress.Report(new StartupProgress(5, "Iniciando DDS - Cofre Real..."));

        var appDataPath = AppPaths.GetAppDataDirectory();
        progress.Report(new StartupProgress(15, $"Preparando ambiente local em {appDataPath}..."));

        var connectionString = AppPaths.GetConnectionString();
        progress.Report(new StartupProgress(32, "Inicializando banco e migrações..."));

        var repository = new AppRepository(connectionString);

        progress.Report(new StartupProgress(55, "Validando segurança e usuário padrão..."));
        _ = repository.GetByLoginAsync("admin").GetAwaiter().GetResult();

        progress.Report(new StartupProgress(72, "Carregando categorias e contas..."));
        var categoriasAtivas = repository.GetCategoriasAsync(apenasAtivas: true).GetAwaiter().GetResult().Count();
        var contasAtivas = repository.GetContasAsync(apenasAtivas: true).GetAwaiter().GetResult().Count();

        progress.Report(new StartupProgress(85, "Lendo integração bancária BCB..."));
        var bancos = repository.GetParticipantesStrCountAsync().GetAwaiter().GetResult();

        progress.Report(new StartupProgress(
            93,
            $"Dados prontos: {categoriasAtivas} categorias, {contasAtivas} contas, {bancos} bancos."));

        var authService = new AuthService(repository);
        progress.Report(new StartupProgress(96, "Preparando autenticação..."));

        return new StartupResult(repository, authService);
    }

    private static SplashProfile ResolveSplashProfile(StartupSettings settings)
    {
        var normalized = StartupSettingsService.Normalize(settings);
        var mode = normalized.SplashMode;
        var defaultDuration = 30;
        var fadeInMs = 900;
        var fadeOutMs = 700;

        if (string.Equals(mode, SplashModes.Rapido, StringComparison.OrdinalIgnoreCase))
        {
            mode = SplashModes.Rapido;
            defaultDuration = 5;
            fadeInMs = 280;
            fadeOutMs = 220;
        }
        else if (string.Equals(mode, SplashModes.Padrao, StringComparison.OrdinalIgnoreCase))
        {
            mode = SplashModes.Padrao;
            defaultDuration = 12;
            fadeInMs = 600;
            fadeOutMs = 480;
        }
        else
        {
            mode = SplashModes.Apresentacao;
        }

        var duration = normalized.SplashDurationSeconds ?? defaultDuration;
        duration = Math.Clamp(duration, 3, 120);

        return new SplashProfile(mode, duration, fadeInMs, fadeOutMs);
    }

    private sealed record StartupProgress(int Percent, string Message);
    private sealed record StartupResult(IAppRepository Repository, AuthService AuthService);
    private sealed record SplashProfile(string Mode, int DurationSeconds, int FadeInDurationMs, int FadeOutDurationMs);
}
