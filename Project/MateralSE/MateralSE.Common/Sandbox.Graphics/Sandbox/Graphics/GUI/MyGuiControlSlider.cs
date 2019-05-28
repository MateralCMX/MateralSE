namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlSlider : MyGuiControlSliderBase
    {
        private int m_labelDecimalPlaces;
        private string m_labelFormat;
        private float m_minValue;
        private float m_maxValue;
        private bool m_intValue;
        private float m_range;
        public Action<MyGuiControlSlider> ValueChanged;
        public Func<MyGuiControlSlider, bool> SliderClicked;

        public MyGuiControlSlider(Vector2? position = new Vector2?(), float minValue = 0f, float maxValue = 1f, float width = 0.29f, float? defaultValue = new float?(), Vector4? color = new Vector4?(), string labelText = null, int labelDecimalPlaces = 1, float labelScale = 0.8f, float labelSpaceWidth = 0f, string labelFont = "White", string toolTip = null, MyGuiControlSliderStyleEnum visualStyle = 0, MyGuiDrawAlignEnum originAlign = 4, bool intValue = false, bool showLabel = false) : base(position, width, null, defaultRatio, color, labelScale, labelSpaceWidth, labelFont, toolTip, visualStyle, originAlign, showLabel)
        {
            float? nullable1;
            this.m_labelFormat = "{0}";
            float? defaultRatio = null;
            this.m_minValue = minValue;
            this.m_maxValue = maxValue;
            this.m_range = this.m_maxValue - this.m_minValue;
            MyGuiSliderProperties properties1 = new MyGuiSliderProperties();
            properties1.FormatLabel = new Func<float, string>(this.FormatValue);
            properties1.RatioFilter = new Func<float, float>(this.FilterRatio);
            properties1.RatioToValue = new Func<float, float>(this.RatioToValue);
            properties1.ValueToRatio = new Func<float, float>(this.ValueToRatio);
            base.Propeties = properties1;
            if (defaultValue != null)
            {
                nullable1 = new float?(this.ValueToRatio(defaultValue.Value));
            }
            else
            {
                defaultRatio = null;
                nullable1 = defaultRatio;
            }
            this.DefaultRatio = nullable1;
            defaultRatio = base.DefaultRatio;
            this.Ratio = (defaultRatio != null) ? defaultRatio.GetValueOrDefault() : minValue;
            this.m_intValue = intValue;
            this.LabelDecimalPlaces = labelDecimalPlaces;
            this.m_labelFormat = labelText;
            base.UpdateLabel();
        }

        private float FilterRatio(float ratio)
        {
            float single1 = MathHelper.Clamp(ratio, 0f, 1f);
            ratio = single1;
            return (!this.m_intValue ? ratio : this.ValueToRatio((float) Math.Round((double) this.RatioToValue(ratio))));
        }

        private string FormatValue(float value) => 
            ((this.m_labelFormat == null) ? null : string.Format(this.m_labelFormat, MyValueFormatter.GetFormatedFloat(value, this.LabelDecimalPlaces)));

        protected override bool OnSliderClicked() => 
            ((this.SliderClicked == null) ? false : this.SliderClicked(this));

        protected override void OnValueChange()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this);
            }
        }

        private float RatioToValue(float ratio) => 
            (!this.m_intValue ? ((ratio * this.m_range) + this.m_minValue) : ((float) Math.Round((double) ((ratio * this.m_range) + this.m_minValue))));

        private void Refresh()
        {
            base.Ratio = base.Ratio;
        }

        public void SetBounds(float minValue, float maxValue)
        {
            this.m_minValue = minValue;
            this.m_maxValue = maxValue;
            this.m_range = maxValue - minValue;
            this.Refresh();
        }

        private float ValueToRatio(float ratio) => 
            ((ratio - this.m_minValue) / this.m_range);

        public int LabelDecimalPlaces
        {
            get => 
                this.m_labelDecimalPlaces;
            set => 
                (this.m_labelDecimalPlaces = value);
        }

        public float ValueNormalized =>
            base.Ratio;

        public float? DefaultValue
        {
            get
            {
                if (base.DefaultRatio != null)
                {
                    return new float?(this.RatioToValue(base.DefaultRatio.Value));
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    base.DefaultRatio = new float?(this.ValueToRatio(value.Value));
                }
                else
                {
                    base.DefaultRatio = null;
                }
            }
        }

        public float MinValue
        {
            get => 
                this.m_minValue;
            set
            {
                this.m_minValue = value;
                this.m_range = this.m_maxValue - this.m_minValue;
                this.Refresh();
            }
        }

        public float MaxValue
        {
            get => 
                this.m_maxValue;
            set
            {
                this.m_maxValue = value;
                this.m_range = this.m_maxValue - this.m_minValue;
                this.Refresh();
            }
        }

        public bool IntValue
        {
            get => 
                this.m_intValue;
            set
            {
                this.m_intValue = value;
                this.Refresh();
            }
        }
    }
}

