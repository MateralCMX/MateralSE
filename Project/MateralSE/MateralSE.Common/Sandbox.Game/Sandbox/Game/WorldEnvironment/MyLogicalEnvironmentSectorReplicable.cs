namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.Serialization;
    using VRageMath;

    internal class MyLogicalEnvironmentSectorReplicable : MyExternalReplicableEvent<MyLogicalEnvironmentSectorBase>
    {
        private static readonly MySerializeInfo serialInfo = new MySerializeInfo(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, MyPrimitiveFlags.None, 0, new DynamicSerializerDelegate(MyObjectBuilderSerializer.SerializeDynamic), null, null);
        private long m_planetEntityId;
        private long m_packedSectorId;
        private MyObjectBuilder_EnvironmentSector m_ob;

        public override BoundingBoxD GetAABB()
        {
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            foreach (Vector3D vectord in base.Instance.Bounds)
            {
                xd = xd.Include(vectord);
            }
            return xd;
        }

        public override IMyReplicable GetParent() => 
            base.m_parent;

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
        }

        public override void OnDestroyClient()
        {
            if (base.Instance != null)
            {
                base.Instance.ServerOwned = false;
            }
        }

        protected override void OnHook()
        {
            base.OnHook();
            if (Sync.IsServer)
            {
                base.Instance.OnClose += new Action(this.Sector_OnClose);
            }
            else
            {
                base.Instance.ServerOwned = true;
            }
            base.m_parent = FindByObject(base.Instance.Owner.Entity);
        }

        protected override void OnLoad(BitStream stream, Action<MyLogicalEnvironmentSectorBase> loadingDoneHandler)
        {
            if (stream != null)
            {
                this.m_planetEntityId = stream.ReadInt64(0x40);
                this.m_packedSectorId = stream.ReadInt64(0x40);
                this.m_ob = MySerializer.CreateAndRead<MyObjectBuilder_EnvironmentSector>(stream, serialInfo);
            }
            MyPlanet entityById = MyEntities.GetEntityById(this.m_planetEntityId, false) as MyPlanet;
            if (entityById == null)
            {
                loadingDoneHandler(null);
            }
            else
            {
                object obj1;
                MyLogicalEnvironmentSectorBase logicalSector = entityById.Components.Get<MyPlanetEnvironmentComponent>().GetLogicalSector(this.m_packedSectorId);
                bool flag = FindByObject(entityById) != null;
                if ((logicalSector != null) & flag)
                {
                    logicalSector.Init(this.m_ob);
                }
                if (((logicalSector != null) && logicalSector.ServerOwned) || !flag)
                {
                    obj1 = null;
                }
                else
                {
                    obj1 = logicalSector;
                }
                loadingDoneHandler((MyLogicalEnvironmentSectorBase) obj1);
            }
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            stream.WriteInt64(base.Instance.Owner.Entity.EntityId, 0x40);
            stream.WriteInt64(base.Instance.Id, 0x40);
            MyObjectBuilder_EnvironmentSector objectBuilder = base.Instance.GetObjectBuilder();
            MySerializer.Write<MyObjectBuilder_EnvironmentSector>(stream, ref objectBuilder, serialInfo);
            return true;
        }

        private void Sector_OnClose()
        {
            base.Instance.OnClose -= new Action(this.Sector_OnClose);
            base.Instance.ServerOwned = false;
            this.RaiseDestroyed();
        }

        public override bool ShouldReplicate(MyClientInfo client)
        {
            HashSet<long> set;
            MyClientState state = client.State as MyClientState;
            if (base.Instance.Owner.Entity == null)
            {
                return false;
            }
            long entityId = base.Instance.Owner.Entity.EntityId;
            return (state.KnownSectors.TryGetValue(entityId, out set) && set.Contains(base.Instance.Id));
        }

        public override bool IncludeInIslands =>
            false;

        public override bool IsValid =>
            ((base.m_parent != null) && base.m_parent.IsValid);

        public override bool HasToBeChild =>
            false;

        public override bool IsSpatial =>
            true;
    }
}

