namespace Sandbox.Game.Replication
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Diagnostics;
    using VRage.Game.Entity;
    using VRage.Network;

    public abstract class MyEntityReplicableBaseEvent<T> : MyEntityReplicableBase<T>, IMyProxyTarget, IMyNetObject, IMyEventOwner where T: MyEntity, IMyEventProxy
    {
        private IMyEventProxy m_proxy;

        protected MyEntityReplicableBaseEvent()
        {
        }

        protected override void OnHook()
        {
            base.OnHook();
            this.m_proxy = base.Instance;
        }

        private void OnMarkForCloseOnClient(MyEntity entity)
        {
            if (MyMultiplayer.Static != null)
            {
                IMyProxyTarget proxyTarget = MyMultiplayer.Static.ReplicationLayer.GetProxyTarget(this.m_proxy);
                if (MySession.Static.Ready && (proxyTarget != null))
                {
                    NetworkId id;
                    MyMultiplayer.Static.ReplicationLayer.TryGetNetworkIdByObject(proxyTarget, out id);
                }
            }
        }

        [Conditional("DEBUG")]
        private void RegisterAsserts()
        {
            if (!Sync.IsServer)
            {
                base.Instance.OnMarkForClose += new Action<MyEntity>(this.OnMarkForCloseOnClient);
                base.Instance.OnClose += new Action<MyEntity>(this.OnMarkForCloseOnClient);
            }
        }

        IMyEventProxy IMyProxyTarget.Target =>
            this.m_proxy;
    }
}

