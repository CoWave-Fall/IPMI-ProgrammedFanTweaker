using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;

namespace 程控静扇.Services;

public class StatusService : INotifyPropertyChanged
{
    public TextBlock? StatusTextBlock { get; set; }
    public ProgressRing? StatusProgressRing { get; set; }
    public FontIcon? StatusSymbolIcon { get; set; }

    private DispatcherQueueTimer _statusRevertTimer;
    public string DefaultStatusText { get; set; }
    public string DefaultStatusSymbolGlyph { get; set; }

    private bool _isEmergencyControlActive = false;
    public bool IsEmergencyControlActive
    {
        get => _isEmergencyControlActive;
        set
        {
            if (_isEmergencyControlActive != value)
            {
                _isEmergencyControlActive = value;
                OnPropertyChanged();
            }
        }
    }

    private string _emergencyStatusText = "正在尝试控制CPU温度";
    public string EmergencyStatusText
    {
        get => _emergencyStatusText;
        set
        {
            if (_emergencyStatusText != value)
            {
                _emergencyStatusText = value;
                OnPropertyChanged();
            }
        }
    }

    private IpmiOperationStatus _ipmiStatus = IpmiOperationStatus.Default;
    public IpmiOperationStatus IpmiStatus
    {
        get => _ipmiStatus;
        set
        {
            if (_ipmiStatus != value)
            {
                _ipmiStatus = value;
                OnPropertyChanged();
            }
        }
    }
    private static readonly Lazy<StatusService> _instance = new(() => new StatusService());
    public static StatusService Instance => _instance.Value;

    private StatusService()
    {
        _statusRevertTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _statusRevertTimer.Interval = TimeSpan.FromSeconds(1); // Revert after 1 second
        _statusRevertTimer.Tick += StatusRevertTimer_Tick;

        // Set initial default values (will be updated on page navigation)
        DefaultStatusText = "正在监控处理器温度"; // Default for SpeedControlPage
        DefaultStatusSymbolGlyph = ""; // Default generic symbol (not used for this state)
    }

    private string _currentStatus = string.Empty;
    public string CurrentStatus
    {
        get => _currentStatus;
        set
        {
            if (_currentStatus != value)
            {
                _currentStatus = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Post(string message)
    {
        CurrentStatus = message;
    }

    public void Clear()
    {
        CurrentStatus = string.Empty;
    }

    public void StopStatusRevertTimer()
    {
        _statusRevertTimer.Stop();
    }

    public void StartStatusRevertTimer()
    {
        _statusRevertTimer.Start();
    }

    private void StatusRevertTimer_Tick(DispatcherQueueTimer sender, object args)
    {
        _statusRevertTimer.Stop();
        UpdateStatus(DefaultStatusText, DefaultStatusSymbolGlyph, false);
    }

    public void UpdateStatus(string text, string symbolGlyph, bool showProgressRing = false)
    {
        CurrentStatus = text; // Update the bound TextBlock

        // Always show ProgressRing for "正在监控处理器温度"
        if (text == "正在监控处理器温度")
        {
            showProgressRing = true;
        }

        if (StatusProgressRing != null)
        {
            StatusProgressRing.Visibility = showProgressRing ? Visibility.Visible : Visibility.Collapsed;
            StatusProgressRing.IsActive = showProgressRing;
        }
        if (StatusSymbolIcon != null)
        {
            StatusSymbolIcon.Visibility = showProgressRing ? Visibility.Collapsed : Visibility.Visible;
            StatusSymbolIcon.Glyph = symbolGlyph; // Update the FontIcon
        }
    }
}

public enum IpmiOperationStatus
{
    Default,
    Success,
    Failure
}
