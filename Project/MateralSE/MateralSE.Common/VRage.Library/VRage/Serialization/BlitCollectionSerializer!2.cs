namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public class BlitCollectionSerializer<T, TData> : ISerializer<T> where T: ICollection<TData>, new()
    {
        public static readonly BlitCollectionSerializer<T, TData> Default;
        public static readonly BlitSerializer<TData> InnerSerializer;

        static BlitCollectionSerializer()
        {
            BlitCollectionSerializer<T, TData>.Default = new BlitCollectionSerializer<T, TData>();
            BlitCollectionSerializer<T, TData>.InnerSerializer = BlitSerializer<TData>.Default;
        }

        public void Deserialize(ByteStream source, out T data)
        {
            data = Activator.CreateInstance<T>();
            int num = source.Read7BitEncodedInt();
            for (int i = 0; i < num; i++)
            {
                TData local;
                BlitCollectionSerializer<T, TData>.InnerSerializer.Deserialize(source, out local);
                data.Add(local);
            }
        }

        public void Serialize(ByteStream destination, ref T data)
        {
            destination.Write7BitEncodedInt(data.Count);
            foreach (TData local in data)
            {
                TData local2 = local;
                BlitCollectionSerializer<T, TData>.InnerSerializer.Serialize(destination, ref local2);
            }
        }
    }
}

