namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    internal static class MyCubeGridDeformationTables
    {
        public static DeformationTable[] ThinUpper = new DeformationTable[] { CreateTable(new Vector3I(1, 0, 0)), CreateTable(new Vector3I(0, 1, 0)), CreateTable(new Vector3I(0, 0, 1)) };
        public static DeformationTable[] ThinLower = new DeformationTable[] { CreateTable(new Vector3I(-1, 0, 0)), CreateTable(new Vector3I(0, -1, 0)), CreateTable(new Vector3I(0, 0, -1)) };

        private static DeformationTable CreateTable(Vector3I normal)
        {
            DeformationTable table = new DeformationTable {
                Normal = normal
            };
            Vector3I vectori = new Vector3I(1, 1, 1);
            Vector3I vectori2 = Vector3I.Abs(normal);
            Vector3I vectori3 = (new Vector3I(1, 1, 1) - vectori2) * 2;
            int x = -vectori3.X;
            while (x <= vectori3.X)
            {
                int y = -vectori3.Y;
                while (true)
                {
                    if (y > vectori3.Y)
                    {
                        x++;
                        break;
                    }
                    int z = -vectori3.Z;
                    while (true)
                    {
                        if (z > vectori3.Z)
                        {
                            y++;
                            break;
                        }
                        Vector3I vectori4 = new Vector3I(x, y, z);
                        float num4 = 1f;
                        if (Math.Max(Math.Abs(z), Math.Max(Math.Abs(x), Math.Abs(y))) > 1f)
                        {
                            num4 = 0.3f;
                        }
                        float num5 = num4 * 0.25f;
                        Vector3I key = (Vector3I) ((vectori + new Vector3I(x, y, z)) + normal);
                        table.OffsetTable.Add(key, Matrix.CreateFromDir((Vector3) (-normal * num5)));
                        Vector3I item = key >> 1;
                        Vector3I vectori7 = (key - Vector3I.One) >> 1;
                        table.CubeOffsets.Add(item);
                        table.CubeOffsets.Add(vectori7);
                        table.MinOffset = Vector3I.Min(table.MinOffset, vectori4);
                        table.MaxOffset = Vector3I.Max(table.MaxOffset, vectori4);
                        z++;
                    }
                }
            }
            return table;
        }

        public class DeformationTable
        {
            public readonly Dictionary<Vector3I, Matrix> OffsetTable = new Dictionary<Vector3I, Matrix>();
            public readonly HashSet<Vector3I> CubeOffsets = new HashSet<Vector3I>();
            public Vector3I Normal;
            public Vector3I MinOffset = Vector3I.MaxValue;
            public Vector3I MaxOffset = Vector3I.MinValue;
        }
    }
}

