namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Game.Multiplayer;
    using System;
    using VRage;
    using VRage.Serialization;

    public class MyControlMessageCallback<TMsg> : IControlMessageCallback<TMsg>, ITransportCallback where TMsg: struct
    {
        private readonly ISerializer<TMsg> m_serializer;
        private readonly ControlMessageHandler<TMsg> m_callback;
        public readonly MyMessagePermissions Permission;

        public MyControlMessageCallback(ControlMessageHandler<TMsg> callback, ISerializer<TMsg> serializer, MyMessagePermissions permission)
        {
            this.m_callback = callback;
            this.m_serializer = serializer;
            this.Permission = permission;
        }

        void ITransportCallback.Receive(ByteStream source, ulong sender)
        {
            if (MySyncLayer.CheckReceivePermissions(sender, this.Permission))
            {
                TMsg local;
                try
                {
                    this.m_serializer.Deserialize(source, out local);
                }
                catch (Exception exception)
                {
                    MySandboxGame.Log.WriteLine(new Exception($"Error deserializing '{typeof(TMsg).Name}', message size '{source.Length}'", exception));
                    return;
                }
                this.m_callback(ref local, sender);
            }
        }

        public void Write(ByteStream destination, ref TMsg msg)
        {
            this.m_serializer.Serialize(destination, ref msg);
        }
    }
}

