namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_BarbarianWaveEventDefinition), (Type) null)]
    public class MyBarbarianWaveEventDefinition : MyGlobalEventDefinition
    {
        private Dictionary<int, Wave> m_waves = new Dictionary<int, Wave>();
        private int m_lastDay;

        public int GetBotCount(int dayNumber)
        {
            int num = 0;
            if ((this.m_lastDay > 0) && (dayNumber >= this.m_lastDay))
            {
                num = dayNumber - this.m_lastDay;
                dayNumber = this.m_lastDay;
            }
            Wave wave = null;
            return (this.m_waves.TryGetValue(dayNumber, out wave) ? (wave.Bots.Count + num) : 0);
        }

        public MyDefinitionId GetBotDefinitionId(int dayNumber, int botNumber)
        {
            int key = dayNumber;
            if ((this.m_lastDay > 0) && (dayNumber >= this.m_lastDay))
            {
                key = this.m_lastDay;
            }
            Wave wave = null;
            if (this.m_waves.TryGetValue(key, out wave) && (wave.Bots.Count > 0))
            {
                return wave.Bots[botNumber % wave.Bots.Count];
            }
            return new MyDefinitionId();
        }

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_BarbarianWaveEventDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_BarbarianWaveEventDefinition;
            objectBuilder.Waves = new MyObjectBuilder_BarbarianWaveEventDefinition.WaveDef[this.m_waves.Count];
            int index = 0;
            foreach (KeyValuePair<int, Wave> pair in this.m_waves)
            {
                MyObjectBuilder_BarbarianWaveEventDefinition.WaveDef def = new MyObjectBuilder_BarbarianWaveEventDefinition.WaveDef {
                    Day = pair.Key,
                    Bots = new MyObjectBuilder_BarbarianWaveEventDefinition.BotDef[pair.Value.Bots.Count]
                };
                int num2 = 0;
                foreach (MyDefinitionId id in pair.Value.Bots)
                {
                    def.Bots[num2] = new MyObjectBuilder_BarbarianWaveEventDefinition.BotDef { SubtypeName = id.SubtypeName };
                    num2++;
                }
                objectBuilder.Waves[index] = def;
                index++;
            }
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            foreach (MyObjectBuilder_BarbarianWaveEventDefinition.WaveDef def in (builder as MyObjectBuilder_BarbarianWaveEventDefinition).Waves)
            {
                Wave wave = new Wave(def);
                this.m_waves.Add(def.Day, wave);
                this.m_lastDay = Math.Max(this.m_lastDay, def.Day);
            }
        }

        public class Wave
        {
            public List<MyDefinitionId> Bots = new List<MyDefinitionId>();

            public Wave(MyObjectBuilder_BarbarianWaveEventDefinition.WaveDef waveOb)
            {
                foreach (MyObjectBuilder_BarbarianWaveEventDefinition.BotDef def in waveOb.Bots)
                {
                    MyObjectBuilderType type;
                    if (!MyObjectBuilderType.TryParse(def.TypeName, out type))
                    {
                        type = typeof(MyObjectBuilder_HumanoidBot);
                    }
                    this.Bots.Add(new MyDefinitionId(type, def.SubtypeName));
                }
            }
        }
    }
}

