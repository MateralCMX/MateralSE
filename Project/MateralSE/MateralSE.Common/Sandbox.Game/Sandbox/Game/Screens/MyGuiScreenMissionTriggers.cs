namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenMissionTriggers : MyGuiScreenBase
    {
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlLabel m_videoLabel;
        protected MyGuiControlTextbox m_videoTextbox;
        private MyGuiControlCombobox[] m_winCombo;
        private MyGuiControlCombobox[] m_loseCombo;
        private MyTrigger[] m_winTrigger;
        private MyGuiControlButton[] m_winButton;
        private MyTrigger[] m_loseTrigger;
        private MyGuiControlButton[] m_loseButton;
        private MyGuiScreenAdvancedScenarioSettings m_advanced;
        private static List<System.Type> m_triggerTypes = GetTriggerTypes();

        public MyGuiScreenMissionTriggers() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.8f, 0.8f), false, null, 0f, 0f)
        {
            this.m_winCombo = new MyGuiControlCombobox[6];
            this.m_loseCombo = new MyGuiControlCombobox[6];
            this.m_winTrigger = new MyTrigger[6];
            this.m_winButton = new MyGuiControlButton[6];
            this.m_loseTrigger = new MyTrigger[6];
            this.m_loseButton = new MyGuiControlButton[6];
            this.RecreateControls(true);
        }

        protected MyGuiControlCompositePanel AddCompositePanel(MyGuiCompositeTexture texture, Vector2 position, Vector2 size, MyGuiDrawAlignEnum panelAlign)
        {
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.BackgroundTexture = texture;
            MyGuiControlCompositePanel control = panel1;
            control.Position = position;
            control.Size = size;
            control.OriginAlign = panelAlign;
            this.Controls.Add(control);
            return control;
        }

        public override bool CloseScreen()
        {
            this.m_videoTextbox.TextChanged -= new Action<MyGuiControlTextbox>(this.OnVideoTextboxChanged);
            return base.CloseScreen();
        }

        private MyTrigger CreateNew(long hash)
        {
            using (List<System.Type>.Enumerator enumerator = m_triggerTypes.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    System.Type current = enumerator.Current;
                    if (current.GetHashCode() == hash)
                    {
                        return (MyTrigger) Activator.CreateInstance(current);
                    }
                }
            }
            return null;
        }

        private int getButtonNr(object sender)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((sender == this.m_winButton[i]) || (sender == this.m_loseButton[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMissionTriggers";

        public static List<System.Type> GetTriggerTypes() => 
            (from type in Assembly.GetCallingAssembly().GetTypes()
                where type.IsSubclassOf(typeof(MyTrigger)) && (MyFakes.ENABLE_NEW_TRIGGERS || ((type != typeof(MyTriggerTimeLimit)) && (type != typeof(MyTriggerBlockDestroyed))))
                select type).ToList<System.Type>();

        private void OnAdvancedButtonClick(object sender)
        {
            this.m_advanced = new MyGuiScreenAdvancedScenarioSettings(this);
            MyGuiSandbox.AddScreen(this.m_advanced);
        }

        private void OnCancelButtonClick(object sender)
        {
            this.CloseScreen();
        }

        private void OnLoseComboSelect()
        {
            for (int i = 0; i < 6; i++)
            {
                if ((this.m_loseTrigger[i] == null) && (this.m_loseCombo[i].GetSelectedKey() != -1L))
                {
                    this.m_loseTrigger[i] = this.CreateNew(this.m_loseCombo[i].GetSelectedKey());
                }
                else if ((this.m_loseTrigger[i] != null) && (this.m_loseCombo[i].GetSelectedKey() == -1L))
                {
                    this.m_loseTrigger[i] = null;
                }
                else if ((this.m_loseTrigger[i] != null) && (this.m_loseCombo[i].GetSelectedKey() != this.m_loseTrigger[i].GetType().GetHashCode()))
                {
                    this.m_loseTrigger[i] = this.CreateNew(this.m_loseCombo[i].GetSelectedKey());
                }
                this.m_loseButton[i].Enabled = this.m_loseCombo[i].GetSelectedKey() != -1L;
            }
        }

        private void OnLoseEditButtonClick(object sender)
        {
            this.m_loseTrigger[this.getButtonNr(sender)].DisplayGUI();
        }

        private void OnOkButtonClick(object sender)
        {
            this.SaveData();
            this.CloseScreen();
        }

        private void OnVideoTextboxChanged(MyGuiControlTextbox source)
        {
            if ((source.Text.Length == 0) || MyGuiSandbox.IsUrlWhitelisted(source.Text))
            {
                source.SetToolTip((MyToolTips) null);
                source.ColorMask = VRageMath.Vector4.One;
                this.m_okButton.Enabled = true;
            }
            else
            {
                MyStringId text = !MySession.Platform.Equals("Steam") ? MySpaceTexts.WwwLinkNotAllowed : MySpaceTexts.WwwLinkNotAllowed_Steam;
                source.SetToolTip(text);
                source.ColorMask = Color.Red.ToVector4();
                this.m_okButton.Enabled = false;
            }
        }

        private void OnWinComboSelect()
        {
            for (int i = 0; i < 6; i++)
            {
                if ((this.m_winTrigger[i] == null) && (this.m_winCombo[i].GetSelectedKey() != -1L))
                {
                    this.m_winTrigger[i] = this.CreateNew(this.m_winCombo[i].GetSelectedKey());
                }
                else if ((this.m_winTrigger[i] != null) && (this.m_winCombo[i].GetSelectedKey() == -1L))
                {
                    this.m_winTrigger[i] = null;
                }
                else if ((this.m_winTrigger[i] != null) && (this.m_winCombo[i].GetSelectedKey() != this.m_winTrigger[i].GetType().GetHashCode()))
                {
                    this.m_winTrigger[i] = this.CreateNew(this.m_winCombo[i].GetSelectedKey());
                }
                this.m_winButton[i].Enabled = this.m_winCombo[i].GetSelectedKey() != -1L;
            }
        }

        private void OnWinEditButtonClick(object sender)
        {
            this.m_winTrigger[this.getButtonNr(sender)].DisplayGUI();
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.MissionScreenCaption, captionTextColor, captionOffset, 0.8f);
            this.AddCompositePanel(MyGuiConstants.TEXTURE_RECTANGLE_DARK, new Vector2(0f, 0.08f), new Vector2(0.75f, 0.45f), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(new Vector2(0.17f, 0.37f), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Refresh), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            captionTextColor = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(0.38f, 0.37f), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            captionOffset = null;
            captionTextColor = null;
            this.m_videoLabel = new MyGuiControlLabel(new Vector2(-0.375f, -0.18f), captionOffset, MyTexts.Get(MySpaceTexts.GuiLabelVideoOnStart).ToString(), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            captionTextColor = null;
            this.m_videoTextbox = new MyGuiControlTextbox(new Vector2?(this.m_videoLabel.Position), MySession.Static.BriefingVideo, 0x55, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.Controls.Add(this.m_videoLabel);
            this.Controls.Add(this.m_videoTextbox);
            this.m_videoTextbox.PositionX = ((this.m_videoLabel.Position.X + this.m_videoLabel.Size.X) + (this.m_videoTextbox.Size.X / 2f)) + 0.03f;
            this.m_videoTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.OnVideoTextboxChanged);
            this.OnVideoTextboxChanged(this.m_videoTextbox);
            vector = new Vector2(0.05f, 0.05f);
            Vector2 vector2 = new Vector2(0.15f, -0.05f);
            captionTextColor = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2(vector2.X - 0.37f, vector2.Y - 0.06f), new Vector2?(new Vector2(455f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), MyTexts.Get(MySpaceTexts.GuiMissionTriggersWinCondition).ToString(), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(control);
            captionTextColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(vector2.X, vector2.Y - 0.06f), new Vector2?(new Vector2(455f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), MyTexts.Get(MySpaceTexts.GuiMissionTriggersLostCondition).ToString(), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(label2);
            for (int i = 0; i < 6; i++)
            {
                float* singlePtr1 = (float*) ref vector2.X;
                singlePtr1[0] -= 0.37f;
                captionOffset = null;
                captionTextColor = null;
                captionOffset = null;
                captionOffset = null;
                captionTextColor = null;
                this.m_winCombo[i] = new MyGuiControlCombobox(new Vector2?(vector2), captionOffset, captionTextColor, captionOffset, 10, captionOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                this.m_winCombo[i].ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnWinComboSelect);
                buttonIndex = null;
                this.m_winCombo[i].AddItem(-1L, "", buttonIndex, null);
                foreach (System.Type type in m_triggerTypes)
                {
                    buttonIndex = null;
                    this.m_winCombo[i].AddItem((long) type.GetHashCode(), MyTexts.Get((MyStringId) type.GetMethod("GetCaption").Invoke(null, null)), buttonIndex, null);
                }
                this.Controls.Add(this.m_winCombo[i]);
                captionTextColor = null;
                buttonIndex = null;
                this.m_winButton[i] = new MyGuiControlButton(new Vector2(vector2.X + 0.15f, vector2.Y), MyGuiControlButtonStyleEnum.Tiny, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, new StringBuilder("*"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnWinEditButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                this.m_winButton[i].Enabled = false;
                this.Controls.Add(this.m_winButton[i]);
                float* singlePtr2 = (float*) ref vector2.X;
                singlePtr2[0] += 0.37f;
                captionOffset = null;
                captionTextColor = null;
                captionOffset = null;
                captionOffset = null;
                captionTextColor = null;
                this.m_loseCombo[i] = new MyGuiControlCombobox(new Vector2?(vector2), captionOffset, captionTextColor, captionOffset, 10, captionOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                this.m_loseCombo[i].ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnLoseComboSelect);
                buttonIndex = null;
                this.m_loseCombo[i].AddItem(-1L, "", buttonIndex, null);
                foreach (System.Type type2 in m_triggerTypes)
                {
                    type2.GetMethod("GetFriendlyName");
                    buttonIndex = null;
                    this.m_loseCombo[i].AddItem((long) type2.GetHashCode(), MyTexts.Get((MyStringId) type2.GetMethod("GetCaption").Invoke(null, null)), buttonIndex, null);
                }
                this.Controls.Add(this.m_loseCombo[i]);
                captionTextColor = null;
                buttonIndex = null;
                this.m_loseButton[i] = new MyGuiControlButton(new Vector2(vector2.X + 0.15f, vector2.Y), MyGuiControlButtonStyleEnum.Tiny, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, new StringBuilder("*"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnLoseEditButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                this.m_loseButton[i].Enabled = false;
                this.Controls.Add(this.m_loseButton[i]);
                float* singlePtr3 = (float*) ref vector2.Y;
                singlePtr3[0] += 0.05f;
            }
            this.SetDefaultValues();
        }

        private void SaveData()
        {
            MySession.Static.BriefingVideo = this.m_videoTextbox.Text;
            foreach (KeyValuePair<MyPlayer.PlayerId, MyMissionTriggers> pair in MySessionComponentMissionTriggers.Static.MissionTriggers)
            {
                pair.Value.HideNotification();
            }
            MySessionComponentMissionTriggers.Static.MissionTriggers.Clear();
            MyMissionTriggers triggers = new MyMissionTriggers();
            MySessionComponentMissionTriggers.Static.MissionTriggers.Add(MyMissionTriggers.DefaultPlayerId, triggers);
            for (int i = 0; i < 6; i++)
            {
                if (this.m_winTrigger[i] != null)
                {
                    triggers.WinTriggers.Add(this.m_winTrigger[i]);
                }
                if (this.m_loseTrigger[i] != null)
                {
                    triggers.LoseTriggers.Add(this.m_loseTrigger[i]);
                }
            }
        }

        private void SetDefaultValues()
        {
            MyMissionTriggers triggers;
            if (!MySessionComponentMissionTriggers.Static.MissionTriggers.TryGetValue(MyMissionTriggers.DefaultPlayerId, out triggers))
            {
                triggers = new MyMissionTriggers();
                MySessionComponentMissionTriggers.Static.MissionTriggers.Add(MyMissionTriggers.DefaultPlayerId, triggers);
            }
            else
            {
                int index = 0;
                foreach (MyTrigger trigger in triggers.WinTriggers)
                {
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 < this.m_winCombo[index].GetItemsCount())
                        {
                            if (this.m_winCombo[index].GetItemByIndex(num2).Key != trigger.GetType().GetHashCode())
                            {
                                this.m_winButton[index].Enabled = false;
                                num2++;
                                continue;
                            }
                            this.m_winCombo[index].ItemSelected -= new MyGuiControlCombobox.ItemSelectedDelegate(this.OnWinComboSelect);
                            this.m_winCombo[index].SelectItemByIndex(num2);
                            this.m_winCombo[index].ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnWinComboSelect);
                            this.m_winTrigger[index] = (MyTrigger) trigger.Clone();
                            this.m_winButton[index].Enabled = true;
                        }
                        index++;
                        break;
                    }
                }
                index = 0;
                foreach (MyTrigger trigger2 in triggers.LoseTriggers)
                {
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 < this.m_loseCombo[index].GetItemsCount())
                        {
                            if (this.m_loseCombo[index].GetItemByIndex(num3).Key != trigger2.GetType().GetHashCode())
                            {
                                this.m_loseButton[index].Enabled = false;
                                num3++;
                                continue;
                            }
                            this.m_loseCombo[index].ItemSelected -= new MyGuiControlCombobox.ItemSelectedDelegate(this.OnLoseComboSelect);
                            this.m_loseCombo[index].SelectItemByIndex(num3);
                            this.m_loseCombo[index].ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnLoseComboSelect);
                            this.m_loseTrigger[index] = (MyTrigger) trigger2.Clone();
                            this.m_loseButton[index].Enabled = true;
                        }
                        index++;
                        break;
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenMissionTriggers.<>c <>9 = new MyGuiScreenMissionTriggers.<>c();
            public static Func<System.Type, bool> <>9__14_0;

            internal bool <GetTriggerTypes>b__14_0(System.Type type) => 
                (type.IsSubclassOf(typeof(MyTrigger)) && (MyFakes.ENABLE_NEW_TRIGGERS || ((type != typeof(MyTriggerTimeLimit)) && (type != typeof(MyTriggerBlockDestroyed)))));
        }
    }
}

