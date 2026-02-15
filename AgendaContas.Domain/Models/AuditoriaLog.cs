namespace AgendaContas.Domain.Models;

public class AuditoriaLog
{
    public int Id { get; set; }
    public DateTime DataHoraUtc { get; set; }
    public int? UsuarioId { get; set; }
    public string UsuarioLogin { get; set; } = "sistema";
    public string Acao { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public int? EntidadeId { get; set; }
    public string? Detalhes { get; set; }
}
