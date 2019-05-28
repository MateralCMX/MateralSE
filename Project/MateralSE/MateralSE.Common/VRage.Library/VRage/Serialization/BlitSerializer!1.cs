namespace VRage.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Utils;

    public class BlitSerializer<T> : ISerializer<T>
    {
        private static Reader<T> m_reader;
        private static Writer<T> m_writer;
        public static int StructSize;
        public static readonly BlitSerializer<T> Default;

        static BlitSerializer()
        {
            BlitSerializer<T>.m_reader = BlitSerializer<T>.GenerateReader();
            BlitSerializer<T>.m_writer = BlitSerializer<T>.GenerateWriter();
            BlitSerializer<T>.StructSize = (int) BlitSerializer<T>.GenerateSize()();
            BlitSerializer<T>.Default = new BlitSerializer<T>();
        }

        public BlitSerializer()
        {
            MyLibraryUtils.ThrowNonBlittable<T>();
        }

        public unsafe void Deserialize(ByteStream source, out T data)
        {
            source.CheckCapacity(source.Position + BlitSerializer<T>.StructSize);
            byte* buffer = &(source.Data[(int) ((IntPtr) source.Position)]);
            BlitSerializer<T>.m_reader(out data, buffer);
            fixed (byte* numRef = null)
            {
                source.Position += BlitSerializer<T>.StructSize;
                return;
            }
        }

        public void DeserializeList(ByteStream source, List<T> resultList)
        {
            int num = source.Read7BitEncodedInt();
            if (resultList.Capacity < num)
            {
                resultList.Capacity = num;
            }
            for (int i = 0; i < num; i++)
            {
                T local;
                this.Deserialize(source, out local);
                resultList.Add(local);
            }
        }

        private static unsafe Reader<T> GenerateReader()
        {
            Type[] parameterTypes = new Type[] { typeof(T).MakeByRefType(), typeof(byte*) };
            DynamicMethod method = new DynamicMethod(string.Empty, null, parameterTypes, Assembly.GetExecutingAssembly().ManifestModule);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Sizeof, typeof(T));
            iLGenerator.Emit(OpCodes.Cpblk);
            iLGenerator.Emit(OpCodes.Ret);
            return (Reader<T>) method.CreateDelegate(typeof(Reader<T>));
        }

        private static Size<T> GenerateSize()
        {
            DynamicMethod method = new DynamicMethod(string.Empty, typeof(uint), new Type[0], Assembly.GetExecutingAssembly().ManifestModule);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Sizeof, typeof(T));
            iLGenerator.Emit(OpCodes.Ret);
            return (Size<T>) method.CreateDelegate(typeof(Size<T>));
        }

        private static unsafe Writer<T> GenerateWriter()
        {
            Type[] parameterTypes = new Type[] { typeof(T).MakeByRefType(), typeof(byte*) };
            DynamicMethod method = new DynamicMethod(string.Empty, null, parameterTypes, Assembly.GetExecutingAssembly().ManifestModule);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Sizeof, typeof(T));
            iLGenerator.Emit(OpCodes.Cpblk);
            iLGenerator.Emit(OpCodes.Ret);
            return (Writer<T>) method.CreateDelegate(typeof(Writer<T>));
        }

        public unsafe void Serialize(ByteStream destination, ref T data)
        {
            destination.EnsureCapacity(destination.Position + BlitSerializer<T>.StructSize);
            byte* buffer = &(destination.Data[(int) ((IntPtr) destination.Position)]);
            BlitSerializer<T>.m_writer(ref data, buffer);
            fixed (byte* numRef = null)
            {
                destination.Position += BlitSerializer<T>.StructSize;
                return;
            }
        }

        public void SerializeList(ByteStream destination, List<T> data)
        {
            int count = data.Count;
            destination.Write7BitEncodedInt(count);
            for (int i = 0; i < count; i++)
            {
                T[] internalArray = data.GetInternalArray<T>();
                this.Serialize(destination, ref internalArray[i]);
            }
        }

        private unsafe delegate void Reader(out T data, byte* buffer);

        private delegate uint Size();

        private unsafe delegate void Writer(ref T data, byte* buffer);
    }
}

