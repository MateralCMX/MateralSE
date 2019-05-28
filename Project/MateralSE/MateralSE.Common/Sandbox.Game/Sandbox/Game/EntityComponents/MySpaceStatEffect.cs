namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders;
    using VRage.Utils;

    [MyStatLogicDescriptor("SpaceStatEffect")]
    public class MySpaceStatEffect : MyStatLogic
    {
        private static MyStringHash HealthId = MyStringHash.GetOrCompute("Health");
        public static readonly float MAX_REGEN_HEALTH_RATIO = 0.7f;
        private int m_healthEffectId;
        private bool m_effectCreated;

        private void ClearRegenEffect()
        {
            MyEntityStat health = this.Health;
            if (health != null)
            {
                health.RemoveEffect(this.m_healthEffectId);
                this.m_effectCreated = false;
            }
        }

        public override void Close()
        {
            MyEntityStat health = this.Health;
            if (health != null)
            {
                health.OnStatChanged -= new MyEntityStat.StatChangedDelegate(this.OnHealthChanged);
            }
            this.ClearRegenEffect();
            base.Close();
        }

        private void CreateRegenEffect()
        {
            MyObjectBuilder_EntityStatRegenEffect objectBuilder = new MyObjectBuilder_EntityStatRegenEffect();
            MyEntityStat health = this.Health;
            if (health != null)
            {
                objectBuilder.TickAmount = MyEffectConstants.HealthTick;
                objectBuilder.Interval = MyEffectConstants.HealthInterval;
                objectBuilder.MaxRegenRatio = MAX_REGEN_HEALTH_RATIO;
                objectBuilder.MinRegenRatio = 0f;
                this.m_healthEffectId = health.AddEffect(objectBuilder);
                this.m_effectCreated = true;
            }
        }

        public override void Init(IMyCharacter character, Dictionary<MyStringHash, MyEntityStat> stats, string scriptName)
        {
            base.Init(character, stats, scriptName);
            this.InitActions();
            MyEntityStat health = this.Health;
            if (health != null)
            {
                health.OnStatChanged += new MyEntityStat.StatChangedDelegate(this.OnHealthChanged);
            }
        }

        private void InitActions()
        {
            MyStatLogic.MyStatAction action = new MyStatLogic.MyStatAction();
            string actionId = "MedRoomHeal";
            action.StatId = HealthId;
            action.Cost = MyEffectConstants.MedRoomHeal;
            base.AddAction(actionId, action);
            action = new MyStatLogic.MyStatAction();
            actionId = "GenericHeal";
            action.StatId = HealthId;
            action.Cost = MyEffectConstants.GenericHeal;
            base.AddAction(actionId, action);
        }

        private void OnHealthChanged(float newValue, float oldValue, object statChangeData)
        {
            MyEntityStat health = this.Health;
            if (((health != null) && ((health.Value - health.MinValue) < 0.001f)) && (base.Character != null))
            {
                base.Character.Kill(statChangeData);
            }
        }

        public override void Update10()
        {
            base.Update10();
            if (MySession.Static.Settings.EnableOxygen && base.EnableAutoHealing)
            {
                if ((MySession.Static.Settings.EnableOxygenPressurization ? Math.Max(base.Character.EnvironmentOxygenLevel, base.Character.OxygenLevel) : base.Character.EnvironmentOxygenLevel) <= MyEffectConstants.MinOxygenLevelForHealthRegeneration)
                {
                    if (this.m_effectCreated)
                    {
                        this.ClearRegenEffect();
                    }
                }
                else if (!this.m_effectCreated)
                {
                    this.CreateRegenEffect();
                }
            }
        }

        private MyEntityStat Health
        {
            get
            {
                MyEntityStat stat;
                return (!base.m_stats.TryGetValue(HealthId, out stat) ? null : stat);
            }
        }
    }
}

