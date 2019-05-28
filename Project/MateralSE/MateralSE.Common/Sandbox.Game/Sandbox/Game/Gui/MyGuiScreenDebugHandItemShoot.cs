namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("Game", "Hand item shoot")]
    internal class MyGuiScreenDebugHandItemShoot : MyGuiScreenDebugHandItemBase
    {
        private Matrix m_storedShootLocation;
        private Matrix m_storedShootLocation3rd;
        private bool m_canUpdateValues = true;
        private float m_itemRotationX;
        private float m_itemRotationY;
        private float m_itemRotationZ;
        private float m_itemPositionX;
        private float m_itemPositionY;
        private float m_itemPositionZ;
        private MyGuiControlSlider m_itemRotationXSlider;
        private MyGuiControlSlider m_itemRotationYSlider;
        private MyGuiControlSlider m_itemRotationZSlider;
        private MyGuiControlSlider m_itemPositionXSlider;
        private MyGuiControlSlider m_itemPositionYSlider;
        private MyGuiControlSlider m_itemPositionZSlider;
        private float m_itemRotationX3rd;
        private float m_itemRotationY3rd;
        private float m_itemRotationZ3rd;
        private float m_itemPositionX3rd;
        private float m_itemPositionY3rd;
        private float m_itemPositionZ3rd;
        private MyGuiControlSlider m_itemRotationX3rdSlider;
        private MyGuiControlSlider m_itemRotationY3rdSlider;
        private MyGuiControlSlider m_itemRotationZ3rdSlider;
        private MyGuiControlSlider m_itemPositionX3rdSlider;
        private MyGuiControlSlider m_itemPositionY3rdSlider;
        private MyGuiControlSlider m_itemPositionZ3rdSlider;
        private MyGuiControlSlider m_itemMuzzlePositionXSlider;
        private MyGuiControlSlider m_itemMuzzlePositionYSlider;
        private MyGuiControlSlider m_itemMuzzlePositionZSlider;
        private MyGuiControlSlider m_blendSlider;
        private MyGuiControlSlider m_shootScatterXSlider;
        private MyGuiControlSlider m_shootScatterYSlider;
        private MyGuiControlSlider m_shootScatterZSlider;
        private MyGuiControlSlider m_scatterSpeedSlider;

        public MyGuiScreenDebugHandItemShoot()
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugHandItemsAnimations3rd";

        protected override void handItemsCombo_ItemSelected()
        {
            base.handItemsCombo_ItemSelected();
            this.m_storedShootLocation = base.CurrentSelectedItem.ItemShootLocation;
            this.m_storedShootLocation3rd = base.CurrentSelectedItem.ItemShootLocation3rd;
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
                base.CurrentSelectedItem.ItemShootLocation = ((this.m_storedShootLocation * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_itemRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_itemRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_itemRotationZ));
                base.CurrentSelectedItem.ItemShootLocation.Translation = new Vector3(this.m_itemPositionX, this.m_itemPositionY, this.m_itemPositionZ);
                this.m_itemRotationX3rd = this.m_itemRotationX3rdSlider.Value;
                this.m_itemRotationY3rd = this.m_itemRotationY3rdSlider.Value;
                this.m_itemRotationZ3rd = this.m_itemRotationZ3rdSlider.Value;
                this.m_itemPositionX3rd = this.m_itemPositionX3rdSlider.Value;
                this.m_itemPositionY3rd = this.m_itemPositionY3rdSlider.Value;
                this.m_itemPositionZ3rd = this.m_itemPositionZ3rdSlider.Value;
                base.CurrentSelectedItem.ItemShootLocation3rd = ((this.m_storedShootLocation3rd * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_itemRotationX3rd))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_itemRotationY3rd))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_itemRotationZ3rd));
                base.CurrentSelectedItem.ItemShootLocation3rd.Translation = new Vector3(this.m_itemPositionX3rd, this.m_itemPositionY3rd, this.m_itemPositionZ3rd);
                base.CurrentSelectedItem.ShootBlend = this.m_blendSlider.Value;
                base.CurrentSelectedItem.MuzzlePosition.X = this.m_itemMuzzlePositionXSlider.Value;
                base.CurrentSelectedItem.MuzzlePosition.Y = this.m_itemMuzzlePositionYSlider.Value;
                base.CurrentSelectedItem.MuzzlePosition.Z = this.m_itemMuzzlePositionZSlider.Value;
                base.CurrentSelectedItem.ShootScatter.X = this.m_shootScatterXSlider.Value;
                base.CurrentSelectedItem.ShootScatter.Y = this.m_shootScatterYSlider.Value;
                base.CurrentSelectedItem.ShootScatter.Z = this.m_shootScatterZSlider.Value;
                base.CurrentSelectedItem.ScatterSpeed = this.m_scatterSpeedSlider.Value;
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
            base.AddCaption("Hand item shoot", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
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
            this.m_itemRotationX3rdSlider = base.AddSlider("item rotation X 3rd", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationX3rdSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemRotationY3rdSlider = base.AddSlider("item rotation Y 3rd", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationY3rdSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemRotationZ3rdSlider = base.AddSlider("item rotation Z 3rd", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationZ3rdSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionX3rdSlider = base.AddSlider("item position X 3rd", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionX3rdSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionY3rdSlider = base.AddSlider("item position Y 3rd", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionY3rdSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionZ3rdSlider = base.AddSlider("item position Z 3rd", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionZ3rdSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemMuzzlePositionXSlider = base.AddSlider("item muzzle X", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemMuzzlePositionXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemMuzzlePositionYSlider = base.AddSlider("item muzzle Y", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemMuzzlePositionYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemMuzzlePositionZSlider = base.AddSlider("item muzzle Z", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemMuzzlePositionZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_blendSlider = base.AddSlider("Shoot blend", (float) 0f, (float) 0f, (float) 3f, color);
            this.m_blendSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_shootScatterXSlider = base.AddSlider("Scatter X", (float) 0f, (float) 0f, (float) 1f, color);
            this.m_shootScatterXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_shootScatterYSlider = base.AddSlider("Scatter Y", (float) 0f, (float) 0f, (float) 1f, color);
            this.m_shootScatterYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_shootScatterZSlider = base.AddSlider("Scatter Z", (float) 0f, (float) 0f, (float) 1f, color);
            this.m_shootScatterZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_scatterSpeedSlider = base.AddSlider("Scatter speed", (float) 0f, (float) 0f, (float) 1f, color);
            this.m_scatterSpeedSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
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
            this.m_itemRotationX = 0f;
            this.m_itemRotationY = 0f;
            this.m_itemRotationZ = 0f;
            this.m_itemPositionX = this.m_storedShootLocation.Translation.X;
            this.m_itemPositionY = this.m_storedShootLocation.Translation.Y;
            this.m_itemPositionZ = this.m_storedShootLocation.Translation.Z;
            this.m_itemRotationX3rd = 0f;
            this.m_itemRotationY3rd = 0f;
            this.m_itemRotationZ3rd = 0f;
            this.m_itemPositionX3rd = this.m_storedShootLocation3rd.Translation.X;
            this.m_itemPositionY3rd = this.m_storedShootLocation3rd.Translation.Y;
            this.m_itemPositionZ3rd = this.m_storedShootLocation3rd.Translation.Z;
            this.m_canUpdateValues = false;
            this.m_itemRotationXSlider.Value = this.m_itemRotationX;
            this.m_itemRotationYSlider.Value = this.m_itemRotationY;
            this.m_itemRotationZSlider.Value = this.m_itemRotationZ;
            this.m_itemPositionXSlider.Value = this.m_itemPositionX;
            this.m_itemPositionYSlider.Value = this.m_itemPositionY;
            this.m_itemPositionZSlider.Value = this.m_itemPositionZ;
            this.m_itemRotationX3rdSlider.Value = this.m_itemRotationX3rd;
            this.m_itemRotationY3rdSlider.Value = this.m_itemRotationY3rd;
            this.m_itemRotationZ3rdSlider.Value = this.m_itemRotationZ3rd;
            this.m_itemPositionX3rdSlider.Value = this.m_itemPositionX3rd;
            this.m_itemPositionY3rdSlider.Value = this.m_itemPositionY3rd;
            this.m_itemPositionZ3rdSlider.Value = this.m_itemPositionZ3rd;
            this.m_itemMuzzlePositionXSlider.Value = base.CurrentSelectedItem.MuzzlePosition.X;
            this.m_itemMuzzlePositionYSlider.Value = base.CurrentSelectedItem.MuzzlePosition.Y;
            this.m_itemMuzzlePositionZSlider.Value = base.CurrentSelectedItem.MuzzlePosition.Z;
            this.m_shootScatterXSlider.Value = base.CurrentSelectedItem.ShootScatter.X;
            this.m_shootScatterYSlider.Value = base.CurrentSelectedItem.ShootScatter.Y;
            this.m_shootScatterZSlider.Value = base.CurrentSelectedItem.ShootScatter.Z;
            this.m_scatterSpeedSlider.Value = base.CurrentSelectedItem.ScatterSpeed;
            this.m_blendSlider.Value = base.CurrentSelectedItem.ShootBlend;
            this.m_canUpdateValues = true;
        }
    }
}

