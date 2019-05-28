namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BlueprintDefinition), (Type) null)]
    public class MyBlueprintDefinition : MyBlueprintDefinitionBase
    {
        public override int GetBlueprints(List<MyBlueprintDefinitionBase.ProductionInfo> blueprints)
        {
            MyBlueprintDefinitionBase.ProductionInfo item = new MyBlueprintDefinitionBase.ProductionInfo {
                Blueprint = this,
                Amount = 1
            };
            blueprints.Add(item);
            return 1;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
            MyObjectBuilder_BlueprintDefinition definition = (MyObjectBuilder_BlueprintDefinition) ob;
            base.Prerequisites = new MyBlueprintDefinitionBase.Item[definition.Prerequisites.Length];
            for (int i = 0; i < base.Prerequisites.Length; i++)
            {
                base.Prerequisites[i] = MyBlueprintDefinitionBase.Item.FromObjectBuilder(definition.Prerequisites[i]);
            }
            if (definition.Result != null)
            {
                base.Results = new MyBlueprintDefinitionBase.Item[] { MyBlueprintDefinitionBase.Item.FromObjectBuilder(definition.Result) };
            }
            else
            {
                base.Results = new MyBlueprintDefinitionBase.Item[definition.Results.Length];
                for (int j = 0; j < base.Results.Length; j++)
                {
                    base.Results[j] = MyBlueprintDefinitionBase.Item.FromObjectBuilder(definition.Results[j]);
                }
            }
            base.BaseProductionTimeInSeconds = definition.BaseProductionTimeInSeconds;
            base.PostprocessNeeded = true;
            base.ProgressBarSoundCue = definition.ProgressBarSoundCue;
            base.IsPrimary = definition.IsPrimary;
        }

        public override void Postprocess()
        {
            bool flag = false;
            float num = 0f;
            foreach (MyBlueprintDefinitionBase.Item item in base.Results)
            {
                MyPhysicalItemDefinition definition;
                if ((item.Id.TypeId != typeof(MyObjectBuilder_Ore)) && (item.Id.TypeId != typeof(MyObjectBuilder_Ingot)))
                {
                    flag = true;
                }
                MyDefinitionManager.Static.TryGetPhysicalItemDefinition(item.Id, out definition);
                if (definition == null)
                {
                    return;
                }
                num += ((float) item.Amount) * definition.Volume;
            }
            base.Atomic = flag;
            base.OutputVolume = num;
            base.PostprocessNeeded = false;
        }
    }
}

