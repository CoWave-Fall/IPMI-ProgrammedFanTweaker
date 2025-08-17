using Microsoft.Win32;
using System.Reflection;

namespace 程控静扇.Helpers
{
    public static class StartupHelper
    {
        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "程控静扇"; // Replace with your application's name

        public static bool IsAppSetToRunOnStartup()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, false))
            {
                return key != null && key.GetValue(AppName) != null;
            }
        }

        public static void SetRunOnStartup(bool enable)
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (key == null)
                {
                    // Handle error: Unable to open registry key
                    return;
                }

                if (enable)
                {
                    string executablePath = Assembly.GetExecutingAssembly().Location;
                    key.SetValue(AppName, executablePath);
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
    }
}
