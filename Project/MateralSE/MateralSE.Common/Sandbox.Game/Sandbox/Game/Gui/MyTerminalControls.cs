namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Plugins;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyTerminalControls : MySessionComponentBase, IMyTerminalControls
    {
        private static MyTerminalControls m_instance;
        private Dictionary<Type, Type> m_interfaceCache = new Dictionary<Type, Type>();
        [CompilerGenerated]
        private CustomControlGetDelegate m_customControlGetter;
        [CompilerGenerated]
        private CustomActionGetDelegate m_customActionGetter;

        public event CustomActionGetDelegate CustomActionGetter
        {
            add
            {
                this.m_customActionGetter += value;
            }
            remove
            {
                this.m_customActionGetter -= value;
            }
        }

        public event CustomControlGetDelegate CustomControlGetter
        {
            add
            {
                this.m_customControlGetter += value;
            }
            remove
            {
                this.m_customControlGetter -= value;
            }
        }

        private event CustomActionGetDelegate m_customActionGetter
        {
            [CompilerGenerated] add
            {
                CustomActionGetDelegate customActionGetter = this.m_customActionGetter;
                while (true)
                {
                    CustomActionGetDelegate a = customActionGetter;
                    CustomActionGetDelegate delegate4 = (CustomActionGetDelegate) Delegate.Combine(a, value);
                    customActionGetter = Interlocked.CompareExchange<CustomActionGetDelegate>(ref this.m_customActionGetter, delegate4, a);
                    if (ReferenceEquals(customActionGetter, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                CustomActionGetDelegate customActionGetter = this.m_customActionGetter;
                while (true)
                {
                    CustomActionGetDelegate source = customActionGetter;
                    CustomActionGetDelegate delegate4 = (CustomActionGetDelegate) Delegate.Remove(source, value);
                    customActionGetter = Interlocked.CompareExchange<CustomActionGetDelegate>(ref this.m_customActionGetter, delegate4, source);
                    if (ReferenceEquals(customActionGetter, source))
                    {
                        return;
                    }
                }
            }
        }

        private event CustomControlGetDelegate m_customControlGetter
        {
            [CompilerGenerated] add
            {
                CustomControlGetDelegate customControlGetter = this.m_customControlGetter;
                while (true)
                {
                    CustomControlGetDelegate a = customControlGetter;
                    CustomControlGetDelegate delegate4 = (CustomControlGetDelegate) Delegate.Combine(a, value);
                    customControlGetter = Interlocked.CompareExchange<CustomControlGetDelegate>(ref this.m_customControlGetter, delegate4, a);
                    if (ReferenceEquals(customControlGetter, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                CustomControlGetDelegate customControlGetter = this.m_customControlGetter;
                while (true)
                {
                    CustomControlGetDelegate source = customControlGetter;
                    CustomControlGetDelegate delegate4 = (CustomControlGetDelegate) Delegate.Remove(source, value);
                    customControlGetter = Interlocked.CompareExchange<CustomControlGetDelegate>(ref this.m_customControlGetter, delegate4, source);
                    if (ReferenceEquals(customControlGetter, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyTerminalControls()
        {
            m_instance = this;
            m_instance.ScanAssembly(MyPlugins.SandboxGameAssembly);
            m_instance.ScanAssembly(MyPlugins.GameAssembly);
        }

        public void AddAction<TBlock>(IMyTerminalAction action)
        {
            if (this.IsTypeValid<TBlock>())
            {
                Type producedType = this.GetProducedType<TBlock>();
                if (producedType != null)
                {
                    MyTerminalControlFactory.GetList(producedType).Actions.Add((ITerminalAction) action);
                }
            }
        }

        public void AddControl<TBlock>(IMyTerminalControl item)
        {
            if (this.IsTypeValid<TBlock>())
            {
                Type producedType = this.GetProducedType<TBlock>();
                if (producedType != null)
                {
                    MyTerminalControlFactory.AddControl(producedType, (ITerminalControl) item);
                    MyTerminalControlFactory.AddActions(producedType, (ITerminalControl) item);
                }
            }
        }

        public IMyTerminalAction CreateAction<TBlock>(string id)
        {
            if (!this.IsTypeValid<TBlock>())
            {
                return null;
            }
            Type producedType = this.GetProducedType<TBlock>();
            if (producedType == null)
            {
                return null;
            }
            Type[] typeArguments = new Type[] { producedType };
            object[] args = new object[] { id, new StringBuilder(""), "" };
            return (IMyTerminalAction) Activator.CreateInstance(typeof(MyTerminalAction<>).MakeGenericType(typeArguments), args);
        }

        public TControl CreateControl<TControl, TBlock>(string id)
        {
            if (!this.IsTypeValid<TBlock>())
            {
                return default(TControl);
            }
            Type producedType = this.GetProducedType<TBlock>();
            if (producedType == null)
            {
                return default(TControl);
            }
            if (!typeof(MyTerminalBlock).IsAssignableFrom(producedType))
            {
                return default(TControl);
            }
            if (!typeof(IMyTerminalControl).IsAssignableFrom(typeof(TControl)))
            {
                return default(TControl);
            }
            if (!MyTerminalControlFactory.AreControlsCreated(producedType))
            {
                MyTerminalControlFactory.EnsureControlsAreCreated(producedType);
            }
            Type type2 = typeof(TControl);
            if (type2 == typeof(IMyTerminalControlTextbox))
            {
                object[] objArray1 = new object[] { id, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlTextbox<>), producedType, objArray1);
            }
            if (type2 == typeof(IMyTerminalControlButton))
            {
                object[] objArray2 = new object[4];
                objArray2[0] = id;
                objArray2[1] = MyStringId.NullOrEmpty;
                objArray2[2] = MyStringId.NullOrEmpty;
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlButton<>), producedType, objArray2);
            }
            if (type2 == typeof(IMyTerminalControlCheckbox))
            {
                object[] objArray3 = new object[] { id, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlCheckbox<>), producedType, objArray3);
            }
            if (type2 == typeof(IMyTerminalControlColor))
            {
                object[] objArray4 = new object[] { id, MyStringId.NullOrEmpty };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlColor<>), producedType, objArray4);
            }
            if (type2 == typeof(IMyTerminalControlCombobox))
            {
                object[] objArray5 = new object[] { id, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlCombobox<>), producedType, objArray5);
            }
            if (type2 == typeof(IMyTerminalControlListbox))
            {
                object[] objArray6 = new object[] { id, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty, false, 0 };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlListbox<>), producedType, objArray6);
            }
            if (type2 == typeof(IMyTerminalControlOnOffSwitch))
            {
                object[] objArray7 = new object[] { id, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlOnOffSwitch<>), producedType, objArray7);
            }
            if (type2 == typeof(IMyTerminalControlSeparator))
            {
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlSeparator<>), producedType, new object[0]);
            }
            if (type2 == typeof(IMyTerminalControlSlider))
            {
                object[] objArray8 = new object[] { id, MyStringId.NullOrEmpty, MyStringId.NullOrEmpty };
                return this.CreateGenericControl<TControl>(typeof(MyTerminalControlSlider<>), producedType, objArray8);
            }
            if (!(type2 == typeof(IMyTerminalControlLabel)))
            {
                return default(TControl);
            }
            object[] args = new object[] { MyStringId.NullOrEmpty };
            return this.CreateGenericControl<TControl>(typeof(MyTerminalControlLabel<>), producedType, args);
        }

        private TControl CreateGenericControl<TControl>(Type controlType, Type blockType, object[] args)
        {
            Type[] typeArguments = new Type[] { blockType };
            return (TControl) ((IMyTerminalControl) Activator.CreateInstance(controlType.MakeGenericType(typeArguments), args));
        }

        public IMyTerminalControlProperty<TValue> CreateProperty<TValue, TBlock>(string id)
        {
            if (!this.IsTypeValid<TBlock>())
            {
                return null;
            }
            Type producedType = this.GetProducedType<TBlock>();
            if (producedType == null)
            {
                return null;
            }
            Type[] typeArguments = new Type[] { producedType, typeof(TValue) };
            object[] args = new object[] { id };
            return (IMyTerminalControlProperty<TValue>) Activator.CreateInstance(typeof(MyTerminalControlProperty<,>).MakeGenericType(typeArguments), args);
        }

        private Type FindTerminalTypeFromInterface<TBlock>()
        {
            Type type2;
            Type key = typeof(TBlock);
            if (!key.IsInterface)
            {
                throw new ArgumentException("Given type is not an interface!");
            }
            if (this.m_interfaceCache.TryGetValue(key, out type2))
            {
                return type2;
            }
            this.ScanAssembly(Assembly.GetExecutingAssembly());
            return (!this.m_interfaceCache.TryGetValue(key, out type2) ? null : type2);
        }

        public List<ITerminalAction> GetActions(Sandbox.ModAPI.IMyTerminalBlock block)
        {
            if (this.m_customActionGetter == null)
            {
                return MyTerminalControlFactory.GetActions(block.GetType()).ToList<ITerminalAction>();
            }
            List<IMyTerminalAction> actions = MyTerminalControlFactory.GetActions(block.GetType()).Cast<IMyTerminalAction>().ToList<IMyTerminalAction>();
            this.m_customActionGetter(block, actions);
            return actions.Cast<ITerminalAction>().ToList<ITerminalAction>();
        }

        public void GetActions<TBlock>(out List<IMyTerminalAction> items)
        {
            items = new List<IMyTerminalAction>();
            if (this.IsTypeValid<TBlock>())
            {
                Type producedType = this.GetProducedType<TBlock>();
                if (producedType != null)
                {
                    foreach (ITerminalAction action in MyTerminalControlFactory.GetList(producedType).Actions)
                    {
                        items.Add((IMyTerminalAction) action);
                    }
                }
            }
        }

        public List<ITerminalControl> GetControls(Sandbox.ModAPI.IMyTerminalBlock block)
        {
            if (this.m_customControlGetter == null)
            {
                return MyTerminalControlFactory.GetControls(block.GetType()).ToList<ITerminalControl>();
            }
            List<IMyTerminalControl> controls = MyTerminalControlFactory.GetControls(block.GetType()).Cast<IMyTerminalControl>().ToList<IMyTerminalControl>();
            this.m_customControlGetter(block, controls);
            return controls.Cast<ITerminalControl>().ToList<ITerminalControl>();
        }

        public void GetControls<TBlock>(out List<IMyTerminalControl> items)
        {
            items = new List<IMyTerminalControl>();
            if (this.IsTypeValid<TBlock>())
            {
                Type producedType = this.GetProducedType<TBlock>();
                if (producedType != null)
                {
                    foreach (ITerminalControl control in MyTerminalControlFactory.GetList(producedType).Controls)
                    {
                        items.Add((IMyTerminalControl) control);
                    }
                }
            }
        }

        private Type GetProducedType<TBlock>() => 
            (!typeof(TBlock).IsInterface ? MyCubeBlockFactory.GetProducedType(typeof(TBlock)) : this.FindTerminalTypeFromInterface<TBlock>());

        private bool IsTypeValid<TBlock>() => 
            (typeof(TBlock).IsInterface ? typeof(Sandbox.ModAPI.Ingame.IMyTerminalBlock).IsAssignableFrom(typeof(TBlock)) : !typeof(MyObjectBuilder_TerminalBlock).IsAssignableFrom(typeof(TBlock)));

        public void RemoveAction<TBlock>(IMyTerminalAction action)
        {
            if (this.IsTypeValid<TBlock>())
            {
                Type producedType = this.GetProducedType<TBlock>();
                if (producedType != null)
                {
                    MyTerminalControlFactory.GetList(producedType).Actions.Remove((ITerminalAction) action);
                }
            }
        }

        public void RemoveControl<TBlock>(IMyTerminalControl item)
        {
            if (this.IsTypeValid<TBlock>())
            {
                Type producedType = this.GetProducedType<TBlock>();
                if (producedType != null)
                {
                    MyTerminalControlFactory.RemoveControl(producedType, item);
                }
            }
        }

        private void ScanAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                MyTerminalInterfaceAttribute customAttribute = type.GetCustomAttribute<MyTerminalInterfaceAttribute>();
                if (customAttribute != null)
                {
                    foreach (Type type2 in customAttribute.LinkedTypes)
                    {
                        this.m_interfaceCache[type2] = type;
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            this.m_customControlGetter = null;
        }

        public static MyTerminalControls Static =>
            m_instance;
    }
}

