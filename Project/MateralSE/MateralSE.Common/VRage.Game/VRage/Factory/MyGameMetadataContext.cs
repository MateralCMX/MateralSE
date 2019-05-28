namespace VRage.Factory
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Meta;
    using VRage.Serialization;

    internal class MyGameMetadataContext : MyMetadataContext
    {
        protected override void Index(Assembly assembly, bool batch = false)
        {
            MyFactory.RegisterFromAssembly(assembly);
            base.Index(assembly, batch);
        }

        public void RegisterAttributeObserver(Type attributeType, AttributeObserver observer)
        {
            base.AttributeIndexers.Add(attributeType, new Crawler(observer));
        }

        internal class Crawler : IMyAttributeIndexer, IMyMetadataIndexer
        {
            public AttributeObserver Observer;

            public Crawler(AttributeObserver observer)
            {
                this.Observer = observer;
            }

            public void Activate()
            {
            }

            public void Close()
            {
            }

            public void Observe(Attribute attribute, Type type)
            {
                this.Observer(type, attribute);
            }

            public void Process()
            {
            }

            public void SetParent(IMyMetadataIndexer indexer)
            {
            }
        }
    }
}

