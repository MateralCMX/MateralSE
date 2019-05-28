namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBiomeMaterial
    {
        public readonly byte Biome;
        public readonly byte Material;
        public static IEqualityComparer<MyBiomeMaterial> Comparer;
        public MyBiomeMaterial(byte biome, byte material)
        {
            this.Biome = biome;
            this.Material = material;
        }

        public override int GetHashCode() => 
            ((this.Biome << 8) | this.Material).GetHashCode();

        public override string ToString() => 
            $"Biome[{this.Biome}]:{MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.Material).Id.SubtypeName}";

        static MyBiomeMaterial()
        {
            Comparer = new MyComparer();
        }
        private class MyComparer : IEqualityComparer<MyBiomeMaterial>
        {
            public unsafe bool Equals(MyBiomeMaterial x, MyBiomeMaterial y)
            {
                ushort* numPtr2 = (ushort*) &y;
                return (x == numPtr2[0]);
            }

            public unsafe int GetHashCode(MyBiomeMaterial obj) => 
                ((ushort) ((IntPtr) &obj)).GetHashCode();
        }
    }
}

