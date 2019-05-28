namespace Sandbox.Engine.Multiplayer
{
    using System;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.Serialization;

    public class MyDynamicObjectResolver : IDynamicResolver
    {
        public void Serialize(BitStream stream, Type baseType, ref Type obj)
        {
            if (stream.Reading)
            {
                TypeId id = new TypeId(stream.ReadUInt32(0x20));
                obj = MyMultiplayer.Static.ReplicationLayer.GetType(id);
            }
            else
            {
                TypeId typeId = MyMultiplayer.Static.ReplicationLayer.GetTypeId(obj);
                stream.WriteUInt32((uint) typeId, 0x20);
            }
        }
    }
}

