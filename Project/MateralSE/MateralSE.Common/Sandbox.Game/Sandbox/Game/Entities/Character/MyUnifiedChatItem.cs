namespace Sandbox.Game.Entities.Character
{
    using Sandbox.Game.Gui;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyUnifiedChatItem
    {
        public string AuthorFont = "Blue";

        public MyUnifiedChatItem()
        {
            this.Text = string.Empty;
            this.Timestamp = DateTime.UtcNow;
            this.Channel = ChatChannel.Global;
            this.CustomAuthor = string.Empty;
            this.AuthorFont = string.Empty;
            this.SenderId = 0L;
            this.TargetId = 0L;
        }

        public static MyUnifiedChatItem CreateChatbotMessage(string text, DateTime timestamp, long senderId, long targetId = 0L, string customAuthor = null, string authorFont = "Blue")
        {
            MyUnifiedChatItem item1 = new MyUnifiedChatItem();
            item1.Text = text;
            item1.Timestamp = timestamp;
            item1.Channel = ChatChannel.ChatBot;
            item1.CustomAuthor = string.IsNullOrEmpty(customAuthor) ? string.Empty : customAuthor;
            MyUnifiedChatItem local1 = item1;
            local1.SenderId = senderId;
            local1.TargetId = targetId;
            local1.AuthorFont = authorFont;
            return local1;
        }

        public static MyUnifiedChatItem CreateFactionMessage(string text, DateTime timestamp, long senderId, long targetId, string authorFont = "Blue")
        {
            MyUnifiedChatItem item1 = new MyUnifiedChatItem();
            item1.Text = text;
            item1.Timestamp = timestamp;
            item1.Channel = ChatChannel.Faction;
            item1.CustomAuthor = string.Empty;
            item1.SenderId = senderId;
            item1.TargetId = targetId;
            item1.AuthorFont = authorFont;
            return item1;
        }

        public static MyUnifiedChatItem CreateGlobalMessage(string text, DateTime timestamp, long senderId, string authorFont = "Blue")
        {
            MyUnifiedChatItem item1 = new MyUnifiedChatItem();
            item1.Text = text;
            item1.Timestamp = timestamp;
            item1.Channel = ChatChannel.Global;
            item1.CustomAuthor = string.Empty;
            item1.SenderId = senderId;
            item1.TargetId = 0L;
            item1.AuthorFont = authorFont;
            return item1;
        }

        public static MyUnifiedChatItem CreatePrivateMessage(string text, DateTime timestamp, long senderId, long targetId, string authorFont = "Blue")
        {
            MyUnifiedChatItem item1 = new MyUnifiedChatItem();
            item1.Text = text;
            item1.Timestamp = timestamp;
            item1.Channel = ChatChannel.Private;
            item1.CustomAuthor = string.Empty;
            item1.SenderId = senderId;
            item1.TargetId = targetId;
            item1.AuthorFont = authorFont;
            return item1;
        }

        public static MyUnifiedChatItem CreateScriptedMessage(string text, DateTime timestamp, string customAuthor, string authorFont = "Blue")
        {
            MyUnifiedChatItem item1 = new MyUnifiedChatItem();
            item1.Text = text;
            item1.Timestamp = timestamp;
            item1.Channel = ChatChannel.GlobalScripted;
            item1.CustomAuthor = string.IsNullOrEmpty(customAuthor) ? string.Empty : customAuthor;
            MyUnifiedChatItem local1 = item1;
            local1.SenderId = 0L;
            local1.TargetId = 0L;
            local1.AuthorFont = authorFont;
            return local1;
        }

        public string Text { get; set; }

        public DateTime Timestamp { get; set; }

        public ChatChannel Channel { get; set; }

        public string CustomAuthor { get; set; }

        public long SenderId { get; set; }

        public long TargetId { get; set; }
    }
}

