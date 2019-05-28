namespace VRageMath
{
    using System;

    public class Base27Directions
    {
        public static readonly Vector3[] Directions;
        public static readonly Vector3I[] DirectionsInt;
        private const float DIRECTION_EPSILON = 1E-05f;
        private static readonly int[] ForwardBackward;
        private static readonly int[] LeftRight;
        private static readonly int[] UpDown;

        static Base27Directions()
        {
            Vector3[] vectorArray1 = new Vector3[0x40];
            vectorArray1[0] = new Vector3(0f, 0f, 0f);
            vectorArray1[1] = new Vector3(0f, 0f, -1f);
            vectorArray1[2] = new Vector3(0f, 0f, 1f);
            vectorArray1[3] = new Vector3(0f, 0f, 0f);
            vectorArray1[4] = new Vector3(-1f, 0f, 0f);
            vectorArray1[5] = new Vector3(-0.7071068f, 0f, -0.7071068f);
            vectorArray1[6] = new Vector3(-0.7071068f, 0f, 0.7071068f);
            vectorArray1[7] = new Vector3(-1f, 0f, 0f);
            vectorArray1[8] = new Vector3(1f, 0f, 0f);
            vectorArray1[9] = new Vector3(0.7071068f, 0f, -0.7071068f);
            vectorArray1[10] = new Vector3(0.7071068f, 0f, 0.7071068f);
            vectorArray1[11] = new Vector3(1f, 0f, 0f);
            vectorArray1[12] = new Vector3(0f, 0f, 0f);
            vectorArray1[13] = new Vector3(0f, 0f, -1f);
            vectorArray1[14] = new Vector3(0f, 0f, 1f);
            vectorArray1[15] = new Vector3(0f, 0f, 0f);
            vectorArray1[0x10] = new Vector3(0f, 1f, 0f);
            vectorArray1[0x11] = new Vector3(0f, 0.7071068f, -0.7071068f);
            vectorArray1[0x12] = new Vector3(0f, 0.7071068f, 0.7071068f);
            vectorArray1[0x13] = new Vector3(0f, 1f, 0f);
            vectorArray1[20] = new Vector3(-0.7071068f, 0.7071068f, 0f);
            vectorArray1[0x15] = new Vector3(-0.5773503f, 0.5773503f, -0.5773503f);
            vectorArray1[0x16] = new Vector3(-0.5773503f, 0.5773503f, 0.5773503f);
            vectorArray1[0x17] = new Vector3(-0.7071068f, 0.7071068f, 0f);
            vectorArray1[0x18] = new Vector3(0.7071068f, 0.7071068f, 0f);
            vectorArray1[0x19] = new Vector3(0.5773503f, 0.5773503f, -0.5773503f);
            vectorArray1[0x1a] = new Vector3(0.5773503f, 0.5773503f, 0.5773503f);
            vectorArray1[0x1b] = new Vector3(0.7071068f, 0.7071068f, 0f);
            vectorArray1[0x1c] = new Vector3(0f, 1f, 0f);
            vectorArray1[0x1d] = new Vector3(0f, 0.7071068f, -0.7071068f);
            vectorArray1[30] = new Vector3(0f, 0.7071068f, 0.7071068f);
            vectorArray1[0x1f] = new Vector3(0f, 1f, 0f);
            vectorArray1[0x20] = new Vector3(0f, -1f, 0f);
            vectorArray1[0x21] = new Vector3(0f, -0.7071068f, -0.7071068f);
            vectorArray1[0x22] = new Vector3(0f, -0.7071068f, 0.7071068f);
            vectorArray1[0x23] = new Vector3(0f, -1f, 0f);
            vectorArray1[0x24] = new Vector3(-0.7071068f, -0.7071068f, 0f);
            vectorArray1[0x25] = new Vector3(-0.5773503f, -0.5773503f, -0.5773503f);
            vectorArray1[0x26] = new Vector3(-0.5773503f, -0.5773503f, 0.5773503f);
            vectorArray1[0x27] = new Vector3(-0.7071068f, -0.7071068f, 0f);
            vectorArray1[40] = new Vector3(0.7071068f, -0.7071068f, 0f);
            vectorArray1[0x29] = new Vector3(0.5773503f, -0.5773503f, -0.5773503f);
            vectorArray1[0x2a] = new Vector3(0.5773503f, -0.5773503f, 0.5773503f);
            vectorArray1[0x2b] = new Vector3(0.7071068f, -0.7071068f, 0f);
            vectorArray1[0x2c] = new Vector3(0f, -1f, 0f);
            vectorArray1[0x2d] = new Vector3(0f, -0.7071068f, -0.7071068f);
            vectorArray1[0x2e] = new Vector3(0f, -0.7071068f, 0.7071068f);
            vectorArray1[0x2f] = new Vector3(0f, -1f, 0f);
            vectorArray1[0x30] = new Vector3(0f, 0f, 0f);
            vectorArray1[0x31] = new Vector3(0f, 0f, -1f);
            vectorArray1[50] = new Vector3(0f, 0f, 1f);
            vectorArray1[0x33] = new Vector3(0f, 0f, 0f);
            vectorArray1[0x34] = new Vector3(-1f, 0f, 0f);
            vectorArray1[0x35] = new Vector3(-0.7071068f, 0f, -0.7071068f);
            vectorArray1[0x36] = new Vector3(-0.7071068f, 0f, 0.7071068f);
            vectorArray1[0x37] = new Vector3(-1f, 0f, 0f);
            vectorArray1[0x38] = new Vector3(1f, 0f, 0f);
            vectorArray1[0x39] = new Vector3(0.7071068f, 0f, -0.7071068f);
            vectorArray1[0x3a] = new Vector3(0.7071068f, 0f, 0.7071068f);
            vectorArray1[0x3b] = new Vector3(1f, 0f, 0f);
            vectorArray1[60] = new Vector3(0f, 0f, 0f);
            vectorArray1[0x3d] = new Vector3(0f, 0f, -1f);
            vectorArray1[0x3e] = new Vector3(0f, 0f, 1f);
            vectorArray1[0x3f] = new Vector3(0f, 0f, 0f);
            Directions = vectorArray1;
            Vector3I[] vectoriArray1 = new Vector3I[0x40];
            vectoriArray1[0] = new Vector3I(0, 0, 0);
            vectoriArray1[1] = new Vector3I(0, 0, -1);
            vectoriArray1[2] = new Vector3I(0, 0, 1);
            vectoriArray1[3] = new Vector3I(0, 0, 0);
            vectoriArray1[4] = new Vector3I(-1, 0, 0);
            vectoriArray1[5] = new Vector3I(-1, 0, -1);
            vectoriArray1[6] = new Vector3I(-1, 0, 1);
            vectoriArray1[7] = new Vector3I(-1, 0, 0);
            vectoriArray1[8] = new Vector3I(1, 0, 0);
            vectoriArray1[9] = new Vector3I(1, 0, -1);
            vectoriArray1[10] = new Vector3I(1, 0, 1);
            vectoriArray1[11] = new Vector3I(1, 0, 0);
            vectoriArray1[12] = new Vector3I(0, 0, 0);
            vectoriArray1[13] = new Vector3I(0, 0, -1);
            vectoriArray1[14] = new Vector3I(0, 0, 1);
            vectoriArray1[15] = new Vector3I(0, 0, 0);
            vectoriArray1[0x10] = new Vector3I(0, 1, 0);
            vectoriArray1[0x11] = new Vector3I(0, 1, -1);
            vectoriArray1[0x12] = new Vector3I(0, 1, 1);
            vectoriArray1[0x13] = new Vector3I(0, 1, 0);
            vectoriArray1[20] = new Vector3I(-1, 1, 0);
            vectoriArray1[0x15] = new Vector3I(-1, 1, -1);
            vectoriArray1[0x16] = new Vector3I(-1, 1, 1);
            vectoriArray1[0x17] = new Vector3I(-1, 1, 0);
            vectoriArray1[0x18] = new Vector3I(1, 1, 0);
            vectoriArray1[0x19] = new Vector3I(1, 1, -1);
            vectoriArray1[0x1a] = new Vector3I(1, 1, 1);
            vectoriArray1[0x1b] = new Vector3I(1, 1, 0);
            vectoriArray1[0x1c] = new Vector3I(0, 1, 0);
            vectoriArray1[0x1d] = new Vector3I(0, 1, -1);
            vectoriArray1[30] = new Vector3I(0, 1, 1);
            vectoriArray1[0x1f] = new Vector3I(0, 1, 0);
            vectoriArray1[0x20] = new Vector3I(0, -1, 0);
            vectoriArray1[0x21] = new Vector3I(0, -1, -1);
            vectoriArray1[0x22] = new Vector3I(0, -1, 1);
            vectoriArray1[0x23] = new Vector3I(0, -1, 0);
            vectoriArray1[0x24] = new Vector3I(-1, -1, 0);
            vectoriArray1[0x25] = new Vector3I(-1, -1, -1);
            vectoriArray1[0x26] = new Vector3I(-1, -1, 1);
            vectoriArray1[0x27] = new Vector3I(-1, -1, 0);
            vectoriArray1[40] = new Vector3I(1, -1, 0);
            vectoriArray1[0x29] = new Vector3I(1, -1, -1);
            vectoriArray1[0x2a] = new Vector3I(1, -1, 1);
            vectoriArray1[0x2b] = new Vector3I(1, -1, 0);
            vectoriArray1[0x2c] = new Vector3I(0, -1, 0);
            vectoriArray1[0x2d] = new Vector3I(0, -1, -1);
            vectoriArray1[0x2e] = new Vector3I(0, -1, 1);
            vectoriArray1[0x2f] = new Vector3I(0, -1, 0);
            vectoriArray1[0x30] = new Vector3I(0, 0, 0);
            vectoriArray1[0x31] = new Vector3I(0, 0, -1);
            vectoriArray1[50] = new Vector3I(0, 0, 1);
            vectoriArray1[0x33] = new Vector3I(0, 0, 0);
            vectoriArray1[0x34] = new Vector3I(-1, 0, 0);
            vectoriArray1[0x35] = new Vector3I(-1, 0, -1);
            vectoriArray1[0x36] = new Vector3I(-1, 0, 1);
            vectoriArray1[0x37] = new Vector3I(-1, 0, 0);
            vectoriArray1[0x38] = new Vector3I(1, 0, 0);
            vectoriArray1[0x39] = new Vector3I(1, 0, -1);
            vectoriArray1[0x3a] = new Vector3I(1, 0, 1);
            vectoriArray1[0x3b] = new Vector3I(1, 0, 0);
            vectoriArray1[60] = new Vector3I(0, 0, 0);
            vectoriArray1[0x3d] = new Vector3I(0, 0, -1);
            vectoriArray1[0x3e] = new Vector3I(0, 0, 1);
            vectoriArray1[0x3f] = new Vector3I(0, 0, 0);
            DirectionsInt = vectoriArray1;
            int[] numArray1 = new int[3];
            numArray1[0] = 1;
            numArray1[2] = 2;
            ForwardBackward = numArray1;
            int[] numArray2 = new int[3];
            numArray2[0] = 4;
            numArray2[2] = 8;
            LeftRight = numArray2;
            int[] numArray3 = new int[3];
            numArray3[0] = 0x20;
            numArray3[2] = 0x10;
            UpDown = numArray3;
        }

        public static Direction GetDirection(Vector3 vec) => 
            GetDirection(ref vec);

        public static Direction GetDirection(Vector3I vec) => 
            GetDirection(ref vec);

        public static Direction GetDirection(ref Vector3 vec) => 
            ((Direction) ((byte) (((0 + ForwardBackward[(int) Math.Round((double) (vec.Z + 1f))]) + LeftRight[(int) Math.Round((double) (vec.X + 1f))]) + UpDown[(int) Math.Round((double) (vec.Y + 1f))])));

        public static Direction GetDirection(ref Vector3I vec) => 
            ((Direction) ((byte) (((0 + ForwardBackward[vec.Z + 1]) + LeftRight[vec.X + 1]) + UpDown[vec.Y + 1])));

        public static Direction GetForward(ref Quaternion rot)
        {
            Vector3 vector;
            Vector3.Transform(ref Vector3.Forward, ref rot, out vector);
            return GetDirection(ref vector);
        }

        public static Direction GetUp(ref Quaternion rot)
        {
            Vector3 vector;
            Vector3.Transform(ref Vector3.Up, ref rot, out vector);
            return GetDirection(ref vector);
        }

        public static Vector3 GetVector(int direction) => 
            Directions[direction];

        public static Vector3 GetVector(Direction dir) => 
            Directions[(int) dir];

        public static Vector3I GetVectorInt(int direction) => 
            DirectionsInt[direction];

        public static Vector3I GetVectorInt(Direction dir) => 
            DirectionsInt[(int) dir];

        public static bool IsBaseDirection(ref Vector3 vec) => 
            (((((vec.X * vec.X) + (vec.Y * vec.Y)) + (vec.Z * vec.Z)) - 1f) < 1E-05f);

        public static bool IsBaseDirection(ref Vector3I vec) => 
            ((vec.X >= -1) && ((vec.X <= 1) && ((vec.Y >= -1) && ((vec.Y <= 1) && ((vec.Z >= -1) && (vec.Z <= 1))))));

        public static bool IsBaseDirection(Vector3 vec) => 
            IsBaseDirection(ref vec);

        [Flags]
        public enum Direction : byte
        {
            Forward = 1,
            Backward = 2,
            Left = 4,
            Right = 8,
            Up = 0x10,
            Down = 0x20
        }
    }
}

