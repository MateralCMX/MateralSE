namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    public class MyHudNameValueData
    {
        private readonly List<Data> m_items;
        private int m_count;
        public string DefaultNameFont;
        public string DefaultValueFont;
        public float LineSpacing;
        public bool ShowBackgroundFog;

        public MyHudNameValueData(int itemCount, string defaultNameFont = "Blue", string defaultValueFont = "White", float lineSpacing = 0.025f, bool showBackgroundFog = false)
        {
            this.DefaultNameFont = defaultNameFont;
            this.DefaultValueFont = defaultValueFont;
            this.LineSpacing = lineSpacing;
            this.m_count = itemCount;
            this.m_items = new List<Data>(itemCount);
            this.ShowBackgroundFog = showBackgroundFog;
            this.EnsureItemsExist();
        }

        internal float ComputeMaxLineWidth(float textScale)
        {
            float num = 0f;
            for (int i = 0; i < this.Count; i++)
            {
                Data data = this.m_items[i];
                Vector2 vector2 = MyGuiManager.MeasureString(data.ValueFont ?? this.DefaultValueFont, data.Value, textScale);
                num = Math.Max(num, MyGuiManager.MeasureString(data.NameFont ?? this.DefaultNameFont, data.Name, textScale).X + vector2.X);
            }
            return num;
        }

        private unsafe void DrawBackgroundFog(Vector2 namesTopLeft, Vector2 valuesTopRight, bool topDown)
        {
            float lineSpacing;
            int num2;
            int count;
            int num4;
            if (topDown)
            {
                lineSpacing = this.LineSpacing;
                num2 = 0;
                count = this.Count;
                num4 = 1;
            }
            else
            {
                lineSpacing = -this.LineSpacing;
                num2 = this.Count - 1;
                count = -1;
                num4 = -1;
            }
            for (int i = num2; i != count; i += num4)
            {
                if (this.m_items[i].Visible)
                {
                    Vector2 position = new Vector2((namesTopLeft.X + valuesTopRight.X) * 0.5f, namesTopLeft.Y + (0.5f * lineSpacing));
                    Vector2 textSize = new Vector2(Math.Abs((float) (namesTopLeft.X - valuesTopRight.X)), this.LineSpacing);
                    MyGuiTextShadows.DrawShadow(ref position, ref textSize, null, 1f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    float* singlePtr1 = (float*) ref namesTopLeft.Y;
                    singlePtr1[0] += lineSpacing;
                    float* singlePtr2 = (float*) ref valuesTopRight.Y;
                    singlePtr2[0] += lineSpacing;
                }
            }
        }

        public unsafe void DrawBottomUp(Vector2 namesBottomLeft, Vector2 valuesBottomRight, float textScale)
        {
            Color white = Color.White;
            if (this.ShowBackgroundFog)
            {
                this.DrawBackgroundFog(namesBottomLeft, valuesBottomRight, false);
            }
            for (int i = this.Count - 1; i >= 0; i--)
            {
                Data data = this.m_items[i];
                if (data.Visible)
                {
                    MyGuiManager.DrawString(data.NameFont ?? this.DefaultNameFont, data.Name, namesBottomLeft, textScale, new Color?(white), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
                    MyGuiManager.DrawString(data.ValueFont ?? this.DefaultValueFont, data.Value, valuesBottomRight, textScale, new Color?(white), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
                    float* singlePtr1 = (float*) ref namesBottomLeft.Y;
                    singlePtr1[0] -= this.LineSpacing;
                    float* singlePtr2 = (float*) ref valuesBottomRight.Y;
                    singlePtr2[0] -= this.LineSpacing;
                }
            }
        }

        public unsafe void DrawTopDown(Vector2 namesTopLeft, Vector2 valuesTopRight, float textScale)
        {
            Color white = Color.White;
            if (this.ShowBackgroundFog)
            {
                this.DrawBackgroundFog(namesTopLeft, valuesTopRight, true);
            }
            for (int i = 0; i < this.Count; i++)
            {
                Data data = this.m_items[i];
                if (data.Visible)
                {
                    MyGuiManager.DrawString(data.NameFont ?? this.DefaultNameFont, data.Name, namesTopLeft, textScale, new Color?(white), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                    MyGuiManager.DrawString(data.ValueFont ?? this.DefaultValueFont, data.Value, valuesTopRight, textScale, new Color?(white), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                    float* singlePtr1 = (float*) ref namesTopLeft.Y;
                    singlePtr1[0] += this.LineSpacing;
                    float* singlePtr2 = (float*) ref valuesTopRight.Y;
                    singlePtr2[0] += this.LineSpacing;
                }
            }
        }

        private void EnsureItemsExist()
        {
            this.m_items.Capacity = Math.Max(this.Count, this.m_items.Capacity);
            while (this.m_items.Count < this.Count)
            {
                this.m_items.Add(new Data());
            }
        }

        public float GetGuiHeight() => 
            ((this.GetVisibleCount() + 1) * this.LineSpacing);

        public int GetVisibleCount()
        {
            int num = 0;
            for (int i = 0; i < this.m_count; i++)
            {
                if (this.m_items[i].Visible)
                {
                    num++;
                }
            }
            return num;
        }

        public int Count
        {
            get => 
                this.m_count;
            set
            {
                this.m_count = value;
                this.EnsureItemsExist();
            }
        }

        public Data this[int i] =>
            this.m_items[i];

        public class Data
        {
            public StringBuilder Name = new StringBuilder();
            public StringBuilder Value = new StringBuilder();
            public string NameFont;
            public string ValueFont;
            public bool Visible = true;
        }
    }
}

