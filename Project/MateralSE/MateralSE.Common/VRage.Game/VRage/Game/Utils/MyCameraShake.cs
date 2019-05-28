namespace VRage.Game.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyCameraShake
    {
        public float MaxShake = 15f;
        public float MaxShakePosX = 0.8f;
        public float MaxShakePosY = 0.2f;
        public float MaxShakePosZ = 0.8f;
        public float MaxShakeDir = 0.2f;
        public float Reduction = 0.6f;
        public float Dampening = 0.8f;
        public float OffConstant = 0.01f;
        public float DirReduction = 0.35f;
        private bool m_shakeEnabled = false;
        private Vector3 m_shakePos;
        private Vector3 m_shakeDir;
        private float m_currentShakePosPower = 0f;
        private float m_currentShakeDirPower = 0f;

        public void AddShake(float shakePower)
        {
            if (!MyUtils.IsZero(shakePower, 1E-05f) && !MyUtils.IsZero(this.MaxShake, 1E-05f))
            {
                float num = MathHelper.Clamp((float) (shakePower / this.MaxShake), (float) 0f, (float) 1f);
                if (this.m_currentShakePosPower < num)
                {
                    this.m_currentShakePosPower = num;
                }
                if (this.m_currentShakeDirPower < (num * this.DirReduction))
                {
                    this.m_currentShakeDirPower = num * this.DirReduction;
                }
                this.m_shakePos = new Vector3(this.m_currentShakePosPower * this.MaxShakePosX, this.m_currentShakePosPower * this.MaxShakePosY, this.m_currentShakePosPower * this.MaxShakePosZ);
                this.m_shakeDir = new Vector3(this.m_currentShakeDirPower * this.MaxShakeDir, this.m_currentShakeDirPower * this.MaxShakeDir, 0f);
                this.m_shakeEnabled = true;
            }
        }

        public bool ShakeActive() => 
            this.m_shakeEnabled;

        public unsafe void UpdateShake(float timeStep, out Vector3 outPos, out Vector3 outDir)
        {
            if (!this.m_shakeEnabled)
            {
                outPos = Vector3.Zero;
                outDir = Vector3.Zero;
            }
            else
            {
                float* singlePtr1 = (float*) ref this.m_shakePos.X;
                singlePtr1[0] *= MyUtils.GetRandomSign();
                float* singlePtr2 = (float*) ref this.m_shakePos.Y;
                singlePtr2[0] *= MyUtils.GetRandomSign();
                float* singlePtr3 = (float*) ref this.m_shakePos.Z;
                singlePtr3[0] *= MyUtils.GetRandomSign();
                outPos.X = (this.m_shakePos.X * Math.Abs(this.m_shakePos.X)) * this.Reduction;
                outPos.Y = (this.m_shakePos.Y * Math.Abs(this.m_shakePos.Y)) * this.Reduction;
                outPos.Z = (this.m_shakePos.Z * Math.Abs(this.m_shakePos.Z)) * this.Reduction;
                float* singlePtr4 = (float*) ref this.m_shakeDir.X;
                singlePtr4[0] *= MyUtils.GetRandomSign();
                float* singlePtr5 = (float*) ref this.m_shakeDir.Y;
                singlePtr5[0] *= MyUtils.GetRandomSign();
                float* singlePtr6 = (float*) ref this.m_shakeDir.Z;
                singlePtr6[0] *= MyUtils.GetRandomSign();
                outDir.X = (this.m_shakeDir.X * Math.Abs(this.m_shakeDir.X)) * 100f;
                outDir.Y = (this.m_shakeDir.Y * Math.Abs(this.m_shakeDir.Y)) * 100f;
                outDir.Z = (this.m_shakeDir.Z * Math.Abs(this.m_shakeDir.Z)) * 100f;
                outDir *= this.DirReduction;
                this.m_currentShakePosPower *= (float) Math.Pow((double) this.Dampening, (double) (timeStep * 60f));
                this.m_currentShakeDirPower *= (float) Math.Pow((double) this.Dampening, (double) (timeStep * 60f));
                if (this.m_currentShakeDirPower < 0f)
                {
                    this.m_currentShakeDirPower = 0f;
                }
                if (this.m_currentShakePosPower < 0f)
                {
                    this.m_currentShakePosPower = 0f;
                }
                this.m_shakePos = new Vector3(this.m_currentShakePosPower * this.MaxShakePosX, this.m_currentShakePosPower * this.MaxShakePosY, this.m_currentShakePosPower * this.MaxShakePosZ);
                this.m_shakeDir = new Vector3(this.m_currentShakeDirPower * this.MaxShakeDir, this.m_currentShakeDirPower * this.MaxShakeDir, 0f);
                if ((this.m_currentShakeDirPower < this.OffConstant) && (this.m_currentShakePosPower < this.OffConstant))
                {
                    this.m_currentShakeDirPower = 0f;
                    this.m_currentShakePosPower = 0f;
                    this.m_shakeEnabled = false;
                }
            }
        }

        public bool ShakeEnabled
        {
            get => 
                this.m_shakeEnabled;
            set => 
                (this.m_shakeEnabled = value);
        }

        public Vector3 ShakePos =>
            this.m_shakePos;

        public Vector3 ShakeDir =>
            this.m_shakeDir;
    }
}

