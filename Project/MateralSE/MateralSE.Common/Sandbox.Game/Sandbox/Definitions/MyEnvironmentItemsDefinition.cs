namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_EnvironmentItemsDefinition), (Type) null)]
    public class MyEnvironmentItemsDefinition : MyDefinitionBase
    {
        private HashSet<MyStringHash> m_itemDefinitions;
        private List<MyStringHash> m_definitionList;
        private List<float> Frequencies;
        private float[] Intervals;
        private MyObjectBuilderType m_itemDefinitionType = MyObjectBuilderType.Invalid;

        public void AddItemDefinition(MyStringHash definition, float frequency, bool recompute = true)
        {
            if (!this.m_itemDefinitions.Contains(definition))
            {
                this.m_itemDefinitions.Add(definition);
                this.m_definitionList.Add(definition);
                this.Frequencies.Add(frequency);
                if (recompute)
                {
                    this.RecomputeFrequencies();
                }
            }
        }

        public bool ContainsItemDefinition(MyEnvironmentItemDefinition itemDefinition) => 
            this.ContainsItemDefinition(itemDefinition.Id);

        public bool ContainsItemDefinition(MyDefinitionId definitionId) => 
            ((definitionId.TypeId == this.m_itemDefinitionType) && this.m_itemDefinitions.Contains(definitionId.SubtypeId));

        public bool ContainsItemDefinition(MyStringHash subtypeId) => 
            this.m_itemDefinitions.Contains(subtypeId);

        public MyEnvironmentItemDefinition GetItemDefinition(int index)
        {
            if ((index < 0) || (index >= this.m_definitionList.Count))
            {
                return null;
            }
            return this.GetItemDefinition(this.m_definitionList[index]);
        }

        public MyEnvironmentItemDefinition GetItemDefinition(MyStringHash subtypeId)
        {
            MyEnvironmentItemDefinition definition = null;
            MyDefinitionId defId = new MyDefinitionId(this.m_itemDefinitionType, subtypeId);
            MyDefinitionManager.Static.TryGetDefinition<MyEnvironmentItemDefinition>(defId, out definition);
            return definition;
        }

        public MyEnvironmentItemDefinition GetRandomItemDefinition()
        {
            if (this.m_definitionList.Count == 0)
            {
                return null;
            }
            float num = ((float) MyRandom.Instance.Next(0, 0x10000)) / 65536f;
            return this.GetItemDefinition(this.m_definitionList[this.Intervals.BinaryIntervalSearch<float>(num)]);
        }

        public MyEnvironmentItemDefinition GetRandomItemDefinition(MyRandom instance)
        {
            if (this.m_definitionList.Count == 0)
            {
                return null;
            }
            float num = ((float) instance.Next(0, 0x10000)) / 65536f;
            return this.GetItemDefinition(this.m_definitionList[this.Intervals.BinaryIntervalSearch<float>(num)]);
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EnvironmentItemsDefinition definition = builder as MyObjectBuilder_EnvironmentItemsDefinition;
            this.m_itemDefinitions = new HashSet<MyStringHash>(MyStringHash.Comparer);
            this.m_definitionList = new List<MyStringHash>();
            object[] customAttributes = builder.Id.TypeId.GetCustomAttributes(typeof(MyEnvironmentItemsAttribute), false);
            if (customAttributes.Length != 1)
            {
                this.m_itemDefinitionType = typeof(MyObjectBuilder_EnvironmentItemDefinition);
            }
            else
            {
                MyEnvironmentItemsAttribute attribute = customAttributes[0] as MyEnvironmentItemsAttribute;
                this.m_itemDefinitionType = attribute.ItemDefinitionType;
            }
            this.Channel = definition.Channel;
            this.MaxViewDistance = definition.MaxViewDistance;
            this.SectorSize = definition.SectorSize;
            this.ItemSize = definition.ItemSize;
            this.Material = MyStringHash.GetOrCompute(definition.PhysicalMaterial);
            this.Frequencies = new List<float>();
        }

        public void RecomputeFrequencies()
        {
            if (this.m_definitionList.Count == 0)
            {
                this.Intervals = null;
            }
            else
            {
                this.Intervals = new float[this.m_definitionList.Count - 1];
                float num = 0f;
                foreach (float num3 in this.Frequencies)
                {
                    num += num3;
                }
                float num2 = 0f;
                for (int i = 0; i < this.Intervals.Length; i++)
                {
                    num2 += this.Frequencies[i];
                    this.Intervals[i] = num2 / num;
                }
            }
        }

        public MyObjectBuilderType ItemDefinitionType =>
            this.m_itemDefinitionType;

        public int Channel { get; private set; }

        public float MaxViewDistance { get; private set; }

        public float SectorSize { get; private set; }

        public float ItemSize { get; private set; }

        public MyStringHash Material { get; private set; }

        public int ItemDefinitionCount =>
            this.m_definitionList.Count;
    }
}

