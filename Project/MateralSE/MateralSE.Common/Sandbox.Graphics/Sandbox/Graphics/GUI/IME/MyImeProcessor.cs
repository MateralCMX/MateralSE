namespace Sandbox.Graphics.GUI.IME
{
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui.IME;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using VRage.Collections;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public sealed class MyImeProcessor
    {
        private const int WH_KEYBOARD_LL = 13;
        private static IntPtr hookID = IntPtr.Zero;
        private LowLevelKeyboardProc hookProc;
        private readonly int WM_IME_STARTCOMPOSITION = 0x10d;
        private static MyImeProcessor instance;
        private bool m_isEnabled;
        private int m_textLimit;
        private int m_charsWritten;
        private bool m_isActive;
        private bool m_isComposing;
        private string m_compositionString = string.Empty;
        private string[] m_candidates;
        private IMyImeActiveControl m_activeTextElement;
        private MyGuiControlIme m_guiControlElement;
        private MyGuiScreenBase m_activeScreen;
        private MyGuiControlContextMenu m_candidateList = new MyGuiControlContextMenu();
        private readonly MyConcurrentQueue<MyDel> m_invokeQueue = new MyConcurrentQueue<MyDel>(0x20);

        private MyImeProcessor()
        {
            this.hookProc = new LowLevelKeyboardProc(this.HookKeyboardCallback);
            hookID = SetHook(this.hookProc);
            this.m_candidateList.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.CandidateClicked);
            this.m_candidateList.CreateNewContextMenu();
        }

        public void Activate(IMyImeActiveControl textElement)
        {
            if (this.m_isEnabled)
            {
                this.m_activeTextElement = textElement;
                this.m_textLimit = Math.Max(0, (this.m_activeTextElement.GetMaxLength() - this.m_activeTextElement.GetTextLength()) + this.m_activeTextElement.GetSelectionLength());
                this.m_charsWritten = 0;
                this.m_isActive = true;
                this.m_guiControlElement.ActivateIme();
            }
        }

        private void AddCandidListToActiveScreen()
        {
            if ((this.m_activeScreen != null) && !this.m_activeScreen.Controls.Contains(this.m_candidateList))
            {
                this.m_activeScreen.Controls.Add(this.m_candidateList);
                this.m_candidateList.Deactivate();
            }
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        private void CandidateClicked(MyGuiControlContextMenu sender, MyGuiControlContextMenu.EventArgs eventArgs)
        {
            this.RejectComposition(true);
            int itemIndex = eventArgs.ItemIndex;
            string str = this.m_candidates[itemIndex];
            this.InvokeCharacterMultiple(true, str.Substring(0, str.Length - 1), true);
            if (str[str.Length - 1] != ' ')
            {
                this.InvokeCharacter(true, str[str.Length - 1], true);
            }
            if (this.m_isEnabled)
            {
                this.m_guiControlElement.DeactivateIme();
                this.m_guiControlElement.ActivateIme();
            }
        }

        public void CaretRepositionReaction()
        {
            this.ConfirmComposition();
            if (this.m_isComposing && this.m_isEnabled)
            {
                this.m_guiControlElement.DeactivateIme();
                this.m_guiControlElement.ActivateIme();
            }
        }

        private void ConfirmComposition()
        {
            this.m_compositionString = string.Empty;
        }

        public static void CreateInstance()
        {
            instance = new MyImeProcessor();
        }

        private string CurrentCompStr(IntPtr handle)
        {
            string str;
            IntPtr hIMC = ImmGetContext(handle);
            try
            {
                int dwIndex = 8;
                int dwBufLen = ImmGetCompositionStringW(hIMC, dwIndex, null, 0);
                if (dwBufLen <= 0)
                {
                    str = string.Empty;
                }
                else
                {
                    byte[] lpBuf = new byte[dwBufLen];
                    ImmGetCompositionStringW(hIMC, dwIndex, lpBuf, dwBufLen);
                    str = Encoding.Unicode.GetString(lpBuf);
                }
            }
            finally
            {
                ImmReleaseContext(handle, hIMC);
            }
            return str;
        }

        public void Deactivate()
        {
            if (this.m_isEnabled)
            {
                if ((this.m_activeScreen != null) && this.m_candidateList.IsGuiControlEqual(this.m_activeScreen.FocusedControl))
                {
                    this.m_activeScreen.FocusedControl = this.m_activeTextElement as MyGuiControlBase;
                }
                else
                {
                    this.RejectComposition(false);
                    this.m_isComposing = false;
                    if (this.m_activeTextElement != null)
                    {
                        this.InvokeDeactivation();
                    }
                    this.m_isActive = false;
                    this.m_activeTextElement = null;
                    if (this.m_isEnabled)
                    {
                        this.m_guiControlElement.DeactivateIme();
                    }
                    this.m_candidateList.Deactivate();
                }
            }
        }

        private void EvtComposition()
        {
            if (this.m_isComposing)
            {
                this.UpdateCandidateList();
            }
        }

        private void EvtCompositionEnd()
        {
            this.RejectComposition(false);
            this.m_isComposing = false;
            if (this.m_activeTextElement != null)
            {
                this.InvokeDeactivation();
            }
            this.m_candidateList.Deactivate();
        }

        private void EvtCompositionStart()
        {
            this.m_isComposing = true;
            if (this.m_isEnabled)
            {
                SetCompositionWindow(this.m_guiControlElement.Handle);
            }
            this.AddCandidListToActiveScreen();
            if (this.m_activeTextElement == null)
            {
                this.m_textLimit = this.m_charsWritten = 0;
            }
            else
            {
                this.m_activeTextElement.IsImeActive = true;
                this.m_textLimit = Math.Max(0, (this.m_activeTextElement.GetMaxLength() - this.m_activeTextElement.GetTextLength()) + this.m_activeTextElement.GetSelectionLength());
                this.m_charsWritten = 0;
            }
            if (this.m_textLimit == 0)
            {
                this.EvtCompositionEnd();
                this.m_guiControlElement.DeactivateIme();
                this.m_guiControlElement.ActivateIme();
            }
        }

        private string[] GetAllCandidateList(IntPtr hwnd)
        {
            string[] strArray = null;
            try
            {
                IntPtr himc = ImmGetContext(hwnd);
                if (himc != IntPtr.Zero)
                {
                    CANDIDATELIST candidatelist1 = new CANDIDATELIST();
                    int cb = ImmGetCandidateList(himc, 0, IntPtr.Zero, 0);
                    if (cb > 0x1c)
                    {
                        CANDIDATELIST structure = new CANDIDATELIST();
                        IntPtr lpCandidateList = Marshal.AllocHGlobal(cb);
                        ImmGetCandidateList(himc, 0, lpCandidateList, cb);
                        Marshal.PtrToStructure<CANDIDATELIST>(lpCandidateList, structure);
                        byte[] destination = new byte[cb];
                        Marshal.Copy(lpCandidateList, destination, 0, cb);
                        Marshal.FreeHGlobal(lpCandidateList);
                        int dwOffset = structure.dwOffset;
                        char[] separator = "\0".ToCharArray();
                        string text2 = Encoding.Unicode.GetString(destination, dwOffset, (destination.Length - dwOffset) - 2);
                        strArray = text2.Split(separator);
                        ImmReleaseContext(hwnd, himc);
                        if (!string.IsNullOrEmpty(text2))
                        {
                            return strArray;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return new string[0];
        }

        private void GetCandidateWindow(IntPtr hwnd)
        {
            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC != IntPtr.Zero)
            {
                CANDIDATEFORM structure = new CANDIDATEFORM {
                    dwIndex = 0
                };
                int cb = Marshal.SizeOf(typeof(CANDIDATEFORM));
                IntPtr lpCandidate = Marshal.AllocHGlobal(cb);
                ImmGetCandidateWindow(hIMC, 0, lpCandidate);
                Marshal.PtrToStructure<CANDIDATEFORM>(lpCandidate, structure);
                Marshal.FreeHGlobal(lpCandidate);
            }
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private IntPtr HookKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (((nCode >= 0) && ((wParam == ((IntPtr) 0x100)) && (Marshal.ReadInt32(lParam) == 0x1b))) && this.IsComposing)
            {
                MyDirectXInput @static = MyInput.Static as MyDirectXInput;
                if (@static != null)
                {
                    @static.NegateEscapePress();
                }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        [DllImport("imm32.dll", EntryPoint="ImmGetCandidateListW")]
        public static extern int ImmGetCandidateList(IntPtr himc, int deIndex, IntPtr lpCandidateList, int dwBufLen);
        [DllImport("imm32.dll")]
        public static extern int ImmGetCandidateWindow(IntPtr hIMC, int dwIndex, IntPtr lpCandidate);
        [DllImport("imm32.dll", CharSet=CharSet.Unicode)]
        public static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("imm32.dll")]
        public static extern bool ImmNotifyIME(IntPtr hIMC, int dwAction, int dwIndex, int dwValue);
        [DllImport("imm32.dll")]
        public static extern int ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("imm32.dll")]
        public static extern bool ImmSetCandidateWindow(IntPtr hIMC, ref CANDIDATEFORM form);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("imm32.dll")]
        public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref CompositionForm form);
        private void InvokeBackspace(bool compositionEnd, bool check = false)
        {
            if (this.m_activeTextElement != null)
            {
                if (!check)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressBackspace(compositionEnd)));
                }
                else if (this.m_charsWritten > 0)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressBackspace(compositionEnd)));
                }
            }
        }

        private void InvokeBackspaceMultiple(bool compositionEnd, int count, bool check = false)
        {
            if ((this.m_activeTextElement != null) && (count > 0))
            {
                if (!check)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressBackspaceMultiple(compositionEnd, count)));
                }
                else
                {
                    int min = Math.Min(this.m_charsWritten, count);
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressBackspaceMultiple(compositionEnd, min)));
                    this.m_charsWritten -= min;
                }
            }
        }

        private void InvokeCharacter(bool compositionEnd, char character, bool check = false)
        {
            if (this.m_activeTextElement != null)
            {
                if (!check)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.InsertChar(compositionEnd, character)));
                }
                else if (this.m_charsWritten < this.m_textLimit)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.InsertChar(compositionEnd, character)));
                    this.m_charsWritten++;
                }
            }
        }

        private void InvokeCharacterMultiple(bool compositionEnd, string chars, bool check = false)
        {
            if ((this.m_activeTextElement != null) && !string.IsNullOrEmpty(chars))
            {
                if (!check)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.InsertCharMultiple(compositionEnd, chars)));
                }
                else if ((this.m_charsWritten + chars.Length) <= this.m_textLimit)
                {
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.InsertCharMultiple(compositionEnd, chars)));
                    this.m_charsWritten += chars.Length;
                }
                else
                {
                    string str = chars.Substring(0, this.m_textLimit - this.m_charsWritten);
                    this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.InsertCharMultiple(compositionEnd, str)));
                    this.m_charsWritten += str.Length;
                }
            }
        }

        private void InvokeDeactivation()
        {
            if (this.m_activeTextElement != null)
            {
                this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.DeactivateIme()));
            }
        }

        private void InvokeDelete(bool compositionEnd, char character, bool check = false)
        {
            if (this.m_activeTextElement != null)
            {
                this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressDelete(compositionEnd)));
            }
        }

        private void InvokeEnter(bool compositionEnd)
        {
            if (this.m_activeTextElement != null)
            {
                this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressEnter(compositionEnd)));
            }
        }

        private void InvokeRedo()
        {
            if (this.m_activeTextElement != null)
            {
                this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressRedo()));
            }
        }

        private void InvokeUndo()
        {
            if (this.m_activeTextElement != null)
            {
                this.QueueInvoke(new MyDel(this.m_activeTextElement, x => x.KeypressUndo()));
            }
        }

        public void LanguageChanged()
        {
            if (this.m_isComposing)
            {
                this.StopComposing();
                this.m_guiControlElement.DeactivateIme();
                this.m_guiControlElement.ActivateIme();
            }
        }

        private unsafe void ModifyCandidateListVisuals(int wordCount, int maxWordLength, Vector2 newPosition, Vector2 carriagePosition)
        {
            this.m_candidateList.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            Vector2 vector = new Vector2(newPosition.X + carriagePosition.X, newPosition.Y + carriagePosition.Y);
            Vector2 listBoxSize = this.m_candidateList.GetListBoxSize();
            Vector2 vector1 = vector + listBoxSize;
            if (vector1.X > 1f)
            {
                vector.X = 1f - listBoxSize.X;
            }
            Vector2* vectorPtr1 = (Vector2*) ref vector;
            vectorPtr1->X = Math.Max(0f, vector.X);
            if ((vector1.Y > 1f) && (listBoxSize.Y < 0.5f))
            {
                float* singlePtr1 = (float*) ref vector.Y;
                singlePtr1[0] -= listBoxSize.Y + 0.03f;
            }
            this.m_candidateList.Position = vector;
        }

        private void ModifyVisibility()
        {
            if (this.m_candidates.Length == 0)
            {
                this.m_candidateList.Deactivate();
            }
            else
            {
                this.m_candidateList.Activate(false);
            }
        }

        public void ProcessInvoke()
        {
            MyDel del;
            while (this.m_invokeQueue.TryDequeue(out del))
            {
                del.Invoke();
            }
        }

        private void QueueInvoke(MyDel del)
        {
            MyConcurrentQueue<MyDel> invokeQueue = this.m_invokeQueue;
            lock (invokeQueue)
            {
                this.m_invokeQueue.Enqueue(del);
            }
        }

        public void RecaptureTopScreen(MyGuiScreenBase screenWithFocus)
        {
            if (((this.m_activeScreen == null) || ((this.m_activeScreen.State != MyGuiScreenState.OPENED) && (this.m_activeScreen.State != MyGuiScreenState.OPENING))) && (screenWithFocus != null))
            {
                this.RegisterActiveScreen(screenWithFocus);
            }
        }

        public void RegisterActiveScreen(MyGuiScreenBase screen)
        {
            if (!ReferenceEquals(this.m_activeScreen, screen))
            {
                if (this.m_activeScreen != null)
                {
                    this.UnregisterActiveScreen(this.m_activeScreen);
                }
                this.m_activeScreen = screen;
                this.AddCandidListToActiveScreen();
            }
        }

        private void RejectComposition(bool check = false)
        {
            if (this.m_activeTextElement != null)
            {
                this.InvokeBackspaceMultiple(false, this.m_compositionString.Length, check);
            }
            this.m_compositionString = string.Empty;
        }

        private void RemoveCandidListFromActiveScreen()
        {
            this.m_isComposing = false;
            this.m_isActive = false;
            this.m_activeTextElement = null;
            this.m_textLimit = 0;
            this.m_charsWritten = 0;
            if (this.m_isEnabled)
            {
                this.m_guiControlElement.DeactivateIme();
            }
            if (this.m_activeScreen != null)
            {
                this.m_activeScreen.Controls.Remove(this.m_candidateList);
            }
            this.m_candidateList.Deactivate();
        }

        private static bool SetCompositionWindow(IntPtr hwnd)
        {
            CompositionForm form = new CompositionForm {
                dwStyle = 0x20,
                ptCurrentPos = { 
                    x = 0x3a98,
                    y = 0
                },
                rcArea = { 
                    left = 0,
                    right = 0,
                    top = 0,
                    bottom = 0
                }
            };
            return ImmSetCompositionWindow(ImmGetContext(hwnd), ref form);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            IntPtr ptr;
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    ptr = SetWindowsHookEx(13, proc, GetModuleHandle(module.ModuleName), 0);
                }
            }
            return ptr;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        public void StopComposing()
        {
            this.EvtCompositionEnd();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        public void UnregisterActiveScreen(MyGuiScreenBase screen)
        {
            if ((this.m_activeScreen != null) && ReferenceEquals(this.m_activeScreen, screen))
            {
                if (this.m_activeScreen != null)
                {
                    this.m_activeScreen.Controls.Remove(this.m_candidateList);
                }
                this.m_activeScreen = null;
                this.Deactivate();
            }
        }

        private void UpdateCandidateList()
        {
            IntPtr zero = IntPtr.Zero;
            if (this.m_isEnabled)
            {
                zero = this.m_guiControlElement.Handle;
            }
            if (zero != IntPtr.Zero)
            {
                int length = this.m_compositionString.Length;
                if (this.m_activeTextElement != null)
                {
                    string str = this.CurrentCompStr(zero);
                    this.InvokeBackspaceMultiple(false, this.m_compositionString.Length, true);
                    if (!string.IsNullOrEmpty(str))
                    {
                        this.InvokeCharacterMultiple(false, str, true);
                    }
                    this.m_compositionString = str;
                }
                string[] allCandidateList = this.GetAllCandidateList(zero);
                this.m_candidates = allCandidateList;
                this.m_candidateList.Clear();
                int maxWordLength = 0;
                int index = 0;
                while (true)
                {
                    if (index >= allCandidateList.Length)
                    {
                        Vector2 cornerPosition;
                        Vector2 carriagePosition;
                        if (this.m_activeTextElement != null)
                        {
                            cornerPosition = this.m_activeTextElement.GetCornerPosition();
                            carriagePosition = this.m_activeTextElement.GetCarriagePosition(length);
                        }
                        else
                        {
                            cornerPosition = new Vector2(0f, 0f);
                            carriagePosition = new Vector2(0f, 0f);
                        }
                        this.ModifyCandidateListVisuals(allCandidateList.Length, maxWordLength, cornerPosition, carriagePosition);
                        this.ModifyVisibility();
                        break;
                    }
                    if (allCandidateList[index].Length > maxWordLength)
                    {
                        maxWordLength = allCandidateList[index].Length;
                    }
                    this.m_candidateList.AddItem(new StringBuilder().AppendFormat("{0}. {1}", index + 1, allCandidateList[index]), "", "", null);
                    index++;
                }
            }
        }

        public bool WndProc(ref Message m)
        {
            int msg = m.Msg;
            if (msg <= 0x51)
            {
                if (msg == 7)
                {
                    MyRenderProxy.HandleFocusMessage(MyWindowFocusMessage.SetFocus);
                }
                else if (msg == 0x51)
                {
                    this.LanguageChanged();
                }
            }
            else if (msg != 0x102)
            {
                switch (msg)
                {
                    case 0x10d:
                        this.EvtCompositionStart();
                        return false;

                    case 270:
                        this.EvtCompositionEnd();
                        return false;

                    case 0x10f:
                        this.EvtComposition();
                        return true;

                    default:
                        switch (msg)
                        {
                            case 0x281:
                            case 0x283:
                            case 0x284:
                            case 0x285:
                            case 0x288:
                            case 0x290:
                            case 0x291:
                                return false;

                            case 0x282:
                            case 0x286:
                                return true;

                            default:
                                break;
                        }
                        break;
                }
            }
            else if (this.m_activeTextElement != null)
            {
                char wParam = (char) ((int) m.WParam);
                if (wParam == '\b')
                {
                    this.InvokeBackspace(true, false);
                }
                else if (wParam == '\r')
                {
                    this.InvokeEnter(true);
                }
                else
                {
                    switch (wParam)
                    {
                        case '\x0019':
                            this.InvokeRedo();
                            break;

                        case '\x001a':
                            this.InvokeUndo();
                            break;

                        case '\x001b':
                            break;

                        default:
                            if (!MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.InvokeCharacter(true, (char) ((int) m.WParam), false);
                            }
                            break;
                    }
                }
                return true;
            }
            return true;
        }

        public static MyImeProcessor Instance =>
            instance;

        public bool IsEnabled =>
            this.m_isEnabled;

        public bool IsActive =>
            this.m_isActive;

        public bool IsTextElementConnected =>
            (this.m_activeTextElement != null);

        public MyGuiControlIme GuiImeControl
        {
            get => 
                this.m_guiControlElement;
            set
            {
                if (this.m_isEnabled)
                {
                    this.Deactivate();
                }
                this.m_guiControlElement = value;
                this.m_isEnabled = this.m_guiControlElement != null;
            }
        }

        public bool IsComposing =>
            this.m_isComposing;

        public MyGuiControlContextMenu CandidateList =>
            this.m_candidateList;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyImeProcessor.<>c <>9 = new MyImeProcessor.<>c();
            public static Action<IMyImeActiveControl> <>9__55_0;
            public static Action<IMyImeActiveControl> <>9__62_0;
            public static Action<IMyImeActiveControl> <>9__63_0;

            internal void <InvokeDeactivation>b__55_0(IMyImeActiveControl x)
            {
                x.DeactivateIme();
            }

            internal void <InvokeRedo>b__62_0(IMyImeActiveControl x)
            {
                x.KeypressRedo();
            }

            internal void <InvokeUndo>b__63_0(IMyImeActiveControl x)
            {
                x.KeypressUndo();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            public int VirtualKeyCode;
            public int ScanCode;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        internal class MyDel
        {
            public IMyImeActiveControl control;
            public Action<IMyImeActiveControl> action;

            public MyDel(IMyImeActiveControl ctrl, Action<IMyImeActiveControl> a)
            {
                this.control = ctrl;
                this.action = a;
            }

            public void Invoke()
            {
                this.action(this.control);
            }
        }
    }
}

