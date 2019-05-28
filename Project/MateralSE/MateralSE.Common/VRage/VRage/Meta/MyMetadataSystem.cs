namespace VRage.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Utils;

    public static class MyMetadataSystem
    {
        private static readonly MyHashSetDictionary<Type, Type> AttributeIndexers = new MyHashSetDictionary<Type, Type>();
        private static readonly List<Type> TypeIndexers = new List<Type>();
        private static readonly List<MyMetadataContext> Stack = new List<MyMetadataContext>();

        public static void FinishBatch()
        {
            if (ActiveContext != null)
            {
                ActiveContext.FinishBatch();
            }
        }

        public static Type GetType(string fullName, bool throwOnError)
        {
            Type type = Type.GetType(fullName, false);
            if (type != null)
            {
                return type;
            }
            int num = 0;
            while (true)
            {
                if (num >= Stack.Count)
                {
                    if (throwOnError)
                    {
                        throw new TypeLoadException($"Type {fullName} was not found in any registered assembly!");
                    }
                    return null;
                }
                HashSetReader<Assembly> known = Stack[num].Known;
                using (HashSet<Assembly>.Enumerator enumerator = known.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        type = enumerator.Current.GetType(fullName, false);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
                num++;
            }
        }

        public static void LoadAssembly(Assembly assembly, bool batch = false)
        {
            if (ActiveContext != null)
            {
                ActiveContext.Index(assembly, batch);
            }
            else
            {
                Log.Error("Assembly {0} will not be indexed because there are no registered indexers.", Array.Empty<object>());
            }
        }

        public static void PopContext()
        {
            if (Stack.Count == 0)
            {
                Log.Error("When popping metadata context: No context set.", Array.Empty<object>());
            }
            else
            {
                Stack.Pop<MyMetadataContext>().Close();
            }
        }

        public static MyMetadataContext PushMetadataContext()
        {
            MyMetadataContext context = new MyMetadataContext();
            PushMetadataContext(context);
            return context;
        }

        public static void PushMetadataContext(MyMetadataContext context)
        {
            context.AddIndexers(AttributeIndexers);
            context.AddIndexers(TypeIndexers);
            MyMetadataContext activeContext = ActiveContext;
            if (activeContext != null)
            {
                context.Hook(activeContext);
            }
            Stack.Add(context);
        }

        public static void RegisterAttributeIndexer(Type attributeType, Type indexerType)
        {
            if (!typeof(IMyAttributeIndexer).IsAssignableFrom(indexerType))
            {
                object[] args = new object[] { indexerType };
                Log.Error("Cannot register metadata indexer {0}, the type is not a IMyMetadataIndexer.", args);
            }
            else if (!indexerType.HasDefaultConstructor())
            {
                object[] args = new object[] { indexerType };
                Log.Error("Cannot register metadata indexer {0}, the type does not define a parameterless constructor.", args);
            }
            else if (!typeof(Attribute).IsAssignableFrom(attributeType))
            {
                object[] args = new object[] { indexerType, attributeType };
                Log.Error("Cannot register metadata indexer {0}, the indexed attribute {1} is not actually an attribute.", args);
            }
            else
            {
                AttributeIndexers.Add(attributeType, indexerType);
                using (List<MyMetadataContext>.Enumerator enumerator = Stack.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.AddIndexer(attributeType, indexerType);
                    }
                }
            }
        }

        public static void RegisterTypeIndexer(Type indexerType)
        {
            if (!typeof(IMyTypeIndexer).IsAssignableFrom(indexerType))
            {
                object[] args = new object[] { indexerType };
                Log.Error("Cannot register metadata indexer {0}, the type is not a IMyMetadataIndexer.", args);
            }
            else if (!indexerType.HasDefaultConstructor())
            {
                object[] args = new object[] { indexerType };
                Log.Error("Cannot register metadata indexer {0}, the type does not define a parameterless constructor.", args);
            }
            else
            {
                TypeIndexers.Add(indexerType);
                using (List<MyMetadataContext>.Enumerator enumerator = Stack.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.AddIndexer(indexerType);
                    }
                }
            }
        }

        public static MyMetadataContext ActiveContext =>
            Stack.LastOrDefault<MyMetadataContext>();

        public static MyLog Log =>
            MyLog.Default;
    }
}

