using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization;
using 程控静扇.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace 程控静扇
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static Window? _window;

        public static Window? MainWindow => _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            this.UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        

        LiveCharts.Configure(config =>
            {
                config
                    .AddSkiaSharp()
                    .AddDefaultMappers();

                // Safely attempt to set a global typeface for Chinese characters to avoid NullReferenceException
                // if a font is not found on the system.
                var typeface = SKFontManager.Default.MatchCharacter('汉');
                if (typeface != null)
                {
                    config.HasGlobalSKTypeface(typeface);
                }
            });


            var savedLang = SettingsService.LoadSetting<string>("AppLanguage");
            if (!string.IsNullOrEmpty(savedLang))
            {
                ApplicationLanguages.PrimaryLanguageOverride = savedLang;
            }
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Mark the exception as handled
            e.Handled = true;

            // Log the exception
            var exception = e.Exception;
            var errorMessage = $"Unhandled exception: {exception}";
            LogService.Instance.AppendLog(errorMessage);

            // Also log to a file for persistence
            await LogToFileAsync(errorMessage);

            // Show a dialog to the user
            await ShowErrorDialogAsync(errorMessage);

            LogException(e.Exception);
            e.Handled = true; // 表示异常已经被处理，防止程序立即退出（但程序可能已处于不稳定状态）
        }

        private async Task LogToFileAsync(string message)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile logFile = await localFolder.CreateFileAsync("crash_log.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(logFile, $"{DateTime.Now}: {message}\n\n");
            }
            catch (Exception ex)
            {
                // If logging to file fails, we can't do much, but we can log it to the in-memory logger
                LogService.Instance.AppendLog($"Failed to log to file: {ex.Message}");
            }
        }

        private async Task ShowErrorDialogAsync(string message)
        {
            if (_window?.Content.XamlRoot != null)
            {
                var dialog = new ContentDialog
                {
                    Title = "应用程序遇到问题",
                    Content = $"发生了一个意外错误，应用可能需要重启。\n\n错误详情:\n{message}",
                    CloseButtonText = "关闭",
                    XamlRoot = _window.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception);
            e.SetObserved(); // 表示异常已经被处理
        }

       

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception);
        }


        

        private void LogException(Exception? ex)
        {
            if (ex == null) return;
            string logPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), // 获取临时文件夹路径
                "SlientFan_CrashLog.txt");    // 日志文件名

            try
            {
                // 将最详细的异常信息写入文件
                System.IO.File.WriteAllText(logPath, ex.ToString());
            }
            catch { }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();

            var theme = SettingsService.Theme;
            Helpers.ThemeHelper.ApplyTheme(theme);

            SettingsService.ThemeChanged += (s, e) => Helpers.ThemeHelper.ApplyTheme(e);
        }
    }
}
