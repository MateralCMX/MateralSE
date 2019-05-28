namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Input;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRageMath;

    public class MyTerminalControlSlider<TBlock> : MyTerminalValueControl<TBlock, float>, IMyTerminalControlSlider, IMyTerminalControl, IMyTerminalValueControl<float>, ITerminalProperty, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        public MyStringId Title;
        public MyStringId Tooltip;
        private MyGuiControlSlider m_slider;
        private MyGuiControlBlockProperty m_control;
        private Action<float> m_amountConfirmed;
        public bool AmountDialogEnabled;
        public MyTerminalControl<TBlock>.WriterDelegate Writer;
        public MyTerminalControl<TBlock>.WriterDelegate CompactWriter;
        public MyTerminalControl<TBlock>.AdvancedWriterDelegate AdvancedWriter;
        public FloatFunc<TBlock> Normalizer;
        public FloatFunc<TBlock> Denormalizer;
        public MyTerminalValueControl<TBlock, float>.GetterDelegate DefaultValueGetter;
        private Action<MyGuiControlSlider> m_valueChanged;

        public MyTerminalControlSlider(string id, MyStringId title, MyStringId tooltip) : base(id)
        {
            this.AmountDialogEnabled = true;
            this.Normalizer = (b, f) => f;
            this.Denormalizer = (b, f) => f;
            this.Title = title;
            this.Tooltip = tooltip;
            this.CompactWriter = new MyTerminalControl<TBlock>.WriterDelegate(this.CompactWriterMethod);
            this.m_amountConfirmed = new Action<float>(this.AmountSetter);
            this.Serializer = (stream, value) => stream.Serialize(ref value);
        }

        private void ActionWriter(TBlock block, StringBuilder appendTo)
        {
            (this.CompactWriter ?? this.Writer)(block, appendTo);
        }

        private void AmountSetter(float value)
        {
            TBlock firstBlock = base.FirstBlock;
            if (firstBlock != null)
            {
                this.m_slider.Value = this.Normalizer(firstBlock, value);
            }
        }

        public void CompactWriterMethod(TBlock block, StringBuilder appendTo)
        {
            int length = appendTo.Length;
            this.Writer(block, appendTo);
            int index = this.FirstIndexOf(appendTo, length, ".,", 0x7fffffff);
            if (index >= 0)
            {
                this.RemoveNumbersFrom(index, appendTo);
            }
        }

        protected override MyGuiControlBase CreateGui()
        {
            float width = MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH;
            float? defaultValue = null;
            VRageMath.Vector4? color = null;
            this.m_slider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 0f, 1f, width, defaultValue, color, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            this.m_valueChanged = new Action<MyGuiControlSlider>(this.OnValueChange);
            this.m_slider.ValueChanged = this.m_valueChanged;
            this.m_slider.SliderClicked = new Func<MyGuiControlSlider, bool>(this.OnSliderClicked);
            this.m_control = new MyGuiControlBlockProperty(MyTexts.GetString(this.Title), MyTexts.GetString(this.Tooltip), this.m_slider, MyGuiControlBlockPropertyLayoutEnum.Vertical, true);
            return this.m_control;
        }

        private void DecreaseAction(TBlock block, float step)
        {
            float num = this.Normalizer(block, this.GetValue(block));
            this.SetValue(block, this.Denormalizer(block, MathHelper.Clamp((float) (num - step), (float) 0f, (float) 1f)));
        }

        private static float DualLogDenormalizer(TBlock block, float value, float min, float max, float centerBand)
        {
            float num = (value * 2f) - 1f;
            return ((Math.Abs(num) >= centerBand) ? (MathHelper.Clamp(MathHelper.InterpLog((Math.Abs(num) - centerBand) / (1f - centerBand), min, max), min, max) * Math.Sign(num)) : 0f);
        }

        private static float DualLogNormalizer(TBlock block, float value, float min, float max, float centerBand)
        {
            if (Math.Abs(value) < min)
            {
                return 0.5f;
            }
            float num = 0.5f - (centerBand / 2f);
            float num2 = MathHelper.Clamp(MathHelper.InterpLogInv(Math.Abs(value), min, max), 0f, 1f) * num;
            return ((value >= 0f) ? ((num2 + num) + centerBand) : (num - num2));
        }

        public void EnableActions(string increaseIcon, string decreaseIcon, StringBuilder increaseName, StringBuilder decreaseName, float step, string resetIcon = null, StringBuilder resetName = null, Func<TBlock, bool> enabled = null, Func<TBlock, bool> callable = null)
        {
            MyTerminalAction<TBlock> action = new MyTerminalAction<TBlock>("Increase" + base.Id, increaseName, b => ((MyTerminalControlSlider<TBlock>) this).IncreaseAction(b, step), new MyTerminalControl<TBlock>.WriterDelegate(this.ActionWriter), increaseIcon, enabled, callable);
            MyTerminalAction<TBlock> action2 = new MyTerminalAction<TBlock>("Decrease" + base.Id, decreaseName, b => ((MyTerminalControlSlider<TBlock>) this).DecreaseAction(b, step), new MyTerminalControl<TBlock>.WriterDelegate(this.ActionWriter), decreaseIcon, enabled, callable);
            if (resetIcon == null)
            {
                MyTerminalAction<TBlock>[] actions = new MyTerminalAction<TBlock>[] { action, action2 };
                this.SetActions(actions);
            }
            else
            {
                MyTerminalAction<TBlock>[] actions = new MyTerminalAction<TBlock>[] { action, action2, new MyTerminalAction<TBlock>("Reset" + base.Id, resetName, new Action<TBlock>(this.ResetAction), new MyTerminalControl<TBlock>.WriterDelegate(this.ActionWriter), resetIcon, enabled, callable) };
                this.SetActions(actions);
            }
        }

        private int FirstIndexOf(StringBuilder sb, int start, string chars, int count = 0x7fffffff)
        {
            int num = Math.Min(start + count, sb.Length);
            int num2 = start;
            while (num2 < num)
            {
                char ch = sb[num2];
                int num3 = 0;
                while (true)
                {
                    if (num3 >= chars.Length)
                    {
                        num2++;
                        break;
                    }
                    if (ch == chars[num3])
                    {
                        return num2;
                    }
                    num3++;
                }
            }
            return -1;
        }

        public override float GetDefaultValue(TBlock block) => 
            this.DefaultValueGetter(block);

        public override float GetMaximum(TBlock block) => 
            this.Denormalizer(block, 1f);

        public override float GetMinimum(TBlock block) => 
            this.Denormalizer(block, 0f);

        public override float GetValue(TBlock block) => 
            base.GetValue(block);

        private void IncreaseAction(TBlock block, float step)
        {
            float num = this.Normalizer(block, this.GetValue(block));
            this.SetValue(block, this.Denormalizer(block, MathHelper.Clamp((float) (num + step), (float) 0f, (float) 1f)));
        }

        private bool OnSliderClicked(MyGuiControlSlider arg)
        {
            TBlock firstBlock = base.FirstBlock;
            if ((!this.AmountDialogEnabled || !MyInput.Static.IsAnyCtrlKeyPressed()) || (firstBlock == null))
            {
                return false;
            }
            float num2 = this.Denormalizer(firstBlock, arg.Value);
            MyGuiScreenDialogAmount screen = new MyGuiScreenDialogAmount(this.Denormalizer(firstBlock, 0f), this.Denormalizer(firstBlock, 1f), MyCommonTexts.DialogAmount_SetValueCaption, 3, false, new float?(num2), MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity);
            screen.OnConfirmed += this.m_amountConfirmed;
            MyGuiSandbox.AddScreen(screen);
            return true;
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            TBlock firstBlock = base.FirstBlock;
            if ((firstBlock != null) && (this.m_slider != null))
            {
                float? nullable1;
                this.m_slider.ValueChanged = null;
                if (this.DefaultValueGetter != null)
                {
                    nullable1 = new float?(this.Normalizer(firstBlock, this.DefaultValueGetter(firstBlock)));
                }
                else
                {
                    nullable1 = null;
                }
                this.m_slider.DefaultValue = nullable1;
                this.m_slider.Value = this.Normalizer(firstBlock, this.GetValue(firstBlock));
                this.m_slider.ValueChanged = this.m_valueChanged;
                this.UpdateDetailedInfo(firstBlock);
            }
        }

        private void OnValueChange(MyGuiControlSlider slider)
        {
            this.SetValue(slider.Value);
            this.UpdateDetailedInfo(base.FirstBlock);
        }

        private void RemoveNumbersFrom(int index, StringBuilder sb)
        {
            sb.Remove(index, 1);
            while ((index < sb.Length) && (((sb[index] >= '0') && (sb[index] <= '9')) || (sb[index] == ' ')))
            {
                sb.Remove(index, 1);
            }
            if ((sb[0] == '-') && (sb[1] == '0'))
            {
                sb.Remove(0, 1);
            }
        }

        private void ResetAction(TBlock block)
        {
            if (this.DefaultValueGetter != null)
            {
                this.SetValue(block, this.DefaultValueGetter(block));
            }
        }

        void IMyTerminalControlSlider.SetDualLogLimits(Func<IMyTerminalBlock, float> minGetter, Func<IMyTerminalBlock, float> maxGetter, float centerBand)
        {
            MyTerminalValueControl<TBlock, float>.GetterDelegate delegate2 = new MyTerminalValueControl<TBlock, float>.GetterDelegate(minGetter.Invoke);
            this.SetDualLogLimits(delegate2, new MyTerminalValueControl<TBlock, float>.GetterDelegate(maxGetter.Invoke), centerBand);
        }

        void IMyTerminalControlSlider.SetLimits(Func<IMyTerminalBlock, float> minGetter, Func<IMyTerminalBlock, float> maxGetter)
        {
            MyTerminalValueControl<TBlock, float>.GetterDelegate delegate2 = new MyTerminalValueControl<TBlock, float>.GetterDelegate(minGetter.Invoke);
            this.SetLimits(delegate2, new MyTerminalValueControl<TBlock, float>.GetterDelegate(maxGetter.Invoke));
        }

        void IMyTerminalControlSlider.SetLogLimits(Func<IMyTerminalBlock, float> minGetter, Func<IMyTerminalBlock, float> maxGetter)
        {
            MyTerminalValueControl<TBlock, float>.GetterDelegate delegate2 = new MyTerminalValueControl<TBlock, float>.GetterDelegate(minGetter.Invoke);
            this.SetLogLimits(delegate2, new MyTerminalValueControl<TBlock, float>.GetterDelegate(maxGetter.Invoke));
        }

        private void SetActions(params MyTerminalAction<TBlock>[] actions)
        {
            base.Actions = actions;
        }

        public void SetDualLogLimits(MyTerminalValueControl<TBlock, float>.GetterDelegate minGetter, MyTerminalValueControl<TBlock, float>.GetterDelegate maxGetter, float centerBand)
        {
            this.Normalizer = (block, f) => MyTerminalControlSlider<TBlock>.DualLogNormalizer(block, f, minGetter(block), maxGetter(block), centerBand);
            this.Denormalizer = (block, f) => MyTerminalControlSlider<TBlock>.DualLogDenormalizer(block, f, minGetter(block), maxGetter(block), centerBand);
        }

        public void SetDualLogLimits(float absMin, float absMax, float centerBand)
        {
            this.Normalizer = (block, f) => MyTerminalControlSlider<TBlock>.DualLogNormalizer(block, f, absMin, absMax, centerBand);
            this.Denormalizer = (block, f) => MyTerminalControlSlider<TBlock>.DualLogDenormalizer(block, f, absMin, absMax, centerBand);
        }

        public void SetLimits(MyTerminalValueControl<TBlock, float>.GetterDelegate minGetter, MyTerminalValueControl<TBlock, float>.GetterDelegate maxGetter)
        {
            this.Normalizer = delegate (TBlock block, float f) {
                float num = minGetter(block);
                float num2 = maxGetter(block);
                return MathHelper.Clamp((float) ((f - num) / (num2 - num)), (float) 0f, (float) 1f);
            };
            this.Denormalizer = delegate (TBlock block, float f) {
                float min = minGetter(block);
                float max = maxGetter(block);
                return MathHelper.Clamp(min + (f * (max - min)), min, max);
            };
        }

        public void SetLimits(float min, float max)
        {
            this.Normalizer = (block, f) => MathHelper.Clamp((float) ((f - min) / (max - min)), (float) 0f, (float) 1f);
            this.Denormalizer = (block, f) => MathHelper.Clamp(min + (f * (max - min)), min, max);
        }

        public void SetLogLimits(MyTerminalValueControl<TBlock, float>.GetterDelegate minGetter, MyTerminalValueControl<TBlock, float>.GetterDelegate maxGetter)
        {
            this.Normalizer = (block, f) => MathHelper.Clamp(MathHelper.InterpLogInv(f, minGetter(block), maxGetter(block)), 0f, 1f);
            this.Denormalizer = delegate (TBlock block, float f) {
                float min = minGetter(block);
                float max = maxGetter(block);
                return MathHelper.Clamp(MathHelper.InterpLog(f, min, max), min, max);
            };
        }

        public void SetLogLimits(float min, float max)
        {
            this.Normalizer = (block, f) => MathHelper.Clamp(MathHelper.InterpLogInv(f, min, max), 0f, 1f);
            this.Denormalizer = (block, f) => MathHelper.Clamp(MathHelper.InterpLog(f, min, max), min, max);
        }

        private void SetValue(float value)
        {
            foreach (TBlock local in base.TargetBlocks)
            {
                this.SetValue(local, this.Denormalizer(local, value));
            }
        }

        public override void SetValue(TBlock block, float value)
        {
            base.SetValue(block, MathHelper.Clamp(value, this.Denormalizer(block, 0f), this.Denormalizer(block, 1f)));
        }

        private void UpdateDetailedInfo(TBlock block)
        {
            if (this.AdvancedWriter != null)
            {
                this.m_control.SetDetailedInfo<TBlock>(this.AdvancedWriter, block);
            }
            else
            {
                this.m_control.SetDetailedInfo<TBlock>(this.Writer, block);
            }
        }

        public float? DefaultValue
        {
            set => 
                (this.DefaultValueGetter = (value != null) ? block => value.Value : null);
        }

        public string Formatter
        {
            set => 
                (this.Writer = (value != null) ? (block, result) => result.AppendFormat(value, ((MyTerminalControlSlider<TBlock>) this).GetValue(block)) : null);
        }

        MyStringId IMyTerminalControlTitleTooltip.Title
        {
            get => 
                this.Title;
            set => 
                (this.Title = value);
        }

        MyStringId IMyTerminalControlTitleTooltip.Tooltip
        {
            get => 
                this.Tooltip;
            set => 
                (this.Tooltip = value);
        }

        Action<IMyTerminalBlock, StringBuilder> IMyTerminalControlSlider.Writer
        {
            get
            {
                MyTerminalControl<TBlock>.WriterDelegate oldWriter = this.Writer;
                return delegate (IMyTerminalBlock x, StringBuilder y) {
                    oldWriter((TBlock) x, y);
                };
            }
            set => 
                (this.Writer = new MyTerminalControl<TBlock>.WriterDelegate(value.Invoke));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalControlSlider<TBlock>.<>c <>9;
            public static MyTerminalValueControl<TBlock, float>.SerializerDelegate <>9__18_0;
            public static MyTerminalControlSlider<TBlock>.FloatFunc <>9__18_1;
            public static MyTerminalControlSlider<TBlock>.FloatFunc <>9__18_2;

            static <>c()
            {
                MyTerminalControlSlider<TBlock>.<>c.<>9 = new MyTerminalControlSlider<TBlock>.<>c();
            }

            internal void <.ctor>b__18_0(BitStream stream, ref float value)
            {
                stream.Serialize(ref value);
            }

            internal float <.ctor>b__18_1(TBlock b, float f) => 
                f;

            internal float <.ctor>b__18_2(TBlock b, float f) => 
                f;
        }

        public delegate float FloatFunc(TBlock block, float val);
    }
}

