namespace SpaceEngineers.Game.Entities.Blocks
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyAirVentBlockRoomInfo : IEquatable<MyAirVentBlockRoomInfo>
    {
        public bool IsRoomAirtight;
        public float OxygenLevel;
        public float RoomEnvironmentOxygen;
        public MyAirVentBlockRoomInfo(bool isRoomAirtight, float oxygenLevel, float roomEnvironmentOxygen)
        {
            this.IsRoomAirtight = isRoomAirtight;
            this.OxygenLevel = oxygenLevel;
            this.RoomEnvironmentOxygen = roomEnvironmentOxygen;
        }

        public bool Equals(MyAirVentBlockRoomInfo other) => 
            ((this.IsRoomAirtight == other.IsRoomAirtight) && (MathHelper.IsEqual(this.OxygenLevel, other.OxygenLevel) && MathHelper.IsEqual(this.RoomEnvironmentOxygen, other.RoomEnvironmentOxygen)));
    }
}

