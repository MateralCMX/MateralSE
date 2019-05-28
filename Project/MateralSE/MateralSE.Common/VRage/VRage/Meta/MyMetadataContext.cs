namespace VRage.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Library.Collections;

    public class MyMetadataContext
    {
        protected readonly Dictionary<Type, IMyMetadataIndexer> Indexers = new Dictionary<Type, IMyMetadataIndexer>();
        protected readonly MyListDictionary<Type, IMyAttributeIndexer> AttributeIndexers = new MyListDictionary<Type, IMyAttributeIndexer>();
        protected readonly List<IMyTypeIndexer> TypeIndexers = new List<IMyTypeIndexer>();
        protected readonly HashSet<Assembly> KnownAssemblies = new HashSet<Assembly>();
        public bool RegisterIndexers = true;

        protected internal virtual void Activate()
        {
            using (Dictionary<Type, IMyMetadataIndexer>.ValueCollection.Enumerator enumerator = this.Indexers.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Activate();
                }
            }
        }

        internal void AddIndexer(Type typeIndexer)
        {
            IMyMetadataIndexer metaIndexer = this.GetMetaIndexer(typeIndexer);
            this.TypeIndexers.Add((IMyTypeIndexer) metaIndexer);
            metaIndexer.Activate();
        }

        internal void AddIndexer(Type attributeType, Type indexerType)
        {
            IMyMetadataIndexer metaIndexer = this.GetMetaIndexer(indexerType);
            this.AttributeIndexers.Add(attributeType, (IMyAttributeIndexer) metaIndexer);
            metaIndexer.Activate();
        }

        internal void AddIndexers(IEnumerable<KeyValuePair<Type, HashSet<Type>>> indexerTypes)
        {
            foreach (KeyValuePair<Type, HashSet<Type>> pair in indexerTypes)
            {
                foreach (Type type in pair.Value)
                {
                    this.AddIndexer(pair.Key, type);
                }
            }
        }

        internal void AddIndexers(IEnumerable<Type> typeIndexers)
        {
            foreach (Type type in typeIndexers)
            {
                this.AddIndexer(type);
            }
        }

        protected internal virtual void Close()
        {
            using (Dictionary<Type, IMyMetadataIndexer>.ValueCollection.Enumerator enumerator = this.Indexers.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
            this.AttributeIndexers.Clear();
            this.KnownAssemblies.Clear();
            this.Indexers.Clear();
        }

        public void FinishBatch()
        {
            using (Dictionary<Type, IMyMetadataIndexer>.ValueCollection.Enumerator enumerator = this.Indexers.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Process();
                }
            }
        }

        private IMyMetadataIndexer GetMetaIndexer(Type indexerType)
        {
            IMyMetadataIndexer indexer;
            if (!this.Indexers.TryGetValue(indexerType, out indexer))
            {
                indexer = (IMyMetadataIndexer) Activator.CreateInstance(indexerType);
                this.Indexers.Add(indexerType, indexer);
            }
            return indexer;
        }

        public void Hook(MyMetadataContext parent)
        {
            foreach (KeyValuePair<Type, IMyMetadataIndexer> pair in this.Indexers)
            {
                IMyMetadataIndexer indexer;
                if (parent.Indexers.TryGetValue(pair.Key, out indexer))
                {
                    pair.Value.SetParent(indexer);
                }
            }
        }

        internal void Index(IEnumerable<Assembly> assemblies, bool batch = false)
        {
            foreach (Assembly assembly in assemblies)
            {
                this.Index(assembly, false);
            }
            if (!batch)
            {
                this.FinishBatch();
            }
        }

        protected internal virtual void Index(Assembly assembly, bool batch = false)
        {
            if (!this.KnownAssemblies.Contains(assembly))
            {
                this.KnownAssemblies.Add(assembly);
                if (this.RegisterIndexers)
                {
                    this.PreProcess(assembly);
                }
                Module[] loadedModules = assembly.GetLoadedModules();
                int index = 0;
                while (index < loadedModules.Length)
                {
                    Type[] types = loadedModules[index].GetTypes();
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= types.Length)
                        {
                            index++;
                            break;
                        }
                        Type element = types[num2];
                        Attribute[] customAttributes = Attribute.GetCustomAttributes(element);
                        int num3 = 0;
                        while (true)
                        {
                            List<IMyAttributeIndexer> list;
                            if (num3 >= customAttributes.Length)
                            {
                                int num4 = 0;
                                while (true)
                                {
                                    if (num4 >= this.TypeIndexers.Count)
                                    {
                                        num2++;
                                        break;
                                    }
                                    this.TypeIndexers[num4].Index(element);
                                    num4++;
                                }
                                break;
                            }
                            if (this.AttributeIndexers.TryGet(customAttributes[num3].GetType(), out list))
                            {
                                using (List<IMyAttributeIndexer>.Enumerator enumerator = list.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        enumerator.Current.Observe(customAttributes[num3], element);
                                    }
                                }
                            }
                            num3++;
                        }
                    }
                }
                if (!batch)
                {
                    this.FinishBatch();
                }
            }
        }

        private void PreProcess(Assembly assembly)
        {
            Module[] loadedModules = assembly.GetLoadedModules();
            int index = 0;
            while (index < loadedModules.Length)
            {
                Type[] types = loadedModules[index].GetTypes();
                int num2 = 0;
                while (true)
                {
                    if (num2 >= types.Length)
                    {
                        index++;
                        break;
                    }
                    Type element = types[num2];
                    if (element.HasAttribute<PreloadRequiredAttribute>())
                    {
                        RuntimeHelpers.RunClassConstructor(element.TypeHandle);
                    }
                    foreach (MyAttributeMetadataIndexerAttributeBase local1 in element.GetCustomAttributes<MyAttributeMetadataIndexerAttributeBase>())
                    {
                        Type attributeType = local1.AttributeType;
                        Type targetType = local1.TargetType;
                        MyMetadataSystem.RegisterAttributeIndexer(attributeType, targetType ?? element);
                    }
                    if (element.GetCustomAttribute<MyTypeMetadataIndexerAttribute>() != null)
                    {
                        MyMetadataSystem.RegisterTypeIndexer(element);
                    }
                    num2++;
                }
            }
        }

        public bool TryGetIndexer(Type type, out IMyMetadataIndexer indexer) => 
            this.Indexers.TryGetValue(type, out indexer);

        public HashSetReader<Assembly> Known =>
            this.KnownAssemblies;
    }
}

