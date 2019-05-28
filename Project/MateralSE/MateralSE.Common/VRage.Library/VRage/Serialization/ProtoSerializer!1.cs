namespace VRage.Serialization
{
    using ProtoBuf.Meta;
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public class ProtoSerializer<T> : ISerializer<T>
    {
        public readonly RuntimeTypeModel Model;
        public static readonly ProtoSerializer<T> Default;

        static ProtoSerializer()
        {
            ProtoSerializer<T>.Default = new ProtoSerializer<T>(null);
        }

        public ProtoSerializer(RuntimeTypeModel model = null)
        {
            this.Model = model ?? RuntimeTypeModel.Default;
        }

        public void Deserialize(ByteStream source, out T data)
        {
            data = (T) this.Model.Deserialize(source, null, typeof(T));
        }

        public void Serialize(ByteStream destination, ref T data)
        {
            this.Model.Serialize(destination, (T) data);
        }
    }
}

