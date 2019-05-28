namespace VRage.Game.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Serialization;

    public abstract class MyModStorageComponentBase : MyEntityComponentBase, IDictionary<Guid, string>, ICollection<KeyValuePair<Guid, string>>, IEnumerable<KeyValuePair<Guid, string>>, IEnumerable
    {
        protected IDictionary<Guid, string> m_storageData = new Dictionary<Guid, string>();

        protected MyModStorageComponentBase()
        {
        }

        public void Add(KeyValuePair<Guid, string> item)
        {
            this.m_storageData.Add(item);
        }

        public void Add(Guid key, string value)
        {
            this.SetValue(key, value);
        }

        public void Clear()
        {
            this.m_storageData.Clear();
        }

        public bool Contains(KeyValuePair<Guid, string> item) => 
            this.m_storageData.Contains(item);

        public bool ContainsKey(Guid key) => 
            this.m_storageData.ContainsKey(key);

        public void CopyTo(KeyValuePair<Guid, string>[] array, int arrayIndex)
        {
            this.m_storageData.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<Guid, string>> GetEnumerator() => 
            this.m_storageData.GetEnumerator();

        public abstract string GetValue(Guid guid);
        public bool Remove(KeyValuePair<Guid, string> item) => 
            this.m_storageData.Remove(item);

        public bool Remove(Guid key) => 
            this.RemoveValue(key);

        public abstract bool RemoveValue(Guid guid);
        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_ModStorageComponent component1 = (MyObjectBuilder_ModStorageComponent) base.Serialize(copy);
            component1.Storage = new SerializableDictionary<Guid, string>((Dictionary<Guid, string>) this.m_storageData);
            return component1;
        }

        public abstract void SetValue(Guid guid, string value);
        IEnumerator IEnumerable.GetEnumerator() => 
            this.m_storageData.GetEnumerator();

        public abstract bool TryGetValue(Guid guid, out string value);

        public override string ComponentTypeDebugString =>
            "Mod Storage";

        public string this[Guid key]
        {
            get => 
                this.GetValue(key);
            set => 
                this.SetValue(key, value);
        }

        public int Count =>
            this.m_storageData.Count;

        public bool IsReadOnly =>
            this.m_storageData.IsReadOnly;

        public ICollection<Guid> Keys =>
            this.m_storageData.Keys;

        public ICollection<string> Values =>
            this.m_storageData.Values;
    }
}

