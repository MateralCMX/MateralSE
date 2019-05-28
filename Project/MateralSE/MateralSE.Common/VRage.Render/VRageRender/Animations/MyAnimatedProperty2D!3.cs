namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyAnimatedProperty2D<T, V, W> : MyAnimatedProperty<T>, IMyAnimatedProperty2D<T, V, W>, IMyAnimatedProperty2D, IMyAnimatedProperty, IMyConstProperty where T: MyAnimatedProperty<V>, new()
    {
        protected MyAnimatedProperty<V>.InterpolatorDelegate m_interpolator2;

        public MyAnimatedProperty2D()
        {
        }

        public MyAnimatedProperty2D(string name, MyAnimatedProperty<V>.InterpolatorDelegate interpolator) : base(name, false, null)
        {
            this.m_interpolator2 = interpolator;
        }

        public virtual void ApplyVariance(ref V interpolatedValue, ref W variance, float multiplier, out V value)
        {
            value = default(V);
        }

        public IMyAnimatedProperty CreateEmptyKeys() => 
            Activator.CreateInstance<T>();

        public override void DeserializeFromObjectBuilder(GenerationProperty property)
        {
            base.m_name = property.Name;
            base.m_keys.Clear();
            foreach (AnimationKey key in property.Keys)
            {
                T val = Activator.CreateInstance<T>();
                val.DeserializeFromObjectBuilder_Animation(key.Value2D, property.Type);
                base.AddKey<T>(key.Time, val);
            }
        }

        public override IMyConstProperty Duplicate() => 
            null;

        protected override void Duplicate(IMyConstProperty targetProp)
        {
            MyAnimatedProperty2D<T, V, W> propertyd = targetProp as MyAnimatedProperty2D<T, V, W>;
            propertyd.Interpolator = base.Interpolator;
            propertyd.m_interpolator2 = this.m_interpolator2;
            propertyd.ClearKeys();
            foreach (MyAnimatedProperty<T>.ValueHolder holder in base.m_keys)
            {
                propertyd.AddKey(holder.Duplicate());
            }
        }

        public void GetInterpolatedKeys(float overallTime, float multiplier, IMyAnimatedProperty interpolatedKeys)
        {
            W variance = default(W);
            this.GetInterpolatedKeys(overallTime, variance, multiplier, interpolatedKeys);
        }

        public unsafe void GetInterpolatedKeys(float overallTime, W variance, float multiplier, IMyAnimatedProperty interpolatedKeysOb)
        {
            T local;
            T local2;
            float num;
            float num2;
            float num3;
            base.GetPreviousValue(overallTime, out local, out num);
            base.GetNextValue(overallTime, out local2, out num2, out num3);
            T local3 = interpolatedKeysOb as T;
            local3.ClearKeys();
            if (local != null)
            {
                if (this.m_interpolator2 != null)
                {
                    local3.Interpolator = this.m_interpolator2;
                }
                for (int i = 0; i < local.GetKeysCount(); i++)
                {
                    float num5;
                    V local4;
                    V local5;
                    V local6;
                    local.GetKey(i, out num5, out local4);
                    local.GetInterpolatedValue<V>(num5, out local5);
                    local2.GetInterpolatedValue<V>(num5, out local6);
                    V local7 = local5;
                    if (num2 != num)
                    {
                        local3.Interpolator(ref local5, ref local6, (overallTime - num) * num3, out local7);
                    }
                    V* interpolatedValue = ref local7;
                    this.ApplyVariance(ref interpolatedValue, ref variance, multiplier, out local7);
                    local3.AddKey<V>(num5, local7);
                }
            }
        }

        public X GetInterpolatedValue<X>(float overallTime, float time) where X: V
        {
            T local;
            T local2;
            float num;
            float num2;
            float num3;
            V local3;
            V local4;
            V local5;
            base.GetPreviousValue(overallTime, out local, out num);
            base.GetNextValue(overallTime, out local2, out num2, out num3);
            local.GetInterpolatedValue<V>(time, out local3);
            local2.GetInterpolatedValue<V>(time, out local4);
            local.Interpolator(ref local3, ref local4, (overallTime - num) * num3, out local5);
            return (X) local5;
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            (value as IMyAnimatedProperty).Serialize(writer);
        }

        public override bool Is2D =>
            true;
    }
}

