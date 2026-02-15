using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;

namespace AgendaContas.Domain.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Usuario?> AuthenticateAsync(string login, string senha)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
        {
            return null;
        }

        var usuario = await _usuarioRepository.GetByLoginAsync(login.Trim());
        if (usuario == null)
        {
            return null;
        }

        return PasswordHasher.VerifyPassword(senha, usuario.SenhaHash, usuario.SenhaSalt, usuario.Iteracoes)
            ? usuario
            : null;
    }
}
