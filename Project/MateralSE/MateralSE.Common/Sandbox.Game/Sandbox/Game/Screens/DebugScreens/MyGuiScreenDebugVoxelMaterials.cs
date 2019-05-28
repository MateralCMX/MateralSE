namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Definitions;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("Game", "Voxel materials")]
    public class MyGuiScreenDebugVoxelMaterials : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_materialsCombo;
        private MyDx11VoxelMaterialDefinition m_selectedVoxelMaterial;
        private bool m_canUpdate;
        private MyGuiControlSlider m_sliderInitialScale;
        private MyGuiControlSlider m_sliderScaleMultiplier;
        private MyGuiControlSlider m_sliderInitialDistance;
        private MyGuiControlSlider m_sliderDistanceMultiplier;
        private MyGuiControlSlider m_sliderTilingScale;
        private MyGuiControlSlider m_sliderFar1Scale;
        private MyGuiControlSlider m_sliderFar1Distance;
        private MyGuiControlSlider m_sliderFar2Scale;
        private MyGuiControlSlider m_sliderFar2Distance;
        private MyGuiControlSlider m_sliderFar3Distance;
        private MyGuiControlSlider m_sliderFar3Scale;
        private MyGuiControlColor m_colorFar3;
        private MyGuiControlSlider m_sliderExtScale;
        private MyGuiControlSlider m_sliderFriction;
        private MyGuiControlSlider m_sliderRestitution;

        public MyGuiScreenDebugVoxelMaterials() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugVoxelMaterials";

        private void materialsCombo_OnSelect()
        {
            this.m_selectedVoxelMaterial = (MyDx11VoxelMaterialDefinition) MyDefinitionManager.Static.GetVoxelMaterialDefinition((byte) this.m_materialsCombo.GetSelectedKey());
            this.UpdateValues();
        }

        private void OnReloadDefinition(MyGuiControlButton button)
        {
            MyDefinitionManager.Static.ReloadVoxelMaterials();
            this.materialsCombo_OnSelect();
            this.m_selectedVoxelMaterial.UpdateVoxelMaterial();
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Voxel materials", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? textColor = null;
            captionOffset = null;
            this.m_materialsCombo = base.AddCombo(null, textColor, captionOffset, 10);
            foreach (MyVoxelMaterialDefinition definition in (from x in MyDefinitionManager.Static.GetVoxelMaterialDefinitions()
                orderby x.Id.SubtypeName
                select x).ToList<MyVoxelMaterialDefinition>())
            {
                int? sortOrder = null;
                this.m_materialsCombo.AddItem((long) definition.Index, new StringBuilder(definition.Id.SubtypeName), sortOrder, null);
            }
            this.m_materialsCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.materialsCombo_OnSelect);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            textColor = null;
            this.m_sliderInitialScale = base.AddSlider("Initial scale", (float) 0f, (float) 1f, (float) 20f, textColor);
            this.m_sliderInitialScale.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderInitialScale.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderScaleMultiplier = base.AddSlider("Scale multiplier", (float) 0f, (float) 1f, (float) 30f, textColor);
            this.m_sliderScaleMultiplier.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderScaleMultiplier.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderInitialDistance = base.AddSlider("Initial distance", (float) 0f, (float) 1f, (float) 30f, textColor);
            this.m_sliderInitialDistance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderInitialDistance.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderDistanceMultiplier = base.AddSlider("Distance multiplier", (float) 0f, (float) 1f, (float) 30f, textColor);
            this.m_sliderDistanceMultiplier.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderDistanceMultiplier.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            textColor = null;
            this.m_sliderTilingScale = base.AddSlider("Tiling scale", (float) 0f, (float) 1f, (float) 1024f, textColor);
            this.m_sliderTilingScale.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderTilingScale.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            textColor = null;
            this.m_sliderFar1Distance = base.AddSlider("Far1 distance", (float) 0f, (float) 0f, (float) 500f, textColor);
            this.m_sliderFar1Distance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFar1Distance.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderFar1Scale = base.AddSlider("Far1 scale", (float) 0f, (float) 1f, (float) 1000f, textColor);
            this.m_sliderFar1Scale.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFar1Scale.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderFar2Distance = base.AddSlider("Far2 distance", (float) 0f, (float) 0f, (float) 1500f, textColor);
            this.m_sliderFar2Distance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFar2Distance.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderFar2Scale = base.AddSlider("Far2 scale", (float) 0f, (float) 1f, (float) 2000f, textColor);
            this.m_sliderFar2Scale.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFar2Scale.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderFar3Distance = base.AddSlider("Far3 distance", (float) 0f, (float) 0f, (float) 40000f, textColor);
            this.m_sliderFar3Distance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFar3Distance.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderFar3Scale = base.AddSlider("Far3 scale", (float) 0f, (float) 1f, (float) 50000f, textColor);
            this.m_sliderFar3Scale.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFar3Scale.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            textColor = null;
            this.m_sliderExtScale = base.AddSlider("Detail scale (/1000)", (float) 0f, (float) 0f, (float) 10f, textColor);
            this.m_sliderExtScale.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderExtScale.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.01f;
            textColor = null;
            this.m_sliderFriction = base.AddSlider("Friction", 0f, 0f, (float) 2f, (Action<MyGuiControlSlider>) null, textColor);
            this.m_sliderFriction.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderFriction.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            textColor = null;
            this.m_sliderRestitution = base.AddSlider("Restitution", 0f, 0f, (float) 2f, (Action<MyGuiControlSlider>) null, textColor);
            this.m_sliderRestitution.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderRestitution.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            this.m_materialsCombo.SelectItemByIndex(0);
            this.m_colorFar3 = base.AddColor("Far3 color", this.m_selectedVoxelMaterial.RenderParams, MemberHelper.GetMember<Vector4>(Expression.Lambda<Func<Vector4>>(Expression.Field(Expression.Field(Expression.Field(Expression.Constant(this, typeof(MyGuiScreenDebugVoxelMaterials)), fieldof(MyGuiScreenDebugVoxelMaterials.m_selectedVoxelMaterial)), fieldof(MyVoxelMaterialDefinition.RenderParams)), fieldof(MyRenderVoxelMaterialData.Far3Color)), Array.Empty<ParameterExpression>())));
            this.m_colorFar3.SetColor(this.m_selectedVoxelMaterial.RenderParams.Far3Color);
            this.m_colorFar3.OnChange += new Action<MyGuiControlColor>(this.ValueChanged);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.01f;
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Reload definition"), new Action<MyGuiControlButton>(this.OnReloadDefinition), null, textColor, captionOffset, true, true);
        }

        private void UpdateValues()
        {
            this.m_canUpdate = false;
            this.m_sliderInitialScale.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.InitialScale;
            this.m_sliderScaleMultiplier.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.ScaleMultiplier;
            this.m_sliderInitialDistance.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.InitialDistance;
            this.m_sliderDistanceMultiplier.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.DistanceMultiplier;
            this.m_sliderTilingScale.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.TilingScale;
            this.m_sliderFar1Scale.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far1Scale;
            this.m_sliderFar1Distance.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far1Distance;
            this.m_sliderFar2Scale.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far2Scale;
            this.m_sliderFar2Distance.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far2Distance;
            this.m_sliderFar3Scale.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far3Scale;
            this.m_sliderFar3Distance.Value = this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far3Distance;
            if (this.m_colorFar3 != null)
            {
                this.m_colorFar3.SetColor(this.m_selectedVoxelMaterial.RenderParams.Far3Color);
            }
            this.m_sliderExtScale.Value = 1000f * this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.ExtensionDetailScale;
            this.m_sliderFriction.Value = this.m_selectedVoxelMaterial.Friction;
            this.m_sliderRestitution.Value = this.m_selectedVoxelMaterial.Restitution;
            this.m_canUpdate = true;
        }

        private void ValueChanged(MyGuiControlBase sender)
        {
            if (this.m_canUpdate)
            {
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.InitialScale = this.m_sliderInitialScale.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.ScaleMultiplier = this.m_sliderScaleMultiplier.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.InitialDistance = this.m_sliderInitialDistance.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.DistanceMultiplier = this.m_sliderDistanceMultiplier.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.TilingScale = this.m_sliderTilingScale.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far1Scale = this.m_sliderFar1Scale.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far1Distance = this.m_sliderFar1Distance.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far2Scale = this.m_sliderFar2Scale.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far2Distance = this.m_sliderFar2Distance.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far3Scale = this.m_sliderFar3Scale.Value;
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.Far3Distance = this.m_sliderFar3Distance.Value;
                this.m_selectedVoxelMaterial.RenderParams.Far3Color = (Vector4) this.m_colorFar3.GetColor();
                this.m_selectedVoxelMaterial.RenderParams.StandardTilingSetup.ExtensionDetailScale = this.m_sliderExtScale.Value / 1000f;
                this.m_selectedVoxelMaterial.Friction = this.m_sliderFriction.Value;
                this.m_selectedVoxelMaterial.Restitution = this.m_sliderRestitution.Value;
                this.m_selectedVoxelMaterial.UpdateVoxelMaterial();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugVoxelMaterials.<>c <>9 = new MyGuiScreenDebugVoxelMaterials.<>c();
            public static Func<MyVoxelMaterialDefinition, string> <>9__20_0;

            internal string <RecreateControls>b__20_0(MyVoxelMaterialDefinition x) => 
                x.Id.SubtypeName;
        }
    }
}

