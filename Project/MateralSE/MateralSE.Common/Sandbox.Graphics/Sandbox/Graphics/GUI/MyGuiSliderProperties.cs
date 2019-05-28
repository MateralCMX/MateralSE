namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class MyGuiSliderProperties
    {
        public Func<float, float> RatioToValue;
        public Func<float, float> ValueToRatio;
        public Func<float, float> RatioFilter;
        public Func<float, string> FormatLabel;
        public static MyGuiSliderProperties Default;

        static MyGuiSliderProperties()
        {
            MyGuiSliderProperties properties1 = new MyGuiSliderProperties();
            properties1.ValueToRatio = f => f;
            properties1.RatioToValue = f => f;
            properties1.RatioFilter = f => f;
            properties1.FormatLabel = f => f.ToString(CultureInfo.CurrentCulture);
            Default = properties1;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiSliderProperties.<>c <>9 = new MyGuiSliderProperties.<>c();

            internal float <.cctor>b__6_0(float f) => 
                f;

            internal float <.cctor>b__6_1(float f) => 
                f;

            internal float <.cctor>b__6_2(float f) => 
                f;

            internal string <.cctor>b__6_3(float f) => 
                f.ToString(CultureInfo.CurrentCulture);
        }
    }
}

