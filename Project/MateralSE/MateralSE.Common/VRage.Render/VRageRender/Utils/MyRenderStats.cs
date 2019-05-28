namespace VRageRender.Utils
{
    using System;
    using System.Collections.Generic;
    using VRage.Stats;

    public static class MyRenderStats
    {
        public static Dictionary<ColumnEnum, List<MyStats>> m_stats;
        public static readonly MyStats Generic = new MyStats();

        static MyRenderStats()
        {
            Generic = new MyStats();
            List<MyStats> list1 = new List<MyStats>();
            list1.Add(Generic);
            Dictionary<ColumnEnum, List<MyStats>> dictionary1 = new Dictionary<ColumnEnum, List<MyStats>>(EqualityComparer<ColumnEnum>.Default);
            dictionary1.Add(ColumnEnum.Left, list1);
            dictionary1.Add(ColumnEnum.Right, new List<MyStats>());
            m_stats = dictionary1;
        }

        public static void SetColumn(ColumnEnum column, params MyStats[] stats)
        {
            List<MyStats> list;
            if (!m_stats.TryGetValue(column, out list))
            {
                list = new List<MyStats>();
                m_stats[column] = list;
            }
            list.Clear();
            list.AddArray<MyStats>(stats);
        }

        public enum ColumnEnum
        {
            Left,
            Right
        }
    }
}

