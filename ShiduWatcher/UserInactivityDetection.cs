using System;
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
        private int inactivityThreshold;
        public event EventHandler UserInactive;

        public UserInactivityDetector(int thresholdInMilliseconds, EventHandler userInactive, int checkIntervalInMilliseconds = 10000)
        {
            inactivityThreshold = thresholdInMilliseconds;
            UserInactive += userInactive;
            inactivityTimer = new Timer(checkIntervalInMilliseconds);
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
            if (GetIdleTime() > inactivityThreshold)
            {
                UserInactive?.Invoke(this, EventArgs.Empty);
            }
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
