using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using 程控静扇.Services;
using 程控静扇.ViewModels;

namespace 程控静扇.Pages
{
    public sealed partial class SpeedControlPage : Page
    {
        public StatusService StatusServiceInstance => StatusService.Instance;
        public EmergencyFanControlService EFCService => EmergencyFanControlService.Instance;
        private readonly ResourceLoader _resourceLoader;
        private readonly DispatcherTimer _timer;
        private int _pointCounter;
        private const int MaxDataPoints = 60;

        private bool _isChartInitialized = false;

        // --- MODIFIED ---
        // 使用可空的 ObservablePoint? 来支持 null 值（用于断开线条）
        private readonly List<(LineSeries<ObservablePoint?> Normal, LineSeries<ObservablePoint?> High)> _cpuSeriesPairs = new();
        private readonly Dictionary<int, ObservablePoint?> _lastPointPerCpu = new();

        private readonly ServerVisualizerService _visualizerService = new();
        private string? _baseSvgContent;

        public ObservableCollection<ISeries> Series { get; set; }
        public ICartesianAxis[]? XAxes { get; set; }
        public ICartesianAxis[]? YAxes { get; set; }
        public ObservableCollection<FanStatusViewModel> FanStatuses { get; set; }

        public string Version
        {
            get
            {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                string VersionReport = string.Format(_resourceLoader.GetString("Version_Label"), version.Major, version.Minor, version.Build);
                return VersionReport;
            }
        }

        public SpeedControlPage()
        {
            this.InitializeComponent();
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            FanStatuses = new ObservableCollection<FanStatusViewModel>();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            Series = new ObservableCollection<ISeries>();
            XAxes = new Axis[] { new Axis() };
            YAxes = new Axis[] { new Axis() };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += Timer_Tick;

            Loaded += SpeedControlPage_Loaded;
            Unloaded += SpeedControlPage_Unloaded;
            EFCService.CooldownCompleted += EFCService_CooldownCompleted;

            ActualThemeChanged += (s, e) => UpdateAxisColors();
        }

        private void EFCService_CooldownCompleted(object? sender, string e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                CooldownInfoBar.Message = e;
                CooldownInfoBar.IsOpen = true;
            });
        }

        private void UpdateAxisColors()
        {
            var isLightTheme = ActualTheme == ElementTheme.Light;
            var axisColor = isLightTheme ? SKColors.Black : SKColors.White;

            if (YAxes != null && YAxes.Length > 0)
            {
                YAxes[0].NamePaint = new SolidColorPaint(axisColor);
                YAxes[0].LabelsPaint = new SolidColorPaint(axisColor);
            }

            if (XAxes != null && XAxes.Length > 0)
            {
                XAxes[0].NamePaint = new SolidColorPaint(axisColor);
                XAxes[0].LabelsPaint = new SolidColorPaint(axisColor);
            }
        }

        private async void SpeedControlPage_Loaded(object? sender, RoutedEventArgs e)
        {
            UpdateAxisColors();
            LoadSettings();
            this.DataContext = this;
            _timer.Start();
            StatusService.Instance.IpmiStatus = SettingsService.LoadIpmiStatus();
            await LoadSvgAssetAsync();
        }

        private void SpeedControlPage_Unloaded(object? sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private async Task LoadSvgAssetAsync()
        {
            try
            {
                Uri uri = new Uri("ms-appx:///Assets/r730.svg");
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                _baseSvgContent = await FileIO.ReadTextAsync(file);

                if (!string.IsNullOrEmpty(_baseSvgContent) && ServerLayoutImage != null)
                {
                    var svgImageSource = new SvgImageSource();
                    var stream = new InMemoryRandomAccessStream();
                    using (var writer = new DataWriter(stream))
                    {
                        writer.WriteString(_baseSvgContent);
                        await writer.StoreAsync();
                        writer.DetachStream();
                    }
                    stream.Seek(0);
                    await svgImageSource.SetSourceAsync(stream);
                    ServerLayoutImage.Source = svgImageSource;
                }
            }
            catch (Exception)
            {
                _baseSvgContent = null;
            }
        }

        private void InitializeChart(int cpuCount)
        {
            var colors = new[] { SKColors.CornflowerBlue, SKColors.IndianRed, SKColors.Green, SKColors.DarkViolet, SKColors.Gold, SKColors.MediumAquamarine };
            var highTempStroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 4 };

            for (int i = 0; i < cpuCount; i++)
            {
                // --- MODIFIED ---
                // 初始化时使用 ObservablePoint?
                var normalSeries = new LineSeries<ObservablePoint?>
                {
                    Values = new ObservableCollection<ObservablePoint?>(),
                    Name = $"CPU {i} 最高温度",
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(colors[i % colors.Length]) { StrokeThickness = 2 },
                };

                var highTempSeries = new LineSeries<ObservablePoint?>
                {
                    Values = new ObservableCollection<ObservablePoint?>(),
                    Name = null,
                    IsVisibleAtLegend = false,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0,
                    Stroke = highTempStroke,
                };

                _cpuSeriesPairs.Add((normalSeries, highTempSeries));

                Series.Add(normalSeries);
                Series.Add(highTempSeries);
            }
            _isChartInitialized = true;
        }

        // --- MODIFIED METHOD ---
        // 使用 null 来创建线条中断
        private void AddNullToSeries(ObservableCollection<ObservablePoint?> series)
        {
            series.Add(null);
        }

        // --- COMPLETELY REPLACED METHOD ---
        // 修正后的计时器事件处理
        private async void Timer_Tick(object? sender, object e)
        {
            var cpuTemps = await TemperatureService.Instance.GetTemperaturesAsync();
            if (!cpuTemps.Any()) return;

            if (!_isChartInitialized)
            {
                InitializeChart(cpuTemps.Count);
            }

            if (cpuTemps.Count != _cpuSeriesPairs.Count) return;

            _pointCounter++;

            for (int i = 0; i < cpuTemps.Count; i++)
            {
                var currentTemp = cpuTemps[i];
                var newPoint = new ObservablePoint(_pointCounter, currentTemp);

                var (normalSeries, highSeries) = _cpuSeriesPairs[i];
                if (normalSeries.Values == null || highSeries.Values == null) continue;

                var normalValues = (ObservableCollection<ObservablePoint?>)normalSeries.Values;
                var highValues = (ObservableCollection<ObservablePoint?>)highSeries.Values;

                _lastPointPerCpu.TryGetValue(i, out var lastPoint);

                bool wasHigh = lastPoint?.Y >= 70;
                bool isHigh = currentTemp >= 70;

                if (isHigh)
                {
                    // 如果刚从低温区过渡到高温区，添加最后一个低温点以确保线条连续
                    if (!wasHigh && lastPoint != null)
                    {
                        highValues.Add(lastPoint);
                        AddNullToSeries(normalValues); // 中断普通温度线条
                    }
                    highValues.Add(newPoint);
                }
                else // isLow
                {
                    // 如果刚从高温区过渡到低温区，添加最后一个高温点以确保线条连续
                    if (wasHigh && lastPoint != null)
                    {
                        normalValues.Add(lastPoint);
                        AddNullToSeries(highValues); // 中断高温温度线条
                    }
                    normalValues.Add(newPoint);
                }

                _lastPointPerCpu[i] = newPoint;

                // 修正后的清理逻辑：能正确处理 null 值
                while (normalValues.Count > 0 && (normalValues[0] == null || normalValues[0]!.X < _pointCounter - MaxDataPoints))
                {
                    normalValues.RemoveAt(0);
                }
                while (highValues.Count > 0 && (highValues[0] == null || highValues[0]!.X < _pointCounter - MaxDataPoints))
                {
                    highValues.RemoveAt(0);
                }
            }

            // 更新 X 轴范围
            if (XAxes != null && XAxes.Length > 0)
            {
                XAxes[0].MinLimit = _pointCounter > MaxDataPoints ? _pointCounter - MaxDataPoints : 0;
                XAxes[0].MaxLimit = _pointCounter;
            }
        }

        private void FanSpeedSlider_ValueChanged(object? sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingsService.SaveSetting("FanSpeed", e.NewValue);
            EFCService.RecalculateFanSpeed();
        }

        private void ParseAndDisplayFanData(string sensorData)
        {
            FanStatuses.Clear();
            var fanLines = sensorData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Where(l => l.Trim().StartsWith("Fan")).ToList();
            if (fanLines.Any())
            {
                FanStatusMessageTextBlock.Visibility = Visibility.Collapsed;
                foreach (var line in fanLines)
                {
                    var parts = line.Split('|').Select(p => p.Trim()).ToArray();
                    if (parts.Length > 2 && double.TryParse(parts[1], out double rpm))
                    {
                        FanStatuses.Add(new FanStatusViewModel
                        {
                            Name = parts[0],
                            Rpm = rpm,
                            Health = rpm < 2000 ? FanHealth.Critical : FanHealth.Ok
                        });
                    }
                }
            }
            else
            {
                FanStatusMessageTextBlock.Text = _resourceLoader.GetString("NoFanInfoFound");
                FanStatusMessageTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void LoadSettings()
        {
            IpAddressTextBox.Text = SettingsService.LoadSetting("iDRAC_IP", "169.254.0.1");
            UsernameTextBox.Text = SettingsService.LoadSetting("iDRAC_User", "root");
            PasswordBox.Password = SettingsService.LoadPassword();
            FanSpeedSlider.Value = SettingsService.LoadSetting("FanSpeed", 20.0);
        }

        private void SaveConnectionSettings()
        {
            SettingsService.SaveSetting("iDRAC_IP", IpAddressTextBox.Text);
            SettingsService.SaveSetting("iDRAC_User", UsernameTextBox.Text);
            SettingsService.SavePassword(PasswordBox.Password);
        }

        private IpmiCredentials GetCredentials()
        {
            return new IpmiCredentials(IpAddressTextBox.Text, UsernameTextBox.Text, PasswordBox.Password);
        }

        private async void SetManualButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading(_resourceLoader.GetString("CommunicatingWithIdrac"));
            SetButtonsEnabled(false);
            try
            {
                var creds = GetCredentials();
                string result = await IpmiService.SetManualFanControl(creds);

                if (!result.StartsWith("Error"))
                {
                    int speed = (int)FanSpeedSlider.Value;
                    result = await IpmiService.SetFanSpeed(creds, speed);

                    if (!result.StartsWith("Error"))
                    {
                        ShowResultInfoBar(string.Format(_resourceLoader.GetString("InfoBar_SettingManualSuccess"), speed), InfoBarSeverity.Success);
                        SaveConnectionSettings();
                        SettingsService.SaveSetting("FanSpeed", FanSpeedSlider.Value);
                    }
                    else
                    {
                        ShowResultInfoBar(_resourceLoader.GetString("InfoBar_SettingManualFailed"), InfoBarSeverity.Error);
                    }
                }
                else
                {
                    ShowResultInfoBar(_resourceLoader.GetString("InfoBar_DisableAutoFailed"), InfoBarSeverity.Error);
                }
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private async void SetAutoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading(_resourceLoader.GetString("CommunicatingWithIdrac"));
            SetButtonsEnabled(false);
            try
            {
                var creds = GetCredentials();
                string result = await IpmiService.SetAutomaticFanControl(creds);

                if (!result.StartsWith("Error"))
                {
                    ShowResultInfoBar(_resourceLoader.GetString("InfoBar_EnableAutoSuccess"), InfoBarSeverity.Success);
                    SaveConnectionSettings();
                }
                else
                {
                    ShowResultInfoBar(_resourceLoader.GetString("InfoBar_EnableAutoFailed"), InfoBarSeverity.Error);
                }
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private async void GetStatusButton_Click(object? sender, RoutedEventArgs e)
        {
            ShowLoading(_resourceLoader.GetString("CommunicatingWithIdrac"));
            SetButtonsEnabled(false);
            if (FanStatusMessageTextBlock != null)
            {
                FanStatuses.Clear();
                FanStatusMessageTextBlock.Text = _resourceLoader.GetString("FetchingFanInfo");
                FanStatusMessageTextBlock.Visibility = Visibility.Visible;
            }

            try
            {
                var creds = GetCredentials();
                string result = await IpmiService.GetSensorData(creds);

                if (!result.StartsWith("Error"))
                {
                    ShowResultInfoBar(_resourceLoader.GetString("InfoBar_GetStatusSuccess"), InfoBarSeverity.Success);
                    ParseAndDisplayFanData(result);
                    await UpdateServerVisualizationAsync(result);
                }
                else
                {
                    ShowResultInfoBar(_resourceLoader.GetString("InfoBar_GetStatusFailed"), InfoBarSeverity.Error);
                    if (FanStatusMessageTextBlock != null) FanStatusMessageTextBlock.Text = _resourceLoader.GetString("FetchInfoFailed");
                }
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private async Task UpdateServerVisualizationAsync(string sensorData)
        {
            if (string.IsNullOrEmpty(_baseSvgContent) || ServerLayoutImage == null)
            {
                return;
            }

            var sensorValues = _visualizerService.ParseSensorDataForVisualization(sensorData);
            if (!sensorValues.Any()) return;

            string updatedSvg = await _visualizerService.UpdateSvgAsync(_baseSvgContent, sensorValues);

            DispatcherQueue.TryEnqueue(async () =>
            {
                var svgImageSource = new SvgImageSource();
                var stream = new InMemoryRandomAccessStream();

                using (var writer = new DataWriter(stream))
                {
                    writer.WriteString(updatedSvg);
                    await writer.StoreAsync();
                    writer.DetachStream();
                }

                stream.Seek(0);
                await svgImageSource.SetSourceAsync(stream);
                ServerLayoutImage.Source = svgImageSource;
            });
        }

        private void SetButtonsEnabled(bool isEnabled)
        {
            SetManualButton.IsEnabled = isEnabled;
            SetAutoButton.IsEnabled = isEnabled;
            GetStatusButton.IsEnabled = isEnabled;
        }

        private void ShowLoading(string message)
        {
            InfoBar.IsOpen = true;
            InfoBar.Severity = InfoBarSeverity.Informational;
            InfoBar.Message = message;
            InfoBar.IsClosable = false;
        }

        private async void ShowResultInfoBar(string message, InfoBarSeverity severity, int autoCloseAfter = 3000)
        {
            InfoBar.Message = message;
            InfoBar.Severity = severity;
            InfoBar.IsClosable = true;
            InfoBar.IsOpen = true;

            if (autoCloseAfter > 0)
            {
                await Task.Delay(autoCloseAfter);
                if (InfoBar.IsOpen && InfoBar.Message == message)
                {
                    InfoBar.IsOpen = false;
                }
            }
        }

        private void OnPointerEntered(object? sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender != null) Helpers.StatusHelper.Generic_PointerEntered(sender, e);
        }

        private void OnPointerExited(object? sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender != null) Helpers.StatusHelper.Generic_PointerExited(sender, e);
        }
    }
}