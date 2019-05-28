namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRageMath;

    public class MyOxygenBlock
    {
        public MyOxygenBlock()
        {
        }

        public MyOxygenBlock(MyOxygenRoomLink roomPointer)
        {
            this.RoomLink = roomPointer;
        }

        internal float OxygenAmount()
        {
            if (this.Room == null)
            {
                return 0f;
            }
            float num = this.Room.IsAirtight ? (this.Room.OxygenAmount / ((float) this.Room.BlockCount)) : this.Room.EnvironmentOxygen;
            float amount = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.OxygenChangeTime)) / 1500f;
            if (amount > 1f)
            {
                amount = 1f;
            }
            return MathHelper.Lerp(this.PreviousOxygenAmount, num, amount);
        }

        public float OxygenLevel(float gridSize) => 
            (this.OxygenAmount() / ((gridSize * gridSize) * gridSize));

        public override string ToString()
        {
            object[] objArray1 = new object[] { "MyOxygenBlock - Oxygen: ", this.OxygenAmount(), "/", this.PreviousOxygenAmount };
            return string.Concat(objArray1);
        }

        [XmlIgnore]
        public MyOxygenRoomLink RoomLink { get; set; }

        public float PreviousOxygenAmount { get; set; }

        public int OxygenChangeTime { get; set; }

        public MyOxygenRoom Room =>
            this.RoomLink?.Room;
    }
}

