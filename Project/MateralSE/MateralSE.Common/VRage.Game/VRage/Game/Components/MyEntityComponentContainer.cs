namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game;
    using VRage.ModAPI;

    public class MyEntityComponentContainer : MyComponentContainer, IMyComponentContainer
    {
        private IMyEntity m_entity;
        [CompilerGenerated]
        private Action<Type, MyEntityComponentBase> ComponentAdded;
        [CompilerGenerated]
        private Action<Type, MyEntityComponentBase> ComponentRemoved;

        public event Action<Type, MyEntityComponentBase> ComponentAdded
        {
            [CompilerGenerated] add
            {
                Action<Type, MyEntityComponentBase> componentAdded = this.ComponentAdded;
                while (true)
                {
                    Action<Type, MyEntityComponentBase> a = componentAdded;
                    Action<Type, MyEntityComponentBase> action3 = (Action<Type, MyEntityComponentBase>) Delegate.Combine(a, value);
                    componentAdded = Interlocked.CompareExchange<Action<Type, MyEntityComponentBase>>(ref this.ComponentAdded, action3, a);
                    if (ReferenceEquals(componentAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Type, MyEntityComponentBase> componentAdded = this.ComponentAdded;
                while (true)
                {
                    Action<Type, MyEntityComponentBase> source = componentAdded;
                    Action<Type, MyEntityComponentBase> action3 = (Action<Type, MyEntityComponentBase>) Delegate.Remove(source, value);
                    componentAdded = Interlocked.CompareExchange<Action<Type, MyEntityComponentBase>>(ref this.ComponentAdded, action3, source);
                    if (ReferenceEquals(componentAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<Type, MyEntityComponentBase> ComponentRemoved
        {
            [CompilerGenerated] add
            {
                Action<Type, MyEntityComponentBase> componentRemoved = this.ComponentRemoved;
                while (true)
                {
                    Action<Type, MyEntityComponentBase> a = componentRemoved;
                    Action<Type, MyEntityComponentBase> action3 = (Action<Type, MyEntityComponentBase>) Delegate.Combine(a, value);
                    componentRemoved = Interlocked.CompareExchange<Action<Type, MyEntityComponentBase>>(ref this.ComponentRemoved, action3, a);
                    if (ReferenceEquals(componentRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Type, MyEntityComponentBase> componentRemoved = this.ComponentRemoved;
                while (true)
                {
                    Action<Type, MyEntityComponentBase> source = componentRemoved;
                    Action<Type, MyEntityComponentBase> action3 = (Action<Type, MyEntityComponentBase>) Delegate.Remove(source, value);
                    componentRemoved = Interlocked.CompareExchange<Action<Type, MyEntityComponentBase>>(ref this.ComponentRemoved, action3, source);
                    if (ReferenceEquals(componentRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyEntityComponentContainer(IMyEntity entity)
        {
            this.Entity = entity;
        }

        public override void Init(MyContainerDefinition definition)
        {
            if (definition.Flags != null)
            {
                IMyEntity entity = this.Entity;
                entity.Flags |= (EntityFlags) definition.Flags.Value;
            }
        }

        protected override void OnComponentAdded(Type t, MyComponentBase component)
        {
            base.OnComponentAdded(t, component);
            MyEntityComponentBase base2 = component as MyEntityComponentBase;
            Action<Type, MyEntityComponentBase> componentAdded = this.ComponentAdded;
            if ((componentAdded != null) && (base2 != null))
            {
                componentAdded(t, base2);
            }
        }

        protected override void OnComponentRemoved(Type t, MyComponentBase component)
        {
            base.OnComponentRemoved(t, component);
            MyEntityComponentBase base2 = component as MyEntityComponentBase;
            if (base2 != null)
            {
                this.ComponentRemoved.InvokeIfNotNull<Type, MyEntityComponentBase>(t, base2);
            }
        }

        public IMyEntity Entity
        {
            get => 
                this.m_entity;
            private set => 
                (this.m_entity = value);
        }
    }
}

