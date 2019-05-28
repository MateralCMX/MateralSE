namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    internal class MyGUIHelper
    {
        public static void Border(Vector2 normalizedPosition, Vector2 normalizedSize, int pixelWidth, Color color, bool top = true, bool bottom = true, bool left = true, bool right = true, Vector2? normalizedOffset = new Vector2?())
        {
            OffsetInnerBorder(normalizedPosition, normalizedSize, 2 * pixelWidth, pixelWidth, color, top, bottom, left, right, normalizedOffset);
        }

        public static bool Contains(Vector2 position, Vector2 size, float x, float y) => 
            ((x >= position.X) && ((y >= position.Y) && ((x <= (position.X + size.X)) && (y <= (position.Y + size.Y)))));

        public static void FillRectangle(Vector2 position, Vector2 size, Color color)
        {
            Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(position, false);
            Point point = new Point((int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y);
            Vector2 vector2 = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(position + size, false);
            Point point2 = new Point((int) vector2.X, (int) vector2.Y);
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", point.X, point.Y, point2.X - point.X, point2.Y - point.Y, color, true);
        }

        public static Vector2 GetOffset(Vector2 basePosition, Vector2 baseSize, Vector2 itemPosition, Vector2 itemSize)
        {
            float x = 0f;
            float y = 0f;
            if (baseSize.X > itemSize.X)
            {
                if ((basePosition.X + baseSize.X) < (itemPosition.X + itemSize.X))
                {
                    x = (basePosition.X + baseSize.X) - (itemPosition.X + itemSize.X);
                }
                if (basePosition.X > itemPosition.X)
                {
                    x = basePosition.X - itemPosition.X;
                }
            }
            if (baseSize.Y > itemSize.Y)
            {
                if ((basePosition.Y + baseSize.Y) < (itemPosition.Y + itemSize.Y))
                {
                    y = (basePosition.Y + baseSize.Y) - (itemPosition.Y + itemSize.Y);
                }
                if (basePosition.Y > itemPosition.Y)
                {
                    y = basePosition.Y - itemPosition.Y;
                }
            }
            return new Vector2(x, y);
        }

        public static void InsideBorder(Vector2 normalizedPosition, Vector2 normalizedSize, int pixelWidth, Color color, bool top = true, bool bottom = true, bool left = true, bool right = true)
        {
            Vector2? normalizedOffset = null;
            OffsetInnerBorder(normalizedPosition, normalizedSize, pixelWidth, 0, color, top, bottom, left, right, normalizedOffset);
        }

        public static bool Intersects(Vector2 aPosition, Vector2 aSize, Vector2 bPosition, Vector2 bSize)
        {
            if ((((aPosition.X > bPosition.X) && (aPosition.X > (bPosition.X + bSize.X))) || (((aPosition.X + aSize.X) < bPosition.X) && ((aPosition.X + aSize.X) < (bPosition.X + bSize.X)))) || ((aPosition.Y > bPosition.Y) && (aPosition.Y > (bPosition.Y + bSize.Y))))
            {
                return false;
            }
            return (((aPosition.Y + aSize.Y) >= bPosition.Y) || ((aPosition.Y + aSize.Y) >= (bPosition.Y + bSize.Y)));
        }

        private static void OffsetInnerBorder(Vector2 normalizedPosition, Vector2 normalizedSize, int pixelWidth, int offset, Color color, bool top = true, bool bottom = true, bool left = true, bool right = true, Vector2? normalizedOffset = new Vector2?())
        {
            Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(normalizedPosition - ((normalizedOffset != null) ? normalizedOffset.Value : Vector2.Zero), false);
            Point point = new Point(((int) screenCoordinateFromNormalizedCoordinate.X) - offset, ((int) screenCoordinateFromNormalizedCoordinate.Y) - offset);
            Vector2 vector2 = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate((normalizedPosition + normalizedSize) + ((normalizedOffset != null) ? normalizedOffset.Value : Vector2.Zero), false);
            Point point2 = new Point(((int) vector2.X) + offset, ((int) vector2.Y) + offset);
            if (top)
            {
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", point.X, point.Y, point2.X - point.X, pixelWidth, color, true);
            }
            if (bottom)
            {
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", point.X, point2.Y - pixelWidth, point2.X - point.X, pixelWidth, color, true);
            }
            if (left)
            {
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", point.X, point.Y + (top ? pixelWidth : 0), pixelWidth, ((point2.Y - point.Y) - (bottom ? pixelWidth : 0)) - (top ? pixelWidth : 0), color, true);
            }
            if (right)
            {
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", point2.X - pixelWidth, point.Y + (top ? pixelWidth : 0), pixelWidth, ((point2.Y - point.Y) - (bottom ? pixelWidth : 0)) - (top ? pixelWidth : 0), color, true);
            }
        }

        public static void OutsideBorder(Vector2 normalizedPosition, Vector2 normalizedSize, int pixelWidth, Color color, bool top = true, bool bottom = true, bool left = true, bool right = true)
        {
            Vector2? normalizedOffset = null;
            OffsetInnerBorder(normalizedPosition, normalizedSize, pixelWidth, pixelWidth, color, top, bottom, left, right, normalizedOffset);
        }
    }
}

