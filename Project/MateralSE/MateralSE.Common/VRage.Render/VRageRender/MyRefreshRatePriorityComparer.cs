namespace VRageRender
{
    using System;
    using System.Collections.Generic;

    public class MyRefreshRatePriorityComparer : IComparer<MyDisplayMode>
    {
        private static readonly float[] m_refreshRates = new float[] { 60f, 75f, 59f, 72f, 100f };

        public int Compare(MyDisplayMode x, MyDisplayMode y)
        {
            if (x.Width != y.Width)
            {
                return x.Width.CompareTo(y.Width);
            }
            if (x.Height != y.Height)
            {
                return x.Height.CompareTo(y.Height);
            }
            if (x.RefreshRateF == y.RefreshRateF)
            {
                return 0;
            }
            for (int i = 0; i < m_refreshRates.Length; i++)
            {
                if (x.RefreshRateF == m_refreshRates[i])
                {
                    return -1;
                }
                if (y.RefreshRateF == m_refreshRates[i])
                {
                    return 1;
                }
            }
            return x.RefreshRate.CompareTo(y.RefreshRate);
        }
    }
}

