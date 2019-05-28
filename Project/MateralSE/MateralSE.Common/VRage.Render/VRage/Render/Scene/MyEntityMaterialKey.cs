namespace VRage.Render.Scene
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyEntityMaterialKey
    {
        public MyStringId Material;
        public static MyEntityMaterialKeyComparerType Comparer;
        static MyEntityMaterialKey()
        {
            Comparer = new MyEntityMaterialKeyComparerType();
        }
        public class MyEntityMaterialKeyComparerType : IEqualityComparer<MyEntityMaterialKey>
        {
            public bool Equals(MyEntityMaterialKey left, MyEntityMaterialKey right) => 
                (left.Material == right.Material);

            public int GetHashCode(MyEntityMaterialKey materialKey) => 
                materialKey.Material.GetHashCode();
        }
    }
}

