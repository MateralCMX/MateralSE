namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_GasFueledPowerProducerDefinition), (Type) null)]
    public class MyGasFueledPowerProducerDefinition : MyFueledPowerProducerDefinition
    {
        public FuelInfo Fuel;
        public float FuelCapacity;
        public MyStringHash ResourceSinkGroup;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyObjectBuilder_GasFueledPowerProducerDefinition definition = (MyObjectBuilder_GasFueledPowerProducerDefinition) builder;
            base.Init(builder);
            this.FuelCapacity = definition.FuelCapacity;
            this.Fuel = new FuelInfo(definition.FuelInfos[0]);
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FuelInfo
        {
            public readonly float Ratio;
            public readonly MyDefinitionId FuelId;
            public FuelInfo(MyObjectBuilder_FueledPowerProducerDefinition.FuelInfo fuelInfo)
            {
                this.FuelId = fuelInfo.Id;
                this.Ratio = fuelInfo.Ratio;
            }
        }
    }
}

