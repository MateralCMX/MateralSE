namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;

    public static class MyTransparentMaterials
    {
        private static readonly Dictionary<MyStringId, MyTransparentMaterial> m_materialsByName = new Dictionary<MyStringId, MyTransparentMaterial>(MyStringId.Comparer);
        public static readonly MyTransparentMaterial ErrorMaterial;
        private static Action m_onUpdate;

        static MyTransparentMaterials()
        {
            Vector2I? targetSize = null;
            ErrorMaterial = new MyTransparentMaterial(MyStringId.GetOrCompute("ErrorMaterial"), MyTransparentMaterialTextureType.FileTexture, @"Textures\FAKE.dds", @"Textures\FAKE.dds", 9999f, false, false, Color.Pink.ToVector4(), (Vector4) Color.Black, (Vector4) Color.Black, Vector4.One * 0.1f, false, false, 1f, 4f, 1f, 0f, false, targetSize, 1f, 0.1f, 0.4f, 0.55f, 20f);
            Clear();
        }

        public static void AddMaterial(MyTransparentMaterial material)
        {
            m_materialsByName[material.Id] = material;
        }

        private static void Clear()
        {
            m_materialsByName.Clear();
            AddMaterial(ErrorMaterial);
        }

        public static bool ContainsMaterial(MyStringId materialId) => 
            m_materialsByName.ContainsKey(materialId);

        public static MyTransparentMaterial GetMaterial(MyStringId materialId)
        {
            MyTransparentMaterial material;
            return (!m_materialsByName.TryGetValue(materialId, out material) ? ErrorMaterial : material);
        }

        public static bool TryGetMaterial(MyStringId materialId, out MyTransparentMaterial material) => 
            m_materialsByName.TryGetValue(materialId, out material);

        public static void Update()
        {
            if (m_onUpdate != null)
            {
                m_onUpdate();
            }
        }

        public static DictionaryValuesReader<MyStringId, MyTransparentMaterial> Materials =>
            new DictionaryValuesReader<MyStringId, MyTransparentMaterial>(m_materialsByName);

        public static int Count =>
            m_materialsByName.Count;
    }
}

