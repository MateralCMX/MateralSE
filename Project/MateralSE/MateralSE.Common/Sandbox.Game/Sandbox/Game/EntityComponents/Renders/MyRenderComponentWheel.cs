namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRage.Game;
    using VRageMath;

    public class MyRenderComponentWheel : MyRenderComponentCubeBlock
    {
        private MyParticleEffect m_dustParticleEffect;
        private string m_dustParticleName = string.Empty;
        private Vector3 m_relativePosition = Vector3.Zero;
        private int m_timer;
        private MyWheel m_wheel;
        private const int PARTICLE_GENERATION_TIMEOUT = 20;

        private MatrixD GetParticleMatrix(ref Vector3D position, ref Vector3 normal)
        {
            Vector3 vector = Vector3.Cross(normal, this.m_wheel.GetTopMostParent(null).Physics.LinearVelocity);
            Vector3 up = Vector3.Cross(normal, vector);
            return MatrixD.CreateWorld(position, normal, up);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_wheel = base.Entity as MyWheel;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (this.m_dustParticleEffect != null)
            {
                this.m_dustParticleEffect.Stop(false);
            }
            this.m_wheel = null;
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            if (this.m_dustParticleEffect != null)
            {
                this.m_dustParticleEffect.Stop(false);
            }
        }

        public bool TrySpawnParticle(string particleName, ref Vector3D position, ref Vector3 normal)
        {
            if (!MyFakes.ENABLE_DRIVING_PARTICLES)
            {
                return false;
            }
            if ((this.m_dustParticleEffect == null) || !particleName.Equals(this.m_dustParticleName))
            {
                if (this.m_dustParticleEffect != null)
                {
                    this.m_dustParticleEffect.Stop(false);
                    this.m_dustParticleEffect = null;
                }
                if (this.m_wheel.GetTopMostParent(null).Physics.LinearVelocity.LengthSquared() > 0.1f)
                {
                    this.m_dustParticleName = particleName;
                    MyParticlesManager.TryCreateParticleEffect(this.m_dustParticleName, this.GetParticleMatrix(ref position, ref normal), out this.m_dustParticleEffect);
                    this.m_timer = 20;
                }
            }
            return true;
        }

        public void UpdateParticle(ref Vector3D position, ref Vector3 normal)
        {
            if (this.m_dustParticleEffect != null)
            {
                float num = this.m_wheel.GetTopMostParent(null).Physics.LinearVelocity.LengthSquared();
                if (num < 0.1f)
                {
                    this.m_dustParticleEffect.Stop(false);
                    this.m_dustParticleEffect = null;
                }
                else
                {
                    float num2 = num / (MyGridPhysics.ShipMaxLinearVelocity() * MyGridPhysics.ShipMaxLinearVelocity());
                    this.m_relativePosition = (Vector3) (position - this.m_wheel.WorldMatrix.Translation);
                    this.m_dustParticleEffect.WorldMatrix = this.GetParticleMatrix(ref position, ref normal);
                    float num3 = 1f + (num2 * 2f);
                    this.m_dustParticleEffect.UserScale = num3;
                    this.m_timer = 20;
                }
            }
        }

        public void UpdatePosition()
        {
            if (this.m_dustParticleEffect != null)
            {
                this.m_timer--;
                if (this.m_timer <= 0)
                {
                    this.m_dustParticleEffect.Stop(false);
                    this.m_dustParticleEffect = null;
                }
                else
                {
                    MatrixD worldMatrix = this.m_dustParticleEffect.WorldMatrix;
                    worldMatrix.Translation = this.m_wheel.WorldMatrix.Translation + this.m_relativePosition;
                    this.m_dustParticleEffect.WorldMatrix = worldMatrix;
                }
            }
        }

        public bool UpdateNeeded =>
            (this.m_timer > 0);
    }
}

