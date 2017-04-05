using System;
using System.Runtime.InteropServices;

namespace EditableDataGridCF
{
    internal static class Win32
    {


        //ComboBox Messages codes
        //more CB message reference < http://msdn.microsoft.com/en-us/library/ms907150.aspx >
        public const uint CB_SHOWDROPDOWN = 0x014f;//335
        public const uint CB_GETDROPPEDSTATE = 0x0157;//343
        public const uint CB_GETEDITSEL = 0x0140;//320
        public const uint CB_SETEDITSEL = 0x0142;//322

        [DllImport("coredll.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, int lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, String lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, String lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, byte[] lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, byte[] lParam);

        public static IntPtr MAKELPARAM(int low, int high)
        {
            return (IntPtr)(high << 16 | low & (int)ushort.MaxValue);
        }
    }
}
