namespace VRage.Network
{
    using System;

    public class MyReplicationSingle : MyReplicationLayerBase
    {
        public MyReplicationSingle(EndpointId localEndpoint)
        {
            base.SetLocalEndpoint(localEndpoint);
        }

        public override void AdvanceSyncTime()
        {
        }

        protected override void DispatchEvent<T1, T2, T3, T4, T5, T6, T7, T8>(CallSite callSite, EndpointId recipient, Vector3D? position, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5, ref T6 arg6, ref T7 arg7, ref T8 arg8) where T1: IMyEventOwner where T8: IMyEventOwner
        {
            if (ShouldServerInvokeLocally(callSite, base.m_localEndpoint, recipient))
            {
                CallSite<T1, T2, T3, T4, T5, T6, T7> site = (CallSite<T1, T2, T3, T4, T5, T6, T7>) callSite;
                base.InvokeLocally<T1, T2, T3, T4, T5, T6, T7>(site, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }
    }
}

