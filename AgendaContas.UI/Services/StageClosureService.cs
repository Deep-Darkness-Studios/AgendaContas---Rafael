using System.Text;
using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;
using AgendaContas.Domain.Services;

namespace AgendaContas.UI.Services;

public sealed class StageClosureReport
{
    public int CategoriasCriadas { get; set; }
    public int CategoriasReativadas { get; set; }
    public int BancosSincronizados { get; set; }
    public int LancamentosGerados { get; set; }
    public int ContasAtivas { get; set; }
    public string? AvisoBancos { get; set; }
    public DateTime ExecutadoEm { get; set; } = DateTime.Now;

    public string ToDisplayText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Fechamento da Etapa - Carga Inicial");
        sb.AppendLine();
        sb.AppendLine($"Executado em: {ExecutadoEm:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"Categorias criadas: {CategoriasCriadas}");
        sb.AppendLine($"Categorias reativadas: {CategoriasReativadas}");
        sb.AppendLine($"Contas ativas atuais: {ContasAtivas}");
        sb.AppendLine($"Bancos sincronizados (BCB): {BancosSincronizados}");
        sb.AppendLine($"Lançamentos recorrentes gerados (mês): {LancamentosGerados}");

        if (!string.IsNullOrWhiteSpace(AvisoBancos))
        {
            sb.AppendLine();
            sb.AppendLine("Aviso:");
            sb.AppendLine(AvisoBancos);
        }

        sb.AppendLine();
        sb.AppendLine("Status: etapa técnica pronta para preenchimento de dados operacionais.");
        return sb.ToString();
    }
}

public static class StageClosureService
{
    private static readonly string[] CategoriasPadrao =
    {
        "Moradia",
        "Transporte",
        "Alimentação",
        "Saúde",
        "Educação",
        "Lazer",
        "Assinaturas",
        "Impostos",
        "Investimentos",
        "Receitas"
    };

    public static async Task<StageClosureReport> ExecutarCargaInicialAsync(IAppRepository repo)
    {
        var report = new StageClosureReport();

        var categoriasExistentes = (await repo.GetCategoriasAsync(apenasAtivas: false)).ToList();
        var categoriasPorNome = categoriasExistentes
            .GroupBy(c => c.Nome.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var nome in CategoriasPadrao)
        {
            if (!categoriasPorNome.TryGetValue(nome, out var categoria))
            {
                await repo.AddAsync(new Categoria { Nome = nome, Ativa = true });
                report.CategoriasCriadas++;
                continue;
            }

            if (!categoria.Ativa)
            {
                categoria.Ativa = true;
                await repo.UpdateAsync(categoria);
                report.CategoriasReativadas++;
            }
        }

        var bancosCount = await repo.GetParticipantesStrCountAsync();
        if (bancosCount == 0)
        {
            try
            {
                report.BancosSincronizados = await repo.SincronizarParticipantesStrAsync();
            }
            catch (Exception ex)
            {
                report.AvisoBancos = "Não foi possível sincronizar os bancos do BCB agora: " + ex.Message;
            }
        }

        report.ContasAtivas = (await repo.GetContasAsync(apenasAtivas: true)).Count();

        var competenciaAtual = DateTime.Now.ToString("yyyy-MM");
        var antes = (await repo.GetByCompetenciaAsync(competenciaAtual)).Count();
        try
        {
            var financeiro = new FinanceiroService(repo, repo);
            await financeiro.GerarLancamentosRecorrentesAsync();
        }
        catch (InvalidOperationException)
        {
            // Competência fechada: mantém relatório sem quebrar.
        }

        var depois = (await repo.GetByCompetenciaAsync(competenciaAtual)).Count();
        report.LancamentosGerados = Math.Max(0, depois - antes);

        return report;
    }
}
