using Microsoft.UI.Xaml;
using 程控静扇.Services;

namespace 程控静扇.Helpers
{
    public static class ThemeHelper
    {
        public static void ApplyTheme(AppTheme theme)
        {
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                switch (theme)
                {
                    case AppTheme.Light:
                        rootElement.RequestedTheme = ElementTheme.Light;
                        break;
                    case AppTheme.Dark:
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        break;
                    case AppTheme.System:
                        rootElement.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }
    }
}
