namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI;
    using VRageMath;

    public static class MyOxygenProviderSystem
    {
        private static List<IMyOxygenProvider> m_oxygenGenerators = new List<IMyOxygenProvider>();

        public static void AddOxygenGenerator(IMyOxygenProvider gravityGenerator)
        {
            m_oxygenGenerators.Add(gravityGenerator);
        }

        public static void ClearOxygenGenerators()
        {
            m_oxygenGenerators.Clear();
        }

        public static float GetOxygenInPoint(Vector3D worldPoint)
        {
            float n = 0f;
            foreach (IMyOxygenProvider provider in m_oxygenGenerators)
            {
                if (provider.IsPositionInRange(worldPoint))
                {
                    n += provider.GetOxygenForPosition(worldPoint);
                }
            }
            return MathHelper.Saturate(n);
        }

        public static void RemoveOxygenGenerator(IMyOxygenProvider gravityGenerator)
        {
            m_oxygenGenerators.Remove(gravityGenerator);
        }
    }
}

