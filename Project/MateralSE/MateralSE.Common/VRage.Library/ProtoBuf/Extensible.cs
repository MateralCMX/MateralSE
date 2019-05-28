namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public abstract class Extensible : IExtensible
    {
        private IExtension extensionObject;

        protected Extensible()
        {
        }

        public static void AppendValue<TValue>(IExtensible instance, int tag, TValue value)
        {
            AppendValue<TValue>(instance, tag, DataFormat.Default, value);
        }

        public static void AppendValue<TValue>(IExtensible instance, int tag, DataFormat format, TValue value)
        {
            ExtensibleUtil.AppendExtendValue(RuntimeTypeModel.Default, instance, tag, format, value);
        }

        public static void AppendValue(TypeModel model, IExtensible instance, int tag, DataFormat format, object value)
        {
            ExtensibleUtil.AppendExtendValue(model, instance, tag, format, value);
        }

        protected virtual IExtension GetExtensionObject(bool createIfMissing) => 
            GetExtensionObject(ref this.extensionObject, createIfMissing);

        public static IExtension GetExtensionObject(ref IExtension extensionObject, bool createIfMissing)
        {
            if (createIfMissing && (extensionObject == null))
            {
                extensionObject = new BufferExtension();
            }
            return extensionObject;
        }

        public static TValue GetValue<TValue>(IExtensible instance, int tag) => 
            GetValue<TValue>(instance, tag, DataFormat.Default);

        public static TValue GetValue<TValue>(IExtensible instance, int tag, DataFormat format)
        {
            TValue local;
            TryGetValue<TValue>(instance, tag, format, out local);
            return local;
        }

        public static IEnumerable<TValue> GetValues<TValue>(IExtensible instance, int tag) => 
            ExtensibleUtil.GetExtendedValues<TValue>(instance, tag, DataFormat.Default, false, false);

        public static IEnumerable<TValue> GetValues<TValue>(IExtensible instance, int tag, DataFormat format) => 
            ExtensibleUtil.GetExtendedValues<TValue>(instance, tag, format, false, false);

        public static IEnumerable GetValues(TypeModel model, Type type, IExtensible instance, int tag, DataFormat format) => 
            ExtensibleUtil.GetExtendedValues(model, type, instance, tag, format, false, false);

        IExtension IExtensible.GetExtensionObject(bool createIfMissing) => 
            this.GetExtensionObject(createIfMissing);

        public static bool TryGetValue<TValue>(IExtensible instance, int tag, out TValue value) => 
            TryGetValue<TValue>(instance, tag, DataFormat.Default, out value);

        public static bool TryGetValue<TValue>(IExtensible instance, int tag, DataFormat format, out TValue value) => 
            TryGetValue<TValue>(instance, tag, format, false, out value);

        public static bool TryGetValue<TValue>(IExtensible instance, int tag, DataFormat format, bool allowDefinedTag, out TValue value)
        {
            value = default(TValue);
            bool flag = false;
            foreach (TValue local in ExtensibleUtil.GetExtendedValues<TValue>(instance, tag, format, true, allowDefinedTag))
            {
                value = local;
                flag = true;
            }
            return flag;
        }

        public static bool TryGetValue(TypeModel model, Type type, IExtensible instance, int tag, DataFormat format, bool allowDefinedTag, out object value)
        {
            value = null;
            bool flag = false;
            foreach (object obj2 in ExtensibleUtil.GetExtendedValues(model, type, instance, tag, format, true, allowDefinedTag))
            {
                value = obj2;
                flag = true;
            }
            return flag;
        }
    }
}

