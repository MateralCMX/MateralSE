namespace VRage.Factory
{
    using System;
    using VRage;
    using VRage.Meta;
    using VRage.Utils;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited=false)]
    public class MyFactorableAttribute : MyAttributeMetadataIndexerAttributeBase
    {
        private readonly Type m_factoryType;
        private readonly Type m_attributeType;

        public MyFactorableAttribute(Type factoryType)
        {
            this.m_factoryType = factoryType;
            Type genericType = typeof(MyObjectFactory<,>);
            if (!this.m_factoryType.IsInstanceOfGenericType(genericType))
            {
                MyLog.Default.Error("Type {0} is not an object factory", Array.Empty<object>());
            }
            else
            {
                while (!factoryType.IsGenericType || (factoryType.GetGenericTypeDefinition() != genericType))
                {
                    factoryType = factoryType.BaseType;
                }
                this.m_attributeType = factoryType.GenericTypeArguments[0];
            }
        }

        public override Type AttributeType =>
            this.m_attributeType;

        public override Type TargetType =>
            this.m_factoryType;
    }
}

