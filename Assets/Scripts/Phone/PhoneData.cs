using System;
using UnityEngine;

namespace RoyalFlush.Phone
{
    [Serializable]
    public class ChatMessageData
    {
        public string senderName;
        [TextArea(2, 5)]
        public string messageText;
        public string timeString = "Now";
        public bool isFromPlayer = false;

        public ChatMessageData(string sender, string text, bool fromPlayer = false, string time = "Now")
        {
            senderName = sender;
            messageText = text;
            isFromPlayer = fromPlayer;
            timeString = time;
        }
    }

    [Serializable]
    public class ChatReplyOption
    {
        [TextArea(1, 3)]
        public string replyLabel;
        [TextArea(2, 5)]
        public string fullMessageText;
        public ChatMessageData incomingResponseAfterReply;
    }

    [Serializable]
    public class ContactConversation
    {
        public string contactId;
        public string contactName;
        public Sprite contactAvatar;
        public System.Collections.Generic.List<ChatMessageData> messages = new System.Collections.Generic.List<ChatMessageData>();
        public System.Collections.Generic.List<ChatReplyOption> pendingReplyOptions = new System.Collections.Generic.List<ChatReplyOption>();
        public bool hasUnread = false;
    }
}
