namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ConsumableItemDefinition), (Type) null)]
    public class MyConsumableItemDefinition : MyUsableItemDefinition
    {
        public List<StatValue> Stats;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ConsumableItemDefinition definition = builder as MyObjectBuilder_ConsumableItemDefinition;
            this.Stats = new List<StatValue>();
            if (definition.Stats != null)
            {
                foreach (MyObjectBuilder_ConsumableItemDefinition.StatValue value2 in definition.Stats)
                {
                    this.Stats.Add(new StatValue(value2.Name, value2.Value, value2.Time));
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StatValue
        {
            public string Name;
            public float Value;
            public float Time;
            public StatValue(string name, float value, float time)
            {
                this.Name = name;
                this.Value = value;
                this.Time = time;
            }
        }
    }
}

