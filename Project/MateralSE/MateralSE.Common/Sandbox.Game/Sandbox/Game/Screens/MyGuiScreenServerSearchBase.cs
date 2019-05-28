namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenServerSearchBase : MyGuiScreenBase
    {
        private static List<MyWorkshopItem> m_subscribedMods;
        private static List<MyWorkshopItem> m_settingsMods;
        private static bool m_needsModRefresh = true;
        protected SearchPageEnum CurrentPage;
        protected Vector2 CurrentPosition;
        protected MyGuiScreenJoinGame JoinScreen;
        protected float Padding;
        protected MyGuiControlScrollablePanel Panel;
        protected MyGuiControlParent Parent;
        protected MyGuiControlCheckbox m_advancedCheckbox;
        protected MyGuiControlButton m_searchButton;
        protected MyGuiControlButton m_settingsButton;
        protected MyGuiControlButton m_advancedButton;
        protected MyGuiControlButton m_modsButton;
        private MyGuiControlRotatingWheel m_loadingWheel;

        public MyGuiScreenServerSearchBase(MyGuiScreenJoinGame joinScreen) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.9398855f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.Padding = 0.02f;
            this.JoinScreen = joinScreen;
            this.CreateScreen();
        }

        protected unsafe MyGuiControlButton AddButton(MyStringId text, Action<MyGuiControlButton> onClick, MyStringId? tooltip = new MyStringId?(), bool enabled = true, bool addToParent = true)
        {
            Vector2? size = null;
            StringBuilder builder = MyTexts.Get(text);
            string toolTip = (tooltip != null) ? MyTexts.GetString(tooltip.Value) : string.Empty;
            int? buttonIndex = null;
            MyGuiControlButton control = new MyGuiControlButton(new Vector2?(this.CurrentPosition), MyGuiControlButtonStyleEnum.ToolbarButton, size, new VRageMath.Vector4?(Color.Yellow.ToVector4()), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, toolTip, builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                Enabled = enabled
            };
            control.PositionX += control.Size.X / 2f;
            if (addToParent)
            {
                this.Controls.Add(control);
            }
            else
            {
                this.Controls.Add(control);
            }
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += control.Size.Y + this.Padding;
            return control;
        }

        protected unsafe MyGuiControlCheckbox AddCheckbox(string text, Action<MyGuiControlCheckbox> onClick, string tooltip = null, string font = null, bool enabled = true)
        {
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox control = new MyGuiControlCheckbox(new Vector2?(this.CurrentPosition), color, tooltip ?? string.Empty, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            control.PositionX += (control.Size.X / 2f) + (this.Padding * 26f);
            this.Parent.Controls.Add(control);
            if (onClick != null)
            {
                control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, onClick);
            }
            Vector2? size = null;
            color = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(text), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER,
                PositionX = control.PositionX - (this.Padding * 25.8f)
            };
            if (!string.IsNullOrEmpty(tooltip))
            {
                label.SetToolTip(tooltip);
            }
            if (!string.IsNullOrEmpty(font))
            {
                label.Font = font;
            }
            this.Parent.Controls.Add(label);
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += control.Size.Y;
            control.Enabled = enabled;
            label.Enabled = enabled;
            return control;
        }

        protected MyGuiControlCheckbox AddCheckbox(MyStringId text, Action<MyGuiControlCheckbox> onClick, MyStringId? tooltip = new MyStringId?(), string font = null, bool enabled = true) => 
            this.AddCheckbox(MyTexts.GetString(text), onClick, (tooltip != null) ? MyTexts.GetString(tooltip.Value) : null, font, enabled);

        protected unsafe MyGuiControlCheckbox[] AddCheckboxDuo(MyStringId?[] text, Action<MyGuiControlCheckbox>[] onClick, MyStringId?[] tooltip, bool[] values)
        {
            VRageMath.Vector4? nullable;
            Vector2? nullable2;
            MyGuiControlCheckbox[] source = new MyGuiControlCheckbox[2];
            float x = this.CurrentPosition.X;
            if (text[0] != null)
            {
                nullable = null;
                MyGuiControlCheckbox control = new MyGuiControlCheckbox(new Vector2?(this.CurrentPosition), nullable, (tooltip[0] != null) ? MyTexts.GetString(tooltip[0].Value) : string.Empty, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                    PositionX = -0.0435f,
                    IsChecked = values[0]
                };
                source[0] = control;
                if (onClick[0] != null)
                {
                    control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, onClick[0]);
                }
                this.CurrentPosition.X = (control.PositionX + (control.Size.X / 2f)) + (this.Padding / 3f);
                nullable2 = null;
                nullable = null;
                MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(control.Position - new Vector2((control.Size.X / 2f) + (this.Padding * 10.45f), 0f)), nullable2, MyTexts.GetString(text[0].Value), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.Controls.Add(control);
                this.Controls.Add(label);
            }
            if (text[1] != null)
            {
                nullable = null;
                MyGuiControlCheckbox control = new MyGuiControlCheckbox(new Vector2?(this.CurrentPosition), nullable, (tooltip[1] != null) ? MyTexts.GetString(tooltip[1].Value) : string.Empty, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                    PositionX = 0.262f,
                    IsChecked = values[1]
                };
                source[1] = control;
                if (onClick[1] != null)
                {
                    control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, onClick[1]);
                }
                this.CurrentPosition.X = (control.PositionX + (control.Size.X / 2f)) + (this.Padding / 2f);
                nullable2 = null;
                nullable = null;
                MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?(control.Position - new Vector2((control.Size.X / 2f) + (this.Padding * 10.45f), 0f)), nullable2, MyTexts.GetString(text[1].Value), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.Controls.Add(control);
                this.Controls.Add(label2);
            }
            this.CurrentPosition.X = x;
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += ((source.First<MyGuiControlCheckbox>(c => (c != null)).Size.Y / 2f) + this.Padding) + 0.005f;
            return source;
        }

        protected unsafe MyGuiControlIndeterminateCheckbox[] AddIndeterminateDuo(MyStringId?[] text, Action<MyGuiControlIndeterminateCheckbox>[] onClick, MyStringId?[] tooltip, CheckStateEnum[] values, bool enabled = true)
        {
            VRageMath.Vector4? nullable;
            Vector2? nullable2;
            MyGuiControlIndeterminateCheckbox[] source = new MyGuiControlIndeterminateCheckbox[2];
            float x = this.CurrentPosition.X;
            if (text[0] != null)
            {
                nullable = null;
                MyGuiControlIndeterminateCheckbox control = new MyGuiControlIndeterminateCheckbox(new Vector2?(this.CurrentPosition), nullable, (tooltip[0] != null) ? MyTexts.GetString(tooltip[0].Value) : string.Empty, CheckStateEnum.Unchecked, MyGuiControlIndeterminateCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                    PositionX = -0.0435f,
                    State = values[0]
                };
                source[0] = control;
                if (onClick[0] != null)
                {
                    control.IsCheckedChanged = (Action<MyGuiControlIndeterminateCheckbox>) Delegate.Combine(control.IsCheckedChanged, onClick[0]);
                }
                this.CurrentPosition.X = (control.PositionX + (control.Size.X / 2f)) + (this.Padding / 3f);
                nullable2 = null;
                nullable = null;
                MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(control.Position - new Vector2((control.Size.X / 2f) + (this.Padding * 10.45f), 0f)), nullable2, MyTexts.GetString(text[0].Value), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                control.Enabled = enabled;
                label.Enabled = enabled;
                this.Controls.Add(control);
                this.Controls.Add(label);
            }
            if (text[1] != null)
            {
                nullable = null;
                MyGuiControlIndeterminateCheckbox control = new MyGuiControlIndeterminateCheckbox(new Vector2?(this.CurrentPosition), nullable, (tooltip[1] != null) ? MyTexts.GetString(tooltip[1].Value) : string.Empty, CheckStateEnum.Unchecked, MyGuiControlIndeterminateCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                    PositionX = 0.262f,
                    State = values[1]
                };
                source[1] = control;
                if (onClick[1] != null)
                {
                    control.IsCheckedChanged = (Action<MyGuiControlIndeterminateCheckbox>) Delegate.Combine(control.IsCheckedChanged, onClick[1]);
                }
                this.CurrentPosition.X = (control.PositionX + (control.Size.X / 2f)) + (this.Padding / 2f);
                nullable2 = null;
                nullable = null;
                MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?(control.Position - new Vector2((control.Size.X / 2f) + (this.Padding * 10.45f), 0f)), nullable2, MyTexts.GetString(text[1].Value), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                control.Enabled = enabled;
                label2.Enabled = enabled;
                this.Controls.Add(control);
                this.Controls.Add(label2);
            }
            this.CurrentPosition.X = x;
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += ((source.First<MyGuiControlIndeterminateCheckbox>(c => (c != null)).Size.Y / 2f) + this.Padding) + 0.005f;
            return source;
        }

        protected unsafe void AddNumericRangeOption(MyStringId text, Action<SerializableRange> onEntry, SerializableRange currentRange, bool active, Action<MyGuiControlCheckbox> onEnable, bool enabled = true)
        {
            float x = this.CurrentPosition.X;
            this.CurrentPosition.X = (-this.WindowSize.X / 2f) + (this.Padding * 12.6f);
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox control = new MyGuiControlCheckbox(new Vector2?(this.CurrentPosition + new Vector2(0f, 0.003f)), color, MyTexts.GetString(MyCommonTexts.ServerSearch_EnableNumericTooltip), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            control.PositionX += control.Size.X / 2f;
            control.IsChecked = active;
            control.Enabled = true;
            control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, onEnable);
            this.Controls.Add(control);
            float* singlePtr1 = (float*) ref this.CurrentPosition.X;
            singlePtr1[0] += (control.Size.X / 2f) + this.Padding;
            color = null;
            MyGuiControlTextbox minText = new MyGuiControlTextbox(new Vector2?(this.CurrentPosition), currentRange.Min.ToString(), 6, color, 0.8f, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            minText.Size = new Vector2(0.12f, minText.Size.Y);
            minText.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_MinimumFilterValue));
            minText.Enabled = control.IsChecked;
            this.Controls.Add(minText);
            float* singlePtr2 = (float*) ref this.CurrentPosition.X;
            singlePtr2[0] += ((minText.Size.X / 1.5f) + this.Padding) + 0.028f;
            Vector2? size = null;
            color = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, "-", color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(label);
            float* singlePtr3 = (float*) ref this.CurrentPosition.X;
            singlePtr3[0] += (label.Size.X / 2f) + (this.Padding / 2f);
            color = null;
            MyGuiControlTextbox maxText = new MyGuiControlTextbox(new Vector2?(this.CurrentPosition), float.IsInfinity(currentRange.Max) ? "-1" : currentRange.Max.ToString(), 6, color, 0.8f, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            maxText.Size = new Vector2(0.12f, maxText.Size.Y);
            maxText.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_MaximumFilterValue));
            maxText.Enabled = control.IsChecked;
            this.Controls.Add(maxText);
            float* singlePtr4 = (float*) ref this.CurrentPosition.X;
            singlePtr4[0] += ((maxText.Size.X / 1.5f) + this.Padding) + 0.01f;
            size = null;
            color = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(-0.27f, this.CurrentPosition.Y), size, MyTexts.GetString(text), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                Enabled = true
            };
            this.Controls.Add(label2);
            this.CurrentPosition.X = x;
            float* singlePtr5 = (float*) ref this.CurrentPosition.Y;
            singlePtr5[0] += label2.Size.Y + this.Padding;
            control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, c => minText.Enabled = maxText.Enabled = c.IsChecked);
            if (onEntry != null)
            {
                maxText.TextChanged += delegate (MyGuiControlTextbox t) {
                    float num;
                    float positiveInfinity;
                    if (float.TryParse(minText.Text, out num) && float.TryParse(maxText.Text, out positiveInfinity))
                    {
                        if (positiveInfinity == -1f)
                        {
                            positiveInfinity = float.PositiveInfinity;
                        }
                        if (num < 0f)
                        {
                            num = 0f;
                        }
                        onEntry(new SerializableRange(num, positiveInfinity));
                    }
                };
                minText.TextChanged += delegate (MyGuiControlTextbox t) {
                    float num;
                    float positiveInfinity;
                    if (float.TryParse(minText.Text, out num) && float.TryParse(maxText.Text, out positiveInfinity))
                    {
                        if (positiveInfinity == -1f)
                        {
                            positiveInfinity = float.PositiveInfinity;
                        }
                        if (num < 0f)
                        {
                            num = 0f;
                        }
                        onEntry(new SerializableRange(num, positiveInfinity));
                    }
                };
            }
        }

        private void AdvancedButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CurrentPage = SearchPageEnum.Advanced;
            this.RecreateControls(false);
        }

        private void CancelButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CloseScreen();
        }

        private void CreateScreen()
        {
            base.CanHideOthers = true;
            base.CanBeHidden = true;
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        private void DefaultModsClick(MyGuiControlButton myGuiControlButton)
        {
            this.FilterOptions.Mods.Clear();
            this.RecreateControls(false);
        }

        private void DefaultSettingsClick(MyGuiControlButton myGuiControlButton)
        {
            this.FilterOptions.SetDefaults(false);
            this.RecreateControls(false);
        }

        private unsafe void DrawAdvancedSelector()
        {
            VRageMath.Vector4? color = null;
            this.m_advancedCheckbox = new MyGuiControlCheckbox(new Vector2(-0.0435f, -0.279f), color, MyTexts.GetString(MyCommonTexts.ServerSearch_EnableAdvancedTooltip), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_advancedCheckbox.IsChecked = this.FilterOptions.AdvancedFilter;
            this.m_advancedCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_advancedCheckbox.IsCheckedChanged, delegate (MyGuiControlCheckbox c) {
                this.FilterOptions.AdvancedFilter = c.IsChecked;
                this.RecreateControls(false);
            });
            this.m_advancedCheckbox.Enabled = this.JoinScreen.EnableAdvancedSearch;
            this.Controls.Add(this.m_advancedCheckbox);
            Vector2? size = null;
            color = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.m_advancedCheckbox.Position - new Vector2((this.m_advancedCheckbox.Size.X / 2f) + (this.Padding * 10.45f), 0f)), size, MyTexts.GetString(MyCommonTexts.ServerSearch_EnableAdvanced), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            control.SetToolTip(MyCommonTexts.ServerSearch_EnableAdvancedTooltip);
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            control.Enabled = this.JoinScreen.EnableAdvancedSearch;
            this.Controls.Add(control);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            color = null;
            list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.23f), base.m_size.Value.X * 0.835f, 0f, color);
            this.Controls.Add(list);
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += 0.07f;
        }

        protected virtual void DrawBottomControls()
        {
        }

        protected virtual unsafe void DrawMidControls()
        {
            Vector2 currentPosition = this.CurrentPosition;
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += this.Padding * 1.32f;
            float* singlePtr2 = (float*) ref this.CurrentPosition.X;
            singlePtr2[0] += this.Padding / 2.4f;
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(MyCommonTexts.JoinGame_ColumnTitle_Ping), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                Enabled = this.JoinScreen.EnableAdvancedSearch
            };
            this.Controls.Add(control);
            float* singlePtr3 = (float*) ref this.CurrentPosition.X;
            singlePtr3[0] += this.Padding * 2.3f;
            colorMask = null;
            MyGuiControlSlider slider = new MyGuiControlSlider(new Vector2?(this.CurrentPosition + new Vector2(0.215f, 0f)), -1f, 1000f, 0.29f, new float?((float) this.FilterOptions.Ping), colorMask, string.Empty, 1, 0f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER,
                LabelDecimalPlaces = 0,
                IntValue = true,
                Size = new Vector2(0.45f - control.Size.X, 1f)
            };
            slider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_Ping));
            slider.PositionX += slider.Size.X / 2f;
            slider.Enabled = this.JoinScreen.EnableAdvancedSearch;
            this.Controls.Add(slider);
            float* singlePtr4 = (float*) ref this.CurrentPosition.X;
            singlePtr4[0] += (slider.Size.X / 2f) + (this.Padding * 14f);
            size = null;
            colorMask = null;
            MyGuiControlLabel val = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, "<" + slider.Value + "ms", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            slider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(slider.ValueChanged, delegate (MyGuiControlSlider x) {
                val.Text = "<" + x.Value + "ms";
                this.FilterOptions.Ping = (int) x.Value;
            });
            val.Enabled = this.JoinScreen.EnableAdvancedSearch;
            this.Controls.Add(val);
            this.CurrentPosition = currentPosition;
            float* singlePtr5 = (float*) ref this.CurrentPosition.Y;
            singlePtr5[0] += 0.04f;
        }

        private unsafe void DrawModSelector()
        {
            List<MyWorkshopItem>.Enumerator enumerator;
            this.Parent = new MyGuiControlParent();
            this.Panel = new MyGuiControlScrollablePanel(this.Parent);
            this.Panel.ScrollbarVEnabled = true;
            this.Panel.PositionX += 0.0075f;
            this.Panel.PositionY += (this.m_settingsButton.Size.Y / 2f) + (this.Padding * 1.7f);
            Vector2? size = base.Size;
            this.Panel.Size = new Vector2(base.Size.Value.X - 0.1f, (size.Value.Y - (this.m_settingsButton.Size.Y * 2f)) - (this.Padding * 13.7f));
            this.Controls.Add(this.Panel);
            VRageMath.Vector4? color = null;
            this.m_advancedCheckbox = new MyGuiControlCheckbox(new Vector2(-0.0435f, -0.279f), color, MyTexts.GetString(MyCommonTexts.ServerSearch_EnableAdvancedTooltip), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_advancedCheckbox.IsChecked = this.FilterOptions.AdvancedFilter;
            this.m_advancedCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_advancedCheckbox.IsCheckedChanged, delegate (MyGuiControlCheckbox c) {
                this.FilterOptions.AdvancedFilter = c.IsChecked;
                this.RecreateControls(false);
            });
            this.m_advancedCheckbox.Enabled = this.JoinScreen.EnableAdvancedSearch;
            this.Controls.Add(this.m_advancedCheckbox);
            size = null;
            color = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.m_advancedCheckbox.Position - new Vector2((this.m_advancedCheckbox.Size.X / 2f) + (this.Padding * 10.45f), 0f)), size, MyTexts.GetString(MyCommonTexts.ServerSearch_EnableAdvanced), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            control.SetToolTip(MyCommonTexts.ServerSearch_EnableAdvancedTooltip);
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            control.Enabled = this.JoinScreen.EnableAdvancedSearch;
            this.Controls.Add(control);
            color = null;
            MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(new Vector2(0.2465f, -0.279f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                IsChecked = this.FilterOptions.ModsExclusive
            };
            checkbox.SetToolTip(MyCommonTexts.ServerSearch_ExclusiveTooltip);
            checkbox.IsCheckedChanged = c => this.FilterOptions.ModsExclusive = c.IsChecked;
            checkbox.Enabled = true;
            size = null;
            color = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?(checkbox.Position - new Vector2((checkbox.Size.X / 2f) + (this.Padding * 10.45f), 0f)), size, MyTexts.GetString(MyCommonTexts.ServerSearch_Exclusive), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label2.SetToolTip(MyCommonTexts.ServerSearch_ExclusiveTooltip);
            label2.Enabled = true;
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.Controls.Add(checkbox);
            this.Controls.Add(label2);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            color = null;
            list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.23f), base.m_size.Value.X * 0.835f, 0f, color);
            this.Controls.Add(list);
            size = null;
            size = null;
            color = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(size, MyGuiControlButtonStyleEnum.Small, size, color, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ServerSearch_Clear), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            color = null;
            MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(new Vector2?(this.CurrentPosition), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            float y = ((checkbox2.Size.Y * (m_subscribedMods.Count + m_settingsMods.Count)) + (button.Size.Y / 2f)) + this.Padding;
            this.CurrentPosition = -this.Panel.Size / 2f;
            this.CurrentPosition.Y = ((-y / 2f) + (checkbox2.Size.Y / 2f)) - 0.005f;
            float* singlePtr1 = (float*) ref this.CurrentPosition.X;
            singlePtr1[0] -= 0.0225f;
            this.Parent.Size = new Vector2(this.Panel.Size.X, y);
            m_subscribedMods.Sort((a, b) => a.Title.CompareTo(b.Title));
            m_settingsMods.Sort((a, b) => a.Title.CompareTo(b.Title));
            button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button.ButtonClicked += new Action<MyGuiControlButton>(this.DefaultModsClick);
            button.Position = this.CurrentPosition + new Vector2(this.Padding, -this.Padding * 6f);
            this.Parent.Controls.Add(button);
            using (enumerator = m_subscribedMods.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyWorkshopItem mod;
                    int num2 = Math.Min(mod.Description.Length, 0x80);
                    int index = mod.Description.IndexOf("\n");
                    if (index > 0)
                    {
                        num2 = Math.Min(num2, index - 1);
                    }
                    MyGuiControlCheckbox checkbox1 = this.AddCheckbox(mod.Title, c => this.ModCheckboxClick(c, mod.Id), mod.Description.Substring(0, num2), null, true);
                    checkbox1.IsChecked = this.FilterOptions.Mods.Contains(mod.Id);
                    checkbox1.Enabled = this.FilterOptions.AdvancedFilter;
                }
            }
            using (enumerator = m_settingsMods.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyWorkshopItem item1;
                    int num4 = Math.Min(item1.Description.Length, 0x80);
                    int index = item1.Description.IndexOf("\n");
                    if (index > 0)
                    {
                        num4 = Math.Min(num4, index - 1);
                    }
                    MyGuiControlCheckbox checkbox3 = this.AddCheckbox(item1.Title, c => this.ModCheckboxClick(c, item1.Id), item1.Description.Substring(0, num4), "DarkBlue", this.EnableAdvanced);
                    checkbox3.IsChecked = this.FilterOptions.Mods.Contains(item1.Id);
                    checkbox3.Enabled = this.FilterOptions.AdvancedFilter;
                }
            }
        }

        private void DrawSettingsSelector()
        {
            int num1;
            this.CurrentPosition.Y = -0.279f;
            MyStringId?[] text = new MyStringId?[] { new MyStringId?(MyCommonTexts.WorldSettings_GameModeCreative), new MyStringId?(MyCommonTexts.WorldSettings_GameModeSurvival) };
            Action<MyGuiControlCheckbox>[] onClick = new Action<MyGuiControlCheckbox>[] { c => this.FilterOptions.CreativeMode = c.IsChecked, c => this.FilterOptions.SurvivalMode = c.IsChecked };
            MyStringId?[] tooltip = new MyStringId?[] { new MyStringId?(MySpaceTexts.ToolTipJoinGameServerSearch_Creative), new MyStringId?(MySpaceTexts.ToolTipJoinGameServerSearch_Survival) };
            bool[] values = new bool[] { this.FilterOptions.CreativeMode, this.FilterOptions.SurvivalMode };
            this.AddCheckboxDuo(text, onClick, tooltip, values);
            MyStringId?[] nullableArray3 = new MyStringId?[] { new MyStringId?(MyCommonTexts.MultiplayerCompatibleVersions), new MyStringId?(MyCommonTexts.MultiplayerJoinSameGameData) };
            Action<MyGuiControlCheckbox>[] actionArray2 = new Action<MyGuiControlCheckbox>[] { c => this.FilterOptions.SameVersion = c.IsChecked, c => this.FilterOptions.SameData = c.IsChecked };
            MyStringId?[] nullableArray4 = new MyStringId?[] { new MyStringId?(MySpaceTexts.ToolTipJoinGameServerSearch_CompatibleVersions), new MyStringId?(MySpaceTexts.ToolTipJoinGameServerSearch_SameGameData) };
            bool[] flagArray2 = new bool[] { this.FilterOptions.SameVersion, this.FilterOptions.SameData };
            this.AddCheckboxDuo(nullableArray3, actionArray2, nullableArray4, flagArray2);
            Vector2 currentPosition = this.CurrentPosition;
            MyStringId?[] nullableArray5 = new MyStringId?[2];
            nullableArray5[0] = new MyStringId?(MyCommonTexts.MultiplayerJoinAllowedGroups);
            Action<MyGuiControlCheckbox>[] actionArray3 = new Action<MyGuiControlCheckbox>[] { c => this.FilterOptions.AllowedGroups = c.IsChecked };
            MyStringId?[] nullableArray6 = new MyStringId?[] { new MyStringId?(MySpaceTexts.ToolTipJoinGameServerSearch_AllowedGroups) };
            bool[] flagArray3 = new bool[] { this.FilterOptions.AllowedGroups };
            this.AddCheckboxDuo(nullableArray5, actionArray3, nullableArray6, flagArray3);
            this.CurrentPosition = currentPosition;
            MyStringId?[] nullableArray7 = new MyStringId?[2];
            nullableArray7[1] = new MyStringId?(MyCommonTexts.MultiplayerJoinHasPassword);
            Action<MyGuiControlIndeterminateCheckbox>[] actionArray4 = new Action<MyGuiControlIndeterminateCheckbox>[2];
            actionArray4[1] = delegate (MyGuiControlIndeterminateCheckbox c) {
                switch (c.State)
                {
                    case CheckStateEnum.Checked:
                        this.FilterOptions.HasPassword = true;
                        return;

                    case CheckStateEnum.Unchecked:
                        this.FilterOptions.HasPassword = false;
                        return;

                    case CheckStateEnum.Indeterminate:
                        this.FilterOptions.HasPassword = null;
                        return;
                }
            };
            MyStringId?[] nullableArray8 = new MyStringId?[2];
            nullableArray8[1] = new MyStringId?(MySpaceTexts.ToolTipJoinGameServerSearch_HasPassword);
            CheckStateEnum[] enumArray1 = new CheckStateEnum[2];
            enumArray1[0] = CheckStateEnum.Indeterminate;
            CheckStateEnum[] enumArray2 = enumArray1;
            if ((this.FilterOptions.HasPassword == null) || !this.FilterOptions.HasPassword.Value)
            {
                num1 = (this.FilterOptions.HasPassword != null) ? 1 : 2;
            }
            else
            {
                num1 = 0;
            }
            enumArray2[1] = (CheckStateEnum) num1;
            this.AddIndeterminateDuo(nullableArray7, actionArray4, nullableArray8, enumArray2, true);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.325f), base.m_size.Value.X * 0.835f, 0f, color);
            this.Controls.Add(control);
            control = new MyGuiControlSeparatorList();
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.409f), base.m_size.Value.X * 0.835f, 0f, color);
            this.Controls.Add(control);
            control = new MyGuiControlSeparatorList();
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.657f), base.m_size.Value.X * 0.835f, 0f, color);
            this.Controls.Add(control);
        }

        protected virtual void DrawTopControls()
        {
            this.CurrentPosition.Y = -0.0225f;
            this.AddNumericRangeOption(MyCommonTexts.MultiplayerJoinOnlinePlayers, r => this.FilterOptions.PlayerCount = r, this.FilterOptions.PlayerCount, this.FilterOptions.CheckPlayer, c => this.FilterOptions.CheckPlayer = c.IsChecked, true);
            this.AddNumericRangeOption(MyCommonTexts.JoinGame_ColumnTitle_Mods, r => this.FilterOptions.ModCount = r, this.FilterOptions.ModCount, this.FilterOptions.CheckMod, c => this.FilterOptions.CheckMod = c.IsChecked, true);
            this.AddNumericRangeOption(MySpaceTexts.WorldSettings_ViewDistance, r => this.FilterOptions.ViewDistance = r, this.FilterOptions.ViewDistance, this.FilterOptions.CheckDistance, c => this.FilterOptions.CheckDistance = c.IsChecked, true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenServerSearchBase";

        private IMyAsyncResult LoadModsBeginAction() => 
            new MyModsLoadListResult(this.FilterOptions.Mods);

        private void LoadModsEndAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            MyModsLoadListResult result1 = (MyModsLoadListResult) result;
            m_subscribedMods = result1.SubscribedMods;
            m_settingsMods = result1.SetMods;
            screen.CloseScreen();
            this.m_loadingWheel.Visible = false;
            this.RecreateControls(false);
        }

        private void ModCheckboxClick(MyGuiControlCheckbox c, ulong modId)
        {
            if (c.IsChecked)
            {
                this.FilterOptions.Mods.Add(modId);
            }
            else
            {
                this.FilterOptions.Mods.Remove(modId);
            }
        }

        private void ModsButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CurrentPage = SearchPageEnum.Mods;
            if (!m_needsModRefresh)
            {
                this.RecreateControls(false);
                this.m_loadingWheel.Visible = false;
            }
            else
            {
                MyStringId? cancelText = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.LoadModsBeginAction), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.LoadModsEndAction), null));
                m_needsModRefresh = false;
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ServerSearch, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(list2);
            MyGuiControlSeparatorList list3 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list3.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.15f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(list3);
            this.CurrentPosition = new Vector2(0f, 0f) - new Vector2(((base.m_size.Value.X * 0.835f) / 2f) - 0.003f, (base.m_size.Value.Y / 2f) - 0.095f);
            float y = this.CurrentPosition.Y;
            MyStringId? tooltip = null;
            this.m_settingsButton = this.AddButton(MyCommonTexts.ServerDetails_Settings, new Action<MyGuiControlButton>(this.SettingsButtonClick), tooltip, true, false);
            this.m_settingsButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_Settings));
            this.CurrentPosition.Y = y;
            float* singlePtr1 = (float*) ref this.CurrentPosition.X;
            singlePtr1[0] += this.m_settingsButton.Size.X + (this.Padding / 3.6f);
            tooltip = null;
            this.m_advancedButton = this.AddButton(MyCommonTexts.Advanced, new Action<MyGuiControlButton>(this.AdvancedButtonClick), tooltip, true, false);
            this.m_advancedButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_Advanced));
            this.CurrentPosition.Y = y;
            float* singlePtr2 = (float*) ref this.CurrentPosition.X;
            singlePtr2[0] += this.m_settingsButton.Size.X + (this.Padding / 3.6f);
            tooltip = null;
            this.m_modsButton = this.AddButton(MyCommonTexts.WorldSettings_Mods, new Action<MyGuiControlButton>(this.ModsButtonClick), tooltip, true, false);
            this.m_modsButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_Mods));
            this.CurrentPosition.Y = y;
            float* singlePtr3 = (float*) ref this.CurrentPosition.X;
            singlePtr3[0] += this.m_settingsButton.Size.X + this.Padding;
            Vector2? textureResolution = null;
            this.m_loadingWheel = new MyGuiControlRotatingWheel(new Vector2?(this.m_modsButton.Position + new Vector2(0.137f, -0.004f)), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, textureResolution, 1.5f);
            this.Controls.Add(this.m_loadingWheel);
            this.m_loadingWheel.Visible = false;
            textureResolution = null;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2?(new Vector2(0f, 0f) - new Vector2(-0.003f, (-base.m_size.Value.Y / 2f) + 0.071f)), MyGuiControlButtonStyleEnum.Default, textureResolution, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ServerSearch_Defaults), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_Defaults));
            button.ButtonClicked += new Action<MyGuiControlButton>(this.DefaultSettingsClick);
            button.ButtonClicked += new Action<MyGuiControlButton>(this.DefaultModsClick);
            this.Controls.Add(button);
            textureResolution = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_searchButton = new MyGuiControlButton(new Vector2?(new Vector2(0f, 0f) - new Vector2(0.18f, (-base.m_size.Value.Y / 2f) + 0.071f)), MyGuiControlButtonStyleEnum.Default, textureResolution, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMods_SearchLabel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_searchButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_Search));
            this.m_searchButton.ButtonClicked += new Action<MyGuiControlButton>(this.SearchClick);
            this.Controls.Add(this.m_searchButton);
            this.CurrentPosition = -this.WindowSize / 2f;
            switch (this.CurrentPage)
            {
                case SearchPageEnum.Settings:
                {
                    base.FocusedControl = this.m_settingsButton;
                    this.m_settingsButton.HighlightType = MyGuiControlHighlightType.FORCED;
                    this.m_settingsButton.HasHighlight = true;
                    this.m_settingsButton.Selected = true;
                    float* singlePtr4 = (float*) ref this.CurrentPosition.Y;
                    singlePtr4[0] += this.Padding * 2f;
                    this.DrawSettingsSelector();
                    this.DrawTopControls();
                    this.DrawMidControls();
                    return;
                }
                case SearchPageEnum.Advanced:
                    base.FocusedControl = this.m_advancedButton;
                    this.m_advancedButton.HighlightType = MyGuiControlHighlightType.FORCED;
                    this.m_advancedButton.HasHighlight = true;
                    this.m_advancedButton.Selected = true;
                    this.DrawAdvancedSelector();
                    this.DrawBottomControls();
                    return;

                case SearchPageEnum.Mods:
                    base.FocusedControl = this.m_modsButton;
                    this.m_modsButton.HighlightType = MyGuiControlHighlightType.FORCED;
                    this.m_modsButton.HasHighlight = true;
                    this.m_modsButton.Selected = true;
                    this.DrawModSelector();
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        private void SearchClick(MyGuiControlButton myGuiControlButton)
        {
            this.CloseScreen();
        }

        private void SettingsButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CurrentPage = SearchPageEnum.Settings;
            this.RecreateControls(false);
        }

        protected bool EnableAdvanced =>
            (this.FilterOptions.AdvancedFilter && this.JoinScreen.EnableAdvancedSearch);

        protected Vector2 WindowSize =>
            new Vector2(base.Size.Value.X - 0.1f, (base.Size.Value.Y - (this.m_settingsButton.Size.Y * 2f)) - (this.Padding * 16f));

        protected MyServerFilterOptions FilterOptions
        {
            get => 
                this.JoinScreen.FilterOptions;
            set => 
                (this.JoinScreen.FilterOptions = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenServerSearchBase.<>c <>9 = new MyGuiScreenServerSearchBase.<>c();
            public static Comparison<MyWorkshopItem> <>9__31_1;
            public static Comparison<MyWorkshopItem> <>9__31_2;
            public static Func<MyGuiControlCheckbox, bool> <>9__47_0;
            public static Func<MyGuiControlIndeterminateCheckbox, bool> <>9__48_0;

            internal bool <AddCheckboxDuo>b__47_0(MyGuiControlCheckbox c) => 
                (c != null);

            internal bool <AddIndeterminateDuo>b__48_0(MyGuiControlIndeterminateCheckbox c) => 
                (c != null);

            internal int <DrawModSelector>b__31_1(MyWorkshopItem a, MyWorkshopItem b) => 
                a.Title.CompareTo(b.Title);

            internal int <DrawModSelector>b__31_2(MyWorkshopItem a, MyWorkshopItem b) => 
                a.Title.CompareTo(b.Title);
        }

        protected enum SearchPageEnum
        {
            Settings,
            Advanced,
            Mods
        }
    }
}

