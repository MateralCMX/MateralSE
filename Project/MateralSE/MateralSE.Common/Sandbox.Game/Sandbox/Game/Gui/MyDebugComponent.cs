namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Input;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public abstract class MyDebugComponent
    {
        private static float m_textOffset = 0f;
        private const int LINE_OFFSET = 15;
        private const int LINE_BREAK_OFFSET = 0x11;
        private static HashSet<ushort> m_enabledShortcutKeys = new HashSet<ushort>();
        private SortedSet<MyShortcut> m_shortCuts;
        private HashSet<MySwitch> m_switches;
        private bool m_enabled;
        public int m_frameCounter;

        public MyDebugComponent() : this(false)
        {
        }

        public MyDebugComponent(bool enabled)
        {
            this.m_shortCuts = new SortedSet<MyShortcut>(MyShortcutComparer.Static);
            this.m_switches = new HashSet<MySwitch>();
            this.m_enabled = true;
            this.Enabled = enabled;
        }

        protected void AddShortcut(MyKeys key, bool newPress, bool control, bool shift, bool alt, Func<string> description, Func<bool> action)
        {
            MyShortcut item = new MyShortcut {
                Key = key,
                NewPress = newPress,
                Control = control,
                Shift = shift,
                Alt = alt,
                Description = description,
                _Action = action
            };
            this.m_shortCuts.Add(item);
        }

        protected void AddSwitch(MyKeys key, Func<MyKeys, bool> action, MyRef<bool> boolRef, string note = "")
        {
            MySwitch item = new MySwitch(key, action, boolRef, note);
            this.m_switches.Add(item);
        }

        protected void AddSwitch(MyKeys key, Func<MyKeys, bool> action, string note = "", bool defaultValue = false)
        {
            MySwitch item = new MySwitch(key, action, note, defaultValue);
            this.m_switches.Add(item);
        }

        public virtual void DispatchUpdate()
        {
            if ((this.m_frameCounter % 10) == 0)
            {
                this.Update10();
            }
            if (this.m_frameCounter >= 100)
            {
                this.Update100();
                this.m_frameCounter = 0;
            }
            this.m_frameCounter++;
        }

        public virtual void Draw()
        {
            if (MySandboxGame.Config.DebugComponentsInfo == MyDebugComponentInfoState.FullInfo)
            {
                float scale = 0.6f;
                MyRenderProxy.DebugDrawText2D(new Vector2(0.1f, m_textOffset), this.GetName() + " debug input:", Color.Gold, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                m_textOffset += 15f;
                foreach (MyShortcut shortcut in this.m_shortCuts)
                {
                    string keysString = shortcut.GetKeysString();
                    string text = shortcut.Description();
                    Color color = m_enabledShortcutKeys.Contains(shortcut.GetId()) ? Color.Red : Color.White;
                    MyRenderProxy.DebugDrawText2D(new Vector2(100f, m_textOffset), keysString + ":", color, scale, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(105f, m_textOffset), text, Color.White, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    m_enabledShortcutKeys.Add(shortcut.GetId());
                    m_textOffset += 15f;
                }
                foreach (MySwitch switch2 in this.m_switches)
                {
                    Color color = this.GetSwitchValue(switch2.Key) ? Color.Red : Color.White;
                    object[] objArray1 = new object[5];
                    objArray1[0] = "switch ";
                    objArray1[1] = switch2.Key;
                    objArray1[2] = (switch2.Note.Length == 0) ? "" : (" " + switch2.Note);
                    object[] local1 = objArray1;
                    local1[3] = " is ";
                    local1[4] = this.GetSwitchValue(switch2.Key) ? "On" : "Off";
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, m_textOffset), string.Concat(local1), color, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    m_textOffset += 15f;
                }
                m_textOffset += 5f;
            }
        }

        public abstract string GetName();
        public bool GetSwitchValue(string note)
        {
            using (HashSet<MySwitch>.Enumerator enumerator = this.m_switches.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySwitch current = enumerator.Current;
                    if (current.Note == note)
                    {
                        return current.IsSet;
                    }
                }
            }
            return false;
        }

        public bool GetSwitchValue(MyKeys key)
        {
            using (HashSet<MySwitch>.Enumerator enumerator = this.m_switches.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySwitch current = enumerator.Current;
                    if (current.Key == key)
                    {
                        return current.IsSet;
                    }
                }
            }
            return false;
        }

        public virtual bool HandleInput()
        {
            using (SortedSet<MyShortcut>.Enumerator enumerator = this.m_shortCuts.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyShortcut current = enumerator.Current;
                    bool flag = ((true & (current.Control == MyInput.Static.IsAnyCtrlKeyPressed())) & (current.Shift == MyInput.Static.IsAnyShiftKeyPressed())) & (current.Alt == MyInput.Static.IsAnyAltKeyPressed());
                    if (flag)
                    {
                        flag = !current.NewPress ? (flag & MyInput.Static.IsKeyPress(current.Key)) : (flag & MyInput.Static.IsNewKeyPressed(current.Key));
                    }
                    if (flag && (current._Action != null))
                    {
                        return current._Action();
                    }
                }
            }
            using (HashSet<MySwitch>.Enumerator enumerator2 = this.m_switches.GetEnumerator())
            {
                bool flag2;
                while (true)
                {
                    if (enumerator2.MoveNext())
                    {
                        MySwitch current = enumerator2.Current;
                        if (!(true & MyInput.Static.IsNewKeyPressed(current.Key)))
                        {
                            continue;
                        }
                        if (current.Action == null)
                        {
                            continue;
                        }
                        flag2 = current.Action(current.Key);
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return flag2;
            }
        TR_0000:
            return false;
        }

        protected void MultilineText(string message, params object[] arguments)
        {
            this.MultilineText(Color.White, 1f, message, arguments);
        }

        protected void MultilineText(Color color, string message, params object[] arguments)
        {
            this.MultilineText(color, 1f, message, arguments);
        }

        protected void MultilineText(Color color, float scale, string message, params object[] arguments)
        {
            if (arguments.Length != 0)
            {
                string text1 = string.Format(message, arguments);
                message = text1;
            }
            int num = 0;
            string str = message;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    num++;
                }
            }
            message = message.Replace("\t", "    ");
            float num2 = (15 + (0x11 * num)) * scale;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, m_textOffset), message, color, 0.6f * scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            m_textOffset += num2;
        }

        public static float NextTextOffset(float scale)
        {
            m_textOffset += 15f * scale;
            return m_textOffset;
        }

        public static void ResetFrame()
        {
            m_textOffset = 0f;
            m_enabledShortcutKeys.Clear();
        }

        protected void Save()
        {
            string name = this.GetName();
            SerializableDictionary<string, MyConfig.MyDebugInputData> debugInputComponents = MySandboxGame.Config.DebugInputComponents;
            MyConfig.MyDebugInputData data = debugInputComponents[name];
            data.Enabled = this.Enabled;
            data.Data = this.InputData;
            debugInputComponents[name] = data;
            MySandboxGame.Config.Save();
        }

        public void Section(string text, params object[] formatArgs)
        {
            this.VSpace(5f);
            this.Text(Color.Yellow, 1.5f, text, formatArgs);
            this.VSpace(5f);
        }

        protected void SetSwitch(MyKeys key, bool value)
        {
            foreach (MySwitch switch2 in this.m_switches)
            {
                if (switch2.Key == key)
                {
                    switch2.IsSet = value;
                    break;
                }
            }
        }

        protected void Text(string message, params object[] arguments)
        {
            this.Text(Color.White, 1f, message, arguments);
        }

        protected void Text(Color color, string message, params object[] arguments)
        {
            this.Text(color, 1f, message, arguments);
        }

        protected void Text(Color color, float scale, string message, params object[] arguments)
        {
            if (arguments.Length != 0)
            {
                string text1 = string.Format(message, arguments);
                message = text1;
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, NextTextOffset(scale)), message, color, 0.6f * scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
        }

        public virtual void Update10()
        {
        }

        public virtual void Update100()
        {
        }

        protected void VSpace(float space)
        {
            m_textOffset += space;
        }

        public static float VerticalTextOffset =>
            m_textOffset;

        protected static float NextVerticalOffset
        {
            get
            {
                m_textOffset += 15f;
                return m_textOffset;
            }
        }

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set => 
                (this.m_enabled = value);
        }

        public virtual object InputData
        {
            get => 
                null;
            set
            {
            }
        }

        public IMyInput Input =>
            MyInput.Static;

        public enum MyDebugComponentInfoState
        {
            NoInfo,
            EnabledInfo,
            FullInfo
        }

        public class MyRef<T>
        {
            private Action<T> modify;
            private Func<T> getter;

            public MyRef(Func<T> getter, Action<T> modify)
            {
                this.modify = modify;
                this.getter = getter;
            }

            public T Value
            {
                get => 
                    this.getter();
                set => 
                    this.modify(value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyShortcut
        {
            public MyKeys Key;
            public bool NewPress;
            public bool Control;
            public bool Shift;
            public bool Alt;
            public Func<string> Description;
            public Func<bool> _Action;
            public string GetKeysString()
            {
                string str = "";
                if (this.Control)
                {
                    str = str + "Ctrl";
                }
                if (this.Shift)
                {
                    str = str + (string.IsNullOrEmpty(str) ? "Shift" : "+Shift");
                }
                if (this.Alt)
                {
                    str = str + (string.IsNullOrEmpty(str) ? "Alt" : "+Alt");
                }
                return (str + (string.IsNullOrEmpty(str) ? MyInput.Static.GetKeyName(this.Key) : ("+" + MyInput.Static.GetKeyName(this.Key))));
            }

            public ushort GetId() => 
                ((ushort) (((ushort) (((ushort) (((ushort) (((byte) this.Key) << 8)) + (this.Control ? ((ushort) 4) : ((ushort) 0)))) + (this.Shift ? ((ushort) 2) : ((ushort) 0)))) + (this.Alt ? ((ushort) 1) : ((ushort) 0))));
        }

        private class MyShortcutComparer : IComparer<MyDebugComponent.MyShortcut>
        {
            public static MyDebugComponent.MyShortcutComparer Static = new MyDebugComponent.MyShortcutComparer();

            public int Compare(MyDebugComponent.MyShortcut x, MyDebugComponent.MyShortcut y) => 
                x.GetId().CompareTo(y.GetId());
        }

        private class MySwitch
        {
            public MyKeys Key;
            public Func<MyKeys, bool> Action;
            public string Note;
            private MyDebugComponent.MyRef<bool> m_boolReference;
            private bool m_value;

            public MySwitch(MyKeys key, Func<MyKeys, bool> action, string note = "")
            {
                this.Key = key;
                this.Action = action;
                this.Note = note;
            }

            public MySwitch(MyKeys key, Func<MyKeys, bool> action, MyDebugComponent.MyRef<bool> field, string note = "")
            {
                this.m_boolReference = field;
                this.Key = key;
                this.Action = action;
                this.Note = note;
            }

            public MySwitch(MyKeys key, Func<MyKeys, bool> action, string note = "", bool defaultValue = false)
            {
                this.Key = key;
                this.Action = action;
                this.Note = note;
                this.IsSet = defaultValue;
            }

            public ushort GetId() => 
                ((ushort) (((byte) this.Key) << 8));

            public bool IsSet
            {
                get => 
                    ((this.m_boolReference == null) ? this.m_value : this.m_boolReference.Value);
                set
                {
                    if (this.m_boolReference != null)
                    {
                        this.m_boolReference.Value = value;
                    }
                    else
                    {
                        this.m_value = value;
                    }
                }
            }
        }
    }
}

