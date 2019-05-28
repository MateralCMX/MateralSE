namespace VRage.Stats
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Library.Utils;

    public class MyStats
    {
        public volatile SortEnum Sort = 2;
        private static Comparer<KeyValuePair<string, MyStat>> m_nameComparer = new MyNameComparer();
        private static Comparer<KeyValuePair<string, MyStat>> m_priorityComparer = new MyPriorityComparer();
        private MyGameTimer m_timer = new MyGameTimer();
        private NumberFormatInfo m_format;
        private FastResourceLock m_lock;
        private Dictionary<string, MyStat> m_stats;
        private List<KeyValuePair<string, MyStat>> m_tmpWriteList;

        public MyStats()
        {
            NumberFormatInfo info1 = new NumberFormatInfo();
            info1.NumberDecimalSeparator = ".";
            info1.NumberGroupSeparator = " ";
            this.m_format = info1;
            this.m_lock = new FastResourceLock();
            this.m_stats = new Dictionary<string, MyStat>(0x400);
            this.m_tmpWriteList = new List<KeyValuePair<string, MyStat>>(0x400);
        }

        private void AppendStat(StringBuilder text, string statKey, MyStat stat)
        {
            MyStat.Value value2;
            MyStat.Value value3;
            MyStat.Value value4;
            MyStat.Value value5;
            MyStat.Value value6;
            int num;
            int num2;
            MyStatTypeEnum enum2;
            MyTimeSpan span;
            stat.ReadAndClear(this.m_timer.Elapsed, out value2, out num, out value3, out value4, out value5, out enum2, out num2, out span, out value6);
            if (span > this.RequiredInactivity(enum2))
            {
                this.Remove(statKey);
            }
            else
            {
                string statName = stat.DrawText ?? statKey;
                bool flag = (enum2 & MyStatTypeEnum.LongFlag) == MyStatTypeEnum.LongFlag;
                float num3 = (float) ((flag ? ((double) value2.AsLong) : ((double) value2.AsFloat)) / ((double) num));
                this.m_format.NumberDecimalDigits = num2;
                this.m_format.NumberGroupSeparator = (num2 == 0) ? "," : string.Empty;
                bool flag2 = (enum2 & MyStatTypeEnum.FormatFlag) == MyStatTypeEnum.FormatFlag;
                switch ((enum2 & (MyStatTypeEnum.Avg | MyStatTypeEnum.Counter | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.Min)))
                {
                    case MyStatTypeEnum.CurrentValue:
                        if (flag)
                        {
                            this.AppendStatLine<long, int, int, long>(text, statName, value5.AsLong, 0, 0, value6.AsLong, this.m_format, flag2 ? null : "{0}: {1}");
                            return;
                        }
                        this.AppendStatLine<float, int, int, float>(text, statName, value5.AsFloat, 0, 0, value6.AsFloat, this.m_format, flag2 ? null : "{0}: {1}");
                        return;

                    case MyStatTypeEnum.Min:
                        if (flag)
                        {
                            this.AppendStatLine<long, int, int, int>(text, statName, value3.AsLong, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                            return;
                        }
                        this.AppendStatLine<float, int, int, int>(text, statName, value3.AsFloat, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                        return;

                    case MyStatTypeEnum.Max:
                        if (flag)
                        {
                            this.AppendStatLine<long, int, int, int>(text, statName, value4.AsLong, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                            return;
                        }
                        this.AppendStatLine<float, int, int, int>(text, statName, value4.AsFloat, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                        return;

                    case MyStatTypeEnum.Avg:
                        this.AppendStatLine<float, int, int, int>(text, statName, num3, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                        return;

                    case MyStatTypeEnum.MinMax:
                        if (flag)
                        {
                            this.AppendStatLine<long, long, int, int>(text, statName, value3.AsLong, value4.AsLong, 0, 0, this.m_format, flag2 ? null : "{0}: {1} / {2}");
                            return;
                        }
                        this.AppendStatLine<float, float, int, int>(text, statName, value3.AsFloat, value4.AsFloat, 0, 0, this.m_format, flag2 ? null : "{0}: {1} / {2}");
                        return;

                    case MyStatTypeEnum.MinMaxAvg:
                        if (flag)
                        {
                            this.AppendStatLine<long, long, float, int>(text, statName, value3.AsLong, value4.AsLong, num3, 0, this.m_format, flag2 ? null : "{0}: {1} / {2} / {3}");
                            return;
                        }
                        this.AppendStatLine<float, float, float, int>(text, statName, value3.AsFloat, value4.AsFloat, num3, 0, this.m_format, flag2 ? null : "{0}: {1} / {2} / {3}");
                        return;

                    case MyStatTypeEnum.Sum:
                        if (flag)
                        {
                            this.AppendStatLine<long, int, int, int>(text, statName, value2.AsLong, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                            return;
                        }
                        this.AppendStatLine<float, int, int, int>(text, statName, value2.AsFloat, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                        return;

                    case MyStatTypeEnum.Counter:
                        this.AppendStatLine<int, int, int, int>(text, statName, num, 0, 0, 0, this.m_format, flag2 ? null : "{0}: {1}");
                        return;

                    case MyStatTypeEnum.CounterSum:
                        if (flag)
                        {
                            this.AppendStatLine<int, long, int, int>(text, statName, num, value2.AsLong, 0, 0, this.m_format, flag2 ? null : "{0}: {1} / {2}");
                            return;
                        }
                        this.AppendStatLine<int, float, int, int>(text, statName, num, value2.AsFloat, 0, 0, this.m_format, flag2 ? null : "{0}: {1} / {2}");
                        return;
                }
            }
        }

        private void AppendStatLine<A, B, C, D>(StringBuilder text, string statName, A arg0, B arg1, C arg2, D arg3, NumberFormatInfo format, string formatString) where A: IConvertible where B: IConvertible where C: IConvertible where D: IConvertible
        {
            if (formatString == null)
            {
                text.ConcatFormat<A, B, C, D>(statName, arg0, arg1, arg2, arg3, format);
            }
            else
            {
                text.ConcatFormat<string, A, B, C, D>(formatString, statName, arg0, arg1, arg2, arg3, format);
            }
            text.AppendLine();
        }

        public void Clear()
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                foreach (KeyValuePair<string, MyStat> pair in this.m_stats)
                {
                    pair.Value.Clear();
                }
            }
        }

        public void Clear(string name)
        {
            this.GetStat(name).Clear();
        }

        private string GetMeasureText(string name, MyStatTypeEnum type)
        {
            switch ((type & (MyStatTypeEnum.Avg | MyStatTypeEnum.Counter | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.Min)))
            {
                case MyStatTypeEnum.MinMax:
                    return (name + ": {0}ms / {1}ms");

                case MyStatTypeEnum.MinMaxAvg:
                    return (name + ": {0}ms / {1}ms / {2}ms");

                case MyStatTypeEnum.Counter:
                    return (name + ": {0}x");

                case MyStatTypeEnum.CounterSum:
                    return (name + ": {0}x / {1}ms");
            }
            return (name + ": {0}ms");
        }

        private MyStat GetStat(string name)
        {
            MyStat stat;
            MyStat stat2;
            using (this.m_lock.AcquireSharedUsing())
            {
                if (this.m_stats.TryGetValue(name, out stat))
                {
                    return stat;
                }
            }
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_stats.TryGetValue(name, out stat))
                {
                    stat2 = stat;
                }
                else
                {
                    stat = new MyStat(0);
                    this.m_stats[name] = stat;
                    stat2 = stat;
                }
            }
            return stat2;
        }

        private MyStat GetStat(MyStatKeys.StatKeysEnum key)
        {
            string str;
            int num;
            MyStat stat;
            MyStat stat2;
            MyStatKeys.GetNameAndPriority(key, out str, out num);
            using (this.m_lock.AcquireSharedUsing())
            {
                if (this.m_stats.TryGetValue(str, out stat))
                {
                    return stat;
                }
            }
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_stats.TryGetValue(str, out stat))
                {
                    stat2 = stat;
                }
                else
                {
                    stat = new MyStat(num);
                    this.m_stats[str] = stat;
                    stat2 = stat;
                }
            }
            return stat2;
        }

        public void Increment(string name, int refreshMs = 0, int clearRateMs = -1)
        {
            this.Write(name, (long) 0L, MyStatTypeEnum.Counter, refreshMs, 0, clearRateMs);
        }

        public void Increment(MyStatKeys.StatKeysEnum key, int refreshMs = 0, int clearRateMs = -1)
        {
            this.Write(key, 0f, MyStatTypeEnum.Counter, refreshMs, 0, clearRateMs);
        }

        public MyStatToken Measure(string name) => 
            this.Measure(name, MyStatTypeEnum.Avg, 200, 1, -1);

        public MyStatToken Measure(string name, MyStatTypeEnum type, int refreshMs = 200, int numDecimals = 1, int clearRateMs = -1)
        {
            MyStat stat = this.GetStat(name);
            if (stat.DrawText == null)
            {
                stat.DrawText = this.GetMeasureText(name, type);
            }
            stat.ChangeSettings((type | MyStatTypeEnum.FormatFlag) & (MyStatTypeEnum.Avg | MyStatTypeEnum.Counter | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.FormatFlag | MyStatTypeEnum.KeepInactiveLongerFlag | MyStatTypeEnum.Min), refreshMs, numDecimals, clearRateMs);
            return new MyStatToken(this.m_timer, stat);
        }

        public void Remove(string name)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_stats.Remove(name);
            }
        }

        public void RemoveAll()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_stats.Clear();
            }
        }

        private MyTimeSpan RequiredInactivity(MyStatTypeEnum type) => 
            (((type & MyStatTypeEnum.DontDisappearFlag) != MyStatTypeEnum.DontDisappearFlag) ? (((type & MyStatTypeEnum.KeepInactiveLongerFlag) != MyStatTypeEnum.KeepInactiveLongerFlag) ? MyTimeSpan.FromSeconds(3.0) : MyTimeSpan.FromSeconds(30.0)) : MyTimeSpan.MaxValue);

        public void Write(string name, long value, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(name).Write(value, type, refreshMs, numDecimals, clearRateMs);
        }

        public void Write(string name, float value, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(name).Write(value, type, refreshMs, numDecimals, clearRateMs, 0f);
        }

        public void Write(MyStatKeys.StatKeysEnum key, float value, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(key).Write(value, type, refreshMs, numDecimals, clearRateMs, 0f);
        }

        public void WriteFormat(string name, long value, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(name).Write(value, type | MyStatTypeEnum.FormatFlag, refreshMs, numDecimals, clearRateMs);
        }

        public void WriteFormat(string name, float value, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(name).Write(value, type | MyStatTypeEnum.FormatFlag, refreshMs, numDecimals, clearRateMs, 0f);
        }

        public void WriteFormat(MyStatKeys.StatKeysEnum key, float value, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(key).Write(value, type | MyStatTypeEnum.FormatFlag, refreshMs, numDecimals, clearRateMs, 0f);
        }

        public void WriteFormat(string name, float value1, float value2, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(name).Write(value1, type | MyStatTypeEnum.FormatFlag, refreshMs, numDecimals, clearRateMs, value2);
        }

        public void WriteFormat(MyStatKeys.StatKeysEnum key, float value1, float value2, MyStatTypeEnum type, int refreshMs, int numDecimals, int clearRateMs = -1)
        {
            this.GetStat(key).Write(value1, type | MyStatTypeEnum.FormatFlag, refreshMs, numDecimals, clearRateMs, value2);
        }

        public void WriteTo(StringBuilder writeTo)
        {
            List<KeyValuePair<string, MyStat>> tmpWriteList = this.m_tmpWriteList;
            lock (tmpWriteList)
            {
                try
                {
                    using (this.m_lock.AcquireSharedUsing())
                    {
                        foreach (KeyValuePair<string, MyStat> pair in this.m_stats)
                        {
                            this.m_tmpWriteList.Add(pair);
                        }
                    }
                    switch (this.Sort)
                    {
                        case 1:
                            this.m_tmpWriteList.Sort(m_nameComparer);
                            break;

                        case 2:
                            this.m_tmpWriteList.Sort(m_priorityComparer);
                            break;

                        default:
                            break;
                    }
                    foreach (KeyValuePair<string, MyStat> pair2 in this.m_tmpWriteList)
                    {
                        this.AppendStat(writeTo, pair2.Key, pair2.Value);
                    }
                }
                finally
                {
                    this.m_tmpWriteList.Clear();
                }
            }
        }

        public enum SortEnum
        {
            None,
            Name,
            Priority
        }
    }
}

