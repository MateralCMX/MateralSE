namespace Sandbox.Game.Replication
{
    using System;
    using VRage.Network;

    public abstract class MyExternalReplicableEvent<T> : MyExternalReplicable<T>, IMyProxyTarget, IMyNetObject, IMyEventOwner where T: IMyEventProxy
    {
        private IMyEventProxy m_proxy;

        protected MyExternalReplicableEvent()
        {
        }

        protected override void OnHook()
        {
            this.m_proxy = base.Instance;
        }

        IMyEventProxy IMyProxyTarget.Target =>
            this.m_proxy;
    }
}

