namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public abstract class MyReplicationLayerBase
    {
        private static DBNull e = DBNull.Value;
        protected readonly MyTypeTable m_typeTable = new MyTypeTable();
        protected EndpointId m_localEndpoint;

        protected MyReplicationLayerBase()
        {
        }

        public abstract void AdvanceSyncTime();
        protected abstract void DispatchEvent<T1, T2, T3, T4, T5, T6, T7, T8>(VRage.Network.CallSite callSite, EndpointId recipient, Vector3D? position, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5, ref T6 arg6, ref T7 arg7, ref T8 arg8) where T1: IMyEventOwner where T8: IMyEventOwner;
        private VRage.Network.CallSite GetCallSite<T>(Func<T, Delegate> callSiteGetter, T arg)
        {
            VRage.Network.CallSite site;
            if (arg == null)
            {
                this.TryGetStaticCallSite<T>(callSiteGetter, out site);
            }
            else
            {
                this.TryGetInstanceCallSite<T>(callSiteGetter, arg, out site);
            }
            if (site != null)
            {
                return site;
            }
            MethodInfo method = callSiteGetter(arg).Method;
            if (!method.HasAttribute<EventAttribute>())
            {
                throw new InvalidOperationException($"Event '{method.Name}' in type '{method.DeclaringType.Name}' is missing attribute '{typeof(EventAttribute).Name}'");
            }
            if ((method.DeclaringType.HasAttribute<StaticEventOwnerAttribute>() || typeof(IMyEventProxy).IsAssignableFrom(method.DeclaringType)) || typeof(IMyNetObject).IsAssignableFrom(method.DeclaringType))
            {
                throw new InvalidOperationException($"Event '{method.Name}' not found, is declaring type '{method.DeclaringType.Name}' registered within replication layer?");
            }
            throw new InvalidOperationException($"Event '{method.Name}' is defined in type '{method.DeclaringType.Name}', which does not implement '{typeof(IMyEventOwner).Name}' or '{typeof(IMyNetObject).Name}' or has attribute '{typeof(StaticEventOwnerAttribute).Name}'");
        }

        public Type GetType(TypeId id) => 
            this.m_typeTable.Get(id).Type;

        public TypeId GetTypeId(Type id) => 
            this.m_typeTable.Get(id).TypeId;

        internal void InvokeLocally<T1, T2, T3, T4, T5, T6, T7>(CallSite<T1, T2, T3, T4, T5, T6, T7> site, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            using (MyEventContext.Set(this.m_localEndpoint, null, true))
            {
                site.Handler(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }

        public void RaiseEvent<T1, T2>(T1 arg1, T2 arg2, Func<T1, Action> action, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T2: IMyEventOwner
        {
            this.DispatchEvent<T1, DBNull, DBNull, DBNull, DBNull, DBNull, DBNull, T2>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref e, ref e, ref e, ref e, ref e, ref e, ref arg2);
        }

        public void RaiseEvent<T1, T2, T3>(T1 arg1, T3 arg3, Func<T1, Action<T2>> action, T2 arg2, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T3: IMyEventOwner
        {
            this.DispatchEvent<T1, T2, DBNull, DBNull, DBNull, DBNull, DBNull, T3>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref arg2, ref e, ref e, ref e, ref e, ref e, ref arg3);
        }

        public void RaiseEvent<T1, T2, T3, T4>(T1 arg1, T4 arg4, Func<T1, Action<T2, T3>> action, T2 arg2, T3 arg3, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T4: IMyEventOwner
        {
            this.DispatchEvent<T1, T2, T3, DBNull, DBNull, DBNull, DBNull, T4>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref arg2, ref arg3, ref e, ref e, ref e, ref e, ref arg4);
        }

        public void RaiseEvent<T1, T2, T3, T4, T5>(T1 arg1, T5 arg5, Func<T1, Action<T2, T3, T4>> action, T2 arg2, T3 arg3, T4 arg4, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T5: IMyEventOwner
        {
            this.DispatchEvent<T1, T2, T3, T4, DBNull, DBNull, DBNull, T5>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref arg2, ref arg3, ref arg4, ref e, ref e, ref e, ref arg5);
        }

        public void RaiseEvent<T1, T2, T3, T4, T5, T6>(T1 arg1, T6 arg6, Func<T1, Action<T2, T3, T4, T5>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T6: IMyEventOwner
        {
            this.DispatchEvent<T1, T2, T3, T4, T5, DBNull, DBNull, T6>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref arg2, ref arg3, ref arg4, ref arg5, ref e, ref e, ref arg6);
        }

        public void RaiseEvent<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T7 arg7, Func<T1, Action<T2, T3, T4, T5, T6>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T7: IMyEventOwner
        {
            this.DispatchEvent<T1, T2, T3, T4, T5, T6, DBNull, T7>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref arg2, ref arg3, ref arg4, ref arg5, ref arg6, ref e, ref arg7);
        }

        public void RaiseEvent<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T8 arg8, Func<T1, Action<T2, T3, T4, T5, T6, T7>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, EndpointId endpointId = new EndpointId(), Vector3D? position = new Vector3D?()) where T1: IMyEventOwner where T8: IMyEventOwner
        {
            this.DispatchEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this.GetCallSite<T1>(action, arg1), endpointId, position, ref arg1, ref arg2, ref arg3, ref arg4, ref arg5, ref arg6, ref arg7, ref arg8);
        }

        public void RegisterFromAssembly(IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                this.RegisterFromAssembly(assembly);
            }
        }

        public void RegisterFromAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (MyTypeTable.ShouldRegister(type))
                    {
                        this.m_typeTable.Register(type);
                    }
                }
            }
        }

        public void SetLocalEndpoint(EndpointId localEndpoint)
        {
            this.m_localEndpoint = localEndpoint;
        }

        protected static bool ShouldServerInvokeLocally(VRage.Network.CallSite site, EndpointId localClientEndpoint, EndpointId recipient) => 
            (site.HasServerFlag || ((recipient == localClientEndpoint) && (site.HasClientFlag || site.HasBroadcastFlag)));

        private bool TryGetInstanceCallSite<T>(Func<T, Delegate> callSiteGetter, T arg, out VRage.Network.CallSite site) => 
            this.m_typeTable.Get(arg.GetType()).EventTable.TryGet<T>(callSiteGetter, callSiteGetter, arg, out site);

        private bool TryGetStaticCallSite<T>(Func<T, Delegate> callSiteGetter, out VRage.Network.CallSite site)
        {
            T arg = default(T);
            return this.m_typeTable.StaticEventTable.TryGet<T>(callSiteGetter, callSiteGetter, arg, out site);
        }

        public DateTime LastMessageFromServer { get; protected set; }
    }
}

