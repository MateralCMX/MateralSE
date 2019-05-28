namespace Sandbox.Game.Definitions
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_CubeBlockStackSizeDefinition), (Type) null)]
    public class MyCubeBlockStackSizeDefinition : MyDefinitionBase
    {
        public Dictionary<MyDefinitionId, MyFixedPoint> BlockMaxStackSizes = new Dictionary<MyDefinitionId, MyFixedPoint>(MyDefinitionId.Comparer);

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CubeBlockStackSizeDefinition definition = builder as MyObjectBuilder_CubeBlockStackSizeDefinition;
            if ((definition != null) && (definition.Blocks != null))
            {
                foreach (MyObjectBuilder_CubeBlockStackSizeDefinition.BlockStackSizeDef def in definition.Blocks)
                {
                    if (def.TypeId == null)
                    {
                        string msg = "\"TypeId\" must be defined in a block item for " + builder.Id;
                        MySandboxGame.Log.WriteLine(msg);
                        throw new ArgumentException(msg);
                    }
                    MyObjectBuilderType type = MyObjectBuilderType.Parse(def.TypeId);
                    if (def.SubtypeId == null)
                    {
                        string msg = "\"SubtypeId\" must be defined in a block item for " + builder.Id;
                        MySandboxGame.Log.WriteLine(msg);
                        throw new ArgumentException(msg);
                    }
                    this.BlockMaxStackSizes.Add(new MyDefinitionId(type, def.SubtypeId), def.MaxStackSize);
                }
            }
        }
    }
}

