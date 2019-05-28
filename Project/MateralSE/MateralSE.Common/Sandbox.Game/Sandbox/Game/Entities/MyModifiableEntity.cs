namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;

    [MyEntityType(typeof(MyObjectBuilder_ModifiableEntity), true)]
    public class MyModifiableEntity : MyEntity, IMyEventProxy, IMyEventOwner
    {
        private List<MyDefinitionId> m_assetModifiers;
        private bool m_assetModifiersDirty;

        public void AddAssetModifier(MyDefinitionId id)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyModifiableEntity, SerializableDefinitionId>(this, x => new Action<SerializableDefinitionId>(x.AddAssetModifierSync), (SerializableDefinitionId) id, targetEndpoint);
        }

        [Event(null, 0x41), Reliable, Broadcast, Server]
        private void AddAssetModifierSync(SerializableDefinitionId id)
        {
            if (this.m_assetModifiers == null)
            {
                this.m_assetModifiers = new List<MyDefinitionId>();
            }
            this.m_assetModifiers.Add(id);
            MySession.Static.GetComponent<MySessionComponentAssetModifiers>();
        }

        public List<MyDefinitionId> GetAssetModifiers() => 
            this.m_assetModifiers;

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_ModifiableEntity objectBuilder = (MyObjectBuilder_ModifiableEntity) base.GetObjectBuilder(copy);
            if ((this.m_assetModifiers != null) && (this.m_assetModifiers.Count > 0))
            {
                objectBuilder.AssetModifiers = new List<SerializableDefinitionId>();
                foreach (MyDefinitionId id in this.m_assetModifiers)
                {
                    objectBuilder.AssetModifiers.Add((SerializableDefinitionId) id);
                }
            }
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_ModifiableEntity entity = objectBuilder as MyObjectBuilder_ModifiableEntity;
            if (entity != null)
            {
                this.m_assetModifiersDirty = false;
                if ((entity.AssetModifiers != null) && (entity.AssetModifiers.Count > 0))
                {
                    this.m_assetModifiersDirty = true;
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                    this.m_assetModifiers = new List<MyDefinitionId>();
                    foreach (SerializableDefinitionId id in entity.AssetModifiers)
                    {
                        this.m_assetModifiers.Add(id);
                    }
                }
            }
        }

        public void RemoveAssetModifier(MyDefinitionId id)
        {
            if (this.m_assetModifiers != null)
            {
                this.m_assetModifiers.Remove(id);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (this.m_assetModifiersDirty)
            {
                MySession.Static.GetComponent<MySessionComponentAssetModifiers>();
                foreach (MyDefinitionId local1 in this.m_assetModifiers)
                {
                }
                this.m_assetModifiersDirty = false;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyModifiableEntity.<>c <>9 = new MyModifiableEntity.<>c();
            public static Func<MyModifiableEntity, Action<SerializableDefinitionId>> <>9__4_0;

            internal Action<SerializableDefinitionId> <AddAssetModifier>b__4_0(MyModifiableEntity x) => 
                new Action<SerializableDefinitionId>(x.AddAssetModifierSync);
        }
    }
}

