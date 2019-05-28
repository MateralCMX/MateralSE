namespace VRage.Win32
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Unsharper;
    using VRage.Library.Utils;

    [UnsharperDisableReflection]
    public static class WinApi
    {
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;
        public const int GW_CHILD = 5;
        public const int GW_ENABLEDPOPUP = 6;
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_REMOVE = 0x1000;
        public const int WM_DEVICECHANGE = 0x219;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 260;
        public const int WM_SYSKEYUP = 0x105;
        private static Func<long> m_workingSetDelegate;

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool AllocConsole();
        [DllImport("kernel32", SetLastError=true)]
        public static extern bool AttachConsole(int dwProcessId);
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [SuppressUnmanagedCodeSecurity, DllImport("msvcrt.dll", EntryPoint="memcpy", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe void* CopyMemory(void* dest, void* src, ulong count);
        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string pName);
        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);
        [DllImport("user32.dll")]
        public static extern bool DrawMenuBar(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        public static IntPtr FindChildWindow(IntPtr windowHandle, string childName)
        {
            IntPtr hWnd = FindWindowEx(windowHandle, IntPtr.Zero, null, childName);
            int num = windowHandle.ToInt32();
            if (hWnd != IntPtr.Zero)
            {
                return hWnd;
            }
            IntPtr zero = IntPtr.Zero;
            for (hWnd = GetWindow(windowHandle, 5); (zero != hWnd) && (hWnd != IntPtr.Zero); hWnd = GetWindow(hWnd, 2))
            {
                if (zero == IntPtr.Zero)
                {
                    zero = hWnd;
                }
                IntPtr ptr3 = FindChildWindow(hWnd, childName);
                if (ptr3 != IntPtr.Zero)
                {
                    return ptr3;
                }
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
        public static IntPtr FindWindowInParent(string parentName, string childName)
        {
            IntPtr windowHandle = FindWindow(null, parentName);
            return (!(windowHandle != IntPtr.Zero) ? IntPtr.Zero : FindChildWindow(windowHandle, childName));
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentProcessId();
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr GetKeyboardLayout(IntPtr threadId);
        [DllImport("user32.dll")]
        public static extern int GetMenuItemCount(IntPtr hMenu);
        [DllImport("user32.dll")]
        public static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        public static bool IsValidWindow(IntPtr windowHandle) => 
            IsWindow(windowHandle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr LoadKeyboardLayout(string keyboardLayoutID, uint flags);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint MapVirtualKeyEx(uint key, MAPVK mappingType, IntPtr keyboardLayout);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWndle, string text, string caption, int buttons);
        [DllImport("ntdll.dll")]
        public static extern NTSTATUS NtQueryTimerResolution(ref uint MinimumResolution, ref uint MaximumResolution, ref uint CurrentResolution);
        [DllImport("ntdll.dll")]
        public static extern NTSTATUS NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("user32.dll")]
        public static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        public static void SendMessage<T>(ref T data, IntPtr windowHandle, IntPtr sourceWindowHandle = null) where T: struct
        {
            try
            {
                int cb = Marshal.SizeOf<T>(data);
                IntPtr ptr = Marshal.AllocHGlobal(cb);
                Marshal.StructureToPtr<T>(data, ptr, true);
                MyCopyData lParam = new MyCopyData {
                    DataSize = cb,
                    DataPointer = ptr
                };
                SendMessage(windowHandle, 0x4a, sourceWindowHandle, ref lParam);
                Marshal.FreeHGlobal(ptr);
            }
            catch (Exception)
            {
            }
        }

        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref MyCopyData lParam);
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(ConsoleEventHandler handler, bool add);
        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();
        [DllImport("kernel32")]
        public static extern bool SetProcessWorkingSetSize(IntPtr handle, int minSize, int maxSize);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bVisible);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool UnloadKeyboardLayout(IntPtr handle);

        public static long WorkingSet
        {
            get
            {
                if (m_workingSetDelegate == null)
                {
                    long workingSet = Environment.WorkingSet;
                    MethodInfo method = typeof(Environment).GetMethod("GetWorkingSet", BindingFlags.NonPublic | BindingFlags.Static);
                    m_workingSetDelegate = (Func<long>) Delegate.CreateDelegate(typeof(Func<long>), method);
                }
                return m_workingSetDelegate();
            }
        }

        public delegate bool ConsoleEventHandler(WinApi.CtrlType sig);

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceChangeHookStruct
        {
            public int lParam;
            public int wParam;
            public int message;
            public int hwnd;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DEVMODE
        {
            public const int CCHDEVICENAME = 0x20;
            public const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20), FieldOffset(0)]
            public string dmDeviceName;
            [FieldOffset(0x20)]
            public short dmSpecVersion;
            [FieldOffset(0x22)]
            public short dmDriverVersion;
            [FieldOffset(0x24)]
            public short dmSize;
            [FieldOffset(0x26)]
            public short dmDriverExtra;
            [FieldOffset(40)]
            public WinApi.DM dmFields;
            [FieldOffset(0x2c)]
            private short dmOrientation;
            [FieldOffset(0x2e)]
            private short dmPaperSize;
            [FieldOffset(0x30)]
            private short dmPaperLength;
            [FieldOffset(50)]
            private short dmPaperWidth;
            [FieldOffset(0x34)]
            private short dmScale;
            [FieldOffset(0x36)]
            private short dmCopies;
            [FieldOffset(0x38)]
            private short dmDefaultSource;
            [FieldOffset(0x3a)]
            private short dmPrintQuality;
            [FieldOffset(0x2c)]
            public WinApi.POINTL dmPosition;
            [FieldOffset(0x34)]
            public int dmDisplayOrientation;
            [FieldOffset(0x38)]
            public int dmDisplayFixedOutput;
            [FieldOffset(60)]
            public short dmColor;
            [FieldOffset(0x3e)]
            public short dmDuplex;
            [FieldOffset(0x40)]
            public short dmYResolution;
            [FieldOffset(0x42)]
            public short dmTTOption;
            [FieldOffset(0x44)]
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20), FieldOffset(0x48)]
            public string dmFormName;
            [FieldOffset(0x66)]
            public short dmLogPixels;
            [FieldOffset(0x68)]
            public int dmBitsPerPel;
            [FieldOffset(0x6c)]
            public int dmPelsWidth;
            [FieldOffset(0x70)]
            public int dmPelsHeight;
            [FieldOffset(0x74)]
            public int dmDisplayFlags;
            [FieldOffset(0x74)]
            public int dmNup;
            [FieldOffset(120)]
            public int dmDisplayFrequency;
        }

        [Flags]
        public enum DM
        {
            Orientation = 1,
            PaperSize = 2,
            PaperLength = 4,
            PaperWidth = 8,
            Scale = 0x10,
            Position = 0x20,
            NUP = 0x40,
            DisplayOrientation = 0x80,
            Copies = 0x100,
            DefaultSource = 0x200,
            PrintQuality = 0x400,
            Color = 0x800,
            Duplex = 0x1000,
            YResolution = 0x2000,
            TTOption = 0x4000,
            Collate = 0x8000,
            FormName = 0x10000,
            LogPixels = 0x20000,
            BitsPerPixel = 0x40000,
            PelsWidth = 0x80000,
            PelsHeight = 0x100000,
            DisplayFlags = 0x200000,
            DisplayFrequency = 0x400000,
            ICMMethod = 0x800000,
            ICMIntent = 0x1000000,
            MediaType = 0x2000000,
            DitherType = 0x4000000,
            PanningWidth = 0x8000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public enum HookType
        {
            WH_JOURNALRECORD,
            WH_JOURNALPLAYBACK,
            WH_KEYBOARD,
            WH_GETMESSAGE,
            WH_CALLWNDPROC,
            WH_CBT,
            WH_SYSMSGFILTER,
            WH_MOUSE,
            WH_HARDWARE,
            WH_DEBUG,
            WH_SHELL,
            WH_FOREGROUNDIDLE,
            WH_CALLWNDPROCRET,
            WH_KEYBOARD_LL,
            WH_MOUSE_LL
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        internal enum MAPVK : uint
        {
            VK_TO_VSC = 0,
            VSC_TO_VK = 1,
            VK_TO_CHAR = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength = ((uint) Marshal.SizeOf(typeof(WinApi.MEMORYSTATUSEX)));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public WinApi.POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyCopyData
        {
            public IntPtr Data;
            public int DataSize;
            public IntPtr DataPointer;
        }

        public enum NTSTATUS : uint
        {
            STATUS_SUCCESS = 0,
            STATUS_TIMER_RESOLUTION_NOT_SET = 0xc0000245
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            public int x;
            public int y;
        }

        public enum SystemCommands
        {
            SC_SIZE = 0xf000,
            SC_MOVE = 0xf010,
            SC_MINIMIZE = 0xf020,
            SC_MAXIMIZE = 0xf030,
            SC_MAXIMIZE2 = 0xf032,
            SC_NEXTWINDOW = 0xf040,
            SC_PREVWINDOW = 0xf050,
            SC_CLOSE = 0xf060,
            SC_VSCROLL = 0xf070,
            SC_HSCROLL = 0xf080,
            SC_MOUSEMENU = 0xf090,
            SC_KEYMENU = 0xf100,
            SC_ARRANGE = 0xf110,
            SC_RESTORE = 0xf120,
            SC_RESTORE2 = 0xf122,
            SC_TASKLIST = 0xf130,
            SC_SCREENSAVE = 0xf140,
            SC_HOTKEY = 0xf150,
            SC_DEFAULT = 0xf160,
            SC_MONITORPOWER = 0xf170,
            SC_CONTEXTHELP = 0xf180,
            SC_SEPARATOR = 0xf00f
        }

        [DontCheck]
        public enum WM
        {
            NULL = 0,
            CREATE = 1,
            DESTROY = 2,
            MOVE = 3,
            SIZE = 5,
            ACTIVATE = 6,
            SETFOCUS = 7,
            KILLFOCUS = 8,
            ENABLE = 10,
            SETREDRAW = 11,
            SETTEXT = 12,
            GETTEXT = 13,
            GETTEXTLENGTH = 14,
            PAINT = 15,
            CLOSE = 0x10,
            QUERYENDSESSION = 0x11,
            QUERYOPEN = 0x13,
            ENDSESSION = 0x16,
            QUIT = 0x12,
            ERASEBKGND = 20,
            SYSCOLORCHANGE = 0x15,
            SHOWWINDOW = 0x18,
            WININICHANGE = 0x1a,
            SETTINGCHANGE = 0x1a,
            DEVMODECHANGE = 0x1b,
            ACTIVATEAPP = 0x1c,
            FONTCHANGE = 0x1d,
            TIMECHANGE = 30,
            CANCELMODE = 0x1f,
            SETCURSOR = 0x20,
            MOUSEACTIVATE = 0x21,
            CHILDACTIVATE = 0x22,
            QUEUESYNC = 0x23,
            GETMINMAXINFO = 0x24,
            PAINTICON = 0x26,
            ICONERASEBKGND = 0x27,
            NEXTDLGCTL = 40,
            SPOOLERSTATUS = 0x2a,
            DRAWITEM = 0x2b,
            MEASUREITEM = 0x2c,
            DELETEITEM = 0x2d,
            VKEYTOITEM = 0x2e,
            CHARTOITEM = 0x2f,
            SETFONT = 0x30,
            GETFONT = 0x31,
            SETHOTKEY = 50,
            GETHOTKEY = 0x33,
            QUERYDRAGICON = 0x37,
            COMPAREITEM = 0x39,
            GETOBJECT = 0x3d,
            COMPACTING = 0x41,
            [Obsolete]
            COMMNOTIFY = 0x44,
            WINDOWPOSCHANGING = 70,
            WINDOWPOSCHANGED = 0x47,
            [Obsolete]
            POWER = 0x48,
            COPYDATA = 0x4a,
            CANCELJOURNAL = 0x4b,
            NOTIFY = 0x4e,
            INPUTLANGCHANGEREQUEST = 80,
            INPUTLANGCHANGE = 0x51,
            TCARD = 0x52,
            HELP = 0x53,
            USERCHANGED = 0x54,
            NOTIFYFORMAT = 0x55,
            CONTEXTMENU = 0x7b,
            STYLECHANGING = 0x7c,
            STYLECHANGED = 0x7d,
            DISPLAYCHANGE = 0x7e,
            GETICON = 0x7f,
            SETICON = 0x80,
            NCCREATE = 0x81,
            NCDESTROY = 130,
            NCCALCSIZE = 0x83,
            NCHITTEST = 0x84,
            NCPAINT = 0x85,
            NCACTIVATE = 0x86,
            GETDLGCODE = 0x87,
            SYNCPAINT = 0x88,
            NCMOUSEMOVE = 160,
            NCLBUTTONDOWN = 0xa1,
            NCLBUTTONUP = 0xa2,
            NCLBUTTONDBLCLK = 0xa3,
            NCRBUTTONDOWN = 0xa4,
            NCRBUTTONUP = 0xa5,
            NCRBUTTONDBLCLK = 0xa6,
            NCMBUTTONDOWN = 0xa7,
            NCMBUTTONUP = 0xa8,
            NCMBUTTONDBLCLK = 0xa9,
            NCXBUTTONDOWN = 0xab,
            NCXBUTTONUP = 0xac,
            NCXBUTTONDBLCLK = 0xad,
            INPUT_DEVICE_CHANGE = 0xfe,
            INPUT = 0xff,
            KEYFIRST = 0x100,
            KEYDOWN = 0x100,
            KEYUP = 0x101,
            CHAR = 0x102,
            DEADCHAR = 0x103,
            SYSKEYDOWN = 260,
            SYSKEYUP = 0x105,
            SYSCHAR = 0x106,
            SYSDEADCHAR = 0x107,
            UNICHAR = 0x109,
            KEYLAST = 0x109,
            IME_STARTCOMPOSITION = 0x10d,
            IME_ENDCOMPOSITION = 270,
            IME_COMPOSITION = 0x10f,
            IME_KEYLAST = 0x10f,
            INITDIALOG = 0x110,
            COMMAND = 0x111,
            SYSCOMMAND = 0x112,
            TIMER = 0x113,
            HSCROLL = 0x114,
            VSCROLL = 0x115,
            INITMENU = 0x116,
            INITMENUPOPUP = 0x117,
            MENUSELECT = 0x11f,
            MENUCHAR = 0x120,
            ENTERIDLE = 0x121,
            MENURBUTTONUP = 290,
            MENUDRAG = 0x123,
            MENUGETOBJECT = 0x124,
            UNINITMENUPOPUP = 0x125,
            MENUCOMMAND = 0x126,
            CHANGEUISTATE = 0x127,
            UPDATEUISTATE = 0x128,
            QUERYUISTATE = 0x129,
            CTLCOLORMSGBOX = 0x132,
            CTLCOLOREDIT = 0x133,
            CTLCOLORLISTBOX = 0x134,
            CTLCOLORBTN = 0x135,
            CTLCOLORDLG = 310,
            CTLCOLORSCROLLBAR = 0x137,
            CTLCOLORSTATIC = 0x138,
            MOUSEFIRST = 0x200,
            MOUSEMOVE = 0x200,
            LBUTTONDOWN = 0x201,
            LBUTTONUP = 0x202,
            LBUTTONDBLCLK = 0x203,
            RBUTTONDOWN = 0x204,
            RBUTTONUP = 0x205,
            RBUTTONDBLCLK = 0x206,
            MBUTTONDOWN = 0x207,
            MBUTTONUP = 520,
            MBUTTONDBLCLK = 0x209,
            MOUSEWHEEL = 0x20a,
            XBUTTONDOWN = 0x20b,
            XBUTTONUP = 0x20c,
            XBUTTONDBLCLK = 0x20d,
            MOUSEHWHEEL = 0x20e,
            MOUSELAST = 0x20e,
            PARENTNOTIFY = 0x210,
            ENTERMENULOOP = 0x211,
            EXITMENULOOP = 530,
            NEXTMENU = 0x213,
            SIZING = 0x214,
            CAPTURECHANGED = 0x215,
            MOVING = 0x216,
            POWERBROADCAST = 0x218,
            DEVICECHANGE = 0x219,
            MDICREATE = 0x220,
            MDIDESTROY = 0x221,
            MDIACTIVATE = 0x222,
            MDIRESTORE = 0x223,
            MDINEXT = 0x224,
            MDIMAXIMIZE = 0x225,
            MDITILE = 550,
            MDICASCADE = 0x227,
            MDIICONARRANGE = 0x228,
            MDIGETACTIVE = 0x229,
            MDISETMENU = 560,
            ENTERSIZEMOVE = 0x231,
            EXITSIZEMOVE = 0x232,
            DROPFILES = 0x233,
            MDIREFRESHMENU = 0x234,
            IME_SETCONTEXT = 0x281,
            IME_NOTIFY = 0x282,
            IME_CONTROL = 0x283,
            IME_COMPOSITIONFULL = 0x284,
            IME_SELECT = 0x285,
            IME_CHAR = 0x286,
            IME_REQUEST = 0x288,
            IME_KEYDOWN = 0x290,
            IME_KEYUP = 0x291,
            MOUSEHOVER = 0x2a1,
            MOUSELEAVE = 0x2a3,
            NCMOUSEHOVER = 0x2a0,
            NCMOUSELEAVE = 0x2a2,
            WTSSESSION_CHANGE = 0x2b1,
            TABLET_FIRST = 0x2c0,
            TABLET_LAST = 0x2df,
            CUT = 0x300,
            COPY = 0x301,
            PASTE = 770,
            CLEAR = 0x303,
            UNDO = 0x304,
            RENDERFORMAT = 0x305,
            RENDERALLFORMATS = 0x306,
            DESTROYCLIPBOARD = 0x307,
            DRAWCLIPBOARD = 0x308,
            PAINTCLIPBOARD = 0x309,
            VSCROLLCLIPBOARD = 0x30a,
            SIZECLIPBOARD = 0x30b,
            ASKCBFORMATNAME = 780,
            CHANGECBCHAIN = 0x30d,
            HSCROLLCLIPBOARD = 0x30e,
            QUERYNEWPALETTE = 0x30f,
            PALETTEISCHANGING = 0x310,
            PALETTECHANGED = 0x311,
            HOTKEY = 0x312,
            PRINT = 0x317,
            PRINTCLIENT = 0x318,
            APPCOMMAND = 0x319,
            THEMECHANGED = 0x31a,
            CLIPBOARDUPDATE = 0x31d,
            DWMCOMPOSITIONCHANGED = 0x31e,
            DWMNCRENDERINGCHANGED = 0x31f,
            DWMCOLORIZATIONCOLORCHANGED = 800,
            DWMWINDOWMAXIMIZEDCHANGE = 0x321,
            GETTITLEBARINFOEX = 0x33f,
            HANDHELDFIRST = 0x358,
            HANDHELDLAST = 0x35f,
            AFXFIRST = 0x360,
            AFXLAST = 0x37f,
            PENWINFIRST = 0x380,
            PENWINLAST = 0x38f,
            APP = 0x8000,
            USER = 0x400,
            CPL_LAUNCH = 0x1400,
            CPL_LAUNCHED = 0x1401,
            SYSTIMER = 280
        }
    }
}

