namespace Sandbox.Definitions
{
    using System;

    public class CubeBlockEffectBase
    {
        public string Name;
        public float ParameterMin;
        public float ParameterMax;
        public CubeBlockEffect[] ParticleEffects;
        public CubeBlockEffect[] SoundEffects;

        public CubeBlockEffectBase(string Name, float ParameterMin, float ParameterMax)
        {
            this.Name = Name;
            this.ParameterMin = ParameterMin;
            this.ParameterMax = ParameterMax;
        }
    }
}

