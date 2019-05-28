namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenVoxelHandSetting : MyGuiScreenBase
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.37f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private Vector2 m_controlPadding;
        private MyGuiControlLabel m_labelSettings;
        private MyGuiControlLabel m_labelSnapToVoxel;
        private MyGuiControlCheckbox m_checkSnapToVoxel;
        private MyGuiControlLabel m_labelProjectToVoxel;
        private MyGuiControlCheckbox m_projectToVoxel;
        private MyGuiControlLabel m_labelFreezePhysics;
        private MyGuiControlCheckbox m_freezePhysicsCheck;
        private MyGuiControlLabel m_labelShowGizmos;
        private MyGuiControlCheckbox m_showGizmos;
        private MyGuiControlLabel m_labelTransparency;
        private MyGuiControlSlider m_sliderTransparency;
        private MyGuiControlLabel m_labelZoom;
        private MyGuiControlSlider m_sliderZoom;
        private MyGuiControlVoxelHandSettings m_voxelControl;

        public MyGuiScreenVoxelHandSetting() : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), new Vector2?(SCREEN_SIZE), false, null, 0f, 0f)
        {
            this.m_controlPadding = new Vector2(0.02f, 0.02f);
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = false;
            this.RecreateControls(true);
        }

        private void BrushTransparency_ValueChanged(MyGuiControlSlider sender)
        {
            MySessionComponentVoxelHand.Static.ShapeColor.A = (byte) ((1f - this.m_sliderTransparency.Value) * 255f);
        }

        private void BrushZoom_ValueChanged(MyGuiControlSlider sender)
        {
            MySessionComponentVoxelHand.Static.SetBrushZoom(this.m_sliderZoom.Value);
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

        private void FreezePhysics_Changed(MyGuiControlCheckbox sender)
        {
            MySessionComponentVoxelHand.Static.FreezePhysics = sender.IsChecked;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenVoxelHandSetting";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.VOXEL_HAND_SETTINGS))
            {
                this.CloseScreen();
            }
            base.HandleInput(receivedFocusInThisUpdate);
        }

        private void OKButtonClicked(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void ProjectToVoxel_Changed(MyGuiControlCheckbox sender)
        {
            MySessionComponentVoxelHand.Static.ProjectToVoxel = this.m_projectToVoxel.IsChecked;
            this.m_sliderZoom.Enabled = !this.m_projectToVoxel.IsChecked;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            float y = -0.465f;
            float num2 = (SCREEN_SIZE.Y - 1f) / 2f;
            base.AddCaption(MyTexts.Get(MyCommonTexts.VoxelHandSettingScreen_Caption).ToString(), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(this.m_controlPadding + new Vector2(-HIDDEN_PART_RIGHT, num2 - 0.03f)), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.44f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.048f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.235f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.394f), base.m_size.Value.X * 0.73f, 0f, color);
            this.Controls.Add(control);
            y += 0.042f;
            Vector2? position = null;
            color = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox1.Position = new Vector2(0.12f, y);
            checkbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_checkSnapToVoxel = checkbox1;
            this.m_checkSnapToVoxel.IsChecked = MySessionComponentVoxelHand.Static.SnapToVoxel;
            this.m_checkSnapToVoxel.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkSnapToVoxel.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.SnapToVoxel_Changed));
            y += 0.01f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.15f, y);
            label1.TextEnum = MyCommonTexts.VoxelHandSettingScreen_HandSnapToVoxel;
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelSnapToVoxel = label1;
            y += 0.036f;
            position = null;
            color = null;
            MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox2.Position = new Vector2(0.12f, y);
            checkbox2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_projectToVoxel = checkbox2;
            this.m_projectToVoxel.IsChecked = MySessionComponentVoxelHand.Static.ProjectToVoxel;
            this.m_projectToVoxel.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_projectToVoxel.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.ProjectToVoxel_Changed));
            y += 0.01f;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = new Vector2(-0.15f, y);
            label2.TextEnum = MyCommonTexts.VoxelHandSettingScreen_HandProjectToVoxel;
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelProjectToVoxel = label2;
            y += 0.036f;
            position = null;
            color = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox3.Position = new Vector2(0.12f, y);
            checkbox3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_freezePhysicsCheck = checkbox3;
            this.m_freezePhysicsCheck.IsChecked = MySessionComponentVoxelHand.Static.FreezePhysics;
            this.m_freezePhysicsCheck.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_freezePhysicsCheck.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.FreezePhysics_Changed));
            y += 0.01f;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(-0.15f, y);
            label3.TextEnum = MyCommonTexts.VoxelHandSettingScreen_FreezePhysics;
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelFreezePhysics = label3;
            y += 0.036f;
            position = null;
            color = null;
            MyGuiControlCheckbox checkbox4 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox4.Position = new Vector2(0.12f, y);
            checkbox4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_showGizmos = checkbox4;
            this.m_showGizmos.IsChecked = MySessionComponentVoxelHand.Static.ShowGizmos;
            this.m_showGizmos.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_showGizmos.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.ShowGizmos_Changed));
            y += 0.01f;
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.Position = new Vector2(-0.15f, y);
            label4.TextEnum = MyCommonTexts.VoxelHandSettingScreen_HandShowGizmos;
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelShowGizmos = label4;
            y += 0.045f;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(-0.15f, y);
            label5.TextEnum = MyCommonTexts.VoxelHandSettingScreen_HandTransparency;
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelTransparency = label5;
            y += 0.027f;
            position = null;
            float? defaultValue = null;
            color = null;
            MyGuiControlSlider slider1 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, defaultValue, color, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            slider1.Position = new Vector2(-0.15f, y);
            slider1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_sliderTransparency = slider1;
            this.m_sliderTransparency.Size = new Vector2(0.263f, 0.1f);
            this.m_sliderTransparency.MinValue = 0f;
            this.m_sliderTransparency.MaxValue = 1f;
            this.m_sliderTransparency.Value = 1f - MySessionComponentVoxelHand.Static.ShapeColor.ToVector4().W;
            this.m_sliderTransparency.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderTransparency.ValueChanged, new Action<MyGuiControlSlider>(this.BrushTransparency_ValueChanged));
            y += 0.057f;
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Position = new Vector2(-0.15f, y);
            label6.TextEnum = MyCommonTexts.VoxelHandSettingScreen_HandDistance;
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_labelZoom = label6;
            y += 0.027f;
            position = null;
            defaultValue = null;
            color = null;
            MyGuiControlSlider slider2 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, defaultValue, color, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            slider2.Position = new Vector2(-0.15f, y);
            slider2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_sliderZoom = slider2;
            this.m_sliderZoom.Size = new Vector2(0.263f, 0.1f);
            this.m_sliderZoom.MaxValue = MySessionComponentVoxelHand.MAX_BRUSH_ZOOM;
            this.m_sliderZoom.Value = MySessionComponentVoxelHand.Static.GetBrushZoom();
            this.m_sliderZoom.MinValue = MySessionComponentVoxelHand.MIN_BRUSH_ZOOM;
            this.m_sliderZoom.Enabled = !MySessionComponentVoxelHand.Static.ProjectToVoxel;
            this.m_sliderZoom.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderZoom.ValueChanged, new Action<MyGuiControlSlider>(this.BrushZoom_ValueChanged));
            this.m_voxelControl = new MyGuiControlVoxelHandSettings();
            this.m_voxelControl.Position = new Vector2(-0.18f, -0.078f);
            this.m_voxelControl.Item = MyToolbarComponent.CurrentToolbar.SelectedItem as MyToolbarItemVoxelHand;
            this.m_voxelControl.UpdateFromBrush(MySessionComponentVoxelHand.Static.CurrentShape);
            StringBuilder output = null;
            StringBuilder builder2 = null;
            StringBuilder builder3 = null;
            StringBuilder builder4 = null;
            MyInput.Static.GetGameControl(MyControlsSpace.PRIMARY_TOOL_ACTION).AppendBoundButtonNames(ref output, ", ", MyInput.Static.GetUnassignedName(), true);
            MyInput.Static.GetGameControl(MyControlsSpace.SECONDARY_TOOL_ACTION).AppendBoundButtonNames(ref builder2, ", ", MyInput.Static.GetUnassignedName(), true);
            MyInput.Static.GetGameControl(MyControlsSpace.SWITCH_LEFT).AppendBoundButtonNames(ref builder3, ", ", MyInput.Static.GetUnassignedName(), true);
            MyInput.Static.GetGameControl(MyControlsSpace.SWITCH_RIGHT).AppendBoundButtonNames(ref builder4, ", ", MyInput.Static.GetUnassignedName(), true);
            color = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(-0.15f, 0.252f), new Vector2(0.275f, 0.125f), color, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.VoxelHands_Description), new object[] { 
                    output,
                    builder2,
                    builder3,
                    builder4
                }))
            };
            this.Controls.Add(text);
            Vector2 vector = new Vector2(-0.083f, 0.36f);
            Vector2 vector2 = new Vector2(0.134f, 0.038f);
            float usableWidth = 0.265f;
            MyGuiControlButton button = this.CreateButton(usableWidth, MyTexts.Get(MyCommonTexts.Close), new Action<MyGuiControlButton>(this.OKButtonClicked), true, new MyStringId?(MySpaceTexts.ToolTipNewsletter_Close), 0.8f);
            button.Position = vector + (new Vector2(0f, 2f) * vector2);
            button.PositionX += vector2.X / 2f;
            button.ShowTooltipWhenDisabled = true;
            this.Controls.Add(button);
            this.Controls.Add(this.m_labelSnapToVoxel);
            this.Controls.Add(this.m_checkSnapToVoxel);
            this.Controls.Add(this.m_labelShowGizmos);
            this.Controls.Add(this.m_showGizmos);
            this.Controls.Add(this.m_labelProjectToVoxel);
            this.Controls.Add(this.m_projectToVoxel);
            this.Controls.Add(this.m_labelFreezePhysics);
            this.Controls.Add(this.m_freezePhysicsCheck);
            this.Controls.Add(this.m_labelTransparency);
            this.Controls.Add(this.m_sliderTransparency);
            this.Controls.Add(this.m_labelZoom);
            this.Controls.Add(this.m_sliderZoom);
            this.Controls.Add(this.m_voxelControl);
        }

        private void ShowGizmos_Changed(MyGuiControlCheckbox sender)
        {
            MySessionComponentVoxelHand.Static.ShowGizmos = this.m_showGizmos.IsChecked;
        }

        private void SnapToVoxel_Changed(MyGuiControlCheckbox sender)
        {
            MySessionComponentVoxelHand.Static.SnapToVoxel = this.m_checkSnapToVoxel.IsChecked;
        }
    }
}

