using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace 程控静扇.Services
{
    public class LogService : INotifyPropertyChanged
    {
        private static readonly Lazy<LogService> _instance = new(() => new LogService());
        public static LogService Instance => _instance.Value;

        private StringBuilder _logContentBuilder = new StringBuilder();
        private string _logContent = string.Empty;

        public string LogContent
        {
            get => _logContent;
            private set
            {
                if (_logContent != value)
                {
                    _logContent = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AppendLog(string message)
        {
            _logContentBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            LogContent = _logContentBuilder.ToString();
        }

        public void ClearLog()
        {
            _logContentBuilder.Clear();
            LogContent = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}