namespace VRage.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyAttributeIndexerBase<TAttribute, TKey> : IMyAttributeIndexer, IMyMetadataIndexer where TAttribute: Attribute, IMyKeyAttribute<TKey>
    {
        protected Dictionary<TKey, Type> IndexedTypes;
        protected MyAttributeIndexerBase<TAttribute, TKey> Parent;
        public static MyAttributeIndexerBase<TAttribute, TKey> Static;

        public MyAttributeIndexerBase()
        {
            this.IndexedTypes = new Dictionary<TKey, Type>();
        }

        public virtual void Activate()
        {
            MyAttributeIndexerBase<TAttribute, TKey>.Static = (MyAttributeIndexerBase<TAttribute, TKey>) this;
        }

        public virtual void Close()
        {
            this.IndexedTypes.Clear();
        }

        public virtual void Observe(Attribute attribute, Type type)
        {
            this.Observe((TAttribute) attribute, type);
        }

        protected virtual void Observe(TAttribute attribute, Type type)
        {
            this.IndexedTypes.Add(attribute.Key, type);
        }

        public virtual void Process()
        {
        }

        public virtual void SetParent(IMyMetadataIndexer indexer)
        {
            this.Parent = (MyAttributeIndexerBase<TAttribute, TKey>) indexer;
        }

        public bool TryGetType(TKey key, out Type indexedType) => 
            (!this.IndexedTypes.TryGetValue(key, out indexedType) ? ((this.Parent != null) && this.Parent.TryGetType(key, out indexedType)) : true);
    }
}

