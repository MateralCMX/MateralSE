namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyEntityStatEffectType(typeof(MyObjectBuilder_EntityStatRegenEffect))]
    public class MyEntityStatRegenEffect
    {
        protected float m_amount;
        protected float m_interval;
        protected float m_maxRegenRatio;
        protected float m_minRegenRatio;
        protected float m_duration;
        protected int m_lastRegenTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        private readonly int m_birthTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        private bool m_enabled;
        private MyEntityStat m_parentStat;

        public MyEntityStatRegenEffect()
        {
            this.Enabled = true;
        }

        public int CalculateTicksBetweenTimes(int startTime, int endTime)
        {
            if ((startTime < this.m_birthTime) || (startTime >= endTime))
            {
                return 0;
            }
            int num1 = Math.Max(startTime, this.m_lastRegenTime);
            startTime = num1;
            int num2 = Math.Min(endTime, this.DeathTime);
            endTime = num2;
            return Math.Max((int) (((double) (endTime - startTime)) / Math.Round((double) (this.m_interval * 1000f))), 0);
        }

        public virtual void Closing()
        {
            if (Sync.IsServer)
            {
                this.IncreaseByRemainingValue();
            }
        }

        public virtual MyObjectBuilder_EntityStatRegenEffect GetObjectBuilder()
        {
            MyObjectBuilder_EntityStatRegenEffect effect1 = new MyObjectBuilder_EntityStatRegenEffect();
            effect1.TickAmount = this.m_amount;
            effect1.Interval = this.m_interval;
            effect1.MaxRegenRatio = this.m_maxRegenRatio;
            effect1.MinRegenRatio = this.m_minRegenRatio;
            effect1.Duration = this.m_duration;
            effect1.AliveTime = this.AliveTime;
            return effect1;
        }

        private void IncreaseByRemainingValue()
        {
            if ((this.m_interval > 0f) && this.Enabled)
            {
                float num = 1f - (((float) (this.m_lastRegenTime - MySandboxGame.TotalGamePlayTimeInMilliseconds)) / (this.m_interval * 1000f));
                if (num > 0f)
                {
                    if ((this.m_amount > 0f) && (this.m_parentStat.Value < this.m_parentStat.MaxValue))
                    {
                        this.m_parentStat.Value = MathHelper.Clamp(this.m_parentStat.Value + (this.m_amount * num), this.m_parentStat.MinValue, Math.Max(this.m_parentStat.MaxValue * this.m_maxRegenRatio, this.m_parentStat.MaxValue));
                    }
                    else if ((this.m_amount < 0f) && (this.m_parentStat.Value > this.m_parentStat.MinValue))
                    {
                        this.m_parentStat.Value = MathHelper.Clamp(this.m_parentStat.Value + (this.m_amount * num), Math.Max(this.m_parentStat.MaxValue * this.m_minRegenRatio, this.m_parentStat.MinValue), this.m_parentStat.MaxValue);
                    }
                }
            }
        }

        public virtual void Init(MyObjectBuilder_Base objectBuilder, MyEntityStat parentStat)
        {
            this.m_parentStat = parentStat;
            MyObjectBuilder_EntityStatRegenEffect effect = objectBuilder as MyObjectBuilder_EntityStatRegenEffect;
            if ((effect != null) && (effect.Interval > 0f))
            {
                this.m_amount = effect.TickAmount;
                this.m_interval = effect.Interval;
                this.m_maxRegenRatio = effect.MaxRegenRatio;
                this.m_minRegenRatio = effect.MinRegenRatio;
                this.m_duration = effect.Duration - (effect.AliveTime / 1000f);
                this.ResetRegenTime();
            }
        }

        public void ResetRegenTime()
        {
            this.m_lastRegenTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + ((int) Math.Round((double) (this.m_interval * 1000f)));
        }

        public void SetAmountAndInterval(float amount, float interval, bool increaseByRemaining)
        {
            if ((amount != this.Amount) || (interval != this.Interval))
            {
                if (increaseByRemaining)
                {
                    this.IncreaseByRemainingValue();
                }
                this.Amount = amount;
                this.Interval = interval;
                this.ResetRegenTime();
            }
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.m_parentStat.ToString(), ": (", this.m_amount, "/", this.m_interval, "/", this.m_duration, ")" };
            return string.Concat(objArray1);
        }

        public virtual void Update(float regenAmountMultiplier = 1f)
        {
            if (this.m_interval > 0f)
            {
                for (bool flag = this.m_duration == 0f; ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastRegenTime) >= 0) | flag; flag = false)
                {
                    if ((this.m_amount > 0f) && (this.m_parentStat.Value < (this.m_parentStat.MaxValue * this.m_maxRegenRatio)))
                    {
                        this.m_parentStat.Value = MathHelper.Clamp(this.m_parentStat.Value + (this.m_amount * regenAmountMultiplier), this.m_parentStat.Value, this.m_parentStat.MaxValue * this.m_maxRegenRatio);
                    }
                    else if ((this.m_amount < 0f) && (this.m_parentStat.Value > Math.Max(this.m_parentStat.MinValue, this.m_parentStat.MaxValue * this.m_minRegenRatio)))
                    {
                        this.m_parentStat.Value = MathHelper.Clamp(this.m_parentStat.Value + this.m_amount, Math.Max(this.m_parentStat.MaxValue * this.m_minRegenRatio, this.m_parentStat.MinValue), this.m_parentStat.Value);
                    }
                    this.m_lastRegenTime += (int) Math.Round((double) (this.m_interval * 1000f));
                }
            }
        }

        public float Amount
        {
            get => 
                this.m_amount;
            set => 
                (this.m_amount = value);
        }

        public float AmountLeftOverDuration =>
            ((this.m_amount * this.TicksLeft) + this.PartialEndAmount);

        public int TicksLeft =>
            this.CalculateTicksBetweenTimes(this.m_lastRegenTime, this.DeathTime);

        private float PartialEndAmount
        {
            get
            {
                float single1 = this.m_duration / this.m_interval;
                return ((single1 - ((float) Math.Truncate((double) single1))) * this.m_amount);
            }
        }

        public float Interval
        {
            get => 
                this.m_interval;
            set => 
                (this.m_interval = value);
        }

        public float Duration =>
            this.m_duration;

        public int LastRegenTime =>
            this.m_lastRegenTime;

        public int BirthTime =>
            this.m_birthTime;

        public int DeathTime =>
            ((this.Duration >= 0f) ? (this.m_birthTime + ((int) (this.m_duration * 1000f))) : 0x7fffffff);

        public int AliveTime =>
            (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.BirthTime);

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set => 
                (this.m_enabled = value);
        }
    }
}

