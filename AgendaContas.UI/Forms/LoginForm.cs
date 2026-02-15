using AgendaContas.Domain.Services;
using AgendaContas.Domain.Models;

namespace AgendaContas.UI.Forms;

public partial class LoginForm : Form
{
    private readonly AuthService _authService;
    public Usuario? AuthenticatedUser { get; private set; }

    public LoginForm(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();
        AcceptButton = btnEntrar;
    }

    private async void btnEntrar_Click(object? sender, EventArgs e)
    {
        btnEntrar.Enabled = false;

        try
        {
            var usuario = await _authService.AuthenticateAsync(txtLogin.Text, txtSenha.Text);
            if (usuario != null)
            {
                AuthenticatedUser = usuario;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show("Login ou senha inválidos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnEntrar.Enabled = true;
        }
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
