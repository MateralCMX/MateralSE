namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    public abstract class MyComponentBase
    {
        private MyComponentContainer m_container;

        protected MyComponentBase()
        {
        }

        public virtual void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
        }

        public virtual T GetAs<T>() where T: MyComponentBase => 
            (this as T);

        public virtual void Init(MyComponentDefinitionBase definition)
        {
        }

        public virtual bool IsSerialized() => 
            false;

        public virtual void OnAddedToContainer()
        {
        }

        public virtual void OnAddedToScene()
        {
        }

        public virtual void OnBeforeRemovedFromContainer()
        {
        }

        public virtual void OnRemovedFromScene()
        {
        }

        public virtual MyObjectBuilder_ComponentBase Serialize(bool copy = false) => 
            MyComponentFactory.CreateObjectBuilder(this);

        public virtual void SetContainer(MyComponentContainer container)
        {
            if (this.m_container != null)
            {
                this.OnBeforeRemovedFromContainer();
            }
            this.m_container = container;
            IMyComponentAggregate aggregate = this as IMyComponentAggregate;
            if (aggregate != null)
            {
                using (List<MyComponentBase>.Enumerator enumerator = aggregate.ChildList.Reader.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetContainer(container);
                    }
                }
            }
            if (container != null)
            {
                this.OnAddedToContainer();
            }
        }

        public MyComponentContainer ContainerBase =>
            this.m_container;
    }
}

