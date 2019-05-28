namespace VRage.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    public class MyEventTable
    {
        private MethodInfo m_createCallSite = typeof(MyEventTable).GetMethod("CreateCallSite", BindingFlags.NonPublic | BindingFlags.Instance);
        private Dictionary<uint, VRage.Network.CallSite> m_idToEvent;
        private Dictionary<MethodInfo, VRage.Network.CallSite> m_methodInfoLookup;
        private ConcurrentDictionary<object, VRage.Network.CallSite> m_associateObjectLookup;
        public readonly MySynchronizedTypeInfo Type;

        public MyEventTable(MySynchronizedTypeInfo type)
        {
            this.Type = type;
            if ((type == null) || (type.BaseType == null))
            {
                this.m_idToEvent = new Dictionary<uint, VRage.Network.CallSite>();
                this.m_methodInfoLookup = new Dictionary<MethodInfo, VRage.Network.CallSite>();
                this.m_associateObjectLookup = new ConcurrentDictionary<object, VRage.Network.CallSite>();
            }
            else
            {
                this.m_idToEvent = new Dictionary<uint, VRage.Network.CallSite>(type.BaseType.EventTable.m_idToEvent);
                this.m_methodInfoLookup = new Dictionary<MethodInfo, VRage.Network.CallSite>(type.BaseType.EventTable.m_methodInfoLookup);
                this.m_associateObjectLookup = new ConcurrentDictionary<object, VRage.Network.CallSite>(type.BaseType.EventTable.m_associateObjectLookup);
            }
            if (this.Type != null)
            {
                this.RegisterEvents();
            }
        }

        public void AddStaticEvents(System.Type fromType)
        {
            this.RegisterEvents(fromType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        private VRage.Network.CallSite CreateCallSite<T1, T2, T3, T4, T5, T6, T7>(MethodInfo info, uint id)
        {
            Expression expression;
            System.Type[] typeArray1 = new System.Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
            ParameterExpression[] source = (from s in typeArray1 select Expression.Parameter(s)).ToArray<ParameterExpression>();
            if (info.IsStatic)
            {
                expression = Expression.Call(info, (from s in source.Skip<ParameterExpression>(1)
                    where s.Type != typeof(DBNull)
                    select s).ToArray<ParameterExpression>());
            }
            else
            {
                expression = Expression.Call(source.First<ParameterExpression>(), info, (from s in source.Skip<ParameterExpression>(1)
                    where s.Type != typeof(DBNull)
                    select s).ToArray<ParameterExpression>());
            }
            Action<T1, T2, T3, T4, T5, T6, T7> handler = Expression.Lambda<Action<T1, T2, T3, T4, T5, T6, T7>>(expression, source).Compile();
            EventAttribute customAttribute = info.GetCustomAttribute<EventAttribute>();
            ServerAttribute attribute2 = info.GetCustomAttribute<ServerAttribute>();
            CallSiteFlags none = CallSiteFlags.None;
            if (attribute2 != null)
            {
                none |= CallSiteFlags.Server;
            }
            if (attribute2 is ServerInvokedAttribute)
            {
                none |= CallSiteFlags.ServerInvoked;
            }
            if (info.HasAttribute<ClientAttribute>())
            {
                none |= CallSiteFlags.Client;
            }
            if (info.HasAttribute<BroadcastAttribute>())
            {
                none |= CallSiteFlags.Broadcast;
            }
            if (info.HasAttribute<BroadcastExceptAttribute>())
            {
                none |= CallSiteFlags.BroadcastExcept;
            }
            if (info.HasAttribute<ReliableAttribute>())
            {
                none |= CallSiteFlags.Reliable;
            }
            if (info.HasAttribute<RefreshReplicableAttribute>())
            {
                none |= CallSiteFlags.RefreshReplicable;
            }
            if (info.HasAttribute<BlockingAttribute>())
            {
                none |= CallSiteFlags.Blocking;
            }
            SerializeDelegate<T1, T2, T3, T4, T5, T6, T7> delegate2 = null;
            Func<T1, T2, T3, T4, T5, T6, T7, bool> func = null;
            if (customAttribute.Serialization != null)
            {
                MethodInfo method = info.DeclaringType.GetMethod(customAttribute.Serialization, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method == null)
                {
                    throw new InvalidOperationException($"Serialization method '{customAttribute.Serialization}' for event '{info.Name}' defined by type '{info.DeclaringType.Name}' not found");
                }
                if (!method.GetParameters().Skip<ParameterInfo>(1).All<ParameterInfo>(s => s.ParameterType.IsByRef))
                {
                    throw new InvalidOperationException($"Serialization method '{customAttribute.Serialization}' for event '{info.Name}' defined by type '{info.DeclaringType.Name}' must have all arguments passed with 'ref' keyword (except BitStream)");
                }
                ParameterExpression[] parameters = MethodInfoExtensions.ExtractParameterExpressionsFrom<SerializeDelegate<T1, T2, T3, T4, T5, T6, T7>>();
                delegate2 = Expression.Lambda<SerializeDelegate<T1, T2, T3, T4, T5, T6, T7>>(Expression.Call(parameters.First<ParameterExpression>(), method, (from s in parameters.Skip<ParameterExpression>(1)
                    where s.Type != typeof(DBNull)
                    select s).ToArray<ParameterExpression>()), parameters).Compile();
            }
            if ((attribute2 != null) && (attribute2.Validation != null))
            {
                MethodInfo method = info.DeclaringType.GetMethod(attribute2.Validation, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method == null)
                {
                    throw new InvalidOperationException($"Validation method '{attribute2.Validation}' for event '{info.Name}' defined by type '{info.DeclaringType.Name}' not found");
                }
                ParameterExpression[] parameters = MethodInfoExtensions.ExtractParameterExpressionsFrom<Func<T1, T2, T3, T4, T5, T6, T7, bool>>();
                func = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(Expression.Call(parameters.First<ParameterExpression>(), method, (from s in parameters.Skip<ParameterExpression>(1)
                    where s.Type != typeof(DBNull)
                    select s).ToArray<ParameterExpression>()), parameters).Compile();
            }
            ValidationType validationFlags = ValidationType.None;
            if (attribute2 != null)
            {
                validationFlags = attribute2.ValidationFlags;
            }
            return new CallSite<T1, T2, T3, T4, T5, T6, T7>(this.Type, id, info, none, handler, delegate2 ?? this.CreateSerializer<T1, T2, T3, T4, T5, T6, T7>(info), func ?? this.CreateValidator<T1, T2, T3, T4, T5, T6, T7>(), validationFlags);
        }

        private SerializeDelegate<T1, T2, T3, T4, T5, T6, T7> CreateSerializer<T1, T2, T3, T4, T5, T6, T7>(MethodInfo info)
        {
            MySerializer<T2> s2 = MyFactory.GetSerializer<T2>();
            MySerializer<T3> s3 = MyFactory.GetSerializer<T3>();
            MySerializer<T4> s4 = MyFactory.GetSerializer<T4>();
            MySerializer<T5> s5 = MyFactory.GetSerializer<T5>();
            MySerializer<T6> s6 = MyFactory.GetSerializer<T6>();
            MySerializer<T7> s7 = MyFactory.GetSerializer<T7>();
            ParameterInfo[] parameters = info.GetParameters();
            MySerializeInfo info2 = MySerializeInfo.CreateForParameter(parameters, 0);
            MySerializeInfo info3 = MySerializeInfo.CreateForParameter(parameters, 1);
            MySerializeInfo info4 = MySerializeInfo.CreateForParameter(parameters, 2);
            MySerializeInfo info5 = MySerializeInfo.CreateForParameter(parameters, 3);
            MySerializeInfo info6 = MySerializeInfo.CreateForParameter(parameters, 4);
            MySerializeInfo info7 = MySerializeInfo.CreateForParameter(parameters, 5);
            return delegate (T1 inst, BitStream stream, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5, ref T6 arg6, ref T7 arg7) {
                if (stream.Reading)
                {
                    MySerializationHelpers.CreateAndRead<T2>(stream, out arg2, s2, info2);
                    MySerializationHelpers.CreateAndRead<T3>(stream, out arg3, s3, info3);
                    MySerializationHelpers.CreateAndRead<T4>(stream, out arg4, s4, info4);
                    MySerializationHelpers.CreateAndRead<T5>(stream, out arg5, s5, info5);
                    MySerializationHelpers.CreateAndRead<T6>(stream, out arg6, s6, info6);
                    MySerializationHelpers.CreateAndRead<T7>(stream, out arg7, s7, info7);
                }
                else
                {
                    MySerializationHelpers.Write<T2>(stream, ref arg2, s2, info2);
                    MySerializationHelpers.Write<T3>(stream, ref arg3, s3, info3);
                    MySerializationHelpers.Write<T4>(stream, ref arg4, s4, info4);
                    MySerializationHelpers.Write<T5>(stream, ref arg5, s5, info5);
                    MySerializationHelpers.Write<T6>(stream, ref arg6, s6, info6);
                    MySerializationHelpers.Write<T7>(stream, ref arg7, s7, info7);
                }
            };
        }

        private Func<T1, T2, T3, T4, T5, T6, T7, bool> CreateValidator<T1, T2, T3, T4, T5, T6, T7>() => 
            (a1, a2, a3, a4, a5, a6, a7) => true;

        public VRage.Network.CallSite Get(uint id) => 
            this.m_idToEvent[id];

        public VRage.Network.CallSite Get<T>(object associatedObject, Func<T, Delegate> getter, T arg)
        {
            VRage.Network.CallSite orAdd;
            if (!this.m_associateObjectLookup.TryGetValue(associatedObject, out orAdd))
            {
                MethodInfo method = getter(arg).Method;
                orAdd = this.m_methodInfoLookup[method];
                orAdd = this.m_associateObjectLookup.GetOrAdd(associatedObject, orAdd);
            }
            return orAdd;
        }

        private void RegisterEvents()
        {
            this.RegisterEvents(this.Type.Type, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        private void RegisterEvents(System.Type type, BindingFlags flags)
        {
            using (IEnumerator<EventReturn> enumerator = (from s in type.GetMethods(flags)
                select new EventReturn(s.GetCustomAttribute<EventAttribute>(), s) into s
                where s.Event != null
                orderby s.Event.Order
                select s).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MethodInfo method = enumerator.Current.Method;
                    System.Type type2 = method.IsStatic ? typeof(IMyEventOwner) : method.DeclaringType;
                    System.Type[] typeArguments = new System.Type[] { type2, typeof(DBNull), typeof(DBNull), typeof(DBNull), typeof(DBNull), typeof(DBNull), typeof(DBNull) };
                    ParameterInfo[] parameters = method.GetParameters();
                    int index = 0;
                    while (true)
                    {
                        if (index >= parameters.Length)
                        {
                            object[] objArray1 = new object[] { method, (uint) this.m_idToEvent.Count };
                            VRage.Network.CallSite site = (VRage.Network.CallSite) this.m_createCallSite.MakeGenericMethod(typeArguments).Invoke(this, objArray1);
                            if ((((site.HasBroadcastExceptFlag ? 1 : 0) + (site.HasBroadcastFlag ? 1 : 0)) + (site.HasClientFlag ? 1 : 0)) > 1)
                            {
                                throw new InvalidOperationException($"Event '{site}' can have only one of [Client], [Broadcast], [BroadcastExcept] attributes");
                            }
                            this.m_idToEvent.Add(site.Id, site);
                            this.m_methodInfoLookup.Add(method, site);
                            break;
                        }
                        typeArguments[index + 1] = parameters[index].ParameterType;
                        index++;
                    }
                }
            }
        }

        public bool TryGet<T>(object associatedObject, Func<T, Delegate> getter, T arg, out VRage.Network.CallSite site)
        {
            if (!this.m_associateObjectLookup.TryGetValue(associatedObject, out site))
            {
                MethodInfo method = getter(arg).Method;
                if (!this.m_methodInfoLookup.TryGetValue(method, out site))
                {
                    return false;
                }
                site = this.m_associateObjectLookup.GetOrAdd(associatedObject, site);
            }
            return true;
        }

        public int Count =>
            this.m_idToEvent.Count;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEventTable.<>c <>9 = new MyEventTable.<>c();
            public static Func<MethodInfo, MyEventTable.EventReturn> <>9__14_0;
            public static Func<MyEventTable.EventReturn, bool> <>9__14_1;
            public static Func<MyEventTable.EventReturn, int> <>9__14_2;

            internal MyEventTable.EventReturn <RegisterEvents>b__14_0(MethodInfo s) => 
                new MyEventTable.EventReturn(s.GetCustomAttribute<EventAttribute>(), s);

            internal bool <RegisterEvents>b__14_1(MyEventTable.EventReturn s) => 
                (s.Event != null);

            internal int <RegisterEvents>b__14_2(MyEventTable.EventReturn s) => 
                s.Event.Order;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__15<T1, T2, T3, T4, T5, T6, T7>
        {
            public static readonly MyEventTable.<>c__15<T1, T2, T3, T4, T5, T6, T7> <>9;
            public static Func<Type, ParameterExpression> <>9__15_0;
            public static Func<ParameterExpression, bool> <>9__15_1;
            public static Func<ParameterExpression, bool> <>9__15_2;
            public static Func<ParameterInfo, bool> <>9__15_3;
            public static Func<ParameterExpression, bool> <>9__15_4;
            public static Func<ParameterExpression, bool> <>9__15_5;

            static <>c__15()
            {
                MyEventTable.<>c__15<T1, T2, T3, T4, T5, T6, T7>.<>9 = new MyEventTable.<>c__15<T1, T2, T3, T4, T5, T6, T7>();
            }

            internal ParameterExpression <CreateCallSite>b__15_0(Type s) => 
                Expression.Parameter(s);

            internal bool <CreateCallSite>b__15_1(ParameterExpression s) => 
                (s.Type != typeof(DBNull));

            internal bool <CreateCallSite>b__15_2(ParameterExpression s) => 
                (s.Type != typeof(DBNull));

            internal bool <CreateCallSite>b__15_3(ParameterInfo s) => 
                s.ParameterType.IsByRef;

            internal bool <CreateCallSite>b__15_4(ParameterExpression s) => 
                (s.Type != typeof(DBNull));

            internal bool <CreateCallSite>b__15_5(ParameterExpression s) => 
                (s.Type != typeof(DBNull));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__17<T1, T2, T3, T4, T5, T6, T7>
        {
            public static readonly MyEventTable.<>c__17<T1, T2, T3, T4, T5, T6, T7> <>9;
            public static Func<T1, T2, T3, T4, T5, T6, T7, bool> <>9__17_0;

            static <>c__17()
            {
                MyEventTable.<>c__17<T1, T2, T3, T4, T5, T6, T7>.<>9 = new MyEventTable.<>c__17<T1, T2, T3, T4, T5, T6, T7>();
            }

            internal bool <CreateValidator>b__17_0(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7) => 
                true;
        }

        private class EventReturn
        {
            public MethodInfo Method;
            public EventAttribute Event;

            public EventReturn(EventAttribute _event, MethodInfo _method)
            {
                this.Event = _event;
                this.Method = _method;
            }
        }
    }
}

