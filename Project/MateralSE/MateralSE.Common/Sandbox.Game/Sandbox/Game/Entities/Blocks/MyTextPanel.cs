namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_TextPanel)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyTextPanel), typeof(Sandbox.ModAPI.Ingame.IMyTextPanel) })]
    public class MyTextPanel : MyFunctionalBlock, IMyTextPanelComponentOwner, Sandbox.ModAPI.IMyTextPanel, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.IMyTextSurface, Sandbox.ModAPI.Ingame.IMyTextSurface, Sandbox.ModAPI.Ingame.IMyTextPanel, Sandbox.ModAPI.IMyTextSurfaceProvider, Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider
    {
        public const double MAX_DRAW_DISTANCE = 200.0;
        private readonly StringBuilder m_publicDescription = new StringBuilder();
        private readonly StringBuilder m_publicTitle = new StringBuilder();
        private readonly StringBuilder m_privateDescription = new StringBuilder();
        private readonly StringBuilder m_privateTitle = new StringBuilder();
        private bool m_isTextPanelOpen;
        private ulong m_userId;
        private MyGuiScreenTextPanel m_textBox;
        private int m_previousUpdateTime;
        private bool m_isOutofRange;
        private MyTextPanelComponent m_panelComponent;
        private bool m_isEditingPublic;
        private StringBuilder m_publicTitleHelper = new StringBuilder();
        private StringBuilder m_privateTitleHelper = new StringBuilder();
        private StringBuilder m_publicDescriptionHelper = new StringBuilder();
        private StringBuilder m_privateDescriptionHelper = new StringBuilder();

        public MyTextPanel()
        {
            this.CreateTerminalControls();
            this.m_isTextPanelOpen = false;
            this.m_privateDescription = new StringBuilder();
            this.m_privateTitle = new StringBuilder();
            this.Render = new MyRenderComponentTextPanel(this);
            this.Render.NeedsDraw = false;
            base.NeedsWorldMatrix = true;
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId));

        private void CloseWindow(bool isPublic)
        {
            MyGuiScreenGamePlay.ActiveGameplayScreen = MyGuiScreenGamePlay.TmpGameplayScreenHolder;
            MyGuiScreenGamePlay.TmpGameplayScreenHolder = null;
            MySession.Static.Gpss.ScanText(this.m_textBox.Description.Text.ToString(), this.PublicTitle);
            foreach (MySlimBlock block in base.CubeGrid.CubeBlocks)
            {
                if ((block.FatBlock != null) && (block.FatBlock.EntityId == base.EntityId))
                {
                    this.SendChangeDescriptionMessage(this.m_textBox.Description.Text, isPublic);
                    this.SendChangeOpenMessage(false, false, 0UL, false);
                    break;
                }
            }
        }

        protected override void Closing()
        {
            base.Closing();
            if (Sync.IsServer && (Sync.Clients != null))
            {
                MyClientCollection clients = Sync.Clients;
                clients.ClientRemoved = (Action<ulong>) Delegate.Remove(clients.ClientRemoved, new Action<ulong>(this.TextPanel_ClientRemoved));
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyTextPanel>())
            {
                base.CreateTerminalControls();
                MyTerminalControlTextbox<MyTextPanel> textbox1 = new MyTerminalControlTextbox<MyTextPanel>("Title", MySpaceTexts.BlockPropertyTitle_TextPanelPublicTitle, MySpaceTexts.Blank);
                MyTerminalControlTextbox<MyTextPanel> textbox2 = new MyTerminalControlTextbox<MyTextPanel>("Title", MySpaceTexts.BlockPropertyTitle_TextPanelPublicTitle, MySpaceTexts.Blank);
                textbox2.Getter = x => x.PublicTitle;
                MyTerminalControlTextbox<MyTextPanel> local4 = textbox2;
                MyTerminalControlTextbox<MyTextPanel> local5 = textbox2;
                local5.Setter = (x, v) => x.SendChangeTitleMessage(v, true);
                MyTerminalControlTextbox<MyTextPanel> control = local5;
                control.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyTextPanel>(control);
                MyTerminalControlFactory.AddControl<MyTextPanel>(new MyTerminalControlSeparator<MyTextPanel>());
                MyTextPanelComponent.CreateTerminalControls<MyTextPanel>();
            }
        }

        private void CreateTextBox(bool isEditable, StringBuilder description, bool isPublic)
        {
            bool editable = isEditable;
            this.m_textBox = new MyGuiScreenTextPanel(isPublic ? this.m_publicTitle.ToString() : this.m_privateTitle.ToString(), "", "", description.ToString(), new Action<VRage.Game.ModAPI.ResultEnum>(this.OnClosedPanelTextBox), null, null, editable, null);
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_TextPanel objectBuilderCubeBlock = (MyObjectBuilder_TextPanel) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Description = this.m_privateDescription.ToString();
            objectBuilderCubeBlock.Title = this.m_privateTitle.ToString();
            objectBuilderCubeBlock.PublicDescription = this.m_publicDescription.ToString();
            objectBuilderCubeBlock.PublicTitle = this.m_publicTitle.ToString();
            objectBuilderCubeBlock.ChangeInterval = this.ChangeInterval;
            objectBuilderCubeBlock.Font = (SerializableDefinitionId) this.PanelComponent.Font;
            objectBuilderCubeBlock.FontSize = this.FontSize;
            objectBuilderCubeBlock.FontColor = this.FontColor;
            objectBuilderCubeBlock.BackgroundColor = this.BackgroundColor;
            objectBuilderCubeBlock.CurrentShownTexture = this.PanelComponent.CurrentSelectedTexture;
            objectBuilderCubeBlock.ShowText = ShowTextOnScreenFlag.NONE;
            objectBuilderCubeBlock.Alignment = (TextAlignmentEnum) this.PanelComponent.Alignment;
            objectBuilderCubeBlock.ContentType = (this.PanelComponent.ContentType == VRage.Game.GUI.TextPanel.ContentType.IMAGE) ? VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE : this.PanelComponent.ContentType;
            objectBuilderCubeBlock.SelectedScript = this.PanelComponent.Script;
            objectBuilderCubeBlock.CustomizeScripts = this.PanelComponent.CustomizeScripts;
            objectBuilderCubeBlock.ScriptBackgroundColor = this.PanelComponent.ScriptBackgroundColor;
            objectBuilderCubeBlock.ScriptForegroundColor = this.PanelComponent.ScriptForegroundColor;
            objectBuilderCubeBlock.TextPadding = this.PanelComponent.TextPadding;
            objectBuilderCubeBlock.PreserveAspectRatio = this.PanelComponent.PreserveAspectRatio;
            objectBuilderCubeBlock.Version = 1;
            if (this.PanelComponent.SelectedTexturesToDraw.Count > 0)
            {
                objectBuilderCubeBlock.SelectedImages = new List<string>();
                foreach (MyLCDTextureDefinition definition in this.PanelComponent.SelectedTexturesToDraw)
                {
                    objectBuilderCubeBlock.SelectedImages.Add(definition.Id.SubtypeName);
                }
            }
            return objectBuilderCubeBlock;
        }

        public void GetSprites(List<string> sprites)
        {
            this.PanelComponent.GetSprites(sprites);
        }

        public override unsafe void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyTextPanelComponent.ContentMetadata metadata;
            MyTextPanelComponent.FontData data;
            MyTextPanelComponent.ScriptData data2;
            base.SyncFlag = true;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            base.ResourceSink = component;
            this.m_panelComponent = new MyTextPanelComponent(0, this, "ScreenArea", "ScreenArea", this.BlockDefinition.TextureResolution, this.BlockDefinition.ScreenWidth, this.BlockDefinition.ScreenHeight, true);
            base.SyncType.Append(this.m_panelComponent);
            this.m_panelComponent.Init(null, new Action<MyTextPanelComponent, int[]>(this.SendAddImagesToSelectionRequest), new Action<MyTextPanelComponent, int[]>(this.SendRemoveSelectedImageRequest));
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_TextPanel panel = (MyObjectBuilder_TextPanel) objectBuilder;
            if (panel == null)
            {
                goto TR_0000;
            }
            else
            {
                MyTextPanelComponent.ScriptData* dataPtr1;
                this.PrivateTitle.Append(panel.Title);
                this.PrivateDescription.Append(panel.Description);
                this.PublicDescription.Append(MyStatControlText.SubstituteTexts(panel.PublicDescription, null));
                this.PublicTitle.Append(panel.PublicTitle);
                this.PanelComponent.CurrentSelectedTexture = panel.CurrentShownTexture;
                if (Sync.IsServer && (Sync.Clients != null))
                {
                    MyClientCollection clients = Sync.Clients;
                    clients.ClientRemoved = (Action<ulong>) Delegate.Combine(clients.ClientRemoved, new Action<ulong>(this.TextPanel_ClientRemoved));
                }
                metadata = new MyTextPanelComponent.ContentMetadata {
                    ContentType = panel.ContentType,
                    BackgroundColor = panel.BackgroundColor,
                    ChangeInterval = MathHelper.Clamp(panel.ChangeInterval, 0f, this.BlockDefinition.MaxChangingSpeed),
                    PreserveAspectRatio = panel.PreserveAspectRatio,
                    TextPadding = panel.TextPadding
                };
                data = new MyTextPanelComponent.FontData {
                    Alignment = (TextAlignment) ((byte) panel.Alignment),
                    Size = MathHelper.Clamp(panel.FontSize, this.BlockDefinition.MinFontSize, this.BlockDefinition.MaxFontSize),
                    TextColor = panel.FontColor
                };
                MyTextPanelComponent.ScriptData data4 = new MyTextPanelComponent.ScriptData();
                dataPtr1->Script = panel.SelectedScript ?? string.Empty;
                dataPtr1 = (MyTextPanelComponent.ScriptData*) ref data4;
                data4.CustomizeScript = panel.CustomizeScripts;
                data4.BackgroundColor = panel.ScriptBackgroundColor;
                data4.ForegroundColor = panel.ScriptForegroundColor;
                data2 = data4;
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
                this.Render.NeedsDrawFromParent = true;
                if (!panel.Font.IsNull())
                {
                    data.Name = panel.Font.SubtypeName;
                }
                if (panel.SelectedImages != null)
                {
                    foreach (string str in panel.SelectedImages)
                    {
                        foreach (MyLCDTextureDefinition definition in this.PanelComponent.Definitions)
                        {
                            if (definition.Id.SubtypeName == str)
                            {
                                this.PanelComponent.SelectedTexturesToDraw.Add(definition);
                                break;
                            }
                        }
                    }
                    this.PanelComponent.CurrentSelectedTexture = Math.Min(this.PanelComponent.CurrentSelectedTexture, this.PanelComponent.SelectedTexturesToDraw.Count);
                    base.RaisePropertiesChanged();
                }
                if (panel.Version != 0)
                {
                    goto TR_0001;
                }
                else if ((panel.ContentType == VRage.Game.GUI.TextPanel.ContentType.NONE) && (((panel.SelectedImages != null) && (panel.SelectedImages.Count > 0)) || ((panel.ShowText != ShowTextOnScreenFlag.NONE) || (panel.PublicDescription != string.Empty))))
                {
                    if (panel.ShowText != ShowTextOnScreenFlag.NONE)
                    {
                        this.PanelComponent.SelectedTexturesToDraw.Clear();
                    }
                    else
                    {
                        this.PublicDescription.Clear();
                    }
                    metadata.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    goto TR_0001;
                }
            }
            metadata.ContentType = (panel.ContentType != VRage.Game.GUI.TextPanel.ContentType.IMAGE) ? panel.ContentType : VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            goto TR_0001;
        TR_0000:
            base.ResourceSink.Update();
            base.ResourceSink.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            return;
        TR_0001:
            this.PanelComponent.SetLocalValues(metadata, data, data2);
            goto TR_0000;
        }

        public bool IsInRange()
        {
            MyCamera mainCamera = MySector.MainCamera;
            if (mainCamera == null)
            {
                return false;
            }
            return (Vector3D.Distance((MatrixD.CreateTranslation(base.PositionComp.LocalVolume.Center) * base.WorldMatrix).Translation, mainCamera.Position) < 200.0);
        }

        public Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale) => 
            MyGuiManager.MeasureStringRaw(font, text, scale);

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.ComponentStack_IsFunctionalChanged();
            this.PanelComponent.Reset();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        [Event(null, 0x306), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void OnChangeDescription(string description, bool isPublic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear().Append(description);
            if (isPublic)
            {
                this.PublicDescription = builder;
            }
            else
            {
                this.PrivateDescription = builder;
            }
        }

        private void OnChangeOpen(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            this.IsTextPanelOpen = isOpen;
            this.UserId = user;
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && (user == Sync.MyId)) & isOpen)
            {
                this.OpenWindow(editable, false, isPublic);
            }
        }

        [Event(null, 0x329), Reliable, Server(ValidationType.Access)]
        private void OnChangeOpenRequest(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            if (!((Sync.IsServer && this.IsTextPanelOpen) & isOpen))
            {
                this.OnChangeOpen(isOpen, editable, user, isPublic);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, bool, bool, ulong, bool>(this, x => new Action<bool, bool, ulong, bool>(x.OnChangeOpenSuccess), isOpen, editable, user, isPublic, targetEndpoint);
            }
        }

        [Event(null, 820), Reliable, Broadcast]
        private void OnChangeOpenSuccess(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            this.OnChangeOpen(isOpen, editable, user, isPublic);
        }

        [Event(null, 0x315), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnChangeTitle(string title, bool isPublic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear().Append(title);
            if (isPublic)
            {
                this.PublicTitle = builder;
            }
            else
            {
                this.PrivateTitle = builder;
            }
        }

        public void OnClosedPanelMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.m_textBox.Description.Text.Remove(0x186a0, this.m_textBox.Description.Text.Length - 0x186a0);
                this.CloseWindow(this.m_isEditingPublic);
            }
            else
            {
                this.CreateTextBox(true, this.m_textBox.Description.Text, this.m_isEditingPublic);
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        public void OnClosedPanelTextBox(VRage.Game.ModAPI.ResultEnum result)
        {
            if (this.m_textBox != null)
            {
                if (this.m_textBox.Description.Text.Length <= 0x186a0)
                {
                    this.CloseWindow(this.m_isEditingPublic);
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextTooLongText), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnClosedPanelMessageBox), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        private void OnEnemyUse(UseActionEnum actionEnum, MyCharacter user)
        {
            if (actionEnum == UseActionEnum.Manipulate)
            {
                this.OpenWindow(false, true, true);
            }
            else if (actionEnum == UseActionEnum.OpenTerminal)
            {
                MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
            }
        }

        private void OnFactionUse(UseActionEnum actionEnum, MyCharacter user)
        {
            bool flag = false;
            if (actionEnum == UseActionEnum.Manipulate)
            {
                if (base.GetUserRelationToOwner(user.GetPlayerIdentityId()) == MyRelationsBetweenPlayerAndBlock.FactionShare)
                {
                    this.OpenWindow(true, true, true);
                }
                else
                {
                    this.OpenWindow(false, true, true);
                }
            }
            else if (actionEnum == UseActionEnum.OpenTerminal)
            {
                if (base.GetUserRelationToOwner(user.GetPlayerIdentityId()) == MyRelationsBetweenPlayerAndBlock.FactionShare)
                {
                    MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, user, this);
                }
                else
                {
                    flag = true;
                }
            }
            if (ReferenceEquals(user.ControllerInfo.Controller.Player, MySession.Static.LocalHumanPlayer) && flag)
            {
                MyHud.Notifications.Add(MyNotificationSingletons.TextPanelReadOnly);
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            if (this.m_panelComponent != null)
            {
                this.m_panelComponent.Reset();
            }
            if (base.ResourceSink != null)
            {
                this.UpdateScreen();
            }
            if (this.CheckIsWorking() && this.ShowTextOnScreen)
            {
                this.Render.UpdateModelProperties();
            }
        }

        private void OnOwnerUse(UseActionEnum actionEnum, MyCharacter user)
        {
            if (actionEnum == UseActionEnum.Manipulate)
            {
                this.OpenWindow(true, true, true);
            }
            else if (actionEnum == UseActionEnum.OpenTerminal)
            {
                MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, user, this);
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            if (this.PanelComponent != null)
            {
                this.PanelComponent.SetRender(null);
            }
        }

        [Event(null, 0x2f5), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnRemoveSelectedImageRequest(int[] selection)
        {
            this.PanelComponent.RemoveItems(selection);
        }

        [Event(null, 0x300), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnSelectImageRequest(int[] selection)
        {
            this.PanelComponent.SelectItems(selection);
        }

        public void OpenWindow(bool isEditable, bool sync, bool isPublic)
        {
            if (sync)
            {
                this.SendChangeOpenMessage(true, isEditable, Sync.MyId, isPublic);
            }
            else
            {
                this.m_isEditingPublic = isPublic;
                this.CreateTextBox(isEditable, isPublic ? this.PublicDescription : this.PrivateDescription, isPublic);
                MyGuiScreenGamePlay.TmpGameplayScreenHolder = MyGuiScreenGamePlay.ActiveGameplayScreen;
                MyGuiScreenGamePlay.ActiveGameplayScreen = this.m_textBox;
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            this.UpdateText();
            base.UpdateIsWorking();
            if (this.Render != null)
            {
                this.UpdateScreen();
            }
        }

        string Sandbox.ModAPI.Ingame.IMyTextPanel.GetPrivateText() => 
            this.m_privateDescription.ToString();

        string Sandbox.ModAPI.Ingame.IMyTextPanel.GetPrivateTitle() => 
            this.m_privateTitle.ToString();

        string Sandbox.ModAPI.Ingame.IMyTextPanel.GetPublicText() => 
            ((Sandbox.ModAPI.Ingame.IMyTextSurface) this).GetText();

        string Sandbox.ModAPI.Ingame.IMyTextPanel.GetPublicTitle() => 
            this.m_publicTitleHelper.ToString();

        void Sandbox.ModAPI.Ingame.IMyTextPanel.ReadPublicText(StringBuilder buffer, bool append)
        {
            ((Sandbox.ModAPI.Ingame.IMyTextSurface) this).ReadText(buffer, append);
        }

        void Sandbox.ModAPI.Ingame.IMyTextPanel.SetShowOnScreen(ShowTextOnScreenFlag set)
        {
            this.ShowTextFlag = set;
        }

        void Sandbox.ModAPI.Ingame.IMyTextPanel.ShowPrivateTextOnScreen()
        {
            this.ShowTextFlag = ShowTextOnScreenFlag.NONE | ShowTextOnScreenFlag.PRIVATE;
        }

        void Sandbox.ModAPI.Ingame.IMyTextPanel.ShowPublicTextOnScreen()
        {
            this.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
        }

        void Sandbox.ModAPI.Ingame.IMyTextPanel.ShowTextureOnScreen()
        {
            this.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
        }

        bool Sandbox.ModAPI.Ingame.IMyTextPanel.WritePrivateText(string value, bool append)
        {
            if (this.m_isTextPanelOpen)
            {
                return false;
            }
            if (!append)
            {
                this.m_privateDescriptionHelper.Clear();
            }
            this.m_privateDescriptionHelper.Append(value);
            this.SendChangeDescriptionMessage(this.m_privateDescriptionHelper, false);
            return true;
        }

        bool Sandbox.ModAPI.Ingame.IMyTextPanel.WritePrivateTitle(string value, bool append)
        {
            if (this.m_isTextPanelOpen)
            {
                return false;
            }
            if (!append)
            {
                this.m_privateTitleHelper.Clear();
            }
            this.m_privateTitleHelper.Append(value);
            this.SendChangeTitleMessage(this.m_privateTitleHelper, false);
            return true;
        }

        bool Sandbox.ModAPI.Ingame.IMyTextPanel.WritePublicText(string value, bool append) => 
            ((Sandbox.ModAPI.Ingame.IMyTextSurface) this).WriteText(value, append);

        bool Sandbox.ModAPI.Ingame.IMyTextPanel.WritePublicText(StringBuilder value, bool append) => 
            ((Sandbox.ModAPI.Ingame.IMyTextSurface) this).WriteText(value, append);

        bool Sandbox.ModAPI.Ingame.IMyTextPanel.WritePublicTitle(string value, bool append)
        {
            if (this.m_isTextPanelOpen)
            {
                return false;
            }
            if (!append)
            {
                this.m_publicTitleHelper.Clear();
            }
            this.m_publicTitleHelper.Append(value);
            this.SendChangeTitleMessage(this.m_publicTitleHelper, true);
            return true;
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.AddImagesToSelection(List<string> ids, bool checkExistence)
        {
            if (ids != null)
            {
                List<int> list = new List<int>();
                foreach (string str in ids)
                {
                    for (int i = 0; i < this.PanelComponent.Definitions.Count; i++)
                    {
                        if (this.PanelComponent.Definitions[i].Id.SubtypeName == str)
                        {
                            bool flag = false;
                            if (checkExistence)
                            {
                                for (int j = 0; j < this.PanelComponent.SelectedTexturesToDraw.Count; j++)
                                {
                                    if (this.PanelComponent.SelectedTexturesToDraw[j].Id.SubtypeName == str)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                list.Add(i);
                            }
                            break;
                        }
                    }
                }
                if (list.Count > 0)
                {
                    this.SendAddImagesToSelectionRequest(this, list.ToArray());
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.AddImageToSelection(string id, bool checkExistence)
        {
            if (id != null)
            {
                for (int i = 0; i < this.PanelComponent.Definitions.Count; i++)
                {
                    if (this.PanelComponent.Definitions[i].Id.SubtypeName == id)
                    {
                        if (checkExistence)
                        {
                            for (int j = 0; j < this.PanelComponent.SelectedTexturesToDraw.Count; j++)
                            {
                                if (this.PanelComponent.SelectedTexturesToDraw[j].Id.SubtypeName == id)
                                {
                                    return;
                                }
                            }
                        }
                        int[] selection = new int[] { i };
                        this.SendAddImagesToSelectionRequest(this, selection);
                        return;
                    }
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.ClearImagesFromSelection()
        {
            if (this.PanelComponent.SelectedTexturesToDraw.Count != 0)
            {
                List<int> list = new List<int>();
                int num = 0;
                while (num < this.PanelComponent.SelectedTexturesToDraw.Count)
                {
                    int item = 0;
                    while (true)
                    {
                        if (item < this.PanelComponent.Definitions.Count)
                        {
                            if (this.PanelComponent.Definitions[item].Id.SubtypeName != this.PanelComponent.SelectedTexturesToDraw[num].Id.SubtypeName)
                            {
                                item++;
                                continue;
                            }
                            list.Add(item);
                        }
                        num++;
                        break;
                    }
                }
                this.SendRemoveSelectedImageRequest(this, list.ToArray());
            }
        }

        MySpriteDrawFrame Sandbox.ModAPI.Ingame.IMyTextSurface.DrawFrame() => 
            ((this.m_panelComponent == null) ? new MySpriteDrawFrame(null) : this.m_panelComponent.DrawFrame());

        void Sandbox.ModAPI.Ingame.IMyTextSurface.GetFonts(List<string> fonts)
        {
            if (fonts != null)
            {
                foreach (MyFontDefinition definition in MyDefinitionManager.Static.GetDefinitions<MyFontDefinition>())
                {
                    fonts.Add(definition.Id.SubtypeName);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.GetScripts(List<string> scripts)
        {
            if (this.m_panelComponent != null)
            {
                this.m_panelComponent.GetScripts(scripts);
            }
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.GetSelectedImages(List<string> output)
        {
            foreach (MyLCDTextureDefinition definition in this.PanelComponent.SelectedTexturesToDraw)
            {
                output.Add(definition.Id.SubtypeName);
            }
        }

        string Sandbox.ModAPI.Ingame.IMyTextSurface.GetText() => 
            this.m_publicDescription.ToString();

        void Sandbox.ModAPI.Ingame.IMyTextSurface.ReadText(StringBuilder buffer, bool append)
        {
            if (!append)
            {
                buffer.Clear();
            }
            buffer.AppendStringBuilder(this.m_publicDescription);
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.RemoveImageFromSelection(string id, bool removeDuplicates)
        {
            if (id != null)
            {
                List<int> list = new List<int>();
                int item = 0;
                while (true)
                {
                    if (item < this.PanelComponent.Definitions.Count)
                    {
                        if (this.PanelComponent.Definitions[item].Id.SubtypeName != id)
                        {
                            item++;
                            continue;
                        }
                        if (!removeDuplicates)
                        {
                            list.Add(item);
                        }
                        else
                        {
                            for (int i = 0; i < this.PanelComponent.SelectedTexturesToDraw.Count; i++)
                            {
                                if (this.PanelComponent.SelectedTexturesToDraw[i].Id.SubtypeName == id)
                                {
                                    list.Add(item);
                                }
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        this.SendRemoveSelectedImageRequest(this, list.ToArray());
                    }
                    return;
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyTextSurface.RemoveImagesFromSelection(List<string> ids, bool removeDuplicates)
        {
            if (ids != null)
            {
                List<int> list = new List<int>();
                foreach (string str in ids)
                {
                    for (int i = 0; i < this.PanelComponent.Definitions.Count; i++)
                    {
                        if (this.PanelComponent.Definitions[i].Id.SubtypeName == str)
                        {
                            if (!removeDuplicates)
                            {
                                list.Add(i);
                            }
                            else
                            {
                                for (int j = 0; j < this.PanelComponent.SelectedTexturesToDraw.Count; j++)
                                {
                                    if (this.PanelComponent.SelectedTexturesToDraw[j].Id.SubtypeName == str)
                                    {
                                        list.Add(i);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                if (list.Count > 0)
                {
                    this.SendRemoveSelectedImageRequest(this, list.ToArray());
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyTextSurface.WriteText(string value, bool append)
        {
            if (this.m_isTextPanelOpen)
            {
                return false;
            }
            if (!append)
            {
                this.m_publicDescriptionHelper.Clear();
            }
            if ((value.Length + this.m_publicDescriptionHelper.Length) > 0x186a0)
            {
                value = value.Remove(0x186a0 - this.m_publicDescriptionHelper.Length);
            }
            this.m_publicDescriptionHelper.Append(value);
            this.SendChangeDescriptionMessage(this.m_publicDescriptionHelper, true);
            return true;
        }

        bool Sandbox.ModAPI.Ingame.IMyTextSurface.WriteText(StringBuilder value, bool append)
        {
            if (this.m_isTextPanelOpen)
            {
                return false;
            }
            if (!append)
            {
                this.m_publicDescriptionHelper.Clear();
            }
            this.m_publicDescriptionHelper.Append(value);
            this.SendChangeDescriptionMessage(this.m_publicDescriptionHelper, true);
            return true;
        }

        Sandbox.ModAPI.Ingame.IMyTextSurface Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider.GetSurface(int index) => 
            ((index == 0) ? this.m_panelComponent : null);

        private void SendAddImagesToSelectionRequest(Sandbox.ModAPI.IMyTextSurface panel, int[] selection)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, int[]>(this, x => new Action<int[]>(x.OnSelectImageRequest), selection, targetEndpoint);
        }

        private void SendChangeDescriptionMessage(StringBuilder description, bool isPublic)
        {
            if (base.CubeGrid.IsPreview || !base.CubeGrid.SyncFlag)
            {
                if (isPublic)
                {
                    this.PublicDescription = description;
                }
                else
                {
                    this.PrivateDescription = description;
                }
            }
            else if (!((description.CompareTo(this.PublicDescription) == 0) & isPublic) && ((description.CompareTo(this.PrivateDescription) != 0) || isPublic))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, string, bool>(this, x => new Action<string, bool>(x.OnChangeDescription), description.ToString(), isPublic, targetEndpoint);
            }
        }

        private void SendChangeOpenMessage(bool isOpen, bool editable = false, ulong user = 0UL, bool isPublic = false)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, bool, bool, ulong, bool>(this, x => new Action<bool, bool, ulong, bool>(x.OnChangeOpenRequest), isOpen, editable, user, isPublic, targetEndpoint);
        }

        private void SendChangeTitleMessage(StringBuilder title, bool isPublic)
        {
            if (base.CubeGrid.IsPreview || !base.CubeGrid.SyncFlag)
            {
                if (isPublic)
                {
                    this.PublicTitle = title;
                }
                else
                {
                    this.PrivateTitle = title;
                }
            }
            else if (!((title.CompareTo(this.PublicTitle) == 0) & isPublic) && ((title.CompareTo(this.PrivateTitle) != 0) || isPublic))
            {
                if (isPublic)
                {
                    this.PublicTitle = title;
                }
                else
                {
                    this.PrivateTitle = title;
                }
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, string, bool>(this, x => new Action<string, bool>(x.OnChangeTitle), title.ToString(), isPublic, targetEndpoint);
            }
        }

        private void SendRemoveSelectedImageRequest(Sandbox.ModAPI.IMyTextSurface panel, int[] selection)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, int[]>(this, x => new Action<int[]>(x.OnRemoveSelectedImageRequest), selection, targetEndpoint);
        }

        private void TextPanel_ClientRemoved(ulong playerId)
        {
            if (playerId == this.m_userId)
            {
                this.SendChangeOpenMessage(false, false, 0UL, false);
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (base.IsFunctional)
            {
                this.m_panelComponent.UpdateAfterSimulation(base.IsWorking, this.IsInRange(), this.PublicDescription.ToString());
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (base.IsBeingHacked)
            {
                this.PrivateDescription.Clear();
                this.SendChangeDescriptionMessage(this.PrivateDescription, false);
            }
            base.ResourceSink.Update();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
            if (orAddCell.ParentCullObject != uint.MaxValue)
            {
                this.Render.SetParent(0, orAddCell.ParentCullObject, new Matrix?(base.PositionComp.LocalMatrix));
            }
            this.PanelComponent.SetRender(this.Render);
            this.UpdateScreen();
        }

        public void UpdateScreen()
        {
            if (this.m_panelComponent != null)
            {
                this.m_panelComponent.UpdateAfterSimulation(this.CheckIsWorking(), this.IsInRange(), this.PublicDescription.ToString());
            }
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f, base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        public void Use(UseActionEnum actionEnum, VRage.ModAPI.IMyEntity entity)
        {
            if (!this.m_isTextPanelOpen)
            {
                MyCharacter user = entity as MyCharacter;
                MyRelationsBetweenPlayerAndBlock userRelationToOwner = base.GetUserRelationToOwner(user.ControllerInfo.Controller.Player.Identity.IdentityId);
                if (base.OwnerId == 0)
                {
                    this.OnOwnerUse(actionEnum, user);
                }
                else
                {
                    switch (userRelationToOwner)
                    {
                        case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                        case MyRelationsBetweenPlayerAndBlock.FactionShare:
                            if (base.OwnerId == 0)
                            {
                                this.OnOwnerUse(actionEnum, user);
                                return;
                            }
                            this.OnFactionUse(actionEnum, user);
                            return;

                        case MyRelationsBetweenPlayerAndBlock.Owner:
                            this.OnOwnerUse(actionEnum, user);
                            return;

                        case MyRelationsBetweenPlayerAndBlock.Neutral:
                        case MyRelationsBetweenPlayerAndBlock.Enemies:
                            if (!ReferenceEquals(MySession.Static.Factions.TryGetPlayerFaction(user.ControllerInfo.Controller.Player.Identity.IdentityId), MySession.Static.Factions.TryGetPlayerFaction(base.IDModule.Owner)) || (actionEnum != UseActionEnum.Manipulate))
                            {
                                this.OnEnemyUse(actionEnum, user);
                                return;
                            }
                            this.OnFactionUse(actionEnum, user);
                            return;
                    }
                }
            }
        }

        public VRage.Game.GUI.TextPanel.ContentType ContentType
        {
            get => 
                this.PanelComponent.ContentType;
            set => 
                (this.PanelComponent.ContentType = value);
        }

        public ShowTextOnScreenFlag ShowTextFlag
        {
            get => 
                this.PanelComponent.ShowTextFlag;
            set => 
                (this.PanelComponent.ShowTextFlag = value);
        }

        public bool ShowTextOnScreen =>
            this.PanelComponent.ShowTextOnScreen;

        public MyTextPanelComponent PanelComponent =>
            this.m_panelComponent;

        public StringBuilder PublicDescription
        {
            get => 
                this.m_publicDescription;
            set
            {
                if (this.m_publicDescription.CompareUpdate(value))
                {
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
                if (!ReferenceEquals(this.m_publicDescriptionHelper, value))
                {
                    this.m_publicDescriptionHelper.Clear().Append(value);
                }
            }
        }

        public StringBuilder PublicTitle
        {
            get => 
                this.m_publicTitle;
            set
            {
                this.m_publicTitle.CompareUpdate(value);
                if (!ReferenceEquals(this.m_publicTitleHelper, value))
                {
                    this.m_publicTitleHelper.Clear().Append(value);
                }
            }
        }

        public StringBuilder PrivateTitle
        {
            get => 
                this.m_privateTitle;
            set
            {
                this.m_privateTitle.CompareUpdate(value);
                if (!ReferenceEquals(this.m_privateTitleHelper, value))
                {
                    this.m_privateTitleHelper.Clear().Append(value);
                }
            }
        }

        public StringBuilder PrivateDescription
        {
            get => 
                this.m_privateDescription;
            set
            {
                if (this.m_privateDescription.CompareUpdate(value))
                {
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
                if (!ReferenceEquals(this.m_privateDescriptionHelper, value))
                {
                    this.m_privateDescriptionHelper.Clear().Append(value);
                }
            }
        }

        public bool IsTextPanelOpen
        {
            get => 
                this.m_isTextPanelOpen;
            set
            {
                if (this.m_isTextPanelOpen != value)
                {
                    this.m_isTextPanelOpen = value;
                    base.RaisePropertiesChanged();
                }
            }
        }

        public ulong UserId
        {
            get => 
                this.m_userId;
            set => 
                (this.m_userId = value);
        }

        public Vector2 SurfaceSize =>
            this.m_panelComponent.SurfaceSize;

        public Vector2 TextureSize =>
            this.m_panelComponent.TextureSize;

        internal MyRenderComponentTextPanel Render
        {
            get => 
                (base.Render as MyRenderComponentTextPanel);
            set => 
                (base.Render = value);
        }

        public MyTextPanelDefinition BlockDefinition =>
            ((MyTextPanelDefinition) base.BlockDefinition);

        public float FontSize
        {
            get => 
                this.m_panelComponent.FontSize;
            set => 
                (this.m_panelComponent.FontSize = (float) Math.Round((double) value, 3));
        }

        public Color FontColor
        {
            get => 
                this.m_panelComponent.FontColor;
            set => 
                (this.m_panelComponent.FontColor = value);
        }

        public Color BackgroundColor
        {
            get => 
                this.m_panelComponent.BackgroundColor;
            set => 
                (this.m_panelComponent.BackgroundColor = value);
        }

        public byte BackgroundAlpha
        {
            get => 
                this.m_panelComponent.BackgroundAlpha;
            set => 
                (this.m_panelComponent.BackgroundAlpha = value);
        }

        public float ChangeInterval
        {
            get => 
                this.m_panelComponent.ChangeInterval;
            set => 
                (this.m_panelComponent.ChangeInterval = (float) Math.Round((double) value, 3));
        }

        ShowTextOnScreenFlag Sandbox.ModAPI.Ingame.IMyTextPanel.ShowOnScreen =>
            this.ShowTextFlag;

        bool Sandbox.ModAPI.Ingame.IMyTextPanel.ShowText =>
            this.ShowTextOnScreen;

        string Sandbox.ModAPI.Ingame.IMyTextSurface.CurrentlyShownImage =>
            ((this.PanelComponent.SelectedTexturesToDraw.Count != 0) ? ((this.PanelComponent.CurrentSelectedTexture < this.PanelComponent.SelectedTexturesToDraw.Count) ? this.PanelComponent.SelectedTexturesToDraw[this.PanelComponent.CurrentSelectedTexture].Id.SubtypeName : this.PanelComponent.SelectedTexturesToDraw[0].Id.SubtypeName) : null);

        string Sandbox.ModAPI.Ingame.IMyTextSurface.Font
        {
            get => 
                this.PanelComponent.Font.SubtypeName;
            set
            {
                if (!string.IsNullOrEmpty(value) && (MyDefinitionManager.Static.GetDefinition<MyFontDefinition>(value) != null))
                {
                    this.PanelComponent.Font = MyDefinitionManager.Static.GetDefinition<MyFontDefinition>(value).Id;
                }
            }
        }

        TextAlignment Sandbox.ModAPI.Ingame.IMyTextSurface.Alignment
        {
            get => 
                ((this.m_panelComponent != null) ? this.m_panelComponent.Alignment : TextAlignment.LEFT);
            set
            {
                if (this.m_panelComponent != null)
                {
                    this.m_panelComponent.Alignment = value;
                }
            }
        }

        string Sandbox.ModAPI.Ingame.IMyTextSurface.Script
        {
            get => 
                ((this.m_panelComponent != null) ? this.m_panelComponent.Script : string.Empty);
            set
            {
                if (this.m_panelComponent != null)
                {
                    this.m_panelComponent.Script = value;
                }
            }
        }

        VRage.Game.GUI.TextPanel.ContentType Sandbox.ModAPI.Ingame.IMyTextSurface.ContentType
        {
            get => 
                this.ContentType;
            set => 
                (this.ContentType = value);
        }

        Vector2 Sandbox.ModAPI.Ingame.IMyTextSurface.SurfaceSize =>
            this.SurfaceSize;

        Vector2 Sandbox.ModAPI.Ingame.IMyTextSurface.TextureSize =>
            this.TextureSize;

        bool Sandbox.ModAPI.Ingame.IMyTextSurface.PreserveAspectRatio
        {
            get => 
                ((this.m_panelComponent != null) ? this.m_panelComponent.PreserveAspectRatio : false);
            set
            {
                if (this.m_panelComponent != null)
                {
                    this.m_panelComponent.PreserveAspectRatio = value;
                }
            }
        }

        float Sandbox.ModAPI.Ingame.IMyTextSurface.TextPadding
        {
            get => 
                ((this.m_panelComponent != null) ? this.m_panelComponent.TextPadding : 0f);
            set
            {
                if (this.m_panelComponent != null)
                {
                    this.m_panelComponent.TextPadding = value;
                }
            }
        }

        Color Sandbox.ModAPI.Ingame.IMyTextSurface.ScriptBackgroundColor
        {
            get => 
                ((this.m_panelComponent != null) ? this.m_panelComponent.ScriptBackgroundColor : Color.White);
            set
            {
                if (this.m_panelComponent != null)
                {
                    this.m_panelComponent.ScriptBackgroundColor = value;
                }
            }
        }

        Color Sandbox.ModAPI.Ingame.IMyTextSurface.ScriptForegroundColor
        {
            get => 
                ((this.m_panelComponent != null) ? this.m_panelComponent.ScriptForegroundColor : Color.White);
            set
            {
                if (this.m_panelComponent != null)
                {
                    this.m_panelComponent.ScriptForegroundColor = value;
                }
            }
        }

        string Sandbox.ModAPI.Ingame.IMyTextSurface.Name =>
            this.m_panelComponent?.Name;

        string Sandbox.ModAPI.Ingame.IMyTextSurface.DisplayName =>
            this.m_panelComponent?.DisplayName;

        int Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider.SurfaceCount =>
            1;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTextPanel.<>c <>9 = new MyTextPanel.<>c();
            public static MyTerminalControlTextbox<MyTextPanel>.GetterDelegate <>9__57_0;
            public static MyTerminalControlTextbox<MyTextPanel>.SetterDelegate <>9__57_1;
            public static Func<MyTextPanel, Action<int[]>> <>9__91_0;
            public static Func<MyTextPanel, Action<int[]>> <>9__93_0;
            public static Func<MyTextPanel, Action<bool, bool, ulong, bool>> <>9__97_0;
            public static Func<MyTextPanel, Action<bool, bool, ulong, bool>> <>9__98_0;
            public static Func<MyTextPanel, Action<string, bool>> <>9__101_0;
            public static Func<MyTextPanel, Action<string, bool>> <>9__102_0;

            internal StringBuilder <CreateTerminalControls>b__57_0(MyTextPanel x) => 
                x.PublicTitle;

            internal void <CreateTerminalControls>b__57_1(MyTextPanel x, StringBuilder v)
            {
                x.SendChangeTitleMessage(v, true);
            }

            internal Action<bool, bool, ulong, bool> <OnChangeOpenRequest>b__98_0(MyTextPanel x) => 
                new Action<bool, bool, ulong, bool>(x.OnChangeOpenSuccess);

            internal Action<int[]> <SendAddImagesToSelectionRequest>b__93_0(MyTextPanel x) => 
                new Action<int[]>(x.OnSelectImageRequest);

            internal Action<string, bool> <SendChangeDescriptionMessage>b__101_0(MyTextPanel x) => 
                new Action<string, bool>(x.OnChangeDescription);

            internal Action<bool, bool, ulong, bool> <SendChangeOpenMessage>b__97_0(MyTextPanel x) => 
                new Action<bool, bool, ulong, bool>(x.OnChangeOpenRequest);

            internal Action<string, bool> <SendChangeTitleMessage>b__102_0(MyTextPanel x) => 
                new Action<string, bool>(x.OnChangeTitle);

            internal Action<int[]> <SendRemoveSelectedImageRequest>b__91_0(MyTextPanel x) => 
                new Action<int[]>(x.OnRemoveSelectedImageRequest);
        }
    }
}

