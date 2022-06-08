using FlowerMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace FlowerMaster
{

    /// <summary>
    /// CordWindow.xaml 的交互逻辑
    /// </summary>
    public class CordCol
    {


        //大量API引用
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Win32Point pt);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool EnumChildWindows(IntPtr window, EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

        private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        /// <summary>
        /// 用于获取鼠标坐标的数据结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// 获取鼠标坐标
        /// </summary>
        /// <returns>当前鼠标坐标（绝对值）</returns>
        public static System.Drawing.Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(out w32Mouse);
            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }

        /// <summary>
        /// 长方形数据结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        /// <summary>
        /// 获取最底层WebHandle用来抓取屏幕
        /// Credits to https://stackoverflow.com/questions/17378973/get-window-hwnd-if-owner-hwnd-class-name-and-size-of-the-window-are-known
        /// </summary>
        /// <param name="Top">顶层Handle</param>
        /// <param name="className">Optional - class name of the desired handle</param>
        /// <returns></returns>
        public static IntPtr GetWebHandle(IntPtr Top, string className = "Chrome_RenderWidgetHostHWND")
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentOutOfRangeException("className", className, "className can't be null or blank.");
            }

            return WindowsByClassFinder.WindowsMatching(className, Top);
        }

        /// <summary>
        /// Returns a sequence of window handles (IntPtrs) for all top-level windows
        /// matching the specfied window class name.
        /// </summary>
        /// <param name="className">The windows class name to match (not to be confused with a C# class name!)</param>
        /// <returns>A non-null sequence of window handles. This will be an empty sequence if no windows match the class name.</returns>

        /// <summary>Finds windows matching a particular window class name.</summary>

        private class WindowsByClassFinder
        {
            /// <summary>Find the windows matching the specified class name.</summary>

            public static IntPtr WindowsMatching(string className, IntPtr Top)
            {
                return new WindowsByClassFinder(className, Top)._result;
            }

            private WindowsByClassFinder(string className, IntPtr Top)
            {
                _className = className;
                EnumChildWindows(Top, Callback, IntPtr.Zero);
            }

            private bool Callback(IntPtr hWnd, IntPtr lparam)
            {
                if (GetClassName(hWnd, _apiResult, _apiResult.Capacity) != 0)
                {
                    if (string.CompareOrdinal(_apiResult.ToString(), _className) == 0)
                    {
                        _result = hWnd;
                    }
                }

                return true; // Keep enumerating.
            }

            private readonly string _className;
            private IntPtr _result;
            private readonly StringBuilder _apiResult = new StringBuilder(1024);
        }

        /// <summary>
        /// 获取坐标像素颜色
        /// </summary>
        /// <param name="hwnd">Handle（需要使用最底层）</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns></returns>
        static public System.Drawing.Color GetPixelColor(IntPtr hwnd, int x, int y)
        {
            IntPtr hdc = GetDC(hwnd);
            uint pixel = GetPixel(hdc, x, y);
            System.Drawing.Color color = System.Drawing.Color.FromArgb(
                (Byte)(pixel),
                (Byte)(pixel >> 8),
                (Byte)(pixel >> 16));
            ReleaseDC(hwnd, hdc);
            return color;
        }
        
        /// <summary>
        /// 获取当前鼠标坐标
        /// </summary>
        public CordCol(IntPtr WebHandle)
        {
            System.Drawing.Point Pointy = GetMousePosition();
            GetWindowRect(WebHandle, out RECT lprect);
            
            Pointy.X -= lprect.Left;
            Pointy.Y -= lprect.Top;
            
            System.Drawing.Color Color = GetPixelColor(WebHandle, Pointy.X - lprect.Left, Pointy.Y - lprect.Top);
            
        }

        public System.Drawing.Point Pointy { get; }
        public System.Drawing.Color Color { get; }

    }
}
