namespace Sandbox.Game.Screens.Terminal.Controls
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Library.Collections;

    public abstract class MyTerminalValueControl<TBlock, TValue> : MyTerminalControl<TBlock>, ITerminalValueControl<TBlock, TValue>, ITerminalProperty<TValue>, ITerminalProperty, ITerminalControl, ITerminalControlSync, IMyTerminalValueControl<TValue> where TBlock: MyTerminalBlock
    {
        public SerializerDelegate<TBlock, TValue> Serializer;

        public MyTerminalValueControl(string id) : base(id)
        {
        }

        public abstract TValue GetDefaultValue(TBlock block);
        public TValue GetDefaultValue(IMyCubeBlock block) => 
            this.GetDefaultValue((TBlock) block);

        public abstract TValue GetMaximum(TBlock block);
        public TValue GetMaximum(IMyCubeBlock block) => 
            this.GetMaximum((TBlock) block);

        public abstract TValue GetMinimum(TBlock block);
        public TValue GetMinimum(IMyCubeBlock block) => 
            this.GetMinimum((TBlock) block);

        [Obsolete("Use GetMinimum instead")]
        public TValue GetMininum(TBlock block) => 
            this.GetMinimum(block);

        [Obsolete("Use GetMinimum instead")]
        public TValue GetMininum(IMyCubeBlock block) => 
            this.GetMinimum((TBlock) block);

        public virtual TValue GetValue(TBlock block) => 
            this.Getter(block);

        public TValue GetValue(IMyCubeBlock block) => 
            this.GetValue((TBlock) block);

        public virtual void Serialize(BitStream stream, TBlock block)
        {
            if (!stream.Reading)
            {
                TValue local2 = this.GetValue(block);
                this.Serializer(stream, ref local2);
            }
            else
            {
                TValue local = default(TValue);
                this.Serializer(stream, ref local);
                this.SetValue(block, local);
            }
        }

        public void Serialize(BitStream stream, MyTerminalBlock block)
        {
            this.Serialize(stream, (TBlock) block);
        }

        public virtual void SetValue(TBlock block, TValue value)
        {
            this.Setter(block, value);
            block.NotifyTerminalValueChanged(this);
        }

        public void SetValue(IMyCubeBlock block, TValue value)
        {
            this.SetValue((TBlock) block, value);
        }

        public GetterDelegate<TBlock, TValue> Getter { get; set; }

        public SetterDelegate<TBlock, TValue> Setter { get; set; }

        public Expression<Func<TBlock, TValue>> MemberExpression
        {
            set
            {
                this.Getter = new GetterDelegate<TBlock, TValue>(value.CreateGetter<TBlock, TValue>().Invoke);
                this.Setter = new SetterDelegate<TBlock, TValue>(value.CreateSetter<TBlock, TValue>().Invoke);
            }
        }

        string ITerminalProperty.Id =>
            base.Id;

        string ITerminalProperty.TypeName =>
            typeof(TValue).Name;

        Func<IMyTerminalBlock, TValue> IMyTerminalValueControl<TValue>.Getter
        {
            get
            {
                GetterDelegate<TBlock, TValue> oldGetter = this.Getter;
                return new Func<IMyTerminalBlock, TValue>(class_1.<Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalValueControl<TValue>.get_Getter>b__0);
            }
            set => 
                (this.Getter = new GetterDelegate<TBlock, TValue>(value.Invoke));
        }

        Action<IMyTerminalBlock, TValue> IMyTerminalValueControl<TValue>.Setter
        {
            get
            {
                SetterDelegate<TBlock, TValue> oldSetter = this.Setter;
                return new Action<IMyTerminalBlock, TValue>(class_1.<Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalValueControl<TValue>.get_Setter>b__0);
            }
            set => 
                (this.Setter = new SetterDelegate<TBlock, TValue>(value.Invoke));
        }

        public delegate void ExternalSetterDelegate(IMyTerminalBlock block, TValue value);

        public delegate TValue GetterDelegate(TBlock block);

        public delegate void SerializerDelegate(BitStream stream, ref TValue value);

        public delegate void SetterDelegate(TBlock block, TValue value);
    }
}

