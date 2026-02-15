using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace AgendaContas.UI.Services;

// Minimal helper to register an AppUserModelID by creating a Start Menu shortcut
// Based on Microsoft samples. Works on Windows Desktop.
internal static class DesktopNotificationManagerCompat
{
    public static bool RegisterAumidAndComServer(string aumid)
    {
        try
        {
            var shortcutPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs",
                "AgendaContas.lnk");

            if (!File.Exists(shortcutPath))
            {
                // Create the shortcut
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;

                var shellLink = (IShellLink)new CShellLink();
                shellLink.SetPath(exePath);
                shellLink.SetArguments("");

                var propertyStore = (IPropertyStore)shellLink;

                using (var appIdProp = new PropVariant(aumid))
                {
                    var PKEY_AppUserModel_ID = new PROPERTYKEY { fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), pid = 5 };
                    propertyStore.SetValue(ref PKEY_AppUserModel_ID, appIdProp);
                    propertyStore.Commit();
                }

                var persistFile = (IPersistFile)shellLink;
                persistFile.Save(shortcutPath, true);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static object CreateToastNotifier(string aUMID)
    {
        throw new NotImplementedException();
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    [ClassInterface(ClassInterfaceType.None)]
    private class CShellLink { }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAW pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    private interface IPropertyStore
    {
        void GetCount(out uint cProps);
        void GetAt(uint iProp, out PROPERTYKEY pkey);
        void GetValue(ref PROPERTYKEY key, out PropVariant pv);
        void SetValue(ref PROPERTYKEY key, [In] PropVariant pv);
        void Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    // Minimal PropVariant implementation for string
    [StructLayout(LayoutKind.Explicit)]
    private sealed class PropVariant : IDisposable
    {
        [FieldOffset(0)]
        private ushort vt;
        [FieldOffset(8)]
        private IntPtr pointer;

        public PropVariant(string value)
        {
            vt = 31; // VT_LPWSTR
            pointer = Marshal.StringToCoTaskMemUni(value);
        }

        public void Dispose()
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pointer);
                pointer = IntPtr.Zero;
            }
        }
    }
}
