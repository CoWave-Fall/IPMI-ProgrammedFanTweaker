using System;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage;
using 程控静扇.Helpers; // Added for StartupHelper

namespace 程控静扇.Services
{
    public enum AppTheme
    {
        System,
        Light,
        Dark
    }

    public static class SettingsService
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public static event EventHandler<AppTheme>? ThemeChanged;

        public static AppTheme Theme
        {
            get => (AppTheme)LoadSetting("Theme", (int)AppTheme.System);
            set
            {
                SaveSetting("Theme", (int)value);
                ThemeChanged?.Invoke(null, value);
            }
        }

        public static bool MinimizeToTrayOnClose
        {
            get => LoadSetting("MinimizeToTrayOnClose", false);
            set => SaveSetting("MinimizeToTrayOnClose", value);
        }

        public static bool RunOnStartup
        {
            get => LoadSetting("RunOnStartup", false);
            set
            {
                SaveSetting("RunOnStartup", value);
                StartupHelper.SetRunOnStartup(value);
            }
        }

        public static void SaveSetting(string key, object value)
        {
            LocalSettings.Values[key] = value;
        }

        public static T? LoadSetting<T>(string key, T? defaultValue = default)
        {
            if (LocalSettings.Values.TryGetValue(key, out object? value))
            {
                return (T)value!;
            }
            return defaultValue;
        }

        public static void SavePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                SaveSetting("EncryptedPassword", "");
                return;
            }
            var data = Encoding.Unicode.GetBytes(password);
            var protectedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            SaveSetting("EncryptedPassword", Convert.ToBase64String(protectedData));
        }

        public static string LoadPassword()
        {
            var protectedBase64 = LoadSetting<string>("EncryptedPassword");
            if (string.IsNullOrEmpty(protectedBase64))
            {
                return string.Empty;
            }
            try
            {
                var protectedData = Convert.FromBase64String(protectedBase64);
                var data = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void SaveIpmiStatus(IpmiOperationStatus status)
        {
            SaveSetting("IpmiOperationStatus", (int)status);
        }

        public static IpmiOperationStatus LoadIpmiStatus()
        {
            return (IpmiOperationStatus)LoadSetting("IpmiOperationStatus", (int)IpmiOperationStatus.Default);
        }

        public static void SaveTemperatureSource(TemperatureSource source)
        {
            SaveSetting("TemperatureSource", (int)source);
        }

        public static TemperatureSource LoadTemperatureSource()
        {
            return (TemperatureSource)LoadSetting("TemperatureSource", (int)TemperatureSource.CoreTemp);
        }
    }
}