namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class SpriteScissorStack
    {
        private readonly Stack<Rectangle> m_rectangleStack = new Stack<Rectangle>();

        public unsafe void Cut(ref RectangleF destination, ref RectangleF source)
        {
            if (!this.Empty)
            {
                RectangleF other = destination;
                Rectangle rectangle = this.m_rectangleStack.Peek();
                RectangleF ef2 = new RectangleF((float) rectangle.X, (float) rectangle.Y, (float) rectangle.Width, (float) rectangle.Height);
                RectangleF.Intersect(ref destination, ref ef2, out destination);
                if (!destination.Equals(other))
                {
                    Vector2 vector = source.Size / other.Size;
                    Vector2 vector2 = destination.Size - other.Size;
                    Vector2 vector3 = destination.Position - other.Position;
                    Vector2* vectorPtr1 = (Vector2*) ref source.Position;
                    vectorPtr1[0] += vector3 * vector;
                    Vector2* vectorPtr2 = (Vector2*) ref source.Size;
                    vectorPtr2[0] += vector2 * vector;
                }
            }
        }

        public RectangleF? Peek()
        {
            if (!this.Empty)
            {
                Rectangle rectangle = this.m_rectangleStack.Peek();
                return new RectangleF((float) rectangle.X, (float) rectangle.Y, (float) rectangle.Width, (float) rectangle.Height);
            }
            return null;
        }

        public void Pop()
        {
            if (!this.Empty)
            {
                this.m_rectangleStack.Pop();
            }
        }

        public void Push(Rectangle scissorRect)
        {
            if (!this.Empty)
            {
                Rectangle rectangle = this.m_rectangleStack.Peek();
                Rectangle.Intersect(ref scissorRect, ref rectangle, out scissorRect);
            }
            this.m_rectangleStack.Push(scissorRect);
        }

        public bool Empty =>
            (this.m_rectangleStack.Count == 0);
    }
}

