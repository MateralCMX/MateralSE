namespace Sandbox.Engine.Voxels
{
    using System;

    public class MyHeightDetailTexture
    {
        public uint Resolution;
        public byte[] Data;

        public MyHeightDetailTexture(byte[] data, uint resolution)
        {
            this.Resolution = resolution;
            this.Data = data;
        }

        public byte GetValue(int x, int y) => 
            this.Data[(int) ((IntPtr) ((y * this.Resolution) + x))];

        public float GetValue(float x, float y) => 
            (this.Data[(int) ((IntPtr) ((((int) (y * this.Resolution)) * this.Resolution) + ((int) (x * this.Resolution))))] * 0.003921569f);
    }
}

