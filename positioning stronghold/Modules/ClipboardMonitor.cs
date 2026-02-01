using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace positioning_stronghold
{
    // 剪贴板监视器类，用于监视系统剪贴板的变化
    // 实现IDisposable接口，确保资源正确释放
    public class ClipboardMonitor : IDisposable
    {
        private IntPtr _windowHandle;
        private HwndSource _source;
        private Action<string> _onClipboardChanged;

        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll")]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        // 初始化剪贴板监视器
        public ClipboardMonitor(Window window, Action<string> onClipboardChanged)
        {
            _onClipboardChanged = onClipboardChanged;
            _windowHandle = new WindowInteropHelper(window).EnsureHandle();
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(WndProc);
            AddClipboardFormatListener(_windowHandle);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        // 窗口过程回调函数，处理剪贴板更新消息
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        string text = Clipboard.GetText();
                        _onClipboardChanged?.Invoke(text);
                    }
                }
                catch
                {
                }
            }
            return IntPtr.Zero;
        }

        // 释放资源
        public void Dispose()
        {
            if (_source != null)
            {
                _source.RemoveHook(WndProc);
                _source = null;
            }
        }
    }
}