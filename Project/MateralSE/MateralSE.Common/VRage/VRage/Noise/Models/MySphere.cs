namespace VRage.Noise.Models
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;

    internal class MySphere : IMyModule
    {
        public MySphere(IMyModule module)
        {
            this.Module = module;
        }

        public double GetValue(double x)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double latitude, double longitude)
        {
            double num;
            double num2;
            double num3;
            this.LatLonToXYZ(latitude, longitude, out num, out num2, out num3);
            return this.Module.GetValue(num, num2, num3);
        }

        public double GetValue(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        protected void LatLonToXYZ(double lat, double lon, out double x, out double y, out double z)
        {
            double num = Math.Cos(0.017453292519943295 * lat);
            x = Math.Cos(0.017453292519943295 * lon) * num;
            y = Math.Sin(0.017453292519943295 * lat);
            z = Math.Sin(0.017453292519943295 * lon) * num;
        }

        public IMyModule Module { get; set; }
    }
}

