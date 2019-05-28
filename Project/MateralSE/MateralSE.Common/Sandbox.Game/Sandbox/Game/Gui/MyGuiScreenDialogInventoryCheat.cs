namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenDialogInventoryCheat : MyGuiScreenBase
    {
        private List<MyPhysicalItemDefinition> m_physicalItemDefinitions;
        private MyGuiControlTextbox m_amountTextbox;
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlCombobox m_items;
        private static double m_lastAmount;
        private static int m_lastSelectedItem;
        private static int addedAsteroidsCount;

        public MyGuiScreenDialogInventoryCheat() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_physicalItemDefinitions = new List<MyPhysicalItemDefinition>();
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void cancelButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void confirmButton_OnButtonClick(MyGuiControlButton sender)
        {
            MyEntity controlledEntity = MySession.Static.ControlledEntity as MyEntity;
            if ((controlledEntity != null) && controlledEntity.HasInventory)
            {
                double result = 0.0;
                double.TryParse(this.m_amountTextbox.Text, out result);
                m_lastAmount = result;
                MyFixedPoint b = (MyFixedPoint) result;
                if (this.m_items.GetSelectedKey() < 0L)
                {
                    return;
                }
                else if (((int) this.m_items.GetSelectedKey()) < this.m_physicalItemDefinitions.Count)
                {
                    MyDefinitionId contentId = this.m_physicalItemDefinitions[(int) this.m_items.GetSelectedKey()].Id;
                    m_lastSelectedItem = (int) this.m_items.GetSelectedKey();
                    MyInventory inventory = controlledEntity.GetInventory(0);
                    if (inventory != null)
                    {
                        if (!MySession.Static.CreativeMode)
                        {
                            b = MyFixedPoint.Min(inventory.ComputeAmountThatFits(contentId, 0f, 0f), b);
                        }
                        inventory.DebugAddItems(b, (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) contentId));
                    }
                }
                else
                {
                    return;
                }
            }
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDialogInventoryCheat";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsKeyPress(MyKeys.Enter))
            {
                this.confirmButton_OnButtonClick(this.m_confirmButton);
            }
            if (MyInput.Static.IsKeyPress(MyKeys.Escape))
            {
                this.cancelButton_OnButtonClick(this.m_cancelButton);
            }
        }

        public override void RecreateControls(bool contructor)
        {
            base.RecreateControls(contructor);
            Vector2? size = null;
            Vector4? colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.1f), size, "Select the amount and type of items to spawn in your inventory", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            colorMask = null;
            this.m_amountTextbox = new MyGuiControlTextbox(new Vector2(-0.2f, 0f), null, 9, colorMask, 0.8f, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default);
            colorMask = null;
            size = null;
            size = null;
            colorMask = null;
            this.m_items = new MyGuiControlCombobox(new Vector2(0.2f, 0f), new Vector2(0.3f, 0.05f), colorMask, size, 10, size, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, colorMask);
            colorMask = null;
            int? buttonIndex = null;
            this.m_confirmButton = new MyGuiControlButton(new Vector2(0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Confirm"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            colorMask = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(-0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Cancel"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            foreach (MyDefinitionBase base2 in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyPhysicalItemDefinition item = base2 as MyPhysicalItemDefinition;
                if ((item != null) && item.CanSpawnFromScreen)
                {
                    int count = this.m_physicalItemDefinitions.Count;
                    this.m_physicalItemDefinitions.Add(item);
                    buttonIndex = null;
                    this.m_items.AddItem((long) count, base2.DisplayNameText, buttonIndex, null);
                }
            }
            this.Controls.Add(this.m_amountTextbox);
            this.Controls.Add(this.m_items);
            this.Controls.Add(this.m_confirmButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_amountTextbox.Text = $"{m_lastAmount}";
            this.m_items.SelectItemByIndex(m_lastSelectedItem);
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
        }
    }
}

