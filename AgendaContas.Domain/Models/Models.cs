namespace AgendaContas.Domain.Models;

public class Conta
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public int DiaVencimento { get; set; }
    public decimal ValorPadrao { get; set; }
    public bool Recorrente { get; set; }
    public bool Ativa { get; set; } = true;
    public string FormaPagamentoPadrao { get; set; } = "Pix";
}

public class Lancamento
{
    public int Id { get; set; }
    public int ContaId { get; set; }
    public string Competencia { get; set; } = string.Empty; // YYYY-MM
    public DateTime Vencimento { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = "Pendente"; // Pendente, Pago, Atrasado
    public DateTime? DataPagamento { get; set; }
    public string? FormaPagamento { get; set; }
    public string? Observacao { get; set; }
    public string? AnexoPath { get; set; }
    public string? NomeConta { get; set; }
}

// Usuario class moved to its own file to avoid duplicate definitions
