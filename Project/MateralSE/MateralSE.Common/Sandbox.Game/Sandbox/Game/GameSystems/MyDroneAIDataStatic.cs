namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;

    public static class MyDroneAIDataStatic
    {
        public static MyDroneAIData Default = new MyDroneAIData();
        private static Dictionary<string, MyDroneAIData> presets = new Dictionary<string, MyDroneAIData>();

        public static MyDroneAIData LoadPreset(string key) => 
            presets.GetValueOrDefault<string, MyDroneAIData>(key, Default);

        public static void Reset()
        {
            presets.Clear();
        }

        public static void SavePreset(string key, MyDroneAIData preset)
        {
            presets[key] = preset;
        }

        public static DictionaryReader<string, MyDroneAIData> Presets =>
            presets;
    }
}

