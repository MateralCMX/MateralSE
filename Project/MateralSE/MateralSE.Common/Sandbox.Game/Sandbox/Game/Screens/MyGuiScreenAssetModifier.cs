namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenAssetModifier : MyGuiScreenBase
    {
        [CompilerGenerated]
        private static MyAssetChangeDelegate LookChanged;
        private MyGuiControlCombobox m_modelPicker;
        private MyGuiControlSlider m_sliderHue;
        private MyGuiControlSlider m_sliderSaturation;
        private MyGuiControlSlider m_sliderValue;
        private MyGuiControlLabel m_labelHue;
        private MyGuiControlLabel m_labelSaturation;
        private MyGuiControlLabel m_labelValue;
        private Dictionary<string, MyTextureChange> m_selectedModifier;
        private Vector3 m_selectedHSV;
        private readonly MyCharacter m_user;
        private readonly List<KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>>> m_modifiers;
        private readonly Vector3 m_storedHSV;
        private MyCameraControllerSettings m_storedCamera;
        private bool m_colorOrModelChanged;

        public static  event MyAssetChangeDelegate LookChanged
        {
            [CompilerGenerated] add
            {
                MyAssetChangeDelegate lookChanged = LookChanged;
                while (true)
                {
                    MyAssetChangeDelegate a = lookChanged;
                    MyAssetChangeDelegate delegate4 = (MyAssetChangeDelegate) Delegate.Combine(a, value);
                    lookChanged = Interlocked.CompareExchange<MyAssetChangeDelegate>(ref LookChanged, delegate4, a);
                    if (ReferenceEquals(lookChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyAssetChangeDelegate lookChanged = LookChanged;
                while (true)
                {
                    MyAssetChangeDelegate source = lookChanged;
                    MyAssetChangeDelegate delegate4 = (MyAssetChangeDelegate) Delegate.Remove(source, value);
                    lookChanged = Interlocked.CompareExchange<MyAssetChangeDelegate>(ref LookChanged, delegate4, source);
                    if (ReferenceEquals(lookChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenAssetModifier(MyCharacter user) : base(new Vector2?(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 0x36, 0x36)), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.Texture, 0f, 0f)
        {
            Vector2? nullable = new Vector2(0.31f, 0.66f);
            base.EnabledBackgroundFade = false;
            this.m_user = user;
            this.m_storedHSV = this.m_user.ColorMask;
            this.m_selectedModifier = null;
            this.m_selectedHSV = this.m_storedHSV;
            this.m_modifiers = new List<KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>>>();
            int num = 0;
            MyDefinitionManager.Static.GetAssetModifierDefinitionsForRender();
            foreach (MyGameInventoryItem item in MyGameService.InventoryItems)
            {
                Dictionary<string, MyTextureChange> assetModifierDefinitionForRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(item.ItemDefinition.AssetModifierId);
                this.m_modifiers.Add(new KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>>(MyStringHash.GetOrCompute(item.ItemDefinition.AssetModifierId), assetModifierDefinitionForRender));
                num++;
            }
            this.m_modifiers.Sort((a, b) => string.CompareOrdinal(a.Key.ToString(), b.Key.ToString()));
            this.RecreateControls(true);
            this.m_sliderHue.Value = this.m_selectedHSV.X * 360f;
            this.m_sliderSaturation.Value = MathHelper.Clamp((float) (this.m_selectedHSV.Y + MyColorPickerConstants.SATURATION_DELTA), (float) 0f, (float) 1f);
            this.m_sliderValue.Value = MathHelper.Clamp((float) ((this.m_selectedHSV.Z + MyColorPickerConstants.VALUE_DELTA) - MyColorPickerConstants.VALUE_COLORIZE_DELTA), (float) 0f, (float) 1f);
            this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.ChangeCamera();
            this.UpdateLabels();
        }

        protected override void Canceling()
        {
            this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.ChangeCharacter(null, this.m_storedHSV);
            this.ChangeCameraBack();
            base.Canceling();
        }

        private void ChangeCamera()
        {
            if (MySession.Static.Settings.Enable3rdPersonView)
            {
                this.m_storedCamera.Controller = MySession.Static.GetCameraControllerEnum();
                this.m_storedCamera.Distance = MySession.Static.GetCameraTargetDistance();
                this.m_storedCamera.ViewMatrix = MySpectatorCameraController.Static.GetViewMatrix();
                Vector3D position = (this.m_user.WorldMatrix.Translation + this.m_user.WorldMatrix.Up) + (this.m_user.WorldMatrix.Forward * 2.0);
                MatrixD viewMatrix = MatrixD.CreateWorld(position, this.m_user.WorldMatrix.Backward, this.m_user.WorldMatrix.Up);
                MySpectatorCameraController.Static.SetViewMatrix(viewMatrix);
                MySession.Static.SetCameraTargetDistance(2.0);
                MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(position));
                MySpectator.Static.SetTarget(this.m_user.WorldMatrix.Translation + this.m_user.WorldMatrix.Up, new Vector3D?(this.m_user.WorldMatrix.Up));
            }
        }

        private void ChangeCameraBack()
        {
            if (MySession.Static.Settings.Enable3rdPersonView)
            {
                Vector3D? position = null;
                MySession.Static.SetCameraController(this.m_storedCamera.Controller, this.m_user, position);
                MySession.Static.SetCameraTargetDistance(this.m_storedCamera.Distance);
                MySpectatorCameraController.Static.SetViewMatrix(this.m_storedCamera.ViewMatrix);
            }
        }

        private void ChangeCharacter(Dictionary<string, MyTextureChange> modifier, Vector3 colorMaskHSV)
        {
            if ((modifier == null) || (this.m_user == null))
            {
                this.ResetCharacter();
                this.m_colorOrModelChanged = true;
                this.m_user.ChangeModelAndColor(this.m_user.ModelName, colorMaskHSV, false, 0L);
            }
            else
            {
                if ((this.m_user.Render != null) && (this.m_user.Render.RenderObjectIDs[0] != uint.MaxValue))
                {
                    MyRenderProxy.ChangeMaterialTexture(this.m_user.Render.RenderObjectIDs[0], modifier);
                }
                this.m_colorOrModelChanged = true;
                this.m_user.ChangeModelAndColor(this.m_user.ModelName, colorMaskHSV, false, 0L);
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenAssetModifier";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.USE))
            {
                this.ChangeCameraBack();
                this.CloseScreen();
            }
            base.HandleInput(receivedFocusInThisUpdate);
        }

        private void OnCancelClick(MyGuiControlButton sender)
        {
            this.ChangeCharacter(null, this.m_storedHSV);
            this.ChangeCameraBack();
            this.CloseScreenNow();
        }

        protected override void OnClosed()
        {
            if (this.m_modifiers != null)
            {
                this.m_modifiers.Clear();
            }
            this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
            base.OnClosed();
        }

        private void OnItemSelected()
        {
            this.m_selectedModifier = this.m_modifiers[(int) this.m_modelPicker.GetSelectedKey()].Value;
            this.ChangeCharacter(this.m_selectedModifier, this.m_selectedHSV);
        }

        private void OnOkClick(MyGuiControlButton sender)
        {
            if (this.m_colorOrModelChanged && (LookChanged != null))
            {
                LookChanged(this.m_user.ModelName, this.m_storedHSV, this.m_user.ModelName, this.m_user.ColorMask);
            }
            this.ChangeCameraBack();
            this.CloseScreenNow();
        }

        private void OnResetClick(MyGuiControlButton sender)
        {
            this.ChangeCharacter(null, this.m_storedHSV);
        }

        private void OnValueChange(MyGuiControlSlider sender)
        {
            this.UpdateLabels();
            this.m_selectedHSV.X = this.m_sliderHue.Value / 360f;
            this.m_selectedHSV.Y = this.m_sliderSaturation.Value - MyColorPickerConstants.SATURATION_DELTA;
            this.m_selectedHSV.Z = (this.m_sliderValue.Value - MyColorPickerConstants.VALUE_DELTA) + MyColorPickerConstants.VALUE_COLORIZE_DELTA;
            this.ChangeCharacter(this.m_selectedModifier, this.m_selectedHSV);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            int? nullable3;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            MyGuiControlLabel control = base.AddCaption(MyCommonTexts.PlayerCharacterModel, captionTextColor, captionOffset, 0.8f);
            Vector2 itemSize = MyGuiControlListbox.GetVisualStyle(MyGuiControlListboxStyleEnum.Default).ItemSize;
            float y = -0.19f;
            captionOffset = null;
            captionTextColor = null;
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            this.m_modelPicker = new MyGuiControlCombobox(new Vector2(0f, y), captionOffset, captionTextColor, captionOffset, 10, captionOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
            int num2 = 0;
            foreach (KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>> pair in this.m_modifiers)
            {
                MyGameInventoryItemDefinition inventoryItemDefinition = MyGameService.GetInventoryItemDefinition(pair.Key.ToString());
                nullable3 = null;
                this.m_modelPicker.AddItem((long) num2, new StringBuilder((pair.Key != MyStringHash.NullOrEmpty) ? inventoryItemDefinition.Name : "<null>"), nullable3, null);
                num2++;
            }
            this.m_modelPicker.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnItemSelected);
            y += 0.045f;
            Vector2 vector2 = itemSize + control.Size;
            float* singlePtr1 = (float*) ref base.m_position.X;
            singlePtr1[0] -= vector2.X / 2.5f;
            float* singlePtr2 = (float*) ref base.m_position.Y;
            singlePtr2[0] += vector2.Y * 3.6f;
            captionOffset = null;
            captionTextColor = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, y), captionOffset, MyTexts.GetString(MyCommonTexts.PlayerCharacterColor), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            y += 0.04f;
            captionOffset = null;
            captionTextColor = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.135f, y), captionOffset, MyTexts.GetString(MyCommonTexts.ScreenWardrobeOld_Hue), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            captionOffset = null;
            captionTextColor = null;
            this.m_labelHue = new MyGuiControlLabel(new Vector2(0.09f, y), captionOffset, string.Empty, captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            y += 0.035f;
            float? defaultValue = null;
            captionTextColor = null;
            this.m_sliderHue = new MyGuiControlSlider(new Vector2(-0.135f, y), 0f, 360f, 0.3f, defaultValue, captionTextColor, null, 0, 0.8f, 0.04166667f, "White", null, MyGuiControlSliderStyleEnum.Hue, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, true, false);
            y += 0.045f;
            captionOffset = null;
            captionTextColor = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.135f, y), captionOffset, MyTexts.GetString(MyCommonTexts.ScreenWardrobeOld_Saturation), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            captionOffset = null;
            captionTextColor = null;
            this.m_labelSaturation = new MyGuiControlLabel(new Vector2(0.09f, y), captionOffset, string.Empty, captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            y += 0.035f;
            captionTextColor = null;
            this.m_sliderSaturation = new MyGuiControlSlider(new Vector2(-0.135f, y), 0f, 1f, 0.3f, 0f, captionTextColor, null, 1, 0.8f, 0.04166667f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, false);
            y += 0.045f;
            captionOffset = null;
            captionTextColor = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(-0.135f, y), captionOffset, MyTexts.GetString(MyCommonTexts.ScreenWardrobeOld_Value), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            captionOffset = null;
            captionTextColor = null;
            this.m_labelValue = new MyGuiControlLabel(new Vector2(0.09f, y), captionOffset, string.Empty, captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            y += 0.035f;
            captionTextColor = null;
            this.m_sliderValue = new MyGuiControlSlider(new Vector2(-0.135f, y), 0f, 1f, 0.3f, 0f, captionTextColor, null, 1, 0.8f, 0.04166667f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, false);
            y += 0.045f;
            this.Controls.Add(control);
            this.Controls.Add(this.m_modelPicker);
            this.Controls.Add(this.m_labelHue);
            this.Controls.Add(this.m_labelSaturation);
            this.Controls.Add(this.m_labelValue);
            this.Controls.Add(this.m_sliderHue);
            this.Controls.Add(this.m_sliderSaturation);
            this.Controls.Add(this.m_sliderValue);
            captionOffset = null;
            captionTextColor = null;
            nullable3 = null;
            this.Controls.Add(new MyGuiControlButton(new Vector2(0f, 0.16f), MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.ToolbarAction_Reset), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnResetClick), GuiSounds.MouseClick, 1f, nullable3, false));
            captionOffset = null;
            captionTextColor = null;
            nullable3 = null;
            this.Controls.Add(new MyGuiControlButton(new Vector2(0f, 0.24f), MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenWardrobeOld_Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, nullable3, false));
            captionOffset = null;
            captionTextColor = null;
            nullable3 = null;
            this.Controls.Add(new MyGuiControlButton(new Vector2(0f, 0.3f), MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenWardrobeOld_Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, nullable3, false));
            this.m_colorOrModelChanged = false;
        }

        private void ResetCharacter()
        {
            if (this.m_user != null)
            {
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Astronaut_head");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Head");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Spacesuit_hood");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "LeftGlove");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "RightGlove");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Boots");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Arms");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "RightArm");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Gear");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Cloth");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Emissive");
                MyAssetModifierComponent.SetDefaultTextures(this.m_user, "Backpack");
                MyAssetModifierComponent.ResetRifle(this.m_user);
                MyAssetModifierComponent.ResetWelder(this.m_user);
                MyAssetModifierComponent.ResetGrinder(this.m_user);
                MyAssetModifierComponent.ResetDrill(this.m_user);
            }
        }

        private void UpdateLabels()
        {
            this.m_labelHue.Text = this.m_sliderHue.Value.ToString() + "\x00b0";
            this.m_labelSaturation.Text = this.m_sliderSaturation.Value.ToString("P1");
            this.m_labelValue.Text = this.m_sliderValue.Value.ToString("P1");
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenAssetModifier.<>c <>9 = new MyGuiScreenAssetModifier.<>c();
            public static Comparison<KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>>> <>9__18_0;

            internal int <.ctor>b__18_0(KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>> a, KeyValuePair<MyStringHash, Dictionary<string, MyTextureChange>> b) => 
                string.CompareOrdinal(a.Key.ToString(), b.Key.ToString());
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyCameraControllerSettings
        {
            public MatrixD ViewMatrix;
            public double Distance;
            public MyCameraControllerEnum Controller;
        }
    }
}

