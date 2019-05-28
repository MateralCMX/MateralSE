namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Voxels.Clipmap;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;
    using VRageRender.Messages;
    using VRageRender.Voxels;

    internal class MyRenderComponentPlanet : MyRenderComponentVoxelMap
    {
        private MyPlanet m_planet;
        private int m_shadowHelperRenderObjectIndex = -1;
        private int m_atmosphereRenderIndex = -1;
        private readonly List<int> m_cloudLayerRenderObjectIndexList = new List<int>();
        private int m_fogUpdateCounter;
        private static bool lastSentFogFlag = true;
        private bool m_oldNeedsDraw;

        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            base.ResizeRenderObjectArray(0x10);
            int length = base.RenderObjectIDs.Length;
            Vector3D positionLeftBottomCorner = this.m_planet.PositionLeftBottomCorner;
            Vector3 atmosphereWavelengths = new Vector3 {
                X = 1f / ((float) Math.Pow((double) this.m_planet.AtmosphereWavelengths.X, 4.0)),
                Y = 1f / ((float) Math.Pow((double) this.m_planet.AtmosphereWavelengths.Y, 4.0)),
                Z = 1f / ((float) Math.Pow((double) this.m_planet.AtmosphereWavelengths.Z, 4.0))
            };
            IMyEntity entity = base.Entity;
            if (this.m_planet.HasAtmosphere)
            {
                MatrixD xd2 = MatrixD.Identity * this.m_planet.AtmosphereRadius;
                xd2.M44 = 1.0;
                xd2.Translation = base.Entity.PositionComp.GetPosition();
                this.m_atmosphereRenderIndex = length;
                length++;
                long entityId = base.Entity.EntityId;
                this.SetRenderObjectID(length, MyRenderProxy.CreateRenderEntityAtmosphere(base.Entity.GetFriendlyName() + " " + entityId.ToString(), @"Models\Environment\Atmosphere_sphere.mwm", xd2, MyMeshDrawTechnique.ATMOSPHERE, RenderFlags.DrawOutsideViewDistance | RenderFlags.Visible, this.GetRenderCullingOptions(), this.m_planet.AtmosphereRadius, this.m_planet.AverageRadius, atmosphereWavelengths, 0f, float.MaxValue, base.FadeIn));
                this.UpdateAtmosphereSettings(this.m_planet.AtmosphereSettings);
            }
            this.m_shadowHelperRenderObjectIndex = length;
            MatrixD worldMatrix = MatrixD.CreateScale((double) this.m_planet.MinimumRadius);
            worldMatrix.Translation = this.m_planet.WorldMatrix.Translation;
            length++;
            this.SetRenderObjectID(length, MyRenderProxy.CreateRenderEntity("Shadow helper", @"Models\Environment\Sky\ShadowHelperSphere.mwm", worldMatrix, MyMeshDrawTechnique.MESH, RenderFlags.CastShadowsOnLow | RenderFlags.SkipInMainView | RenderFlags.NoBackFaceCulling | RenderFlags.DrawOutsideViewDistance | RenderFlags.Visible | RenderFlags.CastShadows, CullingOptions.Default, Color.White, new Vector3(1f, 1f, 1f), 0f, float.MaxValue, 0, 1f, base.FadeIn));
            MyPlanetGeneratorDefinition generator = this.m_planet.Generator;
            if ((MyFakes.ENABLE_PLANETARY_CLOUDS && (generator != null)) && (generator.CloudLayers != null))
            {
                foreach (MyCloudLayerSettings settings in generator.CloudLayers)
                {
                    double minScaledAltitude = ((double) (this.m_planet.AverageRadius + this.m_planet.MaximumRadius)) / 2.0;
                    double altitude = minScaledAltitude + ((this.m_planet.MaximumRadius - minScaledAltitude) * settings.RelativeAltitude);
                    Vector3D rotationAxis = Vector3D.Normalize((settings.RotationAxis == Vector3D.Zero) ? Vector3D.Up : settings.RotationAxis);
                    int index = length + this.m_cloudLayerRenderObjectIndexList.Count;
                    this.SetRenderObjectID(index, MyRenderProxy.CreateRenderEntityCloudLayer((this.m_atmosphereRenderIndex != -1) ? base.m_renderObjectIDs[this.m_atmosphereRenderIndex] : uint.MaxValue, base.Entity.GetFriendlyName() + " " + base.Entity.EntityId.ToString(), settings.Model, settings.Textures, base.Entity.PositionComp.GetPosition(), altitude, minScaledAltitude, settings.ScalingEnabled, (double) settings.FadeOutRelativeAltitudeStart, (double) settings.FadeOutRelativeAltitudeEnd, settings.ApplyFogRelativeDistance, (double) this.m_planet.MaximumRadius, rotationAxis, settings.AngularVelocity, settings.InitialRotation, settings.Color.ToLinearRGB(), base.FadeIn));
                    this.m_cloudLayerRenderObjectIndexList.Add(index);
                }
                length += generator.CloudLayers.Count;
            }
        }

        protected override IMyLodController CreateLodController()
        {
            MatrixD worldMatrix = MatrixD.CreateWorld(base.m_voxelMap.PositionLeftBottomCorner, base.m_voxelMap.Orientation.Forward, base.m_voxelMap.Orientation.Up);
            MyVoxelClipmap clipmap1 = new MyVoxelClipmap(base.m_voxelMap.Size, worldMatrix, base.Mesher, new float?(this.m_planet.AverageRadius), this.m_planet.PositionComp.GetPosition(), "Planet");
            clipmap1.Cache = MyVoxelClipmapCache.Instance;
            return clipmap1;
        }

        public override void Draw()
        {
            if (this.m_oldNeedsDraw)
            {
                base.Draw();
            }
            double num = Vector3D.Distance(MySector.MainCamera.Position, this.m_planet.WorldMatrix.Translation);
            MatrixD xd = MatrixD.CreateScale((double) ((this.m_planet.MinimumRadius * Math.Min((double) (num / ((double) this.m_planet.MinimumRadius)), (double) 1.0)) * 0.996999979019165));
            xd.Translation = this.m_planet.PositionComp.WorldMatrix.Translation;
            BoundingBox? aabb = null;
            Matrix? localMatrix = null;
            MyRenderProxy.UpdateRenderObject(base.m_renderObjectIDs[this.m_shadowHelperRenderObjectIndex], new MatrixD?(xd), aabb, -1, localMatrix);
            this.DrawFog();
        }

        private void DrawFog()
        {
            if (MyFakes.ENABLE_CLOUD_FOG)
            {
                int fogUpdateCounter = this.m_fogUpdateCounter;
                this.m_fogUpdateCounter = fogUpdateCounter - 1;
                if (fogUpdateCounter <= 0)
                {
                    this.m_fogUpdateCounter = (int) (100f * (0.8f + (MyRandom.Instance.NextFloat() * 0.4f)));
                    Vector3D position = MySector.MainCamera.Position;
                    double num2 = this.m_planet.AtmosphereRadius * 2f;
                    if ((position - this.m_planet.PositionComp.GetPosition()).LengthSquared() <= (num2 * num2))
                    {
                        this.m_fogUpdateCounter = (int) (this.m_fogUpdateCounter * 0.67f);
                        bool shouldDrawFog = !this.IsPointInAirtightSpace(position);
                        if (lastSentFogFlag != shouldDrawFog)
                        {
                            lastSentFogFlag = shouldDrawFog;
                            MyRenderProxy.UpdateCloudLayerFogFlag(shouldDrawFog);
                        }
                    }
                }
            }
        }

        private bool IsPointInAirtightSpace(Vector3D worldPosition)
        {
            if (!MySession.Static.Settings.EnableOxygen)
            {
                return true;
            }
            bool flag = false;
            BoundingSphereD boundingSphere = new BoundingSphereD(worldPosition, 0.1);
            List<MyEntity> list = null;
            try
            {
                foreach (MyCubeGrid grid in MyEntities.GetEntitiesInSphere(ref boundingSphere))
                {
                    if (grid == null)
                    {
                        continue;
                    }
                    if (grid.GridSystems.GasSystem != null)
                    {
                        MyOxygenBlock safeOxygenBlock = grid.GridSystems.GasSystem.GetSafeOxygenBlock(worldPosition);
                        if ((safeOxygenBlock != null) && ((safeOxygenBlock.Room != null) && safeOxygenBlock.Room.IsAirtight))
                        {
                            flag = true;
                            break;
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
            return flag;
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_planet = base.Entity as MyPlanet;
            this.m_oldNeedsDraw = this.NeedsDraw;
            this.NeedsDraw = true;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            this.NeedsDraw = this.m_oldNeedsDraw;
            this.m_planet = null;
        }

        public void UpdateAtmosphereSettings(MyAtmosphereSettings settings)
        {
            MyRenderProxy.UpdateAtmosphereSettings(base.m_renderObjectIDs[this.m_atmosphereRenderIndex], settings);
        }
    }
}

