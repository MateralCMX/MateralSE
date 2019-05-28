namespace Sandbox.Definitions
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ProductionBlockDefinition), (Type) null)]
    public class MyProductionBlockDefinition : MyCubeBlockDefinition
    {
        public float InventoryMaxVolume;
        public Vector3 InventorySize;
        public MyStringHash ResourceSinkGroup;
        public float StandbyPowerConsumption;
        public float OperationalPowerConsumption;
        public List<MyBlueprintClassDefinition> BlueprintClasses;
        public MyInventoryConstraint InputInventoryConstraint;
        public MyInventoryConstraint OutputInventoryConstraint;

        protected virtual bool BlueprintClassCanBeUsed(MyBlueprintClassDefinition blueprintClass) => 
            true;

        protected virtual List<MyBlueprintClassDefinition> GetInputClasses() => 
            this.BlueprintClasses;

        protected virtual List<MyBlueprintClassDefinition> GetOutputClasses() => 
            this.BlueprintClasses;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ProductionBlockDefinition ob = builder as MyObjectBuilder_ProductionBlockDefinition;
            this.InventoryMaxVolume = ob.InventoryMaxVolume;
            this.InventorySize = ob.InventorySize;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(ob.ResourceSinkGroup);
            this.StandbyPowerConsumption = ob.StandbyPowerConsumption;
            this.OperationalPowerConsumption = ob.OperationalPowerConsumption;
            if (ob.BlueprintClasses == null)
            {
                this.InitializeLegacyBlueprintClasses(ob);
            }
            this.BlueprintClasses = new List<MyBlueprintClassDefinition>();
            foreach (string str in ob.BlueprintClasses)
            {
                MyBlueprintClassDefinition blueprintClass = MyDefinitionManager.Static.GetBlueprintClass(str);
                if (blueprintClass != null)
                {
                    this.BlueprintClasses.Add(blueprintClass);
                }
            }
        }

        protected virtual void InitializeLegacyBlueprintClasses(MyObjectBuilder_ProductionBlockDefinition ob)
        {
            ob.BlueprintClasses = new string[0];
        }

        public void LoadPostProcess()
        {
            int index = 0;
            while (index < this.BlueprintClasses.Count)
            {
                if (!this.BlueprintClassCanBeUsed(this.BlueprintClasses[index]))
                {
                    this.BlueprintClasses.RemoveAt(index);
                    continue;
                }
                index++;
            }
            this.InputInventoryConstraint = this.PrepareConstraint(MySpaceTexts.ToolTipItemFilter_GenericProductionBlockInput, this.GetInputClasses(), true);
            this.OutputInventoryConstraint = this.PrepareConstraint(MySpaceTexts.ToolTipItemFilter_GenericProductionBlockOutput, this.GetOutputClasses(), false);
        }

        private MyInventoryConstraint PrepareConstraint(MyStringId descriptionId, IEnumerable<MyBlueprintClassDefinition> classes, bool input)
        {
            string icon = null;
            foreach (MyBlueprintClassDefinition definition in classes)
            {
                string str2 = input ? definition.InputConstraintIcon : definition.OutputConstraintIcon;
                if (str2 != null)
                {
                    if (icon == null)
                    {
                        icon = str2;
                        continue;
                    }
                    if (icon != str2)
                    {
                        icon = null;
                        break;
                    }
                }
            }
            MyInventoryConstraint constraint = new MyInventoryConstraint(string.Format(MyTexts.GetString(descriptionId), this.DisplayNameText), icon, true);
            using (IEnumerator<MyBlueprintClassDefinition> enumerator = classes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IEnumerator<MyBlueprintDefinitionBase> enumerator2 = enumerator.Current.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            MyBlueprintDefinitionBase current = enumerator2.Current;
                            foreach (MyBlueprintDefinitionBase.Item item in input ? current.Prerequisites : current.Results)
                            {
                                constraint.Add(item.Id);
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator2 == null)
                        {
                            continue;
                        }
                        enumerator2.Dispose();
                    }
                }
            }
            return constraint;
        }
    }
}

