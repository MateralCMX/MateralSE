namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Replication.StateGroups;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRageMath;

    internal class MyFarBroadcasterReplicable : MyExternalReplicableEvent<MyDataBroadcaster>
    {
        private MyEntityPositionStateGroup m_positionStateGroup;
        private MyProxyAntenna m_proxyAntenna;

        public override BoundingBoxD GetAABB()
        {
            MyDataBroadcaster instance = base.Instance;
            if ((base.Instance == null) || (base.Instance.Entity == null))
            {
                return BoundingBoxD.CreateInvalid();
            }
            return base.Instance.Entity.WorldAABB;
        }

        public override IMyReplicable GetParent() => 
            null;

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            resultList.Add(this.m_positionStateGroup);
        }

        public override void OnDestroyClient()
        {
            if (this.m_proxyAntenna != null)
            {
                this.m_proxyAntenna.Close();
            }
            this.m_proxyAntenna = null;
        }

        protected override void OnHook()
        {
            base.OnHook();
            this.m_positionStateGroup = new MyEntityPositionStateGroup(this, base.Instance.Entity);
            base.Instance.BeforeRemovedFromContainer += component => this.OnRemovedFromContainer();
        }

        protected override void OnLoad(BitStream stream, Action<MyDataBroadcaster> loadingDoneHandler)
        {
            MyObjectBuilder_ProxyAntenna antenna;
            MySerializer.CreateAndRead<MyObjectBuilder_ProxyAntenna>(stream, out antenna, MyObjectBuilderSerializer.Dynamic);
            this.m_proxyAntenna = MyEntities.CreateFromObjectBuilderAndAdd(antenna, false) as MyProxyAntenna;
            loadingDoneHandler(this.m_proxyAntenna.Broadcaster);
        }

        private void OnRemovedFromContainer()
        {
            this.RaiseDestroyed();
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            MyObjectBuilder_ProxyAntenna ob = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ProxyAntenna>();
            base.Instance.InitProxyObjectBuilder(ob);
            MySerializer.Write<MyObjectBuilder_ProxyAntenna>(stream, ref ob, MyObjectBuilderSerializer.Dynamic);
            return true;
        }

        public override bool IsValid =>
            ((base.Instance != null) && ((base.Instance.Entity != null) && !base.Instance.Entity.MarkedForClose));

        public override bool PriorityUpdate =>
            false;

        public override bool HasToBeChild =>
            true;
    }
}

