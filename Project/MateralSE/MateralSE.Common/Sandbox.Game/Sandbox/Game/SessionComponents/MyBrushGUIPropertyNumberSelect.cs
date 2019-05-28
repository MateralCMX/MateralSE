namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyBrushGUIPropertyNumberSelect : IMyVoxelBrushGUIProperty
    {
        private MyGuiControlButton m_lowerValue;
        private MyGuiControlButton m_upperValue;
        private MyGuiControlLabel m_label;
        private MyGuiControlLabel m_labelValue;
        public Action ValueIncreased;
        public Action ValueDecreased;
        public float Value;
        public float ValueMin;
        public float ValueMax;
        public float ValueStep;

        public MyBrushGUIPropertyNumberSelect(float value, float valueMin, float valueMax, float valueStep, MyVoxelBrushGUIPropertyOrder order, MyStringId labelText)
        {
            Vector2 vector = new Vector2(-0.1f, -0.15f);
            Vector2 vector2 = new Vector2(0.035f, -0.15f);
            Vector2 vector3 = new Vector2(0f, -0.1475f);
            Vector2 vector4 = new Vector2(0.08f, -0.1475f);
            if (order == MyVoxelBrushGUIPropertyOrder.Second)
            {
                vector.Y = -0.07f;
                vector2.Y = -0.07f;
                vector3.Y = -0.0675f;
                vector4.Y = -0.0675f;
            }
            else if (order == MyVoxelBrushGUIPropertyOrder.Third)
            {
                vector.Y = 0.01f;
                vector2.Y = 0.01f;
                vector3.Y = 0.0125f;
                vector4.Y = 0.0125f;
            }
            this.Value = value;
            this.ValueMin = valueMin;
            this.ValueMax = valueMax;
            this.ValueStep = valueStep;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = vector;
            label1.TextEnum = labelText;
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_label = label1;
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = vector3;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.ArrowLeft;
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_lowerValue = button1;
            MyGuiControlButton button2 = new MyGuiControlButton();
            button2.Position = vector4;
            button2.VisualStyle = MyGuiControlButtonStyleEnum.ArrowRight;
            button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_upperValue = button2;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = vector2;
            label2.Text = this.Value.ToString();
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelValue = label2;
            this.m_lowerValue.ButtonClicked += new Action<MyGuiControlButton>(this.LowerClicked);
            this.m_upperValue.ButtonClicked += new Action<MyGuiControlButton>(this.UpperClicked);
        }

        public void AddControlsToList(List<MyGuiControlBase> list)
        {
            list.Add(this.m_lowerValue);
            list.Add(this.m_upperValue);
            list.Add(this.m_label);
            list.Add(this.m_labelValue);
        }

        private void LowerClicked(MyGuiControlButton sender)
        {
            this.Value = MathHelper.Clamp(this.Value - this.ValueStep, this.ValueMin, this.ValueMax);
            this.m_labelValue.Text = this.Value.ToString();
            if (this.ValueDecreased != null)
            {
                this.ValueDecreased();
            }
        }

        private void UpperClicked(MyGuiControlButton sender)
        {
            this.Value = MathHelper.Clamp(this.Value + this.ValueStep, this.ValueMin, this.ValueMax);
            this.m_labelValue.Text = this.Value.ToString();
            if (this.ValueIncreased != null)
            {
                this.ValueIncreased();
            }
        }
    }
}

