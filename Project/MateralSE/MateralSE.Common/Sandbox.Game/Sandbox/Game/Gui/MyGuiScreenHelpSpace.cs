namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenHelpSpace : MyGuiScreenBase
    {
        private static readonly MyHackyQuestLogComparer m_hackyQuestComparer = new MyHackyQuestLogComparer();
        public MyGuiControlList contentList;
        private HelpPageEnum m_currentPage;

        public MyGuiScreenHelpSpace() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.8436f, 0.97f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            this.m_currentPage = HelpPageEnum.Tutorials;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        private void AddChatColors_Name()
        {
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Name_Self), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_NameDesc_Self), new Color?(Color.CornflowerBlue)));
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Name_Ally), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_NameDesc_Ally), new Color?(Color.LightGreen)));
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Name_Neutral), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_NameDesc_Neutral), new Color?(Color.PaleGoldenrod)));
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Name_Enemy), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_NameDesc_Enemy), new Color?(Color.Crimson)));
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Name_Admin), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_NameDesc_Admin), new Color?(Color.Purple)));
        }

        private void AddChatColors_Text()
        {
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Text_Faction), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_TextDesc_Faction), new Color?(Color.LimeGreen)));
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Text_Private), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_TextDesc_Private), new Color?(Color.Violet)));
            this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Text_Global), MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_TextDesc_Global), new Color?(Color.White)));
        }

        private void AddChatCommands()
        {
            if (MySession.Static == null)
            {
                this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.ChatCommands_Menu), 1f));
            }
            else
            {
                Color? color = null;
                this.contentList.Controls.Add(this.AddKeyPanel("/? <question>", MyTexts.GetString(MyCommonTexts.ChatCommand_HelpSimple_Question), color));
                int num = 1;
                foreach (KeyValuePair<string, IMyChatCommand> pair in MySession.Static.ChatSystem.CommandSystem.ChatCommands)
                {
                    color = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyStringId.GetOrCompute(pair.Value.CommandText)), MyTexts.GetString(MyStringId.GetOrCompute(pair.Value.HelpSimpleText)), color));
                    num++;
                    if ((num % 5) == 0)
                    {
                        this.contentList.Controls.Add(this.AddTinySpacePanel());
                    }
                }
            }
        }

        private void AddChatControls()
        {
            Color? color = null;
            this.contentList.Controls.Add(this.AddKeyPanel("PageUp", MyTexts.GetString(MyCommonTexts.ChatCommand_HelpSimple_PageUp), color));
            color = null;
            this.contentList.Controls.Add(this.AddKeyPanel("PageDown", MyTexts.GetString(MyCommonTexts.ChatCommand_HelpSimple_PageDown), color));
        }

        private void AddControlsByType(MyGuiControlTypeEnum type)
        {
            DictionaryValuesReader<MyStringId, MyControl> gameControlsList = MyInput.Static.GetGameControlsList();
            int num = 0;
            foreach (MyControl control in gameControlsList)
            {
                if (control.GetControlTypeEnum() == type)
                {
                    if (((num + 1) % 5) == 0)
                    {
                        this.contentList.Controls.Add(this.AddTinySpacePanel());
                    }
                    Color? color = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(this.GetControlButtonName(control), this.GetControlButtonDescription(control), color));
                }
            }
        }

        private MyGuiControlTable.Row AddHelpScreenCategory(MyGuiControlTable table, string rowName, HelpPageEnum pageEnum)
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(pageEnum);
            StringBuilder text = new StringBuilder(rowName);
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(text, null, text.ToString(), new Color?(Color.White), icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            table.Add(row);
            return row;
        }

        private MyGuiControlParent AddImageLinkPanel(string imagePath, string text, string url)
        {
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.Size = new Vector2(0.137f, 0.108f);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            image1.Position = new Vector2(-0.22f, 0.003f);
            image1.BorderEnabled = true;
            image1.BorderSize = 1;
            image1.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            MyGuiControlImage control = image1;
            control.SetTexture(@"Textures\GUI\Screens\image_background.dds");
            MyGuiControlImage image4 = new MyGuiControlImage();
            image4.Size = new Vector2(0.137f, 0.108f);
            image4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            image4.Position = new Vector2(-0.22f, 0.003f);
            image4.BorderEnabled = true;
            image4.BorderSize = 1;
            image4.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            MyGuiControlImage image2 = image4;
            image2.SetTexture(imagePath);
            image2.SetTooltip(url);
            MyGuiControlMultilineText text2 = new MyGuiControlMultilineText {
                Size = new Vector2(0.3f, 0.1f),
                Text = new StringBuilder(text),
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Position = new Vector2(0.08f, -0.005f)
            };
            MyGuiControlButton button = this.MakeButton(new Vector2(0.08f, 0f), MySpaceTexts.Blank, delegate (MyGuiControlButton x) {
                MyGuiSandbox.OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, null);
            });
            button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            button.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            button.Text = MyTexts.GetString(MyCommonTexts.HelpScreen_HomeSteamOverlay);
            button.Alpha = 1f;
            button.VisualStyle = MyGuiControlButtonStyleEnum.ClickableText;
            button.Size = new Vector2(0.22f, 0.13f);
            button.TextScale = 0.736f;
            button.CanHaveFocus = false;
            button.PositionY += 0.05f;
            button.PositionX += 0.113f;
            MyGuiControlImage image5 = new MyGuiControlImage();
            image5.Size = new Vector2(0.0128f, 0.0176f);
            image5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            image5.Position = button.Position + new Vector2(0.01f, -0.01f);
            image5.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            MyGuiControlImage image3 = image5;
            image3.SetTexture(@"Textures\GUI\link.dds");
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.4645f, 0.12f);
            parent1.Controls.Add(control);
            parent1.Controls.Add(image2);
            parent1.Controls.Add(text2);
            parent1.Controls.Add(button);
            parent1.Controls.Add(image3);
            return parent1;
        }

        private void AddIngameHelpContent(MyGuiControlList contentList)
        {
            foreach (MyIngameHelpObjective objective in MySessionComponentIngameHelp.GetFinishedObjectives().Reverse<MyIngameHelpObjective>())
            {
                contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(objective.TitleEnum)));
                contentList.Controls.Add(this.AddTinySpacePanel());
                MyIngameHelpDetail[] details = objective.Details;
                int index = 0;
                while (true)
                {
                    if (index >= details.Length)
                    {
                        contentList.Controls.Add(this.AddTinySpacePanel());
                        contentList.Controls.Add(this.AddSeparatorPanel());
                        contentList.Controls.Add(this.AddTinySpacePanel());
                        break;
                    }
                    MyIngameHelpDetail detail = details[index];
                    contentList.Controls.Add(this.AddTextPanel((detail.Args == null) ? MyTexts.GetString(detail.TextEnum) : string.Format(MyTexts.GetString(detail.TextEnum), detail.Args), 0.9f));
                    index++;
                }
            }
            this.MyDisgustingHackyQuestlogForLearningToSurviveAsWeAreRunningOutOfTime(contentList);
        }

        private MyGuiControlParent AddKeyCategoryPanel(string text)
        {
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? backgroundColor = null;
            MyGuiControlPanel control = new MyGuiControlPanel(position, position, backgroundColor, @"Textures\GUI\Controls\item_highlight_dark.dds", null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                Size = new Vector2(0.44f, 0.035f),
                BorderEnabled = true,
                BorderSize = 1,
                BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f)
            };
            MyGuiControlMultilineText text2 = new MyGuiControlMultilineText {
                Size = new Vector2(0.4645f, 0.5f),
                Text = new StringBuilder(text),
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            text2.PositionX += 0.02f;
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.2f, text2.TextSize.Y + 0.01f);
            parent1.Controls.Add(control);
            parent1.Controls.Add(text2);
            return parent1;
        }

        private MyGuiControlParent AddKeyPanel(string key, string description, Color? color = new Color?())
        {
            MyGuiControlLabel control = new MyGuiControlLabel {
                Text = key,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER,
                Font = (color != null) ? "White" : "Red"
            };
            control.PositionX -= 0.2f;
            if (color != null)
            {
                control.ColorMask = new VRageMath.Vector4(((float) color.Value.X) / 256f, ((float) color.Value.Y) / 256f, ((float) color.Value.Z) / 256f, ((float) color.Value.A) / 256f);
            }
            MyGuiControlLabel label2 = new MyGuiControlLabel {
                Text = description,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            label2.PositionX += 0.2f;
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.2f, 0.013f);
            parent1.Controls.Add(control);
            parent1.Controls.Add(label2);
            return parent1;
        }

        private MyGuiControlParent AddLinkPanel(string text, string url)
        {
            MyGuiControlButton control = this.MakeButton(new Vector2(0.08f, 0f), MySpaceTexts.Blank, delegate (MyGuiControlButton x) {
                MyGuiSandbox.OpenUrl(url, UrlOpenMode.ExternalBrowser, null);
            });
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            control.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            control.Text = text;
            control.Alpha = 1f;
            control.VisualStyle = MyGuiControlButtonStyleEnum.ClickableText;
            control.Size = new Vector2(0.22f, 0.13f);
            control.TextScale = 0.736f;
            control.CanHaveFocus = false;
            control.PositionY += 0.01f;
            control.PositionX += 0.113f;
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.Size = new Vector2(0.0128f, 0.0176f);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            image1.Position = control.Position + new Vector2(0.01f, -0.01f);
            image1.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            MyGuiControlImage image = image1;
            image.SetTexture(@"Textures\GUI\link.dds");
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.4645f, 0.024f);
            parent1.Controls.Add(control);
            parent1.Controls.Add(image);
            return parent1;
        }

        private MyGuiControlParent AddSeparatorPanel()
        {
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(-0.22f, 0f), 0.44f, 0f, color);
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.2f, 0.001f);
            parent1.Controls.Add(control);
            return parent1;
        }

        private MyGuiControlParent AddSignaturePanel()
        {
            VRageMath.Vector4? backgroundColor = null;
            MyGuiControlPanel control = new MyGuiControlPanel(new Vector2(-0.08f, -0.04f), new Vector2?(MyGuiConstants.TEXTURE_KEEN_LOGO.MinSizeGui), backgroundColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                BackgroundTexture = MyGuiConstants.TEXTURE_KEEN_LOGO
            };
            Vector2? size = null;
            backgroundColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(0.19f, -0.01f), size, MyTexts.GetString(MySpaceTexts.WelcomeScreen_SignatureTitle), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM
            };
            size = null;
            backgroundColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(0.19f, 0.015f), size, MyTexts.GetString(MySpaceTexts.WelcomeScreen_Signature), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM
            };
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.2f, 0.1f);
            parent1.Controls.Add(label);
            parent1.Controls.Add(label2);
            parent1.Controls.Add(control);
            return parent1;
        }

        private MyGuiControlParent AddTextPanel(string text, float textScaleMultiplier = 1f)
        {
            MyGuiControlMultilineText control = new MyGuiControlMultilineText {
                Size = new Vector2(0.4645f, 0.5f)
            };
            control.TextScale *= textScaleMultiplier;
            control.Text = new StringBuilder(text);
            control.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            control.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            control.PositionX += 0.013f;
            control.Parse();
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.4645f, control.TextSize.Y + 0.01f);
            parent1.Controls.Add(control);
            return parent1;
        }

        private MyGuiControlParent AddTinySpacePanel()
        {
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(0.2f, 0.005f);
            return parent1;
        }

        private void backButton_ButtonClicked(MyGuiControlButton obj)
        {
            this.CloseScreen();
        }

        public string GetControlButtonDescription(MyControl control) => 
            MyTexts.GetString(control.GetControlName());

        public string GetControlButtonDescription(MyStringId control) => 
            MyTexts.GetString(MyInput.Static.GetGameControl(control).GetControlName());

        public string GetControlButtonName(MyControl control)
        {
            StringBuilder output = new StringBuilder();
            control.AppendBoundButtonNames(ref output, ", ", MyInput.Static.GetUnassignedName(), true);
            return output.ToString();
        }

        public string GetControlButtonName(MyStringId control)
        {
            StringBuilder output = new StringBuilder();
            MyInput.Static.GetGameControl(control).AppendBoundButtonNames(ref output, ", ", MyInput.Static.GetUnassignedName(), true);
            return output.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenHelp";

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, Action<MyGuiControlButton> onClick)
        {
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            VRageMath.Vector4 vector2 = MyGuiConstants.BACK_BUTTON_BACKGROUND_COLOR;
            VRageMath.Vector4 vector1 = MyGuiConstants.BACK_BUTTON_TEXT_COLOR;
            float textScale = 0.8f;
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), new VRageMath.Vector4?(vector2), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(text), textScale, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private void MyDisgustingHackyQuestlogForLearningToSurviveAsWeAreRunningOutOfTime(MyGuiControlList contentList)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                Regex nameRegex = new Regex("O_..x.._IsFinished");
                Regex regex2 = new Regex("O_..x.._IsFailed");
                string str = "Caption";
                List<KeyValuePair<string, bool>> list = MySessionComponentScriptSharedStorage.Instance.GetBoolsByRegex(regex2).ToList<KeyValuePair<string, bool>>();
                List<KeyValuePair<string, bool>> list1 = MySessionComponentScriptSharedStorage.Instance.GetBoolsByRegex(nameRegex).ToList<KeyValuePair<string, bool>>();
                list1.Sort(m_hackyQuestComparer);
                list.Sort(m_hackyQuestComparer);
                int num = -1;
                foreach (KeyValuePair<string, bool> pair in list1)
                {
                    num++;
                    if (pair.Value)
                    {
                        string str2 = pair.Key.Substring(0, 8);
                        contentList.Controls.Add(this.AddKeyCategoryPanel(MyStatControlText.SubstituteTexts("{LOCC:" + MyTexts.GetString(str2 + str) + "}", null)));
                        contentList.Controls.Add(this.AddTinySpacePanel());
                        contentList.Controls.Add(this.AddTextPanel(MyStatControlText.SubstituteTexts("{LOCC:" + (list[num].Value ? MyTexts.GetString("QuestlogDetail_Failed") : MyTexts.GetString("QuestlogDetail_Success")) + "}", null), 0.9f));
                        contentList.Controls.Add(this.AddTinySpacePanel());
                        contentList.Controls.Add(this.AddSeparatorPanel());
                        contentList.Controls.Add(this.AddTinySpacePanel());
                    }
                }
            }
        }

        private void OnCloseClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs args)
        {
            if (sender.SelectedRow != null)
            {
                this.m_currentPage = (HelpPageEnum) sender.SelectedRow.UserData;
                this.RecreateControls(false);
            }
        }

        public override void RecreateControls(bool constructor)
        {
            Color? nullable5;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyTexts.GetString(MyCommonTexts.HelpScreenHeader), captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.87f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.87f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.87f) / 2f, (base.m_size.Value.Y / 2f) - 0.847f), base.m_size.Value.X * 0.87f, 0f, captionTextColor);
            this.Controls.Add(control);
            StringBuilder output = new StringBuilder();
            MyInput.Static.GetGameControl(MyControlsSpace.HELP_SCREEN).AppendBoundButtonNames(ref output, ",", MyInput.Static.GetUnassignedName(), false);
            StringBuilder builder1 = new StringBuilder();
            builder1.AppendFormat(MyTexts.GetString(MyCommonTexts.HelpScreen_Description), output);
            StringBuilder contents = builder1;
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(-0.365f, 0.381f), new Vector2(0.4f, 0.2f), captionTextColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, contents, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            this.Controls.Add(text);
            Vector2? size = null;
            captionTextColor = null;
            contents = MyTexts.Get(MyCommonTexts.ScreenMenuButtonBack);
            visibleLinesCount = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(0.281f, 0.415f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Close), contents, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            button.ButtonClicked += new Action<MyGuiControlButton>(this.backButton_ButtonClicked);
            this.Controls.Add(button);
            captionTextColor = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.365f, -0.39f), new Vector2(0.211f, 0.035f), captionTextColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = panel.Position + new Vector2(0.01f, 0.005f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MyCommonTexts.HelpScreen_HomeSelectCategory);
            MyGuiControlLabel label = label1;
            this.Controls.Add(panel);
            this.Controls.Add(label);
            MyGuiControlTable table1 = new MyGuiControlTable();
            table1.Position = panel.Position + new Vector2(0f, 0.033f);
            table1.Size = new Vector2(0.211f, 0.5f);
            table1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            table1.ColumnsCount = 1;
            table1.VisibleRowsCount = 20;
            table1.HeaderVisible = false;
            MyGuiControlTable table = table1;
            float[] p = new float[] { 1f };
            table.SetCustomColumnWidths(p);
            table.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.Controls.Add(table);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_Tutorials), HelpPageEnum.Tutorials);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_BasicControls), HelpPageEnum.BasicControls);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedControls), HelpPageEnum.AdvancedControls);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_Chat), HelpPageEnum.Chat);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_Support), HelpPageEnum.Support);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_IngameHelp), HelpPageEnum.IngameHelp);
            this.AddHelpScreenCategory(table, MyTexts.GetString(MyCommonTexts.HelpScreen_Welcome), HelpPageEnum.Welcome);
            table.SelectedRow = table.GetRow((int) this.m_currentPage);
            captionTextColor = null;
            this.contentList = new MyGuiControlList(new Vector2?(panel.Position + new Vector2(0.22f, 0f)), new Vector2(0.511f, 0.74f), captionTextColor, null, MyGuiControlListStyleEnum.Default);
            this.contentList.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.contentList.VisualStyle = MyGuiControlListStyleEnum.Dark;
            this.Controls.Add(this.contentList);
            switch (this.m_currentPage)
            {
                case HelpPageEnum.Tutorials:
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\Intro.dds", "Intro", MySteamConstants.URL_TUTORIAL_PART1));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\BasicControls.dds", "Basic Controls", MySteamConstants.URL_TUTORIAL_PART2));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\GameModePossibilities.dds", "Possibilities Within The Game Modes", MySteamConstants.URL_TUTORIAL_PART3));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\DrillingRefiningAssembling.dds", "Drilling, Refining, & Assembling (Survival)", MySteamConstants.URL_TUTORIAL_PART4));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\Building1stShip.dds", "Building Your 1st Ship (Creative)", MySteamConstants.URL_TUTORIAL_PART5));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\Survival.dds", "Survival", MySteamConstants.URL_TUTORIAL_PART10));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\ExperimentalMode.dds", "Experimental Mode", MySteamConstants.URL_TUTORIAL_PART6));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\Building1stVehicle.dds", "Building Your 1st Ground Vehicle (Creative)", MySteamConstants.URL_TUTORIAL_PART7));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\SteamWorkshopBlueprints.dds", "Steam Workshop & Blueprints", MySteamConstants.URL_TUTORIAL_PART8));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\OtherAdvice.dds", "Other Advice & Closing Thoughts", MySteamConstants.URL_TUTORIAL_PART9));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\SteamLink.dds", MyTexts.GetString(MyCommonTexts.HelpScreen_TutorialsLinkSteam), "http://steamcommunity.com/app/244850/guides"));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\WikiLink.dds", MyTexts.GetString(MyCommonTexts.HelpScreen_TutorialsLinkWiki), "http://spaceengineerswiki.com/Main_Page"));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    return;

                case HelpPageEnum.BasicControls:
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_BasicDescription), 1f));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeNavigation) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddControlsByType(MyGuiControlTypeEnum.Navigation);
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeSystems1) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddControlsByType(MyGuiControlTypeEnum.Systems1);
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + " + this.GetControlButtonName(MyControlsSpace.DAMPING), MyTexts.GetString(MySpaceTexts.ControlName_RelativeDampening), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeSystems2) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddControlsByType(MyGuiControlTypeEnum.Systems2);
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeSystems3) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddControlsByType(MyGuiControlTypeEnum.Systems3);
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeToolsOrWeapons) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddControlsByType(MyGuiControlTypeEnum.ToolsOrWeapons);
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeView) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddControlsByType(MyGuiControlTypeEnum.Spectator);
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    return;

                case HelpPageEnum.AdvancedControls:
                {
                    StringBuilder builder3 = null;
                    MyInput.Static.GetGameControl(MyControlsSpace.CUBE_COLOR_CHANGE).AppendBoundButtonNames(ref builder3, ", ", MyInput.Static.GetUnassignedName(), true);
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedDescription), 1f));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedGeneral)));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("F10", MyTexts.Get(MySpaceTexts.OpenBlueprints).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("SHIFT + F10", MyTexts.Get(MySpaceTexts.OpenSpawnScreen).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("ALT + F10", MyTexts.Get(MySpaceTexts.OpenAdminScreen).ToString(), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("F5", MyTexts.GetString(MyCommonTexts.ControlDescQuickLoad), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("SHIFT + F5", MyTexts.GetString(MyCommonTexts.ControlDescQuickSave), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + H", MyTexts.GetString(MySpaceTexts.ControlDescNetgraph), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("F3", MyTexts.GetString(MyCommonTexts.ControlDescPlayersList), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedGridsAndBlueprints)));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + B", MyTexts.Get(MySpaceTexts.CreateManageBlueprints).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(MyTexts.GetString(MyCommonTexts.MouseWheel), MyTexts.GetString(MyCommonTexts.ControlName_ChangeBlockVariants), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("Ctrl + " + MyTexts.GetString(MyCommonTexts.MouseWheel), MyTexts.GetString(MyCommonTexts.ControlDescCopyPasteMove), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + C", MyTexts.Get(MySpaceTexts.CopyObject).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + SHIFT + C", MyTexts.Get(MySpaceTexts.CopyObjectDetached).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + V", MyTexts.Get(MySpaceTexts.PasteObject).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + X", MyTexts.Get(MySpaceTexts.CutObject).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + Del", MyTexts.Get(MySpaceTexts.DeleteObject).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + ALT + E", MyTexts.GetString(MyCommonTexts.ControlDescExportModel), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedCamera)));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("Alt + " + MyTexts.Get(MyCommonTexts.MouseWheel).ToString(), MyTexts.Get(MySpaceTexts.ControlDescZoom).ToString(), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(this.GetControlButtonName(MyControlsSpace.SWITCH_LEFT), this.GetControlButtonDescription(MyControlsSpace.SWITCH_LEFT), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(this.GetControlButtonName(MyControlsSpace.SWITCH_RIGHT), this.GetControlButtonDescription(MyControlsSpace.SWITCH_RIGHT), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedColorPicker)));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(this.GetControlButtonName(MyControlsSpace.LANDING_GEAR), this.GetControlButtonDescription(MyControlsSpace.LANDING_GEAR), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("SHIFT + P", MyTexts.GetString(MySpaceTexts.PickColorFromCube), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel(builder3.ToString(), MyTexts.GetString(MySpaceTexts.ControlDescHoldToColor), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + " + builder3.ToString(), MyTexts.GetString(MySpaceTexts.ControlDescMediumBrush), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("ALT + " + builder3.ToString(), MyTexts.GetString(MySpaceTexts.ControlDescLargeBrush), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedVoxelHands)));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("H", MyTexts.GetString(MyCommonTexts.ControlDescOpenVoxelHandSettings), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("[", MyTexts.GetString(MyCommonTexts.ControlDescNextVoxelMaterial), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("]", MyTexts.GetString(MyCommonTexts.ControlDescPreviousVoxelMaterial), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_AdvancedSpectator)));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("CTRL + SPACE", MyTexts.GetString(MyCommonTexts.ControlDescMoveToSpectator), nullable5));
                    nullable5 = null;
                    this.contentList.Controls.Add(this.AddKeyPanel("SHIFT + " + MyTexts.GetString(MyCommonTexts.MouseWheel), MyTexts.GetString(MySpaceTexts.ControlDescSpectatorSpeed), nullable5));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    return;
                }
                case HelpPageEnum.Chat:
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_ChatDescription), 1f));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Header_Name) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddChatColors_Name();
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Colors_Header_Text) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddChatColors_Text();
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Controls) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddChatControls();
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddKeyCategoryPanel(MyTexts.GetString(MyCommonTexts.ControlTypeChat_Commands) + ":"));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.AddChatCommands();
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    return;

                case HelpPageEnum.Support:
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_SupportDescription), 1f));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\KSWLink.dds", MyTexts.GetString(MyCommonTexts.HelpScreen_SupportLinkUserResponse), "https://support.keenswh.com/"));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddImageLinkPanel(@"Textures\GUI\HelpScreen\KSWLink.dds", MyTexts.GetString(MyCommonTexts.HelpScreen_SupportLinkForum), "http://forums.keenswh.com/"));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_SupportContactDescription), 1f));
                    this.contentList.Controls.Add(this.AddLinkPanel(MyTexts.GetString(MyCommonTexts.HelpScreen_SupportContact), "mailto:support@keenswh.com"));
                    return;

                case HelpPageEnum.IngameHelp:
                    this.AddIngameHelpContent(this.contentList);
                    return;

                case HelpPageEnum.Welcome:
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MyCommonTexts.ScreenCaptionWelcomeScreen), 1f));
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MySpaceTexts.WelcomeScreen_Text1), 1f));
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MySpaceTexts.WelcomeScreen_Text2), 1f));
                    this.contentList.Controls.Add(this.AddTextPanel(MyTexts.GetString(MySpaceTexts.WelcomeScreen_Text3), 1f));
                    this.contentList.Controls.Add(this.AddTinySpacePanel());
                    this.contentList.Controls.Add(this.AddSeparatorPanel());
                    this.contentList.Controls.Add(this.AddSignaturePanel());
                    return;
            }
            this.contentList.Controls.Add(this.AddTextPanel("Incorrect page selected", 1f));
        }

        private enum HelpPageEnum
        {
            Tutorials,
            BasicControls,
            AdvancedControls,
            Chat,
            Support,
            IngameHelp,
            Welcome
        }

        protected class MyHackyQuestLogComparer : IComparer<KeyValuePair<string, bool>>
        {
            int IComparer<KeyValuePair<string, bool>>.Compare(KeyValuePair<string, bool> x, KeyValuePair<string, bool> y) => 
                string.Compare(x.Key, y.Key);
        }
    }
}

