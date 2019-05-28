namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_WeaponBlockDefinition), (Type) null)]
    public class MyWeaponBlockDefinition : MyCubeBlockDefinition
    {
        public MyDefinitionId WeaponDefinitionId;
        public MyStringHash ResourceSinkGroup;
        public float InventoryMaxVolume;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WeaponBlockDefinition definition = builder as MyObjectBuilder_WeaponBlockDefinition;
            this.WeaponDefinitionId = new MyDefinitionId(definition.WeaponDefinitionId.Type, definition.WeaponDefinitionId.Subtype);
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.InventoryMaxVolume = definition.InventoryMaxVolume;
        }
    }
}

