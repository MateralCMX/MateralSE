namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.SessionComponents;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Utils;

    public class MyHudBlockInfo
    {
        public bool ShowDetails;
        public List<ComponentInfo> Components = new List<ComponentInfo>(12);
        public string BlockName;
        private string m_contextHelp;
        public string[] BlockIcons;
        public float BlockIntegrity;
        public float CriticalIntegrity;
        public float OwnershipIntegrity;
        public bool ShowAvailable;
        public int CriticalComponentIndex = -1;
        public int MissingComponentIndex = -1;
        public int PCUCost;
        public long BlockBuiltBy;
        public MyCubeSize GridSize;
        public bool Visible;

        public void SetContextHelp(MyDefinitionBase definition)
        {
            if (string.IsNullOrEmpty(definition.DescriptionText))
            {
                this.m_contextHelp = MyTexts.GetString(MySpaceTexts.Description_NotAvailable);
            }
            else if (string.IsNullOrEmpty(definition.DescriptionArgs))
            {
                this.m_contextHelp = definition.DescriptionText;
            }
            else
            {
                char[] separator = new char[] { ',' };
                string[] strArray = definition.DescriptionArgs.Split(separator);
                object[] args = new object[strArray.Length];
                for (int i = 0; i < strArray.Length; i++)
                {
                    args[i] = MyIngameHelpObjective.GetHighlightedControl(MyStringId.GetOrCompute(strArray[i]));
                }
                this.m_contextHelp = string.Format(definition.DescriptionText, args);
            }
        }

        public string ContextHelp =>
            this.m_contextHelp;

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentInfo
        {
            public MyDefinitionId DefinitionId;
            public string[] Icons;
            public string ComponentName;
            public int MountedCount;
            public int StockpileCount;
            public int TotalCount;
            public int AvailableAmount;
            public int InstalledCount =>
                (this.MountedCount + this.StockpileCount);
            public override string ToString() => 
                $"{this.MountedCount}/{this.StockpileCount}/{this.TotalCount} {this.ComponentName}";
        }
    }
}

