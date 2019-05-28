namespace Sandbox.Game.SessionComponents
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.SessionComponents;
    using VRage.Game.VisualScripting.Campaign;
    using VRage.GameServices;
    using VRage.Network;
    using VRage.Utils;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x29a, typeof(MyObjectBuilder_CampaignSessionComponent), (System.Type) null)]
    public class MyCampaignSessionComponent : MySessionComponentBase
    {
        private MyCampaignStateMachine m_runningCampaignSM;
        private readonly Dictionary<ulong, MyObjectBuilder_Inventory> m_savedCharacterInventoriesPlayerIds = new Dictionary<ulong, MyObjectBuilder_Inventory>();
        private static ulong m_ownerId;
        private static ulong m_oldLobbyId;
        private static ulong m_elapsedMs;

        private void CallCloseOnClients()
        {
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                if (player.Identity.IdentityId != MySession.Static.LocalPlayerId)
                {
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent(s => new Action(MyCampaignSessionComponent.CloseGame), new EndpointId(player.Id.SteamId), position);
                }
            }
        }

        private void CallReconnectOnClients()
        {
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                if (player.Identity.IdentityId != MySession.Static.LocalPlayerId)
                {
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent(s => new Action(MyCampaignSessionComponent.Reconnect), new EndpointId(player.Id.SteamId), position);
                }
            }
        }

        [Event(null, 0x131), Reliable, Client]
        private static void CloseGame()
        {
            MySessionLoader.UnloadAndExitToMenu();
        }

        private static void FindLobby()
        {
            Thread.Sleep(0x1388);
            MyGameService.RequestLobbyList(new Action<bool>(MyCampaignSessionComponent.LobbiesRequestCompleted));
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_CampaignSessionComponent objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_CampaignSessionComponent;
            if ((objectBuilder != null) && this.Running)
            {
                objectBuilder.ActiveState = this.m_runningCampaignSM.CurrentNode.Name;
                objectBuilder.CampaignName = MyCampaignManager.Static.ActiveCampaign.Name;
                objectBuilder.CurrentOutcome = this.CampaignLevelOutcome;
                objectBuilder.IsVanilla = MyCampaignManager.Static.ActiveCampaign.IsVanilla;
                objectBuilder.LocalModFolder = MyCampaignManager.Static.ActiveCampaign.ModFolderPath;
            }
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyVisualScriptManagerSessionComponent component = ((MySession) base.Session).GetComponent<MyVisualScriptManagerSessionComponent>();
            component.CampaignModPath = null;
            if ((MyMultiplayer.Static == null) || MyMultiplayer.Static.IsServer)
            {
                MyObjectBuilder_CampaignSessionComponent component2 = sessionComponent as MyObjectBuilder_CampaignSessionComponent;
                if ((component2 == null) || string.IsNullOrEmpty(component2.CampaignName))
                {
                    if (MyCampaignManager.Static.IsNewCampaignLevelLoading)
                    {
                        component.CampaignModPath = MyCampaignManager.Static.ActiveCampaign.ModFolderPath;
                    }
                }
                else
                {
                    this.CampaignLevelOutcome = component2.CurrentOutcome;
                    if (!MyCampaignManager.Static.SwitchCampaign(component2.CampaignName, component2.IsVanilla, component2.Mod.PublishedFileId, component2.LocalModFolder))
                    {
                        MyLog.Default.WriteLine("MyCampaignManager - Unable to download or switch to campaign: " + component2.CampaignName);
                        throw new Exception("MyCampaignManager - Unable to download or switch to campaign: " + component2.CampaignName);
                    }
                    this.LoadCampaignStateMachine(component2.ActiveState);
                    component.CampaignModPath = MyCampaignManager.Static.ActiveCampaign.ModFolderPath;
                }
            }
        }

        private void LoadCampaignStateMachine(string activeState = null)
        {
            this.m_runningCampaignSM = new MyCampaignStateMachine();
            this.m_runningCampaignSM.Deserialize(MyCampaignManager.Static.ActiveCampaign.StateMachine);
            if (activeState != null)
            {
                this.m_runningCampaignSM.SetState(activeState);
            }
            else
            {
                this.m_runningCampaignSM.ResetToStart();
            }
            this.m_runningCampaignSM.CurrentNode.OnUpdate(this.m_runningCampaignSM);
        }

        public void LoadNextCampaignMission()
        {
            if ((MyMultiplayer.Static == null) || MyMultiplayer.Static.IsServer)
            {
                this.SavePlayersInventories();
                string directoryName = Path.GetDirectoryName(MySession.Static.CurrentPath.Replace(MyFileSystem.SavesPath + @"\", ""));
                if (!this.m_runningCampaignSM.Finished)
                {
                    this.UpdateStateMachine();
                    string savePath = (this.m_runningCampaignSM.CurrentNode as MyCampaignStateMachineNode).SavePath;
                    this.CallReconnectOnClients();
                    MyCampaignManager.Static.LoadSessionFromActiveCampaign(savePath, delegate {
                        MySession.Static.RegisterComponent(this, MyUpdateOrder.NoUpdate, 0x22b);
                        this.LoadPlayersInventories();
                    }, directoryName, MyCampaignManager.Static.ActiveCampaignName, MyOnlineModeEnum.OFFLINE, 0);
                }
                else
                {
                    this.CallCloseOnClients();
                    MySessionLoader.UnloadAndExitToMenu();
                    MyCampaignManager.Static.NotifyCampaignFinished();
                    if (MyCampaignManager.Static.ActiveCampaign.IsVanilla)
                    {
                        MyScreenManager.AddScreen(new MyGuiScreenGameCredits());
                    }
                }
            }
        }

        private void LoadPlayersInventories()
        {
            MyObjectBuilder_Inventory inventory;
            if (this.m_savedCharacterInventoriesPlayerIds.TryGetValue(MySession.Static.LocalHumanPlayer.Id.SteamId, out inventory) && (MySession.Static.LocalCharacter != null))
            {
                MyInventory inventory2 = MySession.Static.LocalCharacter.GetInventory(0);
                foreach (MyObjectBuilder_InventoryItem item in inventory.Items)
                {
                    inventory2.AddItems(item.Amount, item.PhysicalContent);
                }
            }
            if ((MyMultiplayer.Static != null) && MyMultiplayer.Static.IsServer)
            {
                MySession.Static.Players.PlayersChanged += delegate (bool added, MyPlayer.PlayerId id) {
                    MyObjectBuilder_Inventory inventory;
                    MyPlayer playerById = MySession.Static.Players.GetPlayerById(id);
                    if ((playerById.Character != null) && this.m_savedCharacterInventoriesPlayerIds.TryGetValue(playerById.Id.SteamId, out inventory))
                    {
                        MyInventory inventory2 = MySession.Static.LocalCharacter.GetInventory(0);
                        foreach (MyObjectBuilder_InventoryItem item in inventory.Items)
                        {
                            inventory2.AddItems(item.Amount, item.PhysicalContent);
                        }
                    }
                };
            }
        }

        private static void LobbiesRequestCompleted(bool success)
        {
            if (success)
            {
                List<IMyLobby> lobbies = new List<IMyLobby>();
                MyGameService.AddPublicLobbies(lobbies);
                MyGameService.AddFriendLobbies(lobbies);
                string str = MyFinalBuildConstants.APP_VERSION.FormattedText.ToString();
                using (List<IMyLobby>.Enumerator enumerator = lobbies.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        IMyLobby current = enumerator.Current;
                        if ((current.GetData("appVersion") == str) && ((MyMultiplayerLobby.GetLobbyHostSteamId(current) == m_ownerId) && (current.LobbyId != m_oldLobbyId)))
                        {
                            MyScreenManager.RemoveScreenByType(typeof(MyGuiScreenProgress));
                            MyJoinGameHelper.JoinGame(current, true);
                            return;
                        }
                    }
                }
                m_elapsedMs += (ulong) 0x1388L;
                if (m_elapsedMs > 0x1d4c0L)
                {
                    MyScreenManager.RemoveScreenByType(typeof(MyGuiScreenProgress));
                }
                else
                {
                    FindLobby();
                }
            }
        }

        [Event(null, 0x11f), Reliable, Client]
        private static void Reconnect()
        {
            m_ownerId = MyMultiplayer.Static.ServerId;
            m_elapsedMs = 0UL;
            m_oldLobbyId = (MyMultiplayer.Static as MyMultiplayerLobbyClient).LobbyId;
            MySessionLoader.UnloadAndExitToMenu();
            MyGuiSandbox.AddScreen(new MyGuiScreenProgress(MyTexts.Get(MyCommonTexts.LoadingDialogServerIsLoadingWorld), new MyStringId?(MyCommonTexts.Cancel), true, true));
            Parallel.Start(new Action(MyCampaignSessionComponent.FindLobby));
        }

        private void SavePlayersInventories()
        {
            this.m_savedCharacterInventoriesPlayerIds.Clear();
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                if (player.Character == null)
                {
                    continue;
                }
                MyInventory inventory = player.Character.GetInventory(0);
                if (inventory != null)
                {
                    MyObjectBuilder_Inventory objectBuilder = inventory.GetObjectBuilder();
                    this.m_savedCharacterInventoriesPlayerIds[player.Id.SteamId] = objectBuilder;
                }
            }
        }

        private void UpdateStateMachine()
        {
            this.m_runningCampaignSM.TriggerAction(MyStringId.GetOrCompute(this.CampaignLevelOutcome));
            this.m_runningCampaignSM.Update();
            if (this.m_runningCampaignSM.CurrentNode.Name == (this.m_runningCampaignSM.CurrentNode as MyCampaignStateMachineNode).Name)
            {
                MySandboxGame.Log.WriteLine("ERROR: Campaign is stuck in one state! Check the campaign file.");
            }
            this.CampaignLevelOutcome = null;
        }

        public string CampaignLevelOutcome { get; set; }

        public bool Running =>
            ((this.m_runningCampaignSM != null) && (this.m_runningCampaignSM.CurrentNode != null));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCampaignSessionComponent.<>c <>9 = new MyCampaignSessionComponent.<>c();
            public static Func<IMyEventOwner, Action> <>9__15_0;
            public static Func<IMyEventOwner, Action> <>9__16_0;

            internal Action <CallCloseOnClients>b__16_0(IMyEventOwner s) => 
                new Action(MyCampaignSessionComponent.CloseGame);

            internal Action <CallReconnectOnClients>b__15_0(IMyEventOwner s) => 
                new Action(MyCampaignSessionComponent.Reconnect);
        }
    }
}

