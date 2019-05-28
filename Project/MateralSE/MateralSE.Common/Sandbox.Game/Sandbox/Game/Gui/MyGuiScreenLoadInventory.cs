namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Ansel;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;
    using VRageRender.Utils;

    public class MyGuiScreenLoadInventory : MyGuiScreenBase
    {
        [CompilerGenerated]
        private static MyLookChangeDelegate LookChanged;
        private static readonly bool SKIN_STORE_FEATURES_ENABLED;
        private readonly string m_hueScaleTexture;
        private readonly string m_equipCheckbox;
        private Vector2 m_itemsTableSize;
        private MyGuiControlButton m_viewDetailsButton;
        private MyGuiControlButton m_openStoreButton;
        private MyGuiControlButton m_refreshButton;
        private MyGuiControlButton m_browseItemsButton;
        private MyGuiControlButton m_characterButton;
        private MyGuiControlButton m_toolsButton;
        private MyGuiControlButton m_recyclingButton;
        private MyGuiControlButton m_currentButton;
        private bool m_inGame;
        private TabState m_activeTabState;
        private LowerTabState m_activeLowTabState;
        private string m_rotatingWheelTexture;
        private MyGuiControlRotatingWheel m_wheel;
        private MyEntityRemoteController m_entityController;
        private List<MyGuiControlCheckbox> m_itemCheckboxes;
        private bool m_itemCheckActive;
        private MyGuiControlCombobox m_modelPicker;
        private MyGuiControlSlider m_sliderHue;
        private MyGuiControlSlider m_sliderSaturation;
        private MyGuiControlSlider m_sliderValue;
        private MyGuiControlLabel m_labelHue;
        private MyGuiControlLabel m_labelSaturation;
        private MyGuiControlLabel m_labelValue;
        private string m_selectedModel;
        private Vector3 m_selectedHSV;
        private Dictionary<string, int> m_displayModels;
        private Dictionary<int, string> m_models;
        private string m_storedModel;
        private Vector3 m_storedHSV;
        private bool m_colorOrModelChanged;
        private Vector3D m_originalSpectatorPosition;
        private Vector3D m_targetSpectatorPosition;
        private const float ZOOM_SPEED = 2.5f;
        private bool m_zooming;
        private float m_zoomSpeed;
        private Vector3D m_zoomDirection;
        private const float ZOOM_ACCELERATION = 0.15f;
        private MyGameInventoryItemSlot m_filteredSlot;
        private MyGuiControlContextMenu m_contextMenu;
        private MyGuiControlImageButton m_contextMenuLastButton;
        private bool m_hideDuplicatesEnabled;
        private bool m_showOnlyDuplicatesEnabled;
        private MyGuiControlParent m_itemsTableParent;
        private List<MyGameInventoryItem> m_userItems;
        private List<MyPhysicalInventoryItem> m_allTools;
        private MyGuiControlCombobox m_toolPicker;
        private string m_selectedTool;
        private MyGuiControlButton m_OkButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlButton m_craftButton;
        private MyGuiControlCombobox m_rarityPicker;
        private MyGameInventoryItem m_lastCraftedItem;
        private MyGuiControlButton m_coloringButton;
        private bool m_audioSet;
        private bool? m_savedStateAnselEnabled;

        public static  event MyLookChangeDelegate LookChanged
        {
            [CompilerGenerated] add
            {
                MyLookChangeDelegate lookChanged = LookChanged;
                while (true)
                {
                    MyLookChangeDelegate a = lookChanged;
                    MyLookChangeDelegate delegate4 = (MyLookChangeDelegate) Delegate.Combine(a, value);
                    lookChanged = Interlocked.CompareExchange<MyLookChangeDelegate>(ref LookChanged, delegate4, a);
                    if (ReferenceEquals(lookChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyLookChangeDelegate lookChanged = LookChanged;
                while (true)
                {
                    MyLookChangeDelegate source = lookChanged;
                    MyLookChangeDelegate delegate4 = (MyLookChangeDelegate) Delegate.Remove(source, value);
                    lookChanged = Interlocked.CompareExchange<MyLookChangeDelegate>(ref LookChanged, delegate4, source);
                    if (ReferenceEquals(lookChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenLoadInventory() : base(new Vector2(0.32f, 0.05f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.65f, 0.9f), true, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_hueScaleTexture = @"Textures\GUI\HueScale.png";
            this.m_equipCheckbox = "equipCheckbox";
            this.m_originalSpectatorPosition = Vector3D.Zero;
            this.m_targetSpectatorPosition = Vector3D.Zero;
            this.m_zoomDirection = Vector3D.Forward;
            base.EnabledBackgroundFade = false;
        }

        public MyGuiScreenLoadInventory(bool inGame = false, HashSet<string> customCharacterNames = null) : base(new Vector2(0.32f, 0.05f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.65f, 0.9f), true, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_hueScaleTexture = @"Textures\GUI\HueScale.png";
            this.m_equipCheckbox = "equipCheckbox";
            this.m_originalSpectatorPosition = Vector3D.Zero;
            this.m_targetSpectatorPosition = Vector3D.Zero;
            this.m_zoomDirection = Vector3D.Forward;
            base.EnabledBackgroundFade = false;
            this.Initialize(inGame, customCharacterNames);
        }

        private static void Cancel()
        {
            if (MyGameService.InventoryItems != null)
            {
                using (IEnumerator<MyGameInventoryItem> enumerator = MyGameService.InventoryItems.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.IsInUse = false;
                    }
                }
            }
            MyLocalCache.LoadInventoryConfig(MySession.Static.LocalCharacter, true);
        }

        protected override void Canceling()
        {
            Cancel();
            base.Canceling();
        }

        private void ChangeCharacter(string model, Vector3 colorMaskHSV, bool resetToDefault = true)
        {
            this.m_colorOrModelChanged = true;
            MySession.Static.LocalCharacter.ChangeModelAndColor(model, colorMaskHSV, resetToDefault, MySession.Static.LocalPlayerId);
        }

        public override bool CloseScreen()
        {
            MyGuiScreenIntroVideo firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenIntroVideo>();
            if (firstScreenOfType != null)
            {
                firstScreenOfType.UnhideScreen();
            }
            return base.CloseScreen();
        }

        private MyGuiControlWrapPanel CreateItemsTable(MyGuiControlParent parent)
        {
            Vector2 itemSize = new Vector2(0.07f, 0.09f);
            MyGuiControlWrapPanel panel = new MyGuiControlWrapPanel(itemSize) {
                Size = this.m_itemsTableSize,
                Margin = new Thickness(0.018f, 0.044f, 0f, 0f),
                InnerOffset = new Vector2(0.005f, 0.0065f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            MyGuiControlImageButton.StateDefinition definition1 = new MyGuiControlImageButton.StateDefinition();
            definition1.Texture = new MyGuiCompositeTexture(@"Textures\Gui\Screens\screen_background_fade.dds");
            MyGuiControlImageButton.StyleDefinition definition3 = new MyGuiControlImageButton.StyleDefinition();
            definition3.Highlight = definition1;
            MyGuiControlImageButton.StateDefinition definition4 = new MyGuiControlImageButton.StateDefinition();
            definition4.Texture = new MyGuiCompositeTexture(@"Textures\Gui\Screens\screen_background_fade.dds");
            definition3.ActiveHighlight = definition4;
            MyGuiControlImageButton.StateDefinition definition5 = new MyGuiControlImageButton.StateDefinition();
            definition5.Texture = new MyGuiCompositeTexture(@"Textures\Gui\Screens\screen_background_fade.dds");
            definition3.Normal = definition5;
            MyGuiControlImageButton.StyleDefinition style = definition3;
            MyGuiControlCheckbox.StyleDefinition definition6 = new MyGuiControlCheckbox.StyleDefinition();
            definition6.NormalCheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_GREEN_CHECKED;
            definition6.NormalUncheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_BLANK;
            definition6.HighlightCheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_BLANK;
            definition6.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_BLANK;
            MyGuiControlCheckbox.StyleDefinition definition2 = definition6;
            this.m_itemCheckboxes = new List<MyGuiControlCheckbox>();
            this.m_userItems = this.GetInventoryItems();
            if (SKIN_STORE_FEATURES_ENABLED)
            {
                List<MyGameInventoryItem> storeItems = GetStoreItems(this.m_userItems);
                this.m_userItems.AddRange(storeItems);
            }
            int num = 0;
            while (true)
            {
                while (true)
                {
                    if (num >= this.m_userItems.Count)
                    {
                        return panel;
                    }
                    MyGameInventoryItem item = this.m_userItems[num];
                    if ((item.ItemDefinition.ItemSlot != MyGameInventoryItemSlot.None) && ((this.m_filteredSlot == MyGameInventoryItemSlot.None) || (this.m_filteredSlot == item.ItemDefinition.ItemSlot)))
                    {
                        if (this.m_filteredSlot == MyGameInventoryItemSlot.None)
                        {
                            MyGameInventoryItemSlot itemSlot = item.ItemDefinition.ItemSlot;
                            TabState activeTabState = this.m_activeTabState;
                            if (activeTabState != TabState.Character)
                            {
                                if ((activeTabState == TabState.Tools) && (((itemSlot == MyGameInventoryItemSlot.Helmet) || ((itemSlot == MyGameInventoryItemSlot.Gloves) || (itemSlot == MyGameInventoryItemSlot.Suit))) || (itemSlot == MyGameInventoryItemSlot.Boots)))
                                {
                                    break;
                                }
                            }
                            else if (((itemSlot == MyGameInventoryItemSlot.Grinder) || ((itemSlot == MyGameInventoryItemSlot.Rifle) || (itemSlot == MyGameInventoryItemSlot.Welder))) || (itemSlot == MyGameInventoryItemSlot.Drill))
                            {
                                break;
                            }
                        }
                        GridLength[] columns = new GridLength[] { new GridLength(1f, GridUnitType.Ratio), new GridLength(1f, GridUnitType.Ratio) };
                        GridLength[] rows = new GridLength[] { new GridLength(1f, GridUnitType.Ratio), new GridLength(0f, GridUnitType.Ratio) };
                        MyGuiControlLayoutGrid grid = new MyGuiControlLayoutGrid(columns, rows) {
                            Size = itemSize,
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
                        };
                        Vector2? size = null;
                        VRageMath.Vector4? colorMask = null;
                        Action<MyGuiControlImageButton> onButtonClick = new Action<MyGuiControlImageButton>(this.hiddenButton_ButtonClicked);
                        int? buttonIndex = null;
                        MyGuiControlImageButton button = new MyGuiControlImageButton("Button", new Vector2?(grid.Position), size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, item.ItemDefinition.Name, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onButtonClick, new Action<MyGuiControlImageButton>(this.hiddenButton_ButtonRightClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
                        button.Tooltips.AddToolTip(string.Empty, 0.7f, "Blue");
                        if (item.ItemDefinition.ItemSlot == MyGameInventoryItemSlot.Helmet)
                        {
                            MyControl gameControl = MyInput.Static.GetGameControl(MyControlsSpace.HELMET);
                            string toolTip = string.Format(MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryToggleHelmet), gameControl.GetControlButtonName(MyGuiInputDeviceEnum.Keyboard));
                            button.Tooltips.AddToolTip(toolTip, 0.7f, "Blue");
                        }
                        button.Tooltips.AddToolTip(MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryLeftClickTip), 0.7f, "Blue");
                        button.Tooltips.AddToolTip(MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryRightClickTip), 0.7f, "Blue");
                        button.ApplyStyle(style);
                        button.Size = grid.Size;
                        parent.Controls.Add(button);
                        MyGuiControlImage image = null;
                        if (string.IsNullOrEmpty(item.ItemDefinition.BackgroundColor))
                        {
                            size = null;
                            colorMask = null;
                            string[] textArray1 = new string[] { @"Textures\GUI\Controls\grid_item_highlight.dds" };
                            image = new MyGuiControlImage(size, new Vector2?(itemSize), colorMask, null, textArray1, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                            parent.Controls.Add(image);
                        }
                        else
                        {
                            VRageMath.Vector4 vector2 = string.IsNullOrEmpty(item.ItemDefinition.BackgroundColor) ? VRageMath.Vector4.One : ColorExtensions.HexToVector4(item.ItemDefinition.BackgroundColor);
                            size = null;
                            string[] textArray2 = new string[] { @"Textures\GUI\blank.dds" };
                            image = new MyGuiControlImage(size, new Vector2(itemSize.X - 0.004f, itemSize.Y - 0.002f), new VRageMath.Vector4?(vector2), null, textArray2, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                            parent.Controls.Add(image);
                            image.Margin = new Thickness(0.0023f, 0.001f, 0f, 0f);
                        }
                        string[] textures = new string[] { @"Textures\GUI\Blank.dds" };
                        if (!string.IsNullOrEmpty(item.ItemDefinition.IconTexture))
                        {
                            textures[0] = item.ItemDefinition.IconTexture;
                        }
                        size = null;
                        colorMask = null;
                        MyGuiControlImage image2 = new MyGuiControlImage(size, new Vector2(0.06f, 0.08f), colorMask, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) {
                            Margin = new Thickness(0.005f, 0.005f, 0f, 0f)
                        };
                        parent.Controls.Add(image2);
                        size = null;
                        colorMask = null;
                        MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(size, colorMask, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) {
                            Name = this.m_equipCheckbox
                        };
                        checkbox.ApplyStyle(definition2);
                        checkbox.Margin = new Thickness(0.005f, 0.005f, 0.01f, 0.01f);
                        checkbox.IsHitTestVisible = false;
                        parent.Controls.Add(checkbox);
                        checkbox.UserData = item;
                        button.UserData = grid;
                        this.m_itemCheckboxes.Add(checkbox);
                        grid.Add(image, 0, 0);
                        grid.Add(button, 0, 0);
                        grid.Add(image2, 0, 0);
                        grid.Add(checkbox, 1, 0);
                        if (item.IsNew)
                        {
                            size = null;
                            colorMask = null;
                            string[] textArray4 = new string[] { @"Textures\GUI\Icons\HUD 2017\Notification_badge.png" };
                            MyGuiControlImage control = new MyGuiControlImage(size, new Vector2(0.0175f, 0.023f), colorMask, null, textArray4, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) {
                                Margin = new Thickness(0.01f, -0.035f, 0f, 0f)
                            };
                            parent.Controls.Add(control);
                            grid.Add(control, 1, 1);
                        }
                        panel.Add(grid);
                        parent.Controls.Add(grid);
                    }
                    break;
                }
                num++;
            }
        }

        private void CreateRecyclerUI(MyGuiControlStackPanel panel)
        {
            GridLength[] columns = new GridLength[] { new GridLength(1.4f, GridUnitType.Ratio), new GridLength(0.6f, GridUnitType.Ratio), new GridLength(0.8f, GridUnitType.Ratio) };
            GridLength[] rows = new GridLength[] { new GridLength(1f, GridUnitType.Ratio) };
            MyGuiControlLayoutGrid control = new MyGuiControlLayoutGrid(columns, rows) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Margin = new Thickness(0.055f, -0.035f, 0f, 0f),
                Size = new Vector2(0.65f, 0.1f)
            };
            MyGuiControlStackPanel panel2 = new MyGuiControlStackPanel {
                Orientation = MyGuiOrientation.Horizontal,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER,
                Margin = new Thickness(0f)
            };
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel label = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenLoadInventorySelectRarity), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                Margin = new Thickness(0f, 0f, 0.01f, 0f)
            };
            panel2.Add(label);
            this.Controls.Add(label);
            this.m_rarityPicker = new MyGuiControlCombobox();
            this.m_rarityPicker.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_rarityPicker.Size = new Vector2(0.15f, 0f);
            foreach (object obj2 in System.Enum.GetValues(typeof(MyGameInventoryItemQuality)))
            {
                int? sortOrder = null;
                this.m_rarityPicker.AddItem((long) ((int) obj2), MyTexts.GetString(MyStringId.GetOrCompute(obj2.ToString())), sortOrder, null);
            }
            this.m_rarityPicker.SelectItemByIndex(0);
            this.m_rarityPicker.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_rarityPicker_ItemSelected);
            panel2.Add(this.m_rarityPicker);
            this.Controls.Add(this.m_rarityPicker);
            if (this.m_lastCraftedItem != null)
            {
                Vector2 vector = new Vector2(0.07f, 0.09f);
                MyGuiControlImage image = null;
                if (string.IsNullOrEmpty(this.m_lastCraftedItem.ItemDefinition.BackgroundColor))
                {
                    position = null;
                    colorMask = null;
                    string[] textArray1 = new string[] { @"Textures\GUI\Controls\grid_item_highlight.dds" };
                    image = new MyGuiControlImage(position, new Vector2?(vector), colorMask, null, textArray1, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
                    this.Controls.Add(image);
                }
                else
                {
                    VRageMath.Vector4 vector2 = string.IsNullOrEmpty(this.m_lastCraftedItem.ItemDefinition.BackgroundColor) ? VRageMath.Vector4.One : ColorExtensions.HexToVector4(this.m_lastCraftedItem.ItemDefinition.BackgroundColor);
                    position = null;
                    string[] textArray2 = new string[] { @"Textures\GUI\blank.dds" };
                    image = new MyGuiControlImage(position, new Vector2(vector.X - 0.004f, vector.Y - 0.002f), new VRageMath.Vector4?(vector2), null, textArray2, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    this.Controls.Add(image);
                }
                string[] textures = new string[] { @"Textures\GUI\Blank.dds" };
                if (!string.IsNullOrEmpty(this.m_lastCraftedItem.ItemDefinition.IconTexture))
                {
                    textures[0] = this.m_lastCraftedItem.ItemDefinition.IconTexture;
                }
                position = null;
                colorMask = null;
                MyGuiControlImage image2 = new MyGuiControlImage(position, new Vector2(0.06f, 0.08f), colorMask, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                this.Controls.Add(image2);
                position = null;
                position = null;
                colorMask = null;
                MyGuiControlLabel label2 = new MyGuiControlLabel(position, position, this.m_lastCraftedItem.ItemDefinition.Name, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                this.Controls.Add(label2);
                position = null;
                position = null;
                colorMask = null;
                MyGuiControlLabel label3 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryCraftedLabel), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP) {
                    Margin = new Thickness(0f, -0.035f, 0f, 0f)
                };
                this.Controls.Add(label3);
                control.Add(image, 1, 0);
                control.Add(image2, 1, 0);
                control.Add(label2, 2, 0);
                control.Add(label3, 2, 0);
            }
            control.Add(panel2, 0, 0);
            panel.Add(control);
        }

        private void DeleteItemRequest()
        {
            if (this.GetCurrentItem() != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.ScreenLoadInventoryDeleteItemMessageTitle);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.ScreenLoadInventoryDeleteItemMessageText), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.DeleteItemRequestMessageHandler), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void DeleteItemRequestMessageHandler(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyGameInventoryItem currentItem = this.GetCurrentItem();
                if (currentItem != null)
                {
                    MyGameService.ConsumeItem(currentItem);
                    this.RemoveItemFromUI(currentItem);
                }
            }
            base.State = MyGuiScreenState.OPENING;
        }

        public override bool Draw()
        {
            this.DrawScene();
            return base.Draw();
        }

        private unsafe void DrawScene()
        {
            if (MySession.Static != null)
            {
                MyEnvironmentData* dataPtr1;
                if (((MySession.Static.CameraController == null) || !MySession.Static.CameraController.IsInFirstPersonView) && (MyThirdPersonSpectator.Static != null))
                {
                    MyThirdPersonSpectator.Static.Update();
                }
                if (MySector.MainCamera != null)
                {
                    MySession.Static.CameraController.ControlCamera(MySector.MainCamera);
                    MySector.MainCamera.Update(0.01666667f);
                    MySector.MainCamera.UploadViewMatrixToRender();
                }
                MySector.UpdateSunLight();
                MyRenderProxy.UpdateGameplayFrame(MySession.Static.GameplayFrameCounter);
                MyRenderFogSettings settings = new MyRenderFogSettings {
                    FogMultiplier = MySector.FogProperties.FogMultiplier,
                    FogColor = MySector.FogProperties.FogColor,
                    FogDensity = MySector.FogProperties.FogDensity / 100f
                };
                MyRenderProxy.UpdateFogSettings(ref settings);
                MyRenderPlanetSettings settings4 = new MyRenderPlanetSettings {
                    AtmosphereIntensityMultiplier = MySector.PlanetProperties.AtmosphereIntensityMultiplier,
                    AtmosphereIntensityAmbientMultiplier = MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier,
                    AtmosphereDesaturationFactorForward = MySector.PlanetProperties.AtmosphereDesaturationFactorForward,
                    CloudsIntensityMultiplier = MySector.PlanetProperties.CloudsIntensityMultiplier
                };
                MyRenderProxy.UpdatePlanetSettings(ref settings4);
                MyRenderProxy.UpdateSSAOSettings(ref MySector.SSAOSettings);
                MyRenderProxy.UpdateHBAOSettings(ref MySector.HBAOSettings);
                MyEnvironmentData environmentData = MySector.SunProperties.EnvironmentData;
                dataPtr1->Skybox = !string.IsNullOrEmpty(MySession.Static.CustomSkybox) ? MySession.Static.CustomSkybox : MySector.EnvironmentDefinition.EnvironmentTexture;
                dataPtr1 = (MyEnvironmentData*) ref environmentData;
                environmentData.SkyboxOrientation = MySector.EnvironmentDefinition.EnvironmentOrientation.ToQuaternion();
                environmentData.EnvironmentLight.SunLightDirection = -MySector.SunProperties.SunDirectionNormalized;
                MyRenderProxy.UpdateRenderEnvironment(ref environmentData, MySector.ResetEyeAdaptation);
                MySector.ResetEyeAdaptation = false;
                MyRenderProxy.UpdateEnvironmentMap();
                if ((MyVideoSettingsManager.CurrentGraphicsSettings.PostProcessingEnabled != MyPostprocessSettingsWrapper.AllEnabled) || MyPostprocessSettingsWrapper.IsDirty)
                {
                    if (MyVideoSettingsManager.CurrentGraphicsSettings.PostProcessingEnabled)
                    {
                        MyPostprocessSettingsWrapper.SetWardrobePostProcessing();
                    }
                    else
                    {
                        MyPostprocessSettingsWrapper.ReducePostProcessing();
                    }
                }
                MyRenderProxy.SwitchPostprocessSettings(ref MyPostprocessSettingsWrapper.Settings);
                if (MyRenderProxy.SettingsDirty)
                {
                    MyRenderProxy.SwitchRenderSettings(MyRenderProxy.Settings);
                }
                MyRenderProxy.Draw3DScene();
                using (Stats.Generic.Measure("GamePrepareDraw"))
                {
                    if (MySession.Static != null)
                    {
                        MySession.Static.Draw();
                    }
                }
            }
        }

        private void EquipTool()
        {
            if ((this.m_filteredSlot != MyGameInventoryItemSlot.None) && (this.m_activeTabState == TabState.Tools))
            {
                long key = this.m_toolPicker.GetSelectedKey();
                MyPhysicalInventoryItem item = this.m_allTools.FirstOrDefault<MyPhysicalInventoryItem>(t => t.ItemId == key);
                if (item.Content != null)
                {
                    this.m_entityController.ActivateCharacterToolbarItem(new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), item.Content.SubtypeName));
                }
                foreach (MyGameInventoryItem item2 in this.m_userItems)
                {
                    if (item2.IsInUse && (item2.ItemDefinition.ItemSlot == this.m_filteredSlot))
                    {
                        this.m_itemCheckActive = true;
                        MyGameService.GetItemCheckData(item2);
                        break;
                    }
                }
            }
            else
            {
                MyDefinitionId item = new MyDefinitionId();
                this.m_entityController.ActivateCharacterToolbarItem(item);
            }
        }

        private MyGameInventoryItem GetCurrentItem()
        {
            if (this.m_contextMenuLastButton == null)
            {
                return null;
            }
            MyGuiControlLayoutGrid userData = this.m_contextMenuLastButton.UserData as MyGuiControlLayoutGrid;
            if (userData == null)
            {
                return null;
            }
            MyGuiControlCheckbox checkbox = userData.GetAllControls().FirstOrDefault<MyGuiControlBase>(c => c.Name.StartsWith(this.m_equipCheckbox)) as MyGuiControlCheckbox;
            return ((checkbox != null) ? (checkbox.UserData as MyGameInventoryItem) : null);
        }

        private string GetDisplayName(string name) => 
            MyTexts.GetString(name);

        public override string GetFriendlyName() => 
            "MyGuiScreenLoadInventory";

        private List<MyGameInventoryItem> GetInventoryItems()
        {
            List<MyGameInventoryItem> list = new List<MyGameInventoryItem>(MyGameService.InventoryItems);
            List<MyGameInventoryItem> source = null;
            if (this.m_activeLowTabState == LowerTabState.Coloring)
            {
                if ((list.Count <= 0) || !this.m_hideDuplicatesEnabled)
                {
                    source = (from i in list
                        orderby i.IsNew descending, i.IsInUse descending, i.ItemDefinition.Name
                        select i).ToList<MyGameInventoryItem>();
                }
                else
                {
                    source = new List<MyGameInventoryItem>();
                    source.AddRange(from i in list
                        where i.IsNew
                        select i);
                    source.AddRange(from i in list
                        where i.IsInUse
                        select i);
                    using (List<MyGameInventoryItem>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyGameInventoryItem item;
                            if (!item.IsInUse && (source.FirstOrDefault<MyGameInventoryItem>(i => (i.ItemDefinition.AssetModifierId == item.ItemDefinition.AssetModifierId)) == null))
                            {
                                source.Add(item);
                            }
                        }
                    }
                    source = (from i in source
                        orderby i.IsNew descending, i.IsInUse descending, i.ItemDefinition.Name
                        select i).ToList<MyGameInventoryItem>();
                }
            }
            else if ((list.Count <= 0) || !this.m_showOnlyDuplicatesEnabled)
            {
                source = new List<MyGameInventoryItem>();
                source.AddRange(from i in list
                    where !i.IsInUse
                    select i);
                source = (from i in source
                    orderby i.IsNew descending, i.IsInUse descending, i.ItemDefinition.Name
                    select i).ToList<MyGameInventoryItem>();
            }
            else
            {
                HashSet<string> set = new HashSet<string>();
                source = new List<MyGameInventoryItem>();
                foreach (MyGameInventoryItem item in list)
                {
                    if (!item.IsInUse)
                    {
                        if (!set.Contains(item.ItemDefinition.AssetModifierId))
                        {
                            set.Add(item.ItemDefinition.AssetModifierId);
                            continue;
                        }
                        source.Add(item);
                    }
                }
                source = (from i in source
                    orderby i.IsNew descending, i.IsInUse descending, i.ItemDefinition.Name
                    select i).ToList<MyGameInventoryItem>();
            }
            return source;
        }

        private static List<MyGameInventoryItem> GetStoreItems(List<MyGameInventoryItem> userItems)
        {
            List<MyGameInventoryItem> list = new List<MyGameInventoryItem>();
            using (List<MyGameInventoryItemDefinition>.Enumerator enumerator = new List<MyGameInventoryItemDefinition>(MyGameService.Definitions).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyGameInventoryItemDefinition item;
                    if ((item.DefinitionType == MyGameInventoryItemDefinitionType.item) && ((item.ItemSlot != MyGameInventoryItemSlot.None) && (!item.Hidden && (!item.IsStoreHidden && (userItems.FirstOrDefault<MyGameInventoryItem>(i => (i.ItemDefinition.ID == item.ID)) == null)))))
                    {
                        MyGameInventoryItem item1 = new MyGameInventoryItem();
                        item1.ID = 0L;
                        item1.IsStoreFakeItem = true;
                        item1.ItemDefinition = item;
                        item1.Quantity = 1;
                        list.Add(item1);
                    }
                }
            }
            return (from i in list
                orderby i.ItemDefinition.Name
                select i).ToList<MyGameInventoryItem>();
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (this.m_entityController != null)
            {
                bool isMouseOverAnyControl = base.IsMouseOver() || (base.m_lastHandlingControl != null);
                this.m_entityController.Update(isMouseOverAnyControl);
            }
        }

        private void hiddenButton_ButtonClicked(MyGuiControlImageButton obj)
        {
            MyGuiControlLayoutGrid userData = obj.UserData as MyGuiControlLayoutGrid;
            if (userData != null)
            {
                MyGuiControlCheckbox checkbox = userData.GetAllControls().FirstOrDefault<MyGuiControlBase>(c => c.Name.StartsWith(this.m_equipCheckbox)) as MyGuiControlCheckbox;
                if (checkbox != null)
                {
                    checkbox.IsChecked = !checkbox.IsChecked;
                }
            }
        }

        private void hiddenButton_ButtonRightClicked(MyGuiControlImageButton obj)
        {
            this.m_contextMenuLastButton = obj;
            MyGuiControlListbox.Item item = this.m_contextMenu.Items.FirstOrDefault<MyGuiControlListbox.Item>(i => (i.UserData != null) && (((InventoryItemAction) i.UserData) == InventoryItemAction.Recycle));
            if (item != null)
            {
                MyGameInventoryItem currentItem = this.GetCurrentItem();
                if (currentItem != null)
                {
                    item.Text = new StringBuilder(string.Format(MyTexts.Get(MyCommonTexts.ScreenLoadInventoryRecycleItem).ToString(), MyGameService.GetRecyclingReward(currentItem.ItemDefinition.ItemQuality)));
                }
            }
            this.m_contextMenu.Activate(true);
            base.FocusedControl = this.m_contextMenu;
        }

        public void Initialize(bool inGame, HashSet<string> customCharacterNames)
        {
            this.m_inGame = inGame;
            this.m_audioSet = inGame;
            this.m_rotatingWheelTexture = @"Textures\GUI\screens\screen_loading_wheel_loading_screen.dds";
            base.Align = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_filteredSlot = MyGameInventoryItemSlot.None;
            base.IsHitTestVisible = true;
            MyGameService.CheckItemDataReady += new EventHandler<MyGameItemsEventArgs>(this.MyGameService_CheckItemDataReady);
            this.m_storedModel = (MySession.Static.LocalCharacter != null) ? MySession.Static.LocalCharacter.ModelName : string.Empty;
            this.InitModels(customCharacterNames);
            this.m_entityController = new MyEntityRemoteController(MySession.Static.LocalCharacter);
            this.m_entityController.LockRotationAxis(GlobalAxis.Z | GlobalAxis.Y);
            this.m_allTools = this.m_entityController.GetInventoryTools();
            this.RecreateControls(true);
            this.UpdateSliderTooltips();
            MyGuiScreenIntroVideo firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenIntroVideo>();
            if (firstScreenOfType != null)
            {
                firstScreenOfType.HideScreen();
            }
            if (!inGame)
            {
                MyLocalCache.LoadInventoryConfig(MySession.Static.LocalCharacter, true);
            }
            this.EquipTool();
            this.UpdateCheckboxes();
            base.m_isTopMostScreen = false;
        }

        private void InitModels(HashSet<string> customCharacterNames)
        {
            this.m_displayModels = new Dictionary<string, int>();
            this.m_models = new Dictionary<int, string>();
            int num = 0;
            if (customCharacterNames == null)
            {
                foreach (MyCharacterDefinition definition in MyDefinitionManager.Static.Characters)
                {
                    if (!definition.UsableByPlayer)
                    {
                        if (MySession.Static.SurvivalMode)
                        {
                            continue;
                        }
                        if (!this.m_inGame)
                        {
                            continue;
                        }
                        if (!MySession.Static.Settings.IsSettingsExperimental())
                        {
                            continue;
                        }
                    }
                    if (definition.Public)
                    {
                        string displayName = this.GetDisplayName(definition.Name);
                        this.m_displayModels[displayName] = num;
                        num++;
                        this.m_models[num] = definition.Name;
                    }
                }
            }
            else
            {
                DictionaryValuesReader<string, MyCharacterDefinition> characters = MyDefinitionManager.Static.Characters;
                foreach (string str2 in customCharacterNames)
                {
                    MyCharacterDefinition definition2;
                    if (!characters.TryGetValue(str2, out definition2))
                    {
                        continue;
                    }
                    if ((!MySession.Static.SurvivalMode || definition2.UsableByPlayer) && definition2.Public)
                    {
                        string displayName = this.GetDisplayName(definition2.Name);
                        this.m_displayModels[displayName] = num;
                        num++;
                        this.m_models[num] = definition2.Name;
                    }
                }
            }
        }

        private void m_contextMenu_ItemClicked(MyGuiControlContextMenu contextMenu, MyGuiControlContextMenu.EventArgs selectedItem)
        {
            switch (((InventoryItemAction) selectedItem.UserData))
            {
                case InventoryItemAction.Apply:
                    this.hiddenButton_ButtonClicked(this.m_contextMenuLastButton);
                    return;

                case InventoryItemAction.Sell:
                    this.OpenUserInventory();
                    return;

                case InventoryItemAction.Trade:
                    break;

                case InventoryItemAction.Recycle:
                    this.RecycleItemRequest();
                    return;

                case InventoryItemAction.Delete:
                    this.DeleteItemRequest();
                    return;

                case InventoryItemAction.Buy:
                    this.OpenCurrentItemInStore();
                    break;

                default:
                    return;
            }
        }

        private void m_rarityPicker_ItemSelected()
        {
            uint craftingCost = MyGameService.GetCraftingCost((MyGameInventoryItemQuality) this.m_rarityPicker.GetSelectedIndex());
            this.m_craftButton.Text = string.Format(MyTexts.GetString(MyCommonTexts.CraftButton), craftingCost);
            this.m_craftButton.Enabled = MyGameService.RecycleTokens >= craftingCost;
        }

        private void m_toolPicker_ItemSelected()
        {
            MyPhysicalInventoryItem item = this.m_allTools.FirstOrDefault<MyPhysicalInventoryItem>(t => t.ItemId == this.m_toolPicker.GetSelectedKey());
            if (item.Content != null)
            {
                this.m_selectedTool = item.Content.SubtypeName;
                this.EquipTool();
            }
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, MyStringId toolTip, Action<MyGuiControlButton> onClick)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            StringBuilder builder = MyTexts.Get(text);
            int? buttonIndex = null;
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(toolTip), builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            return button1;
        }

        private MyGuiControlImageButton MakeImageButton(Vector2 position, Vector2 size, MyGuiControlImageButton.StyleDefinition style, MyStringId toolTip, Action<MyGuiControlImageButton> onClick)
        {
            Vector2? nullable = null;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            MyGuiControlImageButton button = new MyGuiControlImageButton("Button", new Vector2?(position), nullable, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(toolTip), null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            if (style != null)
            {
                button.ApplyStyle(style);
            }
            button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button.Size = size;
            return button;
        }

        private void MyGameService_CheckItemDataReady(object sender, MyGameItemsEventArgs itemArgs)
        {
            if ((itemArgs.NewItems != null) && (itemArgs.NewItems.Count != 0))
            {
                MyGameInventoryItem item = itemArgs.NewItems[0];
                this.UseItem(item, itemArgs.CheckData);
                foreach (MyGameInventoryItem item2 in new List<MyGameInventoryItem>(MyGameService.InventoryItems))
                {
                    if (item2 == null)
                    {
                        continue;
                    }
                    if ((item2.ID != item.ID) && (item2.ItemDefinition.ItemSlot == item.ItemDefinition.ItemSlot))
                    {
                        item2.IsInUse = false;
                    }
                }
                this.UpdateCheckboxes();
                this.UpdateOKButton();
            }
        }

        private void MyGameService_ItemsAdded(object sender, MyGameItemsEventArgs e)
        {
            if ((e.NewItems != null) && (e.NewItems.Count > 0))
            {
                this.m_lastCraftedItem = e.NewItems[0];
                this.m_lastCraftedItem.IsNew = true;
                MyGameService.ItemsAdded -= new EventHandler<MyGameItemsEventArgs>(this.MyGameService_ItemsAdded);
                this.RefreshUI();
            }
            this.RotatingWheelHide();
        }

        private void MySteamInventory_Refreshed(object sender, System.EventArgs e)
        {
            if (this.m_itemCheckActive)
            {
                this.m_itemCheckActive = false;
            }
            else
            {
                this.RefreshUI();
            }
        }

        private void OnCancelClick(MyGuiControlButton obj)
        {
            Cancel();
            this.CloseScreen();
        }

        private void OnCategoryClicked(MyGuiControlImageButton obj)
        {
            if (obj.UserData != null)
            {
                MyGameInventoryItemSlot userData = (MyGameInventoryItemSlot) obj.UserData;
                if (userData != this.m_filteredSlot)
                {
                    this.m_filteredSlot = userData;
                }
                else if (this.m_activeTabState == TabState.Character)
                {
                    this.m_filteredSlot = MyGameInventoryItemSlot.None;
                }
                this.m_selectedTool = string.Empty;
                this.RecreateControls(false);
                this.EquipTool();
                this.UpdateCheckboxes();
            }
        }

        protected override void OnClosed()
        {
            if (MyGameService.IsActive)
            {
                MyGameService.InventoryRefreshed -= new EventHandler(this.MySteamInventory_Refreshed);
            }
            this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            MyGameService.CheckItemDataReady -= new EventHandler<MyGameItemsEventArgs>(this.MyGameService_CheckItemDataReady);
            MyRenderProxy.UnloadTexture(this.m_rotatingWheelTexture);
            base.OnClosed();
            MyAnalyticsHelper.ReportActivityEnd(null, "show_inventory");
            if (!this.m_inGame)
            {
                MyAnsel.IsAnselSessionEnabled = false;
            }
            else if (this.m_savedStateAnselEnabled != null)
            {
                MyAnsel.IsAnselSessionEnabled = this.m_savedStateAnselEnabled.Value;
                this.m_savedStateAnselEnabled = null;
            }
        }

        private void OnCraftClick(MyGuiControlButton obj)
        {
            MyGameService.ItemsAdded -= new EventHandler<MyGameItemsEventArgs>(this.MyGameService_ItemsAdded);
            MyGameService.ItemsAdded += new EventHandler<MyGameItemsEventArgs>(this.MyGameService_ItemsAdded);
            if (MyGameService.CraftSkin((MyGameInventoryItemQuality) ((int) this.m_rarityPicker.GetSelectedKey())))
            {
                this.RotatingWheelShow();
            }
            else
            {
                MyGameService.ItemsAdded -= new EventHandler<MyGameItemsEventArgs>(this.MyGameService_ItemsAdded);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            this.DrawScene();
        }

        private void OnHideDuplicates(MyGuiControlCheckbox obj)
        {
            this.m_hideDuplicatesEnabled = obj.IsChecked;
            this.RefreshUI();
        }

        private void OnItemCheckChanged(MyGuiControlCheckbox sender)
        {
            if (sender != null)
            {
                MyGameInventoryItem userData = sender.UserData as MyGameInventoryItem;
                if (userData != null)
                {
                    if (sender.IsChecked)
                    {
                        this.m_itemCheckActive = true;
                        MyGameService.GetItemCheckData(userData);
                    }
                    else
                    {
                        this.m_itemCheckActive = false;
                        userData.IsInUse = false;
                        MyCharacter localCharacter = MySession.Static.LocalCharacter;
                        if (localCharacter != null)
                        {
                            if (localCharacter != null)
                            {
                                MyAssetModifierComponent component;
                                MyGameInventoryItemSlot itemSlot = userData.ItemDefinition.ItemSlot;
                                if ((itemSlot - 1) <= MyGameInventoryItemSlot.Boots)
                                {
                                    if (localCharacter.Components.TryGet<MyAssetModifierComponent>(out component))
                                    {
                                        component.ResetSlot(userData.ItemDefinition.ItemSlot);
                                    }
                                }
                                else if ((itemSlot - 6) <= MyGameInventoryItemSlot.Gloves)
                                {
                                    MyEntity currentWeapon = localCharacter.CurrentWeapon as MyEntity;
                                    if ((currentWeapon != null) && currentWeapon.Components.TryGet<MyAssetModifierComponent>(out component))
                                    {
                                        component.ResetSlot(userData.ItemDefinition.ItemSlot);
                                    }
                                }
                            }
                            this.UpdateOKButton();
                        }
                    }
                }
            }
        }

        private void OnItemSelected()
        {
            Cancel();
            int selectedKey = (int) this.m_modelPicker.GetSelectedKey();
            this.m_selectedModel = this.m_models[selectedKey];
            this.ChangeCharacter(this.m_selectedModel, this.m_selectedHSV, true);
            this.RecreateControls(false);
            MyLocalCache.ResetAllInventorySlots(MySession.Static.LocalCharacter);
            this.RefreshItems();
        }

        private void OnOkClick(MyGuiControlButton obj)
        {
            if ((this.m_colorOrModelChanged && (LookChanged != null)) && (MySession.Static != null))
            {
                LookChanged();
            }
            if (MySession.Static.LocalCharacter.Definition.UsableByPlayer)
            {
                MyLocalCache.SaveInventoryConfig(MySession.Static.LocalCharacter);
            }
            this.CloseScreen();
        }

        private void OnOpenStore(MyGuiControlButton obj)
        {
            MyGuiSandbox.OpenUrlWithFallback(string.Format(MySteamConstants.URL_INVENTORY_BROWSE_ALL_ITEMS, MyGameService.AppId), "Store", false);
        }

        private void OnRecycleItem(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyGameInventoryItem currentItem = this.GetCurrentItem();
                if ((currentItem != null) && MyGameService.RecycleItem(currentItem))
                {
                    this.RemoveItemFromUI(currentItem);
                }
            }
            base.State = MyGuiScreenState.OPENING;
        }

        private void OnRefreshClick(MyGuiControlButton obj)
        {
            this.m_refreshButton.Enabled = false;
            this.RotatingWheelShow();
            this.RefreshItems();
        }

        protected override void OnShow()
        {
            this.m_savedStateAnselEnabled = new bool?(MyAnsel.IsAnselSessionEnabled);
            MyAnsel.IsAnselSessionEnabled = false;
            if ((MySector.MainCamera != null) && !this.m_inGame)
            {
                MySector.MainCamera.FieldOfViewDegrees = 55f;
            }
            if (MyGameService.IsActive)
            {
                MyGameService.InventoryRefreshed += new EventHandler(this.MySteamInventory_Refreshed);
            }
            base.OnShow();
            MyAnalyticsHelper.ReportActivityStart(null, "show_inventory", string.Empty, "gui", string.Empty, true);
        }

        private void OnShowOnlyDuplicates(MyGuiControlCheckbox obj)
        {
            this.m_showOnlyDuplicatesEnabled = obj.IsChecked;
            this.RefreshUI();
        }

        private void OnValueChange(MyGuiControlSlider sender)
        {
            this.UpdateSliderTooltips();
            this.m_selectedHSV.X = this.m_sliderHue.Value / 360f;
            this.m_selectedHSV.Y = this.m_sliderSaturation.Value - MyColorPickerConstants.SATURATION_DELTA;
            this.m_selectedHSV.Z = (this.m_sliderValue.Value - MyColorPickerConstants.VALUE_DELTA) + MyColorPickerConstants.VALUE_COLORIZE_DELTA;
            this.ChangeCharacter(this.m_selectedModel, this.m_selectedHSV, false);
        }

        private void OnViewTabCharacter(MyGuiControlButton obj)
        {
            this.m_activeTabState = TabState.Character;
            this.m_filteredSlot = MyGameInventoryItemSlot.None;
            this.m_selectedTool = string.Empty;
            this.EquipTool();
            this.RecreateControls(false);
            this.UpdateCheckboxes();
        }

        private void OnViewTabColoring(MyGuiControlButton obj)
        {
            this.m_activeLowTabState = LowerTabState.Coloring;
            this.EquipTool();
            this.RecreateControls(false);
            this.UpdateCheckboxes();
        }

        private void OnViewTabRecycling(MyGuiControlButton obj)
        {
            this.m_activeLowTabState = LowerTabState.Recycling;
            this.EquipTool();
            this.RecreateControls(false);
            this.UpdateCheckboxes();
        }

        private void OnViewTools(MyGuiControlButton obj)
        {
            this.m_activeTabState = TabState.Tools;
            this.m_filteredSlot = MyGameInventoryItemSlot.Welder;
            this.m_selectedTool = string.Empty;
            this.RefreshUI();
        }

        private void OpenCurrentItemInStore()
        {
            MyGameInventoryItem currentItem = this.GetCurrentItem();
            if (currentItem != null)
            {
                MyGuiSandbox.OpenUrlWithFallback(string.Format(MySteamConstants.URL_INVENTORY_BUY_ITEM_FORMAT, MyGameService.AppId, currentItem.ItemDefinition.ID), "Buy Item", false);
            }
        }

        private void OpenUserInventory()
        {
            MyGuiSandbox.OpenUrlWithFallback(string.Format(MySteamConstants.URL_INVENTORY, MyGameService.UserId, MyGameService.AppId), "User Inventory", false);
        }

        public override void RecreateControls(bool constructor)
        {
            int? nullable3;
            base.RecreateControls(constructor);
            if (this.m_sliderHue != null)
            {
                this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            }
            if (this.m_sliderSaturation != null)
            {
                this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            }
            if (this.m_sliderValue != null)
            {
                this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            }
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0.056f, 0.072f), 0.5385f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0.056f, 0.147f), 0.5385f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0.056f, 0.228f), 0.5385f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0.056f, 0.548f), 0.5385f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0.056f, 0.629f), 0.5385f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0.056f, 0.778f), 0.5385f, 0f, color);
            this.Controls.Add(control);
            MyGuiControlStackPanel panel = new MyGuiControlStackPanel {
                BackgroundTexture = MyGuiConstants.TEXTURE_COMPOSITE_ROUND_ALL,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            };
            MyGuiControlStackPanel panel2 = new MyGuiControlStackPanel {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Margin = new Thickness(0.016f, 0.009f, 0.015f, 0.015f),
                Orientation = MyGuiOrientation.Horizontal
            };
            this.m_coloringButton = this.MakeButton(Vector2.Zero, MyCommonTexts.ScreenLoadInventoryColoring, MyCommonTexts.ScreenLoadInventoryColoringFilterTooltip, new Action<MyGuiControlButton>(this.OnViewTabColoring));
            this.m_coloringButton.VisualStyle = MyGuiControlButtonStyleEnum.ToolbarButton;
            this.m_coloringButton.Margin = new Thickness(0.0415f, 0.0285f, 0.0025f, 0f);
            if (this.m_activeLowTabState == LowerTabState.Coloring)
            {
                this.m_coloringButton.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_coloringButton.HasHighlight = true;
                this.m_coloringButton.Selected = true;
            }
            panel2.Add(this.m_coloringButton);
            this.Controls.Add(this.m_coloringButton);
            this.m_recyclingButton = this.MakeButton(Vector2.Zero, MyCommonTexts.ScreenLoadInventoryRecycling, MyCommonTexts.ScreenLoadInventoryRecyclingFilterTooltip, new Action<MyGuiControlButton>(this.OnViewTabRecycling));
            this.m_recyclingButton.VisualStyle = MyGuiControlButtonStyleEnum.ToolbarButton;
            this.m_recyclingButton.Margin = new Thickness(0.0025f, 0.0285f, 0.0025f, 0f);
            if (this.m_activeLowTabState == LowerTabState.Recycling)
            {
                this.m_recyclingButton.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_recyclingButton.HasHighlight = true;
                this.m_recyclingButton.Selected = true;
            }
            panel2.Add(this.m_recyclingButton);
            this.Controls.Add(this.m_recyclingButton);
            MyGuiControlStackPanel panel3 = new MyGuiControlStackPanel {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Margin = new Thickness(0.016f, 0f, 0.015f, 0.015f),
                Orientation = MyGuiOrientation.Horizontal
            };
            MyGuiControlImageButton.StateDefinition definition1 = new MyGuiControlImageButton.StateDefinition();
            definition1.Texture = MyGuiConstants.TEXTURE_BUTTON_SKINS_HIGHLIGHT;
            MyGuiControlImageButton.StyleDefinition definition4 = new MyGuiControlImageButton.StyleDefinition();
            definition4.Highlight = definition1;
            MyGuiControlImageButton.StateDefinition definition5 = new MyGuiControlImageButton.StateDefinition();
            definition5.Texture = MyGuiConstants.TEXTURE_BUTTON_SKINS_HIGHLIGHT;
            definition4.ActiveHighlight = definition5;
            MyGuiControlImageButton.StateDefinition definition6 = new MyGuiControlImageButton.StateDefinition();
            definition6.Texture = MyGuiConstants.TEXTURE_BUTTON_SKINS_NORMAL;
            definition4.Normal = definition6;
            MyGuiControlImageButton.StyleDefinition style = definition4;
            Vector2 size = new Vector2(0.14f, 0.05f);
            MyGuiControlImageButton.StateDefinition definition7 = new MyGuiControlImageButton.StateDefinition();
            definition7.Texture = MyGuiConstants.TEXTURE_BUTTON_SKINS_HIGHLIGHT;
            MyGuiControlImageButton.StyleDefinition definition8 = new MyGuiControlImageButton.StyleDefinition();
            definition8.Highlight = definition7;
            MyGuiControlImageButton.StateDefinition definition9 = new MyGuiControlImageButton.StateDefinition();
            definition9.Texture = MyGuiConstants.TEXTURE_BUTTON_SKINS_HIGHLIGHT;
            definition8.ActiveHighlight = definition9;
            MyGuiControlImageButton.StateDefinition definition10 = new MyGuiControlImageButton.StateDefinition();
            definition10.Texture = MyGuiConstants.TEXTURE_BUTTON_SKINS_NORMAL;
            definition8.Normal = definition10;
            MyGuiControlImageButton.StyleDefinition definition2 = definition8;
            this.m_characterButton = this.MakeButton(Vector2.Zero, MyCommonTexts.ScreenLoadInventoryCharacter, MyCommonTexts.ScreenLoadInventoryCharacterFilterTooltip, new Action<MyGuiControlButton>(this.OnViewTabCharacter));
            this.m_characterButton.VisualStyle = MyGuiControlButtonStyleEnum.ToolbarButton;
            this.m_characterButton.Margin = new Thickness(0.0415f, 0.0285f, 0.0025f, 0f);
            List<CategoryButton> list2 = new List<CategoryButton>();
            if (this.m_activeTabState == TabState.Character)
            {
                base.FocusedControl = this.m_characterButton;
                this.m_characterButton.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_characterButton.HasHighlight = true;
                this.m_characterButton.Selected = true;
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryHelmetTooltip, MyGameInventoryItemSlot.Helmet, @"Textures\GUI\Icons\Skins\Categories\helmet.png", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryHelmet)));
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventorySuitTooltip, MyGameInventoryItemSlot.Suit, @"Textures\GUI\Icons\Skins\Categories\suit.png", MyTexts.GetString(MyCommonTexts.ScreenLoadInventorySuit)));
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryGlovesTooltip, MyGameInventoryItemSlot.Gloves, @"Textures\GUI\Icons\Skins\Categories\glove.png", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryGloves)));
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryBootsTooltip, MyGameInventoryItemSlot.Boots, @"Textures\GUI\Icons\Skins\Categories\boot.png", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryBoots)));
            }
            panel3.Add(this.m_characterButton);
            this.Controls.Add(this.m_characterButton);
            this.m_toolsButton = this.MakeButton(Vector2.Zero, MyCommonTexts.ScreenLoadInventoryTools, MyCommonTexts.ScreenLoadInventoryToolsFilterTooltip, new Action<MyGuiControlButton>(this.OnViewTools));
            this.m_toolsButton.VisualStyle = MyGuiControlButtonStyleEnum.ToolbarButton;
            this.m_toolsButton.Margin = new Thickness(0.0025f, 0.0285f, 0f, 0f);
            if (this.m_activeTabState == TabState.Tools)
            {
                base.FocusedControl = this.m_toolsButton;
                this.m_toolsButton.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_toolsButton.HasHighlight = true;
                this.m_toolsButton.Selected = true;
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryWelderTooltip, MyGameInventoryItemSlot.Welder, @"Textures\GUI\Icons\WeaponWelder.dds", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryWelder)));
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryGrinderTooltip, MyGameInventoryItemSlot.Grinder, @"Textures\GUI\Icons\WeaponGrinder.dds", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryGrinder)));
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryDrillTooltip, MyGameInventoryItemSlot.Drill, @"Textures\GUI\Icons\WeaponDrill.dds", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryDrill)));
                list2.Add(new CategoryButton(MyCommonTexts.ScreenLoadInventoryRifleTooltip, MyGameInventoryItemSlot.Rifle, @"Textures\GUI\Icons\WeaponAutomaticRifle.dds", MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryRifle)));
            }
            panel3.Add(this.m_toolsButton);
            this.Controls.Add(this.m_toolsButton);
            MyGuiControlStackPanel panel4 = new MyGuiControlStackPanel {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Margin = new Thickness(0.055f, 0.05f, 0f, 0f),
                Orientation = MyGuiOrientation.Horizontal
            };
            foreach (CategoryButton button in list2)
            {
                MyGuiControlImageButton button2 = this.MakeImageButton(Vector2.Zero, size, style, button.Tooltip, new Action<MyGuiControlImageButton>(this.OnCategoryClicked));
                button2.UserData = button.Slot;
                button2.Margin = new Thickness(0f, 0f, 0.004f, 0f);
                panel4.Add(button2);
            }
            if (this.m_modelPicker != null)
            {
                this.m_modelPicker.ItemSelected -= new MyGuiControlCombobox.ItemSelectedDelegate(this.OnItemSelected);
            }
            MyGuiControlStackPanel panel5 = new MyGuiControlStackPanel {
                Orientation = MyGuiOrientation.Horizontal,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER,
                Margin = new Thickness(0.014f)
            };
            Vector2? position = null;
            position = null;
            color = null;
            MyGuiControlLabel label = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.PlayerCharacterModel), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                Margin = new Thickness(0.045f, -0.03f, 0.01f, 0f)
            };
            if (this.m_activeLowTabState == LowerTabState.Coloring)
            {
                this.m_modelPicker = new MyGuiControlCombobox();
                this.m_modelPicker.Size = new Vector2(0.225f, 1f);
                this.m_modelPicker.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_modelPicker.Margin = new Thickness(0.015f, -0.03f, 0.01f, 0f);
                this.m_modelPicker.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipCharacterScreen_Model));
                foreach (KeyValuePair<string, int> pair in this.m_displayModels)
                {
                    nullable3 = null;
                    this.m_modelPicker.AddItem((long) pair.Value, new StringBuilder(pair.Key), nullable3, null);
                }
                this.m_selectedModel = MySession.Static.LocalCharacter.ModelName;
                if (this.m_displayModels.ContainsKey(this.GetDisplayName(this.m_selectedModel)))
                {
                    this.m_modelPicker.SelectItemByKey((long) this.m_displayModels[this.GetDisplayName(this.m_selectedModel)], true);
                }
                else if (this.m_displayModels.Count > 0)
                {
                    this.m_modelPicker.SelectItemByKey((long) this.m_displayModels.First<KeyValuePair<string, int>>().Value, true);
                }
                this.m_modelPicker.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnItemSelected);
                if ((this.m_activeTabState == TabState.Character) || (this.m_activeTabState == TabState.Tools))
                {
                    panel5.Add(label);
                    panel5.Add(this.m_modelPicker);
                    this.Controls.Add(label);
                    this.Controls.Add(this.m_modelPicker);
                }
            }
            if ((this.m_activeTabState == TabState.Tools) && (this.m_filteredSlot != MyGameInventoryItemSlot.None))
            {
                this.m_toolPicker = new MyGuiControlCombobox();
                this.m_toolPicker.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_toolPicker.Margin = new Thickness(0.015f, 0.01f, 0.01f, 0.01f);
                foreach (MyPhysicalInventoryItem item in this.m_allTools)
                {
                    if (this.m_entityController.GetToolSlot(item.Content.SubtypeName) == this.m_filteredSlot)
                    {
                        MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Content);
                        if (physicalItemDefinition != null)
                        {
                            nullable3 = null;
                            this.m_toolPicker.AddItem((long) item.ItemId, new StringBuilder(physicalItemDefinition.DisplayNameText), nullable3, null);
                            continue;
                        }
                        nullable3 = null;
                        this.m_toolPicker.AddItem((long) item.ItemId, new StringBuilder(item.Content.SubtypeName), nullable3, null);
                    }
                }
                if (!string.IsNullOrEmpty(this.m_selectedTool))
                {
                    MyPhysicalInventoryItem item3 = this.m_allTools.FirstOrDefault<MyPhysicalInventoryItem>(t => t.Content.SubtypeName == this.m_selectedTool);
                    this.m_toolPicker.SelectItemByKey((long) item3.ItemId, true);
                }
                else if (this.m_toolPicker.GetItemsCount() > 0)
                {
                    this.m_toolPicker.SelectItemByIndex(0);
                    uint key = (uint) this.m_toolPicker.GetSelectedKey();
                    MyPhysicalInventoryItem item2 = this.m_allTools.FirstOrDefault<MyPhysicalInventoryItem>(t => t.ItemId == key);
                    if (item2.Content != null)
                    {
                        this.m_selectedTool = item2.Content.SubtypeName;
                    }
                }
                this.m_toolPicker.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_toolPicker_ItemSelected);
                position = null;
                position = null;
                color = null;
                new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryToolType), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER).Margin = new Thickness(0f, 0.01f, 0.01f, 0.01f);
            }
            this.m_itemsTableSize = new Vector2(0.582f, 0.29f);
            position = null;
            color = null;
            this.m_itemsTableParent = new MyGuiControlParent(position, new Vector2(this.m_itemsTableSize.X, 0.1f), color, null);
            this.m_itemsTableParent.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_itemsTableParent.SkipForMouseTest = true;
            MyGuiControlWrapPanel panel6 = this.CreateItemsTable(this.m_itemsTableParent);
            panel.Add(panel6);
            panel.Add(panel2);
            if (this.m_activeLowTabState == LowerTabState.Coloring)
            {
                position = null;
                color = null;
                MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    IsChecked = this.m_hideDuplicatesEnabled,
                    IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnHideDuplicates)
                };
                checkbox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipCharacterScreen_HideDuplicates));
                checkbox.Margin = new Thickness(0.039f, -0.03f, 0.01f, 0.01f);
                panel5.Add(checkbox);
                this.Controls.Add(checkbox);
                position = null;
                position = null;
                color = null;
                MyGuiControlLabel label3 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryHideDuplicates), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    Margin = new Thickness(0.005f, -0.03f, 0.01f, 0.01f)
                };
                panel5.Add(label3);
                this.Controls.Add(label3);
            }
            else
            {
                position = null;
                color = null;
                MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    IsChecked = this.m_showOnlyDuplicatesEnabled,
                    IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnShowOnlyDuplicates)
                };
                checkbox2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipCharacterScreen_ShowOnlyDuplicates));
                checkbox2.Margin = new Thickness(0.039f, -0.03f, 0.01f, 0.01f);
                panel5.Add(checkbox2);
                this.Controls.Add(checkbox2);
                position = null;
                position = null;
                color = null;
                MyGuiControlLabel label4 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryShowOnlyDuplicates), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    Margin = new Thickness(0.005f, -0.03f, 0.01f, 0.01f)
                };
                panel5.Add(label4);
                this.Controls.Add(label4);
                position = null;
                position = null;
                color = null;
                MyGuiControlLabel label5 = new MyGuiControlLabel(position, position, string.Format(MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryCurrencyCurrent), MyGameService.RecycleTokens), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    Margin = new Thickness(0.19f, -0.05f, 0.01f, 0.01f)
                };
                panel5.Add(label5);
                this.Controls.Add(label5);
            }
            MyGuiControlStackPanel panel7 = new MyGuiControlStackPanel {
                Orientation = MyGuiOrientation.Horizontal,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Margin = new Thickness(0.018f)
            };
            if (this.m_activeLowTabState == LowerTabState.Coloring)
            {
                bool flag = (this.m_OkButton == null) || ((this.m_OkButton != null) && this.m_OkButton.Enabled);
                this.m_OkButton = this.MakeButton(Vector2.Zero, MyCommonTexts.Ok, MyCommonTexts.ScreenLoadInventoryOkTooltip, new Action<MyGuiControlButton>(this.OnOkClick));
                this.m_OkButton.Enabled = flag;
                this.m_OkButton.Margin = new Thickness(0.0395f, -0.029f, 0.0075f, 0f);
                panel7.Add(this.m_OkButton);
            }
            else
            {
                this.m_craftButton = this.MakeButton(Vector2.Zero, MyCommonTexts.CraftButton, MyCommonTexts.ScreenLoadInventoryCraftTooltip, new Action<MyGuiControlButton>(this.OnCraftClick));
                this.m_craftButton.Enabled = true;
                this.m_craftButton.Margin = new Thickness(0.0395f, -0.029f, 0.0075f, 0f);
                panel7.Add(this.m_craftButton);
                uint craftingCost = MyGameService.GetCraftingCost(MyGameInventoryItemQuality.Common);
                this.m_craftButton.Text = string.Format(MyTexts.GetString(MyCommonTexts.CraftButton), craftingCost);
                this.m_craftButton.Enabled = MyGameService.RecycleTokens >= craftingCost;
            }
            this.m_cancelButton = this.MakeButton(Vector2.Zero, MyCommonTexts.Cancel, MyCommonTexts.ScreenLoadInventoryCancelTooltip, new Action<MyGuiControlButton>(this.OnCancelClick));
            this.m_cancelButton.Margin = new Thickness(0f, -0.029f, 0.0075f, 0.03f);
            panel7.Add(this.m_cancelButton);
            if (SKIN_STORE_FEATURES_ENABLED)
            {
                this.m_openStoreButton = this.MakeButton(Vector2.Zero, MyCommonTexts.ScreenLoadInventoryBrowseItems, MyCommonTexts.ScreenLoadInventoryBrowseItems, new Action<MyGuiControlButton>(this.OnOpenStore));
                panel7.Add(this.m_openStoreButton);
            }
            else
            {
                this.m_refreshButton = this.MakeButton(Vector2.Zero, MyCommonTexts.ScreenLoadSubscribedWorldRefresh, MyCommonTexts.ScreenLoadInventoryRefreshTooltip, new Action<MyGuiControlButton>(this.OnRefreshClick));
                this.m_refreshButton.Margin = new Thickness(0f, -0.029f, 0f, 0f);
                panel7.Add(this.m_refreshButton);
            }
            position = null;
            this.m_wheel = new MyGuiControlRotatingWheel(new Vector2?(Vector2.Zero), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.2f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, this.m_rotatingWheelTexture, false, MyPerGameSettings.GUI.MultipleSpinningWheels, position, 1.5f);
            this.m_wheel.ManualRotationUpdate = false;
            this.m_wheel.Margin = new Thickness(0.21f, 0.047f, 0f, 0f);
            base.Elements.Add(this.m_wheel);
            panel3.Add(this.m_wheel);
            MyGuiControlStackPanel panel8 = new MyGuiControlStackPanel {
                Orientation = MyGuiOrientation.Horizontal,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM
            };
            if (this.m_activeLowTabState != LowerTabState.Coloring)
            {
                this.CreateRecyclerUI(panel8);
            }
            else
            {
                position = null;
                float? defaultValue = null;
                color = null;
                this.m_sliderHue = new MyGuiControlSlider(position, 0f, 360f, 0.177f, defaultValue, color, null, 0, 0.8f, 0f, "White", string.Empty, MyGuiControlSliderStyleEnum.Hue, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true, false);
                this.m_sliderHue.Margin = new Thickness(0.055f, -0.0425f, 0f, 0f);
                this.m_sliderHue.Enabled = this.m_activeTabState == TabState.Character;
                panel8.Add(this.m_sliderHue);
                this.Controls.Add(this.m_sliderHue);
                position = null;
                color = null;
                this.m_sliderSaturation = new MyGuiControlSlider(position, 0f, 1f, 0.177f, 0f, color, null, 1, 0.8f, 0f, "White", string.Empty, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                this.m_sliderSaturation.Margin = new Thickness(0.0052f, -0.0425f, 0f, 0f);
                this.m_sliderSaturation.Enabled = this.m_activeTabState == TabState.Character;
                panel8.Add(this.m_sliderSaturation);
                this.Controls.Add(this.m_sliderSaturation);
                position = null;
                color = null;
                this.m_sliderValue = new MyGuiControlSlider(position, 0f, 1f, 0.177f, 0f, color, null, 1, 0.8f, 0f, "White", string.Empty, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                this.m_sliderValue.Margin = new Thickness(0.006f, -0.0425f, 0f, 0f);
                this.m_sliderValue.Enabled = this.m_activeTabState == TabState.Character;
                panel8.Add(this.m_sliderValue);
                this.Controls.Add(this.m_sliderValue);
            }
            GridLength[] columns = new GridLength[] { new GridLength(1f, GridUnitType.Ratio) };
            GridLength[] rows = new GridLength[] { new GridLength(0.6f, GridUnitType.Ratio), new GridLength(0.5f, GridUnitType.Ratio), new GridLength(0.8f, GridUnitType.Ratio), new GridLength(4.6f, GridUnitType.Ratio), new GridLength(0.6f, GridUnitType.Ratio), new GridLength(0.6f, GridUnitType.Ratio), new GridLength(0.8f, GridUnitType.Ratio) };
            MyGuiControlLayoutGrid grid = new MyGuiControlLayoutGrid(columns, rows) {
                Size = new Vector2(0.65f, 0.9f)
            };
            grid.Add(panel3, 0, 1);
            grid.Add(panel4, 0, 2);
            if ((MyGameService.InventoryItems != null) && (MyGameService.InventoryItems.Count == 0))
            {
                position = null;
                position = null;
                color = null;
                MyGuiControlLabel label6 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryNoItem), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    Margin = new Thickness(0.015f)
                };
                grid.Add(label6, 0, 3);
                base.Elements.Add(label6);
            }
            grid.Add(panel, 0, 3);
            grid.Add(panel5, 0, 4);
            grid.Add(panel8, 0, 5);
            grid.Add(panel7, 0, 6);
            position = null;
            position = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenCaptionInventory), new VRageMath.Vector4?(VRageMath.Vector4.One), 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP) {
                Name = "CaptionLabel",
                Font = "ScreenCaption"
            };
            base.Elements.Add(label2);
            grid.Add(label2, 0, 0);
            GridLength[] lengthArray6 = new GridLength[] { new GridLength(1f, GridUnitType.Ratio) };
            GridLength[] lengthArray7 = new GridLength[] { new GridLength(1f, GridUnitType.Ratio) };
            MyGuiControlLayoutGrid grid1 = new MyGuiControlLayoutGrid(lengthArray6, lengthArray7);
            grid1.Size = new Vector2(1f, 1f);
            grid1.Add(grid, 0, 0);
            grid1.UpdateMeasure();
            this.m_itemsTableParent.Size = new Vector2(this.m_itemsTableSize.X, panel6.Size.Y);
            panel6.Size = this.m_itemsTableSize;
            grid1.UpdateArrange();
            this.m_itemsTableParent.Position = panel6.Position;
            MyGuiControlScrollablePanel panel9 = new MyGuiControlScrollablePanel(this.m_itemsTableParent) {
                ContentOffset = new Vector2(-this.m_itemsTableSize.X, -this.m_itemsTableParent.Size.Y - 0.575f) / 2f,
                ScrollBarOffset = new Vector2(0.005f, 0f),
                ScrollbarVEnabled = true,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Size = this.m_itemsTableSize,
                Position = panel6.Position
            };
            this.Controls.Add(panel9);
            position = base.Size;
            label2.Position = new Vector2(label2.Position.X + (position.Value.X / 2f), (label2.Position.Y + (MyGuiConstants.SCREEN_CAPTION_DELTA_Y / 3f)) + 0.023f);
            foreach (MyGuiControlBase base2 in panel7.GetControls(true))
            {
                this.Controls.Add(base2);
            }
            List<MyGuiControlBase> controls = panel4.GetControls(true);
            for (int i = 0; i < controls.Count; i++)
            {
                MyGuiControlImageButton button3 = controls[i] as MyGuiControlImageButton;
                this.Controls.Add(button3);
                if ((this.m_filteredSlot != MyGameInventoryItemSlot.None) && (this.m_filteredSlot == ((MyGameInventoryItemSlot) button3.UserData)))
                {
                    button3.ApplyStyle(definition2);
                    button3.HighlightType = MyGuiControlHighlightType.FORCED;
                    button3.HasHighlight = true;
                    button3.Selected = true;
                }
                button3.Size = new Vector2(0.1f, 0.1f);
                float x = button3.Position.X;
                if (!string.IsNullOrEmpty(list2[i].ImageName))
                {
                    position = new Vector2(0.03f, 0.04f);
                    color = null;
                    string[] textures = new string[] { list2[i].ImageName };
                    MyGuiControlImage image = new MyGuiControlImage(new Vector2?(button3.Position + new Vector2(0.005f, 0.001f)), position, color, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                    this.Controls.Add(image);
                    x += image.Size.X;
                }
                if (!string.IsNullOrEmpty(list2[i].ButtonText))
                {
                    position = new Vector2?(button3.Size);
                    color = null;
                    MyGuiControlLabel label7 = new MyGuiControlLabel(new Vector2(((x + button3.Position.X) + button3.Size.X) / 2f, button3.Position.Y + (button3.Size.Y / 2.18f)), position, list2[i].ButtonText, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    this.Controls.Add(label7);
                }
            }
            this.m_originalSpectatorPosition = MySpectatorCameraController.Static.Position;
            this.m_wheel.Visible = false;
            base.CloseButtonEnabled = true;
            this.m_storedHSV = MySession.Static.LocalCharacter.ColorMask;
            this.m_selectedHSV = this.m_storedHSV;
            this.m_sliderHue.Value = this.m_selectedHSV.X * 360f;
            this.m_sliderSaturation.Value = MathHelper.Clamp((float) (this.m_selectedHSV.Y + MyColorPickerConstants.SATURATION_DELTA), (float) 0f, (float) 1f);
            this.m_sliderValue.Value = MathHelper.Clamp((float) ((this.m_selectedHSV.Z + MyColorPickerConstants.VALUE_DELTA) - MyColorPickerConstants.VALUE_COLORIZE_DELTA), (float) 0f, (float) 1f);
            this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_contextMenu = new MyGuiControlContextMenu();
            this.m_contextMenu.CreateNewContextMenu();
            if (SKIN_STORE_FEATURES_ENABLED && MyGameService.IsOverlayEnabled)
            {
                StringBuilder builder3 = MyTexts.Get(MyCommonTexts.ScreenLoadInventoryBuyItem);
                this.m_contextMenu.AddItem(builder3, builder3.ToString(), "", InventoryItemAction.Buy);
            }
            StringBuilder text = MyTexts.Get(MyCommonTexts.ScreenLoadInventorySellItem);
            if (MyGameService.IsOverlayEnabled)
            {
                this.m_contextMenu.AddItem(text, text.ToString(), "", InventoryItemAction.Sell);
            }
            StringBuilder builder2 = MyTexts.Get(MyCommonTexts.ScreenLoadInventoryRecycleItem);
            string.Format(builder2.ToString(), 0);
            this.m_contextMenu.AddItem(builder2, string.Empty, "", InventoryItemAction.Recycle);
            this.m_contextMenu.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.m_contextMenu_ItemClicked);
            this.Controls.Add(this.m_contextMenu);
            this.m_contextMenu.Deactivate();
            if (constructor)
            {
                this.m_colorOrModelChanged = false;
            }
            this.UpdateSliderTooltips();
        }

        private void RecycleItemRequest()
        {
            if (this.GetCurrentItem() != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.ScreenLoadInventoryRecycleItemMessageTitle);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.ScreenLoadInventoryRecycleItemMessageText), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnRecycleItem), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void RefreshItems()
        {
            if (MyGameService.IsActive)
            {
                MyGameService.GetAllInventoryItems();
            }
        }

        private void RefreshUI()
        {
            this.RecreateControls(false);
            this.EquipTool();
            this.UpdateCheckboxes();
        }

        private void RemoveItemFromUI(MyGameInventoryItem item)
        {
            MyAssetModifierComponent component;
            MyGuiControlLayoutGrid userData = this.m_contextMenuLastButton.UserData as MyGuiControlLayoutGrid;
            if (userData != null)
            {
                foreach (MyGuiControlBase local1 in userData.GetAllControls())
                {
                    local1.Visible = false;
                    local1.Enabled = false;
                }
            }
            this.m_contextMenuLastButton = null;
            if (((MySession.Static.LocalCharacter != null) && item.IsInUse) && MySession.Static.LocalCharacter.Components.TryGet<MyAssetModifierComponent>(out component))
            {
                component.ResetSlot(item.ItemDefinition.ItemSlot);
            }
            MyLocalCache.SaveInventoryConfig(MySession.Static.LocalCharacter);
        }

        public static void ResetOnFinish(string model, bool resetToDefault)
        {
            MyGuiScreenLoadInventory firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenLoadInventory>();
            if ((firstScreenOfType != null) && (firstScreenOfType.m_selectedModel != MySession.Static.LocalCharacter.ModelName))
            {
                if (resetToDefault)
                {
                    firstScreenOfType.ResetOnFinishInternal(model);
                }
                firstScreenOfType.RecreateControls(false);
                MyLocalCache.ResetAllInventorySlots(MySession.Static.LocalCharacter);
                firstScreenOfType.RefreshItems();
            }
        }

        private void ResetOnFinishInternal(string model)
        {
            if ((model == "Default_Astronaut") || (model == "Default_Astronaut_Female"))
            {
                MyLocalCache.LoadInventoryConfig(MySession.Static.LocalCharacter, false);
            }
            else
            {
                int selectedKey = (int) this.m_modelPicker.GetSelectedKey();
                this.m_selectedModel = this.m_models[selectedKey];
                Cancel();
                this.RecreateControls(false);
                MyLocalCache.ResetAllInventorySlots(MySession.Static.LocalCharacter);
                this.RefreshItems();
            }
        }

        private void RotatingWheelHide()
        {
            this.m_wheel.ManualRotationUpdate = false;
            this.m_wheel.Visible = false;
        }

        private void RotatingWheelShow()
        {
            this.m_wheel.ManualRotationUpdate = true;
            this.m_wheel.Visible = true;
        }

        private static void SetAudioVolumes()
        {
            MyAudio.Static.StopMusic();
            MyAudio.Static.ChangeGlobalVolume(1f, 5f);
            if ((MyPerGameSettings.UseMusicController && (MyFakes.ENABLE_MUSIC_CONTROLLER && (MySandboxGame.Config.EnableDynamicMusic && !Sandbox.Engine.Platform.Game.IsDedicated))) && (MyMusicController.Static == null))
            {
                MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
            }
            MyAudio.Static.MusicAllowed = ReferenceEquals(MyMusicController.Static, null);
            if (MyMusicController.Static != null)
            {
                MyMusicController.Static.Active = true;
            }
            else
            {
                MyMusicTrack track = new MyMusicTrack {
                    TransitionCategory = MyStringId.GetOrCompute("Default")
                };
                MyAudio.Static.PlayMusic(new MyMusicTrack?(track), 0);
            }
        }

        public override bool Update(bool hasFocus)
        {
            if ((base.State != MyGuiScreenState.CLOSING) && (base.State != MyGuiScreenState.CLOSED))
            {
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
            }
            if ((MyInput.Static.IsNewPrimaryButtonPressed() && this.m_contextMenu.IsActiveControl) && !this.m_contextMenu.IsMouseOver)
            {
                this.m_contextMenu.Deactivate();
            }
            base.Update(hasFocus);
            if ((!this.m_audioSet && (MySandboxGame.IsGameReady && ((MyAudio.Static != null) && (MyRenderProxy.VisibleObjectsRead != null)))) && (MyRenderProxy.VisibleObjectsRead.Count > 0))
            {
                SetAudioVolumes();
                this.m_audioSet = true;
            }
            return true;
        }

        private void UpdateCheckboxes()
        {
            MyAssetModifierComponent component;
            if (MySession.Static.LocalCharacter.Components.TryGet<MyAssetModifierComponent>(out component))
            {
                foreach (MyGuiControlCheckbox checkbox in this.m_itemCheckboxes)
                {
                    MyGameInventoryItem userData = checkbox.UserData as MyGameInventoryItem;
                    if (userData != null)
                    {
                        checkbox.IsCheckedChanged = null;
                        checkbox.IsChecked = userData.IsInUse;
                        checkbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnItemCheckChanged);
                    }
                }
            }
        }

        private void UpdateOKButton()
        {
            bool flag = true;
            foreach (MyGameInventoryItem item in this.m_userItems)
            {
                if (item.IsInUse)
                {
                    flag &= !item.IsStoreFakeItem;
                }
            }
            this.m_OkButton.Enabled = flag;
        }

        private void UpdateSliderTooltips()
        {
            this.m_sliderHue.Tooltips.ToolTips.Clear();
            this.m_sliderHue.Tooltips.AddToolTip(string.Format(MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryHue), this.m_sliderHue.Value), 0.7f, "Blue");
            this.m_sliderSaturation.Tooltips.ToolTips.Clear();
            this.m_sliderSaturation.Tooltips.AddToolTip(string.Format(MyTexts.GetString(MyCommonTexts.ScreenLoadInventorySaturation), this.m_sliderSaturation.Value.ToString("P1")), 0.7f, "Blue");
            this.m_sliderValue.Tooltips.ToolTips.Clear();
            this.m_sliderValue.Tooltips.AddToolTip(string.Format(MyTexts.GetString(MyCommonTexts.ScreenLoadInventoryValue), this.m_sliderValue.Value.ToString("P1")), 0.7f, "Blue");
        }

        private void UseItem(MyGameInventoryItem item, byte[] checkData)
        {
            if (MySession.Static.LocalCharacter != null)
            {
                MyAssetModifierComponent component;
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                item.IsNew = false;
                string assetModifierId = item.ItemDefinition.AssetModifierId;
                this.m_colorOrModelChanged = true;
                MyGameInventoryItemSlot itemSlot = item.ItemDefinition.ItemSlot;
                if ((itemSlot - 1) <= MyGameInventoryItemSlot.Boots)
                {
                    if (localCharacter.Components.TryGet<MyAssetModifierComponent>(out component) && component.TryAddAssetModifier(checkData))
                    {
                        item.IsInUse = true;
                        this.m_entityController.PlayRandomCharacterAnimation();
                    }
                }
                else if ((itemSlot - 6) <= MyGameInventoryItemSlot.Gloves)
                {
                    MyEntity currentWeapon = localCharacter.CurrentWeapon as MyEntity;
                    if (((currentWeapon != null) && currentWeapon.Components.TryGet<MyAssetModifierComponent>(out component)) && component.TryAddAssetModifier(checkData))
                    {
                        item.IsInUse = true;
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenLoadInventory.<>c <>9 = new MyGuiScreenLoadInventory.<>c();
            public static Func<MyGameInventoryItem, string> <>9__94_0;
            public static Func<MyGameInventoryItem, bool> <>9__95_0;
            public static Func<MyGameInventoryItem, bool> <>9__95_1;
            public static Func<MyGameInventoryItem, bool> <>9__95_2;
            public static Func<MyGameInventoryItem, bool> <>9__95_3;
            public static Func<MyGameInventoryItem, string> <>9__95_4;
            public static Func<MyGameInventoryItem, bool> <>9__95_5;
            public static Func<MyGameInventoryItem, bool> <>9__95_6;
            public static Func<MyGameInventoryItem, string> <>9__95_7;
            public static Func<MyGameInventoryItem, bool> <>9__95_13;
            public static Func<MyGameInventoryItem, bool> <>9__95_14;
            public static Func<MyGameInventoryItem, string> <>9__95_15;
            public static Func<MyGameInventoryItem, bool> <>9__95_8;
            public static Func<MyGameInventoryItem, bool> <>9__95_9;
            public static Func<MyGameInventoryItem, bool> <>9__95_10;
            public static Func<MyGameInventoryItem, string> <>9__95_11;
            public static Func<MyGuiControlListbox.Item, bool> <>9__97_0;

            internal bool <GetInventoryItems>b__95_0(MyGameInventoryItem i) => 
                i.IsNew;

            internal bool <GetInventoryItems>b__95_1(MyGameInventoryItem i) => 
                i.IsInUse;

            internal bool <GetInventoryItems>b__95_10(MyGameInventoryItem i) => 
                i.IsInUse;

            internal string <GetInventoryItems>b__95_11(MyGameInventoryItem i) => 
                i.ItemDefinition.Name;

            internal bool <GetInventoryItems>b__95_13(MyGameInventoryItem i) => 
                i.IsNew;

            internal bool <GetInventoryItems>b__95_14(MyGameInventoryItem i) => 
                i.IsInUse;

            internal string <GetInventoryItems>b__95_15(MyGameInventoryItem i) => 
                i.ItemDefinition.Name;

            internal bool <GetInventoryItems>b__95_2(MyGameInventoryItem i) => 
                i.IsNew;

            internal bool <GetInventoryItems>b__95_3(MyGameInventoryItem i) => 
                i.IsInUse;

            internal string <GetInventoryItems>b__95_4(MyGameInventoryItem i) => 
                i.ItemDefinition.Name;

            internal bool <GetInventoryItems>b__95_5(MyGameInventoryItem i) => 
                i.IsNew;

            internal bool <GetInventoryItems>b__95_6(MyGameInventoryItem i) => 
                i.IsInUse;

            internal string <GetInventoryItems>b__95_7(MyGameInventoryItem i) => 
                i.ItemDefinition.Name;

            internal bool <GetInventoryItems>b__95_8(MyGameInventoryItem i) => 
                !i.IsInUse;

            internal bool <GetInventoryItems>b__95_9(MyGameInventoryItem i) => 
                i.IsNew;

            internal string <GetStoreItems>b__94_0(MyGameInventoryItem i) => 
                i.ItemDefinition.Name;

            internal bool <hiddenButton_ButtonRightClicked>b__97_0(MyGuiControlListbox.Item i) => 
                ((i.UserData != null) && (((MyGuiScreenLoadInventory.InventoryItemAction) i.UserData) == MyGuiScreenLoadInventory.InventoryItemAction.Recycle));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CategoryButton
        {
            public MyStringId Tooltip;
            public MyGameInventoryItemSlot Slot;
            public string ImageName;
            public string ButtonText;
            public CategoryButton(MyStringId tooltip, MyGameInventoryItemSlot slot, string imageName = null, string buttonText = null)
            {
                this.Tooltip = tooltip;
                this.Slot = slot;
                this.ImageName = imageName;
                this.ButtonText = buttonText;
            }
        }

        private enum InventoryItemAction
        {
            Apply,
            Sell,
            Trade,
            Recycle,
            Delete,
            Buy
        }

        private enum LowerTabState
        {
            Coloring,
            Recycling
        }

        private enum TabState
        {
            Character,
            Tools
        }
    }
}

