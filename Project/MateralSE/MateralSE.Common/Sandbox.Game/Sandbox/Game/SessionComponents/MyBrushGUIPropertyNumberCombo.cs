namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRageMath;

    public class MyBrushGUIPropertyNumberCombo : IMyVoxelBrushGUIProperty
    {
        private MyGuiControlLabel m_label;
        private MyGuiControlCombobox m_combo;
        public Action ItemSelected;
        public long SelectedKey;

        public MyBrushGUIPropertyNumberCombo(MyVoxelBrushGUIPropertyOrder order, MyStringId labelText)
        {
            Vector2 vector = new Vector2(-0.1f, -0.15f);
            Vector2 vector2 = new Vector2(-0.1f, -0.12f);
            if (order == MyVoxelBrushGUIPropertyOrder.Second)
            {
                vector.Y = -0.07f;
                vector2.Y = -0.04f;
            }
            else if (order == MyVoxelBrushGUIPropertyOrder.Third)
            {
                vector.Y = 0.01f;
                vector2.Y = 0.04f;
            }
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = vector;
            label1.TextEnum = labelText;
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_label = label1;
            this.m_combo = new MyGuiControlCombobox();
            this.m_combo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_combo.Position = vector2;
            this.m_combo.Size = new Vector2(0.263f, 0.1f);
            this.m_combo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.Combo_ItemSelected);
        }

        public void AddControlsToList(List<MyGuiControlBase> list)
        {
            list.Add(this.m_label);
            list.Add(this.m_combo);
        }

        public void AddItem(long key, MyStringId text)
        {
            int? sortOrder = null;
            MyStringId? toolTip = null;
            this.m_combo.AddItem(key, text, sortOrder, toolTip);
        }

        private void Combo_ItemSelected()
        {
            this.SelectedKey = this.m_combo.GetSelectedKey();
            if (this.ItemSelected != null)
            {
                this.ItemSelected();
            }
        }

        public void SelectItem(long key)
        {
            this.m_combo.SelectItemByKey(key, true);
        }
    }
}

