namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_RepairBlueprintDefinition), (Type) null)]
    public class MyRepairBlueprintDefinition : MyBlueprintDefinition
    {
        public float RepairAmount;

        protected override void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
            MyObjectBuilder_RepairBlueprintDefinition definition = ob as MyObjectBuilder_RepairBlueprintDefinition;
            this.RepairAmount = definition.RepairAmount;
        }
    }
}

