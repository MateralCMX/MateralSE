namespace VRage.Scripting
{
    using Sandbox.ModAPI;
    using System;
    using System.Reflection;

    public interface IMyWhitelistBatch : IDisposable
    {
        void AllowMembers(MyWhitelistTarget target, params MemberInfo[] members);
        void AllowNamespaceOfTypes(MyWhitelistTarget target, params Type[] types);
        void AllowTypes(MyWhitelistTarget target, params Type[] types);
    }
}

