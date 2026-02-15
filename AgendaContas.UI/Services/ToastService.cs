using System;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace AgendaContas.UI.Services;

public static class ToastService
{
    private const string AUMID = "AgendaContas.Desktop";

    static ToastService()
    {
        // Ensure AUMID registration; result intentionally ignored
        try
        {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer(AUMID);
        }
        catch
        {
            // Silencioso se não suportado no ambiente atual
        }
    }

    public static void ShowToast(string title, string message)
    {
        try
        {
            // Build the toast content and create a ToastNotification manually
            var content = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .GetToastContent();

            // ToastContent does not have GetXml(); use GetContent() (XML string) and load into XmlDocument
            var xml = new XmlDocument();
            xml.LoadXml(content.GetContent());
            var toast = new ToastNotification(xml);

            // Show the toast using the desktop notifier
            var notifier = DesktopNotificationManagerCompat.CreateToastNotifier(AUMID) as ToastNotifier;
            if (notifier != null)
            {
                notifier.Show(toast);
            }
        }
        catch
        {
            // Silencioso se não suportado no ambiente atual
        }
    }
}
