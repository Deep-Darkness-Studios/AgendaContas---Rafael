using AgendaContas.Domain.Models;
using System.Text;

namespace AgendaContas.Domain.Services;

public class UtilService
{
    public static void ExportarParaCSV(IEnumerable<Lancamento> lancamentos, string filePath)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Conta;Vencimento;Valor;Status;Pagamento;Observacao");

        foreach (var l in lancamentos)
        {
            csv.AppendLine($"{l.NomeConta};{l.Vencimento:dd/MM/yyyy};{l.Valor:F2};{l.Status};{l.DataPagamento:dd/MM/yyyy};{l.Observacao}");
        }

        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }

    public static void EnviarNotificacaoToast(string titulo, string mensagem)
    {
        // Notificar por toast é específico da UI/Windows; manter silencioso no Domain.
        try { /* noop: manter compatibilidade em contextos sem suporte a notificações */ }
        catch { }
    }
}
