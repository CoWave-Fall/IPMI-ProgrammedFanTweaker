using Microsoft.UI.Xaml.Controls;
using 程控静扇.Services; // Assuming LogService will be in Services namespace

namespace 程控静扇.Pages
{
    public sealed partial class LogPage : Page
    {
        public LogService LogServiceInstance => LogService.Instance;
        public LogPage()
        {
            this.InitializeComponent();
            this.DataContext = this; // Set DataContext for x:Bind
        }
    }
}