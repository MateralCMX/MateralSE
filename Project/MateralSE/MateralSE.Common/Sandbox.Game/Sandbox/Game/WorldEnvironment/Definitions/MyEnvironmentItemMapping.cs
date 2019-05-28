namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Utils;

    public class MyEnvironmentItemMapping
    {
        public MyDiscreteSampler<MyRuntimeEnvironmentItemInfo>[] Samplers;
        public int[] Keys;
        public MyEnvironmentRule Rule;

        public MyEnvironmentItemMapping(MyRuntimeEnvironmentItemInfo[] map, MyEnvironmentRule rule, MyProceduralEnvironmentDefinition env)
        {
            this.Rule = rule;
            SortedDictionary<int, List<MyRuntimeEnvironmentItemInfo>> dictionary = new SortedDictionary<int, List<MyRuntimeEnvironmentItemInfo>>();
            foreach (MyRuntimeEnvironmentItemInfo info in map)
            {
                List<MyRuntimeEnvironmentItemInfo> list;
                MyItemTypeDefinition type = info.Type;
                if (!dictionary.TryGetValue(type.LodFrom + 1, out list))
                {
                    list = new List<MyRuntimeEnvironmentItemInfo>();
                    dictionary[type.LodFrom + 1] = list;
                }
                list.Add(info);
            }
            this.Keys = dictionary.Keys.ToArray<int>();
            List<MyRuntimeEnvironmentItemInfo>[] array = dictionary.Values.ToArray<List<MyRuntimeEnvironmentItemInfo>>();
            this.Samplers = new MyDiscreteSampler<MyRuntimeEnvironmentItemInfo>[this.Keys.Length];
            for (int i = 0; i < this.Keys.Length; i++)
            {
                this.Samplers[i] = this.PrepareSampler(from x in array.Range<List<MyRuntimeEnvironmentItemInfo>>(i, array.Length) select x);
            }
        }

        public MyRuntimeEnvironmentItemInfo GetItemRated(int lod, float rate)
        {
            int index = this.Keys.BinaryIntervalSearch<int>(lod);
            return ((index <= this.Samplers.Length) ? this.Samplers[index].Sample(rate) : null);
        }

        public MyDiscreteSampler<MyRuntimeEnvironmentItemInfo> PrepareSampler(IEnumerable<MyRuntimeEnvironmentItemInfo> items)
        {
            float num = 0f;
            foreach (MyRuntimeEnvironmentItemInfo info in items)
            {
                num += info.Density;
            }
            if (num >= 1f)
            {
                return new MyDiscreteSampler<MyRuntimeEnvironmentItemInfo>(items, from x in items select x.Density);
            }
            float[] second = new float[] { 1f - num };
            return new MyDiscreteSampler<MyRuntimeEnvironmentItemInfo>(items.Concat<MyRuntimeEnvironmentItemInfo>(new MyRuntimeEnvironmentItemInfo[1]), (from x in items select x.Density).Concat<float>(second));
        }

        public MyDiscreteSampler<MyRuntimeEnvironmentItemInfo> Sampler(int lod)
        {
            int index = this.Keys.BinaryIntervalSearch<int>(lod);
            return ((index < this.Samplers.Length) ? this.Samplers[index] : null);
        }

        public bool ValidForLod(int lod) => 
            (this.Keys.BinaryIntervalSearch<int>(lod) <= this.Samplers.Length);

        public bool Valid =>
            (this.Samplers != null);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEnvironmentItemMapping.<>c <>9 = new MyEnvironmentItemMapping.<>c();
            public static Func<List<MyRuntimeEnvironmentItemInfo>, IEnumerable<MyRuntimeEnvironmentItemInfo>> <>9__3_0;
            public static Func<MyRuntimeEnvironmentItemInfo, float> <>9__4_0;
            public static Func<MyRuntimeEnvironmentItemInfo, float> <>9__4_1;

            internal IEnumerable<MyRuntimeEnvironmentItemInfo> <.ctor>b__3_0(List<MyRuntimeEnvironmentItemInfo> x) => 
                x;

            internal float <PrepareSampler>b__4_0(MyRuntimeEnvironmentItemInfo x) => 
                x.Density;

            internal float <PrepareSampler>b__4_1(MyRuntimeEnvironmentItemInfo x) => 
                x.Density;
        }
    }
}

