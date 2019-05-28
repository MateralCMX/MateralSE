namespace VRage.Game.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public static class MyEmissiveColorPresets
    {
        private static Dictionary<MyStringHash, Dictionary<MyStringHash, MyEmissiveColorState>> m_presets = new Dictionary<MyStringHash, Dictionary<MyStringHash, MyEmissiveColorState>>();

        public static bool AddPreset(MyStringHash id, Dictionary<MyStringHash, MyEmissiveColorState> preset = null, bool overWrite = false)
        {
            if (!m_presets.ContainsKey(id))
            {
                m_presets.Add(id, preset);
                return true;
            }
            if (!overWrite)
            {
                return false;
            }
            m_presets[id] = preset;
            return true;
        }

        public static bool AddPresetState(MyStringHash presetId, MyStringHash stateId, MyEmissiveColorState state, bool overWrite = false)
        {
            if (!m_presets.ContainsKey(presetId))
            {
                return false;
            }
            if (m_presets[presetId] == null)
            {
                m_presets[presetId] = new Dictionary<MyStringHash, MyEmissiveColorState>();
            }
            if (!m_presets[presetId].ContainsKey(stateId))
            {
                m_presets[presetId].Add(stateId, state);
                return true;
            }
            if (!overWrite)
            {
                return false;
            }
            ClearPresetStates(presetId);
            m_presets[presetId][stateId] = state;
            return true;
        }

        public static void ClearPresets()
        {
            m_presets.Clear();
        }

        public static void ClearPresetStates(MyStringHash id)
        {
            if (m_presets.ContainsKey(id) && (m_presets[id] != null))
            {
                m_presets[id].Clear();
            }
        }

        public static bool ContainsPreset(MyStringHash id) => 
            m_presets.ContainsKey(id);

        public static Dictionary<MyStringHash, MyEmissiveColorState> GetPreset(MyStringHash id) => 
            (!m_presets.ContainsKey(id) ? null : m_presets[id]);

        public static bool LoadPresetState(MyStringHash presetId, MyStringHash stateId, out MyEmissiveColorStateResult result)
        {
            result = new MyEmissiveColorStateResult();
            if (presetId == MyStringHash.NullOrEmpty)
            {
                presetId = MyStringHash.GetOrCompute("Default");
            }
            if (!m_presets.ContainsKey(presetId) || !m_presets[presetId].ContainsKey(stateId))
            {
                return false;
            }
            result.EmissiveColor = MyEmissiveColors.GetEmissiveColor(m_presets[presetId][stateId].EmissiveColor);
            result.DisplayColor = MyEmissiveColors.GetEmissiveColor(m_presets[presetId][stateId].DisplayColor);
            result.Emissivity = m_presets[presetId][stateId].Emissivity;
            return true;
        }
    }
}

