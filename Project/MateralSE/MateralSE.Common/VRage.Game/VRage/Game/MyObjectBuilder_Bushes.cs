namespace VRage.Game
{
    using ProtoBuf;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition(typeof(MyObjectBuilder_DestroyableItems), null), MyEnvironmentItems(typeof(MyObjectBuilder_DestroyableItem))]
    public class MyObjectBuilder_Bushes : MyObjectBuilder_EnvironmentItems
    {
    }
}

