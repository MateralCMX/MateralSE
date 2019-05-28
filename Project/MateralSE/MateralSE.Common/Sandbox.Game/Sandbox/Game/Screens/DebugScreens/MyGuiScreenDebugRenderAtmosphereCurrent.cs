namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [MyDebugScreen("Render", "Atmosphere Current")]
    public class MyGuiScreenDebugRenderAtmosphereCurrent : MyGuiScreenDebugBase
    {
        private static long m_selectedPlanetEntityID;
        private static MyAtmosphereSettings m_originalAtmosphereSettings;
        private static MyAtmosphereSettings m_atmosphereSettings;
        private static bool m_atmosphereEnabled = true;

        public MyGuiScreenDebugRenderAtmosphereCurrent() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void OnResetButtonClicked(MyGuiControlButton button)
        {
            m_atmosphereSettings = MyAtmosphereSettings.Defaults();
            this.RecreateControls(false);
            this.UpdateAtmosphere();
        }

        private void OnRestoreButtonClicked(MyGuiControlButton button)
        {
            m_atmosphereSettings = m_originalAtmosphereSettings;
            this.RecreateControls(false);
            this.UpdateAtmosphere();
        }

        private void PickPlanet()
        {
            List<MyLineSegmentOverlapResult<MyEntity>> list = new List<MyLineSegmentOverlapResult<MyEntity>>();
            LineD ray = new LineD(MySector.MainCamera.Position, MySector.MainCamera.ForwardVector);
            MyGamePruningStructure.GetAllEntitiesInRay(ref ray, list, MyEntityQueryType.Both);
            float maxValue = float.MaxValue;
            MyPlanet planet = null;
            foreach (MyLineSegmentOverlapResult<MyEntity> result in list)
            {
                MyPlanet element = result.Element as MyPlanet;
                if ((element != null) && ((element.EntityId != m_selectedPlanetEntityID) && (result.Distance < maxValue)))
                {
                    planet = element;
                }
            }
            if (planet != null)
            {
                m_selectedPlanetEntityID = planet.EntityId;
                m_atmosphereSettings = planet.AtmosphereSettings;
                m_originalAtmosphereSettings = m_atmosphereSettings;
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Atmosphere Current", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            this.PickPlanet();
            if (SelectedPlanet != null)
            {
                if (m_atmosphereSettings.MieColorScattering.X == 0f)
                {
                    m_atmosphereSettings.MieColorScattering = new Vector3(m_atmosphereSettings.MieScattering);
                }
                if (m_atmosphereSettings.Intensity == 0f)
                {
                    m_atmosphereSettings.Intensity = 1f;
                }
                base.AddLabel("Atmosphere Settings", (Vector4) Color.White, 1f, null, "Debug");
                Vector4? color = null;
                this.AddSlider("Rayleigh Scattering R", 1f, 100f, () => m_atmosphereSettings.RayleighScattering.X, delegate (float f) {
                    m_atmosphereSettings.RayleighScattering.X = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Rayleigh Scattering G", 1f, 100f, () => m_atmosphereSettings.RayleighScattering.Y, delegate (float f) {
                    m_atmosphereSettings.RayleighScattering.Y = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Rayleigh Scattering B", 1f, 100f, () => m_atmosphereSettings.RayleighScattering.Z, delegate (float f) {
                    m_atmosphereSettings.RayleighScattering.Z = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Mie Scattering R", 5f, 150f, () => m_atmosphereSettings.MieColorScattering.X, delegate (float f) {
                    m_atmosphereSettings.MieColorScattering.X = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Mie Scattering G", 5f, 150f, () => m_atmosphereSettings.MieColorScattering.Y, delegate (float f) {
                    m_atmosphereSettings.MieColorScattering.Y = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Mie Scattering B", 5f, 150f, () => m_atmosphereSettings.MieColorScattering.Z, delegate (float f) {
                    m_atmosphereSettings.MieColorScattering.Z = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Rayleigh Height Surfrace", 1f, 50f, () => m_atmosphereSettings.RayleighHeight, delegate (float f) {
                    m_atmosphereSettings.RayleighHeight = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Rayleigh Height Space", 1f, 25f, () => m_atmosphereSettings.RayleighHeightSpace, delegate (float f) {
                    m_atmosphereSettings.RayleighHeightSpace = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Rayleigh Transition", 0.1f, 1.5f, () => m_atmosphereSettings.RayleighTransitionModifier, delegate (float f) {
                    m_atmosphereSettings.RayleighTransitionModifier = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Mie Height", 5f, 200f, () => m_atmosphereSettings.MieHeight, delegate (float f) {
                    m_atmosphereSettings.MieHeight = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Sun size", 0.99f, 1f, () => m_atmosphereSettings.MieG, delegate (float f) {
                    m_atmosphereSettings.MieG = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Sea floor modifier", 0.9f, 1.1f, () => m_atmosphereSettings.SeaLevelModifier, delegate (float f) {
                    m_atmosphereSettings.SeaLevelModifier = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Atmosphere top modifier", 0.9f, 1.1f, () => m_atmosphereSettings.AtmosphereTopModifier, delegate (float f) {
                    m_atmosphereSettings.AtmosphereTopModifier = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Intensity", 0.1f, 200f, () => m_atmosphereSettings.Intensity, delegate (float f) {
                    m_atmosphereSettings.Intensity = f;
                    this.UpdateAtmosphere();
                }, color);
                color = null;
                this.AddSlider("Fog Intensity", 0f, 1f, () => m_atmosphereSettings.FogIntensity, delegate (float f) {
                    m_atmosphereSettings.FogIntensity = f;
                    this.UpdateAtmosphere();
                }, color);
                base.AddColor("Sun Light Color", m_atmosphereSettings.SunColor, delegate (MyGuiControlColor v) {
                    m_atmosphereSettings.SunColor = (Vector3) v.Color;
                    this.UpdateAtmosphere();
                });
                base.AddColor("Sun Light Specular Color", m_atmosphereSettings.SunSpecularColor, delegate (MyGuiControlColor v) {
                    m_atmosphereSettings.SunSpecularColor = (Vector3) v.Color;
                    this.UpdateAtmosphere();
                });
                color = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Restore"), new Action<MyGuiControlButton>(this.OnRestoreButtonClicked), null, color, captionOffset, true, true);
                color = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Earth settings"), new Action<MyGuiControlButton>(this.OnResetButtonClicked), null, color, captionOffset, true, true);
            }
        }

        private void UpdateAtmosphere()
        {
            if (SelectedPlanet != null)
            {
                SelectedPlanet.AtmosphereSettings = m_atmosphereSettings;
            }
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderPlanetSettings settings = new MyRenderPlanetSettings {
                AtmosphereIntensityMultiplier = MySector.PlanetProperties.AtmosphereIntensityMultiplier,
                AtmosphereIntensityAmbientMultiplier = MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier,
                AtmosphereDesaturationFactorForward = MySector.PlanetProperties.AtmosphereDesaturationFactorForward,
                CloudsIntensityMultiplier = MySector.PlanetProperties.CloudsIntensityMultiplier
            };
            MyRenderProxy.UpdatePlanetSettings(ref settings);
        }

        private static MyPlanet SelectedPlanet
        {
            get
            {
                MyEntity entity;
                return (!MyEntities.TryGetEntityById(m_selectedPlanetEntityID, out entity, false) ? null : (entity as MyPlanet));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderAtmosphereCurrent.<>c <>9 = new MyGuiScreenDebugRenderAtmosphereCurrent.<>c();
            public static Func<float> <>9__7_0;
            public static Func<float> <>9__7_2;
            public static Func<float> <>9__7_4;
            public static Func<float> <>9__7_6;
            public static Func<float> <>9__7_8;
            public static Func<float> <>9__7_10;
            public static Func<float> <>9__7_12;
            public static Func<float> <>9__7_14;
            public static Func<float> <>9__7_16;
            public static Func<float> <>9__7_18;
            public static Func<float> <>9__7_20;
            public static Func<float> <>9__7_22;
            public static Func<float> <>9__7_24;
            public static Func<float> <>9__7_26;
            public static Func<float> <>9__7_28;

            internal float <RecreateControls>b__7_0() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.RayleighScattering.X;

            internal float <RecreateControls>b__7_10() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.MieColorScattering.Z;

            internal float <RecreateControls>b__7_12() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.RayleighHeight;

            internal float <RecreateControls>b__7_14() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.RayleighHeightSpace;

            internal float <RecreateControls>b__7_16() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.RayleighTransitionModifier;

            internal float <RecreateControls>b__7_18() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.MieHeight;

            internal float <RecreateControls>b__7_2() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.RayleighScattering.Y;

            internal float <RecreateControls>b__7_20() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.MieG;

            internal float <RecreateControls>b__7_22() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.SeaLevelModifier;

            internal float <RecreateControls>b__7_24() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.AtmosphereTopModifier;

            internal float <RecreateControls>b__7_26() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.Intensity;

            internal float <RecreateControls>b__7_28() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.FogIntensity;

            internal float <RecreateControls>b__7_4() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.RayleighScattering.Z;

            internal float <RecreateControls>b__7_6() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.MieColorScattering.X;

            internal float <RecreateControls>b__7_8() => 
                MyGuiScreenDebugRenderAtmosphereCurrent.m_atmosphereSettings.MieColorScattering.Y;
        }
    }
}

