namespace AgendaContas.Domain.Models;

public class LancamentoFiltro
{
    public string? Competencia { get; set; }
    public string? Status { get; set; }
    public int? CategoriaId { get; set; }
    public string? TextoBusca { get; set; }
    public bool IncluirContasInativas { get; set; }
}
