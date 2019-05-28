namespace VRageRender
{
    using System;

    public static class MyPostprocessSettingsWrapper
    {
        public static MyPostprocessSettings Settings = MyPostprocessSettings.Default;
        private static bool m_allEnabled = true;
        private static bool m_isDirty = true;

        public static void MarkDirty()
        {
            m_isDirty = true;
        }

        public static void ReducePostProcessing()
        {
            m_isDirty = false;
            AllEnabled = false;
            Settings.BloomEnabled = false;
            Settings.Data.ChromaticFactor = 0f;
            Settings.Data.VignetteStart = 0f;
            Settings.Data.VignetteLength = 1f;
        }

        public static void ReloadSettingsFrom(MyPostprocessSettings definition)
        {
            m_isDirty = false;
            AllEnabled = true;
            Settings.BloomEnabled = definition.BloomEnabled;
            Settings.Data.ChromaticFactor = definition.Data.ChromaticFactor;
            Settings.Data.VignetteStart = definition.Data.VignetteStart;
            Settings.Data.VignetteLength = definition.Data.VignetteLength;
        }

        public static void SetWardrobePostProcessing()
        {
            m_isDirty = false;
            AllEnabled = false;
            Settings.BloomEnabled = true;
            Settings.Data.ChromaticFactor = 0f;
            Settings.Data.VignetteStart = 0f;
            Settings.Data.VignetteLength = 1f;
        }

        public static bool AllEnabled
        {
            get => 
                m_allEnabled;
            set => 
                (m_allEnabled = value);
        }

        public static bool IsDirty =>
            m_isDirty;
    }
}

