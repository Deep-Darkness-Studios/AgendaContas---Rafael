using System;
using System.IO;
using System.Windows.Forms;
using AgendaContas.Data.Repositories;
using AgendaContas.Domain.Services;

namespace AgendaContas.UI
{
    internal static class ProgramMain
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Database file next to the executable for easy local runs
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agenda.db");
            var connStr = $"Data Source={dbPath}";

            // Initialize repository and domain services
            var repo = new AppRepository(connStr);
            var financeiro = new FinanceiroService(repo, repo);

            // Ensure recurring entries for current month are generated (best-effort)
            try
            {
                financeiro.GerarLancamentosRecorrentesAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore errors during startup generation to avoid blocking UI
            }

            Application.Run(new Forms.InfoForm());
        }
    }
}
