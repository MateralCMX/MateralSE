namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenColorPicker : MyGuiScreenBase
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.37f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private float m_textScale;
        private Vector2 m_controlPadding;
        private MyGuiControlButton m_closeButton;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_defaultsButton;
        private MyGuiControlSlider m_hueSlider;
        private MyGuiControlSlider m_saturationSlider;
        private MyGuiControlSlider m_valueSlider;
        private MyGuiControlLabel m_hueLabel;
        private MyGuiControlLabel m_saturationLabel;
        private MyGuiControlLabel m_valueLabel;
        private MyGuiControlPanel m_colorVariantPanel;
        private List<Vector3> m_oldPaletteList;
        private MyGuiControlPanel m_highlightControlPanel;
        private List<MyGuiControlPanel> m_colorPaletteControlsList;
        private const int x = -170;
        private const int y = -250;
        private const int defColLine = -230;
        private const int defColCol = -42;
        private const string m_hueScaleTexture = @"Textures\GUI\HueScale.png";

        public MyGuiScreenColorPicker() : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), new Vector2?(SCREEN_SIZE), false, null, 0f, 0f)
        {
            this.m_textScale = 0.8f;
            this.m_controlPadding = new Vector2(0.02f, 0.02f);
            this.m_colorPaletteControlsList = new List<MyGuiControlPanel>();
            base.CanHideOthers = false;
            this.RecreateControls(true);
            this.m_oldPaletteList = new List<Vector3>();
            foreach (Vector3 vector in MySession.Static.LocalHumanPlayer.BuildColorSlots)
            {
                this.m_oldPaletteList.Add(vector);
            }
            this.UpdateSliders(MyPlayer.SelectedColor);
            this.UpdateLabels();
            if (MyGuiScreenHudSpace.Static != null)
            {
                MyGuiScreenHudSpace.Static.HideScreen();
            }
        }

        private MyGuiControlButton CreateButton(float usableWidth, StringBuilder text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?(), float textScale = 1f)
        {
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            Action<MyGuiControlButton> onButtonClick = onClick;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, textScale, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onButtonClick, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                Size = new Vector2(usableWidth, 0.034f)
            };
            button.Position += new Vector2(-0.02f, 0f);
            if (tooltip != null)
            {
                button.SetToolTip(tooltip.Value);
            }
            return button;
        }

        public override string GetFriendlyName() => 
            "ColorPick";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.LANDING_GEAR))
            {
                this.CloseScreenNow();
            }
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if ((localHumanPlayer != null) && (MyInput.Static.IsNewLeftMousePressed() || MyControllerHelper.IsControl(MySpaceBindingCreator.CX_GUI, MyControlsGUI.ACCEPT, MyControlStateType.NEW_PRESSED, false)))
            {
                for (int i = 0; i < this.m_colorPaletteControlsList.Count; i++)
                {
                    if (this.m_colorPaletteControlsList[i].IsMouseOver)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                        localHumanPlayer.SelectedBuildColorSlot = i;
                        this.m_highlightControlPanel.Position = new Vector2(-0.1325f + ((localHumanPlayer.SelectedBuildColorSlot % 7) * 0.038f), -0.4f + ((localHumanPlayer.SelectedBuildColorSlot / 7) * 0.035f));
                        this.UpdateSliders(localHumanPlayer.SelectedBuildColor);
                    }
                }
            }
            base.HandleInput(receivedFocusInThisUpdate);
        }

        private void OnCancelClick(MyGuiControlButton sender)
        {
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if (localHumanPlayer != null)
            {
                localHumanPlayer.SetBuildColorSlots(this.m_oldPaletteList);
            }
            this.CloseScreenNow();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
            this.OnSetVisible(false);
        }

        private void OnDefaultsClick(MyGuiControlButton sender)
        {
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if (localHumanPlayer != null)
            {
                localHumanPlayer.SetDefaultColors();
                Color white = Color.White;
                for (int i = 0; i < 14; i++)
                {
                    this.m_colorPaletteControlsList[i].ColorMask = this.prev(localHumanPlayer.BuildColorSlots[i]).HSVtoColor().ToVector4();
                }
                this.UpdateSliders(localHumanPlayer.SelectedBuildColor);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            this.OnSetVisible(false);
        }

        private void OnOkClick(MyGuiControlButton sender)
        {
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if (localHumanPlayer != null)
            {
                bool flag = false;
                int num = 0;
                foreach (Vector3 vector in localHumanPlayer.BuildColorSlots)
                {
                    if (this.m_oldPaletteList[num] != vector)
                    {
                        flag = true;
                        this.m_oldPaletteList[num] = vector;
                    }
                    num++;
                }
                if (flag)
                {
                    Sync.Players.RequestPlayerColorsChanged(localHumanPlayer.Id.SerialId, this.m_oldPaletteList);
                }
            }
            this.CloseScreenNow();
        }

        private void OnSetVisible(bool visible)
        {
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.UseTransparency = !visible;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            this.OnSetVisible(true);
        }

        private void OnValueChange(MyGuiControlSlider sender)
        {
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if (localHumanPlayer != null)
            {
                this.UpdateLabels();
                float y = this.m_saturationSlider.Value;
                float z = this.m_valueSlider.Value;
                this.m_colorPaletteControlsList[localHumanPlayer.SelectedBuildColorSlot].ColorMask = new Vector3(this.m_hueSlider.Value / 360f, y, z).HSVtoColor().ToVector4();
                float num3 = y - MyColorPickerConstants.SATURATION_DELTA;
                localHumanPlayer.SelectedBuildColor = new Vector3(this.m_hueSlider.Value / 360f, num3, (z - MyColorPickerConstants.VALUE_DELTA) + MyColorPickerConstants.VALUE_COLORIZE_DELTA);
            }
        }

        private Vector3 prev(Vector3 HSV) => 
            new Vector3(HSV.X, MathHelper.Clamp((float) (HSV.Y + MyColorPickerConstants.SATURATION_DELTA), (float) 0f, (float) 1f), MathHelper.Clamp((float) ((HSV.Z + MyColorPickerConstants.VALUE_DELTA) - MyColorPickerConstants.VALUE_COLORIZE_DELTA), (float) 0f, (float) 1f));

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            float num = (SCREEN_SIZE.Y - 1f) / 2f;
            base.AddCaption(MyTexts.Get(MyCommonTexts.ColorPicker).ToString(), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(this.m_controlPadding + new Vector2(-HIDDEN_PART_RIGHT, num - 0.03f)), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? nullable = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.44f), base.m_size.Value.X * 0.73f, 0f, nullable);
            nullable = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.326f), base.m_size.Value.X * 0.73f, 0f, nullable);
            nullable = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.045f), base.m_size.Value.X * 0.73f, 0f, nullable);
            nullable = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.352f), base.m_size.Value.X * 0.73f, 0f, nullable);
            this.Controls.Add(control);
            Color white = Color.White;
            int num2 = 0;
            Vector2? size = new Vector2(0.04f, 0.035f);
            nullable = null;
            this.m_highlightControlPanel = new MyGuiControlPanel(new Vector2(-0.1325f + ((MyPlayer.SelectedColorSlot % 7) * 0.038f), -0.4f + ((MyPlayer.SelectedColorSlot / 7) * 0.035f)), size, nullable, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_highlightControlPanel.ColorMask = white.ToVector4();
            this.m_highlightControlPanel.BackgroundTexture = MyGuiConstants.TEXTURE_GUI_BLANK;
            this.Controls.Add(this.m_highlightControlPanel);
            int index = 0;
            while (index < 14)
            {
                size = new Vector2(0.034f, 0.03f);
                nullable = null;
                MyGuiControlPanel item = new MyGuiControlPanel(new Vector2(-0.1325f + ((index % 7) * 0.038f), -0.4f + (num2 * 0.035f)), size, nullable, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                ListReader<Vector3> colorSlots = MyPlayer.ColorSlots;
                item.ColorMask = this.prev(colorSlots.ItemAt(index)).HSVtoColor().ToVector4();
                item.BackgroundTexture = MyGuiConstants.TEXTURE_GUI_BLANK;
                this.m_colorPaletteControlsList.Add(item);
                this.Controls.Add(item);
                index++;
                if ((index % 7) == 0)
                {
                    num2++;
                }
            }
            size = null;
            nullable = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.153125f, -0.2958333f), size, MyTexts.Get(MyCommonTexts.Hue).ToString(), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            size = null;
            nullable = null;
            this.m_hueLabel = new MyGuiControlLabel(new Vector2(0.11375f, -0.2958333f), size, string.Empty, nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_hueLabel);
            float? defaultValue = null;
            nullable = null;
            this.m_hueSlider = new MyGuiControlSlider(new Vector2(-0.15375f, -0.2541667f), 0f, 360f, 0.312f, defaultValue, nullable, string.Empty, 0, 0.8f, 0.04166667f, "White", null, MyGuiControlSliderStyleEnum.Hue, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, false);
            this.m_hueSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_hueSlider.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.Controls.Add(this.m_hueSlider);
            size = null;
            nullable = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.153125f, -0.2125f), size, MyTexts.Get(MyCommonTexts.Saturation).ToString(), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            size = null;
            nullable = null;
            this.m_saturationLabel = new MyGuiControlLabel(new Vector2(0.11375f, -0.2125f), size, string.Empty, nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_saturationLabel);
            nullable = null;
            this.m_saturationSlider = new MyGuiControlSlider(new Vector2(-0.15375f, -0.1716667f), 0f, 1f, 0.312f, 0f, nullable, string.Empty, 1, 0.8f, 0.04166667f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, false);
            this.m_saturationSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_saturationSlider.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.Controls.Add(this.m_saturationSlider);
            size = null;
            nullable = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.153125f, -0.1291667f), size, MyTexts.Get(MyCommonTexts.Value).ToString(), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            size = null;
            nullable = null;
            this.m_valueLabel = new MyGuiControlLabel(new Vector2(0.11375f, -0.1291667f), size, string.Empty, nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_valueLabel);
            nullable = null;
            this.m_valueSlider = new MyGuiControlSlider(new Vector2(-0.15375f, -0.08833333f), 0f, 1f, 0.312f, 0f, nullable, string.Empty, 1, 0.8f, 0.04166667f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, false);
            this.m_valueSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_valueSlider.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.Controls.Add(this.m_valueSlider);
            StringBuilder output = null;
            MyInput.Static.GetGameControl(MyControlsSpace.CUBE_COLOR_CHANGE).AppendBoundButtonNames(ref output, ", ", MyInput.Static.GetUnassignedName(), true);
            nullable = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(-0.15f, -0.02333333f), new Vector2(0.28f, 0.355f), nullable, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ColorPicker_Description), output))
            };
            this.Controls.Add(text);
            Vector2 vector = new Vector2(-0.083f, 0.36f);
            Vector2 vector2 = new Vector2(0.134f, 0.038f);
            float usableWidth = 0.131f;
            float num4 = 0.265f;
            float textScale = this.m_textScale;
            this.m_defaultsButton = this.CreateButton(num4, MyTexts.Get(MyCommonTexts.Defaults), new Action<MyGuiControlButton>(this.OnDefaultsClick), true, new MyStringId?(MySpaceTexts.ToolTipOptionsControls_Defaults), textScale);
            this.m_defaultsButton.Position = vector + (new Vector2(0f, 1f) * vector2);
            this.m_defaultsButton.PositionX += vector2.X / 2f;
            this.Controls.Add(this.m_defaultsButton);
            textScale = this.m_textScale;
            this.m_okButton = this.CreateButton(usableWidth, MyTexts.Get(MyCommonTexts.Ok), new Action<MyGuiControlButton>(this.OnOkClick), true, new MyStringId?(MySpaceTexts.ToolTipOptionsSpace_Ok), textScale);
            this.m_okButton.Position = vector + (new Vector2(0.005f, 2f) * vector2);
            this.m_okButton.ShowTooltipWhenDisabled = true;
            this.Controls.Add(this.m_okButton);
            textScale = this.m_textScale;
            this.m_closeButton = this.CreateButton(usableWidth, MyTexts.Get(MyCommonTexts.Close), new Action<MyGuiControlButton>(this.OnCancelClick), true, new MyStringId?(MySpaceTexts.ToolTipNewsletter_Close), textScale);
            this.m_closeButton.Position = vector + (new Vector2(1f, 2f) * vector2);
            this.m_closeButton.ShowTooltipWhenDisabled = true;
            this.Controls.Add(this.m_closeButton);
        }

        private void UpdateLabels()
        {
            this.m_hueLabel.Text = this.m_hueSlider.Value.ToString("F1") + "\x00b0";
            this.m_saturationLabel.Text = this.m_saturationSlider.Value.ToString("P1");
            this.m_valueLabel.Text = this.m_valueSlider.Value.ToString("P1");
        }

        private void UpdateSliders(Vector3 colorValue)
        {
            this.m_hueSlider.Value = colorValue.X * 360f;
            this.m_saturationSlider.Value = MathHelper.Clamp((float) (colorValue.Y + MyColorPickerConstants.SATURATION_DELTA), (float) 0f, (float) 1f);
            this.m_valueSlider.Value = MathHelper.Clamp((float) ((colorValue.Z + MyColorPickerConstants.VALUE_DELTA) - MyColorPickerConstants.VALUE_COLORIZE_DELTA), (float) 0f, (float) 1f);
        }
    }
}

