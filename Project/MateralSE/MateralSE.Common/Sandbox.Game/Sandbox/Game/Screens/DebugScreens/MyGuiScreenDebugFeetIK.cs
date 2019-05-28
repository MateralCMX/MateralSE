namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("VRage", "Character feet IK")]
    internal class MyGuiScreenDebugFeetIK : MyGuiScreenDebugBase
    {
        private MyGuiControlSlider belowReachableDistance;
        private MyGuiControlSlider aboveReachableDistance;
        private MyGuiControlSlider verticalChangeUpGain;
        private MyGuiControlSlider verticalChangeDownGain;
        private MyGuiControlSlider ankleHeight;
        private MyGuiControlSlider footWidth;
        private MyGuiControlSlider footLength;
        private MyGuiControlCombobox characterMovementStateCombo;
        private MyGuiControlCheckbox enabledIKState;
        public static bool ikSettingsEnabled;
        private MyFeetIKSettings ikSettings;
        public bool updating;

        public MyGuiScreenDebugFeetIK() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void characterMovementStateCombo_ItemSelected()
        {
            MyCharacterMovementEnum selectedKey = (MyCharacterMovementEnum) ((ushort) this.characterMovementStateCombo.GetSelectedKey());
            MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_MOVEMENT_STATE = selectedKey;
            if (!MySession.Static.LocalCharacter.Definition.FeetIKSettings.TryGetValue(selectedKey, out this.ikSettings))
            {
                this.ikSettings = new MyFeetIKSettings();
                this.ikSettings.Enabled = false;
                this.ikSettings.AboveReachableDistance = 0.1f;
                this.ikSettings.BelowReachableDistance = 0.1f;
                this.ikSettings.VerticalShiftDownGain = 0.1f;
                this.ikSettings.VerticalShiftUpGain = 0.1f;
                this.ikSettings.FootSize = new Vector3(0.1f, 0.1f, 0.2f);
            }
            this.updating = true;
            this.enabledIKState.IsChecked = this.ikSettings.Enabled;
            this.belowReachableDistance.Value = this.ikSettings.BelowReachableDistance;
            this.aboveReachableDistance.Value = this.ikSettings.AboveReachableDistance;
            this.verticalChangeUpGain.Value = this.ikSettings.VerticalShiftUpGain;
            this.verticalChangeDownGain.Value = this.ikSettings.VerticalShiftDownGain;
            this.ankleHeight.Value = this.ikSettings.FootSize.Y;
            this.footWidth.Value = this.ikSettings.FootSize.X;
            this.footLength.Value = this.ikSettings.FootSize.Z;
            this.updating = false;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugFeetIK";

        private void IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            this.ItemChanged(null);
        }

        private void ItemChanged(MyGuiControlSlider slider)
        {
            if (!this.updating)
            {
                this.ikSettings.Enabled = this.enabledIKState.IsChecked;
                this.ikSettings.BelowReachableDistance = this.belowReachableDistance.Value;
                this.ikSettings.AboveReachableDistance = this.aboveReachableDistance.Value;
                this.ikSettings.VerticalShiftUpGain = this.verticalChangeUpGain.Value;
                this.ikSettings.VerticalShiftDownGain = this.verticalChangeDownGain.Value;
                this.ikSettings.FootSize.Y = this.ankleHeight.Value;
                this.ikSettings.FootSize.X = this.footWidth.Value;
                this.ikSettings.FootSize.Z = this.footLength.Value;
                MyCharacterMovementEnum standing = MyCharacterMovementEnum.Standing;
                MySession.Static.LocalCharacter.Definition.FeetIKSettings[standing] = this.ikSettings;
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Character feet IK debug draw", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Draw IK Settings ", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_SETTINGS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw ankle final position", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_ANKLE_FINALPOS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw raycast lines and foot lines", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_RAYCASTLINE)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw bones", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_BONES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw raycast hits", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_RAYCASTHITS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw ankle desired positions", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_ANKLE_DESIREDPOSITION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw closest support position", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_CLOSESTSUPPORTPOSITION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw IK solvers debug", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_IKSOLVERS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Enable/Disable Feet IK", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_FOOT_IK)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.enabledIKState = base.AddCheckBox("Enable IK for this state", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyGuiScreenDebugFeetIK.ikSettingsEnabled)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            this.belowReachableDistance = base.AddSlider("Reachable distance below character", (float) 0f, (float) 0f, (float) 2f, color);
            color = null;
            this.aboveReachableDistance = base.AddSlider("Reachable distance above character", (float) 0f, (float) 0f, (float) 2f, color);
            color = null;
            this.verticalChangeUpGain = base.AddSlider("Shift Up Gain", (float) 0.1f, (float) 0f, (float) 1f, color);
            color = null;
            this.verticalChangeDownGain = base.AddSlider("Sift Down Gain", (float) 0.1f, (float) 0f, (float) 1f, color);
            color = null;
            this.ankleHeight = base.AddSlider("Ankle height", (float) 0.1f, (float) 0.001f, (float) 0.3f, color);
            color = null;
            this.footWidth = base.AddSlider("Foot width", (float) 0.1f, (float) 0.001f, (float) 0.3f, color);
            color = null;
            this.footLength = base.AddSlider("Foot length", (float) 0.3f, (float) 0.001f, (float) 0.2f, color);
            this.RegisterEvents();
        }

        private void RegisterEvents()
        {
            this.belowReachableDistance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.belowReachableDistance.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.aboveReachableDistance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.aboveReachableDistance.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.verticalChangeUpGain.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.verticalChangeUpGain.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.verticalChangeDownGain.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.verticalChangeDownGain.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.ankleHeight.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.ankleHeight.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.footWidth.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.footWidth.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.footLength.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.footLength.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.enabledIKState.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.enabledIKState.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.IsCheckedChanged));
        }

        private void UnRegisterEvents()
        {
            this.characterMovementStateCombo.ItemSelected -= new MyGuiControlCombobox.ItemSelectedDelegate(this.characterMovementStateCombo_ItemSelected);
            this.belowReachableDistance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.belowReachableDistance.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.aboveReachableDistance.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.aboveReachableDistance.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.verticalChangeUpGain.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.verticalChangeUpGain.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.verticalChangeDownGain.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.verticalChangeDownGain.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.ankleHeight.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.ankleHeight.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.footWidth.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.footWidth.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.footLength.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.footLength.ValueChanged, new Action<MyGuiControlSlider>(this.ItemChanged));
            this.enabledIKState.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.enabledIKState.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.IsCheckedChanged));
        }

        public MyRagdollMapper PlayerRagdollMapper
        {
            get
            {
                MyCharacterRagdollComponent component = MySession.Static.LocalCharacter.Components.Get<MyCharacterRagdollComponent>();
                return ((component != null) ? component.RagdollMapper : null);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugFeetIK.<>c <>9 = new MyGuiScreenDebugFeetIK.<>c();
        }
    }
}

