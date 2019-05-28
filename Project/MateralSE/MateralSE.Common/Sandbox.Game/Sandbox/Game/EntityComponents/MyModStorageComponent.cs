namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Serialization;
    using VRage.Utils;

    [MyComponentType(typeof(MyModStorageComponent)), MyComponentBuilder(typeof(MyObjectBuilder_ModStorageComponent), true)]
    public class MyModStorageComponent : MyModStorageComponentBase
    {
        private HashSet<Guid> m_cachedGuids = new HashSet<Guid>();

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            SerializableDictionary<Guid, string> storage = ((MyObjectBuilder_ModStorageComponent) builder).Storage;
            if ((storage != null) && (storage.Dictionary != null))
            {
                base.m_storageData = new Dictionary<Guid, string>(storage.Dictionary);
            }
        }

        public override string GetValue(Guid guid) => 
            base.m_storageData[guid];

        public override bool IsSerialized() => 
            (base.m_storageData.Count > 0);

        public override bool RemoveValue(Guid guid) => 
            base.m_storageData.Remove(guid);

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_ModStorageComponent component = (MyObjectBuilder_ModStorageComponent) base.Serialize(copy);
            component.Storage = new SerializableDictionary<Guid, string>();
            foreach (MyModStorageComponentDefinition definition in MyDefinitionManager.Static.GetEntityComponentDefinitions<MyModStorageComponentDefinition>())
            {
                foreach (Guid guid in definition.RegisteredStorageGuids)
                {
                    if (!this.m_cachedGuids.Add(guid))
                    {
                        object[] args = new object[] { guid.ToString(), definition.Context.ModId, definition.Id.ToString() };
                        MyLog.Default.Log(MyLogSeverity.Warning, "Duplicate ModStorageComponent GUID: {0}, in {1}: {2}", args);
                    }
                }
            }
            foreach (Guid guid2 in this.Storage.Keys)
            {
                if (this.m_cachedGuids.Contains(guid2))
                {
                    component.Storage[guid2] = this.Storage[guid2];
                    continue;
                }
                object[] args = new object[] { guid2.ToString() };
                MyLog.Default.Log(MyLogSeverity.Warning, "Not saving ModStorageComponent GUID: {0}, not claimed", args);
            }
            this.m_cachedGuids.Clear();
            return ((component.Storage.Dictionary.Count != 0) ? component : null);
        }

        public override void SetValue(Guid guid, string value)
        {
            base.m_storageData[guid] = value;
        }

        public override bool TryGetValue(Guid guid, out string value)
        {
            if (base.m_storageData.ContainsKey(guid))
            {
                value = base.m_storageData[guid];
                return true;
            }
            value = null;
            return false;
        }

        public IReadOnlyDictionary<Guid, string> Storage =>
            ((IReadOnlyDictionary<Guid, string>) base.m_storageData);
    }
}

