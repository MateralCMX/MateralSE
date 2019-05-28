namespace VRage.Network
{
    using System;
    using System.Reflection;
    using VRage.Library.Collections;

    internal class CallSite<T1, T2, T3, T4, T5, T6, T7> : CallSite
    {
        public readonly Action<T1, T2, T3, T4, T5, T6, T7> Handler;
        public readonly SerializeDelegate<T1, T2, T3, T4, T5, T6, T7> Serializer;
        public readonly Func<T1, T2, T3, T4, T5, T6, T7, bool> Validator;

        public CallSite(MySynchronizedTypeInfo owner, uint id, MethodInfo info, CallSiteFlags flags, Action<T1, T2, T3, T4, T5, T6, T7> handler, SerializeDelegate<T1, T2, T3, T4, T5, T6, T7> serializer, Func<T1, T2, T3, T4, T5, T6, T7, bool> validator, ValidationType validationFlags) : base(owner, id, info, flags, validationFlags)
        {
            this.Handler = handler;
            this.Serializer = serializer;
            this.Validator = validator;
        }

        public override bool Invoke(BitStream stream, object obj, bool validate)
        {
            T1 inst = (T1) obj;
            T2 local2 = default(T2);
            T3 local3 = default(T3);
            T4 local4 = default(T4);
            T5 local5 = default(T5);
            T6 local6 = default(T6);
            T7 local7 = default(T7);
            this.Serializer(inst, stream, ref local2, ref local3, ref local4, ref local5, ref local6, ref local7);
            if (validate && !this.Validator(inst, local2, local3, local4, local5, local6, local7))
            {
                return false;
            }
            this.Handler(inst, local2, local3, local4, local5, local6, local7);
            return true;
        }
    }
}

