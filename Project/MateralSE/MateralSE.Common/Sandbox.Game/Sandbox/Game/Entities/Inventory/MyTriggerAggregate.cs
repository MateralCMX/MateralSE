namespace Sandbox.Game.Entities.Inventory
{
    using Sandbox.Game.Components;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyComponentBuilder(typeof(MyObjectBuilder_TriggerAggregate), true)]
    public class MyTriggerAggregate : MyEntityComponentBase, IMyComponentAggregate
    {
        private int m_triggerCount;
        [CompilerGenerated]
        private Action<MyTriggerAggregate, int> OnTriggerCountChanged;
        private MyAggregateComponentList m_children = new MyAggregateComponentList();

        public event Action<MyTriggerAggregate, int> OnTriggerCountChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTriggerAggregate, int> onTriggerCountChanged = this.OnTriggerCountChanged;
                while (true)
                {
                    Action<MyTriggerAggregate, int> a = onTriggerCountChanged;
                    Action<MyTriggerAggregate, int> action3 = (Action<MyTriggerAggregate, int>) Delegate.Combine(a, value);
                    onTriggerCountChanged = Interlocked.CompareExchange<Action<MyTriggerAggregate, int>>(ref this.OnTriggerCountChanged, action3, a);
                    if (ReferenceEquals(onTriggerCountChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTriggerAggregate, int> onTriggerCountChanged = this.OnTriggerCountChanged;
                while (true)
                {
                    Action<MyTriggerAggregate, int> source = onTriggerCountChanged;
                    Action<MyTriggerAggregate, int> action3 = (Action<MyTriggerAggregate, int>) Delegate.Remove(source, value);
                    onTriggerCountChanged = Interlocked.CompareExchange<Action<MyTriggerAggregate, int>>(ref this.OnTriggerCountChanged, action3, source);
                    if (ReferenceEquals(onTriggerCountChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void AfterComponentAdd(MyComponentBase component)
        {
            if (component is MyTriggerComponent)
            {
                int triggerCount = this.TriggerCount;
                this.TriggerCount = triggerCount + 1;
            }
            else if (component is MyTriggerAggregate)
            {
                (component as MyTriggerAggregate).OnTriggerCountChanged += new Action<MyTriggerAggregate, int>(this.OnChildAggregateCountChanged);
                this.TriggerCount += (component as MyTriggerAggregate).TriggerCount;
            }
        }

        public void BeforeComponentRemove(MyComponentBase component)
        {
            if (component is MyTriggerComponent)
            {
                int triggerCount = this.TriggerCount;
                this.TriggerCount = triggerCount - 1;
            }
            else if (component is MyTriggerAggregate)
            {
                (component as MyTriggerAggregate).OnTriggerCountChanged -= new Action<MyTriggerAggregate, int>(this.OnChildAggregateCountChanged);
                this.TriggerCount -= (component as MyTriggerAggregate).TriggerCount;
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_TriggerAggregate aggregate = builder as MyObjectBuilder_TriggerAggregate;
            if ((aggregate != null) && (aggregate.AreaTriggers != null))
            {
                foreach (MyObjectBuilder_TriggerBase base2 in aggregate.AreaTriggers)
                {
                    MyComponentBase component = MyComponentFactory.CreateInstanceByTypeId(base2.TypeId);
                    component.Deserialize(base2);
                    this.AddComponent(component);
                }
            }
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            using (List<MyComponentBase>.Enumerator enumerator = this.ChildList.Reader.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OnAddedToScene();
                }
            }
        }

        private void OnChildAggregateCountChanged(MyTriggerAggregate obj, int change)
        {
            this.TriggerCount += change;
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            using (List<MyComponentBase>.Enumerator enumerator = this.ChildList.Reader.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OnRemovedFromScene();
                }
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_TriggerAggregate aggregate = base.Serialize(false) as MyObjectBuilder_TriggerAggregate;
            ListReader<MyComponentBase> reader = this.m_children.Reader;
            if (reader.Count > 0)
            {
                aggregate.AreaTriggers = new List<MyObjectBuilder_TriggerBase>(reader.Count);
                using (List<MyComponentBase>.Enumerator enumerator = reader.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyObjectBuilder_TriggerBase item = enumerator.Current.Serialize(false) as MyObjectBuilder_TriggerBase;
                        if (item != null)
                        {
                            aggregate.AreaTriggers.Add(item);
                        }
                    }
                }
            }
            return aggregate;
        }

        public override string ComponentTypeDebugString =>
            "TriggerAggregate";

        public int TriggerCount
        {
            get => 
                this.m_triggerCount;
            private set
            {
                if (this.m_triggerCount != value)
                {
                    int num = value - this.m_triggerCount;
                    this.m_triggerCount = value;
                    if (this.OnTriggerCountChanged != null)
                    {
                        this.OnTriggerCountChanged(this, num);
                    }
                }
            }
        }

        public MyAggregateComponentList ChildList =>
            this.m_children;

        MyComponentContainer IMyComponentAggregate.ContainerBase =>
            base.ContainerBase;
    }
}

