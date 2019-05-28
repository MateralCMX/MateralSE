namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Lights;
    using VRageRender.Messages;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation | MyUpdateOrder.BeforeSimulation, 800)]
    public class MySector : MySessionComponentBase
    {
        public static Vector3 SunRotationAxis;
        public static MySunProperties SunProperties;
        public static MyFogProperties FogProperties;
        public static MyPlanetProperties PlanetProperties;
        public static MySSAOSettings SSAOSettings;
        public static MyHBAOData HBAOSettings;
        public static MyShadowsSettings ShadowSettings = new MyShadowsSettings();
        public static MySectorLodding Lodding = new MySectorLodding();
        internal static MyParticleDustProperties ParticleDustProperties;
        public static VRageRender.MyImpostorProperties[] ImpostorProperties;
        public static bool UseGenerator = false;
        public static List<int> PrimaryMaterials;
        public static List<int> SecondaryMaterials;
        public static MyEnvironmentDefinition EnvironmentDefinition;
        private static MyCamera m_camera;
        public static bool ResetEyeAdaptation;
        private static MyLight m_sunFlare;

        static MySector()
        {
            SetDefaults();
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
        }

        public override void Draw()
        {
            base.Draw();
            MyEntities.Draw();
        }

        public static MyObjectBuilder_EnvironmentSettings GetEnvironmentSettings()
        {
            float num;
            float num2;
            if (SunProperties.Equals(EnvironmentDefinition.SunProperties) && FogProperties.Equals(EnvironmentDefinition.FogProperties))
            {
                return null;
            }
            Vector3.GetAzimuthAndElevation(SunProperties.BaseSunDirectionNormalized, out num, out num2);
            MyObjectBuilder_EnvironmentSettings local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_EnvironmentSettings>();
            local1.SunAzimuth = num;
            local1.SunElevation = num2;
            local1.FogMultiplier = FogProperties.FogMultiplier;
            local1.FogDensity = FogProperties.FogDensity;
            local1.FogColor = FogProperties.FogColor;
            local1.EnvironmentDefinition = (SerializableDefinitionId) EnvironmentDefinition.Id;
            return local1;
        }

        public static void InitEnvironmentSettings(MyObjectBuilder_EnvironmentSettings environmentBuilder = null)
        {
            if (environmentBuilder != null)
            {
                EnvironmentDefinition = MyDefinitionManager.Static.GetDefinition<MyEnvironmentDefinition>(environmentBuilder.EnvironmentDefinition);
            }
            else if (EnvironmentDefinition == null)
            {
                EnvironmentDefinition = MyDefinitionManager.Static.GetDefinition<MyEnvironmentDefinition>(MyStringHash.GetOrCompute("Default"));
            }
            MyEnvironmentDefinition environmentDefinition = EnvironmentDefinition;
            SunProperties = environmentDefinition.SunProperties;
            FogProperties = environmentDefinition.FogProperties;
            PlanetProperties = environmentDefinition.PlanetProperties;
            SSAOSettings = environmentDefinition.SSAOSettings;
            HBAOSettings = environmentDefinition.HBAOSettings;
            ShadowSettings.CopyFrom(environmentDefinition.ShadowSettings);
            SunRotationAxis = SunProperties.SunRotationAxis;
            MyRenderProxy.UpdateShadowsSettings(ShadowSettings);
            Lodding.UpdatePreset(environmentDefinition.LowLoddingSettings, environmentDefinition.MediumLoddingSettings, environmentDefinition.HighLoddingSettings, environmentDefinition.ExtremeLoddingSettings);
            MyPostprocessSettingsWrapper.Settings = environmentDefinition.PostProcessSettings;
            MyPostprocessSettingsWrapper.MarkDirty();
            if (environmentBuilder != null)
            {
                Vector3 vector;
                Vector3.CreateFromAzimuthAndElevation(environmentBuilder.SunAzimuth, environmentBuilder.SunElevation, out vector);
                vector.Normalize();
                SunProperties.BaseSunDirectionNormalized = vector;
                SunProperties.SunDirectionNormalized = vector;
                FogProperties.FogMultiplier = environmentBuilder.FogMultiplier;
                FogProperties.FogDensity = environmentBuilder.FogDensity;
                FogProperties.FogColor = (Vector3) new Color((Vector3) environmentBuilder.FogColor);
            }
        }

        private void InitSunGlare()
        {
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), "Sun");
            MyFlareDefinition definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            m_sunFlare = MyLights.AddLight();
            if (m_sunFlare != null)
            {
                m_sunFlare.Start("Sun");
                m_sunFlare.GlareOn = MyFakes.SUN_GLARE;
                m_sunFlare.GlareQuerySize = 100000f;
                m_sunFlare.GlareQueryFreqMinMs = 0f;
                m_sunFlare.GlareQueryFreqRndMs = 0f;
                m_sunFlare.GlareType = MyGlareTypeEnum.Distant;
                m_sunFlare.GlareMaxDistance = 2000000f;
                m_sunFlare.LightOn = false;
                if ((definition != null) && (definition.SubGlares != null))
                {
                    m_sunFlare.SubGlares = definition.SubGlares;
                    m_sunFlare.GlareIntensity = definition.Intensity;
                    m_sunFlare.GlareSize = definition.Size;
                }
            }
        }

        public override void LoadData()
        {
            int viewDistance;
            MyCamera camera1 = new MyCamera(MySandboxGame.Config.FieldOfView, MySandboxGame.ScreenViewport);
            if ((MyMultiplayer.Static == null) || !Sync.IsServer)
            {
                viewDistance = MySession.Static.Settings.ViewDistance;
            }
            else
            {
                viewDistance = MySession.Static.Settings.SyncDistance;
            }
            camera1.FarPlaneDistance = viewDistance;
            MainCamera = camera1;
            MyEntities.LoadData();
            this.InitSunGlare();
            UpdateSunLight();
        }

        public static void SaveEnvironmentDefinition()
        {
            EnvironmentDefinition.SunProperties = SunProperties;
            EnvironmentDefinition.FogProperties = FogProperties;
            EnvironmentDefinition.SSAOSettings = SSAOSettings;
            EnvironmentDefinition.HBAOSettings = HBAOSettings;
            EnvironmentDefinition.PostProcessSettings = MyPostprocessSettingsWrapper.Settings;
            EnvironmentDefinition.ShadowSettings.CopyFrom(ShadowSettings);
            EnvironmentDefinition.LowLoddingSettings.CopyFrom(Lodding.LowSettings);
            EnvironmentDefinition.MediumLoddingSettings.CopyFrom(Lodding.MediumSettings);
            EnvironmentDefinition.HighLoddingSettings.CopyFrom(Lodding.HighSettings);
            EnvironmentDefinition.ExtremeLoddingSettings.CopyFrom(Lodding.ExtremeSettings);
            MyObjectBuilder_Definitions definitions = new MyObjectBuilder_Definitions();
            definitions.Environments = new MyObjectBuilder_EnvironmentDefinition[] { (MyObjectBuilder_EnvironmentDefinition) EnvironmentDefinition.GetObjectBuilder() };
            definitions.Save(Path.Combine(MyFileSystem.ContentPath, "Data", "Environment.sbc"));
        }

        private static void SetDefaults()
        {
            SunProperties = MySunProperties.Default;
            FogProperties = MyFogProperties.Default;
            PlanetProperties = MyPlanetProperties.Default;
        }

        public override void Simulate()
        {
            MyEntities.Simulate();
            base.Simulate();
        }

        protected override void UnloadData()
        {
            MyEntities.UnloadData();
            MainCamera = null;
            base.UnloadData();
            if (m_sunFlare != null)
            {
                MyLights.RemoveLight(m_sunFlare);
            }
            m_sunFlare = null;
        }

        public override void UpdateAfterSimulation()
        {
            MyEntities.UpdateAfterSimulation();
            MyGameLogic.UpdateAfterSimulation();
            base.UpdateAfterSimulation();
        }

        public override void UpdateBeforeSimulation()
        {
            MyEntities.UpdateBeforeSimulation();
            MyGameLogic.UpdateBeforeSimulation();
            base.UpdateBeforeSimulation();
        }

        public static void UpdateSunLight()
        {
            if (m_sunFlare != null)
            {
                m_sunFlare.Position = MainCamera.Position + (SunProperties.SunDirectionNormalized * 1000000f);
                m_sunFlare.UpdateLight();
            }
        }

        public override void UpdatingStopped()
        {
            MyEntities.UpdatingStopped();
            MyGameLogic.UpdatingStopped();
            base.UpdatingStopped();
        }

        public static MyCamera MainCamera
        {
            get => 
                m_camera;
            private set
            {
                m_camera = value;
                MyGuiManager.SetCamera(MainCamera);
                MyTransparentGeometry.SetCamera(MainCamera);
            }
        }

        public static Vector3 DirectionToSunNormalized =>
            SunProperties.SunDirectionNormalized;

        public override Type[] Dependencies
        {
            get
            {
                Type[] typeArray1 = new Type[9];
                typeArray1[0] = typeof(MyHud);
                typeArray1[1] = typeof(MyPlanets);
                typeArray1[2] = typeof(MyAntennaSystem);
                typeArray1[3] = typeof(MyGravityProviderSystem);
                typeArray1[4] = typeof(MyIGCSystemSessionComponent);
                typeArray1[5] = typeof(MyUnsafeGridsSessionComponent);
                typeArray1[6] = typeof(MyLights);
                typeArray1[7] = typeof(MyThirdPersonSpectator);
                typeArray1[8] = typeof(MyPhysics);
                return typeArray1;
            }
        }
    }
}

