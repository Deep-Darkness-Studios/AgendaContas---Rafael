using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;

namespace AgendaContas.UI.Forms;

public class ContaForm : Form
{
    private readonly IAppRepository _repo;
    private readonly Conta? _contaAtual;

    private readonly TextBox _txtNome = new();
    private readonly ComboBox _cmbCategoria = new();
    private readonly ComboBox _cmbBanco = new();
    private readonly NumericUpDown _numDiaVencimento = new();
    private readonly NumericUpDown _numValor = new();
    private readonly ComboBox _cmbFormaPagamento = new();
    private readonly CheckBox _chkRecorrente = new();
    private readonly CheckBox _chkAtiva = new();
    private readonly Button _btnSalvar = new();
    private readonly Button _btnCancelar = new();

    private List<Categoria> _categorias = new();
    private List<BancoOption> _bancos = new();

    public Conta? ContaResult { get; private set; }

    public ContaForm(IAppRepository repo, Conta? conta = null)
    {
        _repo = repo;
        _contaAtual = conta;
        BuildLayout();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadCategoriasAsync();
        await LoadBancosAsync();
        LoadValues();
    }

    private void BuildLayout()
    {
        Text = _contaAtual == null ? "Nova Conta" : "Editar Conta";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 345);

        var lblNome = new Label { Text = "Nome", Left = 20, Top = 20, Width = 120 };
        _txtNome.Left = 20;
        _txtNome.Top = 40;
        _txtNome.Width = 480;

        var lblCategoria = new Label { Text = "Categoria", Left = 20, Top = 72, Width = 120 };
        _cmbCategoria.Left = 20;
        _cmbCategoria.Top = 92;
        _cmbCategoria.Width = 230;
        _cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblBanco = new Label { Text = "Banco (BCB)", Left = 270, Top = 72, Width = 120 };
        _cmbBanco.Left = 270;
        _cmbBanco.Top = 92;
        _cmbBanco.Width = 230;
        _cmbBanco.DropDownStyle = ComboBoxStyle.DropDown;
        _cmbBanco.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _cmbBanco.AutoCompleteSource = AutoCompleteSource.ListItems;

        var lblDia = new Label { Text = "Dia Vencimento", Left = 20, Top = 132, Width = 120 };
        _numDiaVencimento.Left = 20;
        _numDiaVencimento.Top = 152;
        _numDiaVencimento.Width = 120;
        _numDiaVencimento.Minimum = 1;
        _numDiaVencimento.Maximum = 31;
        _numDiaVencimento.Value = 10;

        var lblValor = new Label { Text = "Valor Padrão", Left = 160, Top = 132, Width = 120 };
        _numValor.Left = 160;
        _numValor.Top = 152;
        _numValor.Width = 120;
        _numValor.DecimalPlaces = 2;
        _numValor.Maximum = 1_000_000;
        _numValor.Value = 100;

        var lblForma = new Label { Text = "Forma Pagamento", Left = 300, Top = 132, Width = 120 };
        _cmbFormaPagamento.Left = 300;
        _cmbFormaPagamento.Top = 152;
        _cmbFormaPagamento.Width = 200;
        _cmbFormaPagamento.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbFormaPagamento.Items.AddRange(new object[] { "Pix", "Boleto", "Cartão", "Débito", "Dinheiro" });
        _cmbFormaPagamento.SelectedIndex = 0;

        _chkRecorrente.Text = "Recorrente";
        _chkRecorrente.Left = 20;
        _chkRecorrente.Top = 194;
        _chkRecorrente.Checked = true;

        _chkAtiva.Text = "Ativa";
        _chkAtiva.Left = 120;
        _chkAtiva.Top = 194;
        _chkAtiva.Checked = true;

        _btnSalvar.Text = "Salvar";
        _btnSalvar.Left = 344;
        _btnSalvar.Top = 286;
        _btnSalvar.Width = 75;
        _btnSalvar.Click += btnSalvar_Click;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Left = 425;
        _btnCancelar.Top = 286;
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
        Controls.Add(lblCategoria);
        Controls.Add(_cmbCategoria);
        Controls.Add(lblBanco);
        Controls.Add(_cmbBanco);
        Controls.Add(lblDia);
        Controls.Add(_numDiaVencimento);
        Controls.Add(lblValor);
        Controls.Add(_numValor);
        Controls.Add(lblForma);
        Controls.Add(_cmbFormaPagamento);
        Controls.Add(_chkRecorrente);
        Controls.Add(_chkAtiva);
        Controls.Add(_btnSalvar);
        Controls.Add(_btnCancelar);
    }

    private async Task LoadCategoriasAsync()
    {
        _categorias = (await _repo.GetCategoriasAsync(apenasAtivas: true)).ToList();
        _cmbCategoria.DataSource = _categorias;
        _cmbCategoria.DisplayMember = nameof(Categoria.Nome);
        _cmbCategoria.ValueMember = nameof(Categoria.Id);
    }

    private async Task LoadBancosAsync()
    {
        var participantes = (await _repo.GetParticipantesStrAsync(somenteCompe: false)).ToList();
        _bancos = new List<BancoOption>
        {
            new()
            {
                Ispb = string.Empty,
                NumeroCodigo = string.Empty,
                Nome = "(Não informar)"
            }
        };

        _bancos.AddRange(participantes.Select(p => new BancoOption
        {
            Ispb = p.Ispb,
            NumeroCodigo = p.NumeroCodigo,
            Nome = p.NomeReduzido
        }));

        _cmbBanco.DataSource = _bancos;
        _cmbBanco.DisplayMember = nameof(BancoOption.Display);
        _cmbBanco.ValueMember = nameof(BancoOption.Ispb);
        _cmbBanco.SelectedIndex = 0;
    }

    private void LoadValues()
    {
        if (_contaAtual == null)
        {
            return;
        }

        _txtNome.Text = _contaAtual.Nome;
        _numDiaVencimento.Value = Math.Clamp(_contaAtual.DiaVencimento, 1, 31);
        _numValor.Value = _contaAtual.ValorPadrao;
        _chkRecorrente.Checked = _contaAtual.Recorrente;
        _chkAtiva.Checked = _contaAtual.Ativa;

        if (!string.IsNullOrWhiteSpace(_contaAtual.FormaPagamentoPadrao))
        {
            _cmbFormaPagamento.SelectedItem = _contaAtual.FormaPagamentoPadrao;
        }

        if (_contaAtual.CategoriaId > 0)
        {
            _cmbCategoria.SelectedValue = _contaAtual.CategoriaId;
        }

        if (!string.IsNullOrWhiteSpace(_contaAtual.BancoIspb))
        {
            _cmbBanco.SelectedValue = _contaAtual.BancoIspb;
        }
    }

    private void btnSalvar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNome.Text))
        {
            MessageBox.Show("Informe o nome da conta.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbCategoria.SelectedValue is not int categoriaId)
        {
            MessageBox.Show("Selecione uma categoria.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var bancoSelecionado = _cmbBanco.SelectedItem as BancoOption;
        var bancoIspb = bancoSelecionado?.Ispb;
        var bancoCodigo = bancoSelecionado?.NumeroCodigo;
        if (string.IsNullOrWhiteSpace(bancoIspb))
        {
            bancoIspb = null;
            bancoCodigo = null;
        }

        ContaResult = new Conta
        {
            Id = _contaAtual?.Id ?? 0,
            Nome = _txtNome.Text.Trim(),
            CategoriaId = categoriaId,
            BancoIspb = bancoIspb,
            BancoCodigo = bancoCodigo,
            BancoNome = bancoSelecionado?.Nome,
            DiaVencimento = (int)_numDiaVencimento.Value,
            ValorPadrao = _numValor.Value,
            Recorrente = _chkRecorrente.Checked,
            Ativa = _chkAtiva.Checked,
            FormaPagamentoPadrao = _cmbFormaPagamento.SelectedItem?.ToString() ?? "Pix"
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private class BancoOption
    {
        public string Ispb { get; set; } = string.Empty;
        public string NumeroCodigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;

        public string Display
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Ispb))
                {
                    return Nome;
                }

                var codigo = string.IsNullOrWhiteSpace(NumeroCodigo) ? "---" : NumeroCodigo;
                return $"{codigo} - {Nome} [{Ispb}]";
            }
        }
    }
}
