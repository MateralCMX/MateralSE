﻿namespace VRageMath
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Rectangle : IEquatable<Rectangle>
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Left =>
            this.X;
        public int Right =>
            (this.X + this.Width);
        public int Top =>
            this.Y;
        public int Bottom =>
            (this.Y + this.Height);
        public Point Location
        {
            get => 
                new Point(this.X, this.Y);
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        public Point Center =>
            new Point(this.X + (this.Width / 2), this.Y + (this.Height / 2));
        static Rectangle()
        {
        }

        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public static bool operator ==(Rectangle a, Rectangle b) => 
            ((a.X == b.X) && ((a.Y == b.Y) && ((a.Width == b.Width) && (a.Height == b.Height))));

        public static bool operator !=(Rectangle a, Rectangle b) => 
            ((a.X != b.X) || ((a.Y != b.Y) || ((a.Width != b.Width) || (a.Height != b.Height))));

        public void Offset(Point amount)
        {
            this.X += amount.X;
            this.Y += amount.Y;
        }

        public void Offset(int offsetX, int offsetY)
        {
            this.X += offsetX;
            this.Y += offsetY;
        }

        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            this.X -= horizontalAmount;
            this.Y -= verticalAmount;
            this.Width += horizontalAmount * 2;
            this.Height += verticalAmount * 2;
        }

        public bool Contains(int x, int y) => 
            ((this.X <= x) && ((x < (this.X + this.Width)) && ((this.Y <= y) && (y < (this.Y + this.Height)))));

        public bool Contains(Point value) => 
            ((this.X <= value.X) && ((value.X < (this.X + this.Width)) && ((this.Y <= value.Y) && (value.Y < (this.Y + this.Height)))));

        public void Contains(ref Point value, out bool result)
        {
            int num1;
            if (((this.X > value.X) || (value.X >= (this.X + this.Width))) || (this.Y > value.Y))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (value.Y < (this.Y + this.Height));
            }
            result = (bool) num1;
        }

        public bool Contains(Rectangle value) => 
            ((this.X <= value.X) && (((value.X + value.Width) <= (this.X + this.Width)) && ((this.Y <= value.Y) && ((value.Y + value.Height) <= (this.Y + this.Height)))));

        public void Contains(ref Rectangle value, out bool result)
        {
            int num1;
            if (((this.X > value.X) || ((value.X + value.Width) > (this.X + this.Width))) || (this.Y > value.Y))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) ((value.Y + value.Height) <= (this.Y + this.Height));
            }
            result = (bool) num1;
        }

        public bool Intersects(Rectangle value) => 
            ((value.X < (this.X + this.Width)) && ((this.X < (value.X + value.Width)) && ((value.Y < (this.Y + this.Height)) && (this.Y < (value.Y + value.Height)))));

        public void Intersects(ref Rectangle value, out bool result)
        {
            int num1;
            if (((value.X >= (this.X + this.Width)) || (this.X >= (value.X + value.Width))) || (value.Y >= (this.Y + this.Height)))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (this.Y < (value.Y + value.Height));
            }
            result = (bool) num1;
        }

        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            Rectangle rectangle;
            int num = value1.X + value1.Width;
            int num2 = value2.X + value2.Width;
            int num3 = value1.Y + value1.Height;
            int num4 = value2.Y + value2.Height;
            int num5 = (value1.X > value2.X) ? value1.X : value2.X;
            int num6 = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int num7 = (num < num2) ? num : num2;
            int num8 = (num3 < num4) ? num3 : num4;
            if ((num7 <= num5) || (num8 <= num6))
            {
                rectangle.X = 0;
                rectangle.Y = 0;
                rectangle.Width = 0;
                rectangle.Height = 0;
            }
            else
            {
                rectangle.X = num5;
                rectangle.Y = num6;
                rectangle.Width = num7 - num5;
                rectangle.Height = num8 - num6;
            }
            return rectangle;
        }

        public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            int num = value1.X + value1.Width;
            int num2 = value2.X + value2.Width;
            int num3 = value1.Y + value1.Height;
            int num4 = value2.Y + value2.Height;
            int num5 = (value1.X > value2.X) ? value1.X : value2.X;
            int num6 = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int num7 = (num < num2) ? num : num2;
            int num8 = (num3 < num4) ? num3 : num4;
            if ((num7 <= num5) || (num8 <= num6))
            {
                result.X = 0;
                result.Y = 0;
                result.Width = 0;
                result.Height = 0;
            }
            else
            {
                result.X = num5;
                result.Y = num6;
                result.Width = num7 - num5;
                result.Height = num8 - num6;
            }
        }

        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            Rectangle rectangle;
            int num = value1.X + value1.Width;
            int num2 = value2.X + value2.Width;
            int num3 = value1.Y + value1.Height;
            int num4 = value2.Y + value2.Height;
            int num5 = (value1.X < value2.X) ? value1.X : value2.X;
            int num6 = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            int num7 = (num > num2) ? num : num2;
            rectangle.X = num5;
            rectangle.Y = num6;
            rectangle.Width = num7 - num5;
            rectangle.Height = ((num3 > num4) ? num3 : num4) - num6;
            return rectangle;
        }

        public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            int num = value1.X + value1.Width;
            int num2 = value2.X + value2.Width;
            int num3 = value1.Y + value1.Height;
            int num4 = value2.Y + value2.Height;
            int num5 = (value1.X < value2.X) ? value1.X : value2.X;
            int num6 = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            int num7 = (num > num2) ? num : num2;
            int num8 = (num3 > num4) ? num3 : num4;
            result.X = num5;
            result.Y = num6;
            result.Width = num7 - num5;
            result.Height = num8 - num6;
        }

        public bool Equals(Rectangle other) => 
            ((this.X == other.X) && ((this.Y == other.Y) && ((this.Width == other.Width) && (this.Height == other.Height))));

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Rectangle)
            {
                flag = this.Equals((Rectangle) obj);
            }
            return flag;
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            object[] args = new object[] { this.X.ToString(currentCulture), this.Y.ToString(currentCulture), this.Width.ToString(currentCulture), this.Height.ToString(currentCulture) };
            return string.Format(currentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3}}}", args);
        }

        public override int GetHashCode() => 
            (((this.X.GetHashCode() + this.Y.GetHashCode()) + this.Width.GetHashCode()) + this.Height.GetHashCode());
    }
}

