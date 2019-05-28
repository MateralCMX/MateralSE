namespace VRage.Game.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public static class MyEmissiveColors
    {
        private static Dictionary<MyStringHash, Color> m_EmissiveColorDictionary = new Dictionary<MyStringHash, Color>();

        public static bool AddEmissiveColor(MyStringHash id, Color color, bool overWrite = false)
        {
            if (!m_EmissiveColorDictionary.ContainsKey(id))
            {
                m_EmissiveColorDictionary.Add(id, color);
                return true;
            }
            if (!overWrite)
            {
                return false;
            }
            m_EmissiveColorDictionary[id] = color;
            return true;
        }

        public static void ClearColors()
        {
            m_EmissiveColorDictionary.Clear();
        }

        public static Color GetEmissiveColor(MyStringHash id) => 
            (!m_EmissiveColorDictionary.ContainsKey(id) ? Color.Black : m_EmissiveColorDictionary[id]);
    }
}

