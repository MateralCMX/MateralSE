namespace Sandbox.Game.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.ModAPI;
    using VRage.Utils;

    public abstract class MyStatBase : IMyHudStat
    {
        private float m_currentValue;
        private string m_valueStringCache;

        protected MyStatBase()
        {
        }

        public string GetValueString()
        {
            if (this.m_valueStringCache == null)
            {
                this.m_valueStringCache = this.ToString();
            }
            return this.m_valueStringCache;
        }

        public override string ToString() => 
            $"{this.CurrentValue:0.00}";

        public abstract void Update();

        public MyStringHash Id { get; protected set; }

        public float CurrentValue
        {
            get => 
                this.m_currentValue;
            protected set
            {
                if (!this.m_currentValue.IsEqual(value, 0.0001f))
                {
                    this.m_currentValue = value;
                    this.m_valueStringCache = null;
                }
            }
        }

        public virtual float MaxValue =>
            1f;

        public virtual float MinValue =>
            0f;
    }
}

