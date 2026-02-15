namespace AgendaContas.UI.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblLogin;
    private Label lblSenha;
    private TextBox txtLogin;
    private TextBox txtSenha;
    private Button btnEntrar;
    private Button btnCancelar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblLogin = new Label();
        lblSenha = new Label();
        txtLogin = new TextBox();
        txtSenha = new TextBox();
        btnEntrar = new Button();
        btnCancelar = new Button();
        SuspendLayout();
        // 
        // lblLogin
        // 
        lblLogin.AutoSize = true;
        lblLogin.Location = new Point(28, 22);
        lblLogin.Name = "lblLogin";
        lblLogin.Size = new Size(37, 15);
        lblLogin.TabIndex = 0;
        lblLogin.Text = "Login";
        // 
        // lblSenha
        // 
        lblSenha.AutoSize = true;
        lblSenha.Location = new Point(28, 69);
        lblSenha.Name = "lblSenha";
        lblSenha.Size = new Size(39, 15);
        lblSenha.TabIndex = 1;
        lblSenha.Text = "Senha";
        // 
        // txtLogin
        // 
        txtLogin.Location = new Point(28, 40);
        txtLogin.Name = "txtLogin";
        txtLogin.Size = new Size(275, 23);
        txtLogin.TabIndex = 2;
        txtLogin.Text = "admin";
        // 
        // txtSenha
        // 
        txtSenha.Location = new Point(28, 87);
        txtSenha.Name = "txtSenha";
        txtSenha.PasswordChar = '*';
        txtSenha.Size = new Size(275, 23);
        txtSenha.TabIndex = 3;
        txtSenha.Text = "admin123";
        // 
        // btnEntrar
        // 
        btnEntrar.Location = new Point(147, 128);
        btnEntrar.Name = "btnEntrar";
        btnEntrar.Size = new Size(75, 30);
        btnEntrar.TabIndex = 4;
        btnEntrar.Text = "Entrar";
        btnEntrar.UseVisualStyleBackColor = true;
        btnEntrar.Click += btnEntrar_Click;
        // 
        // btnCancelar
        // 
        btnCancelar.DialogResult = DialogResult.Cancel;
        btnCancelar.Location = new Point(228, 128);
        btnCancelar.Name = "btnCancelar";
        btnCancelar.Size = new Size(75, 30);
        btnCancelar.TabIndex = 5;
        btnCancelar.Text = "Cancelar";
        btnCancelar.UseVisualStyleBackColor = true;
        btnCancelar.Click += btnCancelar_Click;
        // 
        // LoginForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancelar;
        ClientSize = new Size(332, 178);
        Controls.Add(btnCancelar);
        Controls.Add(btnEntrar);
        Controls.Add(txtSenha);
        Controls.Add(txtLogin);
        Controls.Add(lblSenha);
        Controls.Add(lblLogin);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DDS - Cofre Real - Login";
        ResumeLayout(false);
        PerformLayout();
    }
}
