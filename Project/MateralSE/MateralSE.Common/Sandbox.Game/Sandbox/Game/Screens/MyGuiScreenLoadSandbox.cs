namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenLoadSandbox : MyGuiScreenBase
    {
        public static readonly string CONST_THUMB = "//thumb.jpg";
        public static readonly string CONST_BACKUP = "//Backup";
        private MyGuiControlSaveBrowser m_saveBrowser;
        private MyGuiControlButton m_continueLastSave;
        private MyGuiControlButton m_loadButton;
        private MyGuiControlButton m_editButton;
        private MyGuiControlButton m_saveButton;
        private MyGuiControlButton m_deleteButton;
        private MyGuiControlButton m_publishButton;
        private MyGuiControlButton m_subscribedWorldsButton;
        private MyGuiControlButton m_backupsButton;
        private MyGuiControlButton m_backButton;
        private int m_selectedRow;
        private int m_lastSelectedRow;
        private bool m_rowAutoSelect;
        private MyGuiControlRotatingWheel m_loadingWheel;
        private MyGuiControlImage m_levelImage;
        private MyGuiControlSearchBox m_searchBox;

        public MyGuiScreenLoadSandbox() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.874f, 0.97f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_rowAutoSelect = true;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void CopyBackupUpALevel(ref string saveFilePath, MyWorldInfo worldInfo)
        {
            DirectoryInfo info = new DirectoryInfo(saveFilePath);
            DirectoryInfo targetDirectory = info.Parent.Parent;
            targetDirectory.GetFiles().ForEach<FileInfo>(file => file.Delete());
            info.GetFiles().ForEach<FileInfo>(file => file.CopyTo(Path.Combine(targetDirectory.FullName, file.Name)));
            saveFilePath = targetDirectory.FullName;
        }

        private void DebugOverrideAutosaveCheckboxIsCheckChanged(MyGuiControlCheckbox checkbox)
        {
            MySandboxGame.Config.DebugOverrideAutosave = checkbox.IsChecked;
            MySandboxGame.Config.Save();
        }

        private void DebugWorldCheckboxIsCheckChanged(MyGuiControlCheckbox checkbox)
        {
            string directory = checkbox.IsChecked ? Path.Combine(MyFileSystem.ContentPath, "Worlds") : MyFileSystem.SavesPath;
            this.m_saveBrowser.SetTopMostAndCurrentDir(directory);
            this.m_saveBrowser.Refresh();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenLoadSandbox";

        private void LoadImagePreview()
        {
            MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
            if (selectedRow != null)
            {
                Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                if (save == null)
                {
                    goto TR_0000;
                }
                else if (!save.Item2.IsCorrupted)
                {
                    string str = save.Item1;
                    if (Directory.Exists(str + CONST_BACKUP))
                    {
                        string[] directories = Directory.GetDirectories(str + CONST_BACKUP);
                        if (directories.Any<string>())
                        {
                            string str3 = directories.Last<string>().ToString() + CONST_THUMB;
                            if (File.Exists(str3) && (new FileInfo(str3).Length > 0L))
                            {
                                this.m_levelImage.SetTexture(Directory.GetDirectories(str + CONST_BACKUP).Last<string>().ToString() + CONST_THUMB);
                                return;
                            }
                        }
                    }
                    string path = str + CONST_THUMB;
                    if (File.Exists(path) && (new FileInfo(path).Length > 0L))
                    {
                        this.m_levelImage.SetTexture(null);
                        this.m_levelImage.SetTexture(str + CONST_THUMB);
                        return;
                    }
                    this.m_levelImage.SetTexture(@"Textures\GUI\Screens\image_background.dds");
                }
                else
                {
                    goto TR_0000;
                }
            }
            return;
        TR_0000:
            this.m_levelImage.SetTexture(@"Textures\GUI\Screens\image_background.dds");
        }

        private void LoadSandbox()
        {
            StringBuilder builder;
            MyLog.Default.WriteLine("LoadSandbox() - Start");
            MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
            if (selectedRow != null)
            {
                Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                if (this.m_saveBrowser.GetDirectory(selectedRow) != null)
                {
                    return;
                }
                if (save == null)
                {
                    goto TR_0001;
                }
                else if (!save.Item2.IsCorrupted)
                {
                    string saveFilePath = save.Item1;
                    if (this.m_saveBrowser.InBackupsFolder)
                    {
                        this.CopyBackupUpALevel(ref saveFilePath, save.Item2);
                    }
                    MyOnlineModeEnum? onlineMode = null;
                    MySessionLoader.LoadSingleplayerSession(saveFilePath, null, null, onlineMode, 0);
                }
                else
                {
                    goto TR_0001;
                }
            }
            MyLog.Default.WriteLine("LoadSandbox() - End");
            return;
        TR_0001:
            builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.WorldFileCouldNotBeLoaded), builder, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, Action<MyGuiControlButton> onClick)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(text), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private void OnBackClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void OnBackupsButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.m_saveBrowser.AccessBackups();
        }

        private void OnContinueLastGameClick(MyGuiControlButton sender)
        {
            MySessionLoader.LoadLastSession();
            this.m_continueLastSave.Enabled = false;
        }

        private void OnDeleteClick(MyGuiControlButton sender)
        {
            MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
            if (selectedRow != null)
            {
                MyStringId? nullable;
                Vector2? nullable2;
                Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                if (save != null)
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextAreYouSureYouWantToDeleteSave, save.Item2.SessionName), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnDeleteConfirm), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    DirectoryInfo directory = this.m_saveBrowser.GetDirectory(selectedRow);
                    if (directory != null)
                    {
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextAreYouSureYouWantToDeleteSave, directory.Name), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnDeleteConfirm), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
            }
        }

        private void OnDeleteConfirm(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
                if (selectedRow != null)
                {
                    MyStringId? nullable;
                    Vector2? nullable2;
                    Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                    if (save != null)
                    {
                        try
                        {
                            Directory.Delete(save.Item1, true);
                            this.m_saveBrowser.RemoveSelectedRow();
                            this.m_saveBrowser.SelectedRowIndex = new int?(this.m_selectedRow);
                            this.m_saveBrowser.Refresh();
                            this.m_levelImage.SetTexture(null);
                        }
                        catch (Exception)
                        {
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SessionDeleteFailed), null, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                            return;
                        }
                    }
                    else
                    {
                        try
                        {
                            DirectoryInfo directory = this.m_saveBrowser.GetDirectory(selectedRow);
                            if (directory != null)
                            {
                                directory.Delete(true);
                                this.m_saveBrowser.Refresh();
                            }
                        }
                        catch (Exception)
                        {
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SessionDeleteFailed), null, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    }
                }
            }
        }

        private void OnEditClick(MyGuiControlButton sender)
        {
            MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
            if (selectedRow != null)
            {
                ulong num;
                Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(save.Item1, out num);
                if (((save == null) || save.Item2.IsCorrupted) || (checkpoint == null))
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.WorldFileCouldNotBeLoaded), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                else
                {
                    MySession.FixIncorrectSettings(checkpoint.Settings);
                    object[] args = new object[] { checkpoint, save.Item1 };
                    MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.EditWorldSettingsScreen, args);
                    screen.Closed += source => this.m_saveBrowser.ForceRefresh();
                    MyGuiSandbox.AddScreen(screen);
                    this.m_rowAutoSelect = true;
                }
            }
        }

        private void OnLoadClick(MyGuiControlButton sender)
        {
            this.LoadSandbox();
        }

        private void OnPublishClick(MyGuiControlButton sender)
        {
            MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
            if (selectedRow != null)
            {
                Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                if (save != null)
                {
                    Publish(save.Item1, save.Item2);
                }
            }
        }

        private void OnSaveAsClick(MyGuiControlButton sender)
        {
            MyGuiControlTable.Row selectedRow = this.m_saveBrowser.SelectedRow;
            if (selectedRow != null)
            {
                Tuple<string, MyWorldInfo> save = this.m_saveBrowser.GetSave(selectedRow);
                if (save != null)
                {
                    MyGuiScreenSaveAs screen = new MyGuiScreenSaveAs(save.Item2, save.Item1, null);
                    screen.SaveAsConfirm += new Action(this.OnSaveAsConfirm);
                    MyGuiSandbox.AddScreen(screen);
                }
            }
        }

        private void OnSaveAsConfirm()
        {
            this.m_saveBrowser.ForceRefresh();
        }

        private void OnSearchTextChange(string text)
        {
            this.m_saveBrowser.SearchTextFilter = text;
            this.m_saveBrowser.Refresh();
        }

        private void OnTableItemConfirmedOrDoubleClick(MyGuiControlTable table, MyGuiControlTable.EventArgs args)
        {
            this.LoadSandbox();
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            sender.CanHaveFocus = true;
            base.FocusedControl = sender;
            this.m_selectedRow = eventArgs.RowIndex;
            this.m_lastSelectedRow = this.m_selectedRow;
            this.LoadImagePreview();
        }

        private void OnWorkshopClick(MyGuiControlButton sender)
        {
            MyScreenManager.AddScreen(new MyGuiScreenLoadSubscribedWorld());
        }

        public static void Publish(string sessionPath, MyWorldInfo worlInfo)
        {
            if (MyFakes.XBOX_PREVIEW)
            {
                MyGuiSandbox.Show(MyCommonTexts.MessageBoxTextErrorFeatureNotAvailableYet, MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
            }
            else
            {
                StringBuilder builder;
                MyStringId messageBoxCaptionDoYouWishToUpdateWorld;
                if (worlInfo.WorkshopId != null)
                {
                    builder = new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextDoYouWishToUpdateWorld), MySession.Platform));
                    messageBoxCaptionDoYouWishToUpdateWorld = MyCommonTexts.MessageBoxCaptionDoYouWishToUpdateWorld;
                }
                else
                {
                    builder = new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextDoYouWishToPublishWorld), MySession.Platform, MySession.PlatformLinkAgreement));
                    messageBoxCaptionDoYouWishToUpdateWorld = MyCommonTexts.MessageBoxCaptionDoYouWishToPublishWorld;
                }
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, builder, MyTexts.Get(messageBoxCaptionDoYouWishToUpdateWorld), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum val) {
                    if (val == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        Action<MyGuiScreenMessageBox.ResultEnum, string[]> callback = delegate (MyGuiScreenMessageBox.ResultEnum tagsResult, string[] outTags) {
                            if (tagsResult == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                MyWorkshop.PublishWorldAsync(sessionPath, worlInfo.SessionName, worlInfo.Description, worlInfo.WorkshopId, outTags, MyPublishedFileVisibility.Public, delegate (bool success, MyGameServiceCallResult result, ulong publishedFileId) {
                                    MyStringId? nullable;
                                    Vector2? nullable2;
                                    if (!success)
                                    {
                                        StringBuilder messageText = (result != MyGameServiceCallResult.AccessDenied) ? new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublishFailed), MySession.Platform) : MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_AccessDenied);
                                        nullable = null;
                                        nullable = null;
                                        nullable = null;
                                        nullable = null;
                                        nullable2 = null;
                                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionWorldPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                                    }
                                    else
                                    {
                                        ulong num;
                                        worlInfo.WorkshopId = new ulong?(publishedFileId);
                                        MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out num);
                                        checkpoint.WorkshopId = new ulong?(publishedFileId);
                                        MyLocalCache.SaveCheckpoint(checkpoint, sessionPath);
                                        nullable = null;
                                        nullable = null;
                                        nullable = null;
                                        nullable = null;
                                        nullable2 = null;
                                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublished), MySession.Platform), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWorldPublished), nullable, nullable, nullable, nullable, a => MyGameService.OpenOverlayUrl($"http://steamcommunity.com/sharedfiles/filedetails/?id={publishedFileId}"), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                                    }
                                });
                            }
                        };
                        if (MyWorkshop.WorldCategories.Length != 0)
                        {
                            MyGuiSandbox.AddScreen(new MyGuiScreenWorkshopTags("world", MyWorkshop.WorldCategories, null, callback));
                        }
                        else
                        {
                            string[] textArray1 = new string[] { "world" };
                            callback(MyGuiScreenMessageBox.ResultEnum.YES, textArray1);
                        }
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            MyGuiControlButton button;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenMenuButtonLoadGame, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.872f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.872f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.872f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.872f, 0f, captionTextColor);
            this.Controls.Add(list2);
            MyGuiControlSeparatorList list3 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list3.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.87f) / 2f, (base.m_size.Value.Y / 2f) - 0.25f), base.m_size.Value.X * 0.2f, 0f, captionTextColor);
            this.Controls.Add(list3);
            Vector2 vector = new Vector2(-0.378f, -0.39f);
            Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
            Vector2? size = null;
            this.m_searchBox = new MyGuiControlSearchBox(new Vector2?(vector + new Vector2((minSizeGui.X * 1.1f) - 0.004f, 0.017f)), size, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.OnSearchTextChange);
            this.m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_searchBox.Size = new Vector2((1075f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) * 0.848f, 1f);
            this.Controls.Add(this.m_searchBox);
            this.m_saveBrowser = new MyGuiControlSaveBrowser();
            this.m_saveBrowser.Position = vector + new Vector2((minSizeGui.X * 1.1f) - 0.004f, 0.055f);
            this.m_saveBrowser.Size = new Vector2((1075f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) * 0.848f, 0.15f);
            this.m_saveBrowser.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_saveBrowser.VisibleRowsCount = 0x13;
            this.m_saveBrowser.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_saveBrowser.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            this.m_saveBrowser.ItemConfirmed += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            this.Controls.Add(this.m_saveBrowser);
            Vector2 vector3 = vector + (minSizeGui * 0.5f);
            float* singlePtr1 = (float*) ref vector3.Y;
            singlePtr1[0] += 0.002f;
            Vector2 vector4 = MyGuiConstants.MENU_BUTTONS_POSITION_DELTA;
            float* singlePtr2 = (float*) ref vector3.Y;
            singlePtr2[0] += 0.192f;
            this.m_editButton = button = this.MakeButton(vector3 + (vector4 * -0.25f), MyCommonTexts.LoadScreenButtonEditSettings, new Action<MyGuiControlButton>(this.OnEditClick));
            this.Controls.Add(button);
            this.m_publishButton = button = this.MakeButton(vector3 + (vector4 * 0.75f), MyCommonTexts.LoadScreenButtonPublish, new Action<MyGuiControlButton>(this.OnPublishClick));
            this.Controls.Add(button);
            this.m_backupsButton = button = this.MakeButton(vector3 + (vector4 * 1.75f), MyCommonTexts.LoadScreenButtonBackups, new Action<MyGuiControlButton>(this.OnBackupsButtonClick));
            this.Controls.Add(button);
            this.m_saveButton = button = this.MakeButton(vector3 + (vector4 * 2.75f), MyCommonTexts.LoadScreenButtonSaveAs, new Action<MyGuiControlButton>(this.OnSaveAsClick));
            this.Controls.Add(button);
            this.m_deleteButton = button = this.MakeButton(vector3 + (vector4 * 3.75f), MyCommonTexts.LoadScreenButtonDelete, new Action<MyGuiControlButton>(this.OnDeleteClick));
            this.Controls.Add(button);
            this.m_backupsButton.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipLoadGame_Backups));
            this.m_saveButton.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipLoadGame_SaveAs));
            this.m_deleteButton.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipLoadGame_Delete));
            this.m_editButton.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipLoadGame_EditSettings));
            Vector2 vector5 = vector3 + (vector4 * -3.65f);
            float* singlePtr3 = (float*) ref vector5.X;
            singlePtr3[0] -= (this.m_publishButton.Size.X / 2f) + 0.0025f;
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.Size = new Vector2(this.m_publishButton.Size.X, (this.m_publishButton.Size.X / 4f) * 3f);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            image1.Position = vector5;
            image1.BorderEnabled = true;
            image1.BorderSize = 1;
            image1.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            this.m_levelImage = image1;
            this.m_levelImage.SetTexture(@"Textures\GUI\Screens\image_background.dds");
            this.Controls.Add(this.m_levelImage);
            this.m_loadButton = button = this.MakeButton(new Vector2(0f, 0f) - new Vector2(-0.295f, (-base.m_size.Value.Y / 2f) + 0.071f), MyCommonTexts.LoadScreenButtonLoad, new Action<MyGuiControlButton>(this.OnLoadClick));
            this.Controls.Add(button);
            this.m_loadButton.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipLoadGame_Load));
            this.m_backButton.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            size = null;
            this.m_loadingWheel = new MyGuiControlRotatingWheel(new Vector2?(this.m_loadButton.Position + new Vector2(0.273f, -0.008f)), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.22f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, size, 1.5f);
            this.Controls.Add(this.m_loadingWheel);
            this.m_loadingWheel.Visible = false;
            this.m_publishButton.SetToolTip(MyTexts.GetString(MyCommonTexts.LoadScreenButtonTooltipPublish));
            this.m_loadButton.DrawCrossTextureWhenDisabled = false;
            this.m_editButton.DrawCrossTextureWhenDisabled = false;
            this.m_deleteButton.DrawCrossTextureWhenDisabled = false;
            this.m_saveButton.DrawCrossTextureWhenDisabled = false;
            this.m_publishButton.DrawCrossTextureWhenDisabled = false;
            base.CloseButtonEnabled = true;
        }

        public override bool Update(bool hasFocus)
        {
            if ((((this.m_saveBrowser != null) & hasFocus) && (this.m_saveBrowser.RowsCount != 0)) && this.m_rowAutoSelect)
            {
                if (this.m_lastSelectedRow < this.m_saveBrowser.RowsCount)
                {
                    this.m_saveBrowser.SelectedRow = this.m_saveBrowser.GetRow(this.m_lastSelectedRow);
                    this.m_selectedRow = this.m_lastSelectedRow;
                }
                else
                {
                    this.m_saveBrowser.SelectedRow = this.m_saveBrowser.GetRow(0);
                    this.m_selectedRow = this.m_lastSelectedRow = 0;
                }
                this.m_rowAutoSelect = false;
                this.m_saveBrowser.ScrollToSelection();
                this.LoadImagePreview();
            }
            if (this.m_saveBrowser.GetSave(this.m_saveBrowser.SelectedRow) != null)
            {
                this.m_loadButton.Enabled = true;
                this.m_editButton.Enabled = true;
                this.m_saveButton.Enabled = true;
                this.m_publishButton.Enabled = MyFakes.ENABLE_WORKSHOP_PUBLISH;
                this.m_backupsButton.Enabled = true;
            }
            else
            {
                this.m_loadButton.Enabled = false;
                this.m_editButton.Enabled = false;
                this.m_saveButton.Enabled = false;
                this.m_publishButton.Enabled = false;
                this.m_backupsButton.Enabled = false;
            }
            this.m_deleteButton.Enabled = this.m_saveBrowser.SelectedRow != null;
            return base.Update(hasFocus);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenLoadSandbox.<>c <>9 = new MyGuiScreenLoadSandbox.<>c();
            public static Action<FileInfo> <>9__41_0;

            internal void <CopyBackupUpALevel>b__41_0(FileInfo file)
            {
                file.Delete();
            }
        }
    }
}

