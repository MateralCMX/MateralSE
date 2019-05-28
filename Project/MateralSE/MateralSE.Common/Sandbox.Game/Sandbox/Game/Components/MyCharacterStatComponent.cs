namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Utils;

    [MyComponentBuilder(typeof(MyObjectBuilder_CharacterStatComponent), true)]
    public class MyCharacterStatComponent : MyEntityStatComponent
    {
        public static MyStringHash HealthId = MyStringHash.GetOrCompute("Health");
        public MyDamageInformation LastDamage;
        public static readonly float HEALTH_RATIO_CRITICAL = 0.2f;
        public static readonly float HEALTH_RATIO_LOW = 0.4f;
        private MyCharacter m_character;

        public void Consume(MyFixedPoint amount, MyConsumableItemDefinition definition)
        {
            if (definition != null)
            {
                MyObjectBuilder_EntityStatRegenEffect objectBuilder = new MyObjectBuilder_EntityStatRegenEffect {
                    Interval = 1f,
                    MaxRegenRatio = 1f,
                    MinRegenRatio = 0f
                };
                foreach (MyConsumableItemDefinition.StatValue value2 in definition.Stats)
                {
                    MyEntityStat stat;
                    DictionaryValuesReader<MyStringHash, MyEntityStat> stats = base.Stats;
                    if (stats.TryGetValue(MyStringHash.GetOrCompute(value2.Name), out stat))
                    {
                        objectBuilder.TickAmount = value2.Value * ((float) amount);
                        objectBuilder.Duration = value2.Time;
                        stat.AddEffect(objectBuilder);
                    }
                }
            }
        }

        public void DoDamage(float damage, object statChangeData = null)
        {
            MyEntityStat health = this.Health;
            if (health != null)
            {
                if (this.m_character != null)
                {
                    this.m_character.CharacterAccumulatedDamage += damage;
                }
                if (statChangeData is MyDamageInformation)
                {
                    this.LastDamage = (MyDamageInformation) statChangeData;
                }
                health.Decrease(damage, statChangeData);
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_character = base.Container.Entity as MyCharacter;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            this.m_character = null;
            base.OnBeforeRemovedFromContainer();
        }

        private void OnDamage(float newHealth, float oldHealth)
        {
            if ((this.m_character != null) && !this.m_character.IsDead)
            {
                this.m_character.SoundComp.PlayDamageSound(oldHealth);
                this.m_character.Render.Damage();
            }
        }

        public void OnHealthChanged(float newHealth, float oldHealth, object statChangeData)
        {
            if ((this.m_character != null) && this.m_character.CharacterCanDie)
            {
                this.m_character.ForceUpdateBreath();
                if (newHealth < oldHealth)
                {
                    this.OnDamage(newHealth, oldHealth);
                }
            }
        }

        public override void Update()
        {
            if ((this.m_character != null) && this.m_character.IsDead)
            {
                using (Dictionary<MyStringHash, MyEntityStat>.ValueCollection.Enumerator enumerator = base.Stats.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ClearEffects();
                    }
                }
                base.m_scripts.Clear();
            }
            base.Update();
        }

        public MyEntityStat Health
        {
            get
            {
                MyEntityStat stat;
                return (!base.Stats.TryGetValue(HealthId, out stat) ? null : stat);
            }
        }

        public float HealthRatio
        {
            get
            {
                float num = 1f;
                MyEntityStat health = this.Health;
                if (health != null)
                {
                    num = health.Value / health.MaxValue;
                }
                return num;
            }
        }
    }
}

