namespace VRage.Definitions.Components
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders.Definitions.Components;

    [MyDefinitionType(typeof(MyObjectBuilder_VoxelMesherComponentDefinition), (Type) null)]
    public class MyVoxelMesherComponentDefinition : MyDefinitionBase
    {
        public List<MyObjectBuilder_VoxelPostprocessing> PostProcessingSteps = new List<MyObjectBuilder_VoxelPostprocessing>();

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder() => 
            ((MyObjectBuilder_VoxelMesherComponentDefinition) base.GetObjectBuilder());

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_VoxelMesherComponentDefinition definition = (MyObjectBuilder_VoxelMesherComponentDefinition) builder;
            if (definition.PostprocessingSteps != null)
            {
                this.PostProcessingSteps = definition.PostprocessingSteps;
            }
        }
    }
}

