using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ShiduWatcher
{
    public class UserInactivityDetector
    {
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private Timer inactivityTimer;
        private int inactivityThreshold; // in milliseconds
        public event EventHandler UserInactive;
        public event EventHandler UserActive;

        public bool lastActive { get; private set; } = true;

        public UserInactivityDetector(TimeSpan threshold, TimeSpan checkInterval, EventHandler userInactive, EventHandler userActive)
        {
            inactivityThreshold = (int)threshold.TotalMilliseconds;
            UserInactive += userInactive;
            UserActive += userActive;
            inactivityTimer = new Timer(checkInterval);
            inactivityTimer.Elapsed += CheckInactivity;
        }

        public void Start()
        {
            inactivityTimer.Start();
        }

        public void Stop()
        {
            inactivityTimer.Stop();
        }

        private void CheckInactivity(object? sender, ElapsedEventArgs e)
        {
            var idleTime = GetIdleTime();
            Debug.WriteLine("idleTime: " + idleTime.ToString() + " threshold: " + inactivityThreshold);

            var active = idleTime <= inactivityThreshold;
            if (!active && lastActive)
            {
                UserInactive?.Invoke(this, EventArgs.Empty);
            }
            else if (active && !lastActive)
            {
                UserActive?.Invoke(this, EventArgs.Empty);
            }
            lastActive = active;
        }

        private uint GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
                return idleTime;
            }
            else
            {
                Console.WriteLine("Error getting last input info.");
                return 0;
            }
        }
    }
}
