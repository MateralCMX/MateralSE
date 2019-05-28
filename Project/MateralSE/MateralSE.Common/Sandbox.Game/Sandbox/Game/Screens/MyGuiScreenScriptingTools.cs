namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI.DebugInputComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Components.Session;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.SessionComponents;
    using VRage.Game.VisualScripting;
    using VRage.Game.VisualScripting.Missions;
    using VRage.Generics;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenScriptingTools : MyGuiScreenDebugBase
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private static readonly float ITEM_HORIZONTAL_PADDING = 0.01f;
        private static readonly float ITEM_VERTICAL_PADDING = 0.005f;
        private static readonly Vector2 BUTTON_SIZE = new Vector2(0.06f, 0.03f);
        private static readonly Vector2 ITEM_SIZE = new Vector2(0.06f, 0.02f);
        private static readonly string ENTITY_NAME_PREFIX = "Waypoint_";
        private static int InitialShift = 0;
        private static uint m_entityCounter = 0;
        private static ScriptingToolsScreen m_currentScreen = ScriptingToolsScreen.Transformation;
        private IMyCameraController m_previousCameraController;
        private MyGuiControlButton m_setTriggerSizeButton;
        private MyGuiControlButton m_enlargeTriggerButton;
        private MyGuiControlButton m_shrinkTriggerButton;
        private MyGuiControlListbox m_triggersListBox;
        private MyGuiControlListbox m_waypointsListBox;
        private MyGuiControlListbox m_smListBox;
        private MyGuiControlListbox m_levelScriptListBox;
        private MyGuiControlTextbox m_selectedTriggerNameBox;
        private MyGuiControlTextbox m_selectedEntityNameBox;
        private MyGuiControlTextbox m_selectedFunctionalBlockNameBox;
        private VRage.Game.Entity.MyEntity m_selectedFunctionalBlock;
        private bool m_disablePicking;
        private readonly MyTriggerManipulator m_triggerManipulator;
        private readonly MyEntityTransformationSystem m_transformSys;
        private readonly MyVisualScriptManagerSessionComponent m_scriptManager;
        private readonly MySessionComponentScriptSharedStorage m_scriptStorage;
        private readonly StringBuilder m_helperStringBuilder;
        public List<VRage.Game.Entity.MyEntity> m_waypoints;
        private Dictionary<string, Cutscene> m_cutscenes;
        private Cutscene m_cutsceneCurrent;
        private int m_selectedCutsceneNodeIndex;
        private bool m_cutscenePlaying;
        private MyGuiControlCombobox m_cutsceneSelection;
        private MyGuiControlButton m_cutsceneDeleteButton;
        private MyGuiControlButton m_cutscenePlayButton;
        private MyGuiControlButton m_cutsceneRevertButton;
        private MyGuiControlButton m_cutsceneSaveButton;
        private MyGuiControlTextbox m_cutscenePropertyStartEntity;
        private MyGuiControlTextbox m_cutscenePropertyStartLookAt;
        private MyGuiControlCombobox m_cutscenePropertyNextCutscene;
        private MyGuiControlTextbox m_cutscenePropertyStartingFOV;
        private MyGuiControlCheckbox m_cutscenePropertyCanBeSkipped;
        private MyGuiControlCheckbox m_cutscenePropertyFireEventsDuringSkip;
        private MyGuiControlListbox m_cutsceneNodes;
        private MyGuiControlButton m_cutsceneNodeButtonAdd;
        private MyGuiControlButton m_cutsceneNodeButtonMoveUp;
        private MyGuiControlButton m_cutsceneNodeButtonMoveDown;
        private MyGuiControlButton m_cutsceneNodeButtonDelete;
        private MyGuiControlButton m_cutsceneNodeButtonDeleteAll;
        private MyGuiControlTextbox m_cutsceneNodePropertyTime;
        private MyGuiControlTextbox m_cutsceneNodePropertyMoveTo;
        private MyGuiControlTextbox m_cutsceneNodePropertyMoveToInstant;
        private MyGuiControlTextbox m_cutsceneNodePropertyRotateLike;
        private MyGuiControlTextbox m_cutsceneNodePropertyRotateLikeInstant;
        private MyGuiControlTextbox m_cutsceneNodePropertyRotateTowards;
        private MyGuiControlTextbox m_cutsceneNodePropertyRotateTowardsInstant;
        private MyGuiControlTextbox m_cutsceneNodePropertyRotateTowardsLock;
        private MyGuiControlTextbox m_cutsceneNodePropertyAttachAll;
        private MyGuiControlTextbox m_cutsceneNodePropertyAttachPosition;
        private MyGuiControlTextbox m_cutsceneNodePropertyAttachRotation;
        private MyGuiControlTextbox m_cutsceneNodePropertyEvent;
        private MyGuiControlTextbox m_cutsceneNodePropertyEventDelay;
        private MyGuiControlTextbox m_cutsceneNodePropertyFOVChange;
        private MyGuiControlTextbox m_cutsceneNodePropertyWaypoints;

        public MyGuiScreenScriptingTools() : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new Vector2?(SCREEN_SIZE), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), false)
        {
            this.m_helperStringBuilder = new StringBuilder();
            this.m_waypoints = new List<VRage.Game.Entity.MyEntity>();
            this.m_selectedCutsceneNodeIndex = -1;
            base.CanBeHidden = true;
            base.CanHideOthers = false;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_canShareInput = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            this.m_triggerManipulator = new MyTriggerManipulator(trigger => trigger is MyAreaTriggerComponent);
            this.m_transformSys = MySession.Static.GetComponent<MyEntityTransformationSystem>();
            this.m_transformSys.ControlledEntityChanged += new Action<VRage.Game.Entity.MyEntity, VRage.Game.Entity.MyEntity>(this.TransformSysOnControlledEntityChanged);
            this.m_transformSys.RayCasted += new Action<LineD>(this.TransformSysOnRayCasted);
            this.m_scriptManager = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            this.m_scriptStorage = MySession.Static.GetComponent<MySessionComponentScriptSharedStorage>();
            Vector3D? position = null;
            MySession.Static.SetCameraController(MyCameraControllerEnum.SpectatorFreeMouse, null, position);
            MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
            MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER = true;
            this.RecreateControls(true);
            this.InitializeWaypointList();
            if (m_currentScreen == ScriptingToolsScreen.Transformation)
            {
                this.UpdateWaypointList();
            }
        }

        private void AttachTriggerOnClick(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_transformSys.ControlledEntity != null)
            {
                VRage.Game.Entity.MyEntity selectedEntity = this.m_transformSys.ControlledEntity;
                MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption(MyTexts.Get(MySpaceTexts.EntitySpawnOn).ToString() + ": " + this.m_transformSys.ControlledEntity.DisplayName, "", delegate (string text) {
                    MyAreaTriggerComponent trigger = new MyAreaTriggerComponent(text);
                    this.m_triggerManipulator.SelectedTrigger = trigger;
                    if (!selectedEntity.Components.Contains(typeof(MyTriggerAggregate)))
                    {
                        selectedEntity.Components.Add(typeof(MyTriggerAggregate), new MyTriggerAggregate());
                    }
                    selectedEntity.Components.Get<MyTriggerAggregate>().AddComponent(this.m_triggerManipulator.SelectedTrigger);
                    trigger.Center = MyAPIGateway.Session.Camera.Position;
                    trigger.Radius = 2.0;
                    trigger.CustomDebugColor = new Color?(Color.Yellow);
                    this.DeselectEntity();
                    this.UpdateTriggerList();
                    this.m_triggersListBox.SelectedItems.Clear();
                    MyGuiControlListbox.Item item = this.CreateTriggerListItem(trigger);
                    int? position = null;
                    this.m_triggersListBox.Add(item, position);
                    this.m_triggersListBox.SelectedItem = item;
                    return true;
                }));
            }
        }

        private void ClearAllCutscenesClicked(MyGuiControlButton myGuiControlButton)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_DeleteAll_Caption);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.Cutscene_DeleteAll_Text), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    this.m_cutscenes.Clear();
                    this.m_cutsceneSelection.ClearItems();
                    this.UpdateCutsceneFields();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public override bool CloseScreen()
        {
            MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.UserControlled;
            Vector3D? position = null;
            MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.ControlledEntity.Entity, position);
            MyDebugDrawSettings.ENABLE_DEBUG_DRAW = false;
            MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER = false;
            this.m_transformSys.Active = false;
            MyGuiScreenGamePlay.DisableInput = MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning;
            return base.CloseScreen();
        }

        private void CloseScreenWithSave()
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_Unsaved_Caption);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.Cutscene_Unsaved_Text), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    this.SaveCutsceneClicked(this.m_cutsceneSaveButton);
                }
                if ((result == MyGuiScreenMessageBox.ResultEnum.YES) || (result == MyGuiScreenMessageBox.ResultEnum.NO))
                {
                    this.CloseScreen();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private MyGuiControlButton CreateButton(string text, Action<MyGuiControlButton> onClick, string tooltip = null)
        {
            Vector2? size = null;
            int? buttonIndex = null;
            MyGuiControlButton control = new MyGuiControlButton(new Vector2(base.m_buttonXOffset, base.m_currentPosition.Y), MyGuiControlButtonStyleEnum.Rectangular, size, new VRageMath.Vector4?(Color.Yellow.ToVector4()), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder(text), (0.8f * MyGuiConstants.DEBUG_BUTTON_TEXT_SCALE) * base.m_scale, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            if (!string.IsNullOrEmpty(tooltip))
            {
                control.SetTooltip(tooltip);
            }
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            control.Size = BUTTON_SIZE;
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlCheckbox CreateCheckbox(Action<MyGuiControlCheckbox> onCheckedChanged, bool isChecked, string tooltip = null)
        {
            Vector2? position = null;
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox control = new MyGuiControlCheckbox(position, color, null, isChecked, MyGuiControlCheckboxStyleEnum.Debug, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            if (!string.IsNullOrEmpty(tooltip))
            {
                control.SetTooltip(tooltip);
            }
            control.Size = ITEM_SIZE;
            control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, onCheckedChanged);
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlCombobox CreateComboBox()
        {
            MyGuiControlCombobox combobox1 = new MyGuiControlCombobox();
            combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            combobox1.Size = BUTTON_SIZE;
            MyGuiControlCombobox control = combobox1;
            control.Enabled = true;
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlLabel CreateLabel(string text)
        {
            Vector2? position = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(position, new Vector2?(ITEM_SIZE), text, colorMask, 0.8f, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlListbox CreateListBox()
        {
            Vector2? position = null;
            MyGuiControlListbox listbox1 = new MyGuiControlListbox(position, MyGuiControlListboxStyleEnum.Blueprints);
            listbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            listbox1.Size = new Vector2(1f, 0.15f);
            MyGuiControlListbox control = listbox1;
            control.MultiSelect = false;
            control.Enabled = true;
            control.ItemSize = new Vector2(SCREEN_SIZE.X, ITEM_SIZE.Y);
            control.TextScale = 0.6f;
            control.VisibleRowsCount = 7;
            this.Controls.Add(control);
            return control;
        }

        private void CreateNewCutsceneClicked(MyGuiControlButton myGuiControlButton)
        {
            if (!this.m_cutsceneSaveButton.Enabled)
            {
                this.NewCutscene();
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_Unsaved_Caption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.Cutscene_Unsaved_Text), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.SaveCutsceneClicked(this.m_cutsceneSaveButton);
                    }
                    if ((result == MyGuiScreenMessageBox.ResultEnum.YES) || (result == MyGuiScreenMessageBox.ResultEnum.NO))
                    {
                        this.NewCutscene();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private MyGuiControlTextbox CreateTextbox(string text, Action<MyGuiControlTextbox> textChanged = null)
        {
            Vector2? position = null;
            VRageMath.Vector4? textColor = null;
            MyGuiControlTextbox control = new MyGuiControlTextbox(position, text, 0x200, textColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Debug) {
                Enabled = false,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Size = ITEM_SIZE
            };
            control.TextChanged += textChanged;
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlListbox.Item CreateTriggerListItem(MyTriggerComponent trigger)
        {
            MyAreaTriggerComponent userData = trigger as MyAreaTriggerComponent;
            if (userData == null)
            {
                return null;
            }
            StringBuilder text = new StringBuilder("Trigger: ");
            text.Append(userData.Name).Append(" Entity: ");
            text.Append(string.IsNullOrEmpty(userData.Entity.Name) ? userData.Entity.DisplayName : userData.Entity.Name);
            return new MyGuiControlListbox.Item(text, userData.Name, null, userData, null);
        }

        private void CutsceneChanged()
        {
            this.m_cutsceneSaveButton.Enabled = this.m_cutsceneCurrent != null;
            this.m_cutsceneRevertButton.Enabled = this.m_cutsceneCurrent != null;
            if (this.m_selectedCutsceneNodeIndex >= 0)
            {
                this.m_cutsceneNodes.ItemsSelected -= new Action<MyGuiControlListbox>(this.m_cutsceneNodes_ItemsSelected);
                this.m_cutsceneNodes.Items.RemoveAt(this.m_selectedCutsceneNodeIndex);
                this.m_cutsceneNodes.Items.Insert(this.m_selectedCutsceneNodeIndex, new MyGuiControlListbox.Item(new StringBuilder((this.m_selectedCutsceneNodeIndex + 1).ToString() + ": " + this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].GetNodeSummary()), this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].GetNodeDescription(), null, null, null));
                this.SelectListboxItemAtIndex(this.m_cutsceneNodes, this.m_selectedCutsceneNodeIndex);
                this.m_cutsceneNodes.ItemsSelected += new Action<MyGuiControlListbox>(this.m_cutsceneNodes_ItemsSelected);
            }
        }

        private void CutsceneNodeButtonAddClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_cutsceneCurrent != null)
            {
                if (this.m_cutsceneCurrent.SequenceNodes == null)
                {
                    this.m_cutsceneCurrent.SequenceNodes = new List<CutsceneSequenceNode>();
                }
                CutsceneSequenceNode item = new CutsceneSequenceNode();
                this.m_cutsceneCurrent.SequenceNodes.Add(item);
                int? position = null;
                this.m_cutsceneNodes.Add(new MyGuiControlListbox.Item(new StringBuilder(this.m_cutsceneCurrent.SequenceNodes.Count.ToString() + ": " + item.GetNodeSummary()), item.GetNodeDescription(), null, null, null), position);
                this.SelectListboxItemAtIndex(this.m_cutsceneNodes, this.m_cutsceneCurrent.SequenceNodes.Count - 1);
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodeButtonDeleteAllClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_cutsceneCurrent != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_DeleteAllNodes_Caption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.Cutscene_DeleteAllNodes_Text), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.m_cutsceneCurrent.SequenceNodes.Clear();
                        this.m_cutsceneCurrent.SequenceNodes = null;
                        this.m_cutsceneNodes.ClearItems();
                        this.UpdateCutsceneNodeFields();
                        this.m_cutsceneNodes.ScrollToolbarToTop();
                        this.CutsceneChanged();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void CutsceneNodeButtonDeleteClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_cutsceneCurrent != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_DeleteNode_Caption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.Cutscene_DeleteNode_Text), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if ((result == MyGuiScreenMessageBox.ResultEnum.YES) && (this.m_selectedCutsceneNodeIndex >= 0))
                    {
                        this.m_cutsceneCurrent.SequenceNodes.RemoveAt(this.m_selectedCutsceneNodeIndex);
                        this.m_cutsceneNodes.Items.RemoveAt(this.m_selectedCutsceneNodeIndex);
                        this.SelectListboxItemAtIndex(this.m_cutsceneNodes, this.m_selectedCutsceneNodeIndex);
                        this.CutsceneChanged();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void CutsceneNodeButtonMoveDownClicked(MyGuiControlButton myGuiControlButton)
        {
            int listboxSelectedIndex = this.GetListboxSelectedIndex(this.m_cutsceneNodes);
            if ((this.m_cutsceneCurrent != null) && (listboxSelectedIndex >= 0))
            {
                CutsceneSequenceNode node = this.m_cutsceneCurrent.SequenceNodes[listboxSelectedIndex];
                this.m_cutsceneCurrent.SequenceNodes.RemoveAt(listboxSelectedIndex);
                this.m_cutsceneCurrent.SequenceNodes.Insert(listboxSelectedIndex + 1, node);
                MyGuiControlListbox.Item item = this.m_cutsceneNodes.Items[listboxSelectedIndex];
                this.m_cutsceneNodes.Items.RemoveAt(listboxSelectedIndex);
                this.m_cutsceneNodes.Items.Insert(listboxSelectedIndex + 1, item);
                this.SelectListboxItemAtIndex(this.m_cutsceneNodes, listboxSelectedIndex + 1);
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodeButtonMoveUpClicked(MyGuiControlButton myGuiControlButton)
        {
            int listboxSelectedIndex = this.GetListboxSelectedIndex(this.m_cutsceneNodes);
            if ((this.m_cutsceneCurrent != null) && (listboxSelectedIndex >= 0))
            {
                CutsceneSequenceNode node = this.m_cutsceneCurrent.SequenceNodes[listboxSelectedIndex];
                this.m_cutsceneCurrent.SequenceNodes.RemoveAt(listboxSelectedIndex);
                this.m_cutsceneCurrent.SequenceNodes.Insert(listboxSelectedIndex - 1, node);
                MyGuiControlListbox.Item item = this.m_cutsceneNodes.Items[listboxSelectedIndex];
                this.m_cutsceneNodes.Items.RemoveAt(listboxSelectedIndex);
                this.m_cutsceneNodes.Items.Insert(listboxSelectedIndex - 1, item);
                this.SelectListboxItemAtIndex(this.m_cutsceneNodes, listboxSelectedIndex - 1);
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyAttachPositionTo_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                string text;
                string text2;
                if (obj.Text.Length <= 0)
                {
                    text = null;
                }
                else if ((obj.Text.Length > 1) || !obj.Text.ToUpper().Equals("X"))
                {
                    text = obj.Text;
                }
                else
                {
                    text = "";
                }
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachPositionTo = text2;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyAttachRotationTo_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                string text;
                string text2;
                if (obj.Text.Length <= 0)
                {
                    text = null;
                }
                else if ((obj.Text.Length > 1) || !obj.Text.ToUpper().Equals("X"))
                {
                    text = obj.Text;
                }
                else
                {
                    text = "";
                }
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachRotationTo = text2;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyAttachTo_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                string text;
                string text2;
                if (obj.Text.Length <= 0)
                {
                    text = null;
                }
                else if ((obj.Text.Length > 1) || !obj.Text.ToUpper().Equals("X"))
                {
                    text = obj.Text;
                }
                else
                {
                    text = "";
                }
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachTo = text2;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyEvent_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Event = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyEventDelay_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                float num;
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].EventDelay = !float.TryParse(obj.Text, out num) ? 0f : Math.Max(0f, num);
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyFOV_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                float num;
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].ChangeFOVTo = !float.TryParse(obj.Text, out num) ? 0f : Math.Max(0f, num);
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyLockRotationTo_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                string text;
                string text2;
                if (obj.Text.Length <= 0)
                {
                    text = null;
                }
                else if ((obj.Text.Length > 1) || !obj.Text.ToUpper().Equals("X"))
                {
                    text = obj.Text;
                }
                else
                {
                    text = "";
                }
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LockRotationTo = text2;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyLookAt_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].RotateTowards = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyLookAtInstant_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LookAt = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyMoveTo_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].MoveTo = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyMoveToInstant_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].SetPositionTo = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyRotateLike_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].RotateLike = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyRotateLikeInstant_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].SetRorationLike = (obj.Text.Length > 0) ? obj.Text : null;
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyTime_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                float num;
                this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Time = !float.TryParse(obj.Text, out num) ? 0f : Math.Max(0f, num);
                this.CutsceneChanged();
            }
        }

        private void CutsceneNodePropertyWaypoints_TextChanged(MyGuiControlTextbox obj)
        {
            if ((this.m_cutsceneCurrent != null) && (this.m_selectedCutsceneNodeIndex >= 0))
            {
                bool flag = obj.Text.Length == 0;
                if (!flag)
                {
                    string[] separator = new string[] { ";" };
                    string[] strArray = obj.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray.Length == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        if (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints == null)
                        {
                            this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints = new List<CutsceneSequenceNodeWaypoint>();
                        }
                        this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints.Clear();
                        foreach (string str in strArray)
                        {
                            CutsceneSequenceNodeWaypoint item = new CutsceneSequenceNodeWaypoint {
                                Name = str
                            };
                            this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints.Add(item);
                        }
                    }
                }
                if (flag)
                {
                    if (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints != null)
                    {
                        this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints.Clear();
                    }
                    this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints = null;
                }
                this.CutsceneChanged();
            }
        }

        private void CutscenePropertyCanBeSkippedChanged(MyGuiControlCheckbox checkbox)
        {
            if (this.m_cutsceneCurrent != null)
            {
                this.m_cutsceneCurrent.CanBeSkipped = checkbox.IsChecked;
                this.CutsceneChanged();
            }
        }

        private void CutscenePropertyFireEventsDuringSkipChanged(MyGuiControlCheckbox checkbox)
        {
            if (this.m_cutsceneCurrent != null)
            {
                this.m_cutsceneCurrent.FireEventsDuringSkip = checkbox.IsChecked;
                this.CutsceneChanged();
            }
        }

        private void CutscenePropertyNextCutscene_ItemSelected()
        {
            if (this.m_cutsceneCurrent != null)
            {
                this.m_cutsceneCurrent.NextCutscene = (this.m_cutscenePropertyNextCutscene.GetSelectedKey() != 0) ? this.m_cutscenePropertyNextCutscene.GetSelectedValue().ToString() : null;
                this.CutsceneChanged();
            }
        }

        private void CutscenePropertyStartEntity_TextChanged(MyGuiControlTextbox obj)
        {
            if (this.m_cutsceneCurrent != null)
            {
                this.m_cutsceneCurrent.StartEntity = obj.Text;
                this.CutsceneChanged();
            }
        }

        private void CutscenePropertyStartingFOV_TextChanged(MyGuiControlTextbox obj)
        {
            if (this.m_cutsceneCurrent != null)
            {
                float num;
                this.m_cutsceneCurrent.StartingFOV = !float.TryParse(obj.Text, out num) ? 70f : num;
                this.CutsceneChanged();
            }
        }

        private void CutscenePropertyStartLookAt_TextChanged(MyGuiControlTextbox obj)
        {
            if (this.m_cutsceneCurrent != null)
            {
                this.m_cutsceneCurrent.StartLookAt = obj.Text;
                this.CutsceneChanged();
            }
        }

        private void DeleteCurrentCutsceneClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_cutsceneSelection.GetSelectedIndex() >= 0)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_Delete_Caption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.Cutscene_Delete_Text), this.m_cutsceneSelection.GetItemByIndex(this.m_cutsceneSelection.GetSelectedIndex()).Value), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.m_cutscenes.Remove(this.m_cutsceneSelection.GetItemByIndex(this.m_cutsceneSelection.GetSelectedIndex()).Value.ToString());
                        this.m_cutsceneSelection.RemoveItemByIndex(this.m_cutsceneSelection.GetSelectedIndex());
                        if (this.m_cutscenes.Count > 0)
                        {
                            this.m_cutsceneSelection.SelectItemByIndex(0);
                        }
                        else
                        {
                            this.UpdateCutsceneFields();
                        }
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void DeleteEntityOnClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_transformSys.ControlledEntity != null)
            {
                if (this.m_waypoints.Contains(this.m_transformSys.ControlledEntity))
                {
                    this.m_waypoints.Remove(this.m_transformSys.ControlledEntity);
                    this.UpdateWaypointList();
                }
                this.m_transformSys.ControlledEntity.Close();
                this.m_transformSys.SetControlledEntity(null);
            }
        }

        private void DeleteTriggerOnClick(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_triggerManipulator.SelectedTrigger != null)
            {
                if (this.m_triggerManipulator.SelectedTrigger.Entity != null)
                {
                    this.m_triggerManipulator.SelectedTrigger.Entity.Components.Remove(typeof(MyTriggerAggregate), this.m_triggerManipulator.SelectedTrigger);
                }
                this.m_triggerManipulator.SelectedTrigger = null;
                this.m_helperStringBuilder.Clear();
                this.m_selectedEntityNameBox.SetText(this.m_helperStringBuilder);
            }
        }

        private void DeselectEntity()
        {
            this.m_transformSys.SetControlledEntity(null);
            this.m_waypointsListBox.SelectedItems.Clear();
        }

        private void DeselectEntityOnClicked(MyGuiControlButton myGuiControlButton)
        {
            this.m_transformSys.SetControlledEntity(null);
        }

        private void DeselectTrigger()
        {
            this.m_selectedTriggerNameBox.SetText(new StringBuilder());
            this.m_triggerManipulator.SelectedTrigger = null;
            this.m_triggersListBox.SelectedItems.Clear();
        }

        private void DisableTransformationOnCheckedChanged(MyGuiControlCheckbox checkbox)
        {
            this.m_transformSys.DisableTransformation = checkbox.IsChecked;
        }

        private int DrawDictionary<T>(SerializableDictionary<string, T> dict, string title, Vector2 start, Vector2 offset, float fontScale, int startIndex)
        {
            if (dict.Dictionary.Count != 0)
            {
                MyRenderProxy.DebugDrawText2D(start + (startIndex * offset), $"{title}", Color.Orange, fontScale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                startIndex++;
                foreach (KeyValuePair<string, T> pair in dict.Dictionary)
                {
                    T local = pair.Value;
                    MyRenderProxy.DebugDrawText2D(start + (startIndex * offset), $"{pair.Key.ToString()} :    {local.ToString()}", Color.Yellow, fontScale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    startIndex++;
                }
            }
            return startIndex;
        }

        private void EnlargeTriggerOnClick(MyGuiControlButton button)
        {
            if (this.m_triggerManipulator.SelectedTrigger != null)
            {
                MyAreaTriggerComponent selectedTrigger = (MyAreaTriggerComponent) this.m_triggerManipulator.SelectedTrigger;
                selectedTrigger.Radius += 0.20000000298023224;
            }
        }

        private int GetListboxSelectedIndex(MyGuiControlListbox listbox)
        {
            if (listbox.SelectedItems.Count != 0)
            {
                for (int i = 0; i < listbox.Items.Count; i++)
                {
                    if (listbox.Items[i] == listbox.SelectedItems[0])
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (this.m_transformSys.DisablePicking)
            {
                this.m_transformSys.DisablePicking = false;
            }
            if (MyInput.Static.IsNewPrimaryButtonPressed())
            {
                Vector2 vector = base.GetPosition() - (SCREEN_SIZE * 0.5f);
                if (MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate(MyInput.Static.GetMousePosition()).X > vector.X)
                {
                    this.m_transformSys.DisablePicking = true;
                }
            }
            if (!MyToolbarComponent.IsToolbarControlShown)
            {
                MyToolbarComponent.IsToolbarControlShown = true;
            }
            if (m_currentScreen == ScriptingToolsScreen.Transformation)
            {
                base.FocusedControl = null;
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape) || MyInput.Static.IsNewKeyPressed(MyKeys.F11))
            {
                if ((m_currentScreen == ScriptingToolsScreen.Transformation) || !this.m_cutsceneSaveButton.Enabled)
                {
                    this.CloseScreen();
                }
                else
                {
                    this.CloseScreenWithSave();
                }
            }
            else
            {
                base.HandleInput(receivedFocusInThisUpdate);
                if (MySpectatorCameraController.Static.SpectatorCameraMovement != MySpectatorCameraMovementEnum.FreeMouse)
                {
                    MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.FreeMouse;
                }
                foreach (MyGuiScreenBase base2 in MyScreenManager.Screens)
                {
                    if (!(base2 is MyGuiScreenScriptingTools))
                    {
                        base2.HandleInput(receivedFocusInThisUpdate);
                    }
                }
                if (m_currentScreen == ScriptingToolsScreen.Transformation)
                {
                    this.HandleShortcuts();
                }
            }
        }

        private void HandleShortcuts()
        {
            if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.D))
            {
                this.DeselectEntityOnClicked(null);
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.PageUp))
            {
                InitialShift = !MyInput.Static.IsKeyPress(MyKeys.Control) ? (!MyInput.Static.IsKeyPress(MyKeys.Shift) ? (InitialShift - 1) : (InitialShift - 100)) : (!MyInput.Static.IsKeyPress(MyKeys.Shift) ? (InitialShift - 10) : (InitialShift - 0x3e8));
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.PageDown))
            {
                InitialShift = !MyInput.Static.IsKeyPress(MyKeys.Control) ? (!MyInput.Static.IsKeyPress(MyKeys.Shift) ? (InitialShift + 1) : (InitialShift + 100)) : (!MyInput.Static.IsKeyPress(MyKeys.Shift) ? (InitialShift + 10) : (InitialShift + 0x3e8));
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Home))
            {
                InitialShift = 0;
            }
            if ((!MyInput.Static.IsAnyShiftKeyPressed() && !MyInput.Static.IsAnyCtrlKeyPressed()) && !MyInput.Static.IsAnyAltKeyPressed())
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                {
                    this.EnlargeTriggerOnClick(null);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                {
                    this.ShrinkTriggerOnClick(null);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Delete))
                {
                    this.DeleteEntityOnClicked(null);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.N))
                {
                    this.SpawnEntityClicked(null);
                }
            }
        }

        private void InitializeWaypointList()
        {
            this.m_waypoints.Clear();
            foreach (VRage.Game.Entity.MyEntity entity in Sandbox.Game.Entities.MyEntities.GetEntities())
            {
                if (this.IsWaypoint(entity))
                {
                    this.m_waypoints.Add(entity);
                }
            }
        }

        private bool IsWaypoint(VRage.Game.Entity.MyEntity ent) => 
            ((ent.Name != null) ? ((ent.Name.Length >= ENTITY_NAME_PREFIX.Length) && ENTITY_NAME_PREFIX.Equals(ent.Name.Substring(0, ENTITY_NAME_PREFIX.Length))) : false);

        private void m_cutsceneNodes_ItemsSelected(MyGuiControlListbox obj)
        {
            bool enabled = this.m_cutsceneSaveButton.Enabled;
            this.m_selectedCutsceneNodeIndex = this.GetListboxSelectedIndex(this.m_cutsceneNodes);
            this.UpdateCutsceneNodeFields();
            this.m_cutsceneSaveButton.Enabled = enabled;
            this.m_cutsceneRevertButton.Enabled = enabled;
        }

        private void m_cutsceneSelection_ItemSelected()
        {
            if (!this.m_cutsceneSaveButton.Enabled)
            {
                this.UpdateCutsceneFields();
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_Unsaved_Text);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.Cutscene_Unsaved_Caption), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.SaveCutsceneClicked(this.m_cutsceneSaveButton);
                    }
                    if ((result == MyGuiScreenMessageBox.ResultEnum.YES) || (result == MyGuiScreenMessageBox.ResultEnum.NO))
                    {
                        this.UpdateCutsceneFields();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void NewCutscene()
        {
            MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption(MyTexts.Get(MyCommonTexts.Cutscene_New_Caption).ToString(), "", delegate (string text) {
                if (this.m_cutscenes.ContainsKey(text))
                {
                    return false;
                }
                Cutscene cutscene = new Cutscene {
                    Name = text
                };
                this.m_cutscenes.Add(text, cutscene);
                long key = text.GetHashCode64();
                int? sortOrder = null;
                this.m_cutsceneSelection.AddItem(key, text, sortOrder, null);
                this.m_cutsceneSelection.SelectItemByKey(key, true);
                return true;
            }));
        }

        private unsafe void PositionControl(MyGuiControlBase control)
        {
            float x = (SCREEN_SIZE.X - HIDDEN_PART_RIGHT) - (ITEM_HORIZONTAL_PADDING * 2f);
            control.Position = new Vector2((base.m_currentPosition.X - (SCREEN_SIZE.X / 2f)) + ITEM_HORIZONTAL_PADDING, base.m_currentPosition.Y + ITEM_VERTICAL_PADDING);
            control.Size = new Vector2(x, control.Size.Y);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += control.Size.Y + ITEM_VERTICAL_PADDING;
        }

        private unsafe void PositionControls(MyGuiControlBase[] controls)
        {
            float x = (((SCREEN_SIZE.X - HIDDEN_PART_RIGHT) - (ITEM_HORIZONTAL_PADDING * 2f)) / ((float) controls.Length)) - (0.001f * controls.Length);
            float num2 = x + (0.001f * controls.Length);
            float y = 0f;
            for (int i = 0; i < controls.Length; i++)
            {
                MyGuiControlBase base2 = controls[i];
                base2.Size = (base2 is MyGuiControlCheckbox) ? new Vector2(BUTTON_SIZE.Y) : new Vector2(x, base2.Size.Y);
                base2.PositionX = ((base.m_currentPosition.X + (num2 * i)) - (SCREEN_SIZE.X / 2f)) + ITEM_HORIZONTAL_PADDING;
                base2.PositionY = base.m_currentPosition.Y + ITEM_VERTICAL_PADDING;
                if (base2.Size.Y > y)
                {
                    y = base2.Size.Y;
                }
            }
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += y + ITEM_VERTICAL_PADDING;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            float y = (SCREEN_SIZE.Y - 1f) / 2f;
            Vector2 vector = new Vector2(0.02f, 0f);
            string text = null;
            text = (m_currentScreen != ScriptingToolsScreen.Transformation) ? MyTexts.Get(MySpaceTexts.ScriptingToolsCutscenes).ToString() : MyTexts.Get(MySpaceTexts.ScriptingToolsTransformations).ToString();
            MyGuiControlLabel label = base.AddCaption(text, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector + new Vector2(-HIDDEN_PART_RIGHT, y)), 0.8f);
            base.m_currentPosition.Y = (label.PositionY + label.Size.Y) + ITEM_VERTICAL_PADDING;
            MyGuiControlBase[] controls = new MyGuiControlBase[] { this.CreateButton(MyTexts.Get(MySpaceTexts.TransformationToolsButton).ToString(), new Action<MyGuiControlButton>(this.SwitchPageToTransformation), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_TransformTools)), this.CreateButton(MyTexts.Get(MySpaceTexts.CutsceneToolsButton).ToString(), new Action<MyGuiControlButton>(this.SwitchPageToCutscenes), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_CutsceneTools)) };
            this.PositionControls(controls);
            bool flag = m_currentScreen == ScriptingToolsScreen.Transformation;
            this.m_transformSys.Active = flag;
            base.m_canShareInput = flag;
            MyGuiScreenGamePlay.DisableInput = !flag;
            ScriptingToolsScreen currentScreen = m_currentScreen;
            if (currentScreen == ScriptingToolsScreen.Transformation)
            {
                this.RecreateControlsTransformation();
            }
            else if (currentScreen == ScriptingToolsScreen.Cutscenes)
            {
                this.RecreateControlsCutscenes();
            }
        }

        private unsafe void RecreateControlsCutscenes()
        {
            this.m_cutscenes = MySession.Static.GetComponent<MySessionComponentCutscenes>().GetCutscenes();
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += ITEM_SIZE.Y;
            MyGuiControlBase[] controls = new MyGuiControlBase[] { this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_New).ToString(), new Action<MyGuiControlButton>(this.CreateNewCutsceneClicked), null), this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_ClearAllCutscenes).ToString(), new Action<MyGuiControlButton>(this.ClearAllCutscenesClicked), null) };
            this.PositionControls(controls);
            this.m_cutsceneSelection = this.CreateComboBox();
            foreach (Cutscene cutscene in this.m_cutscenes.Values)
            {
                int? sortOrder = null;
                this.m_cutsceneSelection.AddItem(cutscene.Name.GetHashCode64(), cutscene.Name, sortOrder, null);
            }
            this.m_cutsceneSelection.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_cutsceneSelection_ItemSelected);
            MyGuiControlBase[] baseArray2 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Selected).ToString()), this.m_cutsceneSelection };
            this.PositionControls(baseArray2);
            this.m_cutsceneDeleteButton = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Delete).ToString(), new Action<MyGuiControlButton>(this.DeleteCurrentCutsceneClicked), null);
            this.m_cutscenePlayButton = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Play).ToString(), new Action<MyGuiControlButton>(this.WatchCutsceneClicked), null);
            this.m_cutscenePlayButton.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Play_Extended).ToString());
            this.m_cutsceneSaveButton = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Save).ToString(), new Action<MyGuiControlButton>(this.SaveCutsceneClicked), null);
            this.m_cutsceneRevertButton = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Revert).ToString(), new Action<MyGuiControlButton>(this.RevertCutsceneClicked), null);
            MyGuiControlBase[] baseArray3 = new MyGuiControlBase[] { this.m_cutscenePlayButton, this.m_cutsceneSaveButton, this.m_cutsceneRevertButton, this.m_cutsceneDeleteButton };
            this.PositionControls(baseArray3);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += ITEM_SIZE.Y / 2f;
            this.m_cutscenePropertyNextCutscene = this.CreateComboBox();
            this.m_cutscenePropertyNextCutscene.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.CutscenePropertyNextCutscene_ItemSelected);
            MyGuiControlBase[] baseArray4 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_New).ToString()), this.m_cutscenePropertyNextCutscene };
            this.PositionControls(baseArray4);
            MyGuiControlBase[] baseArray5 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_PosRot).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_LookRot).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_FOV).ToString()) };
            this.PositionControls(baseArray5);
            this.m_cutscenePropertyStartEntity = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutscenePropertyStartEntity_TextChanged));
            this.m_cutscenePropertyStartEntity.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_PosRot_Extended).ToString());
            this.m_cutscenePropertyStartLookAt = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutscenePropertyStartLookAt_TextChanged));
            this.m_cutscenePropertyStartLookAt.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_LookRot_Extended).ToString());
            this.m_cutscenePropertyStartingFOV = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutscenePropertyStartingFOV_TextChanged));
            this.m_cutscenePropertyStartingFOV.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_FOV_Extended).ToString());
            MyGuiControlBase[] baseArray6 = new MyGuiControlBase[] { this.m_cutscenePropertyStartEntity, this.m_cutscenePropertyStartLookAt, this.m_cutscenePropertyStartingFOV };
            this.PositionControls(baseArray6);
            this.m_cutscenePropertyCanBeSkipped = this.CreateCheckbox(new Action<MyGuiControlCheckbox>(this.CutscenePropertyCanBeSkippedChanged), true, null);
            this.m_cutscenePropertyCanBeSkipped.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Skippable).ToString());
            this.m_cutscenePropertyFireEventsDuringSkip = this.CreateCheckbox(new Action<MyGuiControlCheckbox>(this.CutscenePropertyFireEventsDuringSkipChanged), true, null);
            this.m_cutscenePropertyFireEventsDuringSkip.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_SkipWarning).ToString());
            MyGuiControlBase[] baseArray7 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_CanSkip).ToString()), this.m_cutscenePropertyCanBeSkipped, this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Events).ToString()), this.m_cutscenePropertyFireEventsDuringSkip };
            this.PositionControls(baseArray7);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += ITEM_SIZE.Y;
            this.m_cutsceneNodeButtonAdd = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_AddNode).ToString(), new Action<MyGuiControlButton>(this.CutsceneNodeButtonAddClicked), null);
            this.m_cutsceneNodeButtonDelete = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Delete).ToString(), new Action<MyGuiControlButton>(this.CutsceneNodeButtonDeleteClicked), null);
            this.m_cutsceneNodeButtonDeleteAll = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_ClearAll).ToString(), new Action<MyGuiControlButton>(this.CutsceneNodeButtonDeleteAllClicked), null);
            this.m_cutsceneNodeButtonMoveUp = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_MoveUp).ToString(), new Action<MyGuiControlButton>(this.CutsceneNodeButtonMoveUpClicked), null);
            this.m_cutsceneNodeButtonMoveDown = this.CreateButton(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_MoveDown).ToString(), new Action<MyGuiControlButton>(this.CutsceneNodeButtonMoveDownClicked), null);
            MyGuiControlBase[] baseArray8 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Nodes).ToString()), this.m_cutsceneNodeButtonAdd, this.m_cutsceneNodeButtonDeleteAll };
            this.PositionControls(baseArray8);
            MyGuiControlBase[] baseArray9 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_CurrentNode).ToString()), this.m_cutsceneNodeButtonMoveUp, this.m_cutsceneNodeButtonMoveDown, this.m_cutsceneNodeButtonDelete };
            this.PositionControls(baseArray9);
            this.m_cutsceneNodes = this.CreateListBox();
            this.m_cutsceneNodes.VisibleRowsCount = 5;
            this.m_cutsceneNodes.Size = new Vector2(0f, 0.12f);
            this.m_cutsceneNodes.ItemsSelected += new Action<MyGuiControlListbox>(this.m_cutsceneNodes_ItemsSelected);
            this.PositionControl(this.m_cutsceneNodes);
            this.m_cutsceneNodePropertyTime = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyTime_TextChanged));
            this.m_cutsceneNodePropertyTime.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Time_Extended).ToString());
            this.m_cutsceneNodePropertyEvent = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyEvent_TextChanged));
            this.m_cutsceneNodePropertyEvent.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Event_Extended).ToString());
            this.m_cutsceneNodePropertyEventDelay = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyEventDelay_TextChanged));
            this.m_cutsceneNodePropertyEventDelay.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_EventDelay_Extended).ToString());
            this.m_cutsceneNodePropertyFOVChange = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyFOV_TextChanged));
            this.m_cutsceneNodePropertyFOVChange.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_FOVChange_Extended).ToString());
            MyGuiControlBase[] baseArray10 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Time).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Event).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_EventDelay).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_FOVChange).ToString()) };
            this.PositionControls(baseArray10);
            MyGuiControlBase[] baseArray11 = new MyGuiControlBase[] { this.m_cutsceneNodePropertyTime, this.m_cutsceneNodePropertyEvent, this.m_cutsceneNodePropertyEventDelay, this.m_cutsceneNodePropertyFOVChange };
            this.PositionControls(baseArray11);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += ITEM_SIZE.Y / 2f;
            MyGuiControlBase[] baseArray12 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Action).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_OverTime).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Instant).ToString()) };
            this.PositionControls(baseArray12);
            this.m_cutsceneNodePropertyMoveTo = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyMoveTo_TextChanged));
            this.m_cutsceneNodePropertyMoveTo.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_MoveTo_Extended1).ToString());
            this.m_cutsceneNodePropertyMoveToInstant = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyMoveToInstant_TextChanged));
            this.m_cutsceneNodePropertyMoveToInstant.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_MoveTo_Extended2).ToString());
            MyGuiControlBase[] baseArray13 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_MoveTo).ToString()), this.m_cutsceneNodePropertyMoveTo, this.m_cutsceneNodePropertyMoveToInstant };
            this.PositionControls(baseArray13);
            this.m_cutsceneNodePropertyRotateLike = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyRotateLike_TextChanged));
            this.m_cutsceneNodePropertyRotateLike.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_RotateLike_Extended1).ToString());
            this.m_cutsceneNodePropertyRotateLikeInstant = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyRotateLikeInstant_TextChanged));
            this.m_cutsceneNodePropertyRotateLikeInstant.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_RotateLike_Extended2).ToString());
            MyGuiControlBase[] baseArray14 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_RotateLike).ToString()), this.m_cutsceneNodePropertyRotateLike, this.m_cutsceneNodePropertyRotateLikeInstant };
            this.PositionControls(baseArray14);
            this.m_cutsceneNodePropertyRotateTowards = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyLookAt_TextChanged));
            this.m_cutsceneNodePropertyRotateTowards.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_LookAt_Extended1).ToString());
            this.m_cutsceneNodePropertyRotateTowardsInstant = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyLookAtInstant_TextChanged));
            this.m_cutsceneNodePropertyRotateTowardsInstant.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_LookAt_Extended2).ToString());
            MyGuiControlBase[] baseArray15 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_LookAt).ToString()), this.m_cutsceneNodePropertyRotateTowards, this.m_cutsceneNodePropertyRotateTowardsInstant };
            this.PositionControls(baseArray15);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += ITEM_SIZE.Y;
            this.m_cutsceneNodePropertyRotateTowardsLock = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyLockRotationTo_TextChanged));
            this.m_cutsceneNodePropertyRotateTowardsLock.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Track_Extended1).ToString());
            this.m_cutsceneNodePropertyAttachAll = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyAttachTo_TextChanged));
            this.m_cutsceneNodePropertyAttachAll.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Track_Extended2).ToString());
            this.m_cutsceneNodePropertyAttachPosition = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyAttachPositionTo_TextChanged));
            this.m_cutsceneNodePropertyAttachPosition.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Track_Extended3).ToString());
            this.m_cutsceneNodePropertyAttachRotation = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyAttachRotationTo_TextChanged));
            this.m_cutsceneNodePropertyAttachRotation.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Track_Extended4).ToString());
            MyGuiControlBase[] baseArray16 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_TrackLook).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_TrackPosRot).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_TrackPos).ToString()), this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_TrackRot).ToString()) };
            this.PositionControls(baseArray16);
            MyGuiControlBase[] baseArray17 = new MyGuiControlBase[] { this.m_cutsceneNodePropertyRotateTowardsLock, this.m_cutsceneNodePropertyAttachAll, this.m_cutsceneNodePropertyAttachPosition, this.m_cutsceneNodePropertyAttachRotation };
            this.PositionControls(baseArray17);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += ITEM_SIZE.Y / 2f;
            this.m_cutsceneNodePropertyWaypoints = this.CreateTextbox("", new Action<MyGuiControlTextbox>(this.CutsceneNodePropertyWaypoints_TextChanged));
            this.m_cutsceneNodePropertyWaypoints.SetToolTip(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Waypoints_Extended).ToString());
            this.PositionControl(this.CreateLabel(MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_Waypoints).ToString()));
            this.PositionControl(this.m_cutsceneNodePropertyWaypoints);
            this.m_cutsceneCurrent = null;
            this.m_selectedCutsceneNodeIndex = -1;
            this.m_cutsceneSaveButton.Enabled = false;
            if (this.m_cutscenes.Count > 0)
            {
                this.m_cutsceneSelection.SelectItemByIndex(0);
            }
            else
            {
                this.UpdateCutsceneFields();
            }
        }

        private void RecreateControlsTransformation()
        {
            MyGuiControlBase[] controls = new MyGuiControlBase[] { this.CreateLabel(MyTexts.GetString(MySpaceTexts.DisableTransformation)), this.CreateCheckbox(new Action<MyGuiControlCheckbox>(this.DisableTransformationOnCheckedChanged), this.m_transformSys.DisableTransformation, MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_DisableTransform)) };
            this.PositionControls(controls);
            MyGuiControlBase[] baseArray2 = new MyGuiControlBase[] { this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_Translation), x => this.SelectOperation(MyEntityTransformationSystem.OperationMode.Translation), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_Translation)), this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_Rotation), x => this.SelectOperation(MyEntityTransformationSystem.OperationMode.Rotation), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_Rotation)) };
            this.PositionControls(baseArray2);
            MyGuiControlBase[] baseArray3 = new MyGuiControlBase[] { this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_Coords_World), x => this.SelectCoordsWorld(true), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_WorldCoords)), this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_Coords_Local), x => this.SelectCoordsWorld(false), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_LocalCoords)) };
            this.PositionControls(baseArray3);
            this.m_selectedEntityNameBox = this.CreateTextbox("", null);
            MyGuiControlBase[] baseArray4 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.GetString(MySpaceTexts.SelectedEntity) + ": "), this.m_selectedEntityNameBox, this.CreateButton(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_ButtonRename), new Action<MyGuiControlButton>(this.RenameSelectedEntityOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_Rename1)) };
            this.PositionControls(baseArray4);
            this.m_selectedFunctionalBlockNameBox = this.CreateTextbox("", null);
            MyGuiControlBase[] baseArray5 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.GetString(MySpaceTexts.SelectedBlock) + ": "), this.m_selectedFunctionalBlockNameBox, this.CreateButton(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_ButtonRename), new Action<MyGuiControlButton>(this.RenameFunctionalBlockOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_Rename2)) };
            this.PositionControls(baseArray5);
            MyGuiControlBase[] baseArray6 = new MyGuiControlBase[] { this.CreateButton(MyTexts.GetString(MySpaceTexts.SpawnEntity), new Action<MyGuiControlButton>(this.SpawnEntityClicked), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_SpawnEnt)), this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_DeselectEntity), new Action<MyGuiControlButton>(this.DeselectEntityOnClicked), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_DeselectEnt)), this.CreateButton(MyTexts.GetString(MySpaceTexts.DeleteEntity), new Action<MyGuiControlButton>(this.DeleteEntityOnClicked), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_DeleteEnt)), this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_SetPosition), new Action<MyGuiControlButton>(this.SetPositionOnClicked), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_SetPosition)) };
            this.PositionControls(baseArray6);
            this.m_waypointsListBox = this.CreateListBox();
            this.m_waypointsListBox.Size = new Vector2(0f, 0.148f);
            this.m_waypointsListBox.ItemClicked += new Action<MyGuiControlListbox>(this.WaypointsListBoxOnItemDoubleClicked);
            this.PositionControl(this.m_waypointsListBox);
            this.PositionControl(this.CreateLabel(MyTexts.GetString(MySpaceTexts.Triggers)));
            this.PositionControl(this.CreateButton(MyTexts.GetString(MySpaceTexts.AttachToSelectedEntity), new Action<MyGuiControlButton>(this.AttachTriggerOnClick), null));
            this.m_enlargeTriggerButton = this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_Grow), new Action<MyGuiControlButton>(this.EnlargeTriggerOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_SizeGrow));
            this.m_shrinkTriggerButton = this.CreateButton(MyTexts.GetString(MyCommonTexts.ScriptingTools_Shrink), new Action<MyGuiControlButton>(this.ShrinkTriggerOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_SizeShrink));
            this.m_setTriggerSizeButton = this.CreateButton(MyTexts.GetString(MyCommonTexts.Size), new Action<MyGuiControlButton>(this.SetSizeOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_SizeSet));
            MyGuiControlBase[] baseArray7 = new MyGuiControlBase[] { this.m_enlargeTriggerButton, this.m_setTriggerSizeButton, this.m_shrinkTriggerButton };
            this.PositionControls(baseArray7);
            MyGuiControlBase[] baseArray8 = new MyGuiControlBase[] { this.CreateButton(MyTexts.GetString(MyCommonTexts.Snap), new Action<MyGuiControlButton>(this.SnapTriggerToCameraOrEntityOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_TriggerSnap)), this.CreateButton(MyTexts.GetString(MyCommonTexts.Select), new Action<MyGuiControlButton>(this.SelectTriggerOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_TriggerSelect)), this.CreateButton(MyTexts.GetString(MyCommonTexts.Delete), new Action<MyGuiControlButton>(this.DeleteTriggerOnClick), MyTexts.GetString(MyCommonTexts.ScriptingTools_Tooltip_TriggerDelete)) };
            this.PositionControls(baseArray8);
            this.m_selectedTriggerNameBox = this.CreateTextbox(MyTexts.GetString(MySpaceTexts.TriggerNotSelected), null);
            MyGuiControlBase[] baseArray9 = new MyGuiControlBase[] { this.CreateLabel(MyTexts.GetString(MySpaceTexts.SelectedTrigger) + ":"), this.m_selectedTriggerNameBox };
            this.PositionControls(baseArray9);
            this.m_triggersListBox = this.CreateListBox();
            this.m_triggersListBox.Size = new Vector2(0f, 0.14f);
            this.m_triggersListBox.ItemClicked += new Action<MyGuiControlListbox>(this.TriggersListBoxOnItemDoubleClicked);
            this.PositionControl(this.m_triggersListBox);
            this.PositionControl(this.CreateLabel(MyTexts.Get(MySpaceTexts.RunningLevelScripts).ToString()));
            this.m_levelScriptListBox = this.CreateListBox();
            this.m_levelScriptListBox.Size = new Vector2(0f, 0.07f);
            this.PositionControl(this.m_levelScriptListBox);
            foreach (string str in this.m_scriptManager.RunningLevelScriptNames)
            {
                int? position = null;
                this.m_levelScriptListBox.Add(new MyGuiControlListbox.Item(new StringBuilder(str), null, null, false, null), position);
            }
            this.PositionControl(this.CreateLabel(MyTexts.Get(MySpaceTexts.RunningStateMachines).ToString()));
            this.m_smListBox = this.CreateListBox();
            this.m_smListBox.Size = new Vector2(0f, 0.07f);
            this.PositionControl(this.m_smListBox);
            this.m_smListBox.ItemSize = new Vector2(SCREEN_SIZE.X, ITEM_SIZE.Y);
        }

        private void RenameFunctionalBlockOnClick(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_selectedFunctionalBlock != null)
            {
                this.m_disablePicking = true;
                this.m_transformSys.DisablePicking = true;
                ValueGetScreenWithCaption screen = new ValueGetScreenWithCaption(MyTexts.Get(MySpaceTexts.EntityRename).ToString() + ": " + this.m_selectedFunctionalBlock.DisplayNameText, "", delegate (string text) {
                    VRage.Game.Entity.MyEntity entity;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(text, out entity))
                    {
                        return false;
                    }
                    this.m_selectedFunctionalBlock.Name = text;
                    Sandbox.Game.Entities.MyEntities.SetEntityName(this.m_selectedFunctionalBlock, true);
                    this.m_helperStringBuilder.Clear().Append(text);
                    this.m_selectedFunctionalBlockNameBox.SetText(this.m_helperStringBuilder);
                    return true;
                });
                screen.Closed += delegate (MyGuiScreenBase source) {
                    this.m_disablePicking = false;
                    this.m_transformSys.DisablePicking = false;
                };
                MyGuiSandbox.AddScreen(screen);
            }
        }

        private void RenameSelectedEntityOnClick(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_transformSys.ControlledEntity != null)
            {
                this.m_disablePicking = true;
                this.m_transformSys.DisablePicking = true;
                VRage.Game.Entity.MyEntity selectedEntity = this.m_transformSys.ControlledEntity;
                ValueGetScreenWithCaption screen = new ValueGetScreenWithCaption(MyTexts.Get(MySpaceTexts.EntityRename).ToString() + ": " + this.m_transformSys.ControlledEntity.DisplayNameText, "", delegate (string text) {
                    VRage.Game.Entity.MyEntity entity;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(text, out entity))
                    {
                        return false;
                    }
                    selectedEntity.Name = text;
                    Sandbox.Game.Entities.MyEntities.SetEntityName(selectedEntity, true);
                    this.m_helperStringBuilder.Clear().Append(text);
                    this.m_selectedEntityNameBox.SetText(this.m_helperStringBuilder);
                    this.InitializeWaypointList();
                    this.UpdateWaypointList();
                    return true;
                });
                screen.Closed += delegate (MyGuiScreenBase source) {
                    this.m_disablePicking = false;
                    this.m_transformSys.DisablePicking = false;
                };
                MyGuiSandbox.AddScreen(screen);
            }
        }

        private void RevertCutsceneClicked(MyGuiControlButton myGuiControlButton)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Cutscene_Revert_Caption);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.Cutscene_Revert_Text), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    this.UpdateCutsceneFields();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void SaveCutsceneClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_cutsceneCurrent != null)
            {
                this.m_cutscenes[this.m_cutsceneCurrent.Name] = this.m_cutsceneCurrent;
                this.m_cutsceneSaveButton.Enabled = false;
                this.m_cutsceneRevertButton.Enabled = false;
            }
        }

        private void SelectCoordsWorld(bool world)
        {
            this.m_transformSys.ChangeCoordSystem(world);
        }

        private void SelectListboxItemAtIndex(MyGuiControlListbox listbox, int index)
        {
            List<bool> states = new List<bool>();
            for (int i = 0; i < this.m_cutsceneCurrent.SequenceNodes.Count; i++)
            {
                states.Add(i == index);
            }
            this.m_cutsceneNodes.ChangeSelection(states);
        }

        private void SelectOperation(MyEntityTransformationSystem.OperationMode mode)
        {
            this.m_transformSys.ChangeOperationMode(mode);
        }

        private void SelectTriggerOnClick(MyGuiControlButton button)
        {
            this.m_triggerManipulator.SelectClosest(MyAPIGateway.Session.Camera.Position);
            if (this.m_triggerManipulator.SelectedTrigger != null)
            {
                MyAreaTriggerComponent selectedTrigger = (MyAreaTriggerComponent) this.m_triggerManipulator.SelectedTrigger;
                this.m_helperStringBuilder.Clear();
                this.m_helperStringBuilder.Append(selectedTrigger.Name);
                this.m_selectedTriggerNameBox.SetText(this.m_helperStringBuilder);
            }
        }

        private void SetPositionOnClicked(MyGuiControlButton button)
        {
            if (this.m_transformSys.ControlledEntity != null)
            {
                VRage.Game.Entity.MyEntity entity = this.m_transformSys.ControlledEntity;
                Vector3D position = entity.PositionComp.GetPosition();
                MyGuiSandbox.AddScreen(new Vector3GetScreenWithCaption(MyTexts.GetString(MySpaceTexts.SetEntityPositionDialog), position.X.ToString(), position.Y.ToString(), position.Z.ToString(), delegate (string text1, string text2, string text3) {
                    double num;
                    double num2;
                    double num3;
                    if ((!double.TryParse(text1, out num) || !double.TryParse(text2, out num2)) || !double.TryParse(text3, out num3))
                    {
                        return false;
                    }
                    MatrixD worldMatrix = entity.WorldMatrix;
                    worldMatrix.Translation = new Vector3D(num, num2, num3);
                    entity.WorldMatrix = worldMatrix;
                    return true;
                }));
            }
        }

        private void SetSizeOnClick(MyGuiControlButton button)
        {
            if (this.m_triggerManipulator.SelectedTrigger != null)
            {
                MyAreaTriggerComponent areaTrigger = (MyAreaTriggerComponent) this.m_triggerManipulator.SelectedTrigger;
                MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption(MyTexts.Get(MySpaceTexts.SetTriggerSizeDialog).ToString(), areaTrigger.Radius.ToString(CultureInfo.InvariantCulture), delegate (string text) {
                    float num;
                    if (!float.TryParse(text, out num))
                    {
                        return false;
                    }
                    areaTrigger.Radius = num;
                    return true;
                }));
            }
        }

        private void ShrinkTriggerOnClick(MyGuiControlButton button)
        {
            if (this.m_triggerManipulator.SelectedTrigger != null)
            {
                MyAreaTriggerComponent selectedTrigger = (MyAreaTriggerComponent) this.m_triggerManipulator.SelectedTrigger;
                selectedTrigger.Radius -= 0.20000000298023224;
                if (selectedTrigger.Radius < 0.20000000298023224)
                {
                    selectedTrigger.Radius = 0.20000000298023224;
                }
            }
        }

        private void SnapTriggerToCameraOrEntityOnClick(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_triggerManipulator.SelectedTrigger != null)
            {
                MyAreaTriggerComponent selectedTrigger = (MyAreaTriggerComponent) this.m_triggerManipulator.SelectedTrigger;
                if (this.m_transformSys.ControlledEntity != null)
                {
                    selectedTrigger.Center = this.m_transformSys.ControlledEntity.PositionComp.GetPosition();
                }
                else
                {
                    selectedTrigger.Center = MyAPIGateway.Session.Camera.Position;
                }
            }
        }

        private void SpawnEntityClicked(MyGuiControlButton myGuiControlButton)
        {
            while (true)
            {
                VRage.Game.Entity.MyEntity entity;
                m_entityCounter++;
                string name = ENTITY_NAME_PREFIX + m_entityCounter;
                if (!Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity))
                {
                    VRage.Game.Entity.MyEntity entity1 = new VRage.Game.Entity.MyEntity();
                    entity1.WorldMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                    entity1.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                    entity1.DisplayName = "Entity";
                    entity1.Name = name;
                    VRage.Game.Entity.MyEntity entity2 = entity1;
                    entity2.PositionComp.SetPosition(MyAPIGateway.Session.Camera.Position + (MyAPIGateway.Session.Camera.WorldMatrix.Forward * 2.0), null, false, true);
                    entity2.Components.Remove<MyPhysicsComponentBase>();
                    Sandbox.Game.Entities.MyEntities.Add(entity2, true);
                    Sandbox.Game.Entities.MyEntities.SetEntityName(entity2, true);
                    this.m_transformSys.SetControlledEntity(entity2);
                    this.m_waypoints.Add(entity2);
                    this.UpdateWaypointList();
                    return;
                }
            }
        }

        private void SwitchPageToCutscenes(MyGuiControlButton myGuiControlButton)
        {
            if (m_currentScreen != ScriptingToolsScreen.Cutscenes)
            {
                m_currentScreen = ScriptingToolsScreen.Cutscenes;
                this.RecreateControls(false);
            }
        }

        private void SwitchPageToTransformation(MyGuiControlButton myGuiControlButton)
        {
            if (m_currentScreen != ScriptingToolsScreen.Transformation)
            {
                if (this.m_cutsceneSaveButton.Enabled)
                {
                    StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.UnsavedChanges);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MySpaceTexts.UnsavedChangesQuestion), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            this.SaveCutsceneClicked(this.m_cutsceneSaveButton);
                        }
                        if ((result == MyGuiScreenMessageBox.ResultEnum.YES) || (result == MyGuiScreenMessageBox.ResultEnum.NO))
                        {
                            this.SwitchPageToTransformationInternal();
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                else
                {
                    this.SwitchPageToTransformationInternal();
                }
            }
        }

        private void SwitchPageToTransformationInternal()
        {
            m_currentScreen = ScriptingToolsScreen.Transformation;
            Vector3D? position = null;
            MySession.Static.SetCameraController(MyCameraControllerEnum.SpectatorFreeMouse, null, position);
            this.RecreateControls(false);
            this.UpdateWaypointList();
        }

        private void TransformSysOnControlledEntityChanged(VRage.Game.Entity.MyEntity oldEntity, VRage.Game.Entity.MyEntity newEntity)
        {
            if ((m_currentScreen != ScriptingToolsScreen.Cutscenes) && !this.m_disablePicking)
            {
                this.m_helperStringBuilder.Clear();
                if (newEntity != null)
                {
                    this.m_helperStringBuilder.Clear().Append(string.IsNullOrEmpty(newEntity.Name) ? newEntity.DisplayName : newEntity.Name);
                    this.DeselectTrigger();
                    if (!this.m_waypoints.Contains(newEntity))
                    {
                        this.m_waypointsListBox.SelectedItems.Clear();
                    }
                }
                if (this.m_selectedEntityNameBox != null)
                {
                    this.m_selectedEntityNameBox.SetText(this.m_helperStringBuilder);
                }
                this.TransformSysOnRayCasted(this.m_transformSys.LastRay);
            }
        }

        private void TransformSysOnRayCasted(LineD ray)
        {
            if (((this.m_transformSys.ControlledEntity != null) && !this.m_disablePicking) && (m_currentScreen != ScriptingToolsScreen.Cutscenes))
            {
                MyHighlightSystem.MyHighlightData data;
                if (this.m_selectedFunctionalBlock != null)
                {
                    MyHighlightSystem component = MySession.Static.GetComponent<MyHighlightSystem>();
                    if (component != null)
                    {
                        data = new MyHighlightSystem.MyHighlightData {
                            EntityId = this.m_selectedFunctionalBlock.EntityId,
                            PlayerId = -1L,
                            Thickness = -1
                        };
                        component.RequestHighlightChange(data);
                    }
                    this.m_selectedFunctionalBlock = null;
                }
                MyCubeGrid controlledEntity = this.m_transformSys.ControlledEntity as MyCubeGrid;
                if (controlledEntity != null)
                {
                    Vector3I? nullable = controlledEntity.RayCastBlocks(ray.From, ray.To);
                    if (nullable != null)
                    {
                        MySlimBlock cubeBlock = controlledEntity.GetCubeBlock(nullable.Value);
                        if (cubeBlock.FatBlock != null)
                        {
                            this.m_selectedFunctionalBlock = cubeBlock.FatBlock;
                        }
                    }
                }
                this.m_helperStringBuilder.Clear();
                if (this.m_selectedFunctionalBlock != null)
                {
                    this.m_helperStringBuilder.Append(string.IsNullOrEmpty(this.m_selectedFunctionalBlock.Name) ? this.m_selectedFunctionalBlock.DisplayNameText : this.m_selectedFunctionalBlock.Name);
                    MyHighlightSystem component = MySession.Static.GetComponent<MyHighlightSystem>();
                    if (component != null)
                    {
                        data = new MyHighlightSystem.MyHighlightData {
                            EntityId = this.m_selectedFunctionalBlock.EntityId,
                            IgnoreUseObjectData = true,
                            OutlineColor = new Color?(Color.Blue),
                            PulseTimeInFrames = (ulong) 120,
                            Thickness = 3,
                            PlayerId = -1L
                        };
                        component.RequestHighlightChange(data);
                    }
                }
                if (this.m_selectedFunctionalBlockNameBox != null)
                {
                    this.m_selectedFunctionalBlockNameBox.SetText(this.m_helperStringBuilder);
                }
            }
        }

        private void TriggersListBoxOnItemDoubleClicked(MyGuiControlListbox listBox)
        {
            if (this.m_triggersListBox.SelectedItems.Count != 0)
            {
                MyAreaTriggerComponent userData = (MyAreaTriggerComponent) this.m_triggersListBox.SelectedItems[0].UserData;
                this.m_triggerManipulator.SelectedTrigger = userData;
                if (this.m_triggerManipulator.SelectedTrigger != null)
                {
                    MyAreaTriggerComponent selectedTrigger = (MyAreaTriggerComponent) this.m_triggerManipulator.SelectedTrigger;
                    this.m_helperStringBuilder.Clear();
                    this.m_helperStringBuilder.Append(selectedTrigger.Name);
                    this.m_selectedTriggerNameBox.SetText(this.m_helperStringBuilder);
                }
                this.DeselectEntity();
            }
        }

        public override bool Update(bool hasFocus)
        {
            IEnumerator<MyVSStateMachine> enumerator;
            if (m_currentScreen == ScriptingToolsScreen.Cutscenes)
            {
                this.UpdateCutscenes();
                return base.Update(hasFocus);
            }
            if ((MyCubeBuilder.Static.CubeBuilderState.CurrentBlockDefinition != null) || MyInput.Static.IsRightMousePressed())
            {
                base.DrawMouseCursor = false;
            }
            else
            {
                base.DrawMouseCursor = true;
            }
            this.m_triggerManipulator.CurrentPosition = MyAPIGateway.Session.Camera.Position;
            this.UpdateTriggerList();
            for (int i = 0; i < this.m_scriptManager.FailedLevelScriptExceptionTexts.Length; i++)
            {
                string toolTip = this.m_scriptManager.FailedLevelScriptExceptionTexts[i];
                if ((toolTip != null) && ((bool) this.m_levelScriptListBox.Items[i].UserData))
                {
                    this.m_levelScriptListBox.Items[i].Text.Append(" - failed");
                    this.m_levelScriptListBox.Items[i].FontOverride = "Red";
                    this.m_levelScriptListBox.Items[i].ToolTip.AddToolTip(toolTip, 0.7f, "Red");
                }
            }
            using (enumerator = this.m_scriptManager.SMManager.RunningMachines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyVSStateMachine stateMachine;
                    int num2 = this.m_smListBox.Items.FindIndex(item => ReferenceEquals((MyVSStateMachine) item.UserData, stateMachine));
                    if (num2 == -1)
                    {
                        object userData = stateMachine;
                        int? position = null;
                        this.m_smListBox.Add(new MyGuiControlListbox.Item(new StringBuilder(stateMachine.Name), MyTexts.Get(MyCommonTexts.Scripting_Tooltip_Cursors).ToString(), null, userData, null), position);
                        num2 = this.m_smListBox.Items.Count - 1;
                    }
                    MyGuiControlListbox.Item item = this.m_smListBox.Items[num2];
                    int index = item.ToolTip.ToolTips.Count - 1;
                    while (true)
                    {
                        if (index < 0)
                        {
                            foreach (MyStateMachineCursor cursor2 in stateMachine.ActiveCursors)
                            {
                                bool flag2 = false;
                                int num4 = item.ToolTip.ToolTips.Count - 1;
                                while (true)
                                {
                                    if (num4 >= 0)
                                    {
                                        if (item.ToolTip.ToolTips[num4].Text.CompareTo(cursor2.Node.Name) != 0)
                                        {
                                            num4--;
                                            continue;
                                        }
                                        flag2 = true;
                                    }
                                    if (!flag2)
                                    {
                                        item.ToolTip.AddToolTip(cursor2.Node.Name, 0.7f, "Blue");
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                        MyColoredText text = item.ToolTip.ToolTips[index];
                        bool flag = false;
                        foreach (MyStateMachineCursor cursor in stateMachine.ActiveCursors)
                        {
                            if (text.Text.CompareTo(cursor.Node.Name) == 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag && (index != 0))
                        {
                            item.ToolTip.ToolTips.RemoveAtFast<MyColoredText>(index);
                        }
                        index--;
                    }
                }
            }
            if (1 != 0)
            {
                IMyCamera camera = ((IMySession) MySession.Static).Camera;
                Vector2 start = new Vector2(camera.ViewportSize.X * 0.01f, camera.ViewportSize.Y * 0.2f);
                Vector2 offset = new Vector2(0f, camera.ViewportSize.Y * 0.015f);
                Vector2 vector1 = new Vector2(camera.ViewportSize.X * 0.05f, 0f);
                float scale = 0.65f * Math.Min((float) (camera.ViewportSize.X / 1920f), (float) (camera.ViewportSize.Y / 1200f));
                int initialShift = InitialShift;
                foreach (IMyLevelScript script in this.m_scriptManager.LevelScripts)
                {
                    MyRenderProxy.DebugDrawText2D(start + (initialShift * offset), $"Script : {script.GetType().Name}", Color.Orange, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    initialShift++;
                    foreach (FieldInfo info in script.GetType().GetFields())
                    {
                        MyRenderProxy.DebugDrawText2D(start + (initialShift * offset), $"   {info.Name} :     {info.GetValue(script)}", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        initialShift++;
                    }
                }
                initialShift++;
                using (enumerator = this.m_scriptManager.SMManager.RunningMachines.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        DictionaryReader<string, MyStateMachineNode> allNodes = enumerator.Current.AllNodes;
                        IEnumerator<MyStateMachineNode> enumerator4 = allNodes.Values.GetEnumerator();
                        try
                        {
                            while (enumerator4.MoveNext())
                            {
                                MyVSStateMachineNode current = enumerator4.Current as MyVSStateMachineNode;
                                if ((current != null) && (current.ScriptInstance != null))
                                {
                                    MyRenderProxy.DebugDrawText2D(start + (initialShift * offset), $"Script : {current.Name}", Color.Orange, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                    initialShift++;
                                    foreach (FieldInfo info2 in current.ScriptInstance.GetType().GetFields())
                                    {
                                        MyRenderProxy.DebugDrawText2D(start + (initialShift * offset), $"   {info2.Name} :     {info2.GetValue(current.ScriptInstance)}", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                        initialShift++;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (enumerator4 == null)
                            {
                                continue;
                            }
                            enumerator4.Dispose();
                        }
                    }
                }
                initialShift++;
                MyRenderProxy.DebugDrawText2D(start + (initialShift * offset), string.Format("Stored variables:", Array.Empty<object>()), Color.Orange, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                initialShift = this.DrawDictionary<SerializableVector3D>(this.m_scriptStorage.GetVector3D(), "Vectors:", start, offset, scale, this.DrawDictionary<float>(this.m_scriptStorage.GetFloats(), "Floats:", start, offset, scale, this.DrawDictionary<string>(this.m_scriptStorage.GetStrings(), "Strings:", start, offset, scale, this.DrawDictionary<long>(this.m_scriptStorage.GetLongs(), "Longs:", start, offset, scale, this.DrawDictionary<int>(this.m_scriptStorage.GetInts(), "Ints:", start, offset, scale, this.DrawDictionary<bool>(this.m_scriptStorage.GetBools(), "Bools:", start, offset, scale, initialShift + 1))))));
            }
            return base.Update(hasFocus);
        }

        private void UpdateCutsceneFields()
        {
            string name = (this.m_cutsceneSelection.GetSelectedIndex() >= 0) ? this.m_cutsceneSelection.GetSelectedValue().ToString() : "";
            this.m_cutsceneCurrent = null;
            Cutscene cutsceneCopy = MySession.Static.GetComponent<MySessionComponentCutscenes>().GetCutsceneCopy(name);
            bool flag = cutsceneCopy != null;
            this.m_cutsceneDeleteButton.Enabled = flag;
            this.m_cutscenePlayButton.Enabled = flag;
            this.m_cutsceneSaveButton.Enabled = false;
            this.m_cutsceneRevertButton.Enabled = false;
            this.m_cutscenePropertyNextCutscene.Enabled = flag;
            this.m_cutscenePropertyNextCutscene.ClearItems();
            int? sortOrder = null;
            this.m_cutscenePropertyNextCutscene.AddItem(0L, MyTexts.Get(MyCommonTexts.Cutscene_Tooltip_None), sortOrder, null);
            this.m_cutscenePropertyNextCutscene.SelectItemByIndex(0);
            this.m_cutscenePropertyStartEntity.Enabled = flag;
            this.m_cutscenePropertyStartLookAt.Enabled = flag;
            this.m_cutscenePropertyStartingFOV.Enabled = flag;
            this.m_cutscenePropertyCanBeSkipped.Enabled = flag;
            this.m_cutscenePropertyFireEventsDuringSkip.Enabled = flag;
            this.m_cutsceneNodes.ClearItems();
            if (flag)
            {
                this.m_cutscenePropertyStartEntity.Text = cutsceneCopy.StartEntity;
                this.m_cutscenePropertyStartLookAt.Text = cutsceneCopy.StartLookAt;
                this.m_cutscenePropertyStartingFOV.Text = cutsceneCopy.StartingFOV.ToString();
                this.m_cutscenePropertyCanBeSkipped.IsChecked = cutsceneCopy.CanBeSkipped;
                this.m_cutscenePropertyFireEventsDuringSkip.IsChecked = cutsceneCopy.FireEventsDuringSkip;
                foreach (string str2 in this.m_cutscenes.Keys)
                {
                    if (str2.Equals(cutsceneCopy.Name))
                    {
                        continue;
                    }
                    sortOrder = null;
                    this.m_cutscenePropertyNextCutscene.AddItem(str2.GetHashCode64(), str2, sortOrder, null);
                    if (str2.Equals(cutsceneCopy.NextCutscene))
                    {
                        this.m_cutscenePropertyNextCutscene.SelectItemByKey(str2.GetHashCode64(), true);
                    }
                }
                if (cutsceneCopy.SequenceNodes != null)
                {
                    for (int i = 0; i < cutsceneCopy.SequenceNodes.Count; i++)
                    {
                        int num2 = i + 1;
                        sortOrder = null;
                        this.m_cutsceneNodes.Add(new MyGuiControlListbox.Item(new StringBuilder(num2.ToString() + ": " + cutsceneCopy.SequenceNodes[i].GetNodeSummary()), cutsceneCopy.SequenceNodes[i].GetNodeDescription(), null, null, null), sortOrder);
                    }
                }
            }
            this.m_cutsceneCurrent = cutsceneCopy;
            this.UpdateCutsceneNodeFields();
        }

        private void UpdateCutsceneNodeFields()
        {
            bool flag = (this.m_cutsceneCurrent != null) && (this.m_cutsceneNodes.SelectedItems.Count > 0);
            this.m_cutsceneNodeButtonMoveUp.Enabled = flag;
            this.m_cutsceneNodeButtonMoveDown.Enabled = flag;
            this.m_cutsceneNodeButtonDelete.Enabled = flag;
            this.m_cutsceneNodePropertyTime.Enabled = flag;
            this.m_cutsceneNodePropertyMoveTo.Enabled = flag;
            this.m_cutsceneNodePropertyMoveToInstant.Enabled = flag;
            this.m_cutsceneNodePropertyRotateLike.Enabled = flag;
            this.m_cutsceneNodePropertyRotateLikeInstant.Enabled = flag;
            this.m_cutsceneNodePropertyRotateTowards.Enabled = flag;
            this.m_cutsceneNodePropertyRotateTowardsInstant.Enabled = flag;
            this.m_cutsceneNodePropertyEvent.Enabled = flag;
            this.m_cutsceneNodePropertyEventDelay.Enabled = flag;
            this.m_cutsceneNodePropertyFOVChange.Enabled = flag;
            this.m_cutsceneNodePropertyRotateTowardsLock.Enabled = flag;
            this.m_cutsceneNodePropertyAttachAll.Enabled = flag;
            this.m_cutsceneNodePropertyAttachPosition.Enabled = flag;
            this.m_cutsceneNodePropertyAttachRotation.Enabled = flag;
            this.m_cutsceneNodePropertyWaypoints.Enabled = flag;
            if (flag)
            {
                this.m_selectedCutsceneNodeIndex = this.GetListboxSelectedIndex(this.m_cutsceneNodes);
                this.m_cutsceneNodeButtonMoveUp.Enabled = (this.m_selectedCutsceneNodeIndex > 0) && (this.m_cutsceneNodes.Items.Count > 1);
                this.m_cutsceneNodeButtonMoveDown.Enabled = this.m_selectedCutsceneNodeIndex < (this.m_cutsceneNodes.Items.Count - 1);
                this.m_cutsceneNodePropertyTime.Text = Math.Max(this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Time, 0f).ToString();
                this.m_cutsceneNodePropertyMoveTo.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].MoveTo != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].MoveTo : "";
                this.m_cutsceneNodePropertyMoveToInstant.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].SetPositionTo != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].SetPositionTo : "";
                this.m_cutsceneNodePropertyRotateLike.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].RotateLike != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].RotateLike : "";
                this.m_cutsceneNodePropertyRotateLikeInstant.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].SetRorationLike != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].SetRorationLike : "";
                this.m_cutsceneNodePropertyRotateTowards.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].RotateTowards != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].RotateTowards : "";
                this.m_cutsceneNodePropertyRotateTowardsInstant.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LookAt != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LookAt : "";
                this.m_cutsceneNodePropertyEvent.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Event != null) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Event : "";
                this.m_cutsceneNodePropertyEventDelay.Text = Math.Max(this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].EventDelay, 0f).ToString();
                this.m_cutsceneNodePropertyFOVChange.Text = Math.Max(this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].ChangeFOVTo, 0f).ToString();
                this.m_cutsceneNodePropertyRotateTowardsLock.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LockRotationTo == null) ? "" : ((this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LockRotationTo.Length > 0) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].LockRotationTo : "X");
                this.m_cutsceneNodePropertyAttachAll.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachTo == null) ? "" : ((this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachTo.Length > 0) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachTo : "X");
                this.m_cutsceneNodePropertyAttachPosition.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachPositionTo == null) ? "" : ((this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachPositionTo.Length > 0) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachPositionTo : "X");
                this.m_cutsceneNodePropertyAttachRotation.Text = (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachRotationTo == null) ? "" : ((this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachRotationTo.Length > 0) ? this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].AttachRotationTo : "X");
                if (this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints != null)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(";");
                        }
                        builder.Append(this.m_cutsceneCurrent.SequenceNodes[this.m_selectedCutsceneNodeIndex].Waypoints[i].Name);
                    }
                    this.m_cutsceneNodePropertyWaypoints.Text = builder.ToString();
                }
                else
                {
                    this.m_cutsceneNodePropertyWaypoints.Text = "";
                }
            }
        }

        private void UpdateCutscenes()
        {
            MyGuiScreenGamePlay.DisableInput = (base.State != MyGuiScreenState.CLOSING) && (base.State != MyGuiScreenState.CLOSED);
            if (this.m_cutscenePlaying && !MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning)
            {
                base.State = MyGuiScreenState.OPENED;
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.SpectatorFreeMouse, null, position);
                this.m_cutscenePlaying = false;
            }
        }

        private void UpdateTriggerList()
        {
            ObservableCollection<MyGuiControlListbox.Item> items = this.m_triggersListBox.Items;
            List<MyTriggerComponent> allTriggers = MySessionComponentTriggerSystem.Static.GetAllTriggers();
            for (int i = 0; i < items.Count; i++)
            {
                MyAreaTriggerComponent userData = (MyAreaTriggerComponent) items[i].UserData;
                if (!allTriggers.Contains(userData))
                {
                    items.RemoveAtFast<MyGuiControlListbox.Item>(i);
                }
            }
            using (List<MyTriggerComponent>.Enumerator enumerator = allTriggers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyTriggerComponent trigger;
                    if (this.m_triggersListBox.Items.FindIndex(item => ReferenceEquals((MyTriggerComponent) item.UserData, trigger)) < 0)
                    {
                        MyGuiControlListbox.Item item = this.CreateTriggerListItem(trigger);
                        if (item != null)
                        {
                            int? position = null;
                            this.m_triggersListBox.Add(item, position);
                        }
                    }
                }
            }
        }

        private void UpdateWaypointList()
        {
            if (this.m_waypointsListBox != null)
            {
                ObservableCollection<MyGuiControlListbox.Item> items = this.m_waypointsListBox.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    VRage.Game.Entity.MyEntity userData = (VRage.Game.Entity.MyEntity) items[i].UserData;
                    if (!this.m_waypoints.Contains(userData))
                    {
                        items.RemoveAtFast<MyGuiControlListbox.Item>(i);
                    }
                }
                using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = this.m_waypoints.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        VRage.Game.Entity.MyEntity wp;
                        if (this.m_waypointsListBox.Items.FindIndex(item => ReferenceEquals((VRage.Game.Entity.MyEntity) item.UserData, wp)) < 0)
                        {
                            VRage.Game.Entity.MyEntity userData = wp;
                            StringBuilder text = new StringBuilder("Waypoint: ");
                            text.Append(userData.Name);
                            int? position = null;
                            this.m_waypointsListBox.Add(new MyGuiControlListbox.Item(text, userData.Name, null, userData, null), position);
                        }
                    }
                }
            }
        }

        private void WatchCutsceneClicked(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_cutsceneSelection.GetSelectedValue() != null)
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = false;
                MySession.Static.GetComponent<MySessionComponentCutscenes>().PlayCutscene(this.m_cutsceneCurrent, false, "");
                base.State = MyGuiScreenState.HIDDEN;
                this.m_cutscenePlaying = true;
            }
        }

        private void WaypointsListBoxOnItemDoubleClicked(MyGuiControlListbox listBox)
        {
            if (this.m_waypointsListBox.SelectedItems.Count != 0)
            {
                VRage.Game.Entity.MyEntity userData = (VRage.Game.Entity.MyEntity) this.m_waypointsListBox.SelectedItems[0].UserData;
                this.m_transformSys.SetControlledEntity(userData);
                this.DeselectTrigger();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenScriptingTools.<>c <>9 = new MyGuiScreenScriptingTools.<>c();
            public static Predicate<MyTriggerComponent> <>9__30_0;

            internal bool <.ctor>b__30_0(MyTriggerComponent trigger) => 
                (trigger is MyAreaTriggerComponent);
        }

        private enum ScriptingToolsScreen
        {
            Transformation,
            Cutscenes
        }
    }
}

