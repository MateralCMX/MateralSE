namespace VRage.Game.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRage.Library.Utils;
    using VRageMath;

    public class MyAlphaBlinkBehavior
    {
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private int m_intervalLenghtMs = 0x7d0;
        private float m_minAlpha = 0.2f;
        private float m_maxAlpha = 0.8f;
        private float m_currentBlinkAlpha = 1f;

        public virtual void UpdateBlink()
        {
            if (!this.Blink)
            {
                this.CurrentBlinkAlpha = 1f;
            }
            else
            {
                double totalMilliseconds = TIMER.ElapsedTimeSpan.TotalMilliseconds;
                this.CurrentBlinkAlpha = this.m_minAlpha + ((float) (((Math.Cos(((totalMilliseconds / ((double) this.m_intervalLenghtMs)) * 3.1415926535897931) * 2.0) + 1.0) * 0.5) * (this.m_maxAlpha - this.m_minAlpha)));
            }
        }

        public float MinAlpha
        {
            get => 
                this.m_minAlpha;
            set => 
                (this.m_minAlpha = MathHelper.Clamp(value, 0f, 1f));
        }

        public float MaxAlpha
        {
            get => 
                this.m_maxAlpha;
            set => 
                (this.m_maxAlpha = MathHelper.Clamp(value, 0f, 1f));
        }

        public int IntervalMs
        {
            get => 
                this.m_intervalLenghtMs;
            set => 
                (this.m_intervalLenghtMs = MathHelper.Clamp(value, 0, 0x7fffffff));
        }

        public Vector4? ColorMask { get; set; }

        [XmlIgnore]
        public float CurrentBlinkAlpha
        {
            get => 
                this.m_currentBlinkAlpha;
            set => 
                (this.m_currentBlinkAlpha = MathHelper.Clamp(value, 0f, 1f));
        }

        public bool Blink { get; set; }
    }
}

