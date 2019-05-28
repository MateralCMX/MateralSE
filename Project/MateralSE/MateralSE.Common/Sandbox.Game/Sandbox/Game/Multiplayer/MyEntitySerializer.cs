namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Serialization;

    internal class MyEntitySerializer : ISerializer<MyEntity>
    {
        public static readonly MyEntitySerializer Default = new MyEntitySerializer();

        void ISerializer<MyEntity>.Deserialize(ByteStream source, out MyEntity data)
        {
            long num;
            BlitSerializer<long>.Default.Deserialize(source, out num);
            MyEntities.TryGetEntityById(num, out data, false);
        }

        void ISerializer<MyEntity>.Serialize(ByteStream destination, ref MyEntity data)
        {
            long entityId = data.EntityId;
            BlitSerializer<long>.Default.Serialize(destination, ref entityId);
        }
    }
}

