namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Extensions;

    public abstract class MyTerminalControl<TBlock> : ITerminalControl, IMyTerminalControl where TBlock: MyTerminalBlock
    {
        public static readonly float PREFERRED_CONTROL_WIDTH;
        public static readonly MyTerminalBlock[] Empty;
        public readonly string Id;
        public Func<TBlock, bool> Enabled;
        public Func<TBlock, bool> Visible;
        private MyGuiControlBase m_control;

        static MyTerminalControl()
        {
            MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH = 355f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            MyTerminalControl<TBlock>.Empty = new MyTerminalBlock[0];
        }

        public MyTerminalControl(string id)
        {
            this.Enabled = b => true;
            this.Visible = b => true;
            this.Id = id;
            this.SupportsMultipleBlocks = true;
            ((ITerminalControl) this).TargetBlocks = MyTerminalControl<TBlock>.Empty;
        }

        protected abstract MyGuiControlBase CreateGui();
        public MyGuiControlBase GetGuiControl()
        {
            if (this.m_control == null)
            {
                this.m_control = this.CreateGui();
            }
            return this.m_control;
        }

        protected virtual void OnUpdateVisual()
        {
            this.m_control.Enabled = false;
            foreach (TBlock local in this.TargetBlocks)
            {
                this.m_control.Enabled |= !local.HasLocalPlayerAccess() ? false : this.Enabled(local);
            }
        }

        public void RedrawControl()
        {
            if (this.m_control != null)
            {
                this.m_control = this.CreateGui();
            }
        }

        bool ITerminalControl.IsVisible(MyTerminalBlock block) => 
            this.Visible((TBlock) block);

        public void UpdateVisual()
        {
            if (this.m_control != null)
            {
                this.OnUpdateVisual();
            }
        }

        MyTerminalBlock[] ITerminalControl.TargetBlocks { get; set; }

        protected ArrayOfTypeEnumerator<MyTerminalBlock, ArrayEnumerator<MyTerminalBlock>, TBlock> TargetBlocks =>
            ((ITerminalControl) this).TargetBlocks.OfTypeFast<MyTerminalBlock, TBlock>();

        protected TBlock FirstBlock
        {
            get
            {
                ArrayOfTypeEnumerator<MyTerminalBlock, ArrayEnumerator<MyTerminalBlock>, TBlock> enumerator;
                using (enumerator = this.TargetBlocks.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        TBlock current = enumerator.Current;
                        if (current.HasLocalPlayerAccess())
                        {
                            return current;
                        }
                    }
                }
                using (enumerator = this.TargetBlocks.GetEnumerator())
                {
                    TBlock current;
                    if (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    return current;
                }
            TR_0000:
                return default(TBlock);
            }
        }

        public bool SupportsMultipleBlocks { get; set; }

        public MyTerminalAction<TBlock>[] Actions { get; protected set; }

        ITerminalAction[] ITerminalControl.Actions =>
            this.Actions;

        string ITerminalControl.Id =>
            this.Id;

        string IMyTerminalControl.Id =>
            this.Id;

        Func<IMyTerminalBlock, bool> IMyTerminalControl.Enabled
        {
            get
            {
                Func<TBlock, bool> oldEnabled = this.Enabled;
                return x => oldEnabled((TBlock) x);
            }
            set => 
                (this.Enabled = (Func<TBlock, bool>) value);
        }

        Func<IMyTerminalBlock, bool> IMyTerminalControl.Visible
        {
            get
            {
                Func<TBlock, bool> oldVisible = this.Visible;
                return x => oldVisible((TBlock) x);
            }
            set => 
                (this.Visible = (Func<TBlock, bool>) value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalControl<TBlock>.<>c <>9;
            public static Func<TBlock, bool> <>9__21_0;
            public static Func<TBlock, bool> <>9__21_1;

            static <>c()
            {
                MyTerminalControl<TBlock>.<>c.<>9 = new MyTerminalControl<TBlock>.<>c();
            }

            internal bool <.ctor>b__21_0(TBlock b) => 
                true;

            internal bool <.ctor>b__21_1(TBlock b) => 
                true;
        }

        public delegate void AdvancedWriterDelegate(TBlock block, MyGuiControlBlockProperty control, StringBuilder writeTo);

        public delegate void WriterDelegate(TBlock block, StringBuilder writeTo);
    }
}

