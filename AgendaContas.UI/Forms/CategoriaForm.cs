using AgendaContas.Domain.Models;

namespace AgendaContas.UI.Forms;

public class CategoriaForm : Form
{
    private readonly TextBox _txtNome = new();
    private readonly CheckBox _chkAtiva = new();
    private readonly Button _btnSalvar = new();
    private readonly Button _btnCancelar = new();
    private readonly Categoria? _categoriaAtual;

    public Categoria? CategoriaResult { get; private set; }

    public CategoriaForm(Categoria? categoria = null)
    {
        _categoriaAtual = categoria;
        BuildLayout();
        LoadValues();
    }

    private void BuildLayout()
    {
        Text = _categoriaAtual == null ? "Nova Categoria" : "Editar Categoria";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(360, 165);

        var lblNome = new Label
        {
            Text = "Nome",
            Left = 20,
            Top = 20,
            Width = 80
        };

        _txtNome.Left = 20;
        _txtNome.Top = 42;
        _txtNome.Width = 320;

        _chkAtiva.Text = "Ativa";
        _chkAtiva.Left = 20;
        _chkAtiva.Top = 77;
        _chkAtiva.Checked = true;

        _btnSalvar.Text = "Salvar";
        _btnSalvar.Left = 184;
        _btnSalvar.Top = 112;
        _btnSalvar.Width = 75;
        _btnSalvar.Click += btnSalvar_Click;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Left = 265;
        _btnCancelar.Top = 112;
        _btnCancelar.Width = 75;
        _btnCancelar.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        AcceptButton = _btnSalvar;
        CancelButton = _btnCancelar;

        Controls.Add(lblNome);
        Controls.Add(_txtNome);
        Controls.Add(_chkAtiva);
        Controls.Add(_btnSalvar);
        Controls.Add(_btnCancelar);
    }

    private void LoadValues()
    {
        if (_categoriaAtual == null)
        {
            return;
        }

        _txtNome.Text = _categoriaAtual.Nome;
        _chkAtiva.Checked = _categoriaAtual.Ativa;
    }

    private void btnSalvar_Click(object? sender, EventArgs e)
    {
        var nome = _txtNome.Text.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            MessageBox.Show("Informe o nome da categoria.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CategoriaResult = new Categoria
        {
            Id = _categoriaAtual?.Id ?? 0,
            Nome = nome,
            Ativa = _chkAtiva.Checked
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
