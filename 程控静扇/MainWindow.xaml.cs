using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using 程控静扇.Services;
using Microsoft.UI.Dispatching; // Added for DispatcherQueueTimer
using 程控静扇.Helpers; // Added for StatusHelper
using 程控静扇.ViewModels; // Added for ViewModel
using WinRT.Interop; // Added for WindowNative
using Microsoft.UI.Windowing; // Added for AppWindowPresenterKind (explicitly added again)
using H.NotifyIcon; // Example using H.NotifyIcon library
using H.NotifyIcon.Core; // If using core components directly
using Microsoft.UI.Xaml.Media.Imaging; // Added for BitmapImage
using Microsoft.UI.Xaml.Input; // Added for MouseButton
using System.Windows.Input; // For ICommand

namespace 程控静扇
{
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public ICommand ShowWindowCommand { get; private set; }
        public ICommand ExitApplicationCommand { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainWindowViewModel(this);
            NavView.DataContext = ViewModel;
            ExtendsContentIntoTitleBar = true;
            ContentFrame.CacheSize = 5;

            IpmiService.CommunicationStarted += IpmiService_CommunicationStarted;
            IpmiService.CommunicationCompleted += IpmiService_CommunicationCompleted;
            StatusService.Instance.PropertyChanged += StatusService_PropertyChanged;

            // Handle window state changes for minimize to tray
            this.AppWindow.Changed += AppWindow_Changed;

            // Handle window closing to implement minimize to tray
            this.AppWindow.Closing += AppWindow_Closing;

            // Initialize Commands
            ShowWindowCommand = new RelayCommand(ShowWindow);
            ExitApplicationCommand = new RelayCommand(() => App.Current.Exit());
            MyNotifyIcon.DataContext = this;
        }

        public void ShowWindow()
        {
            System.Diagnostics.Debug.WriteLine("ShowWindow method called.");
            this.AppWindow.Show();
            // Hide the tray icon when the window is visible
            MyNotifyIcon.Visibility = Visibility.Collapsed; // Access the TaskbarIcon by its x:Name
            // Restore window state if it was minimized
            if (this.AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.Restore();
            }
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // Check if minimize to tray is enabled in settings
            if (SettingsService.MinimizeToTrayOnClose)
            {
                args.Cancel = true; // Prevent the window from actually closing
                sender.Hide(); // Hide the window
                // Show the tray icon
                MyNotifyIcon.Visibility = Visibility.Visible; // Access the TaskbarIcon by its x:Name
            }
            // If minimize to tray is not enabled, allow the window to close normally
        }

        private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange)
            {
                if (sender.Presenter is OverlappedPresenter overlappedPresenter)
                {
                    if (overlappedPresenter.State == OverlappedPresenterState.Minimized)
                    {
                        // If minimize to tray is enabled, hide the window and show the tray icon
                        if (SettingsService.MinimizeToTrayOnClose)
                        {
                            sender.Hide();
                            MyNotifyIcon.Visibility = Visibility.Visible; // Access the TaskbarIcon by its x:Name
                        }
                    }
                    else if (overlappedPresenter.State == OverlappedPresenterState.Restored || overlappedPresenter.State == OverlappedPresenterState.Maximized)
                    {
                        // Hide the tray icon when the window is visible
                        MyNotifyIcon.Visibility = Visibility.Collapsed; // Access the TaskbarIcon by its x:Name
                    }
                }
            }
        }


        private void StatusService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.PropertyName == nameof(StatusService.IsEmergencyControlActive))
                {
                    EmergencyStatusPanel.Visibility = StatusService.Instance.IsEmergencyControlActive ? Visibility.Visible : Visibility.Collapsed;
                    NormalStatusPanel.Visibility = StatusService.Instance.IsEmergencyControlActive ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (e.PropertyName == nameof(StatusService.CurrentStatus))
                {
                    StatusTextBlock.Text = StatusService.Instance.CurrentStatus!;
                }
            });
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            StatusService.Instance.StatusTextBlock = StatusTextBlock;
            StatusService.Instance.StatusProgressRing = StatusProgressRing;
            StatusService.Instance.StatusSymbolIcon = StatusSymbolIcon;

            // Navigate to the home page initially
            NavigateToPage("SpeedControlPage");
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // In a real app, you might navigate to a settings page.
            }
            else if (args.InvokedItemContainer != null)
            {
                var navItemTag = args.InvokedItemContainer.Tag?.ToString();
                if (navItemTag != null) NavigateToPage(navItemTag);
            }
        }

        private void NavigateToPage(string pageTag)
        {
            Type pageType = Type.GetType($"程控静扇.Pages.{pageTag}");
            if (pageType != null)
            {
                var currentPageType = ContentFrame.CurrentSourcePageType;
                if (currentPageType?.Name != pageTag)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Update the NavView's selected item
            if (ContentFrame.SourcePageType != null)
            {
                #pragma warning disable CS8600
                var tag = ContentFrame.SourcePageType?.Name;
#pragma warning restore CS8600
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == tag);

                // Set default status based on the navigated page
                if (tag == "SpeedControlPage")
                {
                    StatusService.Instance.DefaultStatusText = "正在监控处理器温度";
                    StatusService.Instance.DefaultStatusSymbolGlyph = ""; // Not used for this state
                    StatusService.Instance.UpdateStatus(StatusService.Instance.DefaultStatusText ?? "", "", true); // Show progress ring
                }
                else
                {
                    // For other pages, you might have different defaults or clear the status
                    StatusService.Instance.DefaultStatusText = ""; // Or a generic message for other pages
                    StatusService.Instance.DefaultStatusSymbolGlyph = ""; // Clear symbol
                    StatusService.Instance.UpdateStatus(StatusService.Instance.DefaultStatusText ?? "", StatusService.Instance.DefaultStatusSymbolGlyph ?? "", false);
                }
            }
        }

        private void NavigationViewItem_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Call the generic helper method
            StatusHelper.Generic_PointerEntered(sender, e);
        }

        private void NavigationViewItem_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Call the generic helper method
            StatusHelper.Generic_PointerExited(sender, e);
        }

        private void IpmiService_CommunicationStarted(object? sender, EventArgs e)
        {
            // Ensure UI updates happen on the UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusService.Instance.StopStatusRevertTimer(); // Stop any pending revert
                StatusService.Instance.UpdateStatus("正在等待iDRAC返回数据", "", true); // Show progress ring
            });
        }

        private void IpmiService_CommunicationCompleted(object? sender, CommunicationCompletedEventArgs e)
        {
            // Ensure UI updates happen on the UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusService.Instance.StopStatusRevertTimer(); // Stop any pending revert

                if (e.IsSuccess)
                {
                    StatusService.Instance.UpdateStatus("通信完成", char.ConvertFromUtf32(0xE73E), false); // Show checkmark
                }
                else
                {
                    StatusService.Instance.UpdateStatus("通信失败", char.ConvertFromUtf32(0xE783), false); // Show cross
                }

                StatusService.Instance.StartStatusRevertTimer(); // Start timer to revert to default
            });
        }

        // Add RelayCommand class at the end of the file
        public class RelayCommand : System.Windows.Input.ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool>? _canExecute;

            public event EventHandler? CanExecuteChanged;

            public RelayCommand(Action execute, Func<bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

            public void Execute(object? parameter)
            {
                System.Diagnostics.Debug.WriteLine("RelayCommand executed.");
                _execute();
            }

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}