namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Library.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ContainerTypeDefinition), (Type) null)]
    public class MyContainerTypeDefinition : MyDefinitionBase
    {
        public int CountMin;
        public int CountMax;
        public float ItemsCumulativeFrequency;
        private float m_tempCumulativeFreq;
        public ContainerTypeItem[] Items;
        private bool[] m_itemSelection;

        public void DeselectAll()
        {
            for (int i = 0; i < this.Items.Length; i++)
            {
                this.m_itemSelection[i] = false;
            }
            this.m_tempCumulativeFreq = this.ItemsCumulativeFrequency;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ContainerTypeDefinition definition = builder as MyObjectBuilder_ContainerTypeDefinition;
            this.CountMin = definition.CountMin;
            this.CountMax = definition.CountMax;
            this.ItemsCumulativeFrequency = 0f;
            int index = 0;
            this.Items = new ContainerTypeItem[definition.Items.Length];
            this.m_itemSelection = new bool[definition.Items.Length];
            foreach (MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem item in definition.Items)
            {
                ContainerTypeItem item2 = new ContainerTypeItem {
                    AmountMax = MyFixedPoint.DeserializeStringSafe(item.AmountMax),
                    AmountMin = MyFixedPoint.DeserializeStringSafe(item.AmountMin),
                    Frequency = Math.Max(item.Frequency, 0f),
                    DefinitionId = item.Id
                };
                this.ItemsCumulativeFrequency += item2.Frequency;
                this.Items[index] = item2;
                this.m_itemSelection[index] = false;
                index++;
            }
            this.m_tempCumulativeFreq = this.ItemsCumulativeFrequency;
        }

        public ContainerTypeItem SelectNextRandomItem()
        {
            float num = MyRandom.Instance.NextFloat(0f, this.m_tempCumulativeFreq);
            int index = 0;
            while (true)
            {
                if (index < (this.Items.Length - 1))
                {
                    if (this.m_itemSelection[index])
                    {
                        index++;
                        continue;
                    }
                    if ((num - this.Items[index].Frequency) >= 0f)
                    {
                        index++;
                        continue;
                    }
                }
                this.m_tempCumulativeFreq -= this.Items[index].Frequency;
                this.m_itemSelection[index] = true;
                return this.Items[index];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ContainerTypeItem
        {
            public MyFixedPoint AmountMin;
            public MyFixedPoint AmountMax;
            public float Frequency;
            public MyDefinitionId DefinitionId;
            public bool HasIntegralAmount;
        }
    }
}

