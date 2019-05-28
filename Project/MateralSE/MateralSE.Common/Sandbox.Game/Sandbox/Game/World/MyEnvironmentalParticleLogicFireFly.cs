namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender;

    [MyEnvironmentalParticleLogicType(typeof(MyObjectBuilder_EnvironmentalParticleLogicFireFly), true)]
    public class MyEnvironmentalParticleLogicFireFly : MyEnvironmentalParticleLogic
    {
        private int m_particleSpawnInterval = 60;
        private float m_particleSpawnIntervalRandomness = 0.5f;
        private int m_particleSpawnCounter;
        private static int m_updateCounter;
        private const int m_killDeadParticlesInterval = 60;
        private List<HkBodyCollision> m_bodyCollisions = new List<HkBodyCollision>();
        private List<MyEnvironmentItems.ItemInfo> m_tmpItemInfos = new List<MyEnvironmentItems.ItemInfo>();

        public override void Draw()
        {
            base.Draw();
            float thickness = 0.075f / 1.66f;
            float length = 0.075f;
            foreach (MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle in base.m_activeParticles)
            {
                if (particle.Active)
                {
                    Vector4 color = particle.Color;
                    float num3 = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - particle.BirthTime)) / ((float) particle.LifeTime);
                    if (num3 < 0.1f)
                    {
                        color = particle.Color * num3;
                    }
                    else if (num3 > 0.9f)
                    {
                        color = particle.Color * (1f - num3);
                    }
                    Vector3D vectord = Vector3D.CalculatePerpendicularVector(-Vector3D.Normalize(particle.Position - MySector.MainCamera.Position));
                    MyTransparentGeometry.AddLineBillboard(particle.Material, color, particle.Position, (Vector3) vectord, length, thickness, MyBillboard.BlendTypeEnum.Standard, -1, 1f, null);
                }
            }
        }

        private Vector3D GetInterpolatedPosition(MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle)
        {
            Vector3D position = particle.Position;
            if (particle.UserData != null)
            {
                double num = MathHelper.Clamp((double) (((double) (MySandboxGame.TotalGamePlayTimeInMilliseconds - particle.BirthTime)) / ((double) particle.LifeTime)), (double) 0.0, (double) 1.0);
                int num2 = 14;
                int index = 1 + ((int) (num * num2));
                float num4 = (float) ((num * num2) - Math.Truncate((double) (num * num2)));
                PathData data = (particle.UserData as PathData?).Value;
                position = Vector3D.CatmullRom(data.PathPoints[index - 1], data.PathPoints[index], data.PathPoints[index + 1], data.PathPoints[index + 2], (double) num4);
                if (!position.IsValid())
                {
                    position = particle.Position;
                }
            }
            return position;
        }

        private void InitializePath(MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle)
        {
            PathData data = new PathData();
            if (data.PathPoints == null)
            {
                data.PathPoints = new Vector3D[0x12];
            }
            Vector3D vectord = Vector3D.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(particle.Position));
            data.PathPoints[1] = particle.Position - ((Vector3D.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(particle.Position)) * MyRandom.Instance.NextFloat()) * 2.5);
            for (int i = 2; i < 0x11; i++)
            {
                float num2 = 5f;
                Vector3D vectord2 = Vector3D.Normalize((new Vector3D((double) MyRandom.Instance.NextFloat(), (double) MyRandom.Instance.NextFloat(), (double) MyRandom.Instance.NextFloat()) * 2.0) - Vector3D.One);
                data.PathPoints[i] = (data.PathPoints[i - 1] + ((vectord2 * (MyRandom.Instance.NextFloat() + 1f)) * num2)) - ((vectord / ((double) i)) * num2);
            }
            data.PathPoints[0] = data.PathPoints[1] - vectord;
            data.PathPoints[0x11] = data.PathPoints[0x10] + Vector3D.Normalize(data.PathPoints[0x10] - data.PathPoints[15]);
            particle.UserData = data;
        }

        private bool IsInGridAABB(Vector3D worldPosition)
        {
            BoundingSphereD boundingSphere = new BoundingSphereD(worldPosition, 0.10000000149011612);
            List<VRage.Game.Entity.MyEntity> list = null;
            try
            {
                using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere).GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current is MyCubeGrid)
                        {
                            return true;
                        }
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
            return false;
        }

        public override void Simulate()
        {
            base.Simulate();
            foreach (MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle in base.m_activeParticles)
            {
                Vector3 position = particle.Position;
                Vector3D interpolatedPosition = this.GetInterpolatedPosition(particle);
                particle.Position = (Vector3) interpolatedPosition;
            }
        }

        public override void UpdateAfterSimulation()
        {
            m_updateCounter++;
            if (m_updateCounter >= 60)
            {
                foreach (MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle in base.m_activeParticles)
                {
                    if (this.IsInGridAABB(particle.Position))
                    {
                        particle.Deactivate();
                    }
                }
                m_updateCounter = 0;
            }
            base.UpdateAfterSimulation();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (MyFakes.ENABLE_PLANET_FIREFLIES)
            {
                int particleSpawnCounter = this.m_particleSpawnCounter;
                this.m_particleSpawnCounter = particleSpawnCounter - 1;
                if (particleSpawnCounter <= 0)
                {
                    this.m_particleSpawnCounter = 0;
                    VRage.Game.Entity.MyEntity controlledEntity = MySession.Static.ControlledEntity as VRage.Game.Entity.MyEntity;
                    if (controlledEntity != null)
                    {
                        VRage.Game.Entity.MyEntity topMostParent = controlledEntity.GetTopMostParent(null);
                        if (topMostParent != null)
                        {
                            try
                            {
                                this.m_particleSpawnCounter = (int) Math.Round((double) (this.m_particleSpawnCounter + ((this.m_particleSpawnCounter * this.m_particleSpawnIntervalRandomness) * ((MyRandom.Instance.NextFloat() * 2f) - 1f))));
                                if ((((MyRandom.Instance.FloatNormal() + 3f) / 6f) <= base.m_particleDensity) && (MyGravityProviderSystem.CalculateNaturalGravityInPoint(MySector.MainCamera.Position).Dot(MySector.DirectionToSunNormalized) > 0f))
                                {
                                    Vector3 zero = Vector3.Zero;
                                    if (((topMostParent.Physics != null) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity)) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator))
                                    {
                                        zero = topMostParent.Physics.LinearVelocity;
                                    }
                                    MyCharacter character = topMostParent as MyCharacter;
                                    float num2 = MyGridPhysics.ShipMaxLinearVelocity();
                                    if (((character != null) && (character.Physics != null)) && (character.Physics.CharacterProxy != null))
                                    {
                                        num2 = character.Physics.CharacterProxy.CharacterFlyingMaxLinearVelocity();
                                    }
                                    Vector3 vector3 = Vector3.One * base.m_particleSpawnDistance;
                                    if ((zero.Length() / num2) > 1f)
                                    {
                                        vector3 += (10f * zero) / num2;
                                    }
                                    Vector3D vectord1 = MySector.MainCamera.Position + zero;
                                    if (MyGamePruningStructure.GetClosestPlanet(MySector.MainCamera.Position) != null)
                                    {
                                        Vector3D position = MySector.MainCamera.Position;
                                        Vector3D vectord = new Vector3D();
                                        if (this.m_tmpItemInfos.Count != 0)
                                        {
                                            int num3 = MyRandom.Instance.Next(0, this.m_tmpItemInfos.Count - 1);
                                            vectord = this.m_tmpItemInfos[num3].Transform.Position;
                                            MyEnvironmentalParticleLogic.MyEnvironmentalParticle particle = base.Spawn((Vector3) vectord);
                                            if (particle != null)
                                            {
                                                this.InitializePath(particle);
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                this.m_bodyCollisions.Clear();
                                this.m_tmpItemInfos.Clear();
                            }
                        }
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PathData
        {
            public const int PathPointCount = 0x10;
            public Vector3D[] PathPoints;
        }
    }
}

