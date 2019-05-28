namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRageMath;

    public class MyBrushGUIPropertyNumberSlider : IMyVoxelBrushGUIProperty
    {
        private MyGuiControlLabel m_label;
        private MyGuiControlLabel m_labelValue;
        private MyGuiControlSlider m_sliderValue;
        public Action ValueChanged;
        public float Value;
        public float ValueMin;
        public float ValueMax;
        public float ValueStep;

        public MyBrushGUIPropertyNumberSlider(float value, float valueMin, float valueMax, float valueStep, MyVoxelBrushGUIPropertyOrder order, MyStringId labelText)
        {
            Vector2 vector = new Vector2(-0.1f, -0.15f);
            Vector2 vector2 = new Vector2(0.16f, -0.15f);
            Vector2 vector3 = new Vector2(-0.1f, -0.123f);
            if (order == MyVoxelBrushGUIPropertyOrder.Second)
            {
                vector.Y = -0.066f;
                vector2.Y = -0.066f;
                vector3.Y = -0.039f;
            }
            else if (order == MyVoxelBrushGUIPropertyOrder.Third)
            {
                vector.Y = 0.018f;
                vector2.Y = 0.018f;
                vector3.Y = 0.045f;
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
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = vector2;
            label2.Text = this.Value.ToString();
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_labelValue = label2;
            Vector2? position = null;
            float? defaultValue = null;
            Vector4? color = null;
            MyGuiControlSlider slider1 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, defaultValue, color, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            slider1.Position = vector3;
            slider1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_sliderValue = slider1;
            this.m_sliderValue.Size = new Vector2(0.263f, 0.1f);
            this.m_sliderValue.MaxValue = this.ValueMax;
            this.m_sliderValue.Value = this.Value;
            this.m_sliderValue.MinValue = this.ValueMin;
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.Slider_ValueChanged));
        }

        public void AddControlsToList(List<MyGuiControlBase> list)
        {
            list.Add(this.m_label);
            list.Add(this.m_labelValue);
            list.Add(this.m_sliderValue);
        }

        private void Slider_ValueChanged(MyGuiControlSlider sender)
        {
            float num = 1f / this.ValueStep;
            float num2 = this.m_sliderValue.Value * num;
            this.Value = MathHelper.Clamp(((float) ((int) num2)) / num, this.ValueMin, this.ValueMax);
            this.m_labelValue.Text = this.Value.ToString();
            if (this.ValueChanged != null)
            {
                this.ValueChanged();
            }
        }
    }
}

