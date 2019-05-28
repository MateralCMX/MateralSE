namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Cube;

    [MyCubeBlockType(typeof(MyObjectBuilder_Planter))]
    public class MyPlanter : MyCubeBlock
    {
    }
}

