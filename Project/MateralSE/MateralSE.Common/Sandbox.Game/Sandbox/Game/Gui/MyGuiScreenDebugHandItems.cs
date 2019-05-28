namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using VRageMath;

    [MyDebugScreen("Game", "Hand items")]
    internal class MyGuiScreenDebugHandItems : MyGuiScreenDebugHandItemBase
    {
        private Matrix m_storedLeftHand;
        private Matrix m_storedRightHand;
        private Matrix m_storedItem;
        private bool m_canUpdateValues = true;
        private float m_leftHandRotationX;
        private float m_leftHandRotationY;
        private float m_leftHandRotationZ;
        private float m_leftHandPositionX;
        private float m_leftHandPositionY;
        private float m_leftHandPositionZ;
        private float m_rightHandRotationX;
        private float m_rightHandRotationY;
        private float m_rightHandRotationZ;
        private float m_rightHandPositionX;
        private float m_rightHandPositionY;
        private float m_rightHandPositionZ;
        private float m_itemRotationX;
        private float m_itemRotationY;
        private float m_itemRotationZ;
        private float m_itemPositionX;
        private float m_itemPositionY;
        private float m_itemPositionZ;
        private MyGuiControlSlider m_leftHandRotationXSlider;
        private MyGuiControlSlider m_leftHandRotationYSlider;
        private MyGuiControlSlider m_leftHandRotationZSlider;
        private MyGuiControlSlider m_leftHandPositionXSlider;
        private MyGuiControlSlider m_leftHandPositionYSlider;
        private MyGuiControlSlider m_leftHandPositionZSlider;
        private MyGuiControlSlider m_rightHandRotationXSlider;
        private MyGuiControlSlider m_rightHandRotationYSlider;
        private MyGuiControlSlider m_rightHandRotationZSlider;
        private MyGuiControlSlider m_rightHandPositionXSlider;
        private MyGuiControlSlider m_rightHandPositionYSlider;
        private MyGuiControlSlider m_rightHandPositionZSlider;
        private MyGuiControlSlider m_itemRotationXSlider;
        private MyGuiControlSlider m_itemRotationYSlider;
        private MyGuiControlSlider m_itemRotationZSlider;
        private MyGuiControlSlider m_itemPositionXSlider;
        private MyGuiControlSlider m_itemPositionYSlider;
        private MyGuiControlSlider m_itemPositionZSlider;

        public MyGuiScreenDebugHandItems()
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugHandItems";

        protected override void handItemsCombo_ItemSelected()
        {
            base.handItemsCombo_ItemSelected();
            this.m_storedLeftHand = base.CurrentSelectedItem.LeftHand;
            this.m_storedRightHand = base.CurrentSelectedItem.RightHand;
            this.m_storedItem = base.CurrentSelectedItem.ItemLocation;
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
                base.CurrentSelectedItem.ItemLocation = ((this.m_storedItem * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_itemRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_itemRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_itemRotationZ));
                base.CurrentSelectedItem.ItemLocation.Translation = new Vector3(this.m_itemPositionX, this.m_itemPositionY, this.m_itemPositionZ);
            }
        }

        private void LeftHandChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_leftHandRotationX = this.m_leftHandRotationXSlider.Value;
                this.m_leftHandRotationY = this.m_leftHandRotationYSlider.Value;
                this.m_leftHandRotationZ = this.m_leftHandRotationZSlider.Value;
                this.m_leftHandPositionX = this.m_leftHandPositionXSlider.Value;
                this.m_leftHandPositionY = this.m_leftHandPositionYSlider.Value;
                this.m_leftHandPositionZ = this.m_leftHandPositionZSlider.Value;
                base.CurrentSelectedItem.LeftHand = ((this.m_storedLeftHand * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_leftHandRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_leftHandRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_leftHandRotationZ));
                base.CurrentSelectedItem.LeftHand.Translation = new Vector3(this.m_leftHandPositionX, this.m_leftHandPositionY, this.m_leftHandPositionZ);
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Hand items properties", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.RecreateHandItemsCombo();
            base.m_sliderDebugScale = 0.6f;
            Vector4? color = null;
            this.m_leftHandRotationXSlider = base.AddSlider("Left hand rotation X", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_leftHandRotationXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.LeftHandChanged);
            color = null;
            this.m_leftHandRotationYSlider = base.AddSlider("Left hand rotation Y", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_leftHandRotationYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.LeftHandChanged);
            color = null;
            this.m_leftHandRotationZSlider = base.AddSlider("Left hand rotation Z", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_leftHandRotationZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.LeftHandChanged);
            color = null;
            this.m_leftHandPositionXSlider = base.AddSlider("Left hand position X", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_leftHandPositionXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.LeftHandChanged);
            color = null;
            this.m_leftHandPositionYSlider = base.AddSlider("Left hand position Y", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_leftHandPositionYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.LeftHandChanged);
            color = null;
            this.m_leftHandPositionZSlider = base.AddSlider("Left hand position Z", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_leftHandPositionZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.LeftHandChanged);
            color = null;
            this.m_rightHandRotationXSlider = base.AddSlider("Right hand rotation X", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_rightHandRotationXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.RightHandChanged);
            color = null;
            this.m_rightHandRotationYSlider = base.AddSlider("Right hand rotation Y", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_rightHandRotationYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.RightHandChanged);
            color = null;
            this.m_rightHandRotationZSlider = base.AddSlider("Right hand rotation Z", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_rightHandRotationZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.RightHandChanged);
            color = null;
            this.m_rightHandPositionXSlider = base.AddSlider("Right hand position X", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_rightHandPositionXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.RightHandChanged);
            color = null;
            this.m_rightHandPositionYSlider = base.AddSlider("Right hand position Y", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_rightHandPositionYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.RightHandChanged);
            color = null;
            this.m_rightHandPositionZSlider = base.AddSlider("Right hand position Z", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_rightHandPositionZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.RightHandChanged);
            color = null;
            this.m_itemRotationXSlider = base.AddSlider("Item rotation X", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemRotationYSlider = base.AddSlider("Item rotation Y", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemRotationZSlider = base.AddSlider("Item rotation Z", (float) 0f, (float) 0f, (float) 360f, color);
            this.m_itemRotationZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionXSlider = base.AddSlider("Item position X", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionXSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionYSlider = base.AddSlider("Item position Y", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionYSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            color = null;
            this.m_itemPositionZSlider = base.AddSlider("Item position Z", (float) 0f, (float) -1f, (float) 1f, color);
            this.m_itemPositionZSlider.ValueChanged = new Action<MyGuiControlSlider>(this.ItemChanged);
            base.RecreateSaveAndReloadButtons();
            base.SelectFirstHandItem();
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }

        private void RightHandChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_rightHandRotationX = this.m_rightHandRotationXSlider.Value;
                this.m_rightHandRotationY = this.m_rightHandRotationYSlider.Value;
                this.m_rightHandRotationZ = this.m_rightHandRotationZSlider.Value;
                this.m_rightHandPositionX = this.m_rightHandPositionXSlider.Value;
                this.m_rightHandPositionY = this.m_rightHandPositionYSlider.Value;
                this.m_rightHandPositionZ = this.m_rightHandPositionZSlider.Value;
                base.CurrentSelectedItem.RightHand = ((this.m_storedRightHand * Matrix.CreateRotationX(MathHelper.ToRadians(this.m_rightHandRotationX))) * Matrix.CreateRotationY(MathHelper.ToRadians(this.m_rightHandRotationY))) * Matrix.CreateRotationZ(MathHelper.ToRadians(this.m_rightHandRotationZ));
                base.CurrentSelectedItem.RightHand.Translation = new Vector3(this.m_rightHandPositionX, this.m_rightHandPositionY, this.m_rightHandPositionZ);
            }
        }

        private void UpdateValues()
        {
            this.m_leftHandRotationX = 0f;
            this.m_leftHandRotationY = 0f;
            this.m_leftHandRotationZ = 0f;
            this.m_leftHandPositionX = this.m_storedLeftHand.Translation.X;
            this.m_leftHandPositionY = this.m_storedLeftHand.Translation.Y;
            this.m_leftHandPositionZ = this.m_storedLeftHand.Translation.Z;
            this.m_rightHandRotationX = 0f;
            this.m_rightHandRotationY = 0f;
            this.m_rightHandRotationZ = 0f;
            this.m_rightHandPositionX = this.m_storedRightHand.Translation.X;
            this.m_rightHandPositionY = this.m_storedRightHand.Translation.Y;
            this.m_rightHandPositionZ = this.m_storedRightHand.Translation.Z;
            this.m_itemRotationX = 0f;
            this.m_itemRotationY = 0f;
            this.m_itemRotationZ = 0f;
            this.m_itemPositionX = this.m_storedItem.Translation.X;
            this.m_itemPositionY = this.m_storedItem.Translation.Y;
            this.m_itemPositionZ = this.m_storedItem.Translation.Z;
            this.m_canUpdateValues = false;
            this.m_leftHandRotationXSlider.Value = this.m_leftHandRotationX;
            this.m_leftHandRotationYSlider.Value = this.m_leftHandRotationY;
            this.m_leftHandRotationZSlider.Value = this.m_leftHandRotationZ;
            this.m_leftHandPositionXSlider.Value = this.m_leftHandPositionX;
            this.m_leftHandPositionYSlider.Value = this.m_leftHandPositionY;
            this.m_leftHandPositionZSlider.Value = this.m_leftHandPositionZ;
            this.m_rightHandRotationXSlider.Value = this.m_rightHandRotationX;
            this.m_rightHandRotationYSlider.Value = this.m_rightHandRotationY;
            this.m_rightHandRotationZSlider.Value = this.m_rightHandRotationZ;
            this.m_rightHandPositionXSlider.Value = this.m_rightHandPositionX;
            this.m_rightHandPositionYSlider.Value = this.m_rightHandPositionY;
            this.m_rightHandPositionZSlider.Value = this.m_rightHandPositionZ;
            this.m_itemRotationXSlider.Value = this.m_itemRotationX;
            this.m_itemRotationYSlider.Value = this.m_itemRotationY;
            this.m_itemRotationZSlider.Value = this.m_itemRotationZ;
            this.m_itemPositionXSlider.Value = this.m_itemPositionX;
            this.m_itemPositionYSlider.Value = this.m_itemPositionY;
            this.m_itemPositionZSlider.Value = this.m_itemPositionZ;
            this.m_canUpdateValues = true;
        }
    }
}

