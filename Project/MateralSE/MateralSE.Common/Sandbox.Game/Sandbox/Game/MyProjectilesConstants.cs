namespace Sandbox.Game
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRageMath;

    public static class MyProjectilesConstants
    {
        public const int MAX_PROJECTILES_COUNT = 0x2000;
        public const float HIT_STRENGTH_IMPULSE = 500f;
        private static readonly Dictionary<int, Vector3> m_projectileTrailColors = new Dictionary<int, Vector3>(10);
        public static readonly float AUTOAIMING_PRECISION = 500f;

        static MyProjectilesConstants()
        {
            m_projectileTrailColors.Add(-1, Vector3.One);
            m_projectileTrailColors.Add(1, Vector3.One);
            m_projectileTrailColors.Add(0, new Vector3(10f, 10f, 10f));
            m_projectileTrailColors.Add(3, Vector3.One);
            m_projectileTrailColors.Add(2, Vector3.One);
            m_projectileTrailColors.Add(4, Vector3.One);
        }

        public static Vector3 GetProjectileTrailColorByType(MyAmmoType ammoType)
        {
            Vector3 vector;
            if (!m_projectileTrailColors.TryGetValue((int) ammoType, out vector))
            {
                m_projectileTrailColors.TryGetValue(-1, out vector);
            }
            return vector;
        }
    }
}

