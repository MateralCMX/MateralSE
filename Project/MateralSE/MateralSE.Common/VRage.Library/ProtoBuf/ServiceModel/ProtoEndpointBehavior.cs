namespace ProtoBuf.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public class ProtoEndpointBehavior : IEndpointBehavior
    {
        private static void ReplaceDataContractSerializerOperationBehavior(OperationDescription description)
        {
            DataContractSerializerOperationBehavior item = description.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (item != null)
            {
                description.Behaviors.Remove(item);
                description.Behaviors.Add(new ProtoOperationBehavior(description));
            }
        }

        private static void ReplaceDataContractSerializerOperationBehavior(ServiceEndpoint serviceEndpoint)
        {
            foreach (OperationDescription description in serviceEndpoint.Contract.Operations)
            {
                ReplaceDataContractSerializerOperationBehavior(description);
            }
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            ReplaceDataContractSerializerOperationBehavior(endpoint);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            ReplaceDataContractSerializerOperationBehavior(endpoint);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }
}

