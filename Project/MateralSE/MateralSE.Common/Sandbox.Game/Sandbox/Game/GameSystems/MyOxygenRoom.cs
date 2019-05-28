namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRageMath;

    public class MyOxygenRoom
    {
        public MyOxygenRoom()
        {
            this.IsAirtight = true;
        }

        public MyOxygenRoom(int index)
        {
            this.IsAirtight = true;
            this.EnvironmentOxygen = 0f;
            this.Index = index;
            this.OxygenAmount = 0f;
            this.BlockCount = 0;
            this.DepressurizationTime = 0;
        }

        public float MaxOxygen(float gridSize) => 
            (((this.BlockCount * gridSize) * gridSize) * gridSize);

        public float MissingOxygen(float gridSize) => 
            ((float) Math.Max((double) (this.MaxOxygen(gridSize) - this.OxygenAmount), 0.0));

        public float OxygenLevel(float gridSize) => 
            (this.OxygenAmount / this.MaxOxygen(gridSize));

        public int Index { get; set; }

        public bool IsAirtight { get; set; }

        public float EnvironmentOxygen { get; set; }

        public float OxygenAmount { get; set; }

        public int BlockCount { get; set; }

        public int DepressurizationTime { get; set; }

        [XmlIgnore]
        public MyOxygenRoomLink Link { get; set; }

        public bool IsDirty { get; set; }

        public HashSet<Vector3I> Blocks { get; set; }

        public Vector3I StartingPosition { get; set; }
    }
}

