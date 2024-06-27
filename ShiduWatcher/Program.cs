using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Drawing;
using ShiduWatcher.ShiduWatcher;
using ShiduWatcher.Types;
using Microsoft.Win32;
using System.Diagnostics;

namespace ShiduWatcher
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        private static bool powerSuspend = false;
        private static void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                Debug.WriteLine("system power suspend");
                powerSuspend = true;
            }
            else if (e.Mode == PowerModes.Resume)
            {
                Debug.WriteLine("system power resume");
                powerSuspend = false;
            }
        }
        static async Task Main(string[] args)
        {
            // 隐藏控制台窗口
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            var verbose = true;
            var databasePersister = new DatabasePersister();
            var usageService = new ProgramUsageService(databasePersister, 1000, verbose);
            new UserInactivityDetector(threshold: TimeSpan.FromMinutes(10), checkInterval: TimeSpan.FromSeconds(1),
                userActive: (s, e) => usageService.Resume(),
                userInactive: (s, e) => usageService.Pause()
            ).Start();

            var port = NetworkHelper.FindAvailablePort(1893, 1976);
            Console.WriteLine($"Starting ShiduWatcher on port {port}...");
            var host = RestfulAPIHost.CreateHostBuilder(args, port, usageService).Build();

            var webHostTask = RestfulAPIHost.CreateHostBuilder(args, port, usageService).Build().RunAsync();

            var mainLoopTask = Task.Run(async () =>
            {
                ProgramUsage? currentUsage = null;
                while (true)
                {
                    if (powerSuspend)
                    {
                        continue;
                    }

                    if (!usageService.IsPaused())
                    {
                        var newUsage = ForegroundWindowHelper.GetForegroundProgramUsage();
                        if (newUsage == null)
                        {
                            continue;
                        }

                        if (currentUsage == null)
                        {
                            currentUsage = newUsage;
                        }

                        if (currentUsage != null && currentUsage.ProcessName != newUsage.ProcessName)
                        {
                            currentUsage.Duration = DateTime.Now - currentUsage.StartTime;
                            await usageService.AddUsage(currentUsage);
                            currentUsage = newUsage;
                        }
                    }

                    await Task.Delay(usageService.GetInterval());
                }
            });

            // 使用托盘图标类
            using (TrayIcon trayIcon = new TrayIcon("ShiduWatcher", SystemIcons.Application.Handle))
            {
                // 消息循环
                while (!trayIcon.Quit)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
    }
}
