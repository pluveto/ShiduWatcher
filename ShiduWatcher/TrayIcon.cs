using System;
using System.Runtime.InteropServices;

namespace ShiduWatcher
{
    public class TrayIcon : IDisposable
    {
        private const int WM_CREATE = 0x0001;
        private const int WM_DESTROY = 0x0002;
        private const int WM_COMMAND = 0x0111;
        private const int WM_USER = 0x0400;
        private const int WM_TRAYICON = WM_USER + 1;
        private const int ID_TRAY_EXIT = 1000;

        private IntPtr hWnd;
        private IntPtr hInstance;
        private NOTIFYICONDATA nid;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreateWindowEx(
            int dwExStyle, string lpClassName, string lpWindowName,
            long dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostQuitMessage(int nExitCode);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WNDCLASS
        {
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct NOTIFYICONDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public uint dwState;
            public uint dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public uint dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public bool Quit { get; set; } = false;
        public TrayIcon(string tooltip, IntPtr iconHandle)
        {
            WNDCLASS wc = new WNDCLASS
            {
                lpfnWndProc = WindowProc,
                hInstance = Marshal.GetHINSTANCE(typeof(TrayIcon).Module),
                lpszClassName = "ShiduWatcherClass",
                hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00), // IDI_APPLICATION
                hCursor = LoadCursor(IntPtr.Zero, 0x7F00) // IDC_ARROW
            };

            RegisterClass(ref wc);

            hWnd = CreateWindowEx(0, "ShiduWatcherClass", "Shidu Watcher", 0x80000000, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, wc.hInstance, IntPtr.Zero);

            nid = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = hWnd,
                uID = 1,
                uFlags = 0x00000002 | 0x00000001 | 0x00000004, // NIF_MESSAGE | NIF_ICON | NIF_TIP
                uCallbackMessage = WM_TRAYICON,
                hIcon = iconHandle,
                szTip = tooltip
            };
            Shell_NotifyIcon(0x00000000, ref nid); // NIM_ADD
        }

        private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_TRAYICON:
                    if (lParam.ToInt32() == 0x0205) // WM_RBUTTONUP
                    {
                        IntPtr hTrayMenu = CreatePopupMenu();
                        AppendMenu(hTrayMenu, 0x00000000, ID_TRAY_EXIT, "Exit"); // MF_STRING

                        POINT pt;
                        GetCursorPos(out pt);

                        SetForegroundWindow(hWnd);
                        TrackPopupMenu(hTrayMenu, 0x0000, pt.x, pt.y, 0, hWnd, IntPtr.Zero); // TPM_BOTTOMALIGN | TPM_LEFTALIGN
                    }
                    break;


                case WM_COMMAND:
                    if (wParam.ToInt32() == ID_TRAY_EXIT)
                    {
                        Shell_NotifyIcon(0x00000002, ref nid); // NIM_DELETE
                        PostQuitMessage(0);
                        Quit = true;
                    }
                    break;

                case WM_DESTROY:
                    Shell_NotifyIcon(0x00000002, ref nid); // NIM_DELETE
                    PostQuitMessage(0);
                    Quit = true;
                    break;

                default:
                    return DefWindowProc(hWnd, msg, wParam, lParam);
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Shell_NotifyIcon(0x00000002, ref nid); // NIM_DELETE
            DestroyWindow(hWnd);
        }
    }
}
