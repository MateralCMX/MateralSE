namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public static class MyDecalMaterials
    {
        private static Dictionary<string, List<MyDecalMaterial>> m_decalMaterials = new Dictionary<string, List<MyDecalMaterial>>();

        public static void AddDecalMaterial(MyDecalMaterial decalMaterial)
        {
            List<MyDecalMaterial> list;
            if (!m_decalMaterials.TryGetValue(decalMaterial.StringId, out list))
            {
                list = new List<MyDecalMaterial>();
                m_decalMaterials[decalMaterial.StringId] = list;
            }
            list.Add(decalMaterial);
        }

        public static void ClearMaterials()
        {
            m_decalMaterials.Clear();
        }

        public static string GetStringId(string source, string target) => 
            ((string.IsNullOrEmpty(source) ? "NULL" : source) + "_" + (string.IsNullOrEmpty(target) ? "NULL" : target));

        public static string GetStringId(MyStringHash source, MyStringHash target) => 
            (((source == MyStringHash.NullOrEmpty) ? "NULL" : source.String) + "_" + ((target == MyStringHash.NullOrEmpty) ? "NULL" : target.String));

        public static bool TryGetDecalMaterial(string source, string target, out IReadOnlyList<MyDecalMaterial> decalMaterials)
        {
            List<MyDecalMaterial> list;
            bool flag1 = TryGetDecalMateriald(source, target, out list);
            decalMaterials = list;
            return flag1;
        }

        private static bool TryGetDecalMateriald(string source, string target, out List<MyDecalMaterial> decalMaterial)
        {
            string stringId = GetStringId(source, target);
            return m_decalMaterials.TryGetValue(stringId, out decalMaterial);
        }
    }
}

