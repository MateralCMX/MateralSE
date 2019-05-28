namespace SpaceEngineers.Game.VoiceChat
{
    using Sandbox;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.VoiceChat;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Data.Audio;
    using VRage.Library.Utils;

    public class MyVoiceChatLogic : IMyVoiceChatLogic
    {
        private const float VOICE_DISTANCE = 40f;
        private const float VOICE_DISTANCE_SQ = 1600f;

        public bool ShouldPlayVoice(MyPlayer player, MyTimeSpan timestamp, out MySoundDimensions dimension, out float maxDistance)
        {
            double num = 500.0;
            if ((MySandboxGame.Static.TotalTime - timestamp).Milliseconds > num)
            {
                dimension = MySoundDimensions.D3;
                maxDistance = float.MaxValue;
                return true;
            }
            dimension = MySoundDimensions.D2;
            maxDistance = 0f;
            return false;
        }

        public bool ShouldSendVoice(MyPlayer player) => 
            MyAntennaSystem.Static.CheckConnection(MySession.Static.LocalHumanPlayer.Identity, player.Identity);
    }
}

