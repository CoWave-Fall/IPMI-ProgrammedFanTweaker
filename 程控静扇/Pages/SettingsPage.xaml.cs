using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using 程控静扇.Services;
using System;
using 程控静扇.Helpers; // Added for StartupHelper

namespace 程控静扇.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private bool _isLoaded = false;
        private readonly ResourceLoader _resourceLoader;

        public SettingsPage()
        {
            this.InitializeComponent();
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            Loaded += SettingsPage_Loaded;
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLanguageSettings();
            LoadThemeSettings();
            LoadTempSourceSettings();
            LoadEmergencyControlSettings();
            LoadApplicationBehaviorSettings(); // New method call
            _isLoaded = true;
        }

        private void LoadThemeSettings()
        {
            var currentTheme = SettingsService.Theme;
            var selected = ThemeComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag.ToString() == currentTheme.ToString());
            if (selected != null)
            {
                ThemeComboBox.SelectedItem = selected;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded || e.AddedItems.FirstOrDefault() == null) return;

            var selectedItem = (ComboBoxItem)e.AddedItems.First();
            if (Enum.TryParse<AppTheme>(selectedItem.Tag.ToString(), out var newTheme))
            {
                SettingsService.Theme = newTheme;
            }
        }

        private void LoadTempSourceSettings()
        {
            var currentSource = SettingsService.LoadTemperatureSource();
            var selected = TempSourceComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag.ToString() == currentSource.ToString());
            if (selected != null)
            {
                TempSourceComboBox.SelectedItem = selected;
            }
        }

        private void TempSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (!_isLoaded || e.AddedItems.FirstOrDefault() == null) return;

            var selectedItem = (ComboBoxItem)e.AddedItems.First();
            if (Enum.TryParse<TemperatureSource>(selectedItem.Tag.ToString(), out var newSource))
            {
                SettingsService.SaveTemperatureSource(newSource);
            }
        }

        private void LoadLanguageSettings()
        {
            var currentLanguage = SettingsService.LoadSetting<string>("AppLanguage", ApplicationLanguages.PrimaryLanguageOverride);
            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = "en-US"; // Default language
            }

            var selected = LanguageComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag.ToString() == currentLanguage);
            if (selected != null)
            {
                LanguageComboBox.SelectedItem = selected;
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded || e.AddedItems.FirstOrDefault() == null) return;

            var selectedItem = (ComboBoxItem)e.AddedItems.First();
            var newLanguage = selectedItem.Tag?.ToString();
            var currentLanguage = ApplicationLanguages.PrimaryLanguageOverride;

            if (newLanguage != null && newLanguage != currentLanguage)
            {
                SettingsService.SaveSetting("AppLanguage", newLanguage);

                var dialog = new ContentDialog
                {
                    Title = _resourceLoader.GetString("LanguageChange_Title"),
                    Content = _resourceLoader.GetString("LanguageChange_Content"),
                    PrimaryButtonText = _resourceLoader.GetString("OK_Button"),
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync().AsTask();
            }
        }

        private void LoadEmergencyControlSettings()
        {
            EmergencyControlToggle.IsOn = SettingsService.LoadSetting("EmergencyControl_Enabled", false);
            SensorNameBox.Text = SettingsService.LoadSetting("EmergencyControl_SensorName", "Temp");
            HighTempThresholdBox.Value = SettingsService.LoadSetting("EmergencyControl_HighTemp", 70.0);
            CriticalTempThresholdBox.Value = SettingsService.LoadSetting("EmergencyControl_CriticalTemp", 85.0);
            HighFanMultiplierBox.Value = SettingsService.LoadSetting("EmergencyControl_HighMultiplier", 120.0);
            CriticalFanMultiplierBox.Value = SettingsService.LoadSetting("EmergencyControl_CriticalMultiplier", 150.0);
            CooldownHighTempBox.Value = SettingsService.LoadSetting("EmergencyControl_CooldownHigh", 75.0);
            CooldownLowTempBox.Value = SettingsService.LoadSetting("EmergencyControl_CooldownLow", 60.0);
            CheckIntervalBox.Value = SettingsService.LoadSetting("EmergencyControl_Interval", 10.0);
        }

        private void SaveEmergencyControlSettings()
        {
            SettingsService.SaveSetting("EmergencyControl_Enabled", EmergencyControlToggle.IsOn);
            SettingsService.SaveSetting("EmergencyControl_SensorName", SensorNameBox.Text);
            SettingsService.SaveSetting("EmergencyControl_HighTemp", HighTempThresholdBox.Value);
            SettingsService.SaveSetting("EmergencyControl_CriticalTemp", CriticalTempThresholdBox.Value);
            SettingsService.SaveSetting("EmergencyControl_HighMultiplier", HighFanMultiplierBox.Value);
            SettingsService.SaveSetting("EmergencyControl_CriticalMultiplier", CriticalFanMultiplierBox.Value);
            SettingsService.SaveSetting("EmergencyControl_CooldownHigh", CooldownHighTempBox.Value);
            SettingsService.SaveSetting("EmergencyControl_CooldownLow", CooldownLowTempBox.Value);
            SettingsService.SaveSetting("EmergencyControl_Interval", CheckIntervalBox.Value);
        }

        private void EmergencyControlToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SaveEmergencyControlSettings();
            
            if (EmergencyControlToggle.IsOn)
            {
                EmergencyFanControlService.Instance.Start();
            }
            else
            {
                _ = EmergencyFanControlService.Instance.StopAsync();
            }
        }

        // New methods for application behavior settings
        private void LoadApplicationBehaviorSettings()
        {
            MinimizeToTrayToggle.IsOn = SettingsService.MinimizeToTrayOnClose;
            RunOnStartupToggle.IsOn = SettingsService.RunOnStartup;
        }

        private void MinimizeToTrayToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsService.MinimizeToTrayOnClose = MinimizeToTrayToggle.IsOn;
        }

        private void RunOnStartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsService.RunOnStartup = RunOnStartupToggle.IsOn;
        }

        private void Setting_ValueChanged(object sender, NumberBoxValueChangedEventArgs e)
        {
            if (!_isLoaded) return;
            SaveEmergencyControlSettings();
        }

        private void Setting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            SaveEmergencyControlSettings();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveEmergencyControlSettings();
            EmergencyFanControlService.Instance.Restart();
        }

        private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Helpers.StatusHelper.Generic_PointerEntered(sender, e);
        }

        private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Helpers.StatusHelper.Generic_PointerExited(sender, e);
        }
    }
}
