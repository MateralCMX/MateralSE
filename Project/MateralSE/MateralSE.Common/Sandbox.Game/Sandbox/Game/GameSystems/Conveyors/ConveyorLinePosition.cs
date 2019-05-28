namespace Sandbox.Game.GameSystems.Conveyors
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct ConveyorLinePosition : IEquatable<ConveyorLinePosition>
    {
        public Vector3I LocalGridPosition;
        public VRageMath.Base6Directions.Direction Direction;
        public Vector3I VectorDirection =>
            Base6Directions.GetIntVector(this.Direction);
        public Vector3I NeighbourGridPosition =>
            ((Vector3I) (this.LocalGridPosition + Base6Directions.GetIntVector(this.Direction)));
        public ConveyorLinePosition(Vector3I gridPosition, VRageMath.Base6Directions.Direction direction)
        {
            this.LocalGridPosition = gridPosition;
            this.Direction = direction;
        }

        public ConveyorLinePosition GetConnectingPosition() => 
            new ConveyorLinePosition((Vector3I) (this.LocalGridPosition + this.VectorDirection), Base6Directions.GetFlippedDirection(this.Direction));

        public ConveyorLinePosition GetFlippedPosition() => 
            new ConveyorLinePosition(this.LocalGridPosition, Base6Directions.GetFlippedDirection(this.Direction));

        public bool Equals(ConveyorLinePosition other) => 
            ((this.LocalGridPosition == other.LocalGridPosition) && (this.Direction == other.Direction));

        public override int GetHashCode() => 
            (((((((int) (this.Direction * ((VRageMath.Base6Directions.Direction) 0x18d))) ^ this.LocalGridPosition.X) * ((int) ((VRageMath.Base6Directions.Direction) 0x18d))) ^ this.LocalGridPosition.Y) * ((int) ((VRageMath.Base6Directions.Direction) 0x18d))) ^ this.LocalGridPosition.Z);

        public override string ToString() => 
            (this.LocalGridPosition.ToString() + " -> " + this.Direction.ToString());
    }
}

