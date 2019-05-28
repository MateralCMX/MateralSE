namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Input;
    using VRage.Utils;

    public abstract class MyAbstractControlMenuItem
    {
        private static string CTRL = "ctrl";
        private static string SHIFT = "shift";
        private static string ALT = "alt";
        private static string PLUS = " + ";
        private static string COLON = ":";
        private static string FORMAT_LABEL = "{0}";
        private static string FORMAT_LABEL_HINT = "{0} ({1})";
        private static StringBuilder m_tmpBuilder = new StringBuilder();

        public MyAbstractControlMenuItem(string controlName, MySupportKeysEnum supportKeys = 0)
        {
            this.ControlName = this.ConstructCompleteControl(controlName, supportKeys);
        }

        public MyAbstractControlMenuItem(MyStringId controlCode, MySupportKeysEnum supportKeys = 0)
        {
            this.ControlName = this.ConstructCompleteControl(this.GetControlName(controlCode), supportKeys);
        }

        public abstract void Activate();
        private string ConstructCompleteControl(string controlName, MySupportKeysEnum supportKeys)
        {
            m_tmpBuilder.Clear();
            if (this.HasSupportKey(supportKeys, MySupportKeysEnum.CTRL))
            {
                m_tmpBuilder.Append(CTRL).Append(PLUS);
            }
            if (this.HasSupportKey(supportKeys, MySupportKeysEnum.NONE | MySupportKeysEnum.SHIFT))
            {
                m_tmpBuilder.Append(SHIFT).Append(PLUS);
            }
            if (this.HasSupportKey(supportKeys, MySupportKeysEnum.ALT))
            {
                m_tmpBuilder.Append(ALT).Append(PLUS);
            }
            m_tmpBuilder.Append(controlName);
            return m_tmpBuilder.ToString();
        }

        private string GetControlName(MyStringId controlCode)
        {
            if (controlCode == MyStringId.NullOrEmpty)
            {
                return null;
            }
            string name = null;
            MyControl gameControl = MyInput.Static.GetGameControl(controlCode);
            if (gameControl != null)
            {
                MyMouseButtonsEnum mouseControl = gameControl.GetMouseControl();
                MyKeys keyboardControl = gameControl.GetKeyboardControl();
                if (mouseControl != MyMouseButtonsEnum.None)
                {
                    name = MyInput.Static.GetName(mouseControl);
                }
                else if (keyboardControl != MyKeys.None)
                {
                    name = MyInput.Static.GetKeyName(keyboardControl);
                }
            }
            return name;
        }

        private bool HasSupportKey(MySupportKeysEnum collection, MySupportKeysEnum key) => 
            ((collection & key) == key);

        public virtual void Next()
        {
        }

        public virtual void Previous()
        {
        }

        public virtual void UpdateValue()
        {
        }

        public abstract string Label { get; }

        public virtual string CurrentValue =>
            null;

        public MyStringId ControlCode { get; private set; }

        public string ControlName { get; private set; }

        public virtual bool Enabled =>
            true;

        public string ControlLabel
        {
            get
            {
                if (string.IsNullOrEmpty(this.Label))
                {
                    return null;
                }
                m_tmpBuilder.Clear();
                if (string.IsNullOrEmpty(this.ControlName))
                {
                    m_tmpBuilder.AppendFormat(FORMAT_LABEL, this.Label);
                }
                else
                {
                    m_tmpBuilder.AppendFormat(FORMAT_LABEL_HINT, this.Label, this.ControlName);
                }
                if (!string.IsNullOrEmpty(this.CurrentValue))
                {
                    m_tmpBuilder.Append(COLON);
                }
                return m_tmpBuilder.ToString();
            }
        }
    }
}

