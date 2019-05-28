namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    public class MyBendFilter : IMyModule
    {
        private double m_rangeSizeInverted;
        private double m_clampingMin;
        private double m_clampingMax;

        public MyBendFilter(IMyModule module, double clampRangeMin, double clampRangeMax, double outOfRangeMin, double outOfRangeMax)
        {
            this.Module = module;
            this.m_rangeSizeInverted = 1.0 / (clampRangeMax - clampRangeMin);
            this.m_clampingMin = clampRangeMin;
            this.m_clampingMax = clampRangeMax;
            this.OutOfRangeMin = outOfRangeMin;
            this.OutOfRangeMax = outOfRangeMax;
        }

        private double expandRange(double value) => 
            ((value >= this.m_clampingMin) ? ((value <= this.m_clampingMax) ? ((value - this.m_clampingMin) * this.m_rangeSizeInverted) : this.OutOfRangeMax) : this.OutOfRangeMin);

        public double GetValue(double x)
        {
            double num = this.Module.GetValue(x);
            return this.expandRange(num);
        }

        public double GetValue(double x, double y)
        {
            double num = this.Module.GetValue(x, y);
            return this.expandRange(num);
        }

        public double GetValue(double x, double y, double z)
        {
            double num = this.Module.GetValue(x, y, z);
            return this.expandRange(num);
        }

        public IMyModule Module { get; set; }

        public double OutOfRangeMin { get; set; }

        public double OutOfRangeMax { get; set; }

        public double ClampingMin
        {
            get => 
                this.m_clampingMin;
            set
            {
                this.m_clampingMin = value;
                this.m_rangeSizeInverted = 1.0 / (this.m_clampingMax - this.m_clampingMin);
            }
        }

        public double ClampingMax
        {
            get => 
                this.m_clampingMax;
            set
            {
                this.m_clampingMax = value;
                this.m_rangeSizeInverted = 1.0 / (this.m_clampingMax - this.m_clampingMin);
            }
        }
    }
}

