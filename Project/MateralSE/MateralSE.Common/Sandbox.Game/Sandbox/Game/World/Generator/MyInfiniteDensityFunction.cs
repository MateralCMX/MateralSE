namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Library.Utils;
    using VRage.Noise;
    using VRageMath;

    internal class MyInfiniteDensityFunction : IMyAsteroidFieldDensityFunction, IMyModule
    {
        private IMyModule noise;

        public MyInfiniteDensityFunction(MyRandom random, double frequency)
        {
            this.noise = new MySimplexFast(random.Next(), frequency);
        }

        public bool ExistsInCell(ref BoundingBoxD bbox) => 
            true;

        public double GetValue(double x) => 
            this.noise.GetValue(x);

        public double GetValue(double x, double y) => 
            this.noise.GetValue(x, y);

        public double GetValue(double x, double y, double z) => 
            this.noise.GetValue(x, y, z);
    }
}

