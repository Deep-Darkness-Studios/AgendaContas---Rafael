using AgendaContas.UI.Forms;

namespace AgendaContas.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        using (var login = new LoginForm())
        {
            if (login.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainForm());
            }
        }
    }
}
