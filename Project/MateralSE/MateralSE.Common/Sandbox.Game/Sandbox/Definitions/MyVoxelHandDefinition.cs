namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_VoxelHandDefinition), (Type) null)]
    public class MyVoxelHandDefinition : MyDefinitionBase
    {
        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
        }
    }
}

