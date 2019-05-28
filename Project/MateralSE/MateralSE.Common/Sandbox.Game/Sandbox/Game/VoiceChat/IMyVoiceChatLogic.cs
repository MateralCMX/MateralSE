namespace Sandbox.Game.VoiceChat
{
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Data.Audio;
    using VRage.Library.Utils;

    public interface IMyVoiceChatLogic
    {
        bool ShouldPlayVoice(MyPlayer player, MyTimeSpan timestamp, out MySoundDimensions dimension, out float maxDistance);
        bool ShouldSendVoice(MyPlayer player);
    }
}

