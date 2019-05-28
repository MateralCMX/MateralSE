namespace SpaceEngineers.Game.World.Environment
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender;

    [MyEnvironmentalParticleLogicType(typeof(MyObjectBuilder_EnvironmentalParticleLogicSpace), true)]
    internal class MyEnvironmentalParticleLogicSpace : MyEnvironmentalParticleLogic
    {
        private int m_lastParticleSpawn;
        private float m_particlesLeftToSpawn;
        private bool m_isPlanetary;

        public override void Draw()
        {
            base.Draw();
            if (this.ShouldDrawParticles)
            {
                Vector3 directionNormalized = -Vector3.Normalize(this.ControlledVelocity);
                float num = 0.025f;
                float num2 = (float) MathHelper.Clamp((double) (this.ControlledVelocity.Length() / 50f), 0.0, 1.0);
                float num3 = 1f;
                float num4 = 1f;
                if (this.m_isPlanetary)
                {
                    num3 = 1.5f;
                    num4 = 3f;
                }
                foreach (MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle in base.m_activeParticles)
                {
                    if (particle.Active)
                    {
                        if (this.m_isPlanetary)
                        {
                            MyTransparentGeometry.AddLineBillboard(particle.MaterialPlanet, particle.ColorPlanet, particle.Position, directionNormalized, num2 * num4, num * num3, MyBillboard.BlendTypeEnum.LDR, -1, 1f, null);
                            continue;
                        }
                        MyTransparentGeometry.AddLineBillboard(particle.Material, particle.Color, particle.Position, directionNormalized, num2 * num4, num * num3, MyBillboard.BlendTypeEnum.LDR, -1, 1f, null);
                    }
                }
            }
        }

        private bool HasControlledNonZeroVelocity()
        {
            MyEntity controlledEntity = this.ControlledEntity;
            if ((controlledEntity == null) || MySession.Static.IsCameraUserControlledSpectator())
            {
                return false;
            }
            MyRemoteControl control = controlledEntity as MyRemoteControl;
            if (control != null)
            {
                controlledEntity = control.GetTopMostParent(null);
            }
            MyCockpit cockpit = controlledEntity as MyCockpit;
            if (cockpit != null)
            {
                controlledEntity = cockpit.GetTopMostParent(null);
            }
            return ((controlledEntity != null) && ((controlledEntity.Physics != null) && (controlledEntity.Physics.LinearVelocity != Vector3.Zero)));
        }

        public override void Init(MyObjectBuilder_EnvironmentalParticleLogic builder)
        {
            base.Init(builder);
            MyObjectBuilder_EnvironmentalParticleLogicSpace space1 = builder as MyObjectBuilder_EnvironmentalParticleLogicSpace;
        }

        private bool IsInGridAABB()
        {
            bool flag = false;
            BoundingSphereD boundingSphere = new BoundingSphereD(MySector.MainCamera.Position, 0.10000000149011612);
            List<MyEntity> list = null;
            try
            {
                foreach (MyCubeGrid grid in MyEntities.GetEntitiesInSphere(ref boundingSphere))
                {
                    if ((grid != null) && (grid.GridSizeEnum != MyCubeSize.Small))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            finally
            {
                if (list != null)
                {
                    list.Clear();
                }
            }
            return flag;
        }

        private bool IsNearPlanet() => 
            ((this.ControlledEntity != null) ? !Vector3.IsZero(MyGravityProviderSystem.CalculateNaturalGravityInPoint(this.ControlledEntity.PositionComp.GetPosition())) : false);

        public override void UpdateAfterSimulation()
        {
            if (!this.ShouldDrawParticles)
            {
                base.DeactivateAll();
                this.m_particlesLeftToSpawn = 0f;
            }
            base.UpdateAfterSimulation();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            try
            {
                if (this.ShouldDrawParticles && (this.ControlledVelocity.Length() >= 10f))
                {
                    float particleSpawnDistance = base.ParticleSpawnDistance;
                    double num2 = 1.5707963267948966;
                    float num3 = 1f;
                    Vector3 vector = this.ControlledVelocity - (8.5f * Vector3.Normalize(this.ControlledVelocity));
                    float num4 = ((4f * particleSpawnDistance) * particleSpawnDistance) * num3;
                    this.m_isPlanetary = this.IsNearPlanet();
                    if (!this.m_isPlanetary || MyFakes.ENABLE_STARDUST_ON_PLANET)
                    {
                        this.m_particlesLeftToSpawn += ((((0.25f + (MyRandom.Instance.NextFloat() * 1.25f)) * vector.Length()) * num4) * base.ParticleDensity) * 16f;
                        if (this.m_particlesLeftToSpawn >= 1f)
                        {
                            double num5 = num2 / 2.0;
                            double num6 = num5 + num2;
                            double a = num5 + (MyRandom.Instance.NextFloat() * (num6 - num5));
                            double num8 = num5 + (MyRandom.Instance.NextFloat() * (num6 - num5));
                            float num9 = 6f;
                            while (true)
                            {
                                float particlesLeftToSpawn = this.m_particlesLeftToSpawn;
                                this.m_particlesLeftToSpawn = particlesLeftToSpawn - 1f;
                                if (particlesLeftToSpawn < 1f)
                                {
                                    break;
                                }
                                float num10 = 0.01745329f;
                                if ((Math.Abs((double) (a - 1.5707963267948966)) < (num9 * num10)) && (Math.Abs((double) (num8 - 1.5707963267948966)) < (num9 * num10)))
                                {
                                    a += (Math.Sign(MyRandom.Instance.NextFloat()) * num9) * num10;
                                    num8 += (Math.Sign(MyRandom.Instance.NextFloat()) * num9) * num10;
                                }
                                float num11 = (float) Math.Sin(num8);
                                float num12 = (float) Math.Cos(num8);
                                float num13 = (float) Math.Sin(a);
                                float num14 = (float) Math.Cos(a);
                                Vector3 vector5 = Vector3.Normalize(vector);
                                Vector3 vector6 = Vector3.Cross(vector5, -MySector.MainCamera.UpVector);
                                if (Vector3.IsZero(vector6))
                                {
                                    vector6 = Vector3.CalculatePerpendicularVector(vector5);
                                }
                                else
                                {
                                    vector6.Normalize();
                                }
                                Vector3 position = (Vector3) (MySector.MainCamera.Position + (particleSpawnDistance * (((Vector3.Cross(vector5, vector6) * num12) + ((vector6 * num11) * num14)) + ((vector5 * num11) * num13))));
                                base.Spawn(position);
                                this.m_lastParticleSpawn = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                            }
                        }
                    }
                }
            }
            finally
            {
            }
        }

        public MyEntity ControlledEntity =>
            (MySession.Static.ControlledEntity as MyEntity);

        public Vector3 ControlledVelocity
        {
            get
            {
                if ((this.ControlledEntity is MyCockpit) || (this.ControlledEntity is MyRemoteControl))
                {
                    return this.ControlledEntity.GetTopMostParent(null).Physics.LinearVelocity;
                }
                return this.ControlledEntity.Physics.LinearVelocity;
            }
        }

        public bool ShouldDrawParticles =>
            (this.HasControlledNonZeroVelocity() && !this.IsInGridAABB());
    }
}

