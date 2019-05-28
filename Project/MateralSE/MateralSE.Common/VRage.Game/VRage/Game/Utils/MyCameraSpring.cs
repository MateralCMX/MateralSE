namespace VRage.Game.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyCameraSpring
    {
        public bool Enabled = true;
        private Vector3 m_springCenterLinearVelocity;
        private Vector3 m_springCenterLinearVelocityLast;
        private Vector3 m_springBodyVelocity;
        private Vector3 m_springBodyPosition;
        private float m_stiffness;
        private float m_weight;
        private float m_dampening;
        private float m_maxVelocityChange;
        private static float m_springMaxLength;

        public MyCameraSpring()
        {
            this.Reset(true);
        }

        public void AddCurrentCameraControllerVelocity(Vector3 velocity)
        {
            this.m_springCenterLinearVelocity += velocity;
        }

        public void Reset(bool resetSpringSettings)
        {
            this.m_springCenterLinearVelocity = Vector3.Zero;
            this.m_springCenterLinearVelocityLast = Vector3.Zero;
            this.m_springBodyVelocity = Vector3.Zero;
            this.m_springBodyPosition = Vector3.Zero;
            if (resetSpringSettings)
            {
                this.m_stiffness = 20f;
                this.m_weight = 1f;
                this.m_dampening = 0.7f;
                this.m_maxVelocityChange = 2f;
                m_springMaxLength = 0.5f;
            }
        }

        public void SetCurrentCameraControllerVelocity(Vector3 velocity)
        {
            this.m_springCenterLinearVelocity = velocity;
        }

        private static Vector3 TransformLocalOffset(Vector3 springBodyPosition)
        {
            float num = springBodyPosition.Length();
            return ((num > 1E-05f) ? ((Vector3) ((((m_springMaxLength * num) / (num + 2f)) * springBodyPosition) / num)) : springBodyPosition);
        }

        public bool Update(float timeStep, out Vector3 newCameraLocalOffset)
        {
            if (!this.Enabled)
            {
                newCameraLocalOffset = Vector3.Zero;
                this.m_springCenterLinearVelocity = Vector3.Zero;
                return false;
            }
            Vector3 vector = this.m_springCenterLinearVelocity - this.m_springCenterLinearVelocityLast;
            if (vector.LengthSquared() > (this.m_maxVelocityChange * this.m_maxVelocityChange))
            {
                vector.Normalize();
                vector *= this.m_maxVelocityChange;
            }
            this.m_springCenterLinearVelocityLast = this.m_springCenterLinearVelocity;
            this.m_springBodyPosition += vector * timeStep;
            Vector3 vector2 = (-this.m_springBodyPosition * this.m_stiffness) / this.m_weight;
            this.m_springBodyVelocity += vector2 * timeStep;
            this.m_springBodyPosition += this.m_springBodyVelocity * timeStep;
            this.m_springBodyVelocity *= this.m_dampening;
            newCameraLocalOffset = TransformLocalOffset(this.m_springBodyPosition);
            return true;
        }

        public float SpringStiffness
        {
            get => 
                this.m_stiffness;
            set => 
                (this.m_stiffness = MathHelper.Clamp(value, 0f, 50f));
        }

        public float SpringDampening
        {
            get => 
                this.m_dampening;
            set => 
                (this.m_dampening = MathHelper.Clamp(value, 0f, 1f));
        }

        public float SpringMaxVelocity
        {
            get => 
                this.m_maxVelocityChange;
            set => 
                (this.m_maxVelocityChange = MathHelper.Clamp(value, 0f, 10f));
        }

        public float SpringMaxLength
        {
            get => 
                m_springMaxLength;
            set => 
                (m_springMaxLength = MathHelper.Clamp(value, 0f, 2f));
        }
    }
}

