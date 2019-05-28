namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Common;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyFactoryTag(typeof(MyObjectBuilder_EntityStat), true)]
    public class MyEntityStat
    {
        protected float m_currentValue;
        private float m_lastSyncValue;
        protected float m_minValue;
        protected float m_maxValue;
        protected float m_defaultValue;
        private bool m_syncFlag;
        private Dictionary<int, MyEntityStatRegenEffect> m_effects = new Dictionary<int, MyEntityStatRegenEffect>();
        private static List<int> m_tmpRemoveEffects = new List<int>();
        private int m_updateCounter;
        private float m_statRegenLeft;
        private float m_regenAmountMultiplier = 1f;
        private float m_regenAmountMultiplierDuration;
        private int m_regenAmountMultiplierTimeStart;
        private int m_regenAmountMultiplierTimeAlive;
        private bool m_regenAmountMultiplierActive;
        private MyStringHash m_statId;
        [CompilerGenerated]
        private StatChangedDelegate OnStatChanged;
        public MyEntityStatDefinition StatDefinition;

        public event StatChangedDelegate OnStatChanged
        {
            [CompilerGenerated] add
            {
                StatChangedDelegate onStatChanged = this.OnStatChanged;
                while (true)
                {
                    StatChangedDelegate a = onStatChanged;
                    StatChangedDelegate delegate4 = (StatChangedDelegate) Delegate.Combine(a, value);
                    onStatChanged = Interlocked.CompareExchange<StatChangedDelegate>(ref this.OnStatChanged, delegate4, a);
                    if (ReferenceEquals(onStatChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                StatChangedDelegate onStatChanged = this.OnStatChanged;
                while (true)
                {
                    StatChangedDelegate source = onStatChanged;
                    StatChangedDelegate delegate4 = (StatChangedDelegate) Delegate.Remove(source, value);
                    onStatChanged = Interlocked.CompareExchange<StatChangedDelegate>(ref this.OnStatChanged, delegate4, source);
                    if (ReferenceEquals(onStatChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public int AddEffect(MyObjectBuilder_EntityStatRegenEffect objectBuilder)
        {
            MyEntityStatRegenEffect effect = MyEntityStatEffectFactory.CreateInstance(objectBuilder);
            effect.Init(objectBuilder, this);
            int key = 0;
            while ((key < this.m_effects.Count) && this.m_effects.ContainsKey(key))
            {
                key++;
            }
            this.m_effects.Add(key, effect);
            return key;
        }

        public int AddEffect(float amount, float interval, float duration = -1f, float minRegenRatio = 0f, float maxRegenRatio = 1f)
        {
            MyObjectBuilder_EntityStatRegenEffect effect1 = new MyObjectBuilder_EntityStatRegenEffect();
            effect1.TickAmount = amount;
            effect1.Interval = interval;
            effect1.Duration = duration;
            effect1.MinRegenRatio = minRegenRatio;
            effect1.MaxRegenRatio = maxRegenRatio;
            MyObjectBuilder_EntityStatRegenEffect objectBuilder = effect1;
            return this.AddEffect(objectBuilder);
        }

        public void ApplyRegenAmountMultiplier(float amountMultiplier = 1f, float duration = 2f)
        {
            this.m_regenAmountMultiplier = amountMultiplier;
            this.m_regenAmountMultiplierDuration = duration;
            this.m_regenAmountMultiplierTimeStart = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.m_regenAmountMultiplierActive = duration > 0f;
        }

        public float CalculateRegenLeftForLongestEffect()
        {
            MyEntityStatRegenEffect effect = null;
            this.m_statRegenLeft = 0f;
            foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair in this.m_effects)
            {
                if (pair.Value.Duration <= 0f)
                {
                    continue;
                }
                this.m_statRegenLeft += pair.Value.AmountLeftOverDuration;
                if ((effect == null) || (pair.Value.DeathTime > effect.DeathTime))
                {
                    effect = pair.Value;
                }
            }
            if (effect != null)
            {
                foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair2 in this.m_effects)
                {
                    if (pair2.Value.Duration < 0f)
                    {
                        this.m_statRegenLeft += pair2.Value.Amount * pair2.Value.CalculateTicksBetweenTimes(effect.LastRegenTime, effect.DeathTime);
                    }
                }
            }
            return this.m_statRegenLeft;
        }

        public void ClearEffects()
        {
            foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair in this.m_effects)
            {
                pair.Value.Closing();
            }
            this.m_effects.Clear();
        }

        public void Decrease(float amount, object statChangeData)
        {
            this.SetValue(this.Value - amount, statChangeData);
        }

        public MyEntityStatRegenEffect GetEffect(int id)
        {
            MyEntityStatRegenEffect effect = null;
            return (this.m_effects.TryGetValue(id, out effect) ? effect : null);
        }

        public DictionaryReader<int, MyEntityStatRegenEffect> GetEffects() => 
            this.m_effects;

        public float GetEfficiencyMultiplier(float multiplier, float threshold) => 
            ((this.CurrentRatio < threshold) ? multiplier : 1f);

        public virtual MyObjectBuilder_EntityStat GetObjectBuilder()
        {
            MyObjectBuilder_EntityStat stat = new MyObjectBuilder_EntityStat();
            MyEntityStatDefinition definition = MyDefinitionManager.Static.GetDefinition(new MyDefinitionId(stat.TypeId, this.StatDefinition.Id.SubtypeId)) as MyEntityStatDefinition;
            stat.SubtypeName = this.StatDefinition.Id.SubtypeName;
            if (definition != null)
            {
                stat.Value = this.m_currentValue / ((definition.MaxValue != 0f) ? definition.MaxValue : 1f);
                stat.MaxValue = this.m_maxValue / ((definition.MaxValue != 0f) ? definition.MaxValue : 1f);
            }
            else
            {
                stat.Value = this.m_currentValue / this.m_maxValue;
                stat.MaxValue = 1f;
            }
            if (this.m_regenAmountMultiplierActive)
            {
                stat.StatRegenAmountMultiplier = this.m_regenAmountMultiplier;
                stat.StatRegenAmountMultiplierDuration = this.m_regenAmountMultiplierDuration;
            }
            stat.Effects = null;
            if ((this.m_effects != null) && (this.m_effects.Count > 0))
            {
                int count = this.m_effects.Count;
                foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair in this.m_effects)
                {
                    if (pair.Value.Duration < 0f)
                    {
                        count--;
                    }
                }
                if (count > 0)
                {
                    stat.Effects = new MyObjectBuilder_EntityStatRegenEffect[count];
                    int index = 0;
                    foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair2 in this.m_effects)
                    {
                        if (pair2.Value.Duration >= 0f)
                        {
                            index++;
                            stat.Effects[index] = pair2.Value.GetObjectBuilder();
                        }
                    }
                }
            }
            return stat;
        }

        public void Increase(float amount, object statChangeData)
        {
            this.SetValue(this.Value + amount, statChangeData);
        }

        public virtual void Init(MyObjectBuilder_Base objectBuilder)
        {
            MyEntityStatDefinition definition;
            MyObjectBuilder_EntityStat stat = (MyObjectBuilder_EntityStat) objectBuilder;
            MyDefinitionManager.Static.TryGetDefinition<MyEntityStatDefinition>(new MyDefinitionId(stat.TypeId, stat.SubtypeId), out definition);
            this.StatDefinition = definition;
            this.m_maxValue = definition.MaxValue;
            this.m_minValue = definition.MinValue;
            this.m_currentValue = stat.Value * this.m_maxValue;
            this.m_defaultValue = definition.DefaultValue;
            this.m_lastSyncValue = this.m_currentValue;
            this.m_statId = MyStringHash.GetOrCompute(definition.Name);
            this.m_regenAmountMultiplier = stat.StatRegenAmountMultiplier;
            this.m_regenAmountMultiplierDuration = stat.StatRegenAmountMultiplierDuration;
            this.m_regenAmountMultiplierTimeStart = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.m_regenAmountMultiplierTimeAlive = 0;
            this.m_regenAmountMultiplierActive = this.m_regenAmountMultiplierDuration > 0f;
            this.ClearEffects();
            if (stat.Effects != null)
            {
                foreach (MyObjectBuilder_EntityStatRegenEffect effect in stat.Effects)
                {
                    this.AddEffect(effect);
                }
            }
        }

        public bool RemoveEffect(int id)
        {
            MyEntityStatRegenEffect effect = null;
            if (this.m_effects.TryGetValue(id, out effect))
            {
                effect.Closing();
            }
            return this.m_effects.Remove(id);
        }

        public void ResetRegenAmountMultiplier()
        {
            this.m_regenAmountMultiplier = 1f;
            this.m_regenAmountMultiplierActive = false;
        }

        private void SetValue(float newValue, object statChangeData)
        {
            float currentValue = this.m_currentValue;
            this.m_currentValue = MathHelper.Clamp(newValue, this.MinValue, this.MaxValue);
            if ((this.OnStatChanged != null) && (newValue != currentValue))
            {
                this.OnStatChanged(newValue, currentValue, statChangeData);
            }
        }

        public override string ToString() => 
            this.m_statId.ToString();

        public bool TryGetEffect(int id, out MyEntityStatRegenEffect outEffect) => 
            this.m_effects.TryGetValue(id, out outEffect);

        public virtual void Update()
        {
            this.m_syncFlag = false;
            this.UpdateRegenAmountMultiplier();
            foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair in this.m_effects)
            {
                MyEntityStatRegenEffect effect = pair.Value;
                if ((effect.Duration >= 0f) && (effect.AliveTime >= (effect.Duration * 1000f)))
                {
                    m_tmpRemoveEffects.Add(pair.Key);
                }
                if (Sync.IsServer && effect.Enabled)
                {
                    if (this.m_regenAmountMultiplierActive)
                    {
                        effect.Update(this.m_regenAmountMultiplier);
                    }
                    else
                    {
                        effect.Update(1f);
                    }
                }
            }
            foreach (int num in m_tmpRemoveEffects)
            {
                this.RemoveEffect(num);
            }
            m_tmpRemoveEffects.Clear();
            int updateCounter = this.m_updateCounter;
            this.m_updateCounter = updateCounter + 1;
            if ((((updateCounter % 10) == 0) || (Math.Abs((float) (this.Value - this.MinValue)) <= 0.001)) && (this.m_lastSyncValue != this.m_currentValue))
            {
                this.m_syncFlag = true;
                this.m_lastSyncValue = this.m_currentValue;
            }
        }

        private void UpdateRegenAmountMultiplier()
        {
            if (this.m_regenAmountMultiplierActive)
            {
                this.m_regenAmountMultiplierTimeAlive = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_regenAmountMultiplierTimeStart;
                if (this.m_regenAmountMultiplierTimeAlive >= (this.m_regenAmountMultiplierDuration * 1000f))
                {
                    this.m_regenAmountMultiplier = 1f;
                    this.m_regenAmountMultiplierDuration = 0f;
                    this.m_regenAmountMultiplierActive = false;
                }
            }
        }

        public float Value
        {
            get => 
                this.m_currentValue;
            set => 
                this.SetValue(value, null);
        }

        public float CurrentRatio =>
            (this.Value / (this.MaxValue - this.MinValue));

        public float MinValue =>
            this.m_minValue;

        public float MaxValue =>
            this.m_maxValue;

        public float DefaultValue =>
            this.m_defaultValue;

        public bool ShouldSync =>
            this.m_syncFlag;

        public float StatRegenLeft
        {
            get => 
                this.m_statRegenLeft;
            set => 
                (this.m_statRegenLeft = value);
        }

        public MyStringHash StatId =>
            this.m_statId;

        public delegate void StatChangedDelegate(float newValue, float oldValue, object statChangeData);
    }
}

