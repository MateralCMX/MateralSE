namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ReactorDefinition), (Type) null)]
    public class MyReactorDefinition : MyFueledPowerProducerDefinition
    {
        public Vector3 InventorySize;
        public float InventoryMaxVolume;
        public MyInventoryConstraint InventoryConstraint;
        public FuelInfo[] FuelInfos;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ReactorDefinition definition = builder as MyObjectBuilder_ReactorDefinition;
            this.InventorySize = definition.InventorySize;
            this.InventoryMaxVolume = (this.InventorySize.X * this.InventorySize.Y) * this.InventorySize.Z;
            List<MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo> fuelInfos = definition.FuelInfos;
            if (definition.FuelId != null)
            {
                MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo item = new MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo();
                item.Ratio = 1f;
                item.Id = definition.FuelId.Value;
                List<MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo> list1 = new List<MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo>(fuelInfos);
                list1.Add(item);
                fuelInfos = list1;
            }
            this.FuelInfos = new FuelInfo[fuelInfos.Count];
            this.InventoryConstraint = new MyInventoryConstraint(string.Format(MyTexts.GetString(MySpaceTexts.ToolTipItemFilter_GenericProductionBlockInput), this.DisplayNameText), null, true);
            for (int i = 0; i < fuelInfos.Count; i++)
            {
                MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo fuelInfo = fuelInfos[i];
                this.InventoryConstraint.Add(fuelInfo.Id);
                this.FuelInfos[i] = new FuelInfo(fuelInfo, this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FuelInfo
        {
            public readonly float Ratio;
            public readonly MyDefinitionId FuelId;
            public readonly float ConsumptionPerSecond_Items;
            public readonly MyPhysicalItemDefinition FuelDefinition;
            public readonly MyObjectBuilder_PhysicalObject FuelItem;
            public FuelInfo(MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo fuelInfo, MyReactorDefinition blockDefinition)
            {
                this.FuelId = fuelInfo.Id;
                this.Ratio = fuelInfo.Ratio;
                this.FuelDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(fuelInfo.Id);
                this.FuelItem = MyObjectBuilderSerializer.CreateNewObject(fuelInfo.Id) as MyObjectBuilder_PhysicalObject;
                float num = (blockDefinition.MaxPowerOutput / blockDefinition.FuelProductionToCapacityMultiplier) * this.Ratio;
                this.ConsumptionPerSecond_Items = num / this.FuelDefinition.Mass;
            }
        }
    }
}

