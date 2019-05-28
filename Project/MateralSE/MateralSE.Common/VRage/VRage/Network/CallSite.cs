namespace VRage.Network
{
    using System;
    using System.Reflection;
    using VRage.Library.Collections;

    public abstract class CallSite
    {
        public readonly MySynchronizedTypeInfo OwnerType;
        public readonly uint Id;
        public readonly System.Reflection.MethodInfo MethodInfo;
        public readonly VRage.Network.CallSiteFlags CallSiteFlags;
        public readonly ValidationType ValidationFlags;

        public CallSite(MySynchronizedTypeInfo owner, uint id, System.Reflection.MethodInfo info, VRage.Network.CallSiteFlags flags, ValidationType validationFlags)
        {
            this.OwnerType = owner;
            this.Id = id;
            this.MethodInfo = info;
            this.CallSiteFlags = flags;
            this.ValidationFlags = validationFlags;
        }

        public abstract bool Invoke(BitStream stream, object obj, bool validate);
        public override string ToString() => 
            $"{this.MethodInfo.DeclaringType.Name}.{this.MethodInfo.Name}";

        public bool HasClientFlag =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.Client) == VRage.Network.CallSiteFlags.Client);

        public bool HasServerFlag =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.Server) == VRage.Network.CallSiteFlags.Server);

        public bool HasServerInvokedFlag =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.ServerInvoked) == VRage.Network.CallSiteFlags.ServerInvoked);

        public bool HasBroadcastFlag =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.Broadcast) == VRage.Network.CallSiteFlags.Broadcast);

        public bool HasBroadcastExceptFlag =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.BroadcastExcept) == VRage.Network.CallSiteFlags.BroadcastExcept);

        public bool HasRefreshReplicableFlag =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.RefreshReplicable) == VRage.Network.CallSiteFlags.RefreshReplicable);

        public bool IsReliable =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.Reliable) == VRage.Network.CallSiteFlags.Reliable);

        public bool IsBlocking =>
            ((this.CallSiteFlags & VRage.Network.CallSiteFlags.Blocking) == VRage.Network.CallSiteFlags.Blocking);
    }
}

