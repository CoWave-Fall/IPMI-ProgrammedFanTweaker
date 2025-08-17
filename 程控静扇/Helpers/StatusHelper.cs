using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using 程控静扇.Services;

namespace 程控静扇.Helpers
{
    public static class StatusHelper
    {
        public static void Generic_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string helpText)
            {
                StatusService.Instance.UpdateStatus(helpText, char.ConvertFromUtf32(0xE8F2), false); // Info symbol: &#xE8F2;
            }
            else
            {
                StatusService.Instance.UpdateStatus("鼠标悬停于交互元素。", char.ConvertFromUtf32(0xE8F2), false); // Generic info
            }
            StatusService.Instance.StopStatusRevertTimer(); // Stop any pending revert
        }
        

        public static void Generic_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            StatusService.Instance.StartStatusRevertTimer(); // Start timer to revert to default
        }
    }
}
