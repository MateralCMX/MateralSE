namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_OxygenGeneratorDefinition), (Type) null)]
    public class MyOxygenGeneratorDefinition : MyProductionBlockDefinition
    {
        public float IceConsumptionPerSecond;
        public MySoundPair GenerateSound;
        public MySoundPair IdleSound;
        public MyStringHash ResourceSourceGroup;
        public List<MyGasGeneratorResourceInfo> ProducedGases;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_OxygenGeneratorDefinition definition = builder as MyObjectBuilder_OxygenGeneratorDefinition;
            this.IceConsumptionPerSecond = definition.IceConsumptionPerSecond;
            this.GenerateSound = new MySoundPair(definition.GenerateSound, true);
            this.IdleSound = new MySoundPair(definition.IdleSound, true);
            this.ResourceSourceGroup = MyStringHash.GetOrCompute(definition.ResourceSourceGroup);
            this.ProducedGases = null;
            if (definition.ProducedGases != null)
            {
                this.ProducedGases = new List<MyGasGeneratorResourceInfo>(definition.ProducedGases.Count);
                foreach (MyObjectBuilder_GasGeneratorResourceInfo info in definition.ProducedGases)
                {
                    MyGasGeneratorResourceInfo item = new MyGasGeneratorResourceInfo {
                        Id = info.Id,
                        IceToGasRatio = info.IceToGasRatio
                    };
                    this.ProducedGases.Add(item);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyGasGeneratorResourceInfo
        {
            public MyDefinitionId Id;
            public float IceToGasRatio;
        }
    }
}

