namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVoxelClipmapSettings
    {
        public int CellSizeLg2;
        public int[] LodRanges;
        public static MyVoxelClipmapSettings Default;
        private static readonly Dictionary<string, MyVoxelClipmapSettings> m_settingsPerGroup;
        public bool IsValid
        {
            get
            {
                if ((this.LodRanges == null) || (this.LodRanges.Length != 0x10))
                {
                    return false;
                }
                for (int i = 1; i < 0x10; i++)
                {
                    if (this.LodRanges[i - 1] > this.LodRanges[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool Equals(MyVoxelClipmapSettings other) => 
            ((this.CellSizeLg2 == other.CellSizeLg2) && Equals(this.LodRanges, other.LodRanges));

        public override bool Equals(object obj) => 
            ((obj != null) ? ((obj is MyVoxelClipmapSettings) && this.Equals((MyVoxelClipmapSettings) obj)) : false);

        public override int GetHashCode() => 
            ((this.CellSizeLg2 * 0x18d) ^ ((this.LodRanges != null) ? this.LodRanges.GetHashCode() : 0));

        private static bool Equals(int[] left, int[] right)
        {
            if (left != right)
            {
                if (left.Length != right.Length)
                {
                    return false;
                }
                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static int[] MakeRanges(int lod0Cells, float higherLodCells, int cellSizeLg2, int lastLod = -1, int lastLodRange = -1)
        {
            int num;
            int[] numArray = new int[] { lod0Cells * num };
            num = 1 << (cellSizeLg2 & 0x1f);
            for (int i = 1; i < numArray.Length; i++)
            {
                if ((lastLod != -1) && (lastLod == i))
                {
                    numArray[i] = lastLodRange;
                }
                else
                {
                    float num4 = numArray[i - 1] * higherLodCells;
                    numArray[i] = (num4 <= 2.147484E+09f) ? ((int) num4) : 0x7fffffff;
                }
            }
            return numArray;
        }

        public static MyVoxelClipmapSettings Create(int cellBits, int lod0Size, float lodSize, int lastLod = -1, int lastLodRange = -1) => 
            new MyVoxelClipmapSettings { 
                CellSizeLg2 = cellBits,
                LodRanges = MakeRanges(lod0Size, lodSize, cellBits, lastLod, lastLodRange)
            };

        public static void SetSettingsForGroup(string group, MyVoxelClipmapSettings settings)
        {
            m_settingsPerGroup[group] = settings;
        }

        public static MyVoxelClipmapSettings GetSettings(string settingsGroup)
        {
            MyVoxelClipmapSettings settings;
            return (!m_settingsPerGroup.TryGetValue(settingsGroup, out settings) ? Default : settings);
        }

        static MyVoxelClipmapSettings()
        {
            Default = Create(4, 2, 2f, 6, 0x4000);
            m_settingsPerGroup = new Dictionary<string, MyVoxelClipmapSettings>();
        }
    }
}

