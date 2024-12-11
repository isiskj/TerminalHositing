using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.Web.Http.Headers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TerminalHositing.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TerminalPage : Page
    {
        public TerminalPage()
        {
            this.InitializeComponent();
            this.SizeChanged += TermianlSizeChanged;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // WindowsStyleの変更
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        //public IntPtr hWnd;
        private IntPtr terminalHandle;

        const int GWL_STYLE = -16;
        const uint WS_SYSMENU = 0x00080000;
        const uint WS_CAPTION = 0x00C00000;
        const uint WS_MINIMIZEBOX = 0x00020000;
        const uint WS_MAXIMIZEBOX = 0x00010000;
        const uint WS_THICKFRAME = 0x00040000;

        // これがDependencyProperty
        public static readonly DependencyProperty hWndProperty =
          DependencyProperty.Register(
            nameof(HWnd),
            typeof(long),
            typeof(TerminalPage),
            null
          );

        // これがProperty wrapper
        public IntPtr HWnd
        {
            get { return new IntPtr((long)GetValue(hWndProperty)); }
            set { SetValue(hWndProperty, value.ToInt64()); }
        }

        private void EmbedWindowsTerminal()
        {
            var process = new Process();
            process.StartInfo.FileName = "wt.exe";
            process.StartInfo.UseShellExecute = false;
            process.Start();

            if(HWnd == IntPtr.Zero)
            {
                Console.WriteLine("hWnd=null");
                return;
            }

            while (terminalHandle == IntPtr.Zero)
            {
                terminalHandle = FindWindow("CASCADIA_HOSTING_WINDOW_CLASS", null);
            }

            if (terminalHandle != IntPtr.Zero)
            {
                uint style = (uint) GetWindowLong(terminalHandle, GWL_STYLE);
                style &= ~(WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
                SetWindowLong(terminalHandle, GWL_STYLE, style);
                SetParent(terminalHandle, HWnd);
                PanelSizeChange();
            }
        }

        public void SetText(string pastString)
        {
            SetForegroundWindow(terminalHandle);
            var pack = new DataPackage();
            pack.SetText(pastString);
            Clipboard.SetContent(pack);
            PasteFromClipboard();
        }

        static void PasteFromClipboard()
        {
            INPUT[] inputs = new INPUT[4];

            // Ctrlキーの押下
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = (ushort) VirtualKey.Control;
            inputs[0].u.ki.dwFlags = 0;

            // Vキーの押下
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = (ushort) VirtualKey.V;
            inputs[1].u.ki.dwFlags = 0;

            // Vキーの解放
            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].u.ki.wVk = (ushort) VirtualKey.V;
            inputs[2].u.ki.dwFlags = KEYEVENTF_KEYUP;

            // Ctrlキーの解放
            inputs[3].type = INPUT_KEYBOARD;
            inputs[3].u.ki.wVk = (ushort) VirtualKey.Control;
            inputs[3].u.ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput((uint) inputs.Length, inputs, INPUT.Size);

            Thread.Sleep(100);  // 適度な遅延を挿入（貼り付けの時間を確保）
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            EmbedWindowsTerminal();
        }

        private void TermianlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TerminalPage element)
            {
                PanelSizeChange();
            }
        }
        private void PanelSizeChange()
        {
            var xamlRoot = this.XamlRoot;
            if(xamlRoot != null)
            {
                // DPIスケール因子を取得
                double dpiRatio = xamlRoot.RasterizationScale;
                Console.WriteLine(dpiRatio);
                var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
                var width = (int) (this.ActualWidth * dpiRatio);
                var height = (int) (this.ActualHeight * dpiRatio);
                Point position = this.TransformToVisual(xamlRoot.Content).TransformPoint(new Point(0, 0));
                var posX = (int) (position.X * dpiRatio);
                var posY = (int) (position.Y * dpiRatio);
                SetWindowPos(terminalHandle, IntPtr.Zero, posX, posY, width, height, 0);
            }            
        }
    }
}
