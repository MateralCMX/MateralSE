namespace Sandbox.Game.Gui
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Input;
    using VRageMath;

    public abstract class MyMultiDebugInputComponent : MyDebugComponent
    {
        private int m_activeMode;
        private List<MyKeys> m_keys = new List<MyKeys>();

        protected MyMultiDebugInputComponent()
        {
        }

        public override void Draw()
        {
            MyDebugComponent[] components = this.Components;
            if ((components == null) || (components.Length == 0))
            {
                object[] arguments = new object[] { this.GetName() };
                base.Text(Color.Red, 1.5f, "{0} Debug Input - NO COMPONENTS", arguments);
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                if (components.Length != 0)
                {
                    builder.Append(this.FormatComponentName(0));
                }
                for (int i = 1; i < components.Length; i++)
                {
                    builder.Append(" ");
                    builder.Append(this.FormatComponentName(i));
                }
                object[] arguments = new object[] { this.GetName(), builder.ToString() };
                base.Text(Color.Yellow, 1.5f, "{0} Debug Input: {1}", arguments);
                if (MySandboxGame.Config.DebugComponentsInfo == MyDebugComponent.MyDebugComponentInfoState.FullInfo)
                {
                    base.Text(Color.White, 1.2f, "Select Tab: Left WinKey + Tab Number", Array.Empty<object>());
                }
                base.VSpace(5f);
                this.DrawInternal();
                components[this.m_activeMode].Draw();
            }
        }

        public virtual void DrawInternal()
        {
        }

        private string FormatComponentName(int index)
        {
            string name = this.Components[index].GetName();
            return ((index == this.m_activeMode) ? $"{name.ToUpper()}({index})" : $"{name}({index})");
        }

        public override bool HandleInput()
        {
            if ((this.Components == null) || (this.Components.Length == 0))
            {
                return false;
            }
            if (MyInput.Static.IsKeyPress(MyKeys.LeftWindows) || MyInput.Static.IsKeyPress(MyKeys.RightWindows))
            {
                MyInput.Static.GetPressedKeys(this.m_keys);
                int activeMode = this.m_activeMode;
                foreach (int num2 in this.m_keys)
                {
                    if (num2 < 0x60)
                    {
                        continue;
                    }
                    if (num2 <= 0x69)
                    {
                        int num3 = num2 - 0x60;
                        if (num3 < this.Components.Length)
                        {
                            activeMode = num3;
                        }
                    }
                }
                if (this.m_activeMode != activeMode)
                {
                    this.m_activeMode = activeMode;
                    base.Save();
                    return true;
                }
            }
            return this.Components[this.m_activeMode].HandleInput();
        }

        public override void Update10()
        {
            base.Update10();
            if (this.ActiveComponent != null)
            {
                this.ActiveComponent.Update10();
            }
        }

        public override void Update100()
        {
            base.Update100();
            if (this.ActiveComponent != null)
            {
                this.ActiveComponent.Update100();
            }
        }

        public abstract MyDebugComponent[] Components { get; }

        public MyDebugComponent ActiveComponent
        {
            get
            {
                if ((this.Components == null) || (this.Components.Length == 0))
                {
                    return null;
                }
                return this.Components[this.m_activeMode];
            }
        }

        public override object InputData
        {
            get
            {
                MyDebugComponent[] components = this.Components;
                object[] objArray = new object[components.Length];
                for (int i = 0; i < components.Length; i++)
                {
                    objArray[i] = components[i].InputData;
                }
                return new MultidebugData?(new MultidebugData { 
                    ActiveDebug = this.m_activeMode,
                    ChildDatas = objArray
                });
            }
            set
            {
                MultidebugData? nullable = value as MultidebugData?;
                if (nullable == null)
                {
                    this.m_activeMode = 0;
                }
                else
                {
                    this.m_activeMode = nullable.Value.ActiveDebug;
                    MyDebugComponent[] components = this.Components;
                    if (components.Length == nullable.Value.ChildDatas.Length)
                    {
                        for (int i = 0; i < components.Length; i++)
                        {
                            components[i].InputData = nullable.Value.ChildDatas[i];
                        }
                    }
                }
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct MultidebugData
        {
            public int ActiveDebug;
            public object[] ChildDatas;
        }
    }
}

