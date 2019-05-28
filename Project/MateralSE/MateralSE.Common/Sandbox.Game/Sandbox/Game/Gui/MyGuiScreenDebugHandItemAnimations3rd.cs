namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("Game", "Hand item animations 3rd")]
    internal class MyGuiScreenDebugHandItemAnimations3rd : MyGuiScreenDebugHandItemBase
    {
        private Matrix m_storedItem;
        private Matrix m_storedWalkingItem;
        private bool m_canUpdateValues = true;
        private float m_itemRotationX;
        private float m_itemRotationY;
        private float m_itemRotationZ;
        private float m_itemPositionX;
        private float m_itemPositionY;
        private float m_itemPositionZ;
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
        private MyGuiControlSlider m_itemRotationXSlider;
        private MyGuiControlSlider m_itemRotationYSlider;
        private MyGuiControlSlider m_itemRotationZSlider;
        private MyGuiControlSlider m_itemPositionXSlider;
        private MyGuiControlSlider m_itemPositionYSlider;
        private MyGuiControlSlider m_itemPositionZSlider;
        private MyGuiControlSlider m_amplitudeMultiplierSlider;

        public MyGuiScreenDebugHandItemAnimations3rd()
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugHandItemsAnimations3rd";

        protected override void handItemsCombo_ItemSelected()
        {
            base.handItemsCombo_ItemSelected();
            this.m_storedWalkingItem = base.CurrentSelectedItem.ItemWalkingLocation3rd;
            this.m_storedItem = base.CurrentSelectedItem.ItemLocation3rd;
            this.UpdateValues();
        }

        private void ItemChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_itemRotationX = this.m_itemRotationXSlider.Value;
                this.m_itemRotationY = this.m_itemRotationYSlider.Value;
                this.m_itemRotationZ = this.m_itemRotationZSlider.Value;
                this.m_itemPositionX = this.m_itemPositionXSlider.Value;
                this.m_itemPositionY = this.m_itemPositionYSlider.Value;
                this.m_itemPositionZ = this.m_itemPositionZSlider.Value;
                base.CurrentSelectedItem.ItemLocation3rd = ((this.m_storedItem * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_itemRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_itemRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_itemRotationZ));
                base.CurrentSelectedItem.ItemLocation3rd.Translation = new Vector3(this.m_itemPositionX, this.m_itemPositionY, this.m_itemPositionZ);
            }
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
            base.AddCaption("Hand item animations 3rd", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.RecreateHandItemsCombo();
            base.m_sliderDebugScale = 0.6f;
            Vector4? color = null;
            this.m_itemRotationXSlider = base.AddSlider("item rotation X", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemRotationYSlider = base.AddSlider("item rotation Y", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemRotationZSlider = base.AddSlider("item rotation Z", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionXSlider = base.AddSlider("item position X", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionYSlider = base.AddSlider("item position Y", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionZSlider = base.AddSlider("item position Z", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
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
            this.m_amplitudeMultiplierSlider = base.AddSlider("Amplitude multiplier", (float) 0f, (float) -1f, (float) 3f, color);
            this.m_amplitudeMultiplierSlider.ValueChanged = new Action<MyGuiControlSlider>(this.WalkingItemChanged);
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

        private void UpdateValues()
        {
            this.m_itemWalkingRotationX = 0f;
            this.m_itemWalkingRotationY = 0f;
            this.m_itemWalkingRotationZ = 0f;
            this.m_itemWalkingPositionX = this.m_storedWalkingItem.Translation.X;
            this.m_itemWalkingPositionY = this.m_storedWalkingItem.Translation.Y;
            this.m_itemWalkingPositionZ = this.m_storedWalkingItem.Translation.Z;
            this.m_itemRotationX = 0f;
            this.m_itemRotationY = 0f;
            this.m_itemRotationZ = 0f;
            this.m_itemPositionX = this.m_storedItem.Translation.X;
            this.m_itemPositionY = this.m_storedItem.Translation.Y;
            this.m_itemPositionZ = this.m_storedItem.Translation.Z;
            this.m_canUpdateValues = false;
            this.m_itemWalkingRotationXSlider.Value = this.m_itemWalkingRotationX;
            this.m_itemWalkingRotationYSlider.Value = this.m_itemWalkingRotationY;
            this.m_itemWalkingRotationZSlider.Value = this.m_itemWalkingRotationZ;
            this.m_itemWalkingPositionXSlider.Value = this.m_itemWalkingPositionX;
            this.m_itemWalkingPositionYSlider.Value = this.m_itemWalkingPositionY;
            this.m_itemWalkingPositionZSlider.Value = this.m_itemWalkingPositionZ;
            this.m_itemRotationXSlider.Value = this.m_itemRotationX;
            this.m_itemRotationYSlider.Value = this.m_itemRotationY;
            this.m_itemRotationZSlider.Value = this.m_itemRotationZ;
            this.m_itemPositionXSlider.Value = this.m_itemPositionX;
            this.m_itemPositionYSlider.Value = this.m_itemPositionY;
            this.m_itemPositionZSlider.Value = this.m_itemPositionZ;
            this.m_amplitudeMultiplierSlider.Value = base.CurrentSelectedItem.AmplitudeMultiplier3rd;
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
                base.CurrentSelectedItem.ItemWalkingLocation3rd = ((this.m_storedWalkingItem * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_itemWalkingRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_itemWalkingRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_itemWalkingRotationZ));
                base.CurrentSelectedItem.ItemWalkingLocation3rd.Translation = new Vector3(this.m_itemWalkingPositionX, this.m_itemWalkingPositionY, this.m_itemWalkingPositionZ);
                base.CurrentSelectedItem.AmplitudeMultiplier3rd = this.m_amplitudeMultiplierSlider.Value;
            }
        }
    }
}

