namespace Sandbox.Game.GameSystems
{
    using System;
    using VRage.Game.Entity;

    public class DroneTarget : IComparable<DroneTarget>
    {
        public MyEntity Target;
        public int Priority;

        public DroneTarget(MyEntity target)
        {
            this.Target = target;
            this.Priority = 1;
        }

        public DroneTarget(MyEntity target, int priority)
        {
            this.Target = target;
            this.Priority = priority;
        }

        public int CompareTo(DroneTarget other) => 
            this.Priority.CompareTo(other.Priority);
    }
}

