using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace 程控静扇.Services
{
    public enum EmergencyFanStatus { Inactive, RunningHigh, RunningCritical }

    public class EmergencyFanControlService : INotifyPropertyChanged
    {
        private static readonly Lazy<EmergencyFanControlService> _instance = new(() => new EmergencyFanControlService());
        public static EmergencyFanControlService Instance => _instance.Value;

        private readonly DispatcherTimer _timer;
        private readonly ResourceLoader _resourceLoader;
        
        private EmergencyFanStatus _status = EmergencyFanStatus.Inactive;
        public EmergencyFanStatus Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _boostedFanSpeed = 0;
        public double BoostedFanSpeed
        {
            get => _boostedFanSpeed;
            set
            {
                _boostedFanSpeed = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<string>? CooldownCompleted;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private EmergencyFanControlService()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            if (_timer.IsEnabled) return;
            double interval = SettingsService.LoadSetting("EmergencyControl_Interval", 10.0);
            _timer.Interval = TimeSpan.FromSeconds(interval > 0 ? interval : 10.0);
            _timer.Start();
            StatusService.Instance.IsEmergencyControlActive = true;
        }

        public async Task StopAsync()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
            Status = EmergencyFanStatus.Inactive;
            StatusService.Instance.IsEmergencyControlActive = false;
            BoostedFanSpeed = 0;

            var ip = SettingsService.LoadSetting("iDRAC_IP", string.Empty);
            var user = SettingsService.LoadSetting("iDRAC_User", string.Empty);
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(user)) return;
            var creds = new IpmiCredentials(ip, user, SettingsService.LoadPassword());
            int manualSpeed = (int)SettingsService.LoadSetting("FanSpeed", 20.0);
            await IpmiService.SetFanSpeed(creds, manualSpeed);
        }

        public async void Restart()
        {
            await StopAsync();
            Start();
        }

        private async void Timer_Tick(object? sender, object e)
        {
            var temps = await TemperatureService.Instance.GetTemperaturesAsync();
            if (!temps.Any())
            {
                BoostedFanSpeed = 0;
                return;
            }

            var maxTemp = temps.Max();

            var ip = SettingsService.LoadSetting("iDRAC_IP", string.Empty);
            var user = SettingsService.LoadSetting("iDRAC_User", string.Empty);
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(user)) return;
            var creds = new IpmiCredentials(ip, user, SettingsService.LoadPassword());

            await ApplyFanControlLogic(maxTemp, creds);
        }

        private async Task ApplyFanControlLogic(double currentTemp, IpmiCredentials creds)
        {
            var highTemp = SettingsService.LoadSetting("EmergencyControl_HighTemp", 70.0);
            var criticalTemp = SettingsService.LoadSetting("EmergencyControl_CriticalTemp", 85.0);
            var cooldownHigh = SettingsService.LoadSetting("EmergencyControl_CooldownHigh", 75.0);
            var cooldownLow = SettingsService.LoadSetting("EmergencyControl_CooldownLow", 60.0);

            var oldStatus = Status;
            var newStatus = oldStatus;

            if (currentTemp > criticalTemp)
            {
                newStatus = EmergencyFanStatus.RunningCritical;
            }
            else if (currentTemp > highTemp)
            {
                if (oldStatus != EmergencyFanStatus.RunningCritical) newStatus = EmergencyFanStatus.RunningHigh;
            }
            else if (currentTemp < cooldownLow)
            {
                newStatus = EmergencyFanStatus.Inactive;
            }
            else if (currentTemp < cooldownHigh)
            {
                if (oldStatus == EmergencyFanStatus.RunningCritical) newStatus = EmergencyFanStatus.RunningHigh;
            }

            if (newStatus != oldStatus)
            {
                await SetFanForState(newStatus, creds, currentTemp);
                Status = newStatus;

                if (oldStatus != EmergencyFanStatus.Inactive && newStatus == EmergencyFanStatus.Inactive)
                {
                    CooldownCompleted?.Invoke(this, "温度已恢复正常，风扇转速已自动调整。 ");
                }
            }
        }

        private async Task SetFanForState(EmergencyFanStatus state, IpmiCredentials creds, double currentTemp)
        {
            double baseSpeed = SettingsService.LoadSetting("FanSpeed", 20.0);
            double newSpeed = baseSpeed;
            double multiplier = 100.0;

            if (state == EmergencyFanStatus.RunningHigh)
            {
                multiplier = SettingsService.LoadSetting("EmergencyControl_HighMultiplier", 120.0);
                newSpeed = baseSpeed * (multiplier / 100.0);
            }
            else if (state == EmergencyFanStatus.RunningCritical)
            {
                multiplier = SettingsService.LoadSetting("EmergencyControl_CriticalMultiplier", 150.0);
                newSpeed = baseSpeed * (multiplier / 100.0);
                ShowToastNotification(currentTemp, (int)newSpeed);
            }

            int finalSpeed = Math.Min(100, (int)Math.Round(newSpeed));
            BoostedFanSpeed = finalSpeed;

            await IpmiService.SetManualFanControl(creds);
            await IpmiService.SetFanSpeed(creds, finalSpeed);
        }

        public void RecalculateFanSpeed()
        {
            if (Status == EmergencyFanStatus.Inactive) 
            {
                BoostedFanSpeed = SettingsService.LoadSetting("FanSpeed", 20.0);
                return;
            }

            double baseSpeed = SettingsService.LoadSetting("FanSpeed", 20.0);
            double newSpeed = baseSpeed;
            double multiplier = 100.0;

            if (Status == EmergencyFanStatus.RunningHigh)
            {
                multiplier = SettingsService.LoadSetting("EmergencyControl_HighMultiplier", 120.0);
                newSpeed = baseSpeed * (multiplier / 100.0);
            }
            else if (Status == EmergencyFanStatus.RunningCritical)
            {
                multiplier = SettingsService.LoadSetting("EmergencyControl_CriticalMultiplier", 150.0);
                newSpeed = baseSpeed * (multiplier / 100.0);
            }

            BoostedFanSpeed = Math.Min(100, (int)Math.Round(newSpeed));
        }

        private void ShowToastNotification(double temp, int fanSpeed)
        {
            string title = _resourceLoader.GetString("Notification_CriticalTemp_Title");
            string body = string.Format(_resourceLoader.GetString("Notification_CriticalTemp_Body"), temp, fanSpeed);

            XmlDocument toastXml = new XmlDocument();
            toastXml.LoadXml($"<toast><visual><binding template=\"ToastGeneric\"><text>{title}</text><text>{body}</text></binding></visual></toast>");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}