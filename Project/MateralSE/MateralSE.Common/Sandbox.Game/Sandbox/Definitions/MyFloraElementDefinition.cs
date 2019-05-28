namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders;
    using VRage.Library.Utils;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_FloraElementDefinition), (Type) null)]
    public class MyFloraElementDefinition : MyDefinitionBase
    {
        public Dictionary<string, MyGroupedIds> AppliedGroups;
        public List<GrowthStep> GrowthSteps;
        public MyDefinitionId GatheredItemDefinition;
        public float GatheredAmount;
        public bool IsGatherable;
        public bool Regrowable;
        public float GrowTime;
        public int PostGatherStep;
        public int GatherableStep;
        public float SpawnProbability;
        public MyAreaTransformType AreaTransformType;
        public float DecayTime;
        private static List<string> m_tmpGroupHelper = new List<string>();

        public bool BelongsToGroups(MyStringHash subtypeId)
        {
            using (Dictionary<string, MyGroupedIds>.ValueCollection.Enumerator enumerator = this.AppliedGroups.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGroupedIds.GroupedId[] entries = enumerator.Current.Entries;
                    int index = 0;
                    while (true)
                    {
                        if (index >= entries.Length)
                        {
                            break;
                        }
                        MyGroupedIds.GroupedId id = entries[index];
                        if (!(id.SubtypeId == subtypeId))
                        {
                            index++;
                            continue;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public MyStringHash GetFinalSubtype(string group) => 
            this.AppliedGroups[group].Entries[0].SubtypeId;

        public int GetGroupIndex(string groupName, MyStringHash subtypeId)
        {
            MyGroupedIds.GroupedId[] entries = this.AppliedGroups[groupName].Entries;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].SubtypeId == subtypeId)
                {
                    return i;
                }
            }
            return 0;
        }

        public int GetGrowthStepForSubtype(string group, MyStringHash subtype)
        {
            int groupIndex = this.GetGroupIndex(group, subtype);
            for (int i = this.GrowthSteps.Count - 1; i >= 0; i--)
            {
                if (this.GrowthSteps[i].GroupInsId == groupIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetRandomGroup(MyStringHash subtypeId)
        {
            m_tmpGroupHelper.Clear();
            foreach (KeyValuePair<string, MyGroupedIds> pair in this.AppliedGroups)
            {
                foreach (MyGroupedIds.GroupedId id in pair.Value.Entries)
                {
                    if (id.SubtypeId == subtypeId)
                    {
                        m_tmpGroupHelper.Add(pair.Key);
                    }
                }
            }
            if (m_tmpGroupHelper.Count == 0)
            {
                return null;
            }
            int num = MyRandom.Instance.Next() % m_tmpGroupHelper.Count;
            return m_tmpGroupHelper[num];
        }

        public MyStringHash GetRandomItem()
        {
            string str = this.AppliedGroups.Keys.ToList<string>()[MyRandom.Instance.Next() % this.AppliedGroups.Count];
            return this.AppliedGroups[str].Entries.Last<MyGroupedIds.GroupedId>().SubtypeId;
        }

        public MyStringHash GetSubtypeForGrowthStep(string group, int growthStep)
        {
            GrowthStep step = this.GrowthSteps[growthStep];
            return this.AppliedGroups[group].Entries[step.GroupInsId].SubtypeId;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_FloraElementDefinition definition = builder as MyObjectBuilder_FloraElementDefinition;
            this.AppliedGroups = new Dictionary<string, MyGroupedIds>();
            if (definition.AppliedGroups != null)
            {
                DictionaryValuesReader<string, MyGroupedIds> groupedIds = MyDefinitionManager.Static.GetGroupedIds("EnvGroups");
                MyGroupedIds result = null;
                foreach (string str in definition.AppliedGroups)
                {
                    if (groupedIds.TryGetValue(str, out result))
                    {
                        this.AppliedGroups.Add(str, result);
                    }
                }
            }
            this.GrowthSteps = new List<GrowthStep>();
            if (definition.GrowthSteps != null)
            {
                foreach (MyObjectBuilder_FloraElementDefinition.GrowthStep step in definition.GrowthSteps)
                {
                    this.GrowthSteps.Add(new GrowthStep(step.GroupInsId, step.Percent));
                }
            }
            if (definition.GatheredItem != null)
            {
                this.GatheredItemDefinition = definition.GatheredItem.Id;
                this.GatheredAmount = definition.GatheredItem.Amount;
                this.IsGatherable = true;
            }
            else
            {
                this.GatheredItemDefinition = new MyDefinitionId();
                this.GatheredAmount = -1f;
                this.IsGatherable = false;
            }
            this.Regrowable = definition.Regrowable;
            this.GrowTime = definition.GrowTime;
            this.GatherableStep = definition.GatherableStep;
            this.PostGatherStep = definition.PostGatherStep;
            this.SpawnProbability = definition.SpawnProbability;
            this.AreaTransformType = definition.AreaTransformType;
            this.DecayTime = definition.DecayTime;
        }

        public bool IsFirst(MyStringHash subtypeId)
        {
            using (Dictionary<string, MyGroupedIds>.Enumerator enumerator = this.AppliedGroups.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<string, MyGroupedIds> current = enumerator.Current;
                    if (current.Value.Entries[0].SubtypeId == subtypeId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsFirst(string groupName, MyStringHash subtypeId) => 
            (this.AppliedGroups[groupName].Entries[0].SubtypeId == subtypeId);

        public bool IsGatherableSubtype(string group, MyStringHash subtypeId)
        {
            if (!this.IsGatherable)
            {
                return false;
            }
            if (this.GatherableStep == -1)
            {
                return this.IsFirst(group, subtypeId);
            }
            int groupIndex = this.GetGroupIndex(group, subtypeId);
            return (this.GrowthSteps[this.GatherableStep].GroupInsId == groupIndex);
        }

        public byte TransformTypeByte =>
            ((byte) this.AreaTransformType);

        public bool HasGrowthSteps =>
            (this.GrowthSteps.Count > 0);

        public int StartingId =>
            (this.HasGrowthSteps ? this.GrowthSteps[this.GrowthSteps.Count - 1].GroupInsId : 0);

        public bool ShouldDecay =>
            !(this.DecayTime == 0f);

        [StructLayout(LayoutKind.Sequential)]
        public struct GrowthStep
        {
            public int GroupInsId;
            public float Percent;
            public GrowthStep(int groupIndsId, float percent)
            {
                this.GroupInsId = groupIndsId;
                this.Percent = percent;
            }
        }
    }
}

