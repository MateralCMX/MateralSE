namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Audio;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiControlQuestlog : MyGuiControlBase
    {
        private static readonly float ANIMATION_PERIOD = 10f;
        private static readonly int NUMER_OF_PERIODS = 3;
        private static readonly int CHARACTER_TYPING_FREQUENCY = 2;
        private IMySourceVoice m_currentSoundID;
        public MyHudQuestlog QuestInfo;
        private Vector2 m_position;
        private float m_currentFrame;
        private int m_timer;
        private bool m_characterWasAdded;

        public MyGuiControlQuestlog(Vector2 position) : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_currentFrame = float.MaxValue;
            Vector2? nullable = null;
            nullable = null;
            this.m_position = !MyGuiManager.FullscreenHudEnabled ? MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate(position) : MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(position);
            base.Size = MyHud.Questlog.QuestlogSize;
            base.Position = this.m_position + (base.Size / 2f);
            base.BackgroundTexture = new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_QUESTLOG_BACKGROUND_INFO.Texture);
            base.ColorMask = MyGuiConstants.SCREEN_BACKGROUND_COLOR;
            this.QuestInfo = MyHud.Questlog;
            base.VisibleChanged += new VisibleChangedDelegate(this.VisibilityChanged);
            this.QuestInfo.ValueChanged += new Action(this.QuestInfo_ValueChanged);
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if ((this.m_currentFrame < (NUMER_OF_PERIODS * ANIMATION_PERIOD)) && this.QuestInfo.HighlightChanges)
            {
                backgroundTransitionAlpha = MathHelper.Clamp((float) ((((float) Math.Cos((double) ((float) (6.2831853071795862 * (this.m_currentFrame / ANIMATION_PERIOD))))) + 1.5f) * 0.5f), (float) 0f, (float) 1f);
                this.m_currentFrame++;
            }
            else if ((this.m_currentFrame == (NUMER_OF_PERIODS * ANIMATION_PERIOD)) && (this.m_currentSoundID != null))
            {
                this.m_currentSoundID.Stop(false);
                this.m_currentSoundID = null;
            }
            base.Draw(transitionAlpha, backgroundTransitionAlpha * MySandboxGame.Config.HUDBkOpacity);
        }

        private void QuestInfo_ValueChanged()
        {
            base.Position = this.m_position + (base.Size / 2f);
            this.RecreateControls();
            if (this.QuestInfo.HighlightChanges)
            {
                this.m_currentFrame = 0f;
            }
            else
            {
                this.m_currentFrame = float.MaxValue;
            }
        }

        public void RecreateControls()
        {
            if ((this.QuestInfo != null) && (base.Elements != null))
            {
                base.Elements.Clear();
                Vector2 vector = -base.Size / 2f;
                Vector2 vector2 = new Vector2(0.015f, 0.015f);
                MyGuiControlLabel control = new MyGuiControlLabel {
                    Text = this.QuestInfo.QuestTitle,
                    Position = vector + vector2,
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                    Visible = true,
                    Font = "White"
                };
                base.Elements.Add(control);
                MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
                Vector4? color = null;
                list.AddHorizontal((vector + vector2) + new Vector2(0f, 0.03f), base.Size.X - (2f * vector2.X), 0.003f, color);
                list.Visible = true;
                base.Elements.Add(list);
                this.m_characterWasAdded = true;
                Vector2 vector3 = new Vector2(0f, 0.025f);
                float num = 0.65f;
                float scale = 0.7f;
                MultilineData[] questGetails = this.QuestInfo.GetQuestGetails();
                int num3 = 0;
                for (int i = 0; i < questGetails.Length; i++)
                {
                    if ((questGetails[i] != null) && (questGetails[i].Data != null))
                    {
                        Vector2? size = new Vector2(base.Size.X * 0.92f, vector3.Y * 5f);
                        color = null;
                        int? visibleLinesCount = null;
                        MyGuiBorderThickness? textPadding = null;
                        MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2?(((vector + vector2) + new Vector2(0f, 0.04f)) + (vector3 * num3)), size, color, questGetails[i].Completed ? "Green" : (questGetails[i].IsObjective ? "White" : "Blue"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, false, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding) {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            CharactersDisplayed = questGetails[i].CharactersDisplayed,
                            TextScale = num
                        };
                        if ((questGetails.Length == 2) && (i == 1))
                        {
                            text.AppendLine();
                        }
                        if ((questGetails.Length == 3) && (i == 1))
                        {
                            text.AppendLine();
                        }
                        char[] separator = new char[] { '*' };
                        string[] strArray = $"{(questGetails[i].Completed ? "• " : (questGetails[i].IsObjective ? "• " : ""))}{questGetails[i].Data}".Split(separator);
                        string str = "";
                        bool flag = false;
                        bool flag2 = true;
                        int index = 0;
                        while (true)
                        {
                            if (index >= strArray.Length)
                            {
                                char[] chArray2 = new char[] { '[', ']' };
                                bool flag3 = false;
                                string[] strArray2 = str.Split(chArray2);
                                int num6 = 0;
                                while (true)
                                {
                                    if (num6 >= strArray2.Length)
                                    {
                                        text.Visible = true;
                                        num3 += text.NumberOfRows;
                                        base.Elements.Add(text);
                                        break;
                                    }
                                    string str2 = strArray2[num6];
                                    if (!flag3)
                                    {
                                        text.AppendText(str2);
                                    }
                                    else if (!questGetails[i].Completed)
                                    {
                                        text.AppendText(str2, "UrlHighlight", scale, Color.Yellow.ToVector4());
                                    }
                                    else
                                    {
                                        text.AppendText(str2, "UrlHighlight", scale, Color.Green.ToVector4());
                                    }
                                    flag3 = !flag3;
                                    num6++;
                                }
                                break;
                            }
                            if (!flag)
                            {
                                str = strArray[index];
                            }
                            else
                            {
                                if (flag2)
                                {
                                    text.AppendLine();
                                    flag2 = false;
                                }
                                text.AppendText(strArray[index], "UrlHighlight", text.TextScale * 1.2f, Color.White.ToVector4());
                            }
                            flag = !flag;
                            index++;
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            this.m_timer++;
            if ((this.m_timer % CHARACTER_TYPING_FREQUENCY) == 0)
            {
                this.m_timer = 0;
                if (this.m_characterWasAdded)
                {
                    this.UpdateCharacterDisplay();
                }
            }
        }

        private void UpdateCharacterDisplay()
        {
            int index = 0;
            MultilineData[] questGetails = this.QuestInfo.GetQuestGetails();
            for (int i = 0; i < base.Elements.Count; i++)
            {
                MyGuiControlMultilineText text = base.Elements[i] as MyGuiControlMultilineText;
                if (text != null)
                {
                    this.m_characterWasAdded = false;
                    if (index < questGetails.Length)
                    {
                        questGetails[index].CharactersDisplayed = text.CharactersDisplayed;
                        index++;
                    }
                    if (!this.m_characterWasAdded && (text.CharactersDisplayed != -1))
                    {
                        text.CharactersDisplayed++;
                        this.m_characterWasAdded = true;
                        return;
                    }
                }
            }
        }

        private void VisibilityChanged(object sender, bool isVisible)
        {
            if (base.Visible)
            {
                base.Position = this.m_position + (base.Size / 2f);
                this.RecreateControls();
                this.m_currentFrame = 0f;
            }
            else
            {
                this.m_currentFrame = float.MaxValue;
                if (this.m_currentSoundID != null)
                {
                    this.m_currentSoundID.Stop(false);
                    this.m_currentSoundID = null;
                }
            }
        }
    }
}

