namespace AgendaContas.Domain.Models;

public class ParticipanteStr
{
    public string Ispb { get; set; } = string.Empty;
    public string NomeReduzido { get; set; } = string.Empty;
    public string NumeroCodigo { get; set; } = string.Empty;
    public bool ParticipaDaCompe { get; set; }
    public string AcessoPrincipal { get; set; } = string.Empty;
    public string NomeExtenso { get; set; } = string.Empty;
    public DateTime? InicioOperacao { get; set; }
    public DateTime AtualizadoEmUtc { get; set; }
}
