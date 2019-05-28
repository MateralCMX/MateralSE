namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    public struct Thickness : IEquatable<Thickness>
    {
        private static Thickness zero;
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;
        public static Thickness Zero =>
            zero;
        public Thickness(float uniformLength)
        {
            float num;
            this.Bottom = num = uniformLength;
            this.Right = num = num;
            this.Left = this.Top = num;
        }

        public Thickness(float left, float top, float right, float bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public bool Equals(Thickness other) => 
            ((this.Left == other.Left) && ((this.Top == other.Top) && ((this.Right == other.Right) && (this.Bottom == other.Bottom))));

        public override bool Equals(object obj) => 
            ((obj is Thickness) && (this == ((Thickness) obj)));

        public override int GetHashCode() => 
            (((this.Left.GetHashCode() ^ this.Top.GetHashCode()) ^ this.Right.GetHashCode()) ^ this.Bottom.GetHashCode());

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            object[] args = new object[] { this.Left, this.Top, this.Right, this.Bottom };
            builder.AppendFormat("{0}, {1}, {2}, {3}", args);
            return builder.ToString();
        }

        public static bool operator ==(Thickness t1, Thickness t2)
        {
            if ((((t1.Left != t2.Left) && (!float.IsNaN(t1.Left) || !float.IsNaN(t2.Left))) || ((t1.Top != t2.Top) && (!float.IsNaN(t1.Top) || !float.IsNaN(t2.Top)))) || ((t1.Right != t2.Right) && (!float.IsNaN(t1.Right) || !float.IsNaN(t2.Right))))
            {
                return false;
            }
            return ((t1.Bottom == t2.Bottom) || (float.IsNaN(t1.Bottom) && float.IsNaN(t2.Bottom)));
        }

        public static bool operator !=(Thickness t1, Thickness t2) => 
            !(t1 == t2);

        public static Thickness operator +(Thickness t1, Thickness t2) => 
            new Thickness(t1.Left + t2.Left, t1.Top + t2.Top, t1.Right + t2.Right, t1.Bottom + t2.Bottom);

        public static Thickness operator -(Thickness t1, Thickness t2) => 
            new Thickness(t1.Left - t2.Left, t1.Top - t2.Top, t1.Right - t2.Right, t1.Bottom - t2.Bottom);

        static Thickness()
        {
            zero = new Thickness(0f);
        }
    }
}

