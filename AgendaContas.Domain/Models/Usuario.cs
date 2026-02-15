namespace AgendaContas.Domain.Models;

public static class PerfisUsuario
{
    public const string Admin = "Admin";
    public const string Operador = "Operador";
}

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string SenhaSalt { get; set; } = string.Empty;
    public int Iteracoes { get; set; }
    public string Perfil { get; set; } = PerfisUsuario.Operador;

    public bool IsAdmin =>
        string.Equals(Perfil, PerfisUsuario.Admin, StringComparison.OrdinalIgnoreCase);
}
