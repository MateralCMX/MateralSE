namespace VRage.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;

    public class SyncType
    {
        private List<SyncBase> m_properties;
        private Action<SyncBase> m_registeredHandlers;
        [CompilerGenerated]
        private Action PropertyCountChanged;

        public event Action<SyncBase> PropertyChanged
        {
            add
            {
                this.m_registeredHandlers = (Action<SyncBase>) Delegate.Combine(this.m_registeredHandlers, value);
                using (List<SyncBase>.Enumerator enumerator = this.m_properties.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ValueChanged += value;
                    }
                }
            }
            remove
            {
                using (List<SyncBase>.Enumerator enumerator = this.m_properties.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ValueChanged -= value;
                    }
                }
                this.m_registeredHandlers = (Action<SyncBase>) Delegate.Remove(this.m_registeredHandlers, value);
            }
        }

        public event Action<SyncBase> PropertyChangedNotify
        {
            add
            {
                using (List<SyncBase>.Enumerator enumerator = this.m_properties.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ValueChangedNotify += value;
                    }
                }
            }
            remove
            {
                using (List<SyncBase>.Enumerator enumerator = this.m_properties.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ValueChangedNotify -= value;
                    }
                }
            }
        }

        public event Action PropertyCountChanged
        {
            [CompilerGenerated] add
            {
                Action propertyCountChanged = this.PropertyCountChanged;
                while (true)
                {
                    Action a = propertyCountChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    propertyCountChanged = Interlocked.CompareExchange<Action>(ref this.PropertyCountChanged, action3, a);
                    if (ReferenceEquals(propertyCountChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action propertyCountChanged = this.PropertyCountChanged;
                while (true)
                {
                    Action source = propertyCountChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    propertyCountChanged = Interlocked.CompareExchange<Action>(ref this.PropertyCountChanged, action3, source);
                    if (ReferenceEquals(propertyCountChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public SyncType(List<SyncBase> properties)
        {
            this.m_properties = properties;
        }

        public void Append(object obj)
        {
            SyncHelpers.Compose(obj, this.m_properties.Count, this.m_properties);
            for (int i = this.m_properties.Count; i < this.m_properties.Count; i++)
            {
                this.m_properties[i].ValueChanged += this.m_registeredHandlers;
            }
            this.PropertyCountChanged.InvokeIfNotNull();
        }

        public ListReader<SyncBase> Properties =>
            new ListReader<SyncBase>(this.m_properties);
    }
}

