namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Input;
    using VRage.Utils;

    internal class MyLoadingScreenHint : MyLoadingScreenText
    {
        public readonly MyStringId[] Control;
        private MyControl[] m_control;

        public MyLoadingScreenHint(MyStringId text, MyStringId[] control) : base(text)
        {
            this.Control = control;
            this.m_control = new MyControl[this.Control.Length];
            this.RefreshControls();
        }

        public static void Init()
        {
            MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
            int num = 0;
            while ((nullOrEmpty = MyStringId.TryGet($"Hint{num:00}Text")) != MyStringId.NullOrEmpty)
            {
                int num2 = 0;
                MyStringId id = MyStringId.NullOrEmpty;
                List<MyStringId> list = new List<MyStringId>();
                while (true)
                {
                    if ((id = MyStringId.TryGet($"Hint{num:00}Control{num2}")) == MyStringId.NullOrEmpty)
                    {
                        MyLoadingScreenText.m_texts.Add(new MyLoadingScreenHint(nullOrEmpty, list.ToArray()));
                        num++;
                        break;
                    }
                    MyStringId orCompute = MyStringId.GetOrCompute(MyTexts.GetString(id));
                    list.Add(orCompute);
                    num2++;
                }
            }
        }

        private void RefreshControls()
        {
            for (int i = 0; i < this.m_control.Length; i++)
            {
                this.m_control[i] = MyInput.Static.GetGameControl(this.Control[i]);
            }
        }

        public override string ToString()
        {
            this.RefreshControls();
            return string.Format(MyTexts.GetString(base.Text), (object[]) this.m_control);
        }
    }
}

