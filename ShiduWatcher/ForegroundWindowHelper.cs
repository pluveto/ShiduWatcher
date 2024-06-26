using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ShiduWatcher.Types;

namespace ShiduWatcher
{
    internal static class ForegroundWindowHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static ProgramUsage? GetForegroundProgramUsage()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return null;

            GetWindowThreadProcessId(hWnd, out uint processId);

            try
            {
                Process process = Process.GetProcessById((int)processId);
                string executablePath = process.MainModule?.FileName ?? "";
                return new ProgramUsage(process.ProcessName, executablePath, DateTime.Now, TimeSpan.Zero);
            }
            catch
            {
                return null;
            }
        }
    }
}
