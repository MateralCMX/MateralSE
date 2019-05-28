namespace VRage.Game.Graphics
{
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyEmissiveColorState
    {
        public MyStringHash EmissiveColor;
        public MyStringHash DisplayColor;
        public float Emissivity;

        public MyEmissiveColorState(string emissiveColor, string displayColor, float emissivity)
        {
            this.EmissiveColor = MyStringHash.GetOrCompute(emissiveColor);
            this.DisplayColor = MyStringHash.GetOrCompute(displayColor);
            this.Emissivity = MyMath.Clamp(emissivity, 0f, 1f);
        }
    }
}

