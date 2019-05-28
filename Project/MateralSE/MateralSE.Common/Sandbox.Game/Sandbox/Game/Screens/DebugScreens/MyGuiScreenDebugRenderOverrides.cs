namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Overrides")]
    internal class MyGuiScreenDebugRenderOverrides : MyGuiScreenDebugBase
    {
        private MyGuiControlCheckbox m_lighting;
        private MyGuiControlCheckbox m_sun;
        private MyGuiControlCheckbox m_backLight;
        private MyGuiControlCheckbox m_pointLights;
        private MyGuiControlCheckbox m_spotLights;
        private MyGuiControlCheckbox m_envLight;
        private MyGuiControlCheckbox m_transparent;
        private MyGuiControlCheckbox m_oit;
        private MyGuiControlCheckbox m_billboardsDynamic;
        private MyGuiControlCheckbox m_billboardsStatic;
        private MyGuiControlCheckbox m_gpuParticles;
        private MyGuiControlCheckbox m_atmosphere;
        private MyGuiControlCheckbox m_cloud;
        private MyGuiControlCheckbox m_postprocess;
        private MyGuiControlCheckbox m_ssao;
        private MyGuiControlCheckbox m_bloom;
        private MyGuiControlCheckbox m_fxaa;
        private MyGuiControlCheckbox m_tonemapping;

        public MyGuiScreenDebugRenderOverrides() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderLayers";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            Vector2? captionOffset = null;
            base.AddCaption("Overrides", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Lighting Pass", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            this.m_lighting = base.AddCheckBox("Enabled", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Lighting)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_sun = base.AddCheckBox("Sun", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Sun)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_backLight = base.AddCheckBox("Back light", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.BackLight)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_pointLights = base.AddCheckBox("Point lights", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.PointLights)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_spotLights = base.AddCheckBox("Spot lights", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.SpotLights)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_envLight = base.AddCheckBox("Env light", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.EnvLight)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            color = null;
            captionOffset = null;
            base.AddCheckBox("Shadows", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Shadows)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Fog", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Fog)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Flares", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Flares)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            base.AddLabel("Transparent Pass", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            captionOffset = null;
            this.m_transparent = base.AddCheckBox("Enabled", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Transparent)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_oit = base.AddCheckBox("Order independent", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.OIT)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_billboardsDynamic = base.AddCheckBox("Billboards dynamic", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.BillboardsDynamic)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_billboardsStatic = base.AddCheckBox("Billboards static", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.BillboardsStatic)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_gpuParticles = base.AddCheckBox("GPU Particles", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.GPUParticles)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_cloud = base.AddCheckBox("Cloud", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Clouds)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_atmosphere = base.AddCheckBox("Atmosphere", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Atmosphere)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.01f;
            base.AddLabel("Postprocessing", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            captionOffset = null;
            this.m_postprocess = base.AddCheckBox("Enabled", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Postprocessing)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_ssao = base.AddCheckBox("SSAO", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.SSAO)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_bloom = base.AddCheckBox("Bloom", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Bloom)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_fxaa = base.AddCheckBox("Fxaa", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Fxaa)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_tonemapping = base.AddCheckBox("Tonemapping", MyRenderProxy.DebugOverrides, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(null, fieldof(MyRenderProxy.DebugOverrides)), fieldof(MyRenderDebugOverrides.Tonemapping)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.01f;
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            base.ValueChanged(sender);
            MyRenderProxy.UpdateDebugOverrides();
            this.m_sun.Enabled = this.m_lighting.IsChecked;
            this.m_backLight.Enabled = this.m_lighting.IsChecked;
            this.m_pointLights.Enabled = this.m_lighting.IsChecked;
            this.m_spotLights.Enabled = this.m_lighting.IsChecked;
            this.m_envLight.Enabled = this.m_lighting.IsChecked;
            this.m_oit.Enabled = this.m_transparent.IsChecked;
            this.m_billboardsDynamic.Enabled = this.m_transparent.IsChecked;
            this.m_billboardsStatic.Enabled = this.m_transparent.IsChecked;
            this.m_gpuParticles.Enabled = this.m_transparent.IsChecked;
            this.m_atmosphere.Enabled = this.m_transparent.IsChecked;
            this.m_cloud.Enabled = this.m_transparent.IsChecked;
            this.m_ssao.Enabled = this.m_postprocess.IsChecked;
            this.m_bloom.Enabled = this.m_postprocess.IsChecked;
            this.m_fxaa.Enabled = this.m_postprocess.IsChecked;
            this.m_tonemapping.Enabled = this.m_postprocess.IsChecked;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderOverrides.<>c <>9 = new MyGuiScreenDebugRenderOverrides.<>c();
        }
    }
}

