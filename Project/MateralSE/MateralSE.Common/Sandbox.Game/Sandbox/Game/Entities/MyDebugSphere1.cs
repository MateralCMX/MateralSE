namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Cube;
    using VRage.Game;

    [MyCubeBlockType(typeof(MyObjectBuilder_DebugSphere1))]
    internal class MyDebugSphere1 : MyFunctionalBlock
    {
        private MyDebugSphere1Definition BlockDefinition =>
            ((MyDebugSphere1Definition) base.BlockDefinition);
    }
}

