namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI.Ingame.Utilities;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyWaypointInfo : IEquatable<MyWaypointInfo>
    {
        public static MyWaypointInfo Empty;
        public readonly string Name;
        public Vector3D Coords;
        private static bool IsPrecededByWhitespace(ref TextPtr ptr)
        {
            TextPtr ptr2 = ptr - 1;
            char c = ptr2.Char;
            return (ptr2.IsOutOfBounds() || (char.IsWhiteSpace(c) || !char.IsLetterOrDigit(c)));
        }

        public static void FindAll(string source, List<MyWaypointInfo> gpsList)
        {
            TextPtr ptr = new TextPtr(source);
            gpsList.Clear();
            while (!ptr.IsOutOfBounds())
            {
                MyWaypointInfo info;
                if (((char.ToUpperInvariant(ptr.Char) != 'G') || !IsPrecededByWhitespace(ref ptr)) || !TryParse(ref ptr, out info))
                {
                    ptr += 1;
                    continue;
                }
                gpsList.Add(info);
            }
        }

        public static bool TryParse(string text, out MyWaypointInfo gps)
        {
            if (text == null)
            {
                gps = Empty;
                return false;
            }
            TextPtr ptr = new TextPtr(text);
            bool flag = TryParse(ref ptr, out gps);
            if (!flag || ptr.IsOutOfBounds())
            {
                return flag;
            }
            gps = Empty;
            return false;
        }

        private static bool TryParse(ref TextPtr ptr, out MyWaypointInfo gps)
        {
            StringSegment segment;
            StringSegment segment2;
            StringSegment segment3;
            StringSegment segment4;
            double num;
            double num2;
            double num3;
            while (char.IsWhiteSpace(ptr.Char))
            {
                ptr += 1;
            }
            if (!ptr.StartsWithCaseInsensitive("gps:"))
            {
                gps = Empty;
                return false;
            }
            ptr += 4;
            if (!GrabSegment(ref ptr, out segment))
            {
                gps = Empty;
                return false;
            }
            if (!GrabSegment(ref ptr, out segment2))
            {
                gps = Empty;
                return false;
            }
            if (!GrabSegment(ref ptr, out segment3))
            {
                gps = Empty;
                return false;
            }
            if (!GrabSegment(ref ptr, out segment4))
            {
                gps = Empty;
                return false;
            }
            while (char.IsWhiteSpace(ptr.Char))
            {
                ptr += 1;
            }
            if (!double.TryParse(segment2.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out num))
            {
                gps = Empty;
                return false;
            }
            if (!double.TryParse(segment3.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
            {
                gps = Empty;
                return false;
            }
            if (!double.TryParse(segment4.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out num3))
            {
                gps = Empty;
                return false;
            }
            string name = segment.ToString();
            gps = new MyWaypointInfo(name, num, num2, num3);
            return true;
        }

        private static bool GrabSegment(ref TextPtr ptr, out StringSegment segment)
        {
            if (ptr.IsOutOfBounds())
            {
                segment = new StringSegment();
                return false;
            }
            TextPtr ptr2 = ptr;
            while (!ptr.IsOutOfBounds() && (ptr.Char != ':'))
            {
                ptr += 1;
            }
            if (ptr.Char != ':')
            {
                segment = new StringSegment();
                return false;
            }
            segment = new StringSegment(ptr2.Content, ptr2.Index, ptr.Index - ptr2.Index);
            ptr += 1;
            return true;
        }

        public MyWaypointInfo(string name, double x, double y, double z)
        {
            this.Name = name ?? "";
            this.Coords = new Vector3D(x, y, z);
        }

        public MyWaypointInfo(string name, Vector3D coords) : this(name, coords.X, coords.Y, coords.Z)
        {
        }

        public bool IsEmpty() => 
            ReferenceEquals(this.Name, null);

        public override string ToString()
        {
            object[] args = new object[] { this.Name, this.Coords.X, this.Coords.Y, this.Coords.Z };
            return string.Format(CultureInfo.InvariantCulture, "GPS:{0}:{1:R}:{2:R}:{3:R}:", args);
        }

        public bool Equals(MyWaypointInfo other) => 
            this.Equals(other, 0.0001);

        public bool Equals(MyWaypointInfo other, double epsilon) => 
            (string.Equals(this.Name, other.Name) && ((Math.Abs((double) (this.Coords.X - other.Coords.X)) < epsilon) && ((Math.Abs((double) (this.Coords.Y - other.Coords.Y)) < epsilon) && (Math.Abs((double) (this.Coords.Z - other.Coords.Z)) < epsilon))));

        public override bool Equals(object obj) => 
            ((obj != null) ? ((obj is MyWaypointInfo) && this.Equals((MyWaypointInfo) obj)) : false);

        public override int GetHashCode() => 
            ((((((((this.Name != null) ? this.Name.GetHashCode() : 0) * 0x18d) ^ this.Coords.X.GetHashCode()) * 0x18d) ^ this.Coords.Y.GetHashCode()) * 0x18d) ^ this.Coords.Z.GetHashCode());

        static MyWaypointInfo()
        {
        }
    }
}

