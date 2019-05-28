namespace Sandbox.Definitions
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_PhysicalModelDefinition), typeof(MyPhysicalModelDefinition.Postprocessor))]
    public class MyPhysicalModelDefinition : MyDefinitionBase
    {
        public string Model;
        public MyPhysicalMaterialDefinition PhysicalMaterial;
        public float Mass;
        private string m_material;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PhysicalModelDefinition definition = builder as MyObjectBuilder_PhysicalModelDefinition;
            this.Model = definition.Model;
            if ((base.GetType() == typeof(MyCubeBlockDefinition)) || base.GetType().IsSubclassOf(typeof(MyCubeBlockDefinition)))
            {
                this.PhysicalMaterial = MyDestructionData.GetPhysicalMaterial(this, definition.PhysicalMaterial);
            }
            else
            {
                this.m_material = definition.PhysicalMaterial;
            }
            this.Mass = definition.Mass;
        }

        protected class Postprocessor : MyDefinitionPostprocessor
        {
            public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
            {
            }

            public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
            {
                foreach (MyPhysicalModelDefinition local1 in definitions.Values.Cast<MyPhysicalModelDefinition>())
                {
                    local1.PhysicalMaterial = MyDestructionData.GetPhysicalMaterial(local1, local1.m_material);
                }
            }
        }
    }
}

