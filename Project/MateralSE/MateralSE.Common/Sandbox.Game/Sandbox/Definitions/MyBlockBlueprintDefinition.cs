namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BlockBlueprintDefinition), (Type) null)]
    public class MyBlockBlueprintDefinition : MyBlueprintDefinition
    {
        protected override void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
        }

        public override void Postprocess()
        {
            base.Atomic = false;
            float num = 0f;
            foreach (MyBlueprintDefinitionBase.Item item in base.Results)
            {
                MyCubeBlockDefinition definition;
                MyDefinitionManager.Static.TryGetCubeBlockDefinition(item.Id, out definition);
                if (definition == null)
                {
                    return;
                }
                num += ((float) item.Amount) * definition.Mass;
            }
            base.OutputVolume = num;
            base.PostprocessNeeded = false;
        }
    }
}

