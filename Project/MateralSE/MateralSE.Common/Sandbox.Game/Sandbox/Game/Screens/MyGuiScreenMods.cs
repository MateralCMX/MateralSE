namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
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

    public class MyGuiScreenMods : MyGuiScreenBase
    {
        private MyGuiControlTable m_modsTableEnabled;
        private MyGuiControlTable m_modsTableDisabled;
        private MyGuiControlButton m_moveUpButton;
        private MyGuiControlButton m_moveDownButton;
        private MyGuiControlButton m_moveTopButton;
        private MyGuiControlButton m_moveBottomButton;
        private MyGuiControlButton m_moveLeftButton;
        private MyGuiControlButton m_moveLeftAllButton;
        private MyGuiControlButton m_moveRightButton;
        private MyGuiControlButton m_moveRightAllButton;
        private MyGuiControlButton m_openInWorkshopButton;
        private MyGuiControlButton m_refreshButton;
        private MyGuiControlButton m_browseWorkshopButton;
        private MyGuiControlButton m_publishModButton;
        private MyGuiControlButton m_okButton;
        private MyGuiControlTable.Row m_selectedRow;
        private MyGuiControlTable m_selectedTable;
        private bool m_listNeedsReload;
        private bool m_keepActiveMods;
        private List<MyWorkshopItem> m_subscribedMods;
        private List<MyWorkshopItem> m_worldMods;
        private List<MyObjectBuilder_Checkpoint.ModItem> m_modListToEdit;
        private MyObjectBuilder_Checkpoint.ModItem m_selectedMod;
        private HashSet<string> m_worldLocalMods;
        private HashSet<ulong> m_worldWorkshopMods;
        private MyGuiControlButton m_categoryCategorySelectButton;
        private List<MyGuiControlButton> m_categoryButtonList;
        private MyGuiControlSearchBox m_searchBox;
        private MyGuiControlButton m_searchClear;
        private List<string> m_tmpSearch;
        private List<string> m_selectedCategories;
        private Dictionary<ulong, StringBuilder> m_modsToolTips;

        public MyGuiScreenMods(List<MyObjectBuilder_Checkpoint.ModItem> modListToEdit) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(1.015f, 0.934f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_worldLocalMods = new HashSet<string>();
            this.m_worldWorkshopMods = new HashSet<ulong>();
            this.m_categoryButtonList = new List<MyGuiControlButton>();
            this.m_tmpSearch = new List<string>();
            this.m_selectedCategories = new List<string>();
            this.m_modsToolTips = new Dictionary<ulong, StringBuilder>();
            this.m_modListToEdit = modListToEdit;
            if (this.m_modListToEdit == null)
            {
                this.m_modListToEdit = new List<MyObjectBuilder_Checkpoint.ModItem>();
            }
            base.EnabledBackgroundFade = true;
            this.RefreshWorldMods(this.m_modListToEdit);
            this.m_listNeedsReload = true;
            this.RecreateControls(true);
        }

        private void AddHeaders()
        {
            this.m_modsTableEnabled.SetColumnName(1, new StringBuilder(MyTexts.GetString(MyCommonTexts.ScreenMods_ActiveMods) + "     "));
            this.m_modsTableEnabled.SetHeaderColumnAlign(1, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_modsTableDisabled.SetColumnName(1, new StringBuilder(MyTexts.GetString(MyCommonTexts.ScreenMods_AvailableMods) + "     "));
            this.m_modsTableDisabled.SetHeaderColumnAlign(1, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
        }

        private MyGuiControlTable.Row AddMod(bool active, StringBuilder title, StringBuilder toolTip, StringBuilder modState, MyGuiHighlightTexture? icon, MyObjectBuilder_Checkpoint.ModItem mod, Color? textColor = new Color?())
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(mod);
            Color? nullable = null;
            row.AddCell(new MyGuiControlTable.Cell(string.Empty, null, modState.ToString(), nullable, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            MyGuiHighlightTexture? nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(title, null, toolTip.ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            if (active)
            {
                this.m_modsTableEnabled.Insert(0, row);
            }
            else
            {
                this.m_modsTableDisabled.Add(row);
            }
            if (mod.PublishedFileId != 0)
            {
                this.m_modsToolTips[mod.PublishedFileId] = toolTip;
            }
            return row;
        }

        private IMyAsyncResult beginAction() => 
            new MyModsLoadListResult(this.m_worldWorkshopMods);

        private bool CheckSearch(string name)
        {
            bool flag = true;
            string str = name.ToLower();
            foreach (string str2 in this.m_tmpSearch)
            {
                if (!str.Contains(str2.ToLower()))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        public override bool Draw() => 
            (!this.m_listNeedsReload ? base.Draw() : false);

        private void endAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            this.m_listNeedsReload = false;
            MyModsLoadListResult result2 = result as MyModsLoadListResult;
            if (result2 != null)
            {
                this.m_subscribedMods = result2.SubscribedMods;
                this.m_worldMods = result2.SetMods;
                this.RefreshModList();
                screen.CloseScreen();
            }
        }

        private void FillList()
        {
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginAction), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endAction), null));
        }

        private void FillLocalMods()
        {
            if (!Directory.Exists(MyFileSystem.ModsPath))
            {
                Directory.CreateDirectory(MyFileSystem.ModsPath);
            }
            foreach (string str in Directory.GetDirectories(MyFileSystem.ModsPath, "*", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(str);
                if ((!this.m_worldLocalMods.Contains(fileName) && (Directory.GetFileSystemEntries(str).Length != 0)) && (!MyFakes.ENABLE_MOD_CATEGORIES || this.CheckSearch(fileName)))
                {
                    StringBuilder title = new StringBuilder(fileName);
                    MyWorkshop.GetWorkshopIdFromLocalMod(str);
                    Color? textColor = null;
                    this.AddMod(false, title, new StringBuilder(str), MyTexts.Get(MyCommonTexts.ScreenMods_LocalMod), new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_MODS_LOCAL), new MyObjectBuilder_Checkpoint.ModItem(fileName, 0L), textColor);
                }
            }
        }

        private void FillSubscribedMods()
        {
            foreach (MyWorkshopItem item in this.m_subscribedMods)
            {
                if (item == null)
                {
                    continue;
                }
                if (!this.m_worldWorkshopMods.Contains(item.Id))
                {
                    if (MyFakes.ENABLE_MOD_CATEGORIES)
                    {
                        bool flag = false;
                        foreach (string str in item.Tags)
                        {
                            if (this.m_selectedCategories.Contains(str.ToLower()) || (this.m_selectedCategories.Count == 0))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!this.CheckSearch(item.Title) || !flag)
                        {
                            continue;
                        }
                    }
                    StringBuilder title = new StringBuilder(item.Title);
                    int num = Math.Min(item.Description.Length, 0x80);
                    int index = item.Description.IndexOf("\n");
                    if (index > 0)
                    {
                        num = Math.Min(num, index - 1);
                    }
                    StringBuilder toolTip = new StringBuilder();
                    toolTip.Append(item.Description.Substring(0, num));
                    MyObjectBuilder_Checkpoint.ModItem mod = new MyObjectBuilder_Checkpoint.ModItem(null, item.Id, item.Title);
                    Color? textColor = null;
                    this.AddMod(false, title, toolTip, MyTexts.Get(MyCommonTexts.ScreenMods_WorkshopMod), new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP), mod, textColor);
                }
            }
        }

        private void GetActiveMods(List<MyObjectBuilder_Checkpoint.ModItem> outputList)
        {
            for (int i = this.m_modsTableEnabled.RowsCount - 1; i >= 0; i--)
            {
                outputList.Add((MyObjectBuilder_Checkpoint.ModItem) this.m_modsTableEnabled.GetRow(i).UserData);
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMods";

        private void GetModDependenciesAsync(MyGuiControlTable.Row parentRow, MyObjectBuilder_Checkpoint.ModItem selectedMod)
        {
            ModDependenciesWorkData data1 = new ModDependenciesWorkData();
            data1.ParentId = selectedMod.PublishedFileId;
            data1.ParentModRow = parentRow;
            ModDependenciesWorkData workData = data1;
            Parallel.Start(delegate (WorkData workData) {
                bool flag;
                ModDependenciesWorkData data = workData as ModDependenciesWorkData;
                HashSet<ulong> publishedFileIds = new HashSet<ulong>();
                publishedFileIds.Add(data.ParentId);
                data.Dependencies = MyWorkshop.GetModsDependencyHiearchy(publishedFileIds, out flag);
                data.HasReferenceIssue = flag;
            }, new Action<WorkData>(this.OnGetModDependencyHiearchyCompleted), workData);
        }

        private MyWorkshopItem GetSubscribedItem(ulong publishedFileId)
        {
            List<MyWorkshopItem>.Enumerator enumerator;
            using (enumerator = this.m_subscribedMods.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyWorkshopItem current = enumerator.Current;
                    if (current.Id == publishedFileId)
                    {
                        return current;
                    }
                }
            }
            using (enumerator = this.m_worldMods.GetEnumerator())
            {
                MyWorkshopItem item2;
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        MyWorkshopItem current = enumerator.Current;
                        if (current.Id != publishedFileId)
                        {
                            continue;
                        }
                        item2 = current;
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return item2;
            }
        TR_0000:
            return null;
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, string toolTip, Action<MyGuiControlButton> onClick, MyGuiDrawAlignEnum originAlign = 0)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            string str = toolTip;
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, originAlign, str, MyTexts.Get(text), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, MyStringId toolTip, Action<MyGuiControlButton> onClick, MyGuiDrawAlignEnum originAlign = 0)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            string str = MyTexts.GetString(toolTip);
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, originAlign, str, MyTexts.Get(text), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private MyGuiControlButton MakeButtonCategory(Vector2 position, MyWorkshop.Category category)
        {
            string str = category.Id.Replace(" ", "");
            MyGuiHighlightTexture icon = new MyGuiHighlightTexture {
                Normal = $"Textures\GUI\Icons\buttons\small_variant\{str}.dds",
                Highlight = $"Textures\GUI\Icons\buttons\small_variant\{str}Highlight.dds",
                SizePx = new Vector2(48f, 48f)
            };
            Vector2? size = null;
            MyGuiControlButton item = this.MakeButtonCategoryTiny(position, 0f, category.LocalizableName, icon, new Action<MyGuiControlButton>(this.OnCategoryButtonClick), size);
            item.UserData = category.Id;
            item.HighlightType = MyGuiControlHighlightType.FORCED;
            this.m_categoryButtonList.Add(item);
            item.Size = new Vector2(0.005f, 0.005f);
            return item;
        }

        private MyGuiControlButton MakeButtonCategoryTiny(Vector2 position, float rotation, MyStringId toolTip, MyGuiHighlightTexture icon, Action<MyGuiControlButton> onClick, Vector2? size = new Vector2?())
        {
            Action<MyGuiControlButton> onButtonClick = onClick;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            icon.SizePx = new Vector2(48f, 48f);
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Square48, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(toolTip), null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onButtonClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Icon = new MyGuiHighlightTexture?(icon);
            button1.IconRotation = rotation;
            button1.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            return button1;
        }

        private MyGuiControlButton MakeButtonTiny(Vector2 position, float rotation, MyStringId toolTip, MyGuiHighlightTexture icon, Action<MyGuiControlButton> onClick, Vector2? size = new Vector2?())
        {
            Action<MyGuiControlButton> onButtonClick = onClick;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            icon.SizePx = new Vector2(64f, 64f);
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Square, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(toolTip), null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onButtonClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Icon = new MyGuiHighlightTexture?(icon);
            button1.IconRotation = rotation;
            button1.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            return button1;
        }

        private MyGuiControlLabel MakeLabel(Vector2 position, string text)
        {
            Vector2? size = null;
            return new MyGuiControlLabel(new Vector2?(position), size, MyTexts.GetString(text), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM);
        }

        private void MoveSelectedItem(MyGuiControlTable from, MyGuiControlTable to)
        {
            to.Add(from.SelectedRow);
            from.RemoveSelectedRow();
            this.m_selectedRow = from.SelectedRow;
        }

        private void OnBrowseWorkshopClick(MyGuiControlButton obj)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_MODS, "Steam Workshop", false);
        }

        private void OnCancelClick(MyGuiControlButton obj)
        {
            this.CloseScreen();
        }

        private void OnCategoryButtonClick(MyGuiControlButton sender)
        {
            if ((sender.UserData != null) && (sender.UserData is string))
            {
                string userData = (string) sender.UserData;
                if (this.m_selectedCategories.Contains(userData))
                {
                    this.m_selectedCategories.Remove(userData);
                    sender.Selected = false;
                }
                else
                {
                    this.m_selectedCategories.Add(userData);
                    sender.Selected = true;
                }
                this.RefreshModList();
            }
        }

        private void OnGetModDependencyHiearchyCompleted(WorkData workData)
        {
            if (base.State == MyGuiScreenState.OPENED)
            {
                ModDependenciesWorkData data = workData as ModDependenciesWorkData;
                if (((data != null) && (data.Dependencies != null)) && (data.Dependencies.Count > 1))
                {
                    data.Dependencies.RemoveAt(data.Dependencies.Count - 1);
                    MyGuiControlTable.Row parentModRow = data.ParentModRow;
                    if (parentModRow != null)
                    {
                        MyGuiControlTable.Cell cell = parentModRow.GetCell(1);
                        string toolTip = MyTexts.GetString(MyCommonTexts.ScreenMods_ModDependencies);
                        cell.ToolTip.ToolTips.Clear();
                        StringBuilder builder = null;
                        if (this.m_modsToolTips.TryGetValue(data.ParentId, out builder))
                        {
                            cell.ToolTip.AddToolTip(builder.ToString(), 0.7f, "Blue");
                        }
                        cell.ToolTip.AddToolTip(toolTip, 0.7f, "Blue");
                        foreach (MyWorkshopItem item in data.Dependencies)
                        {
                            cell.ToolTip.AddToolTip(item.Title, 0.7f, "Blue");
                        }
                    }
                }
            }
        }

        private void OnMoveBottomClick(MyGuiControlButton sender)
        {
            this.m_selectedTable.MoveSelectedRowBottom();
        }

        private void OnMoveDownClick(MyGuiControlButton sender)
        {
            this.m_selectedTable.MoveSelectedRowDown();
        }

        private void OnMoveLeftAllClick(MyGuiControlButton sender)
        {
            while (this.m_modsTableEnabled.RowsCount > 0)
            {
                this.m_modsTableEnabled.SelectedRowIndex = 0;
                this.MoveSelectedItem(this.m_modsTableEnabled, this.m_modsTableDisabled);
            }
        }

        private void OnMoveLeftClick(MyGuiControlButton sender)
        {
            this.MoveSelectedItem(this.m_modsTableEnabled, this.m_modsTableDisabled);
        }

        private void OnMoveRightAllClick(MyGuiControlButton sender)
        {
            while (this.m_modsTableDisabled.RowsCount > 0)
            {
                this.m_modsTableDisabled.SelectedRowIndex = 0;
                this.MoveSelectedItem(this.m_modsTableDisabled, this.m_modsTableEnabled);
            }
        }

        private void OnMoveRightClick(MyGuiControlButton sender)
        {
            this.MoveSelectedItem(this.m_modsTableDisabled, this.m_modsTableEnabled);
        }

        private void OnMoveTopClick(MyGuiControlButton sender)
        {
            this.m_selectedTable.MoveSelectedRowTop();
        }

        private void OnMoveUpClick(MyGuiControlButton sender)
        {
            this.m_selectedTable.MoveSelectedRowUp();
        }

        private void OnOkClick(MyGuiControlButton obj)
        {
            this.m_modListToEdit.Clear();
            this.GetActiveMods(this.m_modListToEdit);
            this.CloseScreen();
        }

        private void OnOpenInWorkshopClick(MyGuiControlButton obj)
        {
            if (this.m_selectedRow != null)
            {
                object userData = this.m_selectedRow.UserData;
                if (userData != null)
                {
                    MyGuiSandbox.OpenUrlWithFallback(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, ((MyObjectBuilder_Checkpoint.ModItem) userData).PublishedFileId), "Steam Workshop", false);
                }
            }
        }

        private void OnPublishModClick(MyGuiControlButton sender)
        {
            if ((this.m_selectedRow != null) && (this.m_selectedRow.UserData != null))
            {
                StringBuilder builder2;
                MyStringId messageBoxCaptionDoYouWishToUpdateMod;
                this.m_selectedMod = (MyObjectBuilder_Checkpoint.ModItem) this.m_selectedRow.UserData;
                string localModFolder = Path.Combine(MyFileSystem.ModsPath, this.m_selectedMod.Name);
                this.m_selectedMod.FriendlyName = this.m_selectedRow.GetCell(1).Text.ToString();
                this.m_selectedMod.PublishedFileId = MyWorkshop.GetWorkshopIdFromLocalMod(localModFolder);
                if (this.m_selectedMod.PublishedFileId != 0)
                {
                    builder2 = new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextDoYouWishToUpdateMod), MySession.Platform));
                    messageBoxCaptionDoYouWishToUpdateMod = MyCommonTexts.MessageBoxCaptionDoYouWishToUpdateMod;
                }
                else
                {
                    builder2 = new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextDoYouWishToPublishMod), MySession.Platform, MySession.PlatformLinkAgreement));
                    messageBoxCaptionDoYouWishToUpdateMod = MyCommonTexts.MessageBoxCaptionDoYouWishToPublishMod;
                }
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, builder2, MyTexts.Get(messageBoxCaptionDoYouWishToUpdateMod), okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnPublishModQuestionAnswer), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnPublishModQuestionAnswer(MyGuiScreenMessageBox.ResultEnum val)
        {
            if (val == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                string[] tags = null;
                MyWorkshopItem subscribedItem = this.GetSubscribedItem(this.m_selectedMod.PublishedFileId);
                if (subscribedItem != null)
                {
                    tags = subscribedItem.Tags.ToArray<string>();
                    if (subscribedItem.OwnerId != Sync.MyId)
                    {
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_OwnerMismatchMod), MyTexts.Get(MyCommonTexts.MessageBoxCaptionModPublishFailed), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                        return;
                    }
                }
                MyGuiSandbox.AddScreen(new MyGuiScreenWorkshopTags("mod", MyWorkshop.ModCategories, tags, new Action<MyGuiScreenMessageBox.ResultEnum, string[]>(this.OnPublishWorkshopTagsResult)));
            }
        }

        private void OnPublishWorkshopTagsResult(MyGuiScreenMessageBox.ResultEnum tagsResult, string[] outTags)
        {
            if (tagsResult == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                string modFullPath = Path.Combine(MyFileSystem.ModsPath, this.m_selectedMod.Name);
                MyWorkshop.PublishModAsync(modFullPath, this.m_selectedMod.FriendlyName, null, this.m_selectedMod.PublishedFileId, outTags, MyPublishedFileVisibility.Public, delegate (bool success, MyGameServiceCallResult result, ulong publishedFileId) {
                    MyStringId? nullable;
                    Vector2? nullable2;
                    if (success)
                    {
                        MyWorkshop.GenerateModInfo(modFullPath, publishedFileId, Sync.MyId);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextModPublished), MySession.Platform)), MyTexts.Get(MyCommonTexts.MessageBoxCaptionModPublished), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum a) {
                            MyGameService.OpenOverlayUrl(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, publishedFileId));
                            this.FillList();
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                    else
                    {
                        StringBuilder messageText = (result != MyGameServiceCallResult.AccessDenied) ? new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublishFailed), MySession.Platform) : MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_AccessDenied);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionModPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                });
            }
        }

        private void OnRefreshClick(MyGuiControlButton obj)
        {
            if (!this.m_listNeedsReload)
            {
                this.m_listNeedsReload = true;
                this.FillList();
            }
        }

        private void OnSearchTextChanged(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                this.m_tmpSearch.Clear();
            }
            else
            {
                char[] separator = new char[] { ' ' };
                this.m_tmpSearch = text.Split(separator).ToList<string>();
            }
            this.RefreshModList();
        }

        private void OnSelectCategoryClicked(MyGuiControlButton sender)
        {
            string[] tags = this.m_selectedCategories.ToArray();
            MyGuiSandbox.AddScreen(new MyGuiScreenWorkshopTags("mod", MyWorkshop.ModCategories, tags, delegate (MyGuiScreenMessageBox.ResultEnum tagsResult, string[] outTags) {
                if (tagsResult == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    List<string> list = outTags.ToList<string>();
                    list.Remove("mod");
                    this.m_selectedCategories = list;
                    this.RefreshCategoryButtons();
                    this.RefreshModList();
                }
            }));
        }

        protected override void OnShow()
        {
            base.OnShow();
            if (this.m_listNeedsReload)
            {
                this.FillList();
            }
        }

        private void OnTableItemConfirmedOrDoubleClick(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            if (sender.SelectedRow != null)
            {
                MyGuiControlTable.Row selectedRow = sender.SelectedRow;
                MyObjectBuilder_Checkpoint.ModItem userData = (MyObjectBuilder_Checkpoint.ModItem) selectedRow.UserData;
                MyGuiControlTable to = ReferenceEquals(sender, this.m_modsTableEnabled) ? this.m_modsTableDisabled : this.m_modsTableEnabled;
                this.MoveSelectedItem(sender, to);
                if (ReferenceEquals(to, this.m_modsTableEnabled) && (userData.PublishedFileId != 0))
                {
                    this.GetModDependenciesAsync(selectedRow, userData);
                }
            }
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            int? nullable;
            sender.CanHaveFocus = true;
            base.FocusedControl = sender;
            this.m_selectedRow = sender.SelectedRow;
            this.m_selectedTable = sender;
            if (ReferenceEquals(sender, this.m_modsTableEnabled))
            {
                nullable = null;
                this.m_modsTableDisabled.SelectedRowIndex = nullable;
            }
            if (ReferenceEquals(sender, this.m_modsTableDisabled))
            {
                nullable = null;
                this.m_modsTableEnabled.SelectedRowIndex = nullable;
            }
            if (MyInput.Static.IsAnyCtrlKeyPressed())
            {
                this.OnTableItemConfirmedOrDoubleClick(sender, eventArgs);
            }
        }

        public override void RecreateControls(bool constructor)
        {
            MyGuiControlButton button;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionWorkshop, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.895f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.895f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.895f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.895f, 0f, captionTextColor);
            this.Controls.Add(list2);
            Vector2 vector = new Vector2(-0.454f, -0.417f);
            Vector2 vector2 = new Vector2(0f, -4.75f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA.Y);
            this.m_modsTableDisabled = new MyGuiControlTable();
            this.m_modsTableEnabled = new MyGuiControlTable();
            if (MyFakes.ENABLE_MOD_CATEGORIES)
            {
                this.m_modsTableDisabled.Position = vector + new Vector2(0f, 0.1f);
                this.m_modsTableDisabled.VisibleRowsCount = 0x12;
                this.m_modsTableEnabled.VisibleRowsCount = 0x12;
            }
            else
            {
                this.m_modsTableDisabled.Position = vector;
                this.m_modsTableDisabled.VisibleRowsCount = 20;
                this.m_modsTableEnabled.VisibleRowsCount = 20;
            }
            this.m_modsTableDisabled.Size = new Vector2(base.m_size.Value.X * 0.428f, 1f);
            this.m_modsTableDisabled.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_modsTableDisabled.ColumnsCount = 2;
            this.m_modsTableDisabled.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_modsTableDisabled.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            this.m_modsTableDisabled.ItemConfirmed += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            float[] p = new float[] { 0.085f, 0.905f };
            this.m_modsTableDisabled.SetCustomColumnWidths(p);
            this.m_modsTableDisabled.SetColumnComparison(1, (a, b) => a.Text.CompareToIgnoreCase(b.Text));
            this.Controls.Add(this.m_modsTableDisabled);
            this.m_modsTableEnabled.Position = vector + new Vector2(this.m_modsTableDisabled.Size.X + 0.04f, 0.1f);
            this.m_modsTableEnabled.Size = new Vector2(base.m_size.Value.X * 0.428f, 1f);
            this.m_modsTableEnabled.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_modsTableEnabled.ColumnsCount = 2;
            this.m_modsTableEnabled.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_modsTableEnabled.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            this.m_modsTableEnabled.ItemConfirmed += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            float[] singleArray2 = new float[] { 0.085f, 0.905f };
            this.m_modsTableEnabled.SetCustomColumnWidths(singleArray2);
            this.m_modsTableEnabled.SetColumnComparison(1, (a, b) => a.Text.CompareToIgnoreCase(b.Text));
            this.Controls.Add(this.m_modsTableEnabled);
            Vector2? size = null;
            this.m_moveRightAllButton = button = this.MakeButtonTiny(vector2 + (0f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 0f, MyCommonTexts.ToolTipScreenMods_MoveRightAll, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, new Action<MyGuiControlButton>(this.OnMoveRightAllClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveRightButton = button = this.MakeButtonTiny(vector2 + (1f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 0f, MyCommonTexts.ToolTipScreenMods_MoveRight, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, new Action<MyGuiControlButton>(this.OnMoveRightClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveUpButton = button = this.MakeButtonTiny(vector2 + (2.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), -1.570796f, MyCommonTexts.ToolTipScreenMods_MoveUp, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, new Action<MyGuiControlButton>(this.OnMoveUpClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveTopButton = button = this.MakeButtonTiny(vector2 + (3.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), -1.570796f, MyCommonTexts.ToolTipScreenMods_MoveTop, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, new Action<MyGuiControlButton>(this.OnMoveTopClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveBottomButton = button = this.MakeButtonTiny(vector2 + (4.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 1.570796f, MyCommonTexts.ToolTipScreenMods_MoveBottom, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, new Action<MyGuiControlButton>(this.OnMoveBottomClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveDownButton = button = this.MakeButtonTiny(vector2 + (5.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 1.570796f, MyCommonTexts.ToolTipScreenMods_MoveDown, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, new Action<MyGuiControlButton>(this.OnMoveDownClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveLeftButton = button = this.MakeButtonTiny(vector2 + (7f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 3.141593f, MyCommonTexts.ToolTipScreenMods_MoveLeft, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, new Action<MyGuiControlButton>(this.OnMoveLeftClick), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveLeftAllButton = button = this.MakeButtonTiny(vector2 + (8f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 3.141593f, MyCommonTexts.ToolTipScreenMods_MoveLeftAll, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, new Action<MyGuiControlButton>(this.OnMoveLeftAllClick), size);
            this.Controls.Add(button);
            float num = 0.0075f;
            this.m_okButton = button = this.MakeButton(new Vector2(this.m_modsTableDisabled.Position.X + 0.002f, 0f) - new Vector2(0f, (-base.m_size.Value.Y / 2f) + 0.097f), MyCommonTexts.Ok, MyCommonTexts.Ok, new Action<MyGuiControlButton>(this.OnOkClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(button);
            this.m_refreshButton = button = this.MakeButton(this.m_okButton.Position + new Vector2(this.m_okButton.Size.X + num, 0f), MyCommonTexts.ScreenLoadSubscribedWorldRefresh, MyCommonTexts.ToolTipWorkshopRefreshMod, new Action<MyGuiControlButton>(this.OnRefreshClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(button);
            this.m_browseWorkshopButton = button = this.MakeButton(this.m_okButton.Position + new Vector2((this.m_okButton.Size.X * 2f) + (num * 2f), 0f), MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop, string.Format(MyTexts.GetString(MyCommonTexts.ToolTipWorkshopBrowseWorkshop), MySession.Platform), new Action<MyGuiControlButton>(this.OnBrowseWorkshopClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(button);
            this.m_openInWorkshopButton = button = this.MakeButton(this.m_okButton.Position + new Vector2((this.m_okButton.Size.X * 3f) + (num * 3f), 0f), MyCommonTexts.ScreenLoadSubscribedWorldOpenInWorkshop, MyCommonTexts.ToolTipWorkshopPublish, new Action<MyGuiControlButton>(this.OnOpenInWorkshopClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(button);
            this.m_publishModButton = button = this.MakeButton(this.m_okButton.Position + new Vector2((this.m_okButton.Size.X * 4f) + (num * 4f), 0f), MyCommonTexts.LoadScreenButtonPublish, MyCommonTexts.LoadScreenButtonPublish, new Action<MyGuiControlButton>(this.OnPublishModClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(button);
            this.m_okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipMods_Ok));
            if (MyFakes.ENABLE_MOD_CATEGORIES)
            {
                Vector2 vector3 = this.m_modsTableDisabled.Position + new Vector2(0.015f, -0.036f);
                Vector2 vector4 = new Vector2(0.0335f, 0f);
                MyWorkshop.Category[] modCategories = MyWorkshop.ModCategories;
                int index = 0;
                while (true)
                {
                    if (index >= modCategories.Length)
                    {
                        MyGuiControlButton button1 = new MyGuiControlButton();
                        button1.Position = (vector3 + (vector4 * index)) + new Vector2(-0.013f, -0.02f);
                        button1.Size = new Vector2(0.03f, 0.05f);
                        button1.Name = "SelectCategory";
                        button1.Text = "...";
                        button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                        button1.VisualStyle = MyGuiControlButtonStyleEnum.Square48;
                        MyGuiControlButton button2 = button1;
                        button2.SetToolTip(MyCommonTexts.TooltipScreenMods_SelectCategories);
                        button2.ButtonClicked += new Action<MyGuiControlButton>(this.OnSelectCategoryClicked);
                        this.Controls.Add(button2);
                        Vector2 vector5 = new Vector2(this.m_modsTableEnabled.Position.X, 0f) - new Vector2(0f, (base.m_size.Value.Y / 2f) - 0.099f);
                        size = null;
                        this.m_searchBox = new MyGuiControlSearchBox(new Vector2?(vector5 + new Vector2(0f, 0.013f)), size, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                        this.m_searchBox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipMods_Search));
                        this.m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                        this.m_searchBox.Size = new Vector2(this.m_modsTableEnabled.Size.X, 0.2f);
                        this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.OnSearchTextChanged);
                        Vector2 vector6 = new Vector2(0f, 0.05f);
                        this.m_moveUpButton.Position += vector6;
                        this.m_moveTopButton.Position += vector6;
                        this.m_moveBottomButton.Position += vector6;
                        this.m_moveDownButton.Position += vector6;
                        this.m_moveLeftButton.Position += vector6;
                        this.m_moveLeftAllButton.Position += vector6;
                        this.m_moveRightAllButton.Position += vector6;
                        this.m_moveRightButton.Position += vector6;
                        this.Controls.Add(this.m_searchBox);
                        break;
                    }
                    if (modCategories[index].IsVisibleForFilter)
                    {
                        this.Controls.Add(this.MakeButtonCategory(vector3 + (vector4 * index), modCategories[index]));
                    }
                    index++;
                }
            }
            base.CloseButtonEnabled = true;
            if ((((float) MySandboxGame.ScreenSize.X) / ((float) MySandboxGame.ScreenSize.Y)) == 1.25f)
            {
                base.SetCloseButtonOffset_5_to_4();
            }
            else
            {
                base.SetDefaultCloseButtonOffset();
            }
        }

        private void RefreshCategoryButtons()
        {
            foreach (MyGuiControlButton button in this.m_categoryButtonList)
            {
                if (button.UserData != null)
                {
                    string item = (button.UserData as string).ToLower();
                    button.Selected = this.m_selectedCategories.Contains(item);
                }
            }
        }

        private void RefreshModList()
        {
            this.m_selectedRow = null;
            this.m_selectedTable = null;
            if (this.m_modsTableEnabled != null)
            {
                ListReader<MyObjectBuilder_Checkpoint.ModItem> modListToEdit;
                if (!this.m_keepActiveMods)
                {
                    modListToEdit = this.m_modListToEdit;
                }
                else
                {
                    List<MyObjectBuilder_Checkpoint.ModItem> outputList = new List<MyObjectBuilder_Checkpoint.ModItem>(this.m_modsTableEnabled.RowsCount);
                    this.GetActiveMods(outputList);
                    modListToEdit = outputList;
                }
                this.m_keepActiveMods = true;
                this.RefreshWorldMods(modListToEdit);
                this.m_modsTableEnabled.Clear();
                this.m_modsTableDisabled.Clear();
                this.m_modsToolTips.Clear();
                this.AddHeaders();
                foreach (MyObjectBuilder_Checkpoint.ModItem item in modListToEdit)
                {
                    if (!item.IsDependency)
                    {
                        if (item.PublishedFileId == 0)
                        {
                            StringBuilder builder = new StringBuilder(item.Name);
                            string str = Path.Combine(MyFileSystem.ModsPath, item.Name);
                            StringBuilder builder2 = new StringBuilder(str);
                            StringBuilder builder3 = MyTexts.Get(MyCommonTexts.ScreenMods_LocalMod);
                            Color? nullable = null;
                            MyGuiHighlightTexture texture = MyGuiConstants.TEXTURE_ICON_MODS_LOCAL;
                            if (!Directory.Exists(str) && !File.Exists(str))
                            {
                                builder2 = MyTexts.Get(MyCommonTexts.ScreenMods_MissingLocalMod);
                                builder3 = builder2;
                                nullable = new Color?(MyHudConstants.MARKER_COLOR_RED);
                            }
                            this.AddMod(true, builder, builder2, builder3, new MyGuiHighlightTexture?(texture), item, nullable);
                            continue;
                        }
                        StringBuilder title = new StringBuilder();
                        StringBuilder toolTip = new StringBuilder();
                        StringBuilder modState = MyTexts.Get(MyCommonTexts.ScreenMods_WorkshopMod);
                        Color? textColor = null;
                        MyWorkshopItem subscribedItem = this.GetSubscribedItem(item.PublishedFileId);
                        if (subscribedItem == null)
                        {
                            title.Append(item.PublishedFileId.ToString());
                            toolTip = new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.ScreenMods_MissingDetails), MySession.Platform);
                            textColor = new Color?(MyHudConstants.MARKER_COLOR_RED);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(subscribedItem.Title))
                            {
                                title.Append(subscribedItem.Title);
                            }
                            else
                            {
                                title.Append(string.Format(MyTexts.GetString(MyCommonTexts.ModNotReceived), item.PublishedFileId));
                            }
                            if (string.IsNullOrEmpty(subscribedItem.Description))
                            {
                                toolTip.Append(MyTexts.GetString(MyCommonTexts.ModNotReceived_ToolTip));
                            }
                            else
                            {
                                int num = Math.Min(subscribedItem.Description.Length, 0x80);
                                int index = subscribedItem.Description.IndexOf("\n");
                                if (index > 0)
                                {
                                    num = Math.Min(num, index - 1);
                                }
                                toolTip.Append(subscribedItem.Description.Substring(0, num));
                            }
                        }
                        this.AddMod(true, title, toolTip, modState, new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP), item, textColor);
                    }
                }
                this.FillLocalMods();
                if (this.m_subscribedMods != null)
                {
                    this.FillSubscribedMods();
                }
            }
        }

        private void RefreshWorldMods(ListReader<MyObjectBuilder_Checkpoint.ModItem> mods)
        {
            this.m_worldLocalMods.Clear();
            this.m_worldWorkshopMods.Clear();
            foreach (MyObjectBuilder_Checkpoint.ModItem item in mods)
            {
                if (item.PublishedFileId == 0)
                {
                    this.m_worldLocalMods.Add(item.Name);
                    continue;
                }
                this.m_worldWorkshopMods.Add(item.PublishedFileId);
            }
        }

        public override bool RegisterClicks() => 
            true;

        public override bool Update(bool hasFocus)
        {
            int num1;
            int num2;
            int num3;
            int num4;
            bool flag = this.m_selectedRow != null;
            bool local1 = flag && (this.m_selectedRow.UserData != null);
            bool flag2 = local1 && (((MyObjectBuilder_Checkpoint.ModItem) this.m_selectedRow.UserData).PublishedFileId == 0L);
            bool local2 = local1;
            bool flag3 = local2 && (((MyObjectBuilder_Checkpoint.ModItem) this.m_selectedRow.UserData).PublishedFileId != 0L);
            bool flag4 = local2 && ((MyObjectBuilder_Checkpoint.ModItem) this.m_selectedRow.UserData).IsDependency;
            this.m_openInWorkshopButton.Enabled = flag & flag3;
            this.m_publishModButton.Enabled = (flag & flag2) && MyFakes.ENABLE_WORKSHOP_PUBLISH;
            if ((!flag || (this.m_selectedTable.SelectedRowIndex == null)) || (this.m_selectedTable.SelectedRowIndex.Value <= 0))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) !flag4;
            }
            this.m_moveTopButton.Enabled = this.m_moveUpButton.Enabled = (bool) num1;
            if ((!flag || (this.m_selectedTable.SelectedRowIndex == null)) || (this.m_selectedTable.SelectedRowIndex.Value >= (this.m_selectedTable.RowsCount - 1)))
            {
                num2 = 0;
            }
            else
            {
                num2 = (int) !flag4;
            }
            this.m_moveDownButton.Enabled = this.m_moveBottomButton.Enabled = (bool) num2;
            if (!flag || !ReferenceEquals(this.m_selectedTable, this.m_modsTableEnabled))
            {
                num3 = 0;
            }
            else
            {
                num3 = (int) !flag4;
            }
            this.m_moveLeftButton.Enabled = (bool) num3;
            if (!flag || !ReferenceEquals(this.m_selectedTable, this.m_modsTableDisabled))
            {
                num4 = 0;
            }
            else
            {
                num4 = (int) !flag4;
            }
            this.m_moveRightButton.Enabled = (bool) num4;
            this.m_moveLeftAllButton.Enabled = (this.m_modsTableEnabled.RowsCount > 0) && !flag4;
            this.m_moveRightAllButton.Enabled = (this.m_modsTableDisabled.RowsCount > 0) && !flag4;
            if (MySession.Static == null)
            {
                Parallel.RunCallbacks();
            }
            return base.Update(hasFocus);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenMods.<>c <>9 = new MyGuiScreenMods.<>c();
            public static Comparison<MyGuiControlTable.Cell> <>9__35_0;
            public static Comparison<MyGuiControlTable.Cell> <>9__35_1;
            public static Action<WorkData> <>9__48_0;

            internal void <GetModDependenciesAsync>b__48_0(WorkData workData)
            {
                bool flag;
                MyGuiScreenMods.ModDependenciesWorkData data = workData as MyGuiScreenMods.ModDependenciesWorkData;
                HashSet<ulong> publishedFileIds = new HashSet<ulong>();
                publishedFileIds.Add(data.ParentId);
                data.Dependencies = MyWorkshop.GetModsDependencyHiearchy(publishedFileIds, out flag);
                data.HasReferenceIssue = flag;
            }

            internal int <RecreateControls>b__35_0(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                a.Text.CompareToIgnoreCase(b.Text);

            internal int <RecreateControls>b__35_1(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                a.Text.CompareToIgnoreCase(b.Text);
        }

        private class ModDependenciesWorkData : WorkData
        {
            public ulong ParentId;
            public MyGuiControlTable.Row ParentModRow;
            public List<MyWorkshopItem> Dependencies;
            public bool HasReferenceIssue;
        }
    }
}

