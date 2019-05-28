namespace Sandbox.Game.Gui
{
    using ParallelTasks;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [PreloadRequired]
    public class MyGuiIngameScriptsPage : MyGuiScreenDebugBase
    {
        public const string STEAM_THUMBNAIL_NAME = @"Textures\GUI\Icons\IngameProgrammingIcon.png";
        public const string THUMBNAIL_NAME = "thumb.png";
        public const string DEFAULT_SCRIPT_NAME = "Script";
        public const string SCRIPTS_DIRECTORY = "IngameScripts";
        public const string SCRIPT_EXTENSION = ".cs";
        public const string WORKSHOP_SCRIPT_EXTENSION = ".bin";
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.37f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private static Task m_task;
        private static List<MyWorkshopItem> m_subscribedItemsList = new List<MyWorkshopItem>();
        private Vector2 m_controlPadding;
        private float m_textScale;
        private MyGuiControlButton m_createFromEditorButton;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_detailsButton;
        private MyGuiControlButton m_deleteButton;
        private MyGuiControlButton m_replaceButton;
        private MyGuiControlTextbox m_searchBox;
        private MyGuiControlButton m_searchClear;
        private static MyGuiControlListbox m_scriptList;
        private MyGuiDetailScreenScriptLocal m_detailScreen;
        private bool m_activeDetail;
        private MyGuiControlListbox.Item m_selectedItem;
        private MyGuiControlRotatingWheel m_wheel;
        private string m_localScriptFolder;
        private string m_workshopFolder;
        private Action OnClose;
        private Action<string> OnScriptOpened;
        private Func<string> GetCodeFromEditor;

        static MyGuiIngameScriptsPage()
        {
            Vector2? position = null;
            m_scriptList = new MyGuiControlListbox(position, MyGuiControlListboxStyleEnum.IngameScipts);
        }

        public MyGuiIngameScriptsPage(Action<string> onScriptOpened, Func<string> getCodeFromEditor, Action close) : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new Vector2?(SCREEN_SIZE), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), false)
        {
            this.m_controlPadding = new Vector2(0.02f, 0.02f);
            this.m_textScale = 0.8f;
            base.EnabledBackgroundFade = true;
            this.OnClose = close;
            this.GetCodeFromEditor = getCodeFromEditor;
            this.OnScriptOpened = onScriptOpened;
            this.m_localScriptFolder = Path.Combine(MyFileSystem.UserDataPath, "IngameScripts", "local");
            this.m_workshopFolder = Path.Combine(MyFileSystem.UserDataPath, "IngameScripts", "workshop");
            if (!Directory.Exists(this.m_localScriptFolder))
            {
                Directory.CreateDirectory(this.m_localScriptFolder);
            }
            if (!Directory.Exists(this.m_workshopFolder))
            {
                Directory.CreateDirectory(this.m_workshopFolder);
            }
            m_scriptList.Items.Clear();
            this.GetLocalScriptNames(m_subscribedItemsList.Count == 0);
            this.RecreateControls(true);
            m_scriptList.ItemsSelected += new Action<MyGuiControlListbox>(this.OnSelectItem);
            m_scriptList.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnItemDoubleClick);
            base.OnEnterCallback = (Action) Delegate.Combine(base.OnEnterCallback, new Action(this.Ok));
            base.m_canShareInput = false;
            base.CanBeHidden = true;
            base.CanHideOthers = false;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            this.m_searchBox.TextChanged += new Action<MyGuiControlTextbox>(this.OnSearchTextChange);
        }

        private static void AddWorkshopItemsToList()
        {
            foreach (MyWorkshopItem item in m_subscribedItemsList)
            {
                MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(item.Id));
                info1.Item = item;
                MyBlueprintItemInfo info = info1;
                info.SetAdditionalBlueprintInformation(item.Title, item.Description, item.DLCs.ToArray<uint>());
                object userData = info;
                MyGuiControlListbox.Item item2 = new MyGuiControlListbox.Item(new StringBuilder(item.Title), item.Title, MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Normal, userData, null);
                int? position = null;
                m_scriptList.Add(item2, position);
            }
        }

        public void ChangeName(string newName)
        {
            newName = MyUtils.StripInvalidChars(newName);
            string oldName = this.m_selectedItem.Text.ToString();
            string path = Path.Combine(this.m_localScriptFolder, oldName);
            string newFile = Path.Combine(this.m_localScriptFolder, newName);
            if ((path != newFile) && Directory.Exists(path))
            {
                MyStringId? nullable;
                Vector2? nullable2;
                if (Directory.Exists(newFile))
                {
                    if (path.ToLower() == newFile.ToLower())
                    {
                        this.RenameScript(oldName, newName);
                        this.RefreshAndReloadScriptsList(false);
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendFormat(MySpaceTexts.ProgrammableBlock_ReplaceScriptNameDialogText, newName);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, stringBuilder, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ReplaceScriptNameDialogTitle), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                Directory.Delete(newFile, true);
                                this.RenameScript(oldName, newName);
                                this.RefreshAndReloadScriptsList(false);
                            }
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
                else
                {
                    try
                    {
                        this.RenameScript(oldName, newName);
                        this.RefreshAndReloadScriptsList(false);
                    }
                    catch (IOException)
                    {
                        StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.LoadScreenButtonDelete);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ReplaceScriptNameUsed), messageCaption, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
            }
        }

        public override bool CloseScreen()
        {
            if (this.OnClose != null)
            {
                this.OnClose();
            }
            return base.CloseScreen();
        }

        protected MyGuiControlButton CreateButton(float usableWidth, StringBuilder text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?(), float textScale = 1f)
        {
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlButton button = base.AddButton(text, onClick, null, textColor, size, true, true);
            button.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
            button.TextScale = textScale;
            button.Size = new Vector2(usableWidth, button.Size.Y);
            button.Position += new Vector2(-0.02f, 0f);
            button.Enabled = enabled;
            if (tooltip != null)
            {
                button.SetToolTip(tooltip.Value);
            }
            return button;
        }

        private void CreateButtons()
        {
            Vector2 vector = new Vector2(-0.083f, 0.15f);
            Vector2 vector2 = new Vector2(0.134f, 0.038f);
            float usableWidth = 0.131f;
            float num2 = 0.265f;
            float textScale = this.m_textScale;
            this.m_okButton = this.CreateButton(usableWidth, MyTexts.Get(MyCommonTexts.Ok), new Action<MyGuiControlButton>(this.OnOk), false, new MyStringId?(MyCommonTexts.Scripts_NoSelectedScript), textScale);
            this.m_okButton.Position = vector;
            this.m_okButton.ShowTooltipWhenDisabled = true;
            textScale = this.m_textScale;
            this.m_detailsButton = this.CreateButton(usableWidth, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ButtonDetails), new Action<MyGuiControlButton>(this.OnDetails), false, new MyStringId?(MyCommonTexts.Scripts_NoSelectedScript), textScale);
            this.m_detailsButton.Position = vector + (new Vector2(1f, 0f) * vector2);
            this.m_detailsButton.ShowTooltipWhenDisabled = true;
            textScale = this.m_textScale;
            this.m_replaceButton = this.CreateButton(num2, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ButtonReplaceFromEditor), new Action<MyGuiControlButton>(this.OnReplaceFromEditor), false, new MyStringId?(MyCommonTexts.Scripts_NoSelectedScript), textScale);
            this.m_replaceButton.Position = vector + (new Vector2(0f, 1f) * vector2);
            this.m_replaceButton.PositionX += vector2.X / 2f;
            this.m_replaceButton.ShowTooltipWhenDisabled = true;
            textScale = this.m_textScale;
            this.m_deleteButton = this.CreateButton(num2, MyTexts.Get(MyCommonTexts.LoadScreenButtonDelete), new Action<MyGuiControlButton>(this.OnDelete), false, new MyStringId?(MyCommonTexts.Scripts_NoSelectedScript), textScale);
            this.m_deleteButton.Position = vector + (new Vector2(0f, 2f) * vector2);
            this.m_deleteButton.PositionX += vector2.X / 2f;
            this.m_deleteButton.ShowTooltipWhenDisabled = true;
            vector = new Vector2(-0.083f, 0.305f);
            textScale = this.m_textScale;
            MyGuiControlButton button1 = this.CreateButton(num2, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ButtonCreateFromEditor), new Action<MyGuiControlButton>(this.OnCreateFromEditor), true, new MyStringId?(MyCommonTexts.Scripts_NewFromEditorTooltip), textScale);
            button1.ShowTooltipWhenDisabled = true;
            button1.Position = vector + (new Vector2(0f, 0f) * vector2);
            button1.PositionX += vector2.X / 2f;
            textScale = this.m_textScale;
            MyGuiControlButton button2 = this.CreateButton(num2, MyTexts.Get(MySpaceTexts.DetailScreen_Button_OpenInWorkshop), new Action<MyGuiControlButton>(this.OnOpenWorkshop), true, new MyStringId?(MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop), textScale);
            button2.Position = vector + (new Vector2(0f, 1f) * vector2);
            button2.PositionX += vector2.X / 2f;
            textScale = this.m_textScale;
            MyGuiControlButton button3 = this.CreateButton(num2, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ButtonRefreshScripts), new Action<MyGuiControlButton>(this.OnReload), true, new MyStringId?(MyCommonTexts.Scripts_RefreshTooltip), textScale);
            button3.Position = vector + (new Vector2(0f, 2f) * vector2);
            button3.PositionX += vector2.X / 2f;
            MyGuiControlButton button4 = this.CreateButton(num2, MyTexts.Get(MyCommonTexts.Close), new Action<MyGuiControlButton>(this.OnCancel), true, new MyStringId?(MySpaceTexts.ToolTipNewsletter_Close), this.m_textScale);
            button4.Position = vector + (new Vector2(0f, 3f) * vector2);
            button4.PositionX += vector2.X / 2f;
        }

        private bool DeleteScript(string p)
        {
            string path = Path.Combine(this.m_localScriptFolder, p);
            if (!Directory.Exists(path))
            {
                return false;
            }
            Directory.Delete(path, true);
            return true;
        }

        private void DownloadScriptFromSteam()
        {
            if (this.m_selectedItem != null)
            {
                MyWorkshop.DownloadScriptBlocking((this.m_selectedItem.UserData as MyBlueprintItemInfo).Item);
            }
        }

        public override string GetFriendlyName() => 
            "MyIngameScriptScreen";

        private void GetLocalScriptNames(bool reload = false)
        {
            if (Directory.Exists(this.m_localScriptFolder))
            {
                string[] directories = Directory.GetDirectories(this.m_localScriptFolder);
                for (int i = 0; i < directories.Length; i++)
                {
                    string fileName = Path.GetFileName(directories[i]);
                    ulong? id = null;
                    MyBlueprintItemInfo info = new MyBlueprintItemInfo(MyBlueprintTypeEnum.LOCAL, id);
                    info.SetAdditionalBlueprintInformation(fileName, null, null);
                    object userData = info;
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(fileName), fileName, MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL.Normal, userData, null);
                    int? position = null;
                    m_scriptList.Add(item, position);
                }
                if (m_task.IsComplete & reload)
                {
                    this.GetWorkshopScripts();
                }
                else
                {
                    AddWorkshopItemsToList();
                }
            }
        }

        private void GetScriptsInfo()
        {
            m_subscribedItemsList.Clear();
            bool subscribedIngameScriptsBlocking = MyWorkshop.GetSubscribedIngameScriptsBlocking(m_subscribedItemsList);
            if (subscribedIngameScriptsBlocking)
            {
                if (Directory.Exists(this.m_workshopFolder))
                {
                    try
                    {
                        Directory.Delete(this.m_workshopFolder, true);
                    }
                    catch (IOException)
                    {
                    }
                }
                Directory.CreateDirectory(this.m_workshopFolder);
            }
            if (subscribedIngameScriptsBlocking)
            {
                AddWorkshopItemsToList();
            }
            else
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Couldn't load scripts from steam workshop"), new StringBuilder("Error"), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void GetWorkshopScripts()
        {
            m_task = Parallel.Start(new Action(this.GetScriptsInfo));
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((MyInput.Static.IsNewKeyPressed(MyKeys.F12) || MyInput.Static.IsNewKeyPressed(MyKeys.F11)) || MyInput.Static.IsNewKeyPressed(MyKeys.F10))
            {
                this.CloseScreen();
            }
        }

        protected MyGuiControlLabel MakeLabel(string text, Vector2 position, float textScale = 1f)
        {
            Vector2? size = null;
            return new MyGuiControlLabel(new Vector2?(position), size, text, null, textScale, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
        }

        private void Ok()
        {
            if (this.m_selectedItem == null)
            {
                this.CloseScreen();
            }
            else
            {
                this.OpenSelectedSript();
            }
        }

        private void OnCancel(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.m_activeDetail)
            {
                this.m_detailScreen.CloseScreen();
            }
        }

        public void OnCreateFromEditor(MyGuiControlButton button)
        {
            if ((this.GetCodeFromEditor != null) && Directory.Exists(this.m_localScriptFolder))
            {
                int num = 0;
                while (true)
                {
                    if (!Directory.Exists(Path.Combine(this.m_localScriptFolder, "Script_" + num.ToString())))
                    {
                        string path = Path.Combine(this.m_localScriptFolder, "Script_" + num);
                        Directory.CreateDirectory(path);
                        File.Copy(Path.Combine(MyFileSystem.ContentPath, @"Textures\GUI\Icons\IngameProgrammingIcon.png"), Path.Combine(path, "thumb.png"), true);
                        string contents = this.GetCodeFromEditor();
                        File.WriteAllText(Path.Combine(path, "Script.cs"), contents, Encoding.UTF8);
                        this.RefreshAndReloadScriptsList(false);
                        break;
                    }
                    num++;
                }
            }
        }

        public void OnDelete(MyGuiControlButton button)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.LoadScreenButtonDelete);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ProgrammableBlock_DeleteScriptDialogText), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if ((callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES) && (this.m_selectedItem != null))
                {
                    if (this.DeleteScript(this.m_selectedItem.Text.ToString()))
                    {
                        this.m_okButton.Enabled = false;
                        this.m_detailsButton.Enabled = false;
                        this.m_deleteButton.Enabled = false;
                        this.m_replaceButton.Enabled = false;
                        this.m_okButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
                        this.m_detailsButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
                        this.m_replaceButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
                        this.m_deleteButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
                        this.m_selectedItem = null;
                    }
                    this.RefreshBlueprintList(false);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnDetails(MyGuiControlButton button)
        {
            if (this.m_selectedItem == null)
            {
                if (this.m_activeDetail)
                {
                    MyScreenManager.RemoveScreen(this.m_detailScreen);
                }
            }
            else if (this.m_activeDetail)
            {
                MyScreenManager.RemoveScreen(this.m_detailScreen);
            }
            else if (!this.m_activeDetail)
            {
                if ((this.m_selectedItem.UserData as MyBlueprintItemInfo).Type == MyBlueprintTypeEnum.LOCAL)
                {
                    if (Directory.Exists(Path.Combine(this.m_localScriptFolder, this.m_selectedItem.Text.ToString())))
                    {
                        this.m_detailScreen = new MyGuiDetailScreenScriptLocal(delegate (MyBlueprintItemInfo item) {
                            if (item == null)
                            {
                                this.m_okButton.Enabled = false;
                                this.m_detailsButton.Enabled = false;
                                this.m_deleteButton.Enabled = false;
                                this.m_replaceButton.Enabled = false;
                            }
                            this.m_activeDetail = false;
                            if (m_task.IsComplete)
                            {
                                this.RefreshBlueprintList(this.m_detailScreen.WasPublished);
                            }
                        }, this.m_selectedItem.UserData as MyBlueprintItemInfo, this, this.m_textScale);
                        this.m_activeDetail = true;
                        MyScreenManager.InputToNonFocusedScreens = true;
                        MyScreenManager.AddScreen(this.m_detailScreen);
                    }
                    else
                    {
                        StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ScriptNotFound), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                }
                else if ((this.m_selectedItem.UserData as MyBlueprintItemInfo).Type == MyBlueprintTypeEnum.STEAM)
                {
                    this.m_detailScreen = new MyGuiDetailScreenScriptLocal(delegate (MyBlueprintItemInfo item) {
                        this.m_activeDetail = false;
                        if (m_task.IsComplete)
                        {
                            this.RefreshBlueprintList(false);
                        }
                    }, this.m_selectedItem.UserData as MyBlueprintItemInfo, this, this.m_textScale);
                    this.m_activeDetail = true;
                    MyScreenManager.InputToNonFocusedScreens = true;
                    MyScreenManager.AddScreen(this.m_detailScreen);
                }
            }
        }

        private void OnItemDoubleClick(MyGuiControlListbox list)
        {
            this.m_selectedItem = list.SelectedItems[0];
            object userData = this.m_selectedItem.UserData;
            this.OpenSelectedSript();
        }

        private void OnOk(MyGuiControlButton button)
        {
            this.Ok();
        }

        private void OnOpenWorkshop(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_INGAMESCRIPTS, "Steam Workshop", false);
        }

        private void OnReload(MyGuiControlButton button)
        {
            this.m_selectedItem = null;
            this.m_okButton.Enabled = false;
            this.m_detailsButton.Enabled = false;
            this.m_deleteButton.Enabled = false;
            this.m_replaceButton.Enabled = false;
            this.m_okButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
            this.m_detailsButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
            this.m_replaceButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
            this.m_deleteButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_NoSelectedScript));
            this.RefreshAndReloadScriptsList(true);
        }

        private void OnRename(MyGuiControlButton button)
        {
            if (this.m_selectedItem != null)
            {
                string caption = MyTexts.GetString(MySpaceTexts.ProgrammableBlock_NewScriptName);
                MyScreenManager.AddScreen(new MyGuiBlueprintTextDialog(new Vector2(0.5f, 0.5f), delegate (string result) {
                    if (result != null)
                    {
                        this.ChangeName(result);
                    }
                }, this.m_selectedItem.Text.ToString(), caption, 50, 0.3f));
            }
        }

        public void OnReplaceFromEditor(MyGuiControlButton button)
        {
            if (((this.m_selectedItem != null) && (this.GetCodeFromEditor != null)) && Directory.Exists(this.m_localScriptFolder))
            {
                StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.ProgrammableBlock_ReplaceScriptNameDialogTitle);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ReplaceScriptDialogText), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                    if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                        string path = Path.Combine(this.m_localScriptFolder, userData.Data.Name, "Script.cs");
                        if (File.Exists(path))
                        {
                            File.WriteAllText(path, this.GetCodeFromEditor(), Encoding.UTF8);
                        }
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnScriptDownloaded()
        {
            if ((this.OnScriptOpened != null) && (this.m_selectedItem != null))
            {
                MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                this.OnScriptOpened(userData.Item.Folder);
            }
            m_scriptList.Enabled = true;
        }

        private void OnSearchClear(MyGuiControlButton button)
        {
            this.m_searchBox.Text = "";
        }

        private void OnSearchTextChange(MyGuiControlTextbox box)
        {
            if (box.Text != "")
            {
                char[] separator = new char[] { ' ' };
                string[] strArray = box.Text.Split(separator);
                foreach (MyGuiControlListbox.Item item in m_scriptList.Items)
                {
                    string str = item.Text.ToString().ToLower();
                    bool flag = true;
                    string[] strArray2 = strArray;
                    int index = 0;
                    while (true)
                    {
                        if (index < strArray2.Length)
                        {
                            string str2 = strArray2[index];
                            if (str.Contains(str2.ToLower()))
                            {
                                index++;
                                continue;
                            }
                            flag = false;
                        }
                        item.Visible = flag;
                        break;
                    }
                }
            }
            else
            {
                using (ObservableCollection<MyGuiControlListbox.Item>.Enumerator enumerator = m_scriptList.Items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Visible = true;
                    }
                }
            }
        }

        private void OnSelectItem(MyGuiControlListbox list)
        {
            if (list.SelectedItems.Count != 0)
            {
                this.m_selectedItem = list.SelectedItems[0];
                this.m_detailsButton.Enabled = true;
                MyBlueprintTypeEnum type = (this.m_selectedItem.UserData as MyBlueprintItemInfo).Type;
                ulong? publishedItemId = (this.m_selectedItem.UserData as MyBlueprintItemInfo).PublishedItemId;
                if (type == MyBlueprintTypeEnum.LOCAL)
                {
                    this.m_okButton.Enabled = true;
                    this.m_detailsButton.Enabled = true;
                    this.m_replaceButton.Enabled = true;
                    this.m_deleteButton.Enabled = true;
                    this.m_okButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_OkTooltip));
                    this.m_detailsButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_DetailsTooltip));
                    this.m_replaceButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_ReplaceTooltip));
                    this.m_deleteButton.SetTooltip(MyTexts.GetString(MyCommonTexts.Scripts_DeleteTooltip));
                }
                else if (type != MyBlueprintTypeEnum.STEAM)
                {
                    if (type == MyBlueprintTypeEnum.SHARED)
                    {
                        this.m_detailsButton.Enabled = false;
                        this.m_deleteButton.Enabled = false;
                    }
                }
                else
                {
                    this.m_okButton.Enabled = true;
                    this.m_detailsButton.Enabled = true;
                    this.m_deleteButton.Enabled = false;
                    this.m_replaceButton.Enabled = false;
                    this.m_deleteButton.SetToolTip(MyTexts.GetString(MyCommonTexts.Scripts_LocalScriptsOnly));
                    this.m_replaceButton.SetToolTip(MyTexts.GetString(MyCommonTexts.Scripts_LocalScriptsOnly));
                }
            }
        }

        private void OpenSelectedSript()
        {
            MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
            if (userData.Type == MyBlueprintTypeEnum.STEAM)
            {
                this.OpenSharedScript(userData);
            }
            else if (this.OnScriptOpened != null)
            {
                string[] paths = new string[] { MyFileSystem.UserDataPath, "IngameScripts", "local", userData.Data.Name, "Script.cs" };
                this.OnScriptOpened(Path.Combine(paths));
            }
            this.CloseScreen();
        }

        private void OpenSharedScript(MyBlueprintItemInfo itemInfo)
        {
            m_scriptList.Enabled = false;
            m_task = Parallel.Start(new Action(this.DownloadScriptFromSteam), new Action(this.OnScriptDownloaded));
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector = new Vector2(0.02f, SCREEN_SIZE.Y - 1.076f);
            float num = (SCREEN_SIZE.Y - 1f) / 2f;
            base.AddCaption(MyTexts.Get(MySpaceTexts.ProgrammableBlock_ScriptsScreenTitle).ToString(), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(this.m_controlPadding + new Vector2(-HIDDEN_PART_RIGHT, num - 0.03f)), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.44f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.123f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.278f), base.m_size.Value.X * 0.73f, 0f, color);
            this.Controls.Add(control);
            MyGuiControlLabel label = this.MakeLabel(MyTexts.GetString(MyCommonTexts.ScreenCubeBuilderBlockSearch), vector + new Vector2(-0.129f, -0.015f), this.m_textScale);
            label.Position = new Vector2(-0.15f, -0.406f);
            this.m_searchBox = new MyGuiControlTextbox();
            this.m_searchBox.Position = new Vector2(0.115f, -0.401f);
            this.m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_searchBox.Size = new Vector2(0.257f - label.Size.X, 0.2f);
            this.m_searchBox.SetToolTip(MyCommonTexts.Scripts_SearchTooltip);
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = vector + new Vector2(0.068f, -0.521f);
            button1.Size = new Vector2(0.045f, 0.05666667f);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Close;
            button1.ActivateOnMouseRelease = true;
            this.m_searchClear = button1;
            this.m_searchClear.ButtonClicked += new Action<MyGuiControlButton>(this.OnSearchClear);
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.145f, -0.357f);
            label1.Name = "ControlLabel";
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MyCommonTexts.Scripts_ListOfScripts);
            MyGuiControlLabel label2 = label1;
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.1535f, -0.362f), new Vector2(0.2685f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            m_scriptList.Size -= new Vector2(0.5f, 0f);
            m_scriptList.Position = new Vector2(-0.019f, -0.115f);
            m_scriptList.VisibleRowsCount = 12;
            m_scriptList.MultiSelect = false;
            this.Controls.Add(label);
            this.Controls.Add(this.m_searchBox);
            this.Controls.Add(this.m_searchClear);
            this.Controls.Add(m_scriptList);
            this.Controls.Add(panel);
            this.Controls.Add(label2);
            this.CreateButtons();
            string texture = @"Textures\GUI\screens\screen_loading_wheel.dds";
            Vector2? textureResolution = null;
            this.m_wheel = new MyGuiControlRotatingWheel(new Vector2(-0.02f, -0.1f), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.28f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, texture, true, MyPerGameSettings.GUI.MultipleSpinningWheels, textureResolution, 1.5f);
            this.Controls.Add(this.m_wheel);
            this.m_wheel.Visible = false;
        }

        public void RefreshAndReloadScriptsList(bool refreshWorkshopList = false)
        {
            m_scriptList.Items.Clear();
            this.GetLocalScriptNames(refreshWorkshopList);
        }

        public void RefreshBlueprintList(bool fromTask = false)
        {
            m_scriptList.Items.Clear();
            this.GetLocalScriptNames(fromTask);
        }

        private void RenameScript(string oldName, string newName)
        {
            string path = Path.Combine(this.m_localScriptFolder, oldName);
            if (Directory.Exists(path))
            {
                Directory.Move(path, Path.Combine(this.m_localScriptFolder, newName));
            }
            this.DeleteScript(oldName);
        }

        public override bool Update(bool hasFocus)
        {
            if (!m_task.IsComplete)
            {
                this.m_wheel.Visible = true;
            }
            if (m_task.IsComplete)
            {
                this.m_wheel.Visible = false;
            }
            return base.Update(hasFocus);
        }

        private bool ValidateSelecteditem() => 
            ((this.m_selectedItem != null) ? ((this.m_selectedItem.UserData != null) ? (this.m_selectedItem.Text != null) : false) : false);
    }
}

