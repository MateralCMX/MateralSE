namespace Sandbox.Game.Gui
{
    using System;
    using VRageMath;

    public class MyChatItem
    {
        public string Sender;
        public string Message;
        public Color SenderColor;
        public Color MessageColor;
        public string Font;
        public DateTime Timestamp;

        public MyChatItem(string sender, string message, string font, Color senderColor) : this(sender, message, font, senderColor, Color.White)
        {
        }

        public MyChatItem(string sender, string message, string font, Color senderColor, Color messageColor)
        {
            this.Sender = sender;
            this.Message = message;
            this.SenderColor = senderColor;
            this.MessageColor = messageColor;
            this.Font = font;
            this.Timestamp = DateTime.Now;
        }
    }
}

