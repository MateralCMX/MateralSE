namespace Sandbox.Game.Replication.ClientStates
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGridClientState
    {
        public bool Valid;
        public Vector3 Move;
        public Vector2 Rotation;
        public float Roll;
        public MyGridClientState(BitStream stream)
        {
            Vector2 vector = new Vector2 {
                X = stream.ReadFloat(),
                Y = stream.ReadFloat()
            };
            this.Rotation = vector;
            this.Roll = stream.ReadHalf();
            Vector3 vector2 = new Vector3 {
                X = stream.ReadHalf(),
                Y = stream.ReadHalf(),
                Z = stream.ReadHalf()
            };
            this.Move = vector2;
            this.Valid = true;
        }

        public void Serialize(BitStream stream)
        {
            stream.WriteFloat(this.Rotation.X);
            stream.WriteFloat(this.Rotation.Y);
            stream.WriteHalf(this.Roll);
            stream.WriteHalf(this.Move.X);
            stream.WriteHalf(this.Move.Y);
            stream.WriteHalf(this.Move.Z);
        }
    }
}

