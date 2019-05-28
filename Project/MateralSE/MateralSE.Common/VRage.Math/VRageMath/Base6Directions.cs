namespace VRageMath
{
    using System;
    using System.Runtime.InteropServices;

    public class Base6Directions
    {
        public static readonly Direction[] EnumDirections = new Direction[] { Direction.Forward, Direction.Backward, Direction.Left, Direction.Right, Direction.Up, Direction.Down };
        public static readonly Vector3[] Directions = new Vector3[] { Vector3.Forward, Vector3.Backward, Vector3.Left, Vector3.Right, Vector3.Up, Vector3.Down };
        public static readonly Vector3I[] IntDirections = new Vector3I[] { Vector3I.Forward, Vector3I.Backward, Vector3I.Left, Vector3I.Right, Vector3I.Up, Vector3I.Down };
        private static readonly Direction[] LeftDirections = new Direction[] { 
            Direction.Forward, Direction.Forward, Direction.Down, Direction.Up, Direction.Left, Direction.Right, Direction.Forward, Direction.Forward, Direction.Up, Direction.Down, Direction.Right, Direction.Left, Direction.Up, Direction.Down, Direction.Left, Direction.Left,
            Direction.Backward, Direction.Forward, Direction.Down, Direction.Up, Direction.Left, Direction.Left, Direction.Forward, Direction.Backward, Direction.Right, Direction.Left, Direction.Forward, Direction.Backward, Direction.Left, Direction.Right, Direction.Left, Direction.Right,
            Direction.Backward, Direction.Forward, Direction.Left, Direction.Right
        };
        private const float DIRECTION_EPSILON = 1E-05f;
        private static readonly int[] ForwardBackward;
        private static readonly int[] LeftRight;
        private static readonly int[] UpDown;

        static Base6Directions()
        {
            int[] numArray1 = new int[3];
            numArray1[2] = 1;
            ForwardBackward = numArray1;
            int[] numArray2 = new int[3];
            numArray2[0] = 2;
            numArray2[2] = 3;
            LeftRight = numArray2;
            int[] numArray3 = new int[3];
            numArray3[0] = 5;
            numArray3[2] = 4;
            UpDown = numArray3;
        }

        private Base6Directions()
        {
        }

        public static Axis GetAxis(Direction direction) => 
            ((Axis) ((byte) (((byte) direction) >> 1)));

        public static Direction GetBaseAxisDirection(Axis axis) => 
            ((Direction) ((byte) (((byte) axis) << 1)));

        public static Direction GetClosestDirection(Vector3 vec) => 
            GetClosestDirection(ref vec);

        public static Direction GetClosestDirection(ref Vector3 vec) => 
            GetDirection(ref Vector3.Sign(Vector3.DominantAxisProjection(vec)));

        public static Direction GetCross(Direction dir1, Direction dir2) => 
            GetLeft(dir1, dir2);

        public static Direction GetDirection(Vector3 vec) => 
            GetDirection(ref vec);

        public static Direction GetDirection(ref Vector3 vec) => 
            ((Direction) ((byte) (((0 + ForwardBackward[(int) Math.Round((double) (vec.Z + 1f))]) + LeftRight[(int) Math.Round((double) (vec.X + 1f))]) + UpDown[(int) Math.Round((double) (vec.Y + 1f))])));

        public static Direction GetDirection(Vector3I vec) => 
            GetDirection(ref vec);

        public static Direction GetDirection(ref Vector3I vec) => 
            ((Direction) ((byte) (((0 + ForwardBackward[vec.Z + 1]) + LeftRight[vec.X + 1]) + UpDown[vec.Y + 1])));

        public static DirectionFlags GetDirectionFlag(Direction dir) => 
            ((DirectionFlags) ((byte) (1 << (dir & 0x1f))));

        public static Direction GetDirectionInAxis(Vector3 vec, Axis axis) => 
            GetDirectionInAxis(ref vec, axis);

        public static Direction GetDirectionInAxis(ref Vector3 vec, Axis axis)
        {
            Direction baseAxisDirection = GetBaseAxisDirection(axis);
            Vector3 vector = (Vector3) (IntDirections[(int) baseAxisDirection] * vec);
            return ((((vector.X + vector.Y) + vector.Z) < 1f) ? GetFlippedDirection(baseAxisDirection) : baseAxisDirection);
        }

        public static Direction GetFlippedDirection(Direction toFlip) => 
            (toFlip ^ Direction.Backward);

        public static Direction GetForward(Quaternion rot)
        {
            Vector3 vector;
            Vector3.Transform(ref Vector3.Forward, ref rot, out vector);
            return GetDirection(ref vector);
        }

        public static Direction GetForward(ref Matrix rotation)
        {
            Vector3 vector;
            Vector3.TransformNormal(ref Vector3.Forward, ref rotation, out vector);
            return GetDirection(ref vector);
        }

        public static Direction GetForward(ref Quaternion rot)
        {
            Vector3 vector;
            Vector3.Transform(ref Vector3.Forward, ref rot, out vector);
            return GetDirection(ref vector);
        }

        public static Vector3I GetIntVector(int direction)
        {
            direction = direction % 6;
            return IntDirections[direction];
        }

        public static Vector3I GetIntVector(Direction dir)
        {
            int index = (int) (dir % (Direction.Forward | Direction.Left | Direction.Up));
            return IntDirections[index];
        }

        public static Direction GetLeft(Direction up, Direction forward) => 
            LeftDirections[(int) ((forward * (Direction.Forward | Direction.Left | Direction.Up)) + up)];

        public static Direction GetOppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Backward:
                    return Direction.Forward;

                case Direction.Left:
                    return Direction.Right;

                case Direction.Right:
                    return Direction.Left;

                case Direction.Up:
                    return Direction.Down;

                case Direction.Down:
                    return Direction.Up;
            }
            return Direction.Backward;
        }

        public static Quaternion GetOrientation(Direction forward, Direction up) => 
            Quaternion.CreateFromForwardUp(GetVector(forward), GetVector(up));

        public static Direction GetPerpendicular(Direction dir) => 
            ((GetAxis(dir) != Axis.UpDown) ? Direction.Up : Direction.Right);

        public static Direction GetUp(Quaternion rot)
        {
            Vector3 vector;
            Vector3.Transform(ref Vector3.Up, ref rot, out vector);
            return GetDirection(ref vector);
        }

        public static Direction GetUp(ref Matrix rotation)
        {
            Vector3 vector;
            Vector3.TransformNormal(ref Vector3.Up, ref rotation, out vector);
            return GetDirection(ref vector);
        }

        public static Direction GetUp(ref Quaternion rot)
        {
            Vector3 vector;
            Vector3.Transform(ref Vector3.Up, ref rot, out vector);
            return GetDirection(ref vector);
        }

        public static Vector3 GetVector(int direction)
        {
            direction = direction % 6;
            return Directions[direction];
        }

        public static Vector3 GetVector(Direction dir) => 
            GetVector((int) dir);

        public static void GetVector(Direction dir, out Vector3 result)
        {
            int index = (int) (dir % (Direction.Forward | Direction.Left | Direction.Up));
            result = Directions[index];
        }

        public static bool IsBaseDirection(ref Vector3 vec) => 
            (((((vec.X * vec.X) + (vec.Y * vec.Y)) + (vec.Z * vec.Z)) - 1f) < 1E-05f);

        public static bool IsBaseDirection(Vector3 vec) => 
            IsBaseDirection(ref vec);

        public static bool IsBaseDirection(ref Vector3I vec) => 
            (((((vec.X * vec.X) + (vec.Y * vec.Y)) + (vec.Z * vec.Z)) - 1) == 0);

        public static bool IsValidBlockOrientation(Direction forward, Direction up) => 
            ((forward <= Direction.Down) && ((up <= Direction.Down) && (Vector3.Dot(GetVector(forward), GetVector(up)) == 0f)));

        public enum Axis : byte
        {
            ForwardBackward = 0,
            LeftRight = 1,
            UpDown = 2
        }

        public enum Direction : byte
        {
            Forward = 0,
            Backward = 1,
            Left = 2,
            Right = 3,
            Up = 4,
            Down = 5
        }

        [Flags]
        public enum DirectionFlags : byte
        {
            Forward = 1,
            Backward = 2,
            Left = 4,
            Right = 8,
            Up = 0x10,
            Down = 0x20,
            All = 0x3f
        }
    }
}

