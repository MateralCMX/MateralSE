namespace Sandbox.Game.GameSystems.Chat
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.ModAPI;
    using VRage.Network;

    [StaticEventOwner]
    public class CommandStop : IMyChatCommand
    {
        public void Handle(string[] args)
        {
            string str = string.Empty;
            if ((args != null) && (args.Length != 0))
            {
                str = args[0];
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ulong, string>(x => new Action<ulong, string>(CommandStop.Stop), Sync.MyId, str, targetEndpoint, position);
        }

        public static void SaveFinish(ulong requesting)
        {
            long targetId = MySession.Static.Players.TryGetIdentityId(requesting, 0);
            if (targetId != 0)
            {
                MyMultiplayer.Static.SendChatMessageScripted(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_StopExecuting), ChatChannel.GlobalScripted, targetId, MyTexts.GetString(MySpaceTexts.ChatBotName));
            }
        }

        [Event(null, 0x1c8), Reliable, Server]
        public static void Stop(ulong requester, string name)
        {
            if (!Sync.IsDedicated)
            {
                MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_StopRequiresDS), "Blue");
            }
            else if (MySession.Static.GetUserPromoteLevel(MyEventContext.Current.Sender.Value) < MyPromoteLevel.Admin)
            {
                MyEventContext.ValidationFailed();
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("Executing /stop command");
                MySandboxGame.ExitThreadSafe();
            }
        }

        public string CommandText =>
            "/stop";

        public string HelpText =>
            "ChatCommand_Help_Stop";

        public string HelpSimpleText =>
            "ChatCommand_HelpSimple_Stop";

        public MyPromoteLevel VisibleTo =>
            MyPromoteLevel.Admin;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CommandStop.<>c <>9 = new CommandStop.<>c();
            public static Func<IMyEventOwner, Action<ulong, string>> <>9__9_0;

            internal Action<ulong, string> <Handle>b__9_0(IMyEventOwner x) => 
                new Action<ulong, string>(CommandStop.Stop);
        }
    }
}

