namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyGuiSliderPropertiesExponential : MyGuiSliderProperties
    {
        public MyGuiSliderPropertiesExponential(float min, float max, float exponent = 10f, bool integer = false)
        {
            float maxLog = (float) (Math.Log((double) max) / Math.Log((double) exponent));
            float minLog = (float) (Math.Log((double) min) / Math.Log((double) exponent));
            this.FormatLabel = x => $"{x:N0}m";
            base.ValueToRatio = x => (float) (((Math.Log((double) x) / Math.Log((double) exponent)) - minLog) / ((double) (maxLog - minLog)));
            base.RatioToValue = delegate (float x) {
                double num = Math.Pow((double) exponent, (double) ((x * (maxLog - minLog)) + minLog));
                return integer ? ((float) ((int) num)) : ((float) num);
            };
            this.RatioFilter = x => x;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiSliderPropertiesExponential.<>c <>9 = new MyGuiSliderPropertiesExponential.<>c();
            public static Func<float, string> <>9__0_0;
            public static Func<float, float> <>9__0_3;

            internal string <.ctor>b__0_0(float x) => 
                $"{x:N0}m";

            internal float <.ctor>b__0_3(float x) => 
                x;
        }
    }
}

