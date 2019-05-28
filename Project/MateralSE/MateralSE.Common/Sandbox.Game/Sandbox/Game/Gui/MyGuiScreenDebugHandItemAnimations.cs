namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("Game", "Hand item animations")]
    internal class MyGuiScreenDebugHandItemAnimations : MyGuiScreenDebugHandItemBase
    {
        private Matrix m_storedWalkingItem;
        private bool m_canUpdateValues = true;
        private float m_itemWalkingRotationX;
        private float m_itemWalkingRotationY;
        private float m_itemWalkingRotationZ;
        private float m_itemWalkingPositionX;
        private float m_itemWalkingPositionY;
        private float m_itemWalkingPositionZ;
        private MyGuiControlSlider m_itemWalkingRotationXSlider;
        private MyGuiControlSlider m_itemWalkingRotationYSlider;
        private MyGuiControlSlider m_itemWalkingRotationZSlider;
        private MyGuiControlSlider m_itemWalkingPositionXSlider;
        private MyGuiControlSlider m_itemWalkingPositionYSlider;
        private MyGuiControlSlider m_itemWalkingPositionZSlider;
        private MyGuiControlSlider m_blendTimeSlider;
        private MyGuiControlSlider m_xAmplitudeOffsetSlider;
        private MyGuiControlSlider m_yAmplitudeOffsetSlider;
        private MyGuiControlSlider m_zAmplitudeOffsetSlider;
        private MyGuiControlSlider m_xAmplitudeScaleSlider;
        private MyGuiControlSlider m_yAmplitudeScaleSlider;
        private MyGuiControlSlider m_zAmplitudeScaleSlider;
        private MyGuiControlSlider m_runMultiplierSlider;
        private MyGuiControlCheckbox m_simulateLeftHandCheckbox;
        private MyGuiControlCheckbox m_simulateRightHandCheckbox;

        public MyGuiScreenDebugHandItemAnimations()
        {
            this.RecreateControls(true);
        }

        private void AmplitudeChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                base.CurrentSelectedItem.BlendTime = this.m_blendTimeSlider.Value;
                base.CurrentSelectedItem.XAmplitudeOffset = this.m_xAmplitudeOffsetSlider.Value;
                base.CurrentSelectedItem.YAmplitudeOffset = this.m_yAmplitudeOffsetSlider.Value;
                base.CurrentSelectedItem.ZAmplitudeOffset = this.m_zAmplitudeOffsetSlider.Value;
                base.CurrentSelectedItem.XAmplitudeScale = this.m_xAmplitudeScaleSlider.Value;
                base.CurrentSelectedItem.YAmplitudeScale = this.m_yAmplitudeScaleSlider.Value;
                base.CurrentSelectedItem.ZAmplitudeScale = this.m_zAmplitudeScaleSlider.Value;
                base.CurrentSelectedItem.RunMultiplier = this.m_runMultiplierSlider.Value;
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugHandItemsAnimations";

        protected override void handItemsCombo_ItemSelected()
        {
            base.handItemsCombo_ItemSelected();
            this.m_storedWalkingItem = base.CurrentSelectedItem.ItemWalkingLocation;
            this.UpdateValues();
        }

        private void OnRun(MyGuiControlButton button)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            localCharacter.SwitchAnimation(MyCharacterMovementEnum.Sprinting, true);
            localCharacter.SetCurrentMovementState(MyCharacterMovementEnum.Sprinting);
        }

        private void OnWalk(MyGuiControlButton button)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            localCharacter.SwitchAnimation(MyCharacterMovementEnum.Walking, true);
            localCharacter.SetCurrentMovementState(MyCharacterMovementEnum.Walking);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Hand item animations", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.RecreateHandItemsCombo();
            base.m_sliderDebugScale = 0.6f;
            Vector4? color = null;
            this.m_itemWalkingRotationXSlider = base.AddSlider("Walk item rotation X", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemWalkingRotationXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
            color = null;
            this.m_itemWalkingRotationYSlider = base.AddSlider("Walk item rotation Y", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemWalkingRotationYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
            color = null;
            this.m_itemWalkingRotationZSlider = base.AddSlider("Walk item rotation Z", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemWalkingRotationZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
            color = null;
            this.m_itemWalkingPositionXSlider = base.AddSlider("Walk item position X", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemWalkingPositionXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
            color = null;
            this.m_itemWalkingPositionYSlider = base.AddSlider("Walk item position Y", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemWalkingPositionYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
            color = null;
            this.m_itemWalkingPositionZSlider = base.AddSlider("Walk item position Z", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemWalkingPositionZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
            color = null;
            this.m_blendTimeSlider = base.AddSlider("Blend time", (float) 0f, (float) 0.001f, (float) 1f, color);
            this.m_blendTimeSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_xAmplitudeOffsetSlider = base.AddSlider("X offset amplitude", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_xAmplitudeOffsetSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_yAmplitudeOffsetSlider = base.AddSlider("Y offset amplitude", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_yAmplitudeOffsetSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_zAmplitudeOffsetSlider = base.AddSlider("Z offset amplitude", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_zAmplitudeOffsetSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_xAmplitudeScaleSlider = base.AddSlider("X scale amplitude", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_xAmplitudeScaleSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_yAmplitudeScaleSlider = base.AddSlider("Y scale amplitude", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_yAmplitudeScaleSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_zAmplitudeScaleSlider = base.AddSlider("Z scale amplitude", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_zAmplitudeScaleSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            this.m_runMultiplierSlider = base.AddSlider("Run multiplier", (float) 0f, (float) -5f, (float) 5f, color);
            this.m_runMultiplierSlider.ValueChanged = new Action<MyGuiControlSlider>(this.AmplitudeChanged);
            color = null;
            captionOffset = null;
            this.m_simulateLeftHandCheckbox = base.AddCheckBox("Simulate left hand", false, new Action<MyGuiControlCheckbox>(this.SimulateHandChanged), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_simulateRightHandCheckbox = base.AddCheckBox("Simulate right hand", false, new Action<MyGuiControlCheckbox>(this.SimulateHandChanged), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Walk!"), new Action<MyGuiControlButton>(this.OnWalk), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Run!"), new Action<MyGuiControlButton>(this.OnRun), null, color, captionOffset, true, true);
            base.RecreateSaveAndReloadButtons();
            base.SelectFirstHandItem();
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }

        private void SimulateHandChanged(MyGuiControlCheckbox checkbox)
        {
            if (this.m_canUpdateValues)
            {
                base.CurrentSelectedItem.SimulateLeftHand = this.m_simulateLeftHandCheckbox.IsChecked;
                base.CurrentSelectedItem.SimulateRightHand = this.m_simulateRightHandCheckbox.IsChecked;
            }
        }

        private void UpdateValues()
        {
            this.m_itemWalkingRotationX = 0f;
            this.m_itemWalkingRotationY = 0f;
            this.m_itemWalkingRotationZ = 0f;
            this.m_itemWalkingPositionX = this.m_storedWalkingItem.Translation.X;
            this.m_itemWalkingPositionY = this.m_storedWalkingItem.Translation.Y;
            this.m_itemWalkingPositionZ = this.m_storedWalkingItem.Translation.Z;
            this.m_canUpdateValues = false;
            this.m_itemWalkingRotationXSlider.Value = this.m_itemWalkingRotationX;
            this.m_itemWalkingRotationYSlider.Value = this.m_itemWalkingRotationY;
            this.m_itemWalkingRotationZSlider.Value = this.m_itemWalkingRotationZ;
            this.m_itemWalkingPositionXSlider.Value = this.m_itemWalkingPositionX;
            this.m_itemWalkingPositionYSlider.Value = this.m_itemWalkingPositionY;
            this.m_itemWalkingPositionZSlider.Value = this.m_itemWalkingPositionZ;
            this.m_blendTimeSlider.Value = base.CurrentSelectedItem.BlendTime;
            this.m_xAmplitudeOffsetSlider.Value = base.CurrentSelectedItem.XAmplitudeOffset;
            this.m_yAmplitudeOffsetSlider.Value = base.CurrentSelectedItem.YAmplitudeOffset;
            this.m_zAmplitudeOffsetSlider.Value = base.CurrentSelectedItem.ZAmplitudeOffset;
            this.m_xAmplitudeScaleSlider.Value = base.CurrentSelectedItem.XAmplitudeScale;
            this.m_yAmplitudeScaleSlider.Value = base.CurrentSelectedItem.YAmplitudeScale;
            this.m_zAmplitudeScaleSlider.Value = base.CurrentSelectedItem.ZAmplitudeScale;
            this.m_runMultiplierSlider.Value = base.CurrentSelectedItem.RunMultiplier;
            this.m_simulateLeftHandCheckbox.IsChecked = base.CurrentSelectedItem.SimulateLeftHand;
            this.m_simulateRightHandCheckbox.IsChecked = base.CurrentSelectedItem.SimulateRightHand;
            this.m_canUpdateValues = true;
        }

        private void WalkingItemChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_itemWalkingRotationX = this.m_itemWalkingRotationXSlider.Value;
                this.m_itemWalkingRotationY = this.m_itemWalkingRotationYSlider.Value;
                this.m_itemWalkingRotationZ = this.m_itemWalkingRotationZSlider.Value;
                this.m_itemWalkingPositionX = this.m_itemWalkingPositionXSlider.Value;
                this.m_itemWalkingPositionY = this.m_itemWalkingPositionYSlider.Value;
                this.m_itemWalkingPositionZ = this.m_itemWalkingPositionZSlider.Value;
                base.CurrentSelectedItem.ItemWalkingLocation = ((this.m_storedWalkingItem * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_itemWalkingRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_itemWalkingRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_itemWalkingRotationZ));
                base.CurrentSelectedItem.ItemWalkingLocation.Translation = new Vector3(this.m_itemWalkingPositionX, this.m_itemWalkingPositionY, this.m_itemWalkingPositionZ);
            }
        }
    }
}

