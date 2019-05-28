namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Cube;
    using VRage.Game;

    [MyCubeBlockType(typeof(MyObjectBuilder_DebugSphere3))]
    internal class MyDebugSphere3 : MyFunctionalBlock
    {
        private MyDebugSphere3Definition BlockDefinition =>
            ((MyDebugSphere3Definition) base.BlockDefinition);
    }
}

