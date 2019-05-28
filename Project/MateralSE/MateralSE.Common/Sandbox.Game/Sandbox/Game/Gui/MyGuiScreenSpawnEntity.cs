namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenSpawnEntity : MyGuiScreenBase
    {
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlListbox m_addComponentsListBox;
        private MyGuiControlCheckbox m_replicableEntityCheckBox;
        private Vector3 m_position;

        public MyGuiScreenSpawnEntity(Vector3 position) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
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
            MyContainerDefinition newDefinition = new MyContainerDefinition();
            foreach (MyGuiControlListbox.Item item in this.m_addComponentsListBox.SelectedItems)
            {
                if (item.UserData is MyDefinitionId)
                {
                    MyDefinitionId userData = (MyDefinitionId) item.UserData;
                    MyContainerDefinition.DefaultComponent component = new MyContainerDefinition.DefaultComponent {
                        BuilderType = userData.TypeId,
                        SubtypeId = new MyStringHash?(userData.SubtypeId)
                    };
                    newDefinition.DefaultComponents.Add(component);
                }
            }
            MyObjectBuilder_EntityBase objectBuilder = null;
            if (this.m_replicableEntityCheckBox.IsChecked)
            {
                objectBuilder = new MyObjectBuilder_ReplicableEntity();
                newDefinition.Id = new MyDefinitionId(typeof(MyObjectBuilder_ReplicableEntity), "DebugTest");
            }
            else
            {
                objectBuilder = new MyObjectBuilder_EntityBase();
                newDefinition.Id = new MyDefinitionId(typeof(MyObjectBuilder_EntityBase), "DebugTest");
            }
            MyDefinitionManager.Static.SetEntityContainerDefinition(newDefinition);
            objectBuilder.SubtypeName = newDefinition.Id.SubtypeName;
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(this.m_position, Vector3.Forward, Vector3.Up);
            MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, false);
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenSpawnEntity";

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
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.46f), size, "Select components to include in entity", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            size = null;
            colorMask = null;
            this.m_replicableEntityCheckBox = new MyGuiControlCheckbox(size, colorMask, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_replicableEntityCheckBox.Position = new Vector2(0f, -0.42f);
            this.m_replicableEntityCheckBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.Controls.Add(this.m_replicableEntityCheckBox);
            size = null;
            colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.39f), size, "MyEntityReplicable / MyEntity", colorMask, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            size = null;
            colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.32f), size, "Select components to add", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            if (this.m_addComponentsListBox == null)
            {
                this.m_addComponentsListBox = new MyGuiControlListbox();
            }
            this.m_addComponentsListBox.ClearItems();
            this.m_addComponentsListBox.MultiSelect = true;
            this.m_addComponentsListBox.Name = "AddComponents";
            List<MyDefinitionId> definedComponents = new List<MyDefinitionId>();
            MyDefinitionManager.Static.GetDefinedEntityComponents(ref definedComponents);
            foreach (MyDefinitionId id in definedComponents)
            {
                string str = id.ToString();
                if (str.StartsWith("MyObjectBuilder_"))
                {
                    str = str.Remove(0, "MyObectBuilder_".Length + 1);
                }
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(str), null, null, id, null);
                nullable3 = null;
                this.m_addComponentsListBox.Add(item, nullable3);
            }
            this.m_addComponentsListBox.VisibleRowsCount = definedComponents.Count + 1;
            this.m_addComponentsListBox.Position = new Vector2(0f, 0f);
            this.m_addComponentsListBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.m_addComponentsListBox.ItemSize = new Vector2(0.36f, 0.036f);
            this.m_addComponentsListBox.Size = new Vector2(0.4f, 0.6f);
            this.Controls.Add(this.m_addComponentsListBox);
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

