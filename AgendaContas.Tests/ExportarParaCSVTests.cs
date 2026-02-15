using System.IO;
using System.Linq;
using AgendaContas.Domain.Models;
using AgendaContas.Domain.Services;
using Xunit;

namespace AgendaContas.Tests;

public class ExportarParaCSVTests
{
    [Fact]
    public void ExportarParaCSV_CreatesFile_WithExpectedContent()
    {
        var lancamentos = new[]
        {
            new Lancamento { NomeConta = "Conta A", Vencimento = new System.DateTime(2024,1,15), Valor = 100m, Status = "Pendente", DataPagamento = null, Observacao = "" },
            new Lancamento { NomeConta = "Conta B", Vencimento = new System.DateTime(2024,1,20), Valor = 50.5m, Status = "Pago", DataPagamento = new System.DateTime(2024,1,20), Observacao = "Pago" }
        };

        var tmp = Path.GetTempFileName();
        File.Delete(tmp);
        var path = tmp + ".csv";

        try
        {
            UtilService.ExportarParaCSV(lancamentos, path);

            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            Assert.Equal("Conta;Vencimento;Valor;Status;Pagamento;Observacao", lines[0]);
            Assert.Contains("Conta A", lines[1]);
            Assert.Contains("Conta B", lines[2]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
