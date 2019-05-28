namespace VRageMath
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyBlockOrientation
    {
        public static readonly MyBlockOrientation Identity;
        [ProtoMember(15)]
        public Base6Directions.Direction Forward;
        [ProtoMember(0x12)]
        public Base6Directions.Direction Up;
        public Base6Directions.Direction Left =>
            Base6Directions.GetLeft(this.Up, this.Forward);
        public bool IsValid =>
            Base6Directions.IsValidBlockOrientation(this.Forward, this.Up);
        public MyBlockOrientation(Base6Directions.Direction forward, Base6Directions.Direction up)
        {
            this.Forward = forward;
            this.Up = up;
        }

        public MyBlockOrientation(ref Quaternion q)
        {
            this.Forward = Base6Directions.GetForward((Quaternion) q);
            this.Up = Base6Directions.GetUp((Quaternion) q);
        }

        public MyBlockOrientation(ref Matrix m)
        {
            this.Forward = Base6Directions.GetForward(ref m);
            this.Up = Base6Directions.GetUp(ref m);
        }

        public void GetQuaternion(out Quaternion result)
        {
            Matrix matrix;
            this.GetMatrix(out matrix);
            Quaternion.CreateFromRotationMatrix(ref matrix, out result);
        }

        public void GetMatrix(out Matrix result)
        {
            Vector3 vector;
            Vector3 vector2;
            Base6Directions.GetVector(this.Forward, out vector);
            Base6Directions.GetVector(this.Up, out vector2);
            Matrix.CreateWorld(ref Vector3.Zero, ref vector, ref vector2, out result);
        }

        public override int GetHashCode() => 
            ((((byte) this.Forward) << 0x10) | ((int) this.Up));

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            MyBlockOrientation? nullable = obj as MyBlockOrientation?;
            return ((nullable != null) && (this == nullable.Value));
        }

        public override string ToString() => 
            $"[Forward:{this.Forward}, Up:{this.Up}]";

        public Base6Directions.Direction TransformDirection(Base6Directions.Direction baseDirection)
        {
            Base6Directions.Axis axis = Base6Directions.GetAxis(baseDirection);
            int num = (int) (baseDirection % Base6Directions.Direction.Left);
            return ((axis != Base6Directions.Axis.ForwardBackward) ? ((axis != Base6Directions.Axis.LeftRight) ? ((num == 1) ? Base6Directions.GetFlippedDirection(this.Up) : this.Up) : ((num == 1) ? Base6Directions.GetFlippedDirection(this.Left) : this.Left)) : ((num == 1) ? Base6Directions.GetFlippedDirection(this.Forward) : this.Forward));
        }

        public Base6Directions.Direction TransformDirectionInverse(Base6Directions.Direction baseDirection)
        {
            Base6Directions.Axis axis = Base6Directions.GetAxis(baseDirection);
            return ((axis != Base6Directions.GetAxis(this.Forward)) ? ((axis != Base6Directions.GetAxis(this.Left)) ? ((baseDirection == this.Up) ? Base6Directions.Direction.Up : Base6Directions.Direction.Down) : ((baseDirection == this.Left) ? Base6Directions.Direction.Left : Base6Directions.Direction.Right)) : ((baseDirection == this.Forward) ? Base6Directions.Direction.Forward : Base6Directions.Direction.Backward));
        }

        public static bool operator ==(MyBlockOrientation orientation1, MyBlockOrientation orientation2) => 
            ((orientation1.Forward == orientation2.Forward) && (orientation1.Up == orientation2.Up));

        public static bool operator !=(MyBlockOrientation orientation1, MyBlockOrientation orientation2) => 
            ((orientation1.Forward != orientation2.Forward) || (orientation1.Up != orientation2.Up));

        static MyBlockOrientation()
        {
            Identity = new MyBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);
        }
    }
}

