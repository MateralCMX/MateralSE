namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenOptionsAudio : MyGuiScreenBase
    {
        private MyGuiControlSlider m_gameVolumeSlider;
        private MyGuiControlSlider m_musicVolumeSlider;
        private MyGuiControlSlider m_voiceChatVolumeSlider;
        private MyGuiControlCheckbox m_hudWarnings;
        private MyGuiControlCheckbox m_enableVoiceChat;
        private MyGuiControlCheckbox m_enableMuteWhenNotInFocus;
        private MyGuiControlCheckbox m_enableDynamicMusic;
        private MyGuiControlCheckbox m_enableReverb;
        private MyGuiControlCheckbox m_enableDoppler;
        private MyGuiControlCheckbox m_shipSoundsAreBasedOnSpeed;
        private MyGuiScreenOptionsAudioSettings m_settingsOld;
        private MyGuiScreenOptionsAudioSettings m_settingsNew;
        private bool m_gameAudioPausedWhenOpen;
        private MyGuiControlElementGroup m_elementGroup;

        public MyGuiScreenOptionsAudio() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.6679389f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_settingsOld = new MyGuiScreenOptionsAudioSettings();
            this.m_settingsNew = new MyGuiScreenOptionsAudioSettings();
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        public override bool CloseScreen()
        {
            this.UpdateFromConfig(this.m_settingsOld);
            UpdateValues(this.m_settingsOld);
            if (this.m_gameAudioPausedWhenOpen)
            {
                MyAudio.Static.PauseGameSounds();
            }
            return base.CloseScreen();
        }

        private void EnableDopplerChecked(MyGuiControlCheckbox obj)
        {
            this.m_settingsNew.EnableDoppler = obj.IsChecked;
        }

        private void EnableDynamicMusicChecked(MyGuiControlCheckbox obj)
        {
            this.m_settingsNew.EnableDynamicMusic = obj.IsChecked;
        }

        private void EnableMuteWhenNotInFocusChecked(MyGuiControlCheckbox obj)
        {
            this.m_settingsNew.EnableMuteWhenNotInFocus = obj.IsChecked;
        }

        private void EnableReverbChecked(MyGuiControlCheckbox obj)
        {
            int isChecked;
            if (!MyFakes.AUDIO_ENABLE_REVERB || (MyAudio.Static.SampleRate > MyAudio.MAX_SAMPLE_RATE))
            {
                isChecked = 0;
            }
            else
            {
                isChecked = (int) obj.IsChecked;
            }
            this.m_settingsNew.EnableReverb = (bool) isChecked;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenOptionsAudio";

        private void HudWarningsChecked(MyGuiControlCheckbox obj)
        {
            this.m_settingsNew.HudWarnings = obj.IsChecked;
        }

        private void m_elementGroup_HighlightChanged(MyGuiControlElementGroup obj)
        {
            foreach (MyGuiControlBase base2 in this.m_elementGroup)
            {
                if (base2.HasFocus && !ReferenceEquals(obj.SelectedElement, base2))
                {
                    base.FocusedControl = obj.SelectedElement;
                    break;
                }
            }
        }

        public void OnCancelClick(MyGuiControlButton sender)
        {
            UpdateValues(this.m_settingsOld);
            this.CloseScreen();
        }

        private void OnGameVolumeChange(MyGuiControlSlider sender)
        {
            this.m_settingsNew.GameVolume = this.m_gameVolumeSlider.Value;
            UpdateValues(this.m_settingsNew);
        }

        private void OnMusicVolumeChange(MyGuiControlSlider sender)
        {
            this.m_settingsNew.MusicVolume = this.m_musicVolumeSlider.Value;
            UpdateValues(this.m_settingsNew);
        }

        public void OnOkClick(MyGuiControlButton sender)
        {
            this.Save();
            this.CloseScreen();
        }

        private void OnVoiceChatVolumeChange(MyGuiControlSlider sender)
        {
            this.m_settingsNew.VoiceChatVolume = this.m_voiceChatVolumeSlider.Value;
            UpdateValues(this.m_settingsNew);
        }

        public override void RecreateControls(bool constructor)
        {
            if (constructor)
            {
                base.RecreateControls(constructor);
                this.m_elementGroup = new MyGuiControlElementGroup();
                this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_HighlightChanged);
                VRageMath.Vector4? captionTextColor = null;
                base.AddCaption(MyCommonTexts.ScreenCaptionAudioOptions, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
                MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
                captionTextColor = null;
                control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
                this.Controls.Add(control);
                MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
                captionTextColor = null;
                list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
                this.Controls.Add(list2);
                MyGuiDrawAlignEnum enum2 = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                MyGuiDrawAlignEnum enum3 = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                Vector2 vector = new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
                Vector2 vector2 = new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
                float x = 455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
                float num2 = 25f;
                float y = MyGuiConstants.SCREEN_CAPTION_DELTA_Y * 0.5f;
                Vector2 vector3 = new Vector2(0f, 0.045f);
                float num5 = 0f;
                Vector2 vector4 = new Vector2(0f, 0.008f);
                Vector2 vector5 = (((base.m_size.Value / 2f) - vector) * new Vector2(-1f, -1f)) + new Vector2(0f, y);
                Vector2 vector6 = (((base.m_size.Value / 2f) - vector) * new Vector2(1f, -1f)) + new Vector2(0f, y);
                Vector2 vector7 = ((base.m_size.Value / 2f) - vector2) * new Vector2(0f, 1f);
                Vector2 vector8 = new Vector2(vector6.X - (x + 0.0015f), vector6.Y);
                num5 -= 0.045f;
                Vector2? position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.MusicVolume), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label1.Position = (vector5 + (num5 * vector3)) + vector4;
                label1.OriginAlign = enum2;
                MyGuiControlLabel label = label1;
                position = null;
                string toolTip = MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_MusicVolume);
                captionTextColor = null;
                MyGuiControlSlider slider1 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, new float?(MySandboxGame.Config.MusicVolume), captionTextColor, null, 1, 0.8f, 0f, "White", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                slider1.Position = vector6 + (num5 * vector3);
                slider1.OriginAlign = enum3;
                slider1.Size = new Vector2(x, 0f);
                this.m_musicVolumeSlider = slider1;
                this.m_musicVolumeSlider.ValueChanged = new Action<MyGuiControlSlider>(this.OnMusicVolumeChange);
                num5 += 1.08f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label11 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.GameVolume), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label11.Position = (vector5 + (num5 * vector3)) + vector4;
                label11.OriginAlign = enum2;
                MyGuiControlLabel label2 = label11;
                position = null;
                toolTip = MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_SoundVolume);
                captionTextColor = null;
                MyGuiControlSlider slider2 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, new float?(MySandboxGame.Config.GameVolume), captionTextColor, null, 1, 0.8f, 0f, "White", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                slider2.Position = vector6 + (num5 * vector3);
                slider2.OriginAlign = enum3;
                slider2.Size = new Vector2(x, 0f);
                this.m_gameVolumeSlider = slider2;
                this.m_gameVolumeSlider.ValueChanged = new Action<MyGuiControlSlider>(this.OnGameVolumeChange);
                num5 += 1.08f;
                MyGuiControlLabel label3 = null;
                MyGuiControlLabel label4 = null;
                if (MyPerGameSettings.VoiceChatEnabled)
                {
                    position = null;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlLabel label12 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.VoiceChatVolume), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label12.Position = (vector5 + (num5 * vector3)) + vector4;
                    label12.OriginAlign = enum2;
                    label4 = label12;
                    position = null;
                    toolTip = MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_VoiceChatVolume);
                    captionTextColor = null;
                    MyGuiControlSlider slider3 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, new float?(MySandboxGame.Config.VoiceChatVolume), captionTextColor, null, 1, 0.8f, 0f, "White", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                    slider3.Position = vector6 + (num5 * vector3);
                    slider3.OriginAlign = enum3;
                    slider3.Size = new Vector2(x, 0f);
                    this.m_voiceChatVolumeSlider = slider3;
                    this.m_voiceChatVolumeSlider.ValueChanged = new Action<MyGuiControlSlider>(this.OnVoiceChatVolumeChange);
                    num5 += 1.37f;
                    position = null;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlLabel label13 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.EnableVoiceChat), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label13.Position = (vector5 + (num5 * vector3)) + vector4;
                    label13.OriginAlign = enum2;
                    label3 = label13;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_EnableVoiceChat), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    checkbox1.Position = vector8 + (num5 * vector3);
                    checkbox1.OriginAlign = enum2;
                    this.m_enableVoiceChat = checkbox1;
                    this.m_enableVoiceChat.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.VoiceChatChecked);
                    num5++;
                }
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label14 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.MuteWhenNotInFocus), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label14.Position = (vector5 + (num5 * vector3)) + vector4;
                label14.OriginAlign = enum2;
                MyGuiControlLabel label5 = label14;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_MuteWhenInactive), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox2.Position = vector8 + (num5 * vector3);
                checkbox2.OriginAlign = enum2;
                this.m_enableMuteWhenNotInFocus = checkbox2;
                this.m_enableMuteWhenNotInFocus.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.EnableMuteWhenNotInFocusChecked);
                num5++;
                MyGuiControlLabel label6 = null;
                if (MyPerGameSettings.UseReverbEffect && MyFakes.AUDIO_ENABLE_REVERB)
                {
                    position = null;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlLabel label15 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.AudioSettings_EnableReverb), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label15.Position = (vector5 + (num5 * vector3)) + vector4;
                    label15.OriginAlign = enum2;
                    label6 = label15;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipAudioOptionsEnableReverb), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    checkbox3.Position = vector8 + (num5 * vector3);
                    checkbox3.OriginAlign = enum2;
                    this.m_enableReverb = checkbox3;
                    this.m_enableReverb.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.EnableReverbChecked);
                    this.m_enableReverb.Enabled = MyAudio.Static.SampleRate <= MyAudio.MAX_SAMPLE_RATE;
                    this.m_enableReverb.IsChecked = MyAudio.Static.EnableReverb && (MyAudio.Static.SampleRate <= MyAudio.MAX_SAMPLE_RATE);
                    num5++;
                }
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label16 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.AudioSettings_EnableDoppler), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label16.Position = (vector5 + (num5 * vector3)) + vector4;
                label16.OriginAlign = enum2;
                MyGuiControlLabel label7 = label16;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox4 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipAudioOptionsEnableDoppler), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox4.Position = vector8 + (num5 * vector3);
                checkbox4.OriginAlign = enum2;
                this.m_enableDoppler = checkbox4;
                this.m_enableDoppler.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.EnableDopplerChecked);
                this.m_enableDoppler.Enabled = true;
                this.m_enableDoppler.IsChecked = MyAudio.Static.EnableDoppler;
                num5++;
                MyGuiControlLabel label8 = null;
                if (MyPerGameSettings.EnableShipSoundSystem)
                {
                    position = null;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlLabel label17 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.AudioSettings_ShipSoundsBasedOnSpeed), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label17.Position = (vector5 + (num5 * vector3)) + vector4;
                    label17.OriginAlign = enum2;
                    label8 = label17;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlCheckbox checkbox5 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_SpeedBasedSounds), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    checkbox5.Position = vector8 + (num5 * vector3);
                    checkbox5.OriginAlign = enum2;
                    this.m_shipSoundsAreBasedOnSpeed = checkbox5;
                    this.m_shipSoundsAreBasedOnSpeed.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.ShipSoundsAreBasedOnSpeedChecked);
                    num5++;
                }
                MyGuiControlLabel label9 = null;
                if (MyPerGameSettings.UseMusicController)
                {
                    position = null;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlLabel label18 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.AudioSettings_UseMusicController), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label18.Position = (vector5 + (num5 * vector3)) + vector4;
                    label18.OriginAlign = enum2;
                    label9 = label18;
                    position = null;
                    captionTextColor = null;
                    MyGuiControlCheckbox checkbox6 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_UseContextualMusic), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    checkbox6.Position = vector8 + (num5 * vector3);
                    checkbox6.OriginAlign = enum2;
                    this.m_enableDynamicMusic = checkbox6;
                    num5++;
                }
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label19 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.HudWarnings), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label19.Position = (vector5 + (num5 * vector3)) + vector4;
                label19.OriginAlign = enum2;
                MyGuiControlLabel label10 = label19;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox7 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsAudio_HudWarnings), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox7.Position = vector8 + (num5 * vector3);
                checkbox7.OriginAlign = enum2;
                this.m_hudWarnings = checkbox7;
                this.m_hudWarnings.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.HudWarningsChecked);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                int? buttonIndex = null;
                MyGuiControlButton button = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
                position = null;
                position = null;
                captionTextColor = null;
                buttonIndex = null;
                MyGuiControlButton button2 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                button2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
                button.Position = vector7 + (new Vector2(-num2, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
                button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                button2.Position = vector7 + (new Vector2(num2, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
                button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
                this.Controls.Add(label2);
                this.Controls.Add(this.m_gameVolumeSlider);
                this.Controls.Add(label);
                this.Controls.Add(this.m_musicVolumeSlider);
                this.Controls.Add(label10);
                this.Controls.Add(this.m_hudWarnings);
                this.Controls.Add(label5);
                this.Controls.Add(this.m_enableMuteWhenNotInFocus);
                if (MyPerGameSettings.UseMusicController)
                {
                    this.Controls.Add(label9);
                    this.Controls.Add(this.m_enableDynamicMusic);
                }
                if (MyPerGameSettings.EnableShipSoundSystem)
                {
                    this.Controls.Add(label8);
                    this.Controls.Add(this.m_shipSoundsAreBasedOnSpeed);
                }
                if (MyPerGameSettings.UseReverbEffect && MyFakes.AUDIO_ENABLE_REVERB)
                {
                    this.Controls.Add(label6);
                    this.Controls.Add(this.m_enableReverb);
                }
                this.Controls.Add(label7);
                this.Controls.Add(this.m_enableDoppler);
                if (MyPerGameSettings.VoiceChatEnabled)
                {
                    this.Controls.Add(label3);
                    this.Controls.Add(this.m_enableVoiceChat);
                    this.Controls.Add(label4);
                    this.Controls.Add(this.m_voiceChatVolumeSlider);
                }
                this.Controls.Add(button);
                this.m_elementGroup.Add(button);
                this.Controls.Add(button2);
                this.m_elementGroup.Add(button2);
                this.UpdateFromConfig(this.m_settingsOld);
                this.UpdateFromConfig(this.m_settingsNew);
                this.UpdateControls(this.m_settingsOld);
                base.FocusedControl = button;
                base.CloseButtonEnabled = true;
                this.m_gameAudioPausedWhenOpen = MyAudio.Static.GameSoundIsPaused;
                if (this.m_gameAudioPausedWhenOpen)
                {
                    MyAudio.Static.ResumeGameSounds();
                }
            }
        }

        private void Save()
        {
            int num1;
            MySandboxGame.Config.GameVolume = MyAudio.Static.VolumeGame;
            MySandboxGame.Config.MusicVolume = MyAudio.Static.VolumeMusic;
            MySandboxGame.Config.VoiceChatVolume = this.m_voiceChatVolumeSlider.Value;
            MySandboxGame.Config.HudWarnings = this.m_hudWarnings.IsChecked;
            MySandboxGame.Config.EnableVoiceChat = this.m_enableVoiceChat.IsChecked;
            MySandboxGame.Config.EnableMuteWhenNotInFocus = this.m_enableMuteWhenNotInFocus.IsChecked;
            if (!MyFakes.AUDIO_ENABLE_REVERB || !this.m_enableReverb.IsChecked)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (MyAudio.Static.SampleRate <= MyAudio.MAX_SAMPLE_RATE);
            }
            MySandboxGame.Config.EnableReverb = (bool) num1;
            MyAudio.Static.EnableReverb = MySandboxGame.Config.EnableReverb;
            MySandboxGame.Config.EnableDynamicMusic = this.m_enableDynamicMusic.IsChecked;
            MySandboxGame.Config.ShipSoundsAreBasedOnSpeed = this.m_shipSoundsAreBasedOnSpeed.IsChecked;
            MySandboxGame.Config.EnableDoppler = this.m_enableDoppler.IsChecked;
            MyAudio.Static.EnableDoppler = MySandboxGame.Config.EnableDoppler;
            MySandboxGame.Config.Save();
            if ((MySession.Static != null) && (MyGuiScreenGamePlay.Static != null))
            {
                if (MySandboxGame.Config.EnableDynamicMusic && (MyMusicController.Static == null))
                {
                    MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
                    MyMusicController.Static.Active = true;
                    MyAudio.Static.MusicAllowed = false;
                    MyAudio.Static.StopMusic();
                }
                else if (!MySandboxGame.Config.EnableDynamicMusic && (MyMusicController.Static != null))
                {
                    MyMusicController.Static.Unload();
                    MyMusicController.Static = null;
                    MyAudio.Static.MusicAllowed = true;
                    MyMusicTrack track = new MyMusicTrack {
                        TransitionCategory = MyStringId.GetOrCompute("Default")
                    };
                    MyAudio.Static.PlayMusic(new MyMusicTrack?(track), 0);
                }
                if ((MyFakes.AUDIO_ENABLE_REVERB && ((MyAudio.Static != null) && (MyAudio.Static.EnableReverb != this.m_enableReverb.IsChecked))) && (MyAudio.Static.SampleRate <= MyAudio.MAX_SAMPLE_RATE))
                {
                    MyAudio.Static.EnableReverb = this.m_enableReverb.IsChecked;
                }
            }
        }

        private void ShipSoundsAreBasedOnSpeedChecked(MyGuiControlCheckbox obj)
        {
            this.m_settingsNew.ShipSoundsAreBasedOnSpeed = obj.IsChecked;
        }

        private void UpdateControls(MyGuiScreenOptionsAudioSettings settings)
        {
            this.m_gameVolumeSlider.Value = settings.GameVolume;
            this.m_musicVolumeSlider.Value = settings.MusicVolume;
            this.m_voiceChatVolumeSlider.Value = settings.VoiceChatVolume;
            this.m_hudWarnings.IsChecked = settings.HudWarnings;
            this.m_enableVoiceChat.IsChecked = settings.EnableVoiceChat;
            this.m_enableMuteWhenNotInFocus.IsChecked = settings.EnableMuteWhenNotInFocus;
            if (MyFakes.AUDIO_ENABLE_REVERB)
            {
                this.m_enableReverb.IsChecked = settings.EnableReverb;
            }
            this.m_enableDynamicMusic.IsChecked = settings.EnableDynamicMusic;
            this.m_shipSoundsAreBasedOnSpeed.IsChecked = settings.ShipSoundsAreBasedOnSpeed;
            this.m_enableDoppler.IsChecked = settings.EnableDoppler;
        }

        private void UpdateFromConfig(MyGuiScreenOptionsAudioSettings settings)
        {
            int num1;
            settings.GameVolume = MySandboxGame.Config.GameVolume;
            settings.MusicVolume = MySandboxGame.Config.MusicVolume;
            settings.VoiceChatVolume = MySandboxGame.Config.VoiceChatVolume;
            settings.HudWarnings = MySandboxGame.Config.HudWarnings;
            settings.EnableVoiceChat = MySandboxGame.Config.EnableVoiceChat;
            settings.EnableMuteWhenNotInFocus = MySandboxGame.Config.EnableMuteWhenNotInFocus;
            if (!MyFakes.AUDIO_ENABLE_REVERB || !MySandboxGame.Config.EnableReverb)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (MyAudio.Static.SampleRate <= MyAudio.MAX_SAMPLE_RATE);
            }
            settings.EnableReverb = (bool) num1;
            settings.EnableDynamicMusic = MySandboxGame.Config.EnableDynamicMusic;
            settings.ShipSoundsAreBasedOnSpeed = MySandboxGame.Config.ShipSoundsAreBasedOnSpeed;
            settings.EnableDoppler = MySandboxGame.Config.EnableDoppler;
        }

        private static void UpdateValues(MyGuiScreenOptionsAudioSettings settings)
        {
            MyAudio.Static.VolumeGame = settings.GameVolume;
            MyAudio.Static.VolumeMusic = settings.MusicVolume;
            MyAudio.Static.VolumeVoiceChat = settings.VoiceChatVolume;
            MyAudio.Static.VolumeHud = MyAudio.Static.VolumeGame;
            MyAudio.Static.EnableVoiceChat = settings.EnableVoiceChat;
            MyGuiAudio.HudWarnings = settings.HudWarnings;
        }

        private void VoiceChatChecked(MyGuiControlCheckbox checkbox)
        {
            this.m_settingsNew.EnableVoiceChat = checkbox.IsChecked;
        }

        private class MyGuiScreenOptionsAudioSettings
        {
            public float GameVolume;
            public float MusicVolume;
            public float VoiceChatVolume;
            public bool HudWarnings;
            public bool EnableVoiceChat;
            public bool EnableMuteWhenNotInFocus;
            public bool EnableDynamicMusic;
            public bool EnableReverb;
            public bool ShipSoundsAreBasedOnSpeed;
            public bool EnableDoppler;
        }
    }
}

