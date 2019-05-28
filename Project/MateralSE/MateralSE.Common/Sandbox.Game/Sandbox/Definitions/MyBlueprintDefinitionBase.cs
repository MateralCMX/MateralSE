namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public abstract class MyBlueprintDefinitionBase : MyDefinitionBase
    {
        public Item[] Prerequisites;
        public Item[] Results;
        public string ProgressBarSoundCue;
        public float BaseProductionTimeInSeconds = 1f;
        public float OutputVolume;
        public bool Atomic;
        public bool IsPrimary;

        protected MyBlueprintDefinitionBase()
        {
        }

        public abstract int GetBlueprints(List<ProductionInfo> blueprints);
        public abstract void Postprocess();
        public override string ToString() => 
            string.Format("(" + base.DisplayNameEnum.GetValueOrDefault(MyStringId.NullOrEmpty).String + "){{{0}}}->{{{1}}}", string.Join<Item>(" ", this.Prerequisites), string.Join<Item>(" ", this.Results));

        [Conditional("DEBUG")]
        private unsafe void VerifyInputItemType(MyObjectBuilderType inputType)
        {
            Item[] prerequisites = this.Prerequisites;
            for (int i = 0; i < prerequisites.Length; i++)
            {
                Item* itemPtr1 = (Item*) ref prerequisites[i];
            }
        }

        public MyObjectBuilderType InputItemType =>
            this.Prerequisites[0].Id.TypeId;

        public bool PostprocessNeeded { get; protected set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct Item
        {
            public MyDefinitionId Id;
            public MyFixedPoint Amount;
            public override string ToString() => 
                $"{this.Amount}x {this.Id}";

            public static MyBlueprintDefinitionBase.Item FromObjectBuilder(BlueprintItem obItem) => 
                new MyBlueprintDefinitionBase.Item { 
                    Id = obItem.Id,
                    Amount = MyFixedPoint.DeserializeStringSafe(obItem.Amount)
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProductionInfo
        {
            public MyBlueprintDefinitionBase Blueprint;
            public MyFixedPoint Amount;
        }
    }
}

