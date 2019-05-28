namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenWardrobe : MyGuiScreenBase
    {
        [CompilerGenerated]
        private static MyWardrobeChangeDelegate LookChanged;
        private const string m_hueScaleTexture = @"Textures\GUI\HueScale.png";
        private MyGuiControlCombobox m_modelPicker;
        private MyGuiControlSlider m_sliderHue;
        private MyGuiControlSlider m_sliderSaturation;
        private MyGuiControlSlider m_sliderValue;
        private MyGuiControlLabel m_labelHue;
        private MyGuiControlLabel m_labelSaturation;
        private MyGuiControlLabel m_labelValue;
        private string m_selectedModel;
        private Vector3 m_selectedHSV;
        private MyCharacter m_user;
        private Dictionary<string, int> m_displayModels;
        private Dictionary<int, string> m_models;
        private string m_storedModel;
        private Vector3 m_storedHSV;
        private MyCameraControllerSettings m_storedCamera;
        private bool m_colorOrModelChanged;

        public static  event MyWardrobeChangeDelegate LookChanged
        {
            [CompilerGenerated] add
            {
                MyWardrobeChangeDelegate lookChanged = LookChanged;
                while (true)
                {
                    MyWardrobeChangeDelegate a = lookChanged;
                    MyWardrobeChangeDelegate delegate4 = (MyWardrobeChangeDelegate) Delegate.Combine(a, value);
                    lookChanged = Interlocked.CompareExchange<MyWardrobeChangeDelegate>(ref LookChanged, delegate4, a);
                    if (ReferenceEquals(lookChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyWardrobeChangeDelegate lookChanged = LookChanged;
                while (true)
                {
                    MyWardrobeChangeDelegate source = lookChanged;
                    MyWardrobeChangeDelegate delegate4 = (MyWardrobeChangeDelegate) Delegate.Remove(source, value);
                    lookChanged = Interlocked.CompareExchange<MyWardrobeChangeDelegate>(ref LookChanged, delegate4, source);
                    if (ReferenceEquals(lookChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenWardrobe(MyCharacter user, HashSet<string> customCharacterNames = null) : base(new Vector2?(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 0x36, 0x36)), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.Texture, 0f, 0f)
        {
            Vector2? nullable = new Vector2(0.31f, 0.55f);
            base.EnabledBackgroundFade = false;
            this.m_user = user;
            this.m_storedModel = this.m_user.ModelName;
            this.m_storedHSV = this.m_user.ColorMask;
            this.m_selectedModel = this.GetDisplayName(this.m_user.ModelName);
            this.m_selectedHSV = this.m_storedHSV;
            this.m_displayModels = new Dictionary<string, int>();
            this.m_models = new Dictionary<int, string>();
            int num = 0;
            if (customCharacterNames == null)
            {
                foreach (MyCharacterDefinition definition in MyDefinitionManager.Static.Characters)
                {
                    if ((!MySession.Static.SurvivalMode || definition.UsableByPlayer) && definition.Public)
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
            this.ChangeCharacter(this.m_storedModel, this.m_storedHSV);
            this.ChangeCameraBack();
            base.Canceling();
        }

        private void ChangeCamera()
        {
            if (MySession.Static.Settings.Enable3rdPersonView)
            {
                this.m_storedCamera.Controller = MySession.Static.GetCameraControllerEnum();
                this.m_storedCamera.Distance = MySession.Static.GetCameraTargetDistance();
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.ThirdPersonSpectator, null, position);
                MySession.Static.SetCameraTargetDistance(2.0);
            }
        }

        private void ChangeCameraBack()
        {
            if (MySession.Static.Settings.Enable3rdPersonView)
            {
                Vector3D? position = null;
                MySession.Static.SetCameraController(this.m_storedCamera.Controller, this.m_user, position);
                MySession.Static.SetCameraTargetDistance(this.m_storedCamera.Distance);
            }
        }

        private void ChangeCharacter(string model, Vector3 colorMaskHSV)
        {
            this.m_colorOrModelChanged = true;
            this.m_user.ChangeModelAndColor(model, colorMaskHSV, false, 0L);
            if ((model == "Default_Astronaut") || (model == "Default_Astronaut_Female"))
            {
                MyLocalCache.LoadInventoryConfig(MySession.Static.LocalCharacter, false);
            }
        }

        private string GetDisplayName(string name) => 
            MyTexts.GetString(name);

        public override string GetFriendlyName() => 
            "MyGuiScreenWardrobe";

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
            this.ChangeCharacter(this.m_storedModel, this.m_storedHSV);
            this.ChangeCameraBack();
            this.CloseScreenNow();
        }

        protected override void OnClosed()
        {
            this.m_sliderHue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderHue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderSaturation.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderSaturation.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            this.m_sliderValue.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Remove(this.m_sliderValue.ValueChanged, new Action<MyGuiControlSlider>(this.OnValueChange));
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
            base.OnClosed();
        }

        private void OnItemSelected()
        {
            this.m_selectedModel = this.m_models[(int) this.m_modelPicker.GetSelectedKey()];
            this.ChangeCharacter(this.m_selectedModel, this.m_selectedHSV);
        }

        private void OnOkClick(MyGuiControlButton sender)
        {
            if (this.m_colorOrModelChanged && (LookChanged != null))
            {
                LookChanged(this.m_storedModel, this.m_storedHSV, this.m_user.ModelName, this.m_user.ColorMask);
            }
            if (this.m_user.Definition.UsableByPlayer)
            {
                MyLocalCache.SaveInventoryConfig(MySession.Static.LocalCharacter);
            }
            this.ChangeCameraBack();
            this.CloseScreenNow();
        }

        private void OnValueChange(MyGuiControlSlider sender)
        {
            this.UpdateLabels();
            this.m_selectedHSV.X = this.m_sliderHue.Value / 360f;
            this.m_selectedHSV.Y = this.m_sliderSaturation.Value - MyColorPickerConstants.SATURATION_DELTA;
            this.m_selectedHSV.Z = (this.m_sliderValue.Value - MyColorPickerConstants.VALUE_DELTA) + MyColorPickerConstants.VALUE_COLORIZE_DELTA;
            this.m_selectedModel = this.m_models[(int) this.m_modelPicker.GetSelectedKey()];
            this.ChangeCharacter(this.m_selectedModel, this.m_selectedHSV);
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
            foreach (KeyValuePair<string, int> pair in this.m_displayModels)
            {
                nullable3 = null;
                this.m_modelPicker.AddItem((long) pair.Value, new StringBuilder(pair.Key), nullable3, null);
            }
            if (this.m_displayModels.ContainsKey(this.m_selectedModel))
            {
                this.m_modelPicker.SelectItemByKey((long) this.m_displayModels[this.m_selectedModel], true);
            }
            else if (this.m_displayModels.Count > 0)
            {
                this.m_modelPicker.SelectItemByKey((long) this.m_displayModels.First<KeyValuePair<string, int>>().Value, true);
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
            this.Controls.Add(new MyGuiControlButton(new Vector2(0f, 0.16f), MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenWardrobeOld_Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, nullable3, false));
            captionOffset = null;
            captionTextColor = null;
            nullable3 = null;
            this.Controls.Add(new MyGuiControlButton(new Vector2(0f, 0.22f), MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenWardrobeOld_Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, nullable3, false));
            this.m_colorOrModelChanged = false;
        }

        private void UpdateLabels()
        {
            this.m_labelHue.Text = this.m_sliderHue.Value.ToString() + "\x00b0";
            this.m_labelSaturation.Text = this.m_sliderSaturation.Value.ToString("P1");
            this.m_labelValue.Text = this.m_sliderValue.Value.ToString("P1");
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyCameraControllerSettings
        {
            public double Distance;
            public MyCameraControllerEnum Controller;
        }
    }
}

