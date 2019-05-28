namespace Sandbox.Definitions
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_RefineryDefinition), (Type) null)]
    public class MyRefineryDefinition : MyProductionBlockDefinition
    {
        public float RefineSpeed;
        public float MaterialEfficiency;
        public MyFixedPoint? OreAmountPerPullRequest;

        protected override bool BlueprintClassCanBeUsed(MyBlueprintClassDefinition blueprintClass)
        {
            using (IEnumerator<MyBlueprintDefinitionBase> enumerator = blueprintClass.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyBlueprintDefinitionBase current = enumerator.Current;
                    if (current.Atomic)
                    {
                        MySandboxGame.Log.WriteLine("Blueprint " + current.DisplayNameText + " is atomic, but it is in a class used by refinery block");
                        return false;
                    }
                }
            }
            return base.BlueprintClassCanBeUsed(blueprintClass);
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_RefineryDefinition definition = builder as MyObjectBuilder_RefineryDefinition;
            this.RefineSpeed = definition.RefineSpeed;
            this.MaterialEfficiency = definition.MaterialEfficiency;
            this.OreAmountPerPullRequest = definition.OreAmountPerPullRequest;
        }

        protected override void InitializeLegacyBlueprintClasses(MyObjectBuilder_ProductionBlockDefinition ob)
        {
            ob.BlueprintClasses = new string[] { "Ingots" };
        }
    }
}

