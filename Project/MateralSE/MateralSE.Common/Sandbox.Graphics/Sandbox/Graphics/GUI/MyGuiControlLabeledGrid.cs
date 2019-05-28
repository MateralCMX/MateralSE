namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlLabeledGrid : MyGuiControlGrid
    {
        public List<string> Labels = new List<string>();
        private StringBuilder textBuilder = new StringBuilder();
        public float TextScale = 1f;

        public MyGuiControlLabeledGrid()
        {
            base.m_styleDef.FitSizeToItems = false;
        }

        public void AddLabeledItem(MyGuiGridItem gridItem, string label)
        {
            base.Add(gridItem, 0);
            this.Labels.Add(label);
        }

        public override void Clear()
        {
            base.Clear();
            this.Labels.Clear();
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            this.DrawLabels(transitionAlpha);
        }

        private unsafe void DrawLabels(float transitionAlpha)
        {
            MyGuiBorderThickness itemPadding = base.m_styleDef.ItemPadding;
            string itemFontNormal = base.m_styleDef.ItemFontNormal;
            int row = 0;
            while (row < base.RowsCount)
            {
                int col = 0;
                while (true)
                {
                    if (col >= base.ColumnsCount)
                    {
                        row++;
                        break;
                    }
                    int itemIdx = base.ComputeIndex(row, col);
                    MyGuiGridItem item = base.TryGetItemAt(itemIdx);
                    if ((item != null) && this.Labels.IsValidIndex<string>(itemIdx))
                    {
                        string str2 = this.Labels[itemIdx];
                        this.textBuilder.Clear();
                        this.textBuilder.Append(str2);
                        Vector2 normalizedCoord = base.m_itemsRectangle.Position + (base.m_itemStep * new Vector2((float) col, (float) row));
                        float* singlePtr1 = (float*) ref normalizedCoord.X;
                        singlePtr1[0] += base.m_itemStep.X + itemPadding.MarginStep.X;
                        float* singlePtr2 = (float*) ref normalizedCoord.Y;
                        singlePtr2[0] += base.m_itemStep.Y * 0.5f;
                        bool local1 = base.Enabled && item.Enabled;
                        float maxTextWidth = Math.Abs((float) (base.Size.X - normalizedCoord.X));
                        MyGuiManager.DrawString(itemFontNormal, this.textBuilder, normalizedCoord, this.TextScale, new Color?(ApplyColorMaskModifiers(item.IconColorMask, base.Enabled, transitionAlpha)), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, maxTextWidth);
                    }
                    col++;
                }
            }
        }
    }
}

