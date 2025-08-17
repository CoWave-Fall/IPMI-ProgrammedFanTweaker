using System.Runtime.InteropServices;
using System; // Added for IntPtr

namespace 程控静扇.Helpers
{
    public static class User32
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}