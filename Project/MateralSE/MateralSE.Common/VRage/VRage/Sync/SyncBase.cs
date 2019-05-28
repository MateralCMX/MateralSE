namespace VRage.Sync
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Library.Collections;
    using VRage.Serialization;

    public abstract class SyncBase : IBitSerializable
    {
        public readonly int Id;
        public readonly Type ValueType;
        public readonly MySerializeInfo SerializeInfo;
        public string DebugName;
        [CompilerGenerated]
        private Action<SyncBase> ValueChanged;
        [CompilerGenerated]
        private Action<SyncBase> ValueChangedNotify;

        public event Action<SyncBase> ValueChanged
        {
            [CompilerGenerated] add
            {
                Action<SyncBase> valueChanged = this.ValueChanged;
                while (true)
                {
                    Action<SyncBase> a = valueChanged;
                    Action<SyncBase> action3 = (Action<SyncBase>) Delegate.Combine(a, value);
                    valueChanged = Interlocked.CompareExchange<Action<SyncBase>>(ref this.ValueChanged, action3, a);
                    if (ReferenceEquals(valueChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<SyncBase> valueChanged = this.ValueChanged;
                while (true)
                {
                    Action<SyncBase> source = valueChanged;
                    Action<SyncBase> action3 = (Action<SyncBase>) Delegate.Remove(source, value);
                    valueChanged = Interlocked.CompareExchange<Action<SyncBase>>(ref this.ValueChanged, action3, source);
                    if (ReferenceEquals(valueChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<SyncBase> ValueChangedNotify
        {
            [CompilerGenerated] add
            {
                Action<SyncBase> valueChangedNotify = this.ValueChangedNotify;
                while (true)
                {
                    Action<SyncBase> a = valueChangedNotify;
                    Action<SyncBase> action3 = (Action<SyncBase>) Delegate.Combine(a, value);
                    valueChangedNotify = Interlocked.CompareExchange<Action<SyncBase>>(ref this.ValueChangedNotify, action3, a);
                    if (ReferenceEquals(valueChangedNotify, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<SyncBase> valueChangedNotify = this.ValueChangedNotify;
                while (true)
                {
                    Action<SyncBase> source = valueChangedNotify;
                    Action<SyncBase> action3 = (Action<SyncBase>) Delegate.Remove(source, value);
                    valueChangedNotify = Interlocked.CompareExchange<Action<SyncBase>>(ref this.ValueChangedNotify, action3, source);
                    if (ReferenceEquals(valueChangedNotify, source))
                    {
                        return;
                    }
                }
            }
        }

        public SyncBase(Type valueType, int id, MySerializeInfo serializeInfo)
        {
            this.ValueType = valueType;
            this.Id = id;
            this.SerializeInfo = serializeInfo;
        }

        public abstract SyncBase Clone(int newId);
        protected static void CopyValueChanged(SyncBase from, SyncBase to)
        {
            to.ValueChanged = from.ValueChanged;
            to.ValueChangedNotify = from.ValueChangedNotify;
        }

        public static implicit operator BitReaderWriter(SyncBase sync) => 
            new BitReaderWriter(sync);

        protected void RaiseValueChanged(bool notify)
        {
            Action<SyncBase> valueChanged = this.ValueChanged;
            if (valueChanged != null)
            {
                valueChanged(this);
            }
            if (notify)
            {
                valueChanged = this.ValueChangedNotify;
                if (valueChanged != null)
                {
                    valueChanged(this);
                }
            }
        }

        public abstract bool Serialize(BitStream stream, bool validate, bool setValueIfValid = true);
        public void SetDebugName(string debugName)
        {
            this.DebugName = debugName;
        }
    }
}

