namespace AgendaContas.UI.Forms;

public partial class LoginForm : Form
{
    private TextBox txtLogin = null!;
    private TextBox txtSenha = null!;
    private Button btnEntrar = null!;

    public LoginForm()
    {
        InitializeComponent(GetBtnEntrar1());
    }

    private Button GetBtnEntrar1()
    {
        return btnEntrar;
    }

    private void InitializeComponent(Button btnEntrar1)
    {
        this.Text = "Login - Agenda Financeira";
        this.Size = new Size(300, 200);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblLogin = new Label { Text = "Login:", Location = new Point(20, 20), Width = 50 };
        txtLogin = new TextBox { Location = new Point(80, 20), Width = 180, Text = "admin" };

        var lblSenha = new Label { Text = "Senha:", Location = new Point(20, 60), Width = 50 };
        txtSenha = new TextBox { Location = new Point(80, 60), Width = 180, PasswordChar = '*', Text = "admin123" };

        btnEntrar = new Button { Text = "Entrar", Location = new Point(80, 110), Width = 180, Height = 30 };
        btnEntrar1.Click += (s, e) =>
        {
            if (txtLogin.Text == "admin" && txtSenha.Text == "admin123")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Login ou senha inválidos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblSenha);
        this.Controls.Add(txtSenha);
        this.Controls.Add(btnEntrar);
    }
}
