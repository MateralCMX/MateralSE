namespace ProtoBuf.ServiceModel
{
    using System;
    using System.ServiceModel.Configuration;

    public class ProtoBehaviorExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior() => 
            new ProtoEndpointBehavior();

        public override Type BehaviorType =>
            typeof(ProtoEndpointBehavior);
    }
}

