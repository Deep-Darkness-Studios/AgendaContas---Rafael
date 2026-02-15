using AgendaContas.Domain.Services;
using Xunit;

namespace AgendaContas.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_GeraHashesDiferentesParaMesmoTexto()
    {
        var first = PasswordHasher.HashPassword("admin123");
        var second = PasswordHasher.HashPassword("admin123");

        Assert.NotEqual(first.Hash, second.Hash);
        Assert.NotEqual(first.Salt, second.Salt);
        Assert.Equal(PasswordHasher.DefaultIterations, first.Iterations);
        Assert.Equal(PasswordHasher.DefaultIterations, second.Iterations);
    }

    [Fact]
    public void VerifyPassword_RetornaTrueParaSenhaCorreta_EFalseParaIncorreta()
    {
        var generated = PasswordHasher.HashPassword("admin123");

        Assert.True(PasswordHasher.VerifyPassword("admin123", generated.Hash, generated.Salt, generated.Iterations));
        Assert.False(PasswordHasher.VerifyPassword("outra", generated.Hash, generated.Salt, generated.Iterations));
    }
}
