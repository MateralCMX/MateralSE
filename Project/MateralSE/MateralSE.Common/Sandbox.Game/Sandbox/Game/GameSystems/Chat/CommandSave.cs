namespace Sandbox.Game.GameSystems.Chat
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.ModAPI;
    using VRage.Network;

    [StaticEventOwner]
    public class CommandSave : IMyChatCommand
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
            MyMultiplayer.RaiseStaticEvent<ulong, string>(x => new Action<ulong, string>(CommandSave.Save), Sync.MyId, str, targetEndpoint, position);
        }

        [Event(null, 0x18a), Reliable, Server]
        public static void Save(ulong requester, string name)
        {
            if (MySession.Static.GetUserPromoteLevel(MyEventContext.Current.Sender.Value) < MyPromoteLevel.Admin)
            {
                MyEventContext.ValidationFailed();
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("Executing /save command");
                MyAsyncSaving.Start(() => SaveFinish(requester), string.IsNullOrEmpty(name) ? null : name, false);
            }
        }

        public static void SaveFinish(ulong requesting)
        {
            long targetId = MySession.Static.Players.TryGetIdentityId(requesting, 0);
            if (targetId > 0L)
            {
                if (MyMultiplayer.Static != null)
                {
                    MyMultiplayer.Static.SendChatMessageScripted(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_ExecutingSaveFinished), ChatChannel.GlobalScripted, targetId, MyTexts.GetString(MySpaceTexts.ChatBotName));
                }
                else
                {
                    MyHud.Chat.ShowMessageScripted(MyTexts.GetString(MySpaceTexts.ChatBotName), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_ExecutingSaveFinished));
                }
            }
        }

        public string CommandText =>
            "/save";

        public string HelpText =>
            "ChatCommand_Help_Save";

        public string HelpSimpleText =>
            "ChatCommand_HelpSimple_Save";

        public MyPromoteLevel VisibleTo =>
            MyPromoteLevel.Admin;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CommandSave.<>c <>9 = new CommandSave.<>c();
            public static Func<IMyEventOwner, Action<ulong, string>> <>9__9_0;

            internal Action<ulong, string> <Handle>b__9_0(IMyEventOwner x) => 
                new Action<ulong, string>(CommandSave.Save);
        }
    }
}

