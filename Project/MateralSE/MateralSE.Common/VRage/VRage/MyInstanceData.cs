namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageMath.PackedVector;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyInstanceData
    {
        public HalfVector4 m_row0;
        public HalfVector4 m_row1;
        public HalfVector4 m_row2;
        public HalfVector4 ColorMaskHSV;
        public MyInstanceData(Matrix m)
        {
            this.m_row0 = new HalfVector4(m.M11, m.M21, m.M31, m.M41);
            this.m_row1 = new HalfVector4(m.M12, m.M22, m.M32, m.M42);
            this.m_row2 = new HalfVector4(m.M13, m.M23, m.M33, m.M43);
            this.ColorMaskHSV = new HalfVector4();
        }

        public Matrix LocalMatrix
        {
            get
            {
                Vector4 vector = this.m_row0.ToVector4();
                Vector4 vector2 = this.m_row1.ToVector4();
                Vector4 vector3 = this.m_row2.ToVector4();
                return new Matrix(vector.X, vector2.X, vector3.X, 0f, vector.Y, vector2.Y, vector3.Y, 0f, vector.Z, vector2.Z, vector3.Z, 0f, vector.W, vector2.W, vector3.W, 1f);
            }
            set
            {
                this.m_row0 = new HalfVector4(value.M11, value.M21, value.M31, value.M41);
                this.m_row1 = new HalfVector4(value.M12, value.M22, value.M32, value.M42);
                this.m_row2 = new HalfVector4(value.M13, value.M23, value.M33, value.M43);
            }
        }
        public Vector3 Translation
        {
            get => 
                new Vector3(HalfUtils.Unpack((ushort) (this.m_row0.PackedValue >> 0x30)), HalfUtils.Unpack((ushort) (this.m_row1.PackedValue >> 0x30)), HalfUtils.Unpack((ushort) (this.m_row2.PackedValue >> 0x30)));
            set
            {
                this.m_row0.PackedValue = (this.m_row0.PackedValue & ((ulong) 0xffffffffffffL)) | (HalfUtils.Pack(value.X) << 0x30);
                this.m_row1.PackedValue = (this.m_row1.PackedValue & ((ulong) 0xffffffffffffL)) | (HalfUtils.Pack(value.Y) << 0x30);
                this.m_row2.PackedValue = (this.m_row2.PackedValue & ((ulong) 0xffffffffffffL)) | (HalfUtils.Pack(value.Z) << 0x30);
            }
        }
    }
}

