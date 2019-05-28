namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageRender;

    public class MyAnimatedProperty<T> : IMyAnimatedProperty<T>, IMyAnimatedProperty, IMyConstProperty
    {
        protected List<ValueHolder<T>> m_keys;
        public InterpolatorDelegate<T> Interpolator;
        protected string m_name;
        private bool m_interpolateAfterEnd;
        private static MyKeysComparer<T> m_keysComparer;
        private static int m_globalKeyCounter;

        static MyAnimatedProperty()
        {
            MyAnimatedProperty<T>.m_keysComparer = new MyKeysComparer<T>();
            MyAnimatedProperty<T>.m_globalKeyCounter = 0;
        }

        public MyAnimatedProperty()
        {
            this.m_keys = new List<ValueHolder<T>>();
            this.Init();
        }

        public MyAnimatedProperty(string name, bool interpolateAfterEnd, InterpolatorDelegate<T> interpolator) : this()
        {
            this.m_name = name;
            this.m_interpolateAfterEnd = interpolateAfterEnd;
            if (interpolator != null)
            {
                this.Interpolator = interpolator;
            }
        }

        public void AddKey(ValueHolder<T> val)
        {
            this.m_keys.Add(val);
        }

        public int AddKey<U>(float time, U val) where U: T
        {
            MyAnimatedProperty<T>.m_globalKeyCounter++;
            ValueHolder<T> item = new ValueHolder<T>(MyAnimatedProperty<T>.m_globalKeyCounter, time, val, 0f);
            this.m_keys.Add(item);
            this.m_keys.Sort(MyAnimatedProperty<T>.m_keysComparer);
            int index = 0;
            index = 0;
            while ((index < this.m_keys.Count) && (this.m_keys[index].Time != time))
            {
                index++;
            }
            if (index > 0)
            {
                this.UpdateDiff(index);
            }
            return item.ID;
        }

        public void ClearKeys()
        {
            this.m_keys.Clear();
        }

        public virtual void Deserialize(XmlReader reader)
        {
            this.m_name = reader.GetAttribute("name");
            reader.ReadStartElement();
            this.m_keys.Clear();
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                object obj2;
                reader.ReadStartElement();
                float time = reader.ReadElementContentAsFloat();
                reader.ReadStartElement();
                this.DeserializeValue(reader, out obj2);
                reader.ReadEndElement();
                this.AddKey<T>(time, (T) obj2);
                reader.ReadEndElement();
            }
            if (!isEmptyElement)
            {
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        public virtual void DeserializeFromObjectBuilder(GenerationProperty property)
        {
            this.m_name = property.Name;
            this.DeserializeKeys(property.Keys, property.Type);
        }

        public void DeserializeFromObjectBuilder_Animation(Generation2DProperty property, string type)
        {
            this.DeserializeKeys(property.Keys, type);
        }

        public void DeserializeKeys(List<AnimationKey> keys, string type)
        {
            this.m_keys.Clear();
            using (List<AnimationKey>.Enumerator enumerator = keys.GetEnumerator())
            {
                while (true)
                {
                    AnimationKey current;
                    object valueInt;
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            uint num = <PrivateImplementationDetails>.ComputeStringHash(type);
                            if (num <= 0x528bdc96)
                            {
                                if (num <= 0x2f742c5d)
                                {
                                    if (num == 0x2c89323d)
                                    {
                                        if (type != "GenerationIndex")
                                        {
                                        }
                                    }
                                    else if ((num == 0x2f742c5d) && (type == "Bool"))
                                    {
                                        valueInt = current.ValueBool;
                                        break;
                                    }
                                }
                                else if (num != 0x4c816225)
                                {
                                    if ((num == 0x528bdc96) && (type == "MyTransparentMaterial"))
                                    {
                                        valueInt = MyTransparentMaterials.GetMaterial(MyStringId.GetOrCompute(current.ValueString));
                                        break;
                                    }
                                }
                                else if (type == "Float")
                                {
                                    valueInt = current.ValueFloat;
                                    break;
                                }
                            }
                            else if (num <= 0x840071c3)
                            {
                                if (num != 0x604f4858)
                                {
                                    if ((num == 0x840071c3) && (type == "Vector3"))
                                    {
                                        valueInt = current.ValueVector3;
                                        break;
                                    }
                                }
                                else if (type == "String")
                                {
                                    valueInt = current.ValueString;
                                    break;
                                }
                            }
                            else if (num == 0x890079a2)
                            {
                                if (type == "Vector4")
                                {
                                    valueInt = current.ValueVector4;
                                    break;
                                }
                            }
                            else if (num != 0xe84dda20)
                            {
                                if ((num == 0xf87415fe) && (type != "Int"))
                                {
                                }
                            }
                            else if (type != "Enum")
                            {
                            }
                            valueInt = current.ValueInt;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    }
                    this.AddKey<T>(current.Time, (T) valueInt);
                }
            }
        }

        public virtual void DeserializeValue(XmlReader reader, out object value)
        {
            value = reader.Value;
            reader.Read();
        }

        public virtual IMyConstProperty Duplicate() => 
            null;

        protected virtual void Duplicate(IMyConstProperty targetProp)
        {
            MyAnimatedProperty<T> property = targetProp as MyAnimatedProperty<T>;
            property.Interpolator = this.Interpolator;
            property.ClearKeys();
            foreach (ValueHolder<T> holder in this.m_keys)
            {
                property.AddKey(holder.Duplicate());
            }
        }

        protected virtual bool EqualsValues(object value1, object value2) => 
            false;

        public void GetInterpolatedValue<U>(float time, out U value) where U: T
        {
            if (this.m_keys.Count == 0)
            {
                value = default(U);
            }
            else if (this.m_keys.Count == 1)
            {
                value = this.m_keys[0].Value;
            }
            else if (time > this.m_keys[this.m_keys.Count - 1].Time)
            {
                if (!this.m_interpolateAfterEnd)
                {
                    value = this.m_keys[this.m_keys.Count - 1].Value;
                }
                else
                {
                    T local;
                    T local2;
                    float num;
                    float num2;
                    float num3;
                    this.GetPreviousValue(this.m_keys[this.m_keys.Count - 1].Time, out local, out num);
                    this.GetNextValue(time, out local2, out num2, out num3);
                    if (this.Interpolator == null)
                    {
                        value = default(U);
                    }
                    else
                    {
                        T local3;
                        this.Interpolator(ref local, ref local2, (time - num) * num3, out local3);
                        value = local3;
                    }
                }
            }
            else
            {
                T local4;
                T local5;
                float num4;
                float num5;
                float num6;
                this.GetPreviousValue(time, out local4, out num4);
                this.GetNextValue(time, out local5, out num5, out num6);
                if (num5 == num4)
                {
                    value = local4;
                }
                else if (this.Interpolator == null)
                {
                    value = default(U);
                }
                else
                {
                    T local6;
                    this.Interpolator(ref local4, ref local5, (time - num4) * num6, out local6);
                    value = local6;
                }
            }
        }

        public void GetKey(int index, out float time, out T value)
        {
            time = this.m_keys[index].Time;
            value = this.m_keys[index].Value;
        }

        public void GetKey(int index, out int id, out float time, out T value)
        {
            id = this.m_keys[index].ID;
            time = this.m_keys[index].Time;
            value = this.m_keys[index].Value;
        }

        public void GetKeyByID(int id, out float time, out T value)
        {
            ValueHolder<T> holder = this.m_keys.Find(x => x.ID == id);
            time = holder.Time;
            value = holder.Value;
        }

        public int GetKeysCount() => 
            this.m_keys.Count;

        public void GetNextValue(float time, out T nextValue, out float nextTime, out float difference)
        {
            nextValue = default(T);
            nextTime = -1f;
            difference = 0f;
            int num = 0;
            while (true)
            {
                if (num < this.m_keys.Count)
                {
                    nextTime = this.m_keys[num].Time;
                    nextValue = this.m_keys[num].Value;
                    difference = this.m_keys[num].PrecomputedDiff;
                    if (nextTime < time)
                    {
                        num++;
                        continue;
                    }
                }
                return;
            }
        }

        public void GetPreviousValue(float time, out T previousValue, out float previousTime)
        {
            previousValue = default(T);
            previousTime = 0f;
            if (this.m_keys.Count > 0)
            {
                previousTime = this.m_keys[0].Time;
                previousValue = this.m_keys[0].Value;
            }
            for (int i = 1; (i < this.m_keys.Count) && (this.m_keys[i].Time < time); i++)
            {
                previousTime = this.m_keys[i].Time;
                previousValue = this.m_keys[i].Value;
            }
        }

        public U GetValue<U>() => 
            default(U);

        protected virtual void Init()
        {
        }

        private void RemoveKey(int index)
        {
            this.m_keys.RemoveAt(index);
            this.UpdateDiff(index);
        }

        public void RemoveKey(float time)
        {
            for (int i = 0; i < this.m_keys.Count; i++)
            {
                if (this.m_keys[i].Time == time)
                {
                    this.RemoveKey(i);
                    return;
                }
            }
        }

        private void RemoveRedundantKeys()
        {
            int index = 0;
            bool flag = true;
            while (index < (this.m_keys.Count - 1))
            {
                object obj2 = this.m_keys[index].Value;
                object obj3 = this.m_keys[index + 1].Value;
                bool flag2 = this.EqualsValues(obj2, obj3);
                if (flag2 && !flag)
                {
                    this.RemoveKey(index);
                    continue;
                }
                flag = !flag2;
                index++;
            }
            if (this.m_keys.Count == 2)
            {
                object obj4 = this.m_keys[0].Value;
                if (this.EqualsValues(obj4, this.m_keys[1].Value))
                {
                    this.RemoveKey(index);
                }
            }
        }

        public virtual void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("Keys");
            foreach (ValueHolder<T> holder in this.m_keys)
            {
                writer.WriteStartElement("Key");
                writer.WriteElementString("Time", holder.Time.ToString(CultureInfo.InvariantCulture));
                if (this.Is2D)
                {
                    writer.WriteStartElement("Value2D");
                }
                else
                {
                    writer.WriteStartElement("Value" + this.ValueType);
                }
                this.SerializeValue(writer, holder.Value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public virtual void SerializeValue(XmlWriter writer, object value)
        {
        }

        public void SetValue(object val)
        {
        }

        public void SetValue(T val)
        {
        }

        private void UpdateDiff(int index)
        {
            if ((index >= 1) && (index < this.m_keys.Count))
            {
                float time = this.m_keys[index].Time;
                this.m_keys[index] = new ValueHolder<T>(this.m_keys[index].ID, time, this.m_keys[index].Value, 1f / (time - this.m_keys[index - 1].Time));
            }
        }

        int IMyAnimatedProperty.AddKey(float time, object val) => 
            this.AddKey<T>(time, (T) val);

        void IMyAnimatedProperty.GetInterpolatedValue(float time, out object value)
        {
            T local;
            this.GetInterpolatedValue<T>(time, out local);
            value = local;
        }

        void IMyAnimatedProperty.GetKey(int index, out float time, out object value)
        {
            T local;
            this.GetKey(index, out time, out local);
            value = local;
        }

        void IMyAnimatedProperty.GetKey(int index, out int id, out float time, out object value)
        {
            T local;
            this.GetKey(index, out id, out time, out local);
            value = local;
        }

        void IMyAnimatedProperty.GetKeyByID(int id, out float time, out object value)
        {
            T local;
            this.GetKeyByID(id, out time, out local);
            value = local;
        }

        void IMyAnimatedProperty.RemoveKey(int index)
        {
            this.RemoveKey(index);
        }

        void IMyAnimatedProperty.RemoveKeyByID(int id)
        {
            ValueHolder<T> item = this.m_keys.Find(x => x.ID == id);
            int index = this.m_keys.IndexOf(item);
            this.RemoveKey(index);
        }

        void IMyAnimatedProperty.SetKey(int index, float time)
        {
            ValueHolder<T> holder = this.m_keys[index];
            holder.Time = time;
            this.m_keys[index] = holder;
            this.UpdateDiff(index - 1);
            this.UpdateDiff(index);
            this.UpdateDiff(index + 1);
            this.m_keys.Sort(MyAnimatedProperty<T>.m_keysComparer);
        }

        void IMyAnimatedProperty.SetKey(int index, float time, object value)
        {
            ValueHolder<T> holder = this.m_keys[index];
            holder.Time = time;
            holder.Value = (T) value;
            this.m_keys[index] = holder;
            this.UpdateDiff(index - 1);
            this.UpdateDiff(index);
            this.UpdateDiff(index + 1);
            this.m_keys.Sort(MyAnimatedProperty<T>.m_keysComparer);
        }

        void IMyAnimatedProperty.SetKeyByID(int id, float time)
        {
            ValueHolder<T> item = this.m_keys.Find(x => x.ID == id);
            int index = this.m_keys.IndexOf(item);
            item.Time = time;
            this.m_keys[index] = item;
            this.UpdateDiff(index - 1);
            this.UpdateDiff(index);
            this.UpdateDiff(index + 1);
            this.m_keys.Sort(MyAnimatedProperty<T>.m_keysComparer);
        }

        void IMyAnimatedProperty.SetKeyByID(int id, float time, object value)
        {
            int index = -1;
            ValueHolder<T> item = new ValueHolder<T>();
            int num2 = 0;
            while (true)
            {
                if (num2 < this.m_keys.Count)
                {
                    if (this.m_keys[num2].ID != id)
                    {
                        num2++;
                        continue;
                    }
                    item = this.m_keys[num2];
                    index = num2;
                }
                item.Time = time;
                item.Value = (T) value;
                if (index != -1)
                {
                    this.m_keys[index] = item;
                }
                else
                {
                    item.ID = id;
                    index = this.m_keys.Count;
                    this.m_keys.Add(item);
                }
                this.UpdateDiff(index - 1);
                this.UpdateDiff(index);
                this.UpdateDiff(index + 1);
                this.m_keys.Sort(MyAnimatedProperty<T>.m_keysComparer);
                return;
            }
        }

        object IMyConstProperty.GetValue() => 
            null;

        Type IMyConstProperty.GetValueType() => 
            typeof(T);

        public string Name
        {
            get => 
                this.m_name;
            set => 
                (this.m_name = value);
        }

        public virtual string ValueType =>
            typeof(T).Name;

        public virtual string BaseValueType =>
            this.ValueType;

        public virtual bool Animated =>
            true;

        public virtual bool Is2D =>
            false;

        public delegate void InterpolatorDelegate(ref T previousValue, ref T nextValue, float time, out T value);

        private class MyKeysComparer : IComparer<MyAnimatedProperty<T>.ValueHolder>
        {
            public int Compare(MyAnimatedProperty<T>.ValueHolder x, MyAnimatedProperty<T>.ValueHolder y) => 
                x.Time.CompareTo(y.Time);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ValueHolder
        {
            public T Value;
            public float PrecomputedDiff;
            public float Time;
            public int ID;
            public ValueHolder(int id, float time, T value, float diff)
            {
                this.ID = id;
                this.Time = time;
                this.Value = value;
                this.PrecomputedDiff = diff;
            }

            public MyAnimatedProperty<T>.ValueHolder Duplicate() => 
                new MyAnimatedProperty<T>.ValueHolder { 
                    Time = this.Time,
                    PrecomputedDiff = this.PrecomputedDiff,
                    ID = this.ID,
                    Value = !(this.Value is IMyConstProperty) ? this.Value : ((T) ((IMyConstProperty) this.Value).Duplicate())
                };
        }
    }
}

