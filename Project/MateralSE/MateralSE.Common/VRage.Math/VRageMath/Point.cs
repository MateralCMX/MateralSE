namespace VRageMath
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point>
    {
        private static Point _zero;
        public int X;
        public int Y;
        public static Point Zero =>
            _zero;
        static Point()
        {
        }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(Point a, Point b) => 
            a.Equals(b);

        public static bool operator !=(Point a, Point b) => 
            ((a.X != b.X) || (a.Y != b.Y));

        public bool Equals(Point other) => 
            ((this.X == other.X) && (this.Y == other.Y));

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Point)
            {
                flag = this.Equals((Point) obj);
            }
            return flag;
        }

        public override int GetHashCode() => 
            (this.X.GetHashCode() + this.Y.GetHashCode());

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            object[] args = new object[] { this.X.ToString(currentCulture), this.Y.ToString(currentCulture) };
            return string.Format(currentCulture, "{{X:{0} Y:{1}}}", args);
        }
    }
}

