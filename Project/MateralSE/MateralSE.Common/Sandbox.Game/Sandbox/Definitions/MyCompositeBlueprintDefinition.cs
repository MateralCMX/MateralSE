namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_CompositeBlueprintDefinition), (Type) null)]
    public class MyCompositeBlueprintDefinition : MyBlueprintDefinitionBase
    {
        private MyBlueprintDefinitionBase[] m_blueprints;
        private MyBlueprintDefinitionBase.Item[] m_items;
        private static List<MyBlueprintDefinitionBase.Item> m_tmpPrerequisiteList = new List<MyBlueprintDefinitionBase.Item>();
        private static List<MyBlueprintDefinitionBase.Item> m_tmpResultList = new List<MyBlueprintDefinitionBase.Item>();

        private unsafe void AddToItemList(List<MyBlueprintDefinitionBase.Item> items, MyBlueprintDefinitionBase.Item toAdd)
        {
            int num = 0;
            MyBlueprintDefinitionBase.Item item = new MyBlueprintDefinitionBase.Item();
            num = 0;
            while (true)
            {
                if (num < items.Count)
                {
                    item = items[num];
                    if (item.Id != toAdd.Id)
                    {
                        num++;
                        continue;
                    }
                }
                if (num >= items.Count)
                {
                    items.Add(toAdd);
                    return;
                }
                MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item.Amount;
                pointPtr1[0] += toAdd.Amount;
                items[num] = item;
                return;
            }
        }

        public override unsafe int GetBlueprints(List<MyBlueprintDefinitionBase.ProductionInfo> blueprints)
        {
            int num = 0;
            int index = 0;
            while (index < this.m_blueprints.Length)
            {
                int num3 = this.m_blueprints[index].GetBlueprints(blueprints);
                int count = blueprints.Count;
                int num5 = count - 1;
                while (true)
                {
                    if (num5 < (count - num3))
                    {
                        num += num3;
                        index++;
                        break;
                    }
                    MyBlueprintDefinitionBase.ProductionInfo info = blueprints[num5];
                    MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref info.Amount;
                    pointPtr1[0] *= this.m_items[index].Amount;
                    blueprints[num5] = info;
                    num5--;
                }
            }
            return num;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CompositeBlueprintDefinition definition = builder as MyObjectBuilder_CompositeBlueprintDefinition;
            this.m_items = new MyBlueprintDefinitionBase.Item[(definition.Blueprints == null) ? 0 : definition.Blueprints.Length];
            for (int i = 0; i < this.m_items.Length; i++)
            {
                this.m_items[i] = MyBlueprintDefinitionBase.Item.FromObjectBuilder(definition.Blueprints[i]);
            }
            base.PostprocessNeeded = true;
        }

        public override void Postprocess()
        {
            foreach (MyBlueprintDefinitionBase.Item item in this.m_items)
            {
                if (!MyDefinitionManager.Static.HasBlueprint(item.Id))
                {
                    return;
                }
                if (MyDefinitionManager.Static.GetBlueprintDefinition(item.Id).PostprocessNeeded)
                {
                    return;
                }
            }
            float num = 0f;
            bool flag = false;
            float num2 = 0f;
            this.m_blueprints = new MyBlueprintDefinitionBase[this.m_items.Length];
            m_tmpPrerequisiteList.Clear();
            m_tmpResultList.Clear();
            for (int i = 0; i < this.m_items.Length; i++)
            {
                MyFixedPoint amount = this.m_items[i].Amount;
                MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(this.m_items[i].Id);
                this.m_blueprints[i] = blueprintDefinition;
                flag = flag || blueprintDefinition.Atomic;
                num += blueprintDefinition.OutputVolume * ((float) amount);
                num2 += blueprintDefinition.BaseProductionTimeInSeconds * ((float) amount);
                this.PostprocessAddSubblueprint(blueprintDefinition, amount);
            }
            base.Prerequisites = m_tmpPrerequisiteList.ToArray();
            base.Results = m_tmpResultList.ToArray();
            m_tmpPrerequisiteList.Clear();
            m_tmpResultList.Clear();
            base.Atomic = flag;
            base.OutputVolume = num;
            base.BaseProductionTimeInSeconds = num2;
            base.PostprocessNeeded = false;
        }

        private unsafe void PostprocessAddSubblueprint(MyBlueprintDefinitionBase blueprint, MyFixedPoint blueprintAmount)
        {
            for (int i = 0; i < blueprint.Prerequisites.Length; i++)
            {
                MyBlueprintDefinitionBase.Item toAdd = blueprint.Prerequisites[i];
                MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref toAdd.Amount;
                pointPtr1[0] *= blueprintAmount;
                this.AddToItemList(m_tmpPrerequisiteList, toAdd);
            }
            for (int j = 0; j < blueprint.Results.Length; j++)
            {
                MyBlueprintDefinitionBase.Item toAdd = blueprint.Results[j];
                MyFixedPoint* pointPtr2 = (MyFixedPoint*) ref toAdd.Amount;
                pointPtr2[0] *= blueprintAmount;
                this.AddToItemList(m_tmpResultList, toAdd);
            }
        }
    }
}

