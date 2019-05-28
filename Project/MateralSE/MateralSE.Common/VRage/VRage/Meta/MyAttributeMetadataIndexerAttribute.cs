namespace VRage.Meta
{
    using System;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
    public class MyAttributeMetadataIndexerAttribute : MyAttributeMetadataIndexerAttributeBase
    {
        private readonly Type m_attrType;
        private readonly Type m_target;

        public MyAttributeMetadataIndexerAttribute(Type attrType)
        {
            this.m_attrType = attrType;
            this.m_target = null;
        }

        public MyAttributeMetadataIndexerAttribute(Type attrType, Type indexerType)
        {
            this.m_attrType = attrType;
            this.m_target = indexerType;
        }

        public override Type AttributeType =>
            this.m_attrType;

        public override Type TargetType =>
            this.m_target;
    }
}

