namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRageMath;

    internal class MyRichLabelLine
    {
        private readonly float m_minLineHeight;
        private List<MyRichLabelPart> m_parts;
        private Vector2 m_size;

        public MyRichLabelLine(float minLineHeight)
        {
            this.m_minLineHeight = minLineHeight;
            this.m_parts = new List<MyRichLabelPart>(8);
            this.RecalculateSize();
        }

        public void AddPart(MyRichLabelPart part)
        {
            this.m_parts.Add(part);
            this.RecalculateSize();
        }

        public void ClearParts()
        {
            this.m_parts.Clear();
            this.RecalculateSize();
        }

        public unsafe bool Draw(Vector2 position, float alphamask, ref int charactersLeft)
        {
            Vector2 vector = position;
            float num = position.Y + (this.m_size.Y / 2f);
            for (int i = 0; i < this.m_parts.Count; i++)
            {
                MyRichLabelPart part = this.m_parts[i];
                Vector2 size = part.Size;
                vector.Y = num - (size.Y / 2f);
                if (((vector.Y + this.m_size.Y) >= 0f) && (vector.Y <= 1f))
                {
                    if (!part.Draw(vector, alphamask, ref charactersLeft))
                    {
                        return false;
                    }
                    float* singlePtr1 = (float*) ref vector.X;
                    singlePtr1[0] += size.X;
                    if (charactersLeft <= 0)
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        public IEnumerable<MyRichLabelPart> GetParts() => 
            this.m_parts;

        public unsafe bool HandleInput(Vector2 position)
        {
            for (int i = 0; i < this.m_parts.Count; i++)
            {
                MyRichLabelPart part = this.m_parts[i];
                if (part.HandleInput(position))
                {
                    return true;
                }
                float* singlePtr1 = (float*) ref position.X;
                singlePtr1[0] += part.Size.X;
            }
            return false;
        }

        public bool IsEmpty() => 
            (this.m_parts.Count == 0);

        private unsafe void RecalculateSize()
        {
            Vector2 vector = new Vector2(0f, this.m_minLineHeight);
            for (int i = 0; i < this.m_parts.Count; i++)
            {
                Vector2 size = this.m_parts[i].Size;
                Vector2* vectorPtr1 = (Vector2*) ref vector;
                vectorPtr1->Y = Math.Max(size.Y, vector.Y);
                float* singlePtr1 = (float*) ref vector.X;
                singlePtr1[0] += size.X;
            }
            this.m_size = vector;
        }

        public Vector2 Size =>
            this.m_size;

        public string DebugText
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < this.m_parts.Count; i++)
                {
                    this.m_parts[i].AppendTextTo(builder);
                }
                return builder.ToString();
            }
        }
    }
}

