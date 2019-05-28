namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI.IME;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Input;
    using VRage.Utils;

    public static class MyScreenManager
    {
        private static readonly FastResourceLock lockObject = new FastResourceLock();
        public static int TotalGamePlayTimeInMilliseconds;
        private static MyGuiScreenBase m_lastScreenWithFocus;
        private static List<MyGuiScreenBase> m_screens;
        private static List<MyGuiScreenBase> m_screensToRemove;
        private static List<MyGuiScreenBase> m_screensToAdd;
        private static bool m_inputToNonFocusedScreens = false;
        private static bool m_wasInputToNonFocusedScreens = false;
        [CompilerGenerated]
        private static Action<MyGuiScreenBase> ScreenAdded;
        [CompilerGenerated]
        private static Action<MyGuiScreenBase> ScreenRemoved;
        private static StringBuilder m_sb = new StringBuilder(0x200);

        public static  event Action<MyGuiScreenBase> ScreenAdded
        {
            [CompilerGenerated] add
            {
                Action<MyGuiScreenBase> screenAdded = ScreenAdded;
                while (true)
                {
                    Action<MyGuiScreenBase> a = screenAdded;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Combine(a, value);
                    screenAdded = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref ScreenAdded, action3, a);
                    if (ReferenceEquals(screenAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiScreenBase> screenAdded = ScreenAdded;
                while (true)
                {
                    Action<MyGuiScreenBase> source = screenAdded;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Remove(source, value);
                    screenAdded = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref ScreenAdded, action3, source);
                    if (ReferenceEquals(screenAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyGuiScreenBase> ScreenRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyGuiScreenBase> screenRemoved = ScreenRemoved;
                while (true)
                {
                    Action<MyGuiScreenBase> a = screenRemoved;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Combine(a, value);
                    screenRemoved = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref ScreenRemoved, action3, a);
                    if (ReferenceEquals(screenRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiScreenBase> screenRemoved = ScreenRemoved;
                while (true)
                {
                    Action<MyGuiScreenBase> source = screenRemoved;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Remove(source, value);
                    screenRemoved = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref ScreenRemoved, action3, source);
                    if (ReferenceEquals(screenRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyScreenManager()
        {
            MyLog.Default.WriteLine("MyScreenManager()");
            m_screens = new List<MyGuiScreenBase>();
            m_screensToRemove = new List<MyGuiScreenBase>();
            m_screensToAdd = new List<MyGuiScreenBase>();
        }

        public static void AddScreen(MyGuiScreenBase screen)
        {
            screen.Closed += sender => RemoveScreen(sender);
            MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
            if (screenWithFocus != null)
            {
                screenWithFocus.HideTooltips();
            }
            MyGuiScreenBase base3 = null;
            if (screen.CanHideOthers)
            {
                base3 = GetPreviousScreen(null, x => x.CanBeHidden, x => x.CanHideOthers);
            }
            if ((base3 != null) && (base3.State != MyGuiScreenState.CLOSING))
            {
                base3.HideScreen();
            }
            MyInput.Static.JoystickAsMouse = screen.JoystickAsMouse;
            m_screensToAdd.Add(screen);
        }

        public static void AddScreenNow(MyGuiScreenBase screen)
        {
            screen.Closed += sender => RemoveScreen(sender);
            MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
            if (screenWithFocus != null)
            {
                screenWithFocus.HideTooltips();
            }
            MyGuiScreenBase base3 = null;
            if (screen.CanHideOthers)
            {
                base3 = GetPreviousScreen(null, x => x.CanBeHidden, x => x.CanHideOthers);
            }
            if ((base3 != null) && (base3.State != MyGuiScreenState.CLOSING))
            {
                base3.HideScreen();
            }
            if (!screen.IsLoaded)
            {
                screen.State = MyGuiScreenState.OPENING;
                screen.LoadData();
                screen.LoadContent();
            }
            if (screen.IsAlwaysFirst())
            {
                m_screens.Insert(0, screen);
            }
            else
            {
                m_screens.Insert(GetIndexOfLastNonTopScreen(), screen);
            }
        }

        private static void AddScreens()
        {
            for (int i = 0; i < m_screensToAdd.Count; i++)
            {
                MyGuiScreenBase item = m_screensToAdd[i];
                if (!item.IsLoaded)
                {
                    item.State = MyGuiScreenState.OPENING;
                    item.LoadData();
                    item.LoadContent();
                }
                if (item.IsAlwaysFirst())
                {
                    m_screens.Insert(0, item);
                }
                else
                {
                    m_screens.Insert(GetIndexOfLastNonTopScreen(), item);
                }
                NotifyScreenAdded(item);
            }
            m_screensToAdd.Clear();
        }

        public static void ClearLastScreenWithFocus()
        {
            m_lastScreenWithFocus = null;
        }

        public static void CloseAllScreensExcept(MyGuiScreenBase dontRemove)
        {
            for (int i = m_screens.Count - 1; i >= 0; i--)
            {
                MyGuiScreenBase objA = m_screens[i];
                if (!ReferenceEquals(objA, dontRemove) && objA.CanCloseInCloseAllScreenCalls())
                {
                    objA.CloseScreen();
                }
            }
        }

        public static void CloseAllScreensExceptThisOneAndAllTopMost(MyGuiScreenBase dontRemove)
        {
            foreach (MyGuiScreenBase base2 in m_screens)
            {
                if (ReferenceEquals(base2, dontRemove))
                {
                    continue;
                }
                if (!base2.IsTopMostScreen() && base2.CanCloseInCloseAllScreenCalls())
                {
                    base2.CloseScreen();
                }
            }
        }

        public static void CloseAllScreensNowExcept(MyGuiScreenBase dontRemove)
        {
            for (int i = m_screens.Count - 1; i >= 0; i--)
            {
                MyGuiScreenBase objA = m_screens[i];
                if (!ReferenceEquals(objA, dontRemove) && objA.CanCloseInCloseAllScreenCalls())
                {
                    objA.CloseScreenNow();
                }
            }
            using (List<MyGuiScreenBase>.Enumerator enumerator = m_screensToAdd.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UnloadContent();
                }
            }
            m_screensToAdd.Clear();
        }

        public static void CloseScreen(Type screenType)
        {
            if (m_screens != null)
            {
                for (int i = 0; i < m_screens.Count; i++)
                {
                    if (m_screens[i].GetType() == screenType)
                    {
                        m_screens[i].CloseScreen();
                    }
                }
            }
        }

        public static void CloseScreenNow(Type screenType)
        {
            if (m_screens != null)
            {
                for (int i = 0; i < m_screens.Count; i++)
                {
                    if (m_screens[i].GetType() == screenType)
                    {
                        m_screens[i].CloseScreenNow();
                    }
                }
            }
        }

        public static void Draw()
        {
            MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
            MyGuiScreenBase objB = null;
            bool canHideOthers = false;
            for (int i = m_screens.Count - 1; i >= 0; i--)
            {
                MyGuiScreenBase base4 = m_screens[i];
                bool enabledBackgroundFade = base4.EnabledBackgroundFade;
                bool flag3 = false;
                if ((ReferenceEquals(screenWithFocus, base4) || base4.GetDrawScreenEvenWithoutFocus()) || !canHideOthers)
                {
                    if ((base4.State != MyGuiScreenState.CLOSED) & enabledBackgroundFade)
                    {
                        flag3 = true;
                    }
                }
                else if (IsScreenTransitioning(base4) & enabledBackgroundFade)
                {
                    flag3 = true;
                }
                if (flag3)
                {
                    objB = base4;
                    break;
                }
                canHideOthers = base4.CanHideOthers;
            }
            for (int j = 0; j < m_screens.Count; j++)
            {
                MyGuiScreenBase base5 = m_screens[j];
                bool flag4 = false;
                if ((ReferenceEquals(screenWithFocus, base5) || base5.GetDrawScreenEvenWithoutFocus()) || !canHideOthers)
                {
                    if ((base5.State != MyGuiScreenState.CLOSED) && (base5.State != MyGuiScreenState.HIDDEN))
                    {
                        flag4 = true;
                    }
                }
                else if (!base5.CanBeHidden)
                {
                    flag4 = true;
                }
                else if (IsScreenTransitioning(base5))
                {
                    flag4 = true;
                }
                if (flag4)
                {
                    if (ReferenceEquals(base5, objB))
                    {
                        MyGuiManager.DrawSpriteBatch(@"Textures\Gui\Screens\screen_background_fade.dds", MyGuiManager.GetFullscreenRectangle(), base5.BackgroundFadeColor, true);
                    }
                    base5.Draw();
                }
            }
            if (screenWithFocus != null)
            {
                using (List<MyGuiControlBase>.Enumerator enumerator = screenWithFocus.Controls.GetVisibleControls().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ShowToolTip();
                    }
                }
            }
        }

        public static void GetControlsUnderMouseCursor(List<MyGuiControlBase> outControls, bool visibleOnly)
        {
            foreach (MyGuiScreenBase base2 in m_screens)
            {
                if (base2.State == MyGuiScreenState.OPENED)
                {
                    base2.GetControlsUnderMouseCursor(MyGuiManager.MouseCursorPosition, outControls, visibleOnly);
                }
            }
        }

        public static T GetFirstScreenOfType<T>() where T: MyGuiScreenBase => 
            m_screens.OfType<T>().FirstOrDefault<T>();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        public static StringBuilder GetGuiScreensForDebug()
        {
            m_sb.Clear();
            m_sb.ConcatFormat<string, int, string>("{0}{1}{2}", "GUI screens: [", m_screens.Count, "]: ", null);
            MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
            for (int i = 0; i < m_screens.Count; i++)
            {
                MyGuiScreenBase objB = m_screens[i];
                if (ReferenceEquals(screenWithFocus, objB))
                {
                    m_sb.Append("[F]");
                }
                m_sb.Append(objB.GetFriendlyName());
                m_sb.Append((i < (m_screens.Count - 1)) ? ", " : "");
            }
            return m_sb;
        }

        private static int GetIndexOfLastNonTopScreen()
        {
            int num = 0;
            int num2 = 0;
            while (true)
            {
                if (num2 < m_screens.Count)
                {
                    MyGuiScreenBase base2 = m_screens[num2];
                    if (!base2.IsTopMostScreen() && !base2.IsTopScreen())
                    {
                        num = num2 + 1;
                        num2++;
                        continue;
                    }
                }
                return num;
            }
        }

        public static MyGuiScreenBase GetPreviousScreen(MyGuiScreenBase screen, Predicate<MyGuiScreenBase> condition, Predicate<MyGuiScreenBase> terminatingCondition)
        {
            MyGuiScreenBase base2 = null;
            int screensCount = -1;
            if (screen == null)
            {
                screensCount = GetScreensCount();
            }
            int num2 = GetScreensCount() - 1;
            while (true)
            {
                while (true)
                {
                    if (num2 > 0)
                    {
                        MyGuiScreenBase objB = m_screens[num2];
                        if (ReferenceEquals(screen, objB))
                        {
                            screensCount = num2;
                        }
                        if (num2 < screensCount)
                        {
                            if (condition(objB))
                            {
                                base2 = objB;
                            }
                            else if (!terminatingCondition(objB))
                            {
                                break;
                            }
                            return base2;
                        }
                    }
                    else
                    {
                        return base2;
                    }
                    break;
                }
                num2--;
            }
        }

        public static int GetScreensCount() => 
            m_screens.Count;

        public static MyGuiScreenBase GetScreenWithFocus()
        {
            MyGuiScreenBase base2 = null;
            if ((m_screens != null) && (m_screens.Count > 0))
            {
                for (int i = m_screens.Count - 1; i >= 0; i--)
                {
                    MyGuiScreenBase screen = m_screens[i];
                    if (((screen != null) && ((screen.State == MyGuiScreenState.OPENED) || IsScreenTransitioning(screen))) && screen.CanHaveFocus)
                    {
                        base2 = screen;
                        break;
                    }
                }
            }
            return base2;
        }

        public static MyGuiScreenBase GetTopHiddenScreen()
        {
            MyGuiScreenBase base2 = null;
            int num = GetScreensCount() - 1;
            while (true)
            {
                if (num > 0)
                {
                    MyGuiScreenBase base3 = m_screens[num];
                    if ((base3.State != MyGuiScreenState.HIDDEN) && (base3.State != MyGuiScreenState.HIDING))
                    {
                        num--;
                        continue;
                    }
                    base2 = base3;
                }
                return base2;
            }
        }

        public static void HandleInput()
        {
            try
            {
                if ((m_screens != null) && (m_screens.Count > 0))
                {
                    MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
                    if (m_inputToNonFocusedScreens)
                    {
                        bool flag = false;
                        int num = m_screens.Count - 1;
                        while (true)
                        {
                            if (num < 0)
                            {
                                m_inputToNonFocusedScreens &= flag;
                                break;
                            }
                            if (m_screens.Count > num)
                            {
                                MyGuiScreenBase objA = m_screens[num];
                                if (objA != null)
                                {
                                    if (objA.CanShareInput())
                                    {
                                        objA.HandleInput(!ReferenceEquals(m_lastScreenWithFocus, screenWithFocus));
                                        flag = true;
                                    }
                                    else if (!flag && ReferenceEquals(objA, screenWithFocus))
                                    {
                                        objA.HandleInput(!ReferenceEquals(m_lastScreenWithFocus, screenWithFocus));
                                    }
                                }
                            }
                            num--;
                        }
                    }
                    else
                    {
                        foreach (MyGuiScreenBase base4 in m_screens)
                        {
                            if (!ReferenceEquals(base4, screenWithFocus))
                            {
                                base4.InputLost();
                            }
                        }
                        if (screenWithFocus != null)
                        {
                            switch (screenWithFocus.State)
                            {
                                case MyGuiScreenState.OPENING:
                                case MyGuiScreenState.OPENED:
                                case MyGuiScreenState.UNHIDING:
                                    screenWithFocus.HandleInput(!ReferenceEquals(m_lastScreenWithFocus, screenWithFocus));
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    m_lastScreenWithFocus = screenWithFocus;
                    if (((screenWithFocus != null) && (screenWithFocus.State == MyGuiScreenState.OPENED)) && (MyImeProcessor.Instance != null))
                    {
                        MyImeProcessor.Instance.RecaptureTopScreen(screenWithFocus);
                    }
                }
            }
            finally
            {
            }
        }

        public static void HandleInputAfterSimulation()
        {
            for (int i = m_screens.Count - 1; i >= 0; i--)
            {
                m_screens[i].HandleInputAfterSimulation();
            }
        }

        public static void InsertScreen(MyGuiScreenBase screen, int index)
        {
            int num1 = MyUtils.GetClampInt(index, 0, m_screens.Count - 1);
            index = num1;
            screen.Closed += sender => RemoveScreen(sender);
            m_screens.Insert(index, screen);
            if (!screen.IsLoaded)
            {
                screen.State = MyGuiScreenState.OPENING;
                screen.LoadData();
                screen.LoadContent();
            }
        }

        private static bool IsAnyScreenInTransition()
        {
            bool flag = false;
            if (m_screens.Count > 0)
            {
                for (int i = m_screens.Count - 1; i >= 0; i--)
                {
                    flag = IsScreenTransitioning(m_screens[i]);
                    if (flag)
                    {
                        break;
                    }
                }
            }
            return flag;
        }

        public static bool IsAnyScreenOpening()
        {
            bool flag = false;
            if (m_screens.Count > 0)
            {
                for (int i = m_screens.Count - 1; i >= 0; i--)
                {
                    flag = m_screens[i].State == MyGuiScreenState.OPENING;
                    if (flag)
                    {
                        break;
                    }
                }
            }
            return flag;
        }

        public static bool IsScreenOfTypeOpen(Type screenType)
        {
            using (List<MyGuiScreenBase>.Enumerator enumerator = m_screens.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGuiScreenBase current = enumerator.Current;
                    if ((current.GetType() == screenType) && (current.State == MyGuiScreenState.OPENED))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsScreenOnTop(MyGuiScreenBase screen)
        {
            int num = GetIndexOfLastNonTopScreen() - 1;
            return ((num >= 0) && ((num < m_screens.Count) && ((m_screensToAdd.Count <= 0) ? (m_screens[num] == screen) : false)));
        }

        private static bool IsScreenTransitioning(MyGuiScreenBase screen) => 
            ((screen.State == MyGuiScreenState.CLOSING) || ((screen.State == MyGuiScreenState.OPENING) || ((screen.State == MyGuiScreenState.HIDING) || (screen.State == MyGuiScreenState.UNHIDING))));

        public static void LoadContent()
        {
            MyLog.Default.WriteLine("MyGuiManager.LoadContent() - START");
            MyLog.Default.IncreaseIndent();
            using (List<MyGuiScreenBase>.Enumerator enumerator = m_screens.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.LoadContent();
                }
            }
            MyLog.Default.DecreaseIndent();
            MyLog.Default.WriteLine("MyGuiManager.LoadContent() - END");
        }

        public static void LoadData()
        {
            m_screens.Clear();
            using (lockObject.AcquireExclusiveUsing())
            {
                m_screensToRemove.Clear();
            }
            m_screensToAdd.Clear();
        }

        private static void NotifyScreenAdded(MyGuiScreenBase screen)
        {
            if (ScreenAdded != null)
            {
                ScreenAdded(screen);
            }
        }

        private static void NotifyScreenRemoved(MyGuiScreenBase screen)
        {
            if (ScreenRemoved != null)
            {
                ScreenRemoved(screen);
            }
        }

        public static void RecreateControls()
        {
            if (m_screens != null)
            {
                for (int i = 0; i < m_screens.Count; i++)
                {
                    m_screens[i].RecreateControls(false);
                }
            }
        }

        public static void RemoveAllScreensExcept(MyGuiScreenBase dontRemove)
        {
            foreach (MyGuiScreenBase base2 in m_screens)
            {
                if (!ReferenceEquals(base2, dontRemove))
                {
                    RemoveScreen(base2);
                }
            }
        }

        public static void RemoveScreen(MyGuiScreenBase screen)
        {
            if (!IsAnyScreenOpening())
            {
                MyGuiScreenBase base2 = GetPreviousScreen(screen, x => x.CanBeHidden, x => x.CanHideOthers);
                if ((base2 != null) && ((base2.State == MyGuiScreenState.HIDDEN) || (base2.State == MyGuiScreenState.HIDING)))
                {
                    base2.UnhideScreen();
                    MyInput.Static.JoystickAsMouse = base2.JoystickAsMouse;
                }
            }
            using (lockObject.AcquireExclusiveUsing())
            {
                m_screensToRemove.Add(screen);
            }
        }

        public static void RemoveScreenByType(Type screenType)
        {
            foreach (MyGuiScreenBase base2 in m_screens)
            {
                if (screenType.IsAssignableFrom(base2.GetType()))
                {
                    RemoveScreen(base2);
                }
            }
        }

        private static void RemoveScreens()
        {
            using (lockObject.AcquireExclusiveUsing())
            {
                bool flag = false;
                foreach (MyGuiScreenBase base2 in m_screensToRemove)
                {
                    if (base2.IsLoaded)
                    {
                        base2.UnloadContent();
                        base2.UnloadData();
                    }
                    base2.OnRemoved();
                    m_screens.Remove(base2);
                    flag = true;
                    int index = m_screensToAdd.Count - 1;
                    while (true)
                    {
                        if (index < 0)
                        {
                            NotifyScreenRemoved(base2);
                            break;
                        }
                        if (m_screensToAdd[index] == base2)
                        {
                            m_screensToAdd.RemoveAt(index);
                        }
                        index--;
                    }
                }
                m_screensToRemove.Clear();
                if (flag)
                {
                    MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
                    if ((screenWithFocus != null) && ((screenWithFocus.State == MyGuiScreenState.HIDDEN) || (screenWithFocus.State == MyGuiScreenState.HIDING)))
                    {
                        screenWithFocus.UnhideScreen();
                    }
                }
            }
        }

        public static void UnloadContent()
        {
            foreach (MyGuiScreenBase base2 in m_screens)
            {
                if (base2.IsFirstForUnload())
                {
                    base2.UnloadContent();
                }
            }
            foreach (MyGuiScreenBase base3 in m_screens)
            {
                if (!base3.IsFirstForUnload())
                {
                    base3.UnloadContent();
                }
            }
        }

        public static void Update(int totalTimeInMS)
        {
            TotalGamePlayTimeInMilliseconds = totalTimeInMS;
            RemoveScreens();
            AddScreens();
            RemoveScreens();
            MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
            for (int i = 0; i < m_screens.Count; i++)
            {
                MyGuiScreenBase local1 = m_screens[i];
                local1.Update(local1 == screenWithFocus);
            }
            if ((m_screens.Count > 0) && (m_screens[m_screens.Count - 1].State == MyGuiScreenState.HIDDEN))
            {
                m_screens[m_screens.Count - 1].UnhideScreen();
            }
        }

        public static MyGuiScreenBase LastScreenWithFocus =>
            m_lastScreenWithFocus;

        public static Thread UpdateThread
        {
            [CompilerGenerated]
            get => 
                <UpdateThread>k__BackingField;
            [CompilerGenerated]
            set => 
                (<UpdateThread>k__BackingField = value);
        }

        public static MyGuiControlBase FocusedControl
        {
            get
            {
                MyGuiScreenBase screenWithFocus = GetScreenWithFocus();
                return screenWithFocus?.FocusedControl;
            }
        }

        public static bool InputToNonFocusedScreens
        {
            get => 
                m_inputToNonFocusedScreens;
            set => 
                (m_inputToNonFocusedScreens = value);
        }

        public static IEnumerable<MyGuiScreenBase> Screens =>
            m_screens;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyScreenManager.<>c <>9 = new MyScreenManager.<>c();
            public static MyGuiScreenBase.ScreenHandler <>9__38_0;
            public static Predicate<MyGuiScreenBase> <>9__38_1;
            public static Predicate<MyGuiScreenBase> <>9__38_2;
            public static MyGuiScreenBase.ScreenHandler <>9__39_0;
            public static MyGuiScreenBase.ScreenHandler <>9__40_0;
            public static Predicate<MyGuiScreenBase> <>9__40_1;
            public static Predicate<MyGuiScreenBase> <>9__40_2;
            public static Predicate<MyGuiScreenBase> <>9__41_0;
            public static Predicate<MyGuiScreenBase> <>9__41_1;

            internal void <AddScreen>b__38_0(MyGuiScreenBase sender)
            {
                MyScreenManager.RemoveScreen(sender);
            }

            internal bool <AddScreen>b__38_1(MyGuiScreenBase x) => 
                x.CanBeHidden;

            internal bool <AddScreen>b__38_2(MyGuiScreenBase x) => 
                x.CanHideOthers;

            internal void <AddScreenNow>b__40_0(MyGuiScreenBase sender)
            {
                MyScreenManager.RemoveScreen(sender);
            }

            internal bool <AddScreenNow>b__40_1(MyGuiScreenBase x) => 
                x.CanBeHidden;

            internal bool <AddScreenNow>b__40_2(MyGuiScreenBase x) => 
                x.CanHideOthers;

            internal void <InsertScreen>b__39_0(MyGuiScreenBase sender)
            {
                MyScreenManager.RemoveScreen(sender);
            }

            internal bool <RemoveScreen>b__41_0(MyGuiScreenBase x) => 
                x.CanBeHidden;

            internal bool <RemoveScreen>b__41_1(MyGuiScreenBase x) => 
                x.CanHideOthers;
        }
    }
}

