namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyMeshHelper
    {
        private static readonly int C_BUFFER_CAPACITY = 0x1388;
        private static List<Vector3D> m_tmpVectorBuffer = new List<Vector3D>(C_BUFFER_CAPACITY);

        public static void GenerateSphere(ref MatrixD worldMatrix, float radius, int steps, List<Vector3D> vertices)
        {
            m_tmpVectorBuffer.Clear();
            int num = 0;
            float num2 = 360 / steps;
            float num3 = 90f - num2;
            float num4 = 360f - num2;
            float degrees = 0f;
            while (degrees <= num3)
            {
                float num6 = 0f;
                while (true)
                {
                    Vector3D vectord;
                    if (num6 > num4)
                    {
                        degrees += num2;
                        break;
                    }
                    vectord.X = (float) ((radius * Math.Sin((double) MathHelper.ToRadians(num6))) * Math.Sin((double) MathHelper.ToRadians(degrees)));
                    vectord.Y = (float) ((radius * Math.Cos((double) MathHelper.ToRadians(num6))) * Math.Sin((double) MathHelper.ToRadians(degrees)));
                    vectord.Z = (float) (radius * Math.Cos((double) MathHelper.ToRadians(degrees)));
                    m_tmpVectorBuffer.Add(vectord);
                    num++;
                    vectord.X = (float) ((radius * Math.Sin((double) MathHelper.ToRadians(num6))) * Math.Sin((double) MathHelper.ToRadians((float) (degrees + num2))));
                    vectord.Y = (float) ((radius * Math.Cos((double) MathHelper.ToRadians(num6))) * Math.Sin((double) MathHelper.ToRadians((float) (degrees + num2))));
                    vectord.Z = (float) (radius * Math.Cos((double) MathHelper.ToRadians((float) (degrees + num2))));
                    m_tmpVectorBuffer.Add(vectord);
                    num++;
                    vectord.X = (float) ((radius * Math.Sin((double) MathHelper.ToRadians((float) (num6 + num2)))) * Math.Sin((double) MathHelper.ToRadians(degrees)));
                    vectord.Y = (float) ((radius * Math.Cos((double) MathHelper.ToRadians((float) (num6 + num2)))) * Math.Sin((double) MathHelper.ToRadians(degrees)));
                    vectord.Z = (float) (radius * Math.Cos((double) MathHelper.ToRadians(degrees)));
                    m_tmpVectorBuffer.Add(vectord);
                    num++;
                    vectord.X = (float) ((radius * Math.Sin((double) MathHelper.ToRadians((float) (num6 + num2)))) * Math.Sin((double) MathHelper.ToRadians((float) (degrees + num2))));
                    vectord.Y = (float) ((radius * Math.Cos((double) MathHelper.ToRadians((float) (num6 + num2)))) * Math.Sin((double) MathHelper.ToRadians((float) (degrees + num2))));
                    vectord.Z = (float) (radius * Math.Cos((double) MathHelper.ToRadians((float) (degrees + num2))));
                    m_tmpVectorBuffer.Add(vectord);
                    num++;
                    num6 += num2;
                }
            }
            int count = m_tmpVectorBuffer.Count;
            foreach (Vector3D vectord2 in m_tmpVectorBuffer)
            {
                vertices.Add(vectord2);
            }
            foreach (Vector3D vectord3 in m_tmpVectorBuffer)
            {
                Vector3D item = new Vector3D(vectord3.X, vectord3.Y, -vectord3.Z);
                vertices.Add(item);
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = Vector3D.Transform(vertices[i], (MatrixD) worldMatrix);
            }
        }
    }
}

