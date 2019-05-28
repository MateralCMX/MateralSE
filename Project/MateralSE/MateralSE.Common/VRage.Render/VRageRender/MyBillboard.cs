namespace VRageRender
{
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyBillboard : IComparable
    {
        public MyStringId Material;
        public BlendTypeEnum BlendType;
        public Vector3D Position0;
        public Vector3D Position1;
        public Vector3D Position2;
        public Vector3D Position3;
        public Vector4 Color;
        public float ColorIntensity;
        public float SoftParticleDistanceScale;
        public Vector2 UVOffset;
        public Vector2 UVSize;
        public LocalTypeEnum LocalType;
        public uint ParentID = uint.MaxValue;
        public float DistanceSquared;
        public float Reflectivity;
        public float AlphaCutout;
        public int CustomViewProjection;

        public int CompareTo(object compareToObject) => 
            this.Material.Id.CompareTo(((MyBillboard) compareToObject).Material.Id);

        public enum BlendTypeEnum
        {
            Standard = 0,
            AdditiveBottom = 1,
            AdditiveTop = 2,
            LDR = 3,
            SDR = 3
        }

        public enum LocalTypeEnum
        {
            Custom,
            Line,
            Point
        }
    }
}

