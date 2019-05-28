namespace Sandbox.Game.Screens.Terminal.Controls
{
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.CompilerServices;

    public class MyTerminalControlProperty<TBlock, TValue> : MyTerminalValueControl<TBlock, TValue>, IMyTerminalControlProperty<TValue>, IMyTerminalControl, IMyTerminalValueControl<TValue>, ITerminalProperty where TBlock: MyTerminalBlock
    {
        public MyTerminalControlProperty(string id) : base(id)
        {
            this.Visible = x => false;
        }

        protected override MyGuiControlBase CreateGui()
        {
            this.Visible = x => false;
            return null;
        }

        public override TValue GetDefaultValue(TBlock block) => 
            default(TValue);

        public override TValue GetMaximum(TBlock block) => 
            this.GetDefaultValue(block);

        public override TValue GetMinimum(TBlock block) => 
            this.GetDefaultValue(block);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalControlProperty<TBlock, TValue>.<>c <>9;
            public static Func<TBlock, bool> <>9__0_0;
            public static Func<TBlock, bool> <>9__4_0;

            static <>c()
            {
                MyTerminalControlProperty<TBlock, TValue>.<>c.<>9 = new MyTerminalControlProperty<TBlock, TValue>.<>c();
            }

            internal bool <.ctor>b__0_0(TBlock x) => 
                false;

            internal bool <CreateGui>b__4_0(TBlock x) => 
                false;
        }
    }
}

