namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_WeaponItemDefinition), (Type) null)]
    public class MyWeaponItemDefinition : MyPhysicalItemDefinition
    {
        public MyDefinitionId WeaponDefinitionId;
        public bool ShowAmmoCount;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WeaponItemDefinition definition = builder as MyObjectBuilder_WeaponItemDefinition;
            this.WeaponDefinitionId = new MyDefinitionId(definition.WeaponDefinitionId.Type, definition.WeaponDefinitionId.Subtype);
            this.ShowAmmoCount = definition.ShowAmmoCount;
        }
    }
}

