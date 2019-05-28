namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Cube;
    using VRage.Game;

    [MyCubeBlockType(typeof(MyObjectBuilder_DebugSphere2))]
    internal class MyDebugSphere2 : MyFunctionalBlock
    {
        private MyDebugSphere2Definition BlockDefinition =>
            ((MyDebugSphere2Definition) base.BlockDefinition);
    }
}

