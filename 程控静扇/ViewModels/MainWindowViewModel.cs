using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Windows.Input;
using 程控静扇.Helpers; // Added for User32

namespace 程控静扇.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private Window _mainWindow;

        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        [RelayCommand]
        private void ShowHideWindow()
        {
            if (_mainWindow.Visible)
            {
                _mainWindow.AppWindow.Hide();
            }
            else
            {
                _mainWindow.AppWindow.Show();
                // Use P/Invoke to set the window to foreground
                User32.SetForegroundWindow(WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow));
            }
        }

        [RelayCommand]
        private void ExitApplication()
        {
            _mainWindow.Close();
        }
    }
}