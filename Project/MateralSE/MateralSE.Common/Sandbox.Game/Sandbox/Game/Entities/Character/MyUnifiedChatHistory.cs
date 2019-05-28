namespace Sandbox.Game.Entities.Character
{
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyUnifiedChatHistory
    {
        protected Queue<MyUnifiedChatItem> m_chat = new Queue<MyUnifiedChatItem>();

        public void ClearNonGlobalHistory()
        {
            Queue<MyUnifiedChatItem> queue = new Queue<MyUnifiedChatItem>();
            foreach (MyUnifiedChatItem item in this.m_chat)
            {
                ChatChannel channel = item.Channel;
                switch (channel)
                {
                    case ChatChannel.Global:
                    case ChatChannel.GlobalScripted:
                    case ChatChannel.ChatBot:
                    {
                        queue.Enqueue(item);
                        continue;
                    }
                }
            }
            this.m_chat = queue;
        }

        public void EnqueueMessage(ref MyUnifiedChatItem item)
        {
            this.m_chat.Enqueue(item);
        }

        public void EnqueueMessage(string text, ChatChannel channel, long senderId, long targetId = 0L, DateTime? timestamp = new DateTime?(), string authorFont = "Blue")
        {
            MyUnifiedChatItem item;
            DateTime utcNow;
            if ((timestamp == null) || (timestamp == null))
            {
                utcNow = DateTime.UtcNow;
            }
            else
            {
                utcNow = timestamp.Value;
            }
            switch (channel)
            {
                case ChatChannel.Global:
                case ChatChannel.GlobalScripted:
                    item = MyUnifiedChatItem.CreateGlobalMessage(text, utcNow, senderId, authorFont);
                    break;

                case ChatChannel.Faction:
                    item = MyUnifiedChatItem.CreateFactionMessage(text, utcNow, senderId, targetId, authorFont);
                    break;

                case ChatChannel.Private:
                    item = MyUnifiedChatItem.CreatePrivateMessage(text, utcNow, senderId, targetId, authorFont);
                    break;

                case ChatChannel.ChatBot:
                    item = MyUnifiedChatItem.CreateChatbotMessage(text, utcNow, senderId, targetId, null, authorFont);
                    break;

                default:
                    item = null;
                    break;
            }
            if (item != null)
            {
                this.EnqueueMessage(ref item);
            }
        }

        public void EnqueueMessageScripted(string text, string customAuthor, string authorFont = "Blue")
        {
            MyUnifiedChatItem item = MyUnifiedChatItem.CreateScriptedMessage(text, DateTime.UtcNow, customAuthor, authorFont);
            this.EnqueueMessage(ref item);
        }

        public void GetChatbotHistory(ref List<MyUnifiedChatItem> list)
        {
            foreach (MyUnifiedChatItem item in this.m_chat)
            {
                if (item.Channel == ChatChannel.ChatBot)
                {
                    list.Add(item);
                }
            }
        }

        public void GetCompleteHistory(ref List<MyUnifiedChatItem> list)
        {
            foreach (MyUnifiedChatItem item in this.m_chat)
            {
                list.Add(item);
            }
        }

        public void GetFactionHistory(ref List<MyUnifiedChatItem> list, long factionId)
        {
            foreach (MyUnifiedChatItem item in this.m_chat)
            {
                if (item.Channel != ChatChannel.Faction)
                {
                    continue;
                }
                if (item.TargetId == factionId)
                {
                    list.Add(item);
                }
            }
        }

        public void GetGeneralHistory(ref List<MyUnifiedChatItem> list)
        {
            foreach (MyUnifiedChatItem item in this.m_chat)
            {
                if ((item.Channel == ChatChannel.Global) || (item.Channel == ChatChannel.GlobalScripted))
                {
                    list.Add(item);
                }
            }
        }

        public void GetPrivateHistory(ref List<MyUnifiedChatItem> list, long playerId)
        {
            foreach (MyUnifiedChatItem item in this.m_chat)
            {
                if (item.Channel != ChatChannel.Private)
                {
                    continue;
                }
                if ((item.TargetId == playerId) || (item.SenderId == playerId))
                {
                    list.Add(item);
                }
            }
        }
    }
}

