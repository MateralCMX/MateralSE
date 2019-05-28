namespace Sandbox.Game.GUI.HudViewers
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    public class MyHudControlChat : MyGuiControlMultilineText
    {
        public static readonly float FADE_OUT_TIME = 10000f;
        public static float SCROLL_SPEED = 0.03f;
        private int m_displayedMessageCount;
        private MyHudChat m_chat;
        private int m_lastTimestamp;
        private bool m_forceUpdate;
        private MyChatVisibilityEnum m_visibility;
        private float m_fadeOut;
        private float m_scrollPosition;

        public MyHudControlChat(MyHudChat chat, Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string font = "White", float textScale = 0.5f, MyGuiDrawAlignEnum textAlign = 0, StringBuilder contents = null, MyGuiDrawAlignEnum textBoxAlign = 2, int? visibleLinesCount = new int?(), bool selectable = false) : base(position, size, backgroundColor, font, textScale, textAlign, contents, true, false, textBoxAlign, nullable, selectable, true, null, nullable2)
        {
            this.m_fadeOut = 1f;
            this.m_scrollPosition = 1f;
            this.m_forceUpdate = true;
            this.m_chat = chat;
            this.m_chat.ChatControl = this;
            base.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            base.VisibleChanged += new VisibleChangedDelegate(this.MyHudControlChat_VisibleChanged);
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            this.UpdateText();
            base.Draw(transitionAlpha * this.m_fadeOut, backgroundTransitionAlpha * this.m_fadeOut);
        }

        private void MyHudControlChat_VisibleChanged(object sender, bool isVisible)
        {
            if (!isVisible)
            {
                this.m_forceUpdate = true;
            }
        }

        public void ScrollDown()
        {
            this.m_scrollPosition += SCROLL_SPEED;
            float num = base.m_scrollbarV.MaxSize - base.m_scrollbarV.PageSize;
            if (this.m_scrollPosition >= num)
            {
                this.m_scrollPosition = num;
            }
            base.SetScrollbarValueV = this.m_scrollPosition;
        }

        public void ScrollUp()
        {
            this.m_scrollPosition -= SCROLL_SPEED;
            if (this.m_scrollPosition <= 0f)
            {
                this.m_scrollPosition = 0f;
            }
            base.SetScrollbarValueV = this.m_scrollPosition;
        }

        private void UpdateText()
        {
            if (this.m_chat.TimeSinceLastUpdate <= FADE_OUT_TIME)
            {
                this.m_fadeOut = 1f;
            }
            else
            {
                this.m_fadeOut -= 0.01f;
                if (this.m_fadeOut < 0f)
                {
                    this.m_fadeOut = 0f;
                }
            }
            if (this.m_forceUpdate || (this.m_lastTimestamp != this.m_chat.Timestamp))
            {
                float num = base.m_scrollbarV.Value;
                bool flag = true;
                float num2 = base.m_scrollbarV.MaxSize - base.m_scrollbarV.PageSize;
                if ((num2 > 0f) && (base.m_scrollbarV.Value < num2))
                {
                    flag = false;
                }
                base.Clear();
                bool showChatTimestamp = MySandboxGame.Config.ShowChatTimestamp;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= this.m_chat.MessageHistory.Count)
                    {
                        this.m_displayedMessageCount = this.m_chat.MessageHistory.Count;
                        this.m_forceUpdate = false;
                        this.m_lastTimestamp = this.m_chat.Timestamp;
                        base.RecalculateScrollBar();
                        if (!flag)
                        {
                            base.m_scrollbarV.Value = num;
                            break;
                        }
                        base.m_scrollbarV.Value = base.m_scrollbarV.MaxSize - base.m_scrollbarV.PageSize;
                        return;
                    }
                    MyChatItem item = this.m_chat.MessageHistory[num3];
                    StringBuilder text = new StringBuilder(item.Sender);
                    if (showChatTimestamp)
                    {
                        StringBuilder builder2 = new StringBuilder("[").Append(item.Timestamp.ToLongTimeString()).Append("] ");
                        base.AppendText(builder2, item.Font, base.TextScale, (Vector4) Color.LightGray);
                    }
                    text.Append(": ");
                    base.AppendText(text, item.Font, base.TextScale, (Vector4) item.SenderColor);
                    base.AppendText(new StringBuilder(item.Message), "White", base.TextScale, (Vector4) item.MessageColor);
                    base.AppendLine();
                    num3++;
                }
            }
        }

        public MyChatVisibilityEnum Visibility
        {
            get => 
                this.m_visibility;
            set
            {
                this.m_visibility = value;
                this.m_forceUpdate = true;
                this.UpdateText();
                switch (value)
                {
                    case MyChatVisibilityEnum.Fade:
                    case MyChatVisibilityEnum.AlwaysVisible:
                    case MyChatVisibilityEnum.AlwaysHidden:
                        return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public enum MyChatVisibilityEnum
        {
            Fade,
            AlwaysVisible,
            AlwaysHidden
        }
    }
}

