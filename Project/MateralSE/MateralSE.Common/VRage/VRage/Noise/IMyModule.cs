namespace VRage.Noise
{
    using System;

    public interface IMyModule
    {
        double GetValue(double x);
        double GetValue(double x, double y);
        double GetValue(double x, double y, double z);
    }
}

