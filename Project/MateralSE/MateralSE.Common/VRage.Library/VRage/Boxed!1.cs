namespace VRage
{
    using System;

    public sealed class Boxed<T> where T: struct
    {
        public T BoxedValue;

        public Boxed(T value)
        {
            this.BoxedValue = value;
        }

        public override int GetHashCode() => 
            this.BoxedValue.GetHashCode();

        public static explicit operator VRage.Boxed<T>(T value) => 
            new VRage.Boxed<T>(value);

        public static implicit operator T(VRage.Boxed<T> box) => 
            box.BoxedValue;

        public override string ToString() => 
            this.BoxedValue.ToString();
    }
}

