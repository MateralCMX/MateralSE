namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenConfigComponents : MyGuiScreenBase
    {
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private long m_entityId;
        private List<MyEntity> m_entities;
        private MyGuiControlCombobox m_entitiesSelection;
        private MyGuiControlListbox m_removeComponentsListBox;
        private MyGuiControlListbox m_addComponentsListBox;

        public MyGuiScreenConfigComponents(List<MyEntity> entities) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_entities = entities;
            this.m_entityId = entities.FirstOrDefault<MyEntity>().EntityId;
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
            foreach (MyGuiControlListbox.Item item in this.m_removeComponentsListBox.SelectedItems)
            {
                MyComponentContainerExtension.TryRemoveComponent(this.m_entityId, item.UserData as Type);
            }
            foreach (MyGuiControlListbox.Item item2 in this.m_addComponentsListBox.SelectedItems)
            {
                if (item2.UserData is MyDefinitionId)
                {
                    MyComponentContainerExtension.TryAddComponent(this.m_entityId, (MyDefinitionId) item2.UserData);
                }
            }
            this.CloseScreen();
        }

        private void EntitySelected()
        {
            this.m_entityId = this.m_entitiesSelection.GetSelectedKey();
            this.RecreateControls(false);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenConfigComponents";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
        }

        public override void RecreateControls(bool contructor)
        {
            MyEntity entity;
            List<Type> list;
            int? nullable3;
            base.RecreateControls(contructor);
            Vector2? size = null;
            Vector4? colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.46f), size, "Select components to remove and components to add", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            if (this.m_entitiesSelection == null)
            {
                this.m_entitiesSelection = new MyGuiControlCombobox();
                this.m_entitiesSelection.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.EntitySelected);
            }
            this.m_entitiesSelection.Position = new Vector2(0f, -0.42f);
            this.m_entitiesSelection.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.m_entitiesSelection.ClearItems();
            foreach (MyEntity entity2 in this.m_entities)
            {
                nullable3 = null;
                this.m_entitiesSelection.AddItem(entity2.EntityId, entity2.ToString(), nullable3, null);
            }
            this.m_entitiesSelection.SelectItemByKey(this.m_entityId, false);
            this.Controls.Add(this.m_entitiesSelection);
            size = null;
            colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.39f), size, $"EntityID = {this.m_entityId}", colorMask, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            if (MyEntities.TryGetEntityById(this.m_entityId, out entity, false))
            {
                size = null;
                colorMask = null;
                this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.36f), size, string.Format("Name: {1}, Type: {0}", entity.GetType().Name, entity.DisplayNameText), colorMask, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            }
            size = null;
            colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.21f, -0.32f), size, "Select components to remove", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            if (this.m_removeComponentsListBox == null)
            {
                this.m_removeComponentsListBox = new MyGuiControlListbox();
            }
            this.m_removeComponentsListBox.ClearItems();
            this.m_removeComponentsListBox.MultiSelect = true;
            this.m_removeComponentsListBox.Name = "RemoveComponents";
            if (MyComponentContainerExtension.TryGetEntityComponentTypes(this.m_entityId, out list))
            {
                foreach (Type type in list)
                {
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(type.Name), null, null, type, null);
                    nullable3 = null;
                    this.m_removeComponentsListBox.Add(item, nullable3);
                }
                this.m_removeComponentsListBox.VisibleRowsCount = list.Count + 1;
            }
            this.m_removeComponentsListBox.Position = new Vector2(-0.21f, 0f);
            this.m_removeComponentsListBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.m_removeComponentsListBox.ItemSize = new Vector2(0.38f, 0.036f);
            this.m_removeComponentsListBox.Size = new Vector2(0.4f, 0.6f);
            this.Controls.Add(this.m_removeComponentsListBox);
            size = null;
            colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0.21f, -0.32f), size, "Select components to add", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            if (this.m_addComponentsListBox == null)
            {
                this.m_addComponentsListBox = new MyGuiControlListbox();
            }
            this.m_addComponentsListBox.ClearItems();
            this.m_addComponentsListBox.MultiSelect = true;
            this.m_addComponentsListBox.Name = "AddComponents";
            list.Clear();
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
            this.m_addComponentsListBox.Position = new Vector2(0.21f, 0f);
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

