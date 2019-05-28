namespace Sandbox.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Noise;

    internal class MyPlanetDetailModulator : IMyModule
    {
        private MyPlanetGeneratorDefinition m_planetDefinition;
        private MyPlanetMaterialProvider m_oreDeposit;
        private float m_radius;
        private Dictionary<byte, MyModulatorData> m_modulators = new Dictionary<byte, MyModulatorData>();

        public MyPlanetDetailModulator(MyPlanetGeneratorDefinition planetDefinition, MyPlanetMaterialProvider oreDeposit, int seed, float radius)
        {
            this.m_planetDefinition = planetDefinition;
            this.m_oreDeposit = oreDeposit;
            this.m_radius = radius;
            foreach (MyPlanetDistortionDefinition definition in this.m_planetDefinition.DistortionTable)
            {
                int num3;
                MyModuleFast fast = null;
                float num2 = definition.Frequency * (radius / 6f);
                string type = definition.Type;
                if (type == "Billow")
                {
                    num3 = seed;
                    fast = new MyBillowFast(MyNoiseQuality.High, definition.LayerCount, num3, (double) num2, 2.0, 0.5);
                }
                else if (type == "RidgedMultifractal")
                {
                    num3 = seed;
                    fast = new MyRidgedMultifractalFast(MyNoiseQuality.High, definition.LayerCount, num3, (double) num2, 2.0, 2.0, 1.0);
                }
                else if (type == "Perlin")
                {
                    num3 = seed;
                    fast = new MyPerlinFast(MyNoiseQuality.High, definition.LayerCount, num3, (double) num2, 2.0, 0.5);
                }
                else if (type == "Simplex")
                {
                    MySimplexFast fast1 = new MySimplexFast(1, 1.0);
                    fast1.Seed = seed;
                    fast1.Frequency = num2;
                    fast = fast1;
                }
                if (fast != null)
                {
                    MyModulatorData data = new MyModulatorData {
                        Height = definition.Height,
                        Modulator = fast
                    };
                    this.m_modulators.Add(definition.Value, data);
                }
            }
        }

        public double GetValue(double x) => 
            0.0;

        public double GetValue(double x, double y) => 
            0.0;

        public double GetValue(double x, double y, double z) => 
            0.0;

        [StructLayout(LayoutKind.Sequential)]
        private struct MyModulatorData
        {
            public float Height;
            public MyModuleFast Modulator;
        }
    }
}

