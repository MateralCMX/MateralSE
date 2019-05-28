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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VRage;
    using VRage.Game.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public static class MyChatCommands
    {
        private static StringBuilder m_tempBuilder = new StringBuilder();

        [ChatCommand("/f", "ChatCommand_Help_F", "ChatCommand_HelpSimple_F", MyPromoteLevel.None)]
        private static void CommandChannelFaction(string[] args)
        {
            long playerId = MySession.Static.Players.TryGetIdentityId(Sync.MyId, 0);
            if (playerId != 0)
            {
                if (MySession.Static.Factions.GetPlayerFaction(playerId) == null)
                {
                    MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_FactionChatTarget), "Blue");
                }
                else
                {
                    MySession.Static.ChatSystem.ChangeChatChannel_Faction();
                    if ((args != null) && (args.Length != 0))
                    {
                        string str = args[0];
                        int index = 1;
                        while (true)
                        {
                            if (index >= args.Length)
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    MyGuiScreenChat.SendChatMessage(str);
                                }
                                break;
                            }
                            str = str + " " + args[index];
                            index++;
                        }
                    }
                }
            }
        }

        [ChatCommand("/g", "ChatCommand_Help_G", "ChatCommand_HelpSimple_G", MyPromoteLevel.None)]
        private static void CommandChannelGlobal(string[] args)
        {
            MySession.Static.ChatSystem.ChangeChatChannel_Global();
            if ((args != null) && (args.Length != 0))
            {
                string str = args[0];
                int index = 1;
                while (true)
                {
                    if (index >= args.Length)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            MyGuiScreenChat.SendChatMessage(str);
                        }
                        break;
                    }
                    str = str + " " + args[index];
                    index++;
                }
            }
        }

        [ChatCommand("/w", "ChatCommand_Help_W", "ChatCommand_HelpSimple_W", MyPromoteLevel.None)]
        private static void CommandChannelWhisper(string[] args)
        {
            string name = string.Empty;
            string str2 = string.Empty;
            if ((args == null) || (args.Count<string>() < 1))
            {
                MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_WhisperChatTarget), "Blue");
            }
            else
            {
                if ((args[0].Length <= 0) || (args[0][0] != '"'))
                {
                    name = args[0];
                    if (args.Length > 1)
                    {
                        string str7 = args[1];
                        int index = 2;
                        while (true)
                        {
                            if (index >= args.Length)
                            {
                                if (string.IsNullOrEmpty(str7))
                                {
                                    return;
                                }
                                str2 = str7;
                                break;
                            }
                            str7 = str7 + " " + args[index];
                            index++;
                        }
                    }
                }
                else
                {
                    int index = 0;
                    bool flag = false;
                    while (true)
                    {
                        if (index < args.Length)
                        {
                            if (args[index][args[index].Length - 1] != '"')
                            {
                                index++;
                                continue;
                            }
                            flag = true;
                        }
                        if (!flag)
                        {
                            name = args[0];
                            if (args.Length > 1)
                            {
                                string str6 = args[1];
                                int num4 = 2;
                                while (true)
                                {
                                    if (num4 >= args.Length)
                                    {
                                        str2 = str6;
                                        break;
                                    }
                                    str6 = str6 + " " + args[num4];
                                    num4++;
                                }
                            }
                        }
                        else if (index == 0)
                        {
                            name = (args[0].Length > 2) ? args[0].Substring(1, args[0].Length - 2) : string.Empty;
                            if (index < (args.Length - 1))
                            {
                                string str3 = args[1];
                                int num2 = 2;
                                while (true)
                                {
                                    if (num2 >= args.Length)
                                    {
                                        str2 = str3;
                                        break;
                                    }
                                    str3 = str3 + " " + args[num2];
                                    num2++;
                                }
                            }
                        }
                        else
                        {
                            string str4 = args[0];
                            int num3 = 1;
                            while (true)
                            {
                                if (num3 > index)
                                {
                                    name = (str4.Length > 2) ? str4.Substring(1, str4.Length - 2) : string.Empty;
                                    if (index < (args.Length - 1))
                                    {
                                        string str5 = args[index + 1];
                                        num3 = index + 2;
                                        while (true)
                                        {
                                            if (num3 >= args.Length)
                                            {
                                                str2 = str5;
                                                break;
                                            }
                                            str5 = str5 + " " + args[num3];
                                            num3++;
                                        }
                                    }
                                    break;
                                }
                                str4 = str4 + " " + args[num3];
                                num3++;
                            }
                        }
                        break;
                    }
                }
                MyPlayer playerByName = MySession.Static.Players.GetPlayerByName(name);
                if (playerByName == null)
                {
                    MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), string.Format(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_WhisperTargetNotFound), name), "Blue");
                }
                else
                {
                    MySession.Static.ChatSystem.ChangeChatChannel_Whisper(playerByName.Identity.IdentityId);
                    if (!string.IsNullOrEmpty(str2))
                    {
                        MyGuiScreenChat.SendChatMessage(str2);
                    }
                }
            }
        }

        [ChatCommand("/gps", "ChatCommand_Help_GPS", "ChatCommand_HelpSimple_GPS", MyPromoteLevel.None)]
        private static void CommandGPS(string[] args)
        {
            MyGps gps = new MyGps();
            MySession.Static.Gpss.GetNameForNewCurrent(m_tempBuilder);
            gps.Name = m_tempBuilder.ToString();
            gps.Description = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewFromCurrent_Desc).ToString();
            gps.Coords = MySession.Static.LocalHumanPlayer.GetPosition();
            gps.ShowOnHud = true;
            gps.DiscardAt = null;
            if ((args == null) || (args.Length == 0))
            {
                MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
            }
            else if (args[0] == "share")
            {
                MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
                if (MyMultiplayer.Static != null)
                {
                    MyMultiplayer.Static.SendChatMessage(gps.ToString(), ChatChannel.Global, 0L);
                }
                else
                {
                    MyHud.Chat.ShowMessageScripted(MyTexts.GetString(MySpaceTexts.ChatBotName), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_GPSRequireOnline));
                }
            }
            else if (args[0] == "faction")
            {
                MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
                MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(MySession.Static.LocalPlayerId);
                if (playerFaction == null)
                {
                    MyHud.Chat.ShowMessage(MyTexts.GetString(MySpaceTexts.ChatBotName), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_GPSRequireFaction), "Blue");
                }
                else if (MyMultiplayer.Static != null)
                {
                    MyMultiplayer.Static.SendChatMessage(gps.ToString(), ChatChannel.Faction, playerFaction.FactionId);
                }
                else
                {
                    MyHud.Chat.ShowMessageScripted(MyTexts.GetString(MySpaceTexts.ChatBotName), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_GPSRequireOnline));
                }
            }
            else
            {
                string gpsName = args[0];
                for (int i = 1; i < args.Length; i++)
                {
                    gpsName = gpsName + " " + args[i];
                }
                if (MySession.Static.Gpss.GetGpsByName(MySession.Static.LocalPlayerId, gpsName) != null)
                {
                    int num2 = 1;
                    while (true)
                    {
                        num2++;
                        if (MySession.Static.Gpss.GetGpsByName(MySession.Static.LocalPlayerId, gpsName + "_" + num2.ToString()) == null)
                        {
                            gpsName = gpsName + "_" + num2.ToString();
                            break;
                        }
                    }
                }
                gps.Name = gpsName;
                MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
            }
        }

        [ChatCommand("/help", "ChatCommand_Help_Help", "ChatCommand_HelpSimple_Help", MyPromoteLevel.None)]
        private static void CommandHelp(string[] args)
        {
            MyPromoteLevel userPromoteLevel = MySession.Static.GetUserPromoteLevel(Sync.MyId);
            if ((args != null) && (args.Length != 0))
            {
                IMyChatCommand command;
                if (!MySession.Static.ChatSystem.CommandSystem.ChatCommands.TryGetValue(args[0], out command))
                {
                    if (args[0] == "?")
                    {
                        MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyStringId.GetOrCompute("ChatCommand_Help_Question")), Color.Red);
                    }
                    else
                    {
                        MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_NotFound) + args[0], Color.Red);
                    }
                }
                else if (userPromoteLevel < command.VisibleTo)
                {
                    MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_NoPermission), Color.Red);
                }
                else
                {
                    MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), command.CommandText + ": " + MyTexts.GetString(MyStringId.GetOrCompute(command.HelpText)), Color.Red);
                }
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(MyTexts.GetString(MyCommonTexts.ChatCommand_AvailableControls));
                builder.Append("PageUp/Down - " + MyTexts.GetString(MyCommonTexts.ChatCommand_PageUpdown));
                StringBuilder builder2 = new StringBuilder();
                builder2.Append(MyTexts.GetString(MyCommonTexts.ChatCommand_AvailableCommands));
                builder2.Append("?, ");
                foreach (KeyValuePair<string, IMyChatCommand> pair in MySession.Static.ChatSystem.CommandSystem.ChatCommands)
                {
                    if (userPromoteLevel >= pair.Value.VisibleTo)
                    {
                        builder2.Append(pair.Key);
                        builder2.Append(", ");
                    }
                }
                builder2.Remove(builder2.Length - 2, 2);
                MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), builder.ToString(), Color.Red);
                MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), builder2.ToString(), Color.Red);
                MyHud.Chat.ShowMessage(MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Author), MyTexts.GetString(MyCommonTexts.ChatCommand_Texts_Help), Color.Red);
            }
        }

        [ChatCommand("/timestamp", "ChatCommand_Help_Timestamp", "ChatCommand_HelpSimple_Timestamp", MyPromoteLevel.None)]
        private static void CommandTiemstampToggle(string[] args)
        {
            if ((args != null) && (args.Length != 0))
            {
                if (args[0].Equals("on"))
                {
                    goto TR_0000;
                }
                else if (!args[0].Equals("true"))
                {
                    if (args[0].Equals("off") || args[0].Equals("false"))
                    {
                        MySandboxGame.Config.ShowChatTimestamp = false;
                        MySandboxGame.Config.Save();
                    }
                }
                else
                {
                    goto TR_0000;
                }
            }
            return;
        TR_0000:
            MySandboxGame.Config.ShowChatTimestamp = true;
            MySandboxGame.Config.Save();
        }
    }
}

