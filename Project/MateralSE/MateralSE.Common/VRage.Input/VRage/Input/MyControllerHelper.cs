namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public static class MyControllerHelper
    {
        private static readonly Dictionary<MyJoystickAxesEnum, char> XBOX_AXES_CODES;
        private static readonly Dictionary<MyJoystickButtonsEnum, char> XBOX_BUTTONS_CODES;
        public static readonly MyStringId CX_BASE;
        public static readonly MyStringId CX_GUI;
        public static readonly MyStringId CX_CHARACTER;
        private static EmptyControl m_nullControl;
        private static Dictionary<MyStringId, Context> m_bindings;

        static MyControllerHelper()
        {
            Dictionary<MyJoystickAxesEnum, char> dictionary1 = new Dictionary<MyJoystickAxesEnum, char>();
            dictionary1.Add(MyJoystickAxesEnum.Xneg, 0xe016);
            dictionary1.Add(MyJoystickAxesEnum.Xpos, 0xe015);
            dictionary1.Add(MyJoystickAxesEnum.Ypos, 0xe014);
            dictionary1.Add(MyJoystickAxesEnum.Yneg, 0xe017);
            dictionary1.Add(MyJoystickAxesEnum.RotationXneg, 0xe020);
            dictionary1.Add(MyJoystickAxesEnum.RotationXpos, 0xe019);
            dictionary1.Add(MyJoystickAxesEnum.RotationYneg, 0xe021);
            dictionary1.Add(MyJoystickAxesEnum.RotationYpos, 0xe018);
            dictionary1.Add(MyJoystickAxesEnum.Zneg, 0xe007);
            dictionary1.Add(MyJoystickAxesEnum.Zpos, 0xe008);
            XBOX_AXES_CODES = dictionary1;
            Dictionary<MyJoystickButtonsEnum, char> dictionary2 = new Dictionary<MyJoystickButtonsEnum, char>();
            dictionary2.Add(MyJoystickButtonsEnum.J01, 0xe001);
            dictionary2.Add(MyJoystickButtonsEnum.J02, 0xe003);
            dictionary2.Add(MyJoystickButtonsEnum.J03, 0xe002);
            dictionary2.Add(MyJoystickButtonsEnum.J04, 0xe004);
            dictionary2.Add(MyJoystickButtonsEnum.J05, 0xe005);
            dictionary2.Add(MyJoystickButtonsEnum.J06, 0xe006);
            dictionary2.Add(MyJoystickButtonsEnum.J07, 0xe00d);
            dictionary2.Add(MyJoystickButtonsEnum.J08, 0xe00e);
            dictionary2.Add(MyJoystickButtonsEnum.J09, 0xe00b);
            dictionary2.Add(MyJoystickButtonsEnum.J10, 0xe00c);
            dictionary2.Add(MyJoystickButtonsEnum.JDLeft, 0xe010);
            dictionary2.Add(MyJoystickButtonsEnum.JDUp, 0xe011);
            dictionary2.Add(MyJoystickButtonsEnum.JDRight, 0xe012);
            dictionary2.Add(MyJoystickButtonsEnum.JDDown, 0xe013);
            dictionary2.Add(MyJoystickButtonsEnum.J11, 0xe007);
            dictionary2.Add(MyJoystickButtonsEnum.J12, 0xe008);
            XBOX_BUTTONS_CODES = dictionary2;
            CX_BASE = MyStringId.GetOrCompute("BASE");
            CX_GUI = MyStringId.GetOrCompute("GUI");
            CX_CHARACTER = MyStringId.GetOrCompute("CHARACTER");
            m_nullControl = new EmptyControl();
            m_bindings = new Dictionary<MyStringId, Context>(MyStringId.Comparer);
            m_bindings.Add(MyStringId.NullOrEmpty, new Context());
        }

        public static void AddContext(MyStringId context, MyStringId? parent = new MyStringId?())
        {
            if (!m_bindings.ContainsKey(context))
            {
                Context context2 = new Context();
                m_bindings.Add(context, context2);
                if ((parent != null) && m_bindings.ContainsKey(parent.Value))
                {
                    context2.ParentContext = m_bindings[parent.Value];
                }
            }
        }

        public static void AddControl(MyStringId context, MyStringId stringId, MyJoystickAxesEnum axis)
        {
            m_bindings[context][stringId] = new JoystickAxis(axis);
        }

        public static void AddControl(MyStringId context, MyStringId stringId, MyJoystickButtonsEnum button)
        {
            m_bindings[context][stringId] = new JoystickButton(button);
        }

        public static char GetCodeForControl(MyStringId context, MyStringId stringId) => 
            m_bindings[context][stringId].ControlCode();

        public static bool IsControl(MyStringId context, MyStringId stringId, MyControlStateType type = 0, bool joystickOnly = false)
        {
            switch (type)
            {
                case MyControlStateType.NEW_PRESSED:
                    if (joystickOnly || !MyInput.Static.IsNewGameControlPressed(stringId))
                    {
                        return m_bindings[context][stringId].IsNewPressed();
                    }
                    return true;

                case MyControlStateType.PRESSED:
                    if (joystickOnly || !MyInput.Static.IsGameControlPressed(stringId))
                    {
                        return m_bindings[context][stringId].IsPressed();
                    }
                    return true;

                case MyControlStateType.NEW_RELEASED:
                    if (joystickOnly || !MyInput.Static.IsNewGameControlReleased(stringId))
                    {
                        return m_bindings[context][stringId].IsNewReleased();
                    }
                    return true;
            }
            return false;
        }

        public static float IsControlAnalog(MyStringId context, MyStringId stringId) => 
            (MyInput.Static.GetGameControlAnalogState(stringId) + m_bindings[context][stringId].AnalogValue());

        public static void NullControl(MyStringId context, MyJoystickAxesEnum axis)
        {
            MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
            foreach (KeyValuePair<MyStringId, IControl> pair in m_bindings[context].Bindings)
            {
                if ((pair.Value is JoystickAxis) && (pair.Value.Code == axis))
                {
                    nullOrEmpty = pair.Key;
                    break;
                }
            }
            if (nullOrEmpty != MyStringId.NullOrEmpty)
            {
                m_bindings[context][nullOrEmpty] = m_nullControl;
            }
        }

        public static void NullControl(MyStringId context, MyJoystickButtonsEnum button)
        {
            MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
            foreach (KeyValuePair<MyStringId, IControl> pair in m_bindings[context].Bindings)
            {
                if ((pair.Value is JoystickButton) && (pair.Value.Code == button))
                {
                    nullOrEmpty = pair.Key;
                    break;
                }
            }
            if (nullOrEmpty != MyStringId.NullOrEmpty)
            {
                m_bindings[context][nullOrEmpty] = m_nullControl;
            }
        }

        public static void NullControl(MyStringId context, MyStringId stringId)
        {
            m_bindings[context][stringId] = m_nullControl;
        }

        private class Context
        {
            public MyControllerHelper.Context ParentContext;
            public Dictionary<MyStringId, MyControllerHelper.IControl> Bindings = new Dictionary<MyStringId, MyControllerHelper.IControl>(MyStringId.Comparer);

            public MyControllerHelper.IControl this[MyStringId id]
            {
                get => 
                    (!this.Bindings.ContainsKey(id) ? ((this.ParentContext == null) ? MyControllerHelper.m_nullControl : this.ParentContext[id]) : this.Bindings[id]);
                set => 
                    (this.Bindings[id] = value);
            }
        }

        private class EmptyControl : MyControllerHelper.IControl
        {
            public float AnalogValue() => 
                0f;

            public char ControlCode() => 
                ' ';

            public bool IsNewPressed() => 
                false;

            public bool IsNewReleased() => 
                false;

            public bool IsPressed() => 
                false;

            public byte Code =>
                0;
        }

        private interface IControl
        {
            float AnalogValue();
            char ControlCode();
            bool IsNewPressed();
            bool IsNewReleased();
            bool IsPressed();

            byte Code { get; }
        }

        private class JoystickAxis : MyControllerHelper.IControl
        {
            public MyJoystickAxesEnum Axis;

            public JoystickAxis(MyJoystickAxesEnum axis)
            {
                this.Axis = axis;
            }

            public float AnalogValue() => 
                MyInput.Static.GetJoystickAxisStateForGameplay(this.Axis);

            public char ControlCode() => 
                MyControllerHelper.XBOX_AXES_CODES[this.Axis];

            public bool IsNewPressed() => 
                MyInput.Static.IsJoystickAxisNewPressed(this.Axis);

            public bool IsNewReleased() => 
                MyInput.Static.IsNewJoystickAxisReleased(this.Axis);

            public bool IsPressed() => 
                MyInput.Static.IsJoystickAxisPressed(this.Axis);

            public byte Code =>
                ((byte) this.Axis);
        }

        private class JoystickButton : MyControllerHelper.IControl
        {
            public MyJoystickButtonsEnum Button;

            public JoystickButton(MyJoystickButtonsEnum button)
            {
                this.Button = button;
            }

            public float AnalogValue() => 
                (this.IsPressed() ? ((float) 1) : ((float) 0));

            public char ControlCode() => 
                MyControllerHelper.XBOX_BUTTONS_CODES[this.Button];

            public bool IsNewPressed() => 
                MyInput.Static.IsJoystickButtonNewPressed(this.Button);

            public bool IsNewReleased() => 
                MyInput.Static.IsNewJoystickButtonReleased(this.Button);

            public bool IsPressed() => 
                MyInput.Static.IsJoystickButtonPressed(this.Button);

            public byte Code =>
                ((byte) this.Button);
        }
    }
}

