namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_VoxelMaterialModifierDefinition), typeof(MyVoxelMaterialModifierDefinition.Postprocessor))]
    public class MyVoxelMaterialModifierDefinition : MyDefinitionBase
    {
        public MyDiscreteSampler<VoxelMapChange> Options;
        private MyObjectBuilder_VoxelMaterialModifierDefinition m_ob;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            this.m_ob = (MyObjectBuilder_VoxelMaterialModifierDefinition) builder;
        }

        private class Postprocessor : MyDefinitionPostprocessor
        {
            public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
            {
            }

            public override unsafe void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
            {
                foreach (MyVoxelMaterialModifierDefinition definition in definitions.Values)
                {
                    definition.Options = new MyDiscreteSampler<VoxelMapChange>(definition.m_ob.Options.Select<MyVoxelMapModifierOption, VoxelMapChange>(delegate (MyVoxelMapModifierOption x) {
                        VoxelMapChange* changePtr1;
                        VoxelMapChange change = new VoxelMapChange();
                        changePtr1->Changes = (x.Changes == null) ? null : x.Changes.ToDictionary<MyVoxelMapModifierChange, byte, byte>(y => MyDefinitionManager.Static.GetVoxelMaterialDefinition(y.From).Index, y => MyDefinitionManager.Static.GetVoxelMaterialDefinition(y.To).Index);
                        changePtr1 = (VoxelMapChange*) ref change;
                        return change;
                    }), from x in definition.m_ob.Options select x.Chance);
                    definition.m_ob = null;
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyVoxelMaterialModifierDefinition.Postprocessor.<>c <>9 = new MyVoxelMaterialModifierDefinition.Postprocessor.<>c();
                public static Func<MyVoxelMapModifierChange, byte> <>9__1_2;
                public static Func<MyVoxelMapModifierChange, byte> <>9__1_3;
                public static Func<MyVoxelMapModifierOption, VoxelMapChange> <>9__1_0;
                public static Func<MyVoxelMapModifierOption, float> <>9__1_1;

                internal unsafe VoxelMapChange <AfterPostprocess>b__1_0(MyVoxelMapModifierOption x)
                {
                    VoxelMapChange* changePtr1;
                    VoxelMapChange change = new VoxelMapChange();
                    changePtr1->Changes = (x.Changes == null) ? null : x.Changes.ToDictionary<MyVoxelMapModifierChange, byte, byte>(y => MyDefinitionManager.Static.GetVoxelMaterialDefinition(y.From).Index, y => MyDefinitionManager.Static.GetVoxelMaterialDefinition(y.To).Index);
                    changePtr1 = (VoxelMapChange*) ref change;
                    return change;
                }

                internal float <AfterPostprocess>b__1_1(MyVoxelMapModifierOption x) => 
                    x.Chance;

                internal byte <AfterPostprocess>b__1_2(MyVoxelMapModifierChange y) => 
                    MyDefinitionManager.Static.GetVoxelMaterialDefinition(y.From).Index;

                internal byte <AfterPostprocess>b__1_3(MyVoxelMapModifierChange y) => 
                    MyDefinitionManager.Static.GetVoxelMaterialDefinition(y.To).Index;
            }
        }
    }
}

