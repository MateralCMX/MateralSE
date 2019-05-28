namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenSpawnDefinedEntity : MyGuiScreenBase
    {
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlListbox m_containersListBox;
        private MyGuiControlCheckbox m_replicableEntityCheckBox;
        private Vector3 m_position;

        public MyGuiScreenSpawnDefinedEntity(Vector3 position) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_position = position;
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
            MyContainerDefinition definition1 = new MyContainerDefinition();
            foreach (MyGuiControlListbox.Item item in this.m_containersListBox.SelectedItems)
            {
                if (item.UserData is MyDefinitionId)
                {
                    Vector3? up = null;
                    up = null;
                    MyEntities.CreateEntityAndAdd((MyDefinitionId) item.UserData, false, true, new Vector3?(this.m_position), up, up);
                }
            }
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenSpawnDefinedEntity";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
        }

        public override void RecreateControls(bool contructor)
        {
            int? nullable3;
            base.RecreateControls(contructor);
            Vector2? size = null;
            Vector4? colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.46f), size, "Select entity to spawn", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            if (this.m_containersListBox == null)
            {
                this.m_containersListBox = new MyGuiControlListbox();
            }
            this.m_containersListBox.ClearItems();
            this.m_containersListBox.MultiSelect = false;
            this.m_containersListBox.Name = "Containers";
            List<MyDefinitionId> definedContainers = new List<MyDefinitionId>();
            MyDefinitionManager.Static.GetDefinedEntityContainers(ref definedContainers);
            foreach (MyDefinitionId id in definedContainers)
            {
                string str = id.ToString();
                if (str.StartsWith("MyObjectBuilder_"))
                {
                    str = str.Remove(0, "MyObectBuilder_".Length + 1);
                }
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(str), null, null, id, null);
                nullable3 = null;
                this.m_containersListBox.Add(item, nullable3);
            }
            this.m_containersListBox.VisibleRowsCount = definedContainers.Count + 1;
            this.m_containersListBox.Position = new Vector2(0f, 0f);
            this.m_containersListBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.m_containersListBox.ItemSize = new Vector2(0.36f, 0.036f);
            this.m_containersListBox.Size = new Vector2(0.4f, 0.6f);
            this.Controls.Add(this.m_containersListBox);
            colorMask = null;
            nullable3 = null;
            this.m_confirmButton = new MyGuiControlButton(new Vector2(0.21f, 0.35f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Confirm"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, nullable3, false);
            colorMask = null;
            nullable3 = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(-0.21f, 0.35f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Cancel"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, nullable3, false);
            this.Controls.Add(this.m_confirmButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
        }
    }
}

