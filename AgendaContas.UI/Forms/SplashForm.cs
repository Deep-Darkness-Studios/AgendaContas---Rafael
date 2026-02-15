using AgendaContas.UI.Properties;

namespace AgendaContas.UI.Forms;

public sealed class SplashForm : Form
{
    private const int FadeSteps = 30;

    private readonly PictureBox _background = new();
    private readonly Panel _bottomPanel = new();
    private readonly Label _lblAppName = new();
    private readonly Label _lblTagline = new();
    private readonly Label _lblStatus = new();
    private readonly Label _lblPercent = new();
    private readonly ProgressBar _progressBar = new();
    private readonly int _fadeInDurationMs;
    private readonly int _fadeOutDurationMs;

    public int CurrentProgress => _progressBar.Value;

    public SplashForm(string appName, int fadeInDurationMs, int fadeOutDurationMs)
    {
        _fadeInDurationMs = Math.Clamp(fadeInDurationMs, 100, 3000);
        _fadeOutDurationMs = Math.Clamp(fadeOutDurationMs, 100, 3000);
        BuildLayout(appName);
        Opacity = 0;
    }

    public void ReportProgress(int percent, string status)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ReportProgress(percent, status)));
            return;
        }

        var safePercent = Math.Clamp(percent, 0, 100);
        if (safePercent < _progressBar.Value)
        {
            safePercent = _progressBar.Value;
        }

        _progressBar.Value = safePercent;
        _lblPercent.Text = $"{safePercent}%";
        _lblStatus.Text = string.IsNullOrWhiteSpace(status) ? "Inicializando..." : status;
        _lblStatus.Refresh();
        _progressBar.Refresh();
    }

    public void FadeOutAndClose()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(FadeOutAndClose));
            return;
        }

        if (IsDisposed)
        {
            return;
        }

        AnimateOpacity(from: Math.Max(0, Opacity), to: 0, _fadeOutDurationMs);
        Close();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AnimateOpacity(from: 0, to: 1, _fadeInDurationMs);
    }

    private void BuildLayout(string appName)
    {
        Text = appName;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(1004, 561);
        BackColor = Color.Black;
        DoubleBuffered = true;

        _background.Dock = DockStyle.Fill;
        _background.SizeMode = PictureBoxSizeMode.StretchImage;
        _background.BackColor = Color.Black;
        try
        {
            _background.Image = Resources.fb_page_cover_dds;
        }
        catch
        {
            _background.Image = null;
        }

        _bottomPanel.Dock = DockStyle.Bottom;
        _bottomPanel.Height = 130;
        _bottomPanel.BackColor = Color.FromArgb(12, 24, 30);

        _lblAppName.AutoSize = true;
        _lblAppName.Left = 20;
        _lblAppName.Top = 14;
        _lblAppName.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
        _lblAppName.ForeColor = Color.Gold;
        _lblAppName.Text = appName;

        _lblTagline.AutoSize = true;
        _lblTagline.Left = 22;
        _lblTagline.Top = 49;
        _lblTagline.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        _lblTagline.ForeColor = Color.FromArgb(180, 220, 230);
        _lblTagline.Text = "Inicializando ambiente seguro e financeiro...";

        _lblStatus.AutoSize = true;
        _lblStatus.Left = 22;
        _lblStatus.Top = 77;
        _lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _lblStatus.ForeColor = Color.White;
        _lblStatus.Text = "Preparando aplicação...";

        _lblPercent.Width = 80;
        _lblPercent.Height = 20;
        _lblPercent.Left = ClientSize.Width - 95;
        _lblPercent.Top = 82;
        _lblPercent.TextAlign = ContentAlignment.MiddleRight;
        _lblPercent.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        _lblPercent.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        _lblPercent.ForeColor = Color.Gold;
        _lblPercent.Text = "0%";

        _progressBar.Left = 20;
        _progressBar.Top = 101;
        _progressBar.Width = ClientSize.Width - 40;
        _progressBar.Height = 18;
        _progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _progressBar.Style = ProgressBarStyle.Continuous;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 100;
        _progressBar.Value = 0;

        _bottomPanel.Controls.Add(_lblAppName);
        _bottomPanel.Controls.Add(_lblTagline);
        _bottomPanel.Controls.Add(_lblStatus);
        _bottomPanel.Controls.Add(_lblPercent);
        _bottomPanel.Controls.Add(_progressBar);

        Controls.Add(_background);
        Controls.Add(_bottomPanel);
        _bottomPanel.BringToFront();
    }

    private void AnimateOpacity(double from, double to, int durationMs)
    {
        var start = Math.Clamp(from, 0, 1);
        var end = Math.Clamp(to, 0, 1);
        var steps = Math.Max(2, FadeSteps);
        var delay = Math.Max(8, durationMs / steps);

        for (var step = 0; step <= steps; step++)
        {
            var ratio = step / (double)steps;
            var eased = EaseInOutCubic(ratio);
            Opacity = start + ((end - start) * eased);
            Application.DoEvents();
            Thread.Sleep(delay);
        }

        Opacity = end;
    }

    private static double EaseInOutCubic(double t)
    {
        if (t < 0.5d)
        {
            return 4d * t * t * t;
        }

        return 1d - (Math.Pow(-2d * t + 2d, 3d) / 2d);
    }
}
