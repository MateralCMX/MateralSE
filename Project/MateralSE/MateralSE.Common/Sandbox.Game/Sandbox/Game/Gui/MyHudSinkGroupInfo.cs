namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using System;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Utils;

    public class MyHudSinkGroupInfo
    {
        private bool m_needsRefresh = true;
        private float[] m_missingPowerByGroup;
        private MyStringId[] m_groupNames;
        private float m_missingTotal;
        public int GroupCount;
        private int m_workingGroupCount;
        private bool m_visible;
        private MyHudNameValueData m_data;

        public MyHudSinkGroupInfo()
        {
            this.Reload();
        }

        private void Refresh()
        {
            this.m_needsRefresh = false;
            MyHudNameValueData data = this.Data;
            for (int i = 0; i < (data.Count - 1); i++)
            {
                data[i].Name.Clear().AppendStringBuilder(MyTexts.Get(this.m_groupNames[i]));
            }
            data[this.GroupCount].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudEnergyMissingTotal));
            Sandbox.Game.Gui.MyHudNameValueData.Data data2 = data[this.GroupCount];
            data2.Value.Clear();
            MyValueFormatter.AppendWorkInBestUnit(-this.m_missingTotal, data2.Value);
            for (int j = 0; j < this.GroupCount; j++)
            {
                data2 = data[j];
                if (j < this.m_workingGroupCount)
                {
                    string str;
                    data2.ValueFont = (string) (str = null);
                    data2.NameFont = str;
                }
                else
                {
                    data2.NameFont = data2.ValueFont = "Red";
                }
                data2.Value.Clear();
                MyValueFormatter.AppendWorkInBestUnit(-this.m_missingPowerByGroup[j], data2.Value);
            }
        }

        public void Reload()
        {
            MyResourceDistributorComponent.InitializeMappings();
            if ((MyResourceDistributorComponent.SinkGroupPrioritiesTotal != -1) && ((this.m_groupNames == null) || (this.m_groupNames.Length < MyResourceDistributorComponent.SinkGroupPrioritiesTotal)))
            {
                this.GroupCount = MyResourceDistributorComponent.SinkGroupPrioritiesTotal;
                this.WorkingGroupCount = this.GroupCount;
                this.m_groupNames = new MyStringId[this.GroupCount];
                this.m_missingPowerByGroup = new float[this.GroupCount];
                this.m_data = new MyHudNameValueData(this.GroupCount + 1, "Blue", "White", 0.025f, true);
            }
            if (this.m_groupNames != null)
            {
                ListReader<MyResourceDistributionGroupDefinition> definitionsOfType = MyDefinitionManager.Static.GetDefinitionsOfType<MyResourceDistributionGroupDefinition>();
                DictionaryReader<MyStringHash, int> sinkSubtypesToPriority = MyResourceDistributorComponent.SinkSubtypesToPriority;
                foreach (MyResourceDistributionGroupDefinition definition in definitionsOfType)
                {
                    int num;
                    if (definition.IsSource)
                    {
                        continue;
                    }
                    if (sinkSubtypesToPriority.TryGetValue(definition.Id.SubtypeId, out num) && (num < this.GroupCount))
                    {
                        this.m_groupNames[num] = MyStringId.GetOrCompute(definition.Id.SubtypeName);
                    }
                }
                this.Data[this.GroupCount].NameFont = "Red";
                this.Data[this.GroupCount].ValueFont = "Red";
            }
        }

        internal void SetGroupDeficit(int groupIndex, float missingPower)
        {
            if (this.m_missingPowerByGroup == null)
            {
                this.Reload();
            }
            this.m_missingTotal += missingPower - this.m_missingPowerByGroup[groupIndex];
            this.m_missingPowerByGroup[groupIndex] = missingPower;
            this.m_needsRefresh = true;
        }

        public int WorkingGroupCount
        {
            get => 
                this.m_workingGroupCount;
            set
            {
                if (this.m_workingGroupCount != value)
                {
                    this.m_workingGroupCount = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public bool Visible
        {
            get => 
                (this.m_visible && (this.WorkingGroupCount != this.GroupCount));
            set => 
                (this.m_visible = value);
        }

        public MyHudNameValueData Data
        {
            get
            {
                if (this.m_needsRefresh)
                {
                    this.Refresh();
                }
                return this.m_data;
            }
        }
    }
}

