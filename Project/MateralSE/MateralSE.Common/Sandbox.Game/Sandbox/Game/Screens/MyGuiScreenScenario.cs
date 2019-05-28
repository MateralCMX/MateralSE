namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenScenario : MyGuiScreenScenarioBase
    {
        private int m_listLoadedParts;
        private List<Tuple<string, MyWorldInfo>> m_availableSavesLocal = new List<Tuple<string, MyWorldInfo>>();
        private List<Tuple<string, MyWorldInfo>> m_availableSavesKeens = new List<Tuple<string, MyWorldInfo>>();
        private List<Tuple<string, MyWorldInfo>> m_availableSavesWorkshop = new List<Tuple<string, MyWorldInfo>>();
        private string m_sessionPath;
        private List<MyWorkshopItem> m_subscribedScenarios;
        protected MyObjectBuilder_SessionSettings m_settings;
        private MyObjectBuilder_Checkpoint m_checkpoint;
        private MyGuiControlLabel m_difficultyLabel;
        private MyGuiControlCombobox m_difficultyCombo;
        private MyGuiControlLabel m_onlineModeLabel;
        private MyGuiControlCombobox m_onlineMode;
        private MyGuiControlLabel m_maxPlayersLabel;
        private MyGuiControlSlider m_maxPlayersSlider;
        private MyGuiControlButton m_removeButton;
        private MyGuiControlButton m_publishButton;
        private MyGuiControlButton m_editButton;
        private MyGuiControlButton m_browseWorkshopButton;
        private MyGuiControlButton m_refreshButton;
        private MyGuiControlButton m_openInWorkshopButton;
        private MyGuiControlList m_scenarioTypesList;
        private MyGuiControlRadioButtonGroup m_scenarioTypesGroup;

        public MyGuiScreenScenario()
        {
            this.RecreateControls(true);
        }

        private void AfterPartLoaded()
        {
            int num = this.m_listLoadedParts + 1;
            this.m_listLoadedParts = num;
            if (num == 3)
            {
                base.ClearSaves();
                base.m_state = MyGuiScreenScenarioBase.StateEnum.ListLoaded;
                base.AddSaves(this.m_availableSavesKeens);
                this.m_availableSavesKeens = null;
                base.AddSaves(this.m_availableSavesWorkshop);
                this.m_availableSavesWorkshop.Clear();
                foreach (Tuple<string, MyWorldInfo> tuple in this.m_availableSavesLocal)
                {
                    if (tuple.Item2.ScenarioEditMode)
                    {
                        base.AddSave(tuple);
                    }
                }
                this.m_availableSavesLocal.Clear();
                base.RefreshGameList();
            }
        }

        private IMyAsyncResult beginKeens() => 
            new MyLoadMissionListResult();

        private IMyAsyncResult beginLocal() => 
            new MyLoadWorldInfoListResult(null);

        private IMyAsyncResult beginWorkshop() => 
            new LoadWorkshopResult();

        protected override void BuildControls()
        {
            base.BuildControls();
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(this.ScreenCaption, captionTextColor, captionOffset, 0.8f);
            Vector2 vector1 = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 local1 = (base.m_size.Value / 2f) - new Vector2(0.65f, 0.1f);
            MyGuiControlLabel control = base.MakeLabel(MySpaceTexts.Difficulty);
            MyGuiControlLabel label2 = base.MakeLabel(MyCommonTexts.WorldSettings_OnlineMode);
            this.m_maxPlayersLabel = base.MakeLabel(MyCommonTexts.MaxPlayers);
            float x = 0.309375f;
            captionOffset = null;
            captionTextColor = null;
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            this.m_difficultyCombo = new MyGuiControlCombobox(captionOffset, new Vector2(x, 0.04f), captionTextColor, captionOffset, 10, captionOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
            this.m_difficultyCombo.Enabled = false;
            int? sortOrder = null;
            MyStringId? toolTip = null;
            this.m_difficultyCombo.AddItem(0L, MySpaceTexts.DifficultyEasy, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_difficultyCombo.AddItem(1L, MySpaceTexts.DifficultyNormal, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_difficultyCombo.AddItem(2L, MySpaceTexts.DifficultyHard, sortOrder, toolTip);
            captionOffset = null;
            captionTextColor = null;
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            this.m_onlineMode = new MyGuiControlCombobox(captionOffset, new Vector2(x, 0.04f), captionTextColor, captionOffset, 10, captionOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
            this.m_onlineMode.Enabled = false;
            this.m_onlineMode.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnOnlineModeSelect);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(0L, MyCommonTexts.WorldSettings_OnlineModeOffline, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(3L, MyCommonTexts.WorldSettings_OnlineModePrivate, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(2L, MyCommonTexts.WorldSettings_OnlineModeFriends, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(1L, MyCommonTexts.WorldSettings_OnlineModePublic, sortOrder, toolTip);
            float width = this.m_onlineMode.Size.X;
            float? defaultValue = null;
            captionTextColor = null;
            this.m_maxPlayersSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 2f, (float) MyMultiplayerLobby.MAX_PLAYERS, width, defaultValue, captionTextColor, new StringBuilder("{0}").ToString(), 0, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            this.m_scenarioTypesList = new MyGuiControlList();
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            sortOrder = null;
            this.m_removeButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.buttonRemove), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            sortOrder = null;
            this.m_publishButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.buttonPublish), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnPublishButtonClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            sortOrder = null;
            this.m_editButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.buttonEdit), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnEditButtonClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            sortOrder = null;
            this.m_browseWorkshopButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.buttonBrowseWorkshop), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnBrowseWorkshopClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            sortOrder = null;
            this.m_refreshButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.buttonRefresh), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnRefreshButtonClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            sortOrder = null;
            this.m_openInWorkshopButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.buttonOpenInWorkshop), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            this.m_removeButton.Enabled = false;
            this.m_publishButton.Enabled = false;
            this.m_editButton.Enabled = false;
            this.m_openInWorkshopButton.Enabled = false;
            base.CloseButtonEnabled = true;
            base.m_sideMenuLayout.Add(control, MyAlignH.Left, MyAlignV.Top, 2, 0, 1, 1);
            base.m_sideMenuLayout.Add(this.m_difficultyCombo, MyAlignH.Left, MyAlignV.Top, 2, 1, 1, 1);
            base.m_sideMenuLayout.Add(label2, MyAlignH.Left, MyAlignV.Top, 3, 0, 1, 1);
            base.m_sideMenuLayout.Add(this.m_onlineMode, MyAlignH.Left, MyAlignV.Top, 3, 1, 1, 1);
            base.m_sideMenuLayout.Add(this.m_maxPlayersLabel, MyAlignH.Left, MyAlignV.Top, 4, 0, 1, 1);
            base.m_sideMenuLayout.Add(this.m_maxPlayersSlider, MyAlignH.Left, MyAlignV.Top, 4, 1, 1, 1);
            base.m_buttonsLayout.Add(this.m_removeButton, MyAlignH.Left, MyAlignV.Top, 0, 0, 1, 1);
            base.m_buttonsLayout.Add(this.m_publishButton, MyAlignH.Left, MyAlignV.Top, 0, 1, 1, 1);
            base.m_buttonsLayout.Add(this.m_editButton, MyAlignH.Left, MyAlignV.Top, 0, 2, 1, 1);
            base.m_buttonsLayout.Add(this.m_browseWorkshopButton, MyAlignH.Left, MyAlignV.Top, 0, 3, 1, 1);
            base.m_buttonsLayout.Add(this.m_refreshButton, MyAlignH.Left, MyAlignV.Top, 1, 0, 1, 1);
            base.m_buttonsLayout.Add(this.m_openInWorkshopButton, MyAlignH.Left, MyAlignV.Top, 1, 1, 1, 1);
        }

        public override bool CloseScreen() => 
            base.CloseScreen();

        private void endKeens(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            MyLoadListResult result2 = (MyLoadListResult) result;
            this.m_availableSavesKeens = result2.AvailableSaves;
            this.m_availableSavesKeens.Sort((x, y) => x.Item2.SessionName.CompareTo(y.Item2.SessionName));
            if (result2.ContainsCorruptedWorlds)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SomeWorldFilesCouldNotBeLoaded), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            this.AfterPartLoaded();
            screen.CloseScreen();
        }

        private void endLocal(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            MyLoadListResult result2 = (MyLoadListResult) result;
            result2.AvailableSaves.Sort((x, y) => x.Item2.SessionName.CompareTo(y.Item2.SessionName));
            this.m_availableSavesLocal = result2.AvailableSaves;
            this.AfterPartLoaded();
            screen.CloseScreen();
        }

        private void endWorkshop(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            LoadWorkshopResult result2 = (LoadWorkshopResult) result;
            this.m_subscribedScenarios = result2.SubscribedScenarios;
            foreach (MyWorkshopItem item in result2.SubscribedScenarios)
            {
                MyWorldInfo info = new MyWorldInfo {
                    SessionName = item.Title,
                    Briefing = item.Description,
                    WorkshopId = new ulong?(item.Id)
                };
                this.m_availableSavesWorkshop.Add(new Tuple<string, MyWorldInfo>("workshop", info));
            }
            this.m_availableSavesWorkshop.Sort((x, y) => x.Item2.SessionName.CompareTo(y.Item2.SessionName));
            this.AfterPartLoaded();
            screen.CloseScreen();
        }

        protected override void FillList()
        {
            base.FillList();
            this.m_listLoadedParts = 0;
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginKeens), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endKeens), null));
            cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginWorkshop), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endWorkshop), null));
            cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginLocal), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endLocal), null));
        }

        private MyWorkshopItem FindWorkshopScenario(ulong workshopId)
        {
            using (List<MyWorkshopItem>.Enumerator enumerator = this.m_subscribedScenarios.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyWorkshopItem current = enumerator.Current;
                    if (current.Id == workshopId)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenScenario";

        protected override MyGuiHighlightTexture GetIcon(Tuple<string, MyWorldInfo> save) => 
            ((save.Item1 != "workshop") ? (!save.Item2.ScenarioEditMode ? MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL : MyGuiConstants.TEXTURE_ICON_MODS_LOCAL) : MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP);

        private MyWorkshopItem GetSubscribedItem(ulong? publishedFileId)
        {
            using (List<MyWorkshopItem>.Enumerator enumerator = this.m_subscribedScenarios.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyWorkshopItem current = enumerator.Current;
                    ulong? nullable = publishedFileId;
                    if ((current.Id == nullable.GetValueOrDefault()) & (nullable != null))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        protected override void LoadSandboxInternal(Tuple<string, MyWorldInfo> save, bool MP)
        {
            base.LoadSandboxInternal(save, MP);
            if (save.Item1 == "workshop")
            {
                MyWorkshop.CreateWorldInstanceAsync(this.FindWorkshopScenario(save.Item2.WorkshopId.Value), MyWorkshop.MyWorkshopPathInfo.CreateScenarioInfo(), true, delegate (bool success, string sessionPath) {
                    if (success)
                    {
                        ulong num;
                        MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out num);
                        checkpoint.Briefing = save.Item2.Briefing;
                        MyLocalCache.SaveCheckpoint(checkpoint, sessionPath);
                        MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Scenario);
                        MyScenarioSystem.LoadMission(sessionPath, MP, (MyOnlineModeEnum) ((int) this.m_onlineMode.GetSelectedKey()), (short) this.m_maxPlayersSlider.Value, MyGameModeEnum.Survival);
                    }
                    else
                    {
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextWorkshopDownloadFailed), MyTexts.Get(MyCommonTexts.ScreenCaptionWorkshop), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                });
            }
            else
            {
                MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Scenario);
                MyScenarioSystem.LoadMission(save.Item1, MP, (MyOnlineModeEnum) ((int) this.m_onlineMode.GetSelectedKey()), (short) this.m_maxPlayersSlider.Value, MyGameModeEnum.Survival);
            }
        }

        private void OnBrowseWorkshopClick(MyGuiControlButton obj)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_SCENARIOS, "Steam Workshop", false);
        }

        private void OnEditButtonClick(object sender)
        {
            MyGuiControlTable.Row selectedRow = base.m_scenarioTable.SelectedRow;
            if (selectedRow != null)
            {
                Tuple<string, MyWorldInfo> tuple = base.FindSave(selectedRow);
                if (tuple != null)
                {
                    this.CloseScreen();
                    MyOnlineModeEnum? onlineMode = null;
                    MySessionLoader.LoadSingleplayerSession(tuple.Item1, null, null, onlineMode, 0);
                }
            }
        }

        private void OnOnlineModeSelect()
        {
            this.m_maxPlayersSlider.Enabled = this.m_onlineMode.GetSelectedKey() != 0L;
            this.m_maxPlayersLabel.Enabled = this.m_onlineMode.GetSelectedKey() != 0L;
        }

        private void OnPublishButtonClick(MyGuiControlButton sender)
        {
            MyGuiControlTable.Row selectedRow = base.m_scenarioTable.SelectedRow;
            if ((selectedRow != null) && (selectedRow.UserData != null))
            {
                StringBuilder builder;
                MyStringId messageBoxCaptionDoYouWishToUpdateScenario;
                string fullPath = ((Tuple<string, MyWorldInfo>) selectedRow.UserData).Item1;
                MyWorldInfo worldInfo = base.FindSave(base.m_scenarioTable.SelectedRow).Item2;
                if (worldInfo.WorkshopId != null)
                {
                    builder = new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.MessageBoxTextDoYouWishToUpdateScenario), MySession.Platform);
                    messageBoxCaptionDoYouWishToUpdateScenario = MySpaceTexts.MessageBoxCaptionDoYouWishToUpdateScenario;
                }
                else
                {
                    builder = new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.MessageBoxTextDoYouWishToPublishScenario), MySession.Platform, MySession.PlatformLinkAgreement);
                    messageBoxCaptionDoYouWishToUpdateScenario = MySpaceTexts.MessageBoxCaptionDoYouWishToPublishScenario;
                }
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, builder, MyTexts.Get(messageBoxCaptionDoYouWishToUpdateScenario), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum val) {
                    if (val == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        MyWorkshopItem subscribedItem = this.GetSubscribedItem(worldInfo.WorkshopId);
                        if (subscribedItem != null)
                        {
                            subscribedItem.Tags.ToArray<string>();
                            if (subscribedItem.OwnerId != Sync.MyId)
                            {
                                MyStringId? nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                Vector2? nullable2 = null;
                                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_OwnerMismatchMod), MyTexts.Get(MyCommonTexts.MessageBoxCaptionModPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                                return;
                            }
                        }
                        MyWorkshop.PublishScenarioAsync(fullPath, worldInfo.SessionName, worldInfo.Description, worldInfo.WorkshopId, MyPublishedFileVisibility.Public, delegate (bool success, MyGameServiceCallResult result, ulong publishedFileId) {
                            MyStringId? nullable;
                            Vector2? nullable2;
                            if (!success)
                            {
                                StringBuilder messageText = (result != MyGameServiceCallResult.AccessDenied) ? new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.MessageBoxTextScenarioPublishFailed), MySession.Platform) : MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_AccessDenied);
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionModPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                            }
                            else
                            {
                                ulong num;
                                worldInfo.WorkshopId = new ulong?(publishedFileId);
                                MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(fullPath, out num);
                                checkpoint.WorkshopId = new ulong?(publishedFileId);
                                MyLocalCache.SaveCheckpoint(checkpoint, fullPath);
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.MessageBoxTextScenarioPublished), MySession.Platform), MyTexts.Get(MySpaceTexts.MessageBoxCaptionScenarioPublished), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum a) {
                                    MyGameService.OpenOverlayUrl($"http://steamcommunity.com/sharedfiles/filedetails/?id={publishedFileId}");
                                    this.FillList();
                                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                            }
                        });
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnRefreshButtonClick(object sender)
        {
            base.m_state = MyGuiScreenScenarioBase.StateEnum.ListNeedsReload;
        }

        protected override void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            base.OnTableItemSelected(sender, eventArgs);
            if (eventArgs.RowIndex < 2)
            {
                this.m_publishButton.Enabled = false;
                this.m_onlineMode.Enabled = false;
                this.m_onlineMode.SelectItemByIndex(0);
                this.m_editButton.Enabled = false;
            }
            else
            {
                this.m_publishButton.Enabled = false;
                this.m_onlineMode.Enabled = true;
                this.m_editButton.Enabled = false;
                if (base.m_scenarioTable.SelectedRow != null)
                {
                    this.m_publishButton.Enabled = MyFakes.ENABLE_WORKSHOP_PUBLISH;
                    if (base.FindSave(base.m_scenarioTable.SelectedRow).Item1 != "workshop")
                    {
                        this.m_editButton.Enabled = true;
                    }
                }
            }
        }

        protected override void SetDefaultValues()
        {
            base.SetDefaultValues();
            this.m_difficultyCombo.SelectItemByIndex(1);
            this.m_onlineMode.SelectItemByIndex(0);
        }

        public MyObjectBuilder_SessionSettings Settings =>
            this.m_settings;

        public MyObjectBuilder_Checkpoint Checkpoint =>
            this.m_checkpoint;

        protected override MyStringId ScreenCaption =>
            MySpaceTexts.ScreenCaptionScenario;

        protected override bool IsOnlineMode =>
            (this.m_onlineMode.GetSelectedKey() != 0L);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenScenario.<>c <>9 = new MyGuiScreenScenario.<>c();
            public static Comparison<Tuple<string, MyWorldInfo>> <>9__42_0;
            public static Comparison<Tuple<string, MyWorldInfo>> <>9__44_0;
            public static Comparison<Tuple<string, MyWorldInfo>> <>9__46_0;

            internal int <endKeens>b__42_0(Tuple<string, MyWorldInfo> x, Tuple<string, MyWorldInfo> y) => 
                x.Item2.SessionName.CompareTo(y.Item2.SessionName);

            internal int <endLocal>b__44_0(Tuple<string, MyWorldInfo> x, Tuple<string, MyWorldInfo> y) => 
                x.Item2.SessionName.CompareTo(y.Item2.SessionName);

            internal int <endWorkshop>b__46_0(Tuple<string, MyWorldInfo> x, Tuple<string, MyWorldInfo> y) => 
                x.Item2.SessionName.CompareTo(y.Item2.SessionName);
        }

        private class LoadWorkshopResult : IMyAsyncResult
        {
            public List<MyWorkshopItem> SubscribedScenarios;

            public LoadWorkshopResult()
            {
                this.Task = Parallel.Start(delegate {
                    this.SubscribedScenarios = new List<MyWorkshopItem>();
                    if (MyGameService.IsOnline)
                    {
                        MyWorkshop.GetSubscribedScenariosBlocking(this.SubscribedScenarios);
                    }
                });
            }

            public bool IsCompleted =>
                this.Task.IsComplete;

            public ParallelTasks.Task Task { get; private set; }
        }
    }
}

