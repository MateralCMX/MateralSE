namespace Sandbox.Game.Gui
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Linq;
    using VRage;
    using VRage.Collections;
    using VRage.Compression;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [StaticEventOwner]
    public class MyGuiBlueprintScreen_Reworked : MyGuiScreenDebugBase
    {
        public static readonly float MAGIC_SPACING_BIG = 0.00535f;
        public static readonly float MAGIC_SPACING_SMALL = 0.00888f;
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.878f, 0.97f);
        private static HashSet<ulong> m_downloadQueued = new HashSet<ulong>();
        private static MyConcurrentHashSet<ulong> m_downloadFinished = new MyConcurrentHashSet<ulong>();
        private static MyWaitForScreenshotData m_waitingForScreenshot = new MyWaitForScreenshotData();
        private static bool m_downloadFromSteam = true;
        private static bool m_needsExtract = false;
        private static bool m_showDlcIcons = false;
        private static List<MyBlueprintItemInfo> m_recievedBlueprints = new List<MyBlueprintItemInfo>();
        private readonly List<MyGuiControlImage> m_dlcIcons;
        private static Sandbox.Game.Gui.LoadPrefabData m_LoadPrefabData;
        private static Dictionary<Content, List<MyWorkshopItem>> m_subscribedItemsListDict = new Dictionary<Content, List<MyWorkshopItem>>();
        private static Dictionary<Content, string> m_currentLocalDirectoryDict = new Dictionary<Content, string>();
        private static Dictionary<Content, SortOption> m_selectedSortDict = new Dictionary<Content, SortOption>();
        private static Dictionary<Content, MyBlueprintTypeEnum> m_selectedBlueprintTypeDict = new Dictionary<Content, MyBlueprintTypeEnum>();
        private static Dictionary<Content, bool> m_thumbnailsVisibleDict = new Dictionary<Content, bool>();
        public static ParallelTasks.Task Task;
        public static readonly FastResourceLock SubscribedItemsLock = new FastResourceLock();
        private Tab m_selectedTab;
        private float m_guiMultilineHeight;
        private float m_guiAdditionalInfoOffset;
        private MyGridClipboard m_clipboard;
        private MyBlueprintAccessType m_accessType;
        private bool m_allowCopyToClipboard;
        private MyObjectBuilder_Definitions m_loadedPrefab;
        private ulong? m_publishedItemId;
        private bool m_blueprintBeingLoaded;
        private Action<string> m_onScriptOpened;
        private Func<string> m_getCodeFromEditor;
        private Action m_onCloseAction;
        private MyBlueprintItemInfo m_selectedBlueprint;
        private MyGuiControlContentButton m_selectedButton;
        private MyGuiControlRadioButtonGroup m_BPTypesGroup;
        private MyGuiControlList m_BPList;
        private List<string> m_preloadedTextures;
        private Content m_content;
        private bool m_wasPublished;
        private MyGuiControlSeparatorList m_separator;
        private MyGuiControlSearchBox m_searchBox;
        private MyGuiControlMultilineText m_multiline;
        private MyGuiControlPanel m_detailsBackground;
        private MyGuiControlLabel m_detailName;
        private MyGuiControlLabel m_detailBlockCount;
        private MyGuiControlLabel m_detailBlockCountValue;
        private MyGuiControlLabel m_detailSizeValue;
        private MyGuiControlLabel m_detailAuthor;
        private MyGuiControlLabel m_detailAuthorName;
        private MyGuiControlLabel m_detailDLC;
        private MyGuiControlLabel m_detailSize;
        private MyGuiControlLabel m_detailSendTo;
        private MyGuiControlButton m_button_Refresh;
        private MyGuiControlButton m_button_GroupSelection;
        private MyGuiControlButton m_button_Sorting;
        private MyGuiControlButton m_button_OpenWorkshop;
        private MyGuiControlButton m_button_NewBlueprint;
        private MyGuiControlButton m_button_DirectorySelection;
        private MyGuiControlButton m_button_HideThumbnails;
        private MyGuiControlButton m_button_TabInfo;
        private MyGuiControlButton m_button_TabEdit;
        private MyGuiControlButton m_button_OpenInWorkshop;
        private MyGuiControlButton m_button_CopyToClipboard;
        private MyGuiControlButton m_button_Rename;
        private MyGuiControlButton m_button_Replace;
        private MyGuiControlButton m_button_Delete;
        private MyGuiControlButton m_button_TakeScreenshot;
        private MyGuiControlButton m_button_Publish;
        private MyGuiControlButton m_button_Close;
        private MyGuiControlCombobox m_sendToCombo;
        private MyGuiControlImage m_icon_Refresh;
        private MyGuiControlImage m_icon_GroupSelection;
        private MyGuiControlImage m_icon_Sorting;
        private MyGuiControlImage m_icon_OpenWorkshop;
        private MyGuiControlImage m_icon_DirectorySelection;
        private MyGuiControlImage m_icon_NewBlueprint;
        private MyGuiControlImage m_icon_HideThumbnails;
        private MyGuiControlImage m_thumbnailImage;

        private MyGuiBlueprintScreen_Reworked() : base(new Vector2(0.5f, 0.5f), new Vector2?(SCREEN_SIZE), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), false)
        {
            this.m_dlcIcons = new List<MyGuiControlImage>();
            this.m_preloadedTextures = new List<string>();
            base.CanHideOthers = true;
            base.m_canShareInput = false;
            base.CanBeHidden = true;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            this.InitializeBPList();
            this.m_BPList.Clear();
            this.m_BPTypesGroup.Clear();
        }

        private void AddBlueprintButton(MyBlueprintItemInfo data, bool isLocalMod = false, bool isWorkshopMod = false, bool filter = false)
        {
            string blueprintName = data.BlueprintName;
            string imagePath = this.GetImagePath(data);
            if (File.Exists(imagePath))
            {
                List<string> texturesToLoad = new List<string>();
                texturesToLoad.Add(imagePath);
                MyRenderProxy.PreloadTextures(texturesToLoad, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
            }
            MyGuiControlContentButton button1 = new MyGuiControlContentButton(blueprintName, imagePath);
            button1.UserData = data;
            button1.IsLocalMod = isLocalMod;
            button1.IsWorkshopMod = isWorkshopMod;
            button1.Key = this.m_BPTypesGroup.Count;
            MyGuiControlContentButton control = button1;
            control.MouseOverChanged += new Action<MyGuiControlRadioButton, bool>(this.OnMouseOverItem);
            control.FocusChanged += new Action<MyGuiControlBase, bool>(this.OnFocusedItem);
            control.SetTooltip(blueprintName);
            control.SetPreviewVisibility(this.GetThumbnailVisibility());
            this.m_BPTypesGroup.Add(control);
            this.m_BPList.Controls.Add(control);
            if (filter)
            {
                this.ApplyFiltering(control);
            }
        }

        private void AddBlueprintButtons(ref List<MyBlueprintItemInfo> data, bool isLocalMod = false, bool isWorkshopMod = false, bool filter = false)
        {
            int num2;
            this.m_preloadedTextures.Clear();
            List<string> list = new List<string>();
            int num = 0;
            while (true)
            {
                if (num >= data.Count)
                {
                    num2 = 0;
                    break;
                }
                string imagePath = this.GetImagePath(data[num]);
                list.Add(imagePath);
                if (File.Exists(imagePath))
                {
                    this.m_preloadedTextures.Add(imagePath);
                    MyRenderProxy.PreloadTextures(this.m_preloadedTextures, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
                }
                num++;
            }
            while (true)
            {
                string name;
                MyGuiControlContentButton button;
                while (true)
                {
                    if (num2 >= data.Count)
                    {
                        return;
                    }
                    name = data[num2].Data.Name;
                    MyGuiControlContentButton button1 = new MyGuiControlContentButton(name, File.Exists(list[num2]) ? list[num2] : "");
                    button1.UserData = data[num2];
                    button1.IsLocalMod = isLocalMod;
                    button1.IsWorkshopMod = isWorkshopMod;
                    button1.Key = this.m_BPTypesGroup.Count;
                    button = button1;
                    if (m_showDlcIcons)
                    {
                        if ((data[num2].Item != null) && (data[num2].Item.DLCs.Count > 0))
                        {
                            using (List<uint>.Enumerator enumerator = data[num2].Item.DLCs.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    string dLCIcon = MyDLCs.GetDLCIcon(enumerator.Current);
                                    if (!string.IsNullOrEmpty(dLCIcon))
                                    {
                                        button.AddDlcIcon(dLCIcon);
                                    }
                                }
                                break;
                            }
                        }
                        if ((data[num2].Data.DLCs != null) && (data[num2].Data.DLCs.Length != 0))
                        {
                            uint[] dLCs = data[num2].Data.DLCs;
                            for (int i = 0; i < dLCs.Length; i++)
                            {
                                string dLCIcon = MyDLCs.GetDLCIcon(dLCs[i]);
                                if (!string.IsNullOrEmpty(dLCIcon))
                                {
                                    button.AddDlcIcon(dLCIcon);
                                }
                            }
                        }
                    }
                    break;
                }
                button.MouseOverChanged += new Action<MyGuiControlRadioButton, bool>(this.OnMouseOverItem);
                button.FocusChanged += new Action<MyGuiControlBase, bool>(this.OnFocusedItem);
                button.SetTooltip(name);
                button.SetPreviewVisibility(this.GetThumbnailVisibility());
                this.m_BPTypesGroup.Add(button);
                this.m_BPList.Controls.Add(button);
                if (filter)
                {
                    this.ApplyFiltering(button);
                }
                num2++;
            }
        }

        private void AddWorkshopItemsToList()
        {
            List<MyBlueprintItemInfo> list = new List<MyBlueprintItemInfo>();
            using (SubscribedItemsLock.AcquireSharedUsing())
            {
                foreach (MyWorkshopItem item in GetSubscribedItemsList(Content.Script))
                {
                    MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(item.Id));
                    info1.BlueprintName = item.Title;
                    info1.Item = item;
                    MyBlueprintItemInfo info = info1;
                    info.SetAdditionalBlueprintInformation(item.Title, item.Description, item.DLCs.ToArray<uint>());
                    list.Add(info);
                }
            }
            MySandboxGame.Static.Invoke(delegate {
                this.SortBlueprints(list, MyBlueprintTypeEnum.STEAM);
                this.AddBlueprintButtons(ref list, false, false, false);
            }, "string");
        }

        private void ApplyFiltering()
        {
            bool flag = false;
            string[] strArray = new string[0];
            if (this.m_searchBox != null)
            {
                flag = this.m_searchBox.SearchText != "";
                char[] separator = new char[] { ' ' };
                strArray = this.m_searchBox.SearchText.Split(separator);
            }
            foreach (MyGuiControlBase base2 in this.m_BPList.Controls)
            {
                MyGuiControlContentButton button = base2 as MyGuiControlContentButton;
                if (button != null)
                {
                    bool flag2 = true;
                    MyBlueprintTypeEnum selectedBlueprintType = this.GetSelectedBlueprintType();
                    if (((selectedBlueprintType != MyBlueprintTypeEnum.MIXED) && (selectedBlueprintType != ((MyBlueprintItemInfo) button.UserData).Type)) && ((selectedBlueprintType != MyBlueprintTypeEnum.STEAM) || (((MyBlueprintItemInfo) button.UserData).Type != MyBlueprintTypeEnum.SHARED)))
                    {
                        flag2 = false;
                    }
                    if (flag2 & flag)
                    {
                        string str = button.Title.ToString().ToLower();
                        foreach (string str2 in strArray)
                        {
                            if (!str.Contains(str2.ToLower()))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }
                    base2.Visible = flag2;
                }
            }
            this.m_BPList.SetScrollBarPage(0f);
        }

        private void ApplyFiltering(MyGuiControlContentButton button)
        {
            if (button != null)
            {
                bool flag = (this.m_searchBox != null) && (this.m_searchBox.SearchText != "");
                string[] strArray = new string[0];
                if (flag)
                {
                    char[] separator = new char[] { ' ' };
                    strArray = this.m_searchBox.SearchText.Split(separator);
                }
                bool flag2 = true;
                MyBlueprintTypeEnum selectedBlueprintType = this.GetSelectedBlueprintType();
                if ((selectedBlueprintType != MyBlueprintTypeEnum.MIXED) && (selectedBlueprintType != ((MyBlueprintItemInfo) button.UserData).Type))
                {
                    flag2 = false;
                }
                if (flag2 & flag)
                {
                    string str = button.Title.ToString().ToLower();
                    foreach (string str2 in strArray)
                    {
                        if (!str.Contains(str2.ToLower()))
                        {
                            flag2 = false;
                            break;
                        }
                    }
                }
                if (flag2)
                {
                    button.Visible = true;
                }
                else
                {
                    button.Visible = false;
                }
            }
        }

        private void ChangeName(string name)
        {
            name = MyUtils.StripInvalidChars(name);
            string str = string.Empty;
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                str = MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL;
            }
            else if (content == Content.Script)
            {
                str = MyBlueprintUtils.SCRIPT_FOLDER_LOCAL;
            }
            string str2 = this.m_selectedBlueprint.Data.Name;
            string file = Path.Combine(str, this.GetCurrentLocalDirectory(), str2);
            string newFile = Path.Combine(str, this.GetCurrentLocalDirectory(), name);
            if ((file != newFile) && Directory.Exists(file))
            {
                StringBuilder builder;
                MyStringId? nullable;
                Vector2? nullable2;
                if (!Directory.Exists(newFile))
                {
                    try
                    {
                        content = this.m_content;
                        if (content == Content.Blueprint)
                        {
                            this.UpdatePrefab(this.m_selectedBlueprint, true);
                            this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeId = name;
                            this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeName = name;
                            this.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = name;
                            Directory.Move(file, newFile);
                            MyRenderProxy.UnloadTexture(Path.Combine(newFile, "thumb.png"));
                            MyBlueprintUtils.SavePrefabToFile(this.m_loadedPrefab, name, this.GetCurrentLocalDirectory(), true, MyBlueprintTypeEnum.LOCAL);
                        }
                        else if (content == Content.Script)
                        {
                            if (Directory.Exists(file))
                            {
                                Directory.Move(file, newFile);
                            }
                            if (Directory.Exists(file))
                            {
                                Directory.Delete(file, true);
                            }
                        }
                        this.RefreshBlueprintList(false);
                        this.UpdatePrefab(this.m_selectedBlueprint, false);
                        using (FileStream stream2 = new FileStream(Path.Combine(newFile, "bp.sbc"), FileMode.Open))
                        {
                            this.UpdateInfo(stream2, this.m_selectedBlueprint);
                        }
                    }
                    catch (IOException)
                    {
                        builder = MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_Delete);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_DeleteMessage), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
                else if (file.ToLower() != newFile.ToLower())
                {
                    builder = MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_Replace);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_ReplaceMessage1).Append(name).Append(MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_ReplaceMessage2)), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                        if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            Content content = this.m_content;
                            if (content == Content.Blueprint)
                            {
                                this.DeleteItem(newFile);
                                this.UpdatePrefab(this.m_selectedBlueprint, true);
                                this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeId = name;
                                this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeName = name;
                                this.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = name;
                                Directory.Move(file, newFile);
                                MyRenderProxy.UnloadTexture(Path.Combine(newFile, "thumb.png"));
                                MyBlueprintUtils.SavePrefabToFile(this.m_loadedPrefab, name, this.GetCurrentLocalDirectory(), true, MyBlueprintTypeEnum.LOCAL);
                            }
                            else if (content == Content.Script)
                            {
                                Directory.Delete(newFile, true);
                                if (Directory.Exists(file))
                                {
                                    Directory.Move(file, newFile);
                                }
                                if (Directory.Exists(file))
                                {
                                    Directory.Delete(file, true);
                                }
                            }
                            this.RefreshBlueprintList(false);
                            this.UpdatePrefab(this.m_selectedBlueprint, false);
                            using (FileStream stream = new FileStream(Path.Combine(newFile, "bp.sbc"), FileMode.Open))
                            {
                                this.UpdateInfo(stream, this.m_selectedBlueprint);
                            }
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    content = this.m_content;
                    if (content != Content.Blueprint)
                    {
                        if (content == Content.Script)
                        {
                            if (Directory.Exists(file))
                            {
                                Directory.Move(file, newFile);
                            }
                            if (Directory.Exists(file))
                            {
                                Directory.Delete(file, true);
                            }
                        }
                    }
                    else
                    {
                        this.UpdatePrefab(this.m_selectedBlueprint, true);
                        this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeId = name;
                        this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeName = name;
                        this.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = name;
                        string path = Path.Combine(str, "temp");
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        Directory.Move(file, path);
                        Directory.Move(path, newFile);
                        MyBlueprintUtils.SavePrefabToFile(this.m_loadedPrefab, name, this.GetCurrentLocalDirectory(), true, MyBlueprintTypeEnum.LOCAL);
                    }
                    this.RefreshBlueprintList(false);
                    this.UpdatePrefab(this.m_selectedBlueprint, false);
                    using (FileStream stream = new FileStream(Path.Combine(newFile, "bp.sbc"), FileMode.Open))
                    {
                        this.UpdateInfo(stream, this.m_selectedBlueprint);
                    }
                }
            }
        }

        private static bool CheckBlueprintForModsAndModifiedBlocks(MyObjectBuilder_Definitions prefab)
        {
            MyObjectBuilder_ShipBlueprintDefinition[] shipBlueprints = prefab.ShipBlueprints;
            return ((shipBlueprints == null) || MyGridClipboard.CheckPastedBlocks(shipBlueprints[0].CubeGrids));
        }

        private static void CheckCurrentLocalDirectory_Blueprint()
        {
            if (!Directory.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, GetCurrentLocalDirectory(Content.Blueprint))))
            {
                SetCurrentLocalDirectory(Content.Blueprint, string.Empty);
            }
        }

        public override bool CloseScreen()
        {
            Content content = this.m_content;
            if (content != Content.Blueprint)
            {
                if ((content == Content.Script) && (this.m_onCloseAction != null))
                {
                    this.m_onCloseAction();
                }
                return base.CloseScreen();
            }
            if (!this.m_blueprintBeingLoaded)
            {
                return base.CloseScreen();
            }
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.BlueprintsMessageBoxDesc_StillLoading);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.BlueprintsMessageBoxTitle_StillLoading), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    this.m_blueprintBeingLoaded = false;
                    Task.valid = false;
                    this.CloseScreen();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            return false;
        }

        private void CopyBlueprintAndClose()
        {
            if (this.CopySelectedItemToClipboard())
            {
                this.CloseScreen();
            }
        }

        public static bool CopyBlueprintPrefabToClipboard(MyObjectBuilder_Definitions prefab, MyGridClipboard clipboard, bool setOwner = true)
        {
            if (prefab.ShipBlueprints == null)
            {
                return false;
            }
            MyObjectBuilder_CubeGrid[] cubeGrids = prefab.ShipBlueprints[0].CubeGrids;
            if ((cubeGrids == null) || (cubeGrids.Length == 0))
            {
                return false;
            }
            BoundingSphere sphere = cubeGrids[0].CalculateBoundingSphere();
            MyPositionAndOrientation orientation = cubeGrids[0].PositionAndOrientation.Value;
            MatrixD xd = MatrixD.CreateWorld((Vector3D) orientation.Position, (Vector3) orientation.Forward, (Vector3) orientation.Up);
            Matrix matrix = Matrix.Normalize(Matrix.Invert((Matrix) xd));
            BoundingSphere sphere2 = sphere.Transform((Matrix) xd);
            Vector3 dragPointDelta = Vector3.TransformNormal(((Vector3) orientation.Position) - sphere2.Center, matrix);
            float dragVectorLength = sphere.Radius + 10f;
            if ((!MySession.Static.IsUserAdmin(Sync.MyId) || !MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.KeepOriginalOwnershipOnPaste)) && setOwner)
            {
                MyObjectBuilder_CubeGrid[] gridArray2 = cubeGrids;
                for (int i = 0; i < gridArray2.Length; i++)
                {
                    foreach (MyObjectBuilder_CubeBlock block in gridArray2[i].CubeBlocks)
                    {
                        if (block.Owner != 0)
                        {
                            block.Owner = MySession.Static.LocalPlayerId;
                        }
                    }
                }
            }
            if (MyFakes.ENABLE_FRACTURE_COMPONENT)
            {
                for (int i = 0; i < cubeGrids.Length; i++)
                {
                    cubeGrids[i] = MyFracturedBlock.ConvertFracturedBlocksToComponents(cubeGrids[i]);
                }
            }
            clipboard.SetGridFromBuilders(cubeGrids, dragPointDelta, dragVectorLength);
            clipboard.ShowModdedBlocksWarning = false;
            return true;
        }

        private bool CopySelectedItemToClipboard()
        {
            if (this.ValidateSelecteditem())
            {
                ulong? nullable2;
                string path = "";
                MyObjectBuilder_Definitions prefab = null;
                switch (this.m_selectedBlueprint.Type)
                {
                    case MyBlueprintTypeEnum.STEAM:
                    {
                        ulong? publishedItemId = this.m_selectedBlueprint.PublishedItemId;
                        path = this.m_selectedBlueprint.Item.Folder;
                        if (File.Exists(path) || MyFileSystem.IsDirectory(path))
                        {
                            m_LoadPrefabData = new Sandbox.Game.Gui.LoadPrefabData(prefab, path, this, publishedItemId);
                            Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadWorkshopPrefab), null, m_LoadPrefabData);
                        }
                        break;
                    }
                    case MyBlueprintTypeEnum.LOCAL:
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), this.m_selectedBlueprint.Data.Name, "bp.sbc");
                        if (File.Exists(path))
                        {
                            nullable2 = null;
                            m_LoadPrefabData = new Sandbox.Game.Gui.LoadPrefabData(prefab, path, this, nullable2);
                            Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefab), null, m_LoadPrefabData);
                        }
                        break;

                    case MyBlueprintTypeEnum.SHARED:
                        return false;

                    case MyBlueprintTypeEnum.DEFAULT:
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, this.m_selectedBlueprint.Data.Name, "bp.sbc");
                        if (File.Exists(path))
                        {
                            nullable2 = null;
                            m_LoadPrefabData = new Sandbox.Game.Gui.LoadPrefabData(prefab, path, this, nullable2);
                            Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefab), null, m_LoadPrefabData);
                        }
                        break;

                    case MyBlueprintTypeEnum.CLOUD:
                        m_LoadPrefabData = new Sandbox.Game.Gui.LoadPrefabData(prefab, this.m_selectedBlueprint, this);
                        Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefabFromCloud), null, m_LoadPrefabData);
                        break;

                    default:
                        break;
                }
            }
            return false;
        }

        private void CopyToClipboard()
        {
            if (this.m_selectedBlueprint != null)
            {
                Content content = this.m_content;
                if (content != Content.Blueprint)
                {
                    if (content == Content.Script)
                    {
                        this.OpenSelectedSript();
                    }
                }
                else if (!this.m_blueprintBeingLoaded)
                {
                    if (this.m_selectedBlueprint.IsDirectory)
                    {
                        if (!string.IsNullOrEmpty(this.m_selectedBlueprint.BlueprintName))
                        {
                            this.SetCurrentLocalDirectory(Path.Combine(this.GetCurrentLocalDirectory(), this.m_selectedBlueprint.BlueprintName));
                        }
                        else
                        {
                            char[] separator = new char[] { Path.DirectorySeparatorChar };
                            string[] paths = this.GetCurrentLocalDirectory().Split(separator);
                            if (paths.Length <= 1)
                            {
                                this.SetCurrentLocalDirectory(string.Empty);
                            }
                            else
                            {
                                paths[paths.Length - 1] = string.Empty;
                                this.SetCurrentLocalDirectory(Path.Combine(paths));
                            }
                        }
                        CheckCurrentLocalDirectory_Blueprint();
                        this.RefreshAndReloadItemList();
                    }
                    else
                    {
                        this.m_blueprintBeingLoaded = true;
                        switch (this.m_selectedBlueprint.Type)
                        {
                            case MyBlueprintTypeEnum.STEAM:
                                this.m_blueprintBeingLoaded = true;
                                Task = Parallel.Start(delegate {
                                    if (!MyWorkshop.IsUpToDate(this.m_selectedBlueprint.Item))
                                    {
                                        this.DownloadBlueprintFromSteam(this.m_selectedBlueprint.Item);
                                    }
                                }, () => this.CopyBlueprintAndClose());
                                return;

                            case MyBlueprintTypeEnum.LOCAL:
                            case MyBlueprintTypeEnum.DEFAULT:
                            case MyBlueprintTypeEnum.CLOUD:
                                this.m_blueprintBeingLoaded = true;
                                this.CopyBlueprintAndClose();
                                return;

                            case MyBlueprintTypeEnum.SHARED:
                                this.OpenSharedBlueprint(this.m_selectedBlueprint);
                                return;
                        }
                    }
                }
            }
        }

        public void CreateBlueprintFromClipboard(bool withScreenshot = false, bool replace = false)
        {
            if (this.m_clipboard.CopiedGridsName != null)
            {
                string str = MyUtils.StripInvalidChars(this.m_clipboard.CopiedGridsName);
                string str2 = str;
                string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), str);
                for (int i = 1; MyFileSystem.DirectoryExists(path); i++)
                {
                    str2 = str + "_" + i;
                    path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), str2);
                }
                MyObjectBuilder_ShipBlueprintDefinition definition = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ShipBlueprintDefinition>();
                definition.Id = (SerializableDefinitionId) new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_ShipBlueprintDefinition)), MyUtils.StripInvalidChars(str));
                definition.CubeGrids = this.m_clipboard.CopiedGrids.ToArray();
                definition.DLCs = this.GetNecessaryDLCs(definition.CubeGrids);
                definition.RespawnShip = false;
                definition.DisplayName = MyGameService.UserName;
                definition.OwnerSteamId = Sync.MyId;
                definition.CubeGrids[0].DisplayName = str;
                MyObjectBuilder_Definitions prefab = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
                prefab.ShipBlueprints = new MyObjectBuilder_ShipBlueprintDefinition[] { definition };
                if (MySandboxGame.Config.EnableSteamCloud & withScreenshot)
                {
                    this.SavePrefabToCloudWithScreenshot(prefab, this.m_clipboard.CopiedGridsName, this.GetCurrentLocalDirectory(), replace);
                }
                else
                {
                    if (withScreenshot)
                    {
                        this.TakeScreenshotLocalBP(str2, this.m_selectedButton);
                    }
                    MyBlueprintUtils.SavePrefabToFile(prefab, this.m_clipboard.CopiedGridsName, this.GetCurrentLocalDirectory(), replace, MyBlueprintTypeEnum.LOCAL);
                    this.SetGroupSelection(MyBlueprintTypeEnum.MIXED);
                    this.RefreshBlueprintList(false);
                    this.SelectNewBlueprint(str2, MySandboxGame.Config.EnableSteamCloud ? MyBlueprintTypeEnum.CLOUD : MyBlueprintTypeEnum.LOCAL);
                }
            }
        }

        public static MyGuiBlueprintScreen_Reworked CreateBlueprintScreen(MyGridClipboard clipboard, bool allowCopyToClipboard, MyBlueprintAccessType accessType)
        {
            MyGuiBlueprintScreen_Reworked reworked1 = new MyGuiBlueprintScreen_Reworked();
            reworked1.SetBlueprintInitData(clipboard, allowCopyToClipboard, accessType);
            reworked1.FinishInitialization();
            return reworked1;
        }

        private MyGuiControlImage CreateButtonIcon(MyGuiControlButton butt, string texture)
        {
            butt.Size = new Vector2(butt.Size.X, (butt.Size.X * 4f) / 3f);
            float y = 0.95f * Math.Min(butt.Size.X, butt.Size.Y);
            Vector2? size = new Vector2(y * 0.75f, y);
            VRageMath.Vector4? backgroundColor = null;
            string[] textures = new string[] { texture };
            MyGuiControlImage control = new MyGuiControlImage(new Vector2?(butt.Position + new Vector2(-0.0016f, 0.018f)), size, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.Controls.Add(control);
            return control;
        }

        private void CreateButtons()
        {
            Vector2 vector = new Vector2(-0.37f, -0.27f);
            Vector2 vector2 = new Vector2(-0.0955f, -0.27f);
            Vector2 vector3 = new Vector2(-0.0812f, 0.255f);
            Vector2 vector4 = new Vector2(-0.0812f, 0.395f);
            Vector2 vector1 = new Vector2(0.144f, 0.035f);
            float usableWidth = 0.029f;
            float num2 = 0.178f;
            float textScale = 0.8f;
            MyStringId? tooltip = null;
            this.m_button_Refresh = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_Refresh), true, tooltip, textScale);
            this.m_button_Refresh.Position = vector + (new Vector2(usableWidth, 0f) * 0f);
            this.m_button_Refresh.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_GroupSelection = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_GroupSelection), true, tooltip, textScale);
            this.m_button_GroupSelection.Position = vector + (new Vector2(usableWidth, 0f) * 1f);
            this.m_button_GroupSelection.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_Sorting = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_Sorting), true, tooltip, textScale);
            this.m_button_Sorting.Position = vector + (new Vector2(usableWidth, 0f) * 2f);
            this.m_button_Sorting.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_NewBlueprint = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_NewBlueprint), true, tooltip, textScale);
            this.m_button_NewBlueprint.Position = vector + (new Vector2(usableWidth, 0f) * 3f);
            this.m_button_NewBlueprint.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_DirectorySelection = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_DirectorySelection), true, tooltip, textScale);
            this.m_button_DirectorySelection.Position = vector + (new Vector2(usableWidth, 0f) * 4f);
            this.m_button_DirectorySelection.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_HideThumbnails = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_HideThumbnails), true, tooltip, textScale);
            this.m_button_HideThumbnails.Position = vector + (new Vector2(usableWidth, 0f) * 5f);
            this.m_button_HideThumbnails.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_OpenWorkshop = MyBlueprintUtils.CreateButton(this, usableWidth, null, new Action<MyGuiControlButton>(this.OnButton_OpenWorkshop), true, tooltip, textScale);
            this.m_button_OpenWorkshop.Position = vector + (new Vector2(usableWidth, 0f) * 6f);
            this.m_button_OpenWorkshop.ShowTooltipWhenDisabled = true;
            float x = 0.1502911f;
            tooltip = null;
            this.m_button_TabInfo = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_TabInfo), true, tooltip, textScale);
            this.m_button_TabInfo.Position = vector2 + (new Vector2(x, 0f) * 0f);
            this.m_button_TabInfo.Size = new Vector2(x, (usableWidth * 4f) / 3f);
            this.m_button_TabInfo.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_TabEdit = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_TabEdit), true, tooltip, textScale);
            this.m_button_TabEdit.Position = vector2 + (new Vector2(x + MyGuiConstants.GENERIC_BUTTON_SPACING.X, 0f) * 1f);
            this.m_button_TabEdit.Size = new Vector2(x, (usableWidth * 4f) / 3f);
            this.m_button_TabEdit.ShowTooltipWhenDisabled = true;
            float num5 = 0.01f;
            tooltip = null;
            this.m_button_Rename = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_Rename), true, tooltip, textScale);
            this.m_button_Rename.Position = vector3 + (new Vector2(num2 + num5, 0f) * 0f);
            this.m_button_Rename.Size = new Vector2(this.m_button_Rename.Size.X, this.m_button_Rename.Size.Y * 1.3f);
            this.m_button_Rename.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_Replace = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_Replace), true, tooltip, textScale);
            this.m_button_Replace.Position = vector3 + (new Vector2(num2 + num5, 0f) * 1f);
            this.m_button_Replace.Size = new Vector2(this.m_button_Replace.Size.X, this.m_button_Replace.Size.Y * 1.3f);
            this.m_button_Replace.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_Delete = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_Delete), true, tooltip, textScale);
            this.m_button_Delete.Position = vector3 + (new Vector2(num2 + num5, 0f) * 2f);
            this.m_button_Delete.Size = new Vector2(this.m_button_Delete.Size.X, this.m_button_Delete.Size.Y * 1.3f);
            this.m_button_Delete.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_TakeScreenshot = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_TakeScreenshot), true, tooltip, textScale);
            this.m_button_TakeScreenshot.Position = (vector3 + (new Vector2(num2 + num5, 0f) * 1f)) + new Vector2(0f, 0.055f);
            this.m_button_TakeScreenshot.Size = new Vector2(this.m_button_TakeScreenshot.Size.X, this.m_button_TakeScreenshot.Size.Y * 1.3f);
            this.m_button_TakeScreenshot.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_Publish = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_Publish), true, tooltip, textScale);
            this.m_button_Publish.Position = (vector3 + (new Vector2(num2 + num5, 0f) * 2f)) + new Vector2(0f, 0.055f);
            this.m_button_Publish.Size = new Vector2(this.m_button_Publish.Size.X, this.m_button_Publish.Size.Y * 1.3f);
            this.m_button_Publish.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_OpenInWorkshop = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_OpenInWorkshop), true, tooltip, textScale);
            this.m_button_OpenInWorkshop.Position = vector4 + (new Vector2(num2 + num5, 0f) * 1f);
            this.m_button_OpenInWorkshop.Size = new Vector2(this.m_button_OpenInWorkshop.Size.X, this.m_button_OpenInWorkshop.Size.Y * 1.3f);
            this.m_button_OpenInWorkshop.ShowTooltipWhenDisabled = true;
            tooltip = null;
            this.m_button_CopyToClipboard = MyBlueprintUtils.CreateButton(this, num2, null, new Action<MyGuiControlButton>(this.OnButton_CopyToClipboard), true, tooltip, textScale);
            this.m_button_CopyToClipboard.Position = vector4 + (new Vector2(num2 + num5, 0f) * 2f);
            this.m_button_CopyToClipboard.Size = new Vector2(this.m_button_CopyToClipboard.Size.X, this.m_button_CopyToClipboard.Size.Y * 1.3f);
            this.m_button_CopyToClipboard.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = new Vector2(0.4f, -0.45f);
            button1.Size = new Vector2(0.045f, 0.05666667f);
            button1.Name = "Close";
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Close;
            button1.ActivateOnMouseRelease = true;
            this.m_button_Close = button1;
            this.m_button_Close.ButtonClicked += new Action<MyGuiControlButton>(this.OnButtonClose);
            this.Controls.Add(this.m_button_Close);
            this.m_icon_Refresh = this.CreateButtonIcon(this.m_button_Refresh, @"Textures\GUI\Icons\Blueprints\Refresh.png");
            this.m_icon_GroupSelection = this.CreateButtonIcon(this.m_button_GroupSelection, "");
            this.SetIconForGroupSelection();
            this.m_icon_Sorting = this.CreateButtonIcon(this.m_button_Sorting, "");
            this.SetIconForSorting();
            this.m_icon_OpenWorkshop = this.CreateButtonIcon(this.m_button_OpenWorkshop, @"Textures\GUI\Icons\Blueprints\Steam.png");
            this.m_icon_DirectorySelection = this.CreateButtonIcon(this.m_button_NewBlueprint, @"Textures\GUI\Icons\Blueprints\BP_New.png");
            this.m_icon_NewBlueprint = this.CreateButtonIcon(this.m_button_DirectorySelection, @"Textures\GUI\Icons\Blueprints\FolderIcon.png");
            this.m_icon_HideThumbnails = this.CreateButtonIcon(this.m_button_HideThumbnails, "");
            this.SetIconForHideThubnails();
        }

        public void CreateScriptFromEditor()
        {
            if ((this.m_getCodeFromEditor != null) && Directory.Exists(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL))
            {
                int num = 0;
                while (true)
                {
                    if (!Directory.Exists(Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), MyBlueprintUtils.DEFAULT_SCRIPT_NAME + "_" + num.ToString())))
                    {
                        string str = MyBlueprintUtils.DEFAULT_SCRIPT_NAME + "_" + num;
                        string path = Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), str);
                        Directory.CreateDirectory(path);
                        File.Copy(Path.Combine(MyFileSystem.ContentPath, MyBlueprintUtils.STEAM_THUMBNAIL_NAME), Path.Combine(path, MyBlueprintUtils.THUMB_IMAGE_NAME), true);
                        string contents = this.m_getCodeFromEditor();
                        File.WriteAllText(Path.Combine(path, MyBlueprintUtils.DEFAULT_SCRIPT_NAME + MyBlueprintUtils.SCRIPT_EXTENSION), contents, Encoding.UTF8);
                        this.RefreshAndReloadItemList();
                        this.SelectNewBlueprint(str, MyBlueprintTypeEnum.LOCAL);
                        break;
                    }
                    num++;
                }
            }
        }

        public static MyGuiBlueprintScreen_Reworked CreateScriptScreen(Action<string> onScriptOpened, Func<string> getCodeFromEditor, Action onCloseAction)
        {
            MyGuiBlueprintScreen_Reworked reworked1 = new MyGuiBlueprintScreen_Reworked();
            reworked1.SetScriptInitData(onScriptOpened, getCodeFromEditor, onCloseAction);
            reworked1.FinishInitialization();
            return reworked1;
        }

        private bool DeleteItem(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }
            Directory.Delete(path, true);
            return true;
        }

        private void DownloadBlueprintFromSteam(MyWorkshopItem item)
        {
            if (!MyWorkshop.IsUpToDate(item))
            {
                MyWorkshop.DownloadBlueprintBlockingUGC(item, false);
                this.ExtractWorkshopItem(item);
            }
        }

        private void DownloadBlueprints()
        {
            m_downloadFromSteam = true;
            List<MyWorkshopItem> results = new List<MyWorkshopItem>();
            bool subscribedBlueprintsBlocking = MyWorkshop.GetSubscribedBlueprintsBlocking(results);
            if (subscribedBlueprintsBlocking)
            {
                Directory.CreateDirectory(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP);
                foreach (MyWorkshopItem item in results)
                {
                    this.DownloadBlueprintFromSteam(item);
                    m_downloadFinished.Add(item.Id);
                }
            }
            using (SubscribedItemsLock.AcquireExclusiveUsing())
            {
                this.SetSubscriveItemList(ref results);
            }
            if (subscribedBlueprintsBlocking)
            {
                m_needsExtract = true;
                m_downloadFromSteam = false;
            }
            else
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.CannotFindBlueprintSteam), MySession.Platform), MyTexts.Get(MyCommonTexts.Error), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void DownloadScriptFromSteam()
        {
            if (this.m_selectedBlueprint != null)
            {
                MyWorkshop.DownloadScriptBlocking(this.m_selectedBlueprint.Item);
            }
        }

        private void ExtractWorkshopItem(MyWorkshopItem subItem)
        {
            if (!MyFileSystem.IsDirectory(subItem.Folder))
            {
                try
                {
                    string folder = subItem.Folder;
                    string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_WORKSHOP_TEMP, subItem.Id.ToString());
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    Directory.CreateDirectory(path);
                    MyObjectBuilder_ModInfo objectBuilder = new MyObjectBuilder_ModInfo {
                        SubtypeName = subItem.Title,
                        WorkshopId = subItem.Id,
                        SteamIDOwner = subItem.OwnerId
                    };
                    string str3 = Path.Combine(MyBlueprintUtils.BLUEPRINT_WORKSHOP_TEMP, subItem.Id.ToString(), "info.temp");
                    if (File.Exists(str3))
                    {
                        File.Delete(str3);
                    }
                    MyObjectBuilderSerializer.SerializeXML(str3, false, objectBuilder, null);
                    if (string.IsNullOrEmpty(folder))
                    {
                        object[] objArray1 = new object[] { "Path in Folder directory of blueprint \"", subItem.Title, "\" ", subItem.Id, " is null, it shouldn't be and who knows what problems it causes. " };
                        MyLog.Default.Critical(new StringBuilder(string.Concat(objArray1)));
                    }
                    else
                    {
                        MyZipArchive archive = MyZipArchive.OpenOnFile(folder, FileMode.Open, FileAccess.Read, FileShare.Read, false);
                        if ((archive != null) && archive.FileExists(MyBlueprintUtils.THUMB_IMAGE_NAME))
                        {
                            Stream stream = archive.GetFile(MyBlueprintUtils.THUMB_IMAGE_NAME).GetStream(FileMode.Open, FileAccess.Read);
                            if (stream != null)
                            {
                                using (FileStream stream2 = File.Create(Path.Combine(path, MyBlueprintUtils.THUMB_IMAGE_NAME)))
                                {
                                    stream.CopyTo(stream2);
                                }
                            }
                            stream.Close();
                        }
                        archive.Dispose();
                    }
                }
                catch (IOException exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
            MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(subItem.Id));
            info1.BlueprintName = subItem.Title;
            MyBlueprintItemInfo info = info1;
            MyGuiControlListbox.Item listItem = new MyGuiControlListbox.Item(null, null, null, info, null);
            if (this.m_BPList.Controls.FindIndex(delegate (MyGuiControlBase item) {
                ulong? publishedItemId = (item.UserData as MyBlueprintItemInfo).PublishedItemId;
                ulong? nullable2 = (listItem.UserData as MyBlueprintItemInfo).PublishedItemId;
                return ((publishedItemId.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((publishedItemId != null) == (nullable2 != null))) && ((item.UserData as MyBlueprintItemInfo).Type == MyBlueprintTypeEnum.STEAM);
            }) == -1)
            {
                MySandboxGame.Static.Invoke(() => this.AddBlueprintButton(info, false, false, true), "AddBlueprintButton");
            }
        }

        private void FinishInitialization()
        {
            if (m_downloadFromSteam)
            {
                m_downloadFromSteam = false;
            }
            this.RecreateControls(true);
            this.TrySelectFirstBlueprint();
        }

        private void GetBlueprints(string directory, MyBlueprintTypeEnum type)
        {
            List<MyBlueprintItemInfo> list = new List<MyBlueprintItemInfo>();
            if (Directory.Exists(directory))
            {
                List<string> list2 = new List<string>();
                List<string> list3 = new List<string>();
                foreach (string str in Directory.GetDirectories(directory))
                {
                    list2.Add(str + @"\bp.sbc");
                    char[] separator = new char[] { '\\' };
                    string[] strArray2 = str.Split(separator);
                    list3.Add(strArray2[strArray2.Length - 1]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    string name = list3[i];
                    string path = list2[i];
                    if (File.Exists(path))
                    {
                        ulong? id = null;
                        MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(type, id);
                        info1.TimeCreated = new DateTime?(File.GetCreationTimeUtc(path));
                        info1.TimeUpdated = new DateTime?(File.GetLastWriteTimeUtc(path));
                        info1.BlueprintName = name;
                        MyBlueprintItemInfo item = info1;
                        item.SetAdditionalBlueprintInformation(name, name, null);
                        list.Add(item);
                    }
                }
                this.SortBlueprints(list, MyBlueprintTypeEnum.LOCAL);
                this.AddBlueprintButtons(ref list, false, false, false);
            }
        }

        private void GetBlueprintsFromCloud()
        {
            List<MyCloudFileInfo> cloudFiles = MyGameService.GetCloudFiles(MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY);
            if (cloudFiles != null)
            {
                List<MyBlueprintItemInfo> list = new List<MyBlueprintItemInfo>();
                Dictionary<string, MyBlueprintItemInfo> dictionary = new Dictionary<string, MyBlueprintItemInfo>();
                Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
                foreach (MyCloudFileInfo info in cloudFiles)
                {
                    char[] separator = new char[] { '/' };
                    string[] textArray1 = info.Name.Split(separator);
                    string key = textArray1[textArray1.Length - 2];
                    string str2 = textArray1[textArray1.Length - 1];
                    if (str2.Equals(MyBlueprintUtils.THUMB_IMAGE_NAME))
                    {
                        if (!dictionary2.ContainsKey(key))
                        {
                            dictionary2.Add(key, info.LocalPath);
                            continue;
                        }
                        MyBlueprintItemInfo info2 = dictionary2[key] as MyBlueprintItemInfo;
                        if (info2 == null)
                        {
                            continue;
                        }
                        info2.Data.CloudImagePath = info.LocalPath;
                        continue;
                    }
                    if (str2.Equals(MyBlueprintUtils.BLUEPRINT_LOCAL_NAME))
                    {
                        MyBlueprintItemInfo info3 = null;
                        if (!dictionary.TryGetValue(key, out info3))
                        {
                            ulong? id = null;
                            MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.CLOUD, id);
                            info1.TimeCreated = new DateTime?(DateTime.FromFileTimeUtc(info.Timestamp));
                            info1.TimeUpdated = new DateTime?(DateTime.FromFileTimeUtc(info.Timestamp));
                            info1.BlueprintName = key;
                            info1.CloudInfo = info;
                            info3 = info1;
                            info3.SetAdditionalBlueprintInformation(key, info.Name, null);
                            dictionary.Add(key, info3);
                            list.Add(info3);
                        }
                        if (info.Name.EndsWith(MyObjectBuilderSerializer.ProtobufferExtension))
                        {
                            info3.CloudPathPB = info.Name;
                        }
                        else if (info.Name.EndsWith(MyBlueprintUtils.BLUEPRINT_LOCAL_NAME))
                        {
                            info3.CloudPathXML = info.Name;
                        }
                        if (!dictionary2.ContainsKey(key))
                        {
                            dictionary2.Add(key, info3);
                        }
                        else
                        {
                            string str3 = dictionary2[key] as string;
                            if (!string.IsNullOrEmpty(str3))
                            {
                                info3.Data.CloudImagePath = str3;
                            }
                        }
                    }
                }
                this.SortBlueprints(list, MyBlueprintTypeEnum.CLOUD);
                this.AddBlueprintButtons(ref list, false, false, false);
            }
        }

        public string GetCurrentLocalDirectory() => 
            GetCurrentLocalDirectory(this.m_content);

        public static string GetCurrentLocalDirectory(Content content)
        {
            if (!m_currentLocalDirectoryDict.ContainsKey(content))
            {
                m_currentLocalDirectoryDict.Add(content, string.Empty);
            }
            return m_currentLocalDirectoryDict[content];
        }

        private string GetImagePath(MyBlueprintItemInfo data)
        {
            string cloudImagePath = string.Empty;
            if (data.Type == MyBlueprintTypeEnum.LOCAL)
            {
                Content content = this.m_content;
                if (content == Content.Blueprint)
                {
                    cloudImagePath = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), data.BlueprintName, MyBlueprintUtils.THUMB_IMAGE_NAME);
                }
                else if (content == Content.Script)
                {
                    cloudImagePath = Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), data.Data.Name, MyBlueprintUtils.THUMB_IMAGE_NAME);
                }
            }
            else if (data.Type == MyBlueprintTypeEnum.CLOUD)
            {
                cloudImagePath = data.Data.CloudImagePath;
            }
            else if (data.Type != MyBlueprintTypeEnum.STEAM)
            {
                if (data.Type == MyBlueprintTypeEnum.DEFAULT)
                {
                    cloudImagePath = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, data.BlueprintName, MyBlueprintUtils.THUMB_IMAGE_NAME);
                }
            }
            else
            {
                if (this.m_content == Content.Script)
                {
                    return cloudImagePath;
                }
                ulong? publishedItemId = data.PublishedItemId;
                if ((publishedItemId != null) && (data.Item != null))
                {
                    bool flag = false;
                    if ((data.Item.Folder == null) || !MyFileSystem.IsDirectory(data.Item.Folder))
                    {
                        cloudImagePath = Path.Combine(MyBlueprintUtils.BLUEPRINT_WORKSHOP_TEMP, publishedItemId.ToString(), MyBlueprintUtils.THUMB_IMAGE_NAME);
                    }
                    else
                    {
                        cloudImagePath = Path.Combine(data.Item.Folder, MyBlueprintUtils.THUMB_IMAGE_NAME);
                        flag = true;
                    }
                    bool flag2 = m_downloadFinished.Contains(data.Item.Id);
                    MyWorkshopItem worshopData = data.Item;
                    if ((flag2 && !this.IsExtracted(worshopData)) && !flag)
                    {
                        this.m_BPList.Enabled = false;
                        this.ExtractWorkshopItem(worshopData);
                        this.m_BPList.Enabled = true;
                    }
                    if (!m_downloadQueued.Contains(data.Item.Id) && !flag2)
                    {
                        this.m_BPList.Enabled = false;
                        m_downloadQueued.Add(data.Item.Id);
                        Task = Parallel.Start(delegate {
                            this.DownloadBlueprintFromSteam(worshopData);
                        }, delegate {
                            this.OnBlueprintDownloadedThumbnail(worshopData);
                        });
                        cloudImagePath = string.Empty;
                    }
                }
            }
            return cloudImagePath;
        }

        private void GetLocalNames_Blueprints(bool reload = false)
        {
            string directory = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory());
            this.GetBlueprints(directory, MyBlueprintTypeEnum.LOCAL);
            if (MySandboxGame.Config.EnableSteamCloud)
            {
                this.GetBlueprintsFromCloud();
            }
            if (Task.IsComplete)
            {
                if (reload)
                {
                    this.GetWorkshopItems();
                }
                else
                {
                    this.GetWorkshopItemsSteam();
                }
            }
            this.SortBlueprints(m_recievedBlueprints, MyBlueprintTypeEnum.LOCAL);
            this.AddBlueprintButtons(ref m_recievedBlueprints, false, false, false);
            if (MyFakes.ENABLE_DEFAULT_BLUEPRINTS)
            {
                this.GetBlueprints(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, MyBlueprintTypeEnum.DEFAULT);
            }
        }

        private void GetLocalNames_Scripts(bool reload = false)
        {
            string path = Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.GetCurrentLocalDirectory());
            if (!Directory.Exists(path))
            {
                MyFileSystem.CreateDirectoryRecursive(path);
            }
            List<MyBlueprintItemInfo> list = new List<MyBlueprintItemInfo>();
            foreach (string str2 in Directory.GetDirectories(path))
            {
                if (MyBlueprintUtils.IsItem_Script(str2))
                {
                    string fileName = Path.GetFileName(str2);
                    ulong? id = null;
                    MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.LOCAL, id);
                    info1.BlueprintName = fileName;
                    MyBlueprintItemInfo item = info1;
                    item.SetAdditionalBlueprintInformation(fileName, null, null);
                    list.Add(item);
                }
            }
            this.SortBlueprints(list, MyBlueprintTypeEnum.LOCAL);
            this.AddBlueprintButtons(ref list, false, false, false);
            if (Task.IsComplete & reload)
            {
                this.GetWorkshopItems();
            }
            else
            {
                this.AddWorkshopItemsToList();
            }
        }

        private string[] GetNecessaryDLCs(MyObjectBuilder_CubeGrid[] cubeGrids)
        {
            if (cubeGrids.IsNullOrEmpty<MyObjectBuilder_CubeGrid>())
            {
                return null;
            }
            HashSet<string> source = new HashSet<string>();
            MyObjectBuilder_CubeGrid[] gridArray = cubeGrids;
            for (int i = 0; i < gridArray.Length; i++)
            {
                foreach (MyObjectBuilder_CubeBlock block in gridArray[i].CubeBlocks)
                {
                    MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(block);
                    if ((cubeBlockDefinition != null) && ((cubeBlockDefinition.DLCs != null) && (cubeBlockDefinition.DLCs.Length != 0)))
                    {
                        for (int j = 0; j < cubeBlockDefinition.DLCs.Length; j++)
                        {
                            source.Add(cubeBlockDefinition.DLCs[j]);
                        }
                    }
                }
            }
            return source.ToArray<string>();
        }

        private void GetScriptsInfo()
        {
            List<MyWorkshopItem> results = new List<MyWorkshopItem>();
            bool subscribedIngameScriptsBlocking = MyWorkshop.GetSubscribedIngameScriptsBlocking(results);
            if (subscribedIngameScriptsBlocking)
            {
                if (Directory.Exists(MyBlueprintUtils.SCRIPT_FOLDER_WORKSHOP))
                {
                    try
                    {
                        Directory.Delete(MyBlueprintUtils.SCRIPT_FOLDER_WORKSHOP, true);
                    }
                    catch (IOException)
                    {
                    }
                }
                Directory.CreateDirectory(MyBlueprintUtils.SCRIPT_FOLDER_WORKSHOP);
            }
            using (SubscribedItemsLock.AcquireExclusiveUsing())
            {
                this.SetSubscriveItemList(ref results);
            }
            if (subscribedIngameScriptsBlocking)
            {
                this.AddWorkshopItemsToList();
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

        public MyBlueprintTypeEnum GetSelectedBlueprintType() => 
            this.GetSelectedBlueprintType(this.m_content);

        public MyBlueprintTypeEnum GetSelectedBlueprintType(Content content)
        {
            if (!m_selectedBlueprintTypeDict.ContainsKey(content))
            {
                m_selectedBlueprintTypeDict.Add(content, MyBlueprintTypeEnum.MIXED);
            }
            return m_selectedBlueprintTypeDict[content];
        }

        private SortOption GetSelectedSort() => 
            this.GetSelectedSort(this.m_content);

        private SortOption GetSelectedSort(Content content)
        {
            if (!m_selectedSortDict.ContainsKey(content))
            {
                m_selectedSortDict.Add(content, SortOption.None);
            }
            return m_selectedSortDict[content];
        }

        public List<MyWorkshopItem> GetSubscribedItemsList() => 
            GetSubscribedItemsList(this.m_content);

        public static List<MyWorkshopItem> GetSubscribedItemsList(Content content)
        {
            if (!m_subscribedItemsListDict.ContainsKey(content))
            {
                m_subscribedItemsListDict.Add(content, new List<MyWorkshopItem>());
            }
            return m_subscribedItemsListDict[content];
        }

        public bool GetThumbnailVisibility() => 
            this.GetThumbnailVisibility(this.m_content);

        public bool GetThumbnailVisibility(Content content)
        {
            if (!m_thumbnailsVisibleDict.ContainsKey(content))
            {
                m_thumbnailsVisibleDict.Add(content, true);
            }
            return m_thumbnailsVisibleDict[content];
        }

        private void GetWorkshopItems()
        {
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                Task = Parallel.Start(new Action(this.DownloadBlueprints));
            }
            else if (content == Content.Script)
            {
                Task = Parallel.Start(new Action(this.GetScriptsInfo));
            }
        }

        private void GetWorkshopItemsSteam()
        {
            List<MyBlueprintItemInfo> list = new List<MyBlueprintItemInfo>();
            using (SubscribedItemsLock.AcquireSharedUsing())
            {
                List<MyWorkshopItem> subscribedItemsList = this.GetSubscribedItemsList();
                for (int i = 0; i < subscribedItemsList.Count; i++)
                {
                    MyWorkshopItem item = subscribedItemsList[i];
                    MyAnalyticsHelper.ReportActivityStart(null, "show_blueprints", string.Empty, "gui", string.Empty, true);
                    string title = item.Title;
                    MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(item.Id));
                    info1.BlueprintName = title;
                    info1.Item = item;
                    MyBlueprintItemInfo info = info1;
                    info.SetAdditionalBlueprintInformation(title, title, item.DLCs.ToArray<uint>());
                    list.Add(info);
                }
            }
            this.SortBlueprints(list, MyBlueprintTypeEnum.STEAM);
            this.AddBlueprintButtons(ref list, false, false, false);
        }

        private void InitializeBPList()
        {
            float y = 0.307f;
            float x = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(x, y);
            this.m_BPTypesGroup = new MyGuiControlRadioButtonGroup();
            this.m_BPTypesGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.OnSelectItem);
            this.m_BPTypesGroup.MouseDoubleClick += new Action<MyGuiControlRadioButton>(this.OnMouseDoubleClickItem);
            MyGuiControlList list1 = new MyGuiControlList();
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            list1.Position = vector;
            list1.Size = new Vector2(0.2f, (base.m_size.Value.Y - y) - 0.048f);
            this.m_BPList = list1;
        }

        private bool IsExtracted(MyWorkshopItem subItem)
        {
            Content content = this.m_content;
            if (content != Content.Blueprint)
            {
                return (content == Content.Script);
            }
            return Directory.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_WORKSHOP_TEMP, subItem.Id.ToString()));
        }

        private void OnBlueprintDownloadedDetails(MyWorkshopItem workshopDetails, bool loadPrefab = true)
        {
            if (((this.m_selectedBlueprint != null) && (this.m_selectedBlueprint.Item != null)) && (workshopDetails.Id == this.m_selectedBlueprint.Item.Id))
            {
                this.UpdateDetailKeyEnable();
                MySandboxGame.Static.Invoke(() => this.UpdateNameAndDescription(), "OnBlueprintDownloadedDetails");
                ulong? publishedItemId = this.m_selectedBlueprint.PublishedItemId;
                string folder = workshopDetails.Folder;
                MyObjectBuilder_Definitions definitions = null;
                if (loadPrefab)
                {
                    definitions = MyBlueprintUtils.LoadWorkshopPrefab(folder, publishedItemId, false);
                    if (definitions == null)
                    {
                        return;
                    }
                }
                if (((this.m_selectedBlueprint != null) && (this.m_selectedBlueprint.Item != null)) && (workshopDetails.Id == this.m_selectedBlueprint.Item.Id))
                {
                    this.m_publishedItemId = publishedItemId;
                    if (loadPrefab)
                    {
                        this.m_loadedPrefab = definitions;
                    }
                    string path = Path.Combine(folder, "bp.sbc");
                    if (MyFileSystem.FileExists(path))
                    {
                        using (Stream stream = MyFileSystem.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            this.UpdateInfo(stream, this.m_selectedBlueprint);
                        }
                    }
                }
            }
        }

        private void OnBlueprintDownloadedThumbnail(MyWorkshopItem item)
        {
            Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, "temp", item.Id.ToString(), MyBlueprintUtils.THUMB_IMAGE_NAME);
            m_downloadQueued.Remove(item.Id);
            m_downloadFinished.Add(item.Id);
        }

        private void OnButton_CopyToClipboard(MyGuiControlButton button)
        {
            this.CopyToClipboard();
        }

        private void OnButton_Delete(MyGuiControlButton button)
        {
            if (this.m_selectedBlueprint != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Delete);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.DeleteBlueprintQuestion), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                    if ((callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES) && (this.m_selectedBlueprint != null))
                    {
                        Content content = this.m_content;
                        if (content != Content.Blueprint)
                        {
                            if (content == Content.Script)
                            {
                                string path = Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), this.m_selectedBlueprint.Data.Name);
                                if (this.DeleteItem(path))
                                {
                                    this.m_selectedBlueprint = null;
                                    this.ResetBlueprintUI();
                                }
                            }
                        }
                        else
                        {
                            switch (this.m_selectedBlueprint.Type)
                            {
                                case MyBlueprintTypeEnum.LOCAL:
                                case MyBlueprintTypeEnum.DEFAULT:
                                {
                                    string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), this.m_selectedBlueprint.BlueprintName);
                                    if (this.DeleteItem(path))
                                    {
                                        this.m_selectedBlueprint = null;
                                        this.ResetBlueprintUI();
                                    }
                                    break;
                                }
                                case MyBlueprintTypeEnum.CLOUD:
                                {
                                    string[] textArray1 = new string[] { MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY, "/", this.m_selectedBlueprint.BlueprintName, "/", MyBlueprintUtils.BLUEPRINT_LOCAL_NAME };
                                    string fileName = string.Concat(textArray1);
                                    if (MyGameService.DeleteFromCloud(fileName))
                                    {
                                        this.m_selectedBlueprint = null;
                                        this.ResetBlueprintUI();
                                        MyGameService.DeleteFromCloud(fileName + MyObjectBuilderSerializer.ProtobufferExtension);
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                        this.RefreshBlueprintList(false);
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnButton_DirectorySelection(MyGuiControlButton button)
        {
            string rootPath = string.Empty;
            Func<string, bool> isItem = null;
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                rootPath = MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL;
                isItem = new Func<string, bool>(MyBlueprintUtils.IsItem_Blueprint);
            }
            else if (content == Content.Script)
            {
                rootPath = MyBlueprintUtils.SCRIPT_FOLDER_LOCAL;
                isItem = new Func<string, bool>(MyBlueprintUtils.IsItem_Script);
            }
            MyGuiSandbox.AddScreen(new MyGuiFolderScreen(false, new Action<bool, string>(this.OnPathSelected), rootPath, this.GetCurrentLocalDirectory(), isItem));
        }

        private void OnButton_GroupSelection(MyGuiControlButton button)
        {
            MyBlueprintTypeEnum mIXED = MyBlueprintTypeEnum.MIXED;
            switch (this.GetSelectedBlueprintType())
            {
                case MyBlueprintTypeEnum.STEAM:
                    mIXED = MyBlueprintTypeEnum.CLOUD;
                    break;

                case MyBlueprintTypeEnum.LOCAL:
                    mIXED = MyBlueprintTypeEnum.STEAM;
                    break;

                case MyBlueprintTypeEnum.CLOUD:
                    mIXED = MyBlueprintTypeEnum.MIXED;
                    break;

                case MyBlueprintTypeEnum.MIXED:
                    mIXED = MyBlueprintTypeEnum.LOCAL;
                    break;

                default:
                    break;
            }
            this.SetGroupSelection(mIXED);
        }

        private void OnButton_HideThumbnails(MyGuiControlButton button)
        {
            this.SetThumbnailVisibility(!this.GetThumbnailVisibility());
            this.SetIconForHideThubnails();
            this.TogglePreviewVisibility();
        }

        private void OnButton_NewBlueprint(MyGuiControlButton button)
        {
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                this.CreateBlueprintFromClipboard(false, false);
            }
            else if (content == Content.Script)
            {
                this.CreateScriptFromEditor();
            }
        }

        private void OnButton_OpenInWorkshop(MyGuiControlButton button)
        {
            if (this.m_publishedItemId != null)
            {
                MyGuiSandbox.OpenUrlWithFallback($"http://steamcommunity.com/sharedfiles/filedetails/?id={this.m_publishedItemId}", "Steam Workshop", false);
            }
            else
            {
                StringBuilder messageCaption = new StringBuilder("Invalid workshop id");
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(""), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnButton_OpenWorkshop(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_BLUEPRINTS, "Steam Workshop", false);
        }

        private void OnButton_Publish(MyGuiControlButton button)
        {
            string localDirectory = this.GetCurrentLocalDirectory();
            Content content = this.m_content;
            if (content != Content.Blueprint)
            {
                if ((content == Content.Script) && (this.m_selectedBlueprint != null))
                {
                    MyBlueprintUtils.PublishScript(button, localDirectory, this.m_selectedBlueprint, () => this.m_wasPublished = true);
                }
            }
            else if (this.m_selectedBlueprint != null)
            {
                string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, localDirectory, this.m_selectedBlueprint.Data.Name, "bp.sbc");
                if (File.Exists(path))
                {
                    ulong? id = null;
                    m_LoadPrefabData = new Sandbox.Game.Gui.LoadPrefabData(null, path, null, id);
                    Action<WorkData> completionCallback = delegate (WorkData workData) {
                        Sandbox.Game.Gui.LoadPrefabData data = workData as Sandbox.Game.Gui.LoadPrefabData;
                        if (data.Prefab != null)
                        {
                            MyBlueprintUtils.PublishBlueprint(data.Prefab, this.m_selectedBlueprint.Data.Name, localDirectory, null);
                        }
                    };
                    Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefab), completionCallback, m_LoadPrefabData);
                }
            }
        }

        private void OnButton_Refresh(MyGuiControlButton button)
        {
            bool flag = false;
            MyBlueprintItemInfo itemInfo = null;
            if (this.m_selectedBlueprint != null)
            {
                flag = true;
                itemInfo = this.m_selectedBlueprint;
            }
            this.m_selectedButton = null;
            this.m_selectedBlueprint = null;
            this.UpdateDetailKeyEnable();
            m_downloadFinished.Clear();
            m_downloadQueued.Clear();
            this.RefreshAndReloadItemList();
            this.TrySelectFirstBlueprint();
            if (flag)
            {
                this.SelectBlueprint(itemInfo);
            }
            this.UpdateDetailKeyEnable();
        }

        private void OnButton_Rename(MyGuiControlButton button)
        {
            if (this.m_selectedBlueprint != null)
            {
                string caption = MyTexts.GetString(MySpaceTexts.DetailScreen_Button_Rename);
                MyScreenManager.AddScreen(new MyGuiBlueprintTextDialog(base.m_position, delegate (string result) {
                    if (result != null)
                    {
                        this.ChangeName(result);
                    }
                }, this.m_selectedBlueprint.Data.Name, caption, 40, 0.3f));
            }
        }

        private void OnButton_Replace(MyGuiControlButton button)
        {
            if (this.m_selectedBlueprint != null)
            {
                MyStringId? nullable;
                Vector2? nullable2;
                if ((this.m_selectedBlueprint.Type == MyBlueprintTypeEnum.CLOUD) && !MySandboxGame.Config.EnableSteamCloud)
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.Blueprints_ReplaceError_CloudOff), MyTexts.Get(MyCommonTexts.Blueprints_ReplaceError_CloudOff_Caption), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else if ((this.m_selectedBlueprint.Type == MyBlueprintTypeEnum.LOCAL) && MySandboxGame.Config.EnableSteamCloud)
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.Blueprints_ReplaceError_CloudOn), MyTexts.Get(MyCommonTexts.Blueprints_ReplaceError_CloudOn_Caption), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    StringBuilder builder;
                    Content content = this.m_content;
                    if (content == Content.Blueprint)
                    {
                        builder = MyTexts.Get(MyCommonTexts.BlueprintsMessageBoxTitle_Replace);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.BlueprintsMessageBoxDesc_Replace), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                string name = this.m_selectedBlueprint.Data.Name;
                                string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), name, "bp.sbc");
                                if (File.Exists(path))
                                {
                                    MyObjectBuilder_Definitions prefab = MyBlueprintUtils.LoadPrefab(path);
                                    this.m_clipboard.CopiedGrids[0].DisplayName = name;
                                    prefab.ShipBlueprints[0].CubeGrids = this.m_clipboard.CopiedGrids.ToArray();
                                    prefab.ShipBlueprints[0].DLCs = this.GetNecessaryDLCs(prefab.ShipBlueprints[0].CubeGrids);
                                    MyBlueprintUtils.SavePrefabToFile(prefab, this.m_clipboard.CopiedGridsName, this.GetCurrentLocalDirectory(), true, MyBlueprintTypeEnum.LOCAL);
                                    this.RefreshBlueprintList(false);
                                }
                            }
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                    else if (content == Content.Script)
                    {
                        builder = MyTexts.Get(MySpaceTexts.ProgrammableBlock_ReplaceScriptNameDialogTitle);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ReplaceScriptDialogText), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                string path = Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.m_selectedBlueprint.Data.Name, MyBlueprintUtils.DEFAULT_SCRIPT_NAME + MyBlueprintUtils.SCRIPT_EXTENSION);
                                if (File.Exists(path))
                                {
                                    File.WriteAllText(path, this.m_getCodeFromEditor(), Encoding.UTF8);
                                }
                            }
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
            }
        }

        private void OnButton_Sorting(MyGuiControlButton button)
        {
            switch (this.GetSelectedSort())
            {
                case SortOption.None:
                    this.SetSelectedSort(SortOption.Alphabetical);
                    break;

                case SortOption.Alphabetical:
                    this.SetSelectedSort(SortOption.CreationDate);
                    break;

                case SortOption.CreationDate:
                    this.SetSelectedSort(SortOption.UpdateDate);
                    break;

                case SortOption.UpdateDate:
                    this.SetSelectedSort(SortOption.None);
                    break;

                default:
                    break;
            }
            this.SetIconForSorting();
            this.OnReload(null);
        }

        private void OnButton_TabEdit(MyGuiControlButton button)
        {
            this.m_selectedTab = Tab.Edit;
            this.RecomputeDetailOffsets();
            this.RecomputeTabSelection();
            this.RepositionDetailedPage();
        }

        private void OnButton_TabInfo(MyGuiControlButton button)
        {
            this.m_selectedTab = Tab.Info;
            this.RecomputeDetailOffsets();
            this.RecomputeTabSelection();
            this.RepositionDetailedPage();
        }

        private void OnButton_TakeScreenshot(MyGuiControlButton button)
        {
            if (this.m_selectedBlueprint != null)
            {
                MyBlueprintTypeEnum type = this.m_selectedBlueprint.Type;
                if (type == MyBlueprintTypeEnum.LOCAL)
                {
                    this.TakeScreenshotLocalBP(this.m_selectedBlueprint.Data.Name, this.m_selectedButton);
                }
                else if (type == MyBlueprintTypeEnum.CLOUD)
                {
                    string pathRel = Path.Combine(MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY, this.m_selectedBlueprint.BlueprintName, MyBlueprintUtils.THUMB_IMAGE_NAME);
                    this.TakeScreenshotCloud(pathRel, Path.Combine(MyFileSystem.UserDataPath, pathRel), this.m_selectedButton);
                }
            }
        }

        private void OnButtonClose(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        private void OnFocusedItem(MyGuiControlBase control, bool state)
        {
            if (state)
            {
                MyGuiControlContentButton butt = control as MyGuiControlContentButton;
                this.SelectButton(butt, -1, false, false);
            }
        }

        private void OnLinkClicked(MyGuiControlBase sender, string url)
        {
            MyGuiSandbox.OpenUrlWithFallback(url, "Space Engineers Steam Workshop", false);
        }

        private void OnMouseDoubleClickItem(MyGuiControlRadioButton obj)
        {
            this.CopyToClipboard();
        }

        private void OnMouseOverItem(MyGuiControlRadioButton butt, bool isMouseOver)
        {
            if (!this.GetThumbnailVisibility())
            {
                if (!isMouseOver)
                {
                    this.m_thumbnailImage.SetTexture(null);
                    this.m_thumbnailImage.Visible = false;
                }
                else
                {
                    MyBlueprintItemInfo userData = butt.UserData as MyBlueprintItemInfo;
                    if (userData == null)
                    {
                        this.m_thumbnailImage.SetTexture(null);
                        this.m_thumbnailImage.Visible = false;
                    }
                    else
                    {
                        string imagePath = this.GetImagePath(userData);
                        if (File.Exists(imagePath))
                        {
                            List<string> texturesToLoad = new List<string>();
                            texturesToLoad.Add(imagePath);
                            MyRenderProxy.PreloadTextures(texturesToLoad, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
                            this.m_thumbnailImage.SetTexture(imagePath);
                            if (this.m_thumbnailImage.IsAnyTextureValid())
                            {
                                this.m_thumbnailImage.Visible = true;
                                this.UpdateThumbnailPosition();
                            }
                        }
                    }
                }
            }
        }

        public void OnPathSelected(bool confirmed, string pathNew)
        {
            if (confirmed)
            {
                SetCurrentLocalDirectory(this.m_content, pathNew);
                this.RefreshAndReloadItemList();
            }
        }

        internal void OnPrefabLoaded(MyObjectBuilder_Definitions prefab)
        {
            StringBuilder builder;
            MyStringId? nullable;
            Vector2? nullable2;
            this.m_blueprintBeingLoaded = false;
            if (prefab == null)
            {
                builder = MyTexts.Get(MyCommonTexts.Error);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.CannotFindBlueprint), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            else
            {
                if (MySandboxGame.Static.SessionCompatHelper != null)
                {
                    MySandboxGame.Static.SessionCompatHelper.CheckAndFixPrefab(prefab);
                }
                if (!CheckBlueprintForModsAndModifiedBlocks(prefab))
                {
                    builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextDoYouWantToPasteGridWithMissingBlocks), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            this.CloseScreen();
                            if (CopyBlueprintPrefabToClipboard(prefab, this.m_clipboard, true) && (this.m_accessType == MyBlueprintAccessType.NORMAL))
                            {
                                if (MySession.Static.IsCopyPastingEnabled)
                                {
                                    MySandboxGame.Static.Invoke(() => MyClipboardComponent.Static.HandlePasteInput(true), "BlueprintSelectionAutospawn2");
                                }
                                else
                                {
                                    MyClipboardComponent.ShowCannotPasteWarning();
                                }
                            }
                        }
                        MyGuiScreenMessageBox.ResultEnum enum1 = result;
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    this.CloseScreen();
                    if (CopyBlueprintPrefabToClipboard(prefab, this.m_clipboard, true) && (this.m_accessType == MyBlueprintAccessType.NORMAL))
                    {
                        if (MySession.Static.IsCopyPastingEnabled)
                        {
                            MySandboxGame.Static.Invoke(() => MyClipboardComponent.Static.HandlePasteInput(true), "BlueprintSelectionAutospawn1");
                        }
                        else
                        {
                            MyClipboardComponent.ShowCannotPasteWarning();
                        }
                    }
                }
            }
        }

        private void OnReload(MyGuiControlButton button)
        {
            this.m_selectedButton = null;
            this.m_selectedBlueprint = null;
            this.UpdateDetailKeyEnable();
            m_downloadFinished.Clear();
            m_downloadQueued.Clear();
            this.RefreshAndReloadItemList();
            this.ApplyFiltering();
            this.TrySelectFirstBlueprint();
        }

        private void OnScriptDownloaded()
        {
            if ((this.m_onScriptOpened != null) && (this.m_selectedBlueprint != null))
            {
                this.m_onScriptOpened(this.m_selectedBlueprint.Item.Folder);
            }
            this.m_BPList.Enabled = true;
        }

        private void OnSearchTextChange(string message)
        {
            this.ApplyFiltering();
            this.TrySelectFirstBlueprint();
        }

        private void OnSelectItem(MyGuiControlRadioButtonGroup args)
        {
            MyGuiControlContentButton selectedButton;
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                this.m_selectedBlueprint = null;
                this.m_selectedButton = null;
                this.m_loadedPrefab = null;
                m_LoadPrefabData = null;
                this.UpdateDetailKeyEnable();
                selectedButton = args.SelectedButton as MyGuiControlContentButton;
                if (selectedButton != null)
                {
                    MyBlueprintItemInfo userData = selectedButton.UserData as MyBlueprintItemInfo;
                    if (userData == null)
                    {
                        return;
                    }
                    this.m_selectedButton = selectedButton;
                    this.m_selectedBlueprint = userData;
                }
                this.UpdatePrefab(this.m_selectedBlueprint, false);
            }
            else if (content == Content.Script)
            {
                this.m_selectedBlueprint = null;
                this.m_selectedButton = null;
                selectedButton = args.SelectedButton as MyGuiControlContentButton;
                if (selectedButton != null)
                {
                    MyBlueprintItemInfo userData = selectedButton.UserData as MyBlueprintItemInfo;
                    if (userData == null)
                    {
                        return;
                    }
                    this.m_selectedButton = selectedButton;
                    this.m_selectedBlueprint = userData;
                    this.m_publishedItemId = this.m_selectedBlueprint.PublishedItemId;
                }
                this.UpdateNameAndDescription();
                this.UpdateInfo(null, this.m_selectedBlueprint);
                this.UpdateDetailKeyEnable();
            }
        }

        private void OnSendToPlayer()
        {
            if (this.m_sendToCombo.GetSelectedIndex() != 0)
            {
                if (this.m_selectedBlueprint == null)
                {
                    this.m_sendToCombo.SelectItemByIndex(0);
                }
                else
                {
                    ulong selectedKey = (ulong) this.m_sendToCombo.GetSelectedKey();
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<ulong, string, ulong, string>(x => new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen_Reworked.ShareBlueprintRequest), this.m_publishedItemId.Value, this.m_selectedBlueprint.Data.Name, selectedKey, MySession.Static.LocalHumanPlayer.DisplayName, targetEndpoint, position);
                }
            }
        }

        private void OpenSelectedSript()
        {
            if (this.m_selectedBlueprint.Type == MyBlueprintTypeEnum.STEAM)
            {
                this.OpenSharedScript(this.m_selectedBlueprint);
            }
            else if (this.m_onScriptOpened != null)
            {
                this.m_onScriptOpened(Path.Combine(MyBlueprintUtils.SCRIPT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), this.m_selectedBlueprint.Data.Name, MyBlueprintUtils.DEFAULT_SCRIPT_NAME + MyBlueprintUtils.SCRIPT_EXTENSION));
            }
            this.CloseScreen();
        }

        private void OpenSharedBlueprint(MyBlueprintItemInfo itemInfo)
        {
            StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.SharedBlueprint);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO_CANCEL, new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.SharedBlueprintQuestion), MySession.Platform), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MyGameService.OpenOverlayUrl($"http://steamcommunity.com/sharedfiles/filedetails/?id={itemInfo.PublishedItemId}");
                    m_recievedBlueprints.Remove(this.m_selectedBlueprint);
                    this.m_selectedBlueprint = null;
                    this.UpdateDetailKeyEnable();
                    this.RefreshBlueprintList(false);
                }
                else if (callbackReturn != MyGuiScreenMessageBox.ResultEnum.NO)
                {
                    MyGuiScreenMessageBox.ResultEnum enum1 = callbackReturn;
                }
                else
                {
                    m_recievedBlueprints.Remove(this.m_selectedBlueprint);
                    this.m_selectedBlueprint = null;
                    this.UpdateDetailKeyEnable();
                    this.RefreshBlueprintList(false);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OpenSharedScript(MyBlueprintItemInfo itemInfo)
        {
            this.m_BPList.Enabled = false;
            Task = Parallel.Start(new Action(this.DownloadScriptFromSteam), new Action(this.OnScriptDownloaded));
        }

        private void RecomputeDetailOffsets()
        {
            Tab selectedTab = this.m_selectedTab;
            if (selectedTab == Tab.Info)
            {
                this.m_guiMultilineHeight = 0.412f;
                this.m_guiAdditionalInfoOffset = 0.271f;
            }
            else if (selectedTab != Tab.Edit)
            {
                this.m_guiMultilineHeight = 0.382f;
                this.m_guiAdditionalInfoOffset = 0.285f;
            }
            else
            {
                this.m_guiMultilineHeight = 0.272f;
                this.m_guiAdditionalInfoOffset = 0.131f;
            }
        }

        private void RecomputeTabSelection()
        {
            if (this.m_button_TabInfo != null)
            {
                this.m_button_TabInfo.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_button_TabInfo.HasHighlight = this.m_selectedTab == Tab.Info;
                this.m_button_TabInfo.Selected = this.m_selectedTab == Tab.Info;
            }
            if (this.m_button_TabEdit != null)
            {
                this.m_button_TabEdit.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_button_TabEdit.HasHighlight = this.m_selectedTab == Tab.Edit;
                this.m_button_TabEdit.Selected = this.m_selectedTab == Tab.Edit;
            }
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            float x = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            Vector2 local1 = (-base.m_size.Value / 2f) + new Vector2(x, 0.22f);
            this.RecomputeDetailOffsets();
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            text1.Position = new Vector2((-0.5f * base.m_size.Value.X) + x, -0.345f);
            text1.Size = new Vector2(base.m_size.Value.X - 0.1f, 0.05f);
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            text1.Text = MyTexts.Get((this.m_content == Content.Blueprint) ? MyCommonTexts.BlueprintsScreen_Description : MyCommonTexts.ScriptsScreen_Description);
            MyGuiControlMultilineText local2 = text1;
            local2.Font = "Blue";
            MyGuiControlMultilineText control = local2;
            this.Controls.Add(control);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            list.AddHorizontal(new Vector2((-0.5f * base.m_size.Value.X) + x, -0.39f), base.m_size.Value.X - (2f * x), 0f, color);
            color = null;
            list.AddHorizontal(new Vector2((-0.5f * base.m_size.Value.X) + x, -0.3f), base.m_size.Value.X - (2f * x), 0f, color);
            color = null;
            list.AddHorizontal(new Vector2(-0.1715f, -0.225f), base.m_size.Value.X - 0.3245f, 0f, color);
            this.m_separator = new MyGuiControlSeparatorList();
            color = null;
            this.m_separator.AddHorizontal(new Vector2(-0.1715f, 0.232f), base.m_size.Value.X - 0.3245f, 0f, color);
            this.m_separator.Visible = this.m_selectedTab == Tab.Edit;
            this.Controls.Add(this.m_separator);
            color = null;
            list.AddHorizontal(new Vector2(-0.1715f, 0.374f), base.m_size.Value.X - 0.3245f, 0f, color);
            this.Controls.Add(list);
            MyStringId id = MySpaceTexts.ScreenBlueprintsRew_Caption;
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                id = MySpaceTexts.ScreenBlueprintsRew_Caption_Blueprint;
            }
            else if (content == Content.Script)
            {
                id = MySpaceTexts.ScreenBlueprintsRew_Caption_Script;
            }
            base.AddCaption(MyTexts.GetString(id), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2(0f, 0.02f), 0.8f);
            this.m_detailName = base.AddCaption("Blueprint Name", new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2(0.1035f, 0.233f), 0.8f);
            Vector2? position = null;
            position = null;
            color = null;
            int? visibleLinesCount = null;
            this.m_multiline = new MyGuiControlMultilineText(position, position, color, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, MyGuiConstants.TEXTURE_RECTANGLE_DARK, new MyGuiBorderThickness(0.005f, 0f, 0f, 0f));
            this.m_multiline.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            this.Controls.Add(this.m_multiline);
            this.m_detailsBackground = new MyGuiControlPanel();
            this.m_detailsBackground.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            this.Controls.Add(this.m_detailsBackground);
            this.m_searchBox = new MyGuiControlSearchBox(new Vector2(-0.382f, -0.21f), new Vector2(this.m_BPList.Size.X, 0.032f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.OnSearchTextChange);
            this.Controls.Add(this.m_searchBox);
            position = null;
            position = null;
            color = null;
            this.m_detailBlockCount = new MyGuiControlLabel(position, position, string.Format(MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_NumOfBlocks), string.Empty), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailBlockCount);
            position = null;
            position = null;
            color = null;
            this.m_detailBlockCountValue = new MyGuiControlLabel(position, position, "0", color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailBlockCountValue);
            position = null;
            position = null;
            color = null;
            this.m_detailSize = new MyGuiControlLabel(position, position, string.Format(MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_GridType), string.Empty), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailSize);
            position = null;
            position = null;
            color = null;
            this.m_detailSizeValue = new MyGuiControlLabel(position, position, "Unknown", color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailSizeValue);
            position = null;
            position = null;
            color = null;
            this.m_detailAuthor = new MyGuiControlLabel(position, position, string.Format(MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_Author), string.Empty), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailAuthor);
            position = null;
            position = null;
            color = null;
            this.m_detailAuthorName = new MyGuiControlLabel(position, position, "N/A", color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailAuthorName);
            position = null;
            position = null;
            color = null;
            this.m_detailDLC = new MyGuiControlLabel(position, position, string.Format(MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_Dlc), string.Empty), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailDLC);
            position = null;
            position = null;
            color = null;
            this.m_detailSendTo = new MyGuiControlLabel(position, position, string.Format(MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_PCU), string.Empty), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_detailSendTo);
            this.UpdatePrefab(null, false);
            this.UpdateInfo(null, null);
            color = null;
            this.m_sendToCombo = base.AddCombo(null, color, new Vector2(0.16f, 0.1f), 10);
            this.m_sendToCombo.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_SendToPlayer);
            visibleLinesCount = null;
            this.m_sendToCombo.AddItem(0L, new StringBuilder("   "), visibleLinesCount, null);
            foreach (MyNetworkClient client in Sync.Clients.GetClients())
            {
                if (client.SteamUserId != Sync.MyId)
                {
                    visibleLinesCount = null;
                    this.m_sendToCombo.AddItem(Convert.ToInt64(client.SteamUserId), new StringBuilder(client.DisplayName), visibleLinesCount, null);
                }
            }
            this.m_sendToCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnSendToPlayer);
            this.CreateButtons();
            this.RecomputeTabSelection();
            this.Controls.Add(this.m_BPList);
            content = this.m_content;
            if (content == Content.Blueprint)
            {
                this.m_detailSendTo.Visible = true;
            }
            else if (content == Content.Script)
            {
                this.m_sendToCombo.Visible = false;
                this.m_detailAuthor.Visible = false;
                this.m_detailAuthorName.Visible = false;
                this.m_detailBlockCount.Visible = false;
                this.m_detailBlockCountValue.Visible = false;
                this.m_detailSize.Visible = false;
                this.m_detailSizeValue.Visible = false;
                this.m_detailDLC.Visible = false;
                this.m_detailSendTo.Visible = false;
            }
            this.m_searchBox.Position = new Vector2((this.m_button_Refresh.Position.X - (this.m_button_Refresh.Size.X * 0.5f)) - 0.002f, this.m_searchBox.Position.Y);
            this.m_searchBox.Size = new Vector2((this.m_button_OpenWorkshop.Position.X + this.m_button_OpenWorkshop.Size.X) - this.m_button_Refresh.Position.X, this.m_searchBox.Size.Y);
            this.m_BPList.Position = new Vector2(this.m_searchBox.Position.X, this.m_BPList.Position.Y);
            this.m_BPList.Size = new Vector2(this.m_searchBox.Size.X, this.m_BPList.Size.Y);
            this.RefreshThumbnail();
            this.Controls.Add(this.m_thumbnailImage);
            this.RepositionDetailedPage();
            this.SetDetailPageTexts();
            this.UpdateDetailKeyEnable();
            base.FocusedControl = this.m_searchBox.TextBox;
        }

        public void RefreshAndReloadItemList()
        {
            this.m_BPList.Clear();
            this.m_BPTypesGroup.Clear();
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                this.GetLocalNames_Blueprints(true);
            }
            else if (content == Content.Script)
            {
                this.GetLocalNames_Scripts(true);
            }
            this.ApplyFiltering();
            this.TrySelectFirstBlueprint();
        }

        public void RefreshBlueprintList(bool fromTask = false)
        {
            bool flag = false;
            MyBlueprintItemInfo itemInfo = null;
            if (this.m_selectedBlueprint != null)
            {
                flag = true;
                itemInfo = this.m_selectedBlueprint;
            }
            this.m_BPList.Clear();
            this.m_BPTypesGroup.Clear();
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                this.GetLocalNames_Blueprints(fromTask);
            }
            else if (content == Content.Script)
            {
                this.GetLocalNames_Scripts(fromTask);
            }
            this.ApplyFiltering();
            this.m_selectedButton = null;
            this.m_selectedBlueprint = null;
            this.TrySelectFirstBlueprint();
            if (flag)
            {
                this.SelectBlueprint(itemInfo);
            }
            this.UpdateDetailKeyEnable();
        }

        public void RefreshThumbnail()
        {
            this.m_thumbnailImage = new MyGuiControlImage();
            this.m_thumbnailImage.Position = new Vector2(-0.31f, -0.224f);
            this.m_thumbnailImage.Size = new Vector2(0.2f, 0.175f);
            this.m_thumbnailImage.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            this.m_thumbnailImage.SetPadding(new MyGuiBorderThickness(2f, 2f, 2f, 2f));
            this.m_thumbnailImage.Visible = false;
            this.m_thumbnailImage.BorderEnabled = true;
            this.m_thumbnailImage.BorderSize = 1;
            this.m_thumbnailImage.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
        }

        private void RepositionDetailedPage()
        {
            Vector2 vector = new Vector2(-0.168f, this.m_guiAdditionalInfoOffset);
            Vector2 vector2 = new Vector2(-0.173f, -0.2655f);
            Vector2 vector3 = new Vector2(base.m_size.Value.X - 0.3245f, this.m_guiMultilineHeight);
            Vector2 zero = Vector2.Zero;
            Tab selectedTab = this.m_selectedTab;
            if (selectedTab == Tab.Info)
            {
                this.m_button_Rename.Visible = false;
                this.m_button_Replace.Visible = false;
                this.m_button_Delete.Visible = false;
                this.m_button_TakeScreenshot.Visible = false;
                this.m_button_Publish.Visible = false;
                this.m_separator.Visible = false;
                zero = new Vector2(0.394f, 0f) + new Vector2(-0.024f, 0.04f);
            }
            else if (selectedTab == Tab.Edit)
            {
                this.m_button_Rename.Visible = true;
                this.m_button_Replace.Visible = true;
                this.m_button_Delete.Visible = true;
                this.m_button_TakeScreenshot.Visible = this.m_content == Content.Blueprint;
                this.m_button_Publish.Visible = true;
                this.m_separator.Visible = true;
                zero = new Vector2(0.394f, 0f) + new Vector2(-0.024f, 0.04f);
            }
            this.m_multiline.Position = (vector2 + (0.5f * vector3)) + new Vector2(0f, 0.09f);
            this.m_multiline.Size = vector3;
            float x = (this.m_detailBlockCount.Position.X + Math.Max(Math.Max(this.m_detailBlockCount.Size.X, this.m_detailSize.Size.X), this.m_detailAuthor.Size.X)) + 0.001f;
            this.m_detailAuthor.Position = vector + new Vector2(0f, 0f);
            this.m_detailBlockCount.Position = vector + new Vector2(0f, 0.03f);
            this.m_detailSize.Position = vector + new Vector2(0f, 0.06f);
            this.m_detailBlockCountValue.Position = new Vector2(x, this.m_detailBlockCount.Position.Y);
            this.m_detailSizeValue.Position = new Vector2(x, this.m_detailSize.Position.Y);
            this.m_detailAuthorName.Position = new Vector2(x, this.m_detailAuthor.Position.Y);
            this.m_detailDLC.Position = vector + new Vector2(0.27f, 0f);
            this.m_detailSendTo.Position = vector + new Vector2(0.27f, 0.06f);
            this.m_sendToCombo.Position = vector + zero;
            this.m_sendToCombo.Size = this.m_button_CopyToClipboard.Size * 0.978f;
            vector3 = this.m_sendToCombo.Position - vector;
            this.m_detailsBackground.Position = this.m_multiline.Position + new Vector2(0f, (this.m_multiline.Size.Y / 2f) + 0.0715f);
            this.m_detailsBackground.Size = new Vector2(this.m_multiline.Size.X, (vector3.Y + this.m_sendToCombo.Size.Y) + 0.02f);
            foreach (MyGuiControlImage local1 in this.m_dlcIcons)
            {
                Vector2 position = local1.Position;
                position.Y = vector.Y;
                local1.Position = position;
            }
        }

        private void ResetBlueprintUI()
        {
            this.m_selectedBlueprint = null;
            this.UpdateDetailKeyEnable();
        }

        public void SavePrefabToCloudWithScreenshot(MyObjectBuilder_Definitions prefab, string name, string currentDirectory, bool replace = false)
        {
            string text1 = Path.Combine(MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY, name);
            string filePath = Path.Combine(text1, MyBlueprintUtils.BLUEPRINT_LOCAL_NAME);
            string str2 = Path.Combine(text1, MyBlueprintUtils.THUMB_IMAGE_NAME);
            if (!m_waitingForScreenshot.IsWaiting())
            {
                string pathFull = Path.Combine(MyFileSystem.UserDataPath, str2);
                m_waitingForScreenshot.SetData_CreateNewBlueprintCloud(str2, pathFull);
                MyRenderProxy.TakeScreenshot(new Vector2(0.5f, 0.5f), pathFull, false, true, false);
                MyBlueprintUtils.SaveToCloud(prefab, filePath, replace);
                this.SetGroupSelection(MyBlueprintTypeEnum.MIXED);
                this.RefreshBlueprintList(false);
                this.SelectNewBlueprint(name, MyBlueprintTypeEnum.CLOUD);
            }
        }

        public static void ScreenshotTaken(bool success, string filename)
        {
            if (m_waitingForScreenshot.IsWaiting())
            {
                switch (m_waitingForScreenshot.Option)
                {
                    case WaitForScreenshotOptions.TakeScreenshotLocal:
                        if (success)
                        {
                            MyRenderProxy.UnloadTexture(filename);
                            if (m_waitingForScreenshot.UsedButton != null)
                            {
                                m_waitingForScreenshot.UsedButton.CreatePreview(filename);
                            }
                        }
                        break;

                    case WaitForScreenshotOptions.CreateNewBlueprintCloud:
                        if (success && File.Exists(m_waitingForScreenshot.PathFull))
                        {
                            MyBlueprintUtils.SaveToCloudFile(m_waitingForScreenshot.PathFull, m_waitingForScreenshot.PathRel);
                        }
                        break;

                    case WaitForScreenshotOptions.TakeScreenshotCloud:
                        if (success)
                        {
                            if (File.Exists(m_waitingForScreenshot.PathFull))
                            {
                                MyBlueprintUtils.SaveToCloudFile(m_waitingForScreenshot.PathFull, m_waitingForScreenshot.PathRel);
                            }
                            MyRenderProxy.UnloadTexture(filename);
                            if (m_waitingForScreenshot.UsedButton != null)
                            {
                                if (string.IsNullOrEmpty(m_waitingForScreenshot.UsedButton.PreviewImagePath))
                                {
                                    m_waitingForScreenshot.UsedButton.CreatePreview(filename);
                                }
                                else
                                {
                                    MyRenderProxy.UnloadTexture(m_waitingForScreenshot.UsedButton.PreviewImagePath);
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
                m_waitingForScreenshot.Clear();
            }
        }

        public void SelectBlueprint(MyBlueprintItemInfo itemInfo)
        {
            int idx = -1;
            MyGuiControlContentButton butt = null;
            foreach (MyGuiControlRadioButton button2 in this.m_BPTypesGroup)
            {
                idx++;
                MyBlueprintItemInfo userData = button2.UserData as MyBlueprintItemInfo;
                if ((userData != null) && userData.Equals(itemInfo))
                {
                    butt = button2 as MyGuiControlContentButton;
                    break;
                }
            }
            if (butt != null)
            {
                this.SelectButton(butt, idx, true, true);
            }
        }

        public void SelectButton(MyGuiControlContentButton butt, int idx = -1, bool forceToTop = true, bool alwaysScroll = true)
        {
            float num;
            float num2;
            if (idx < 0)
            {
                bool flag = false;
                int num4 = -1;
                foreach (MyGuiControlRadioButton button in this.m_BPTypesGroup)
                {
                    num4++;
                    if (ReferenceEquals(butt, button))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return;
                }
                idx = num4;
            }
            if (!ReferenceEquals(this.m_selectedButton, butt))
            {
                this.m_BPTypesGroup.SelectByIndex(idx);
            }
            ScrollTestResult result = this.ShouldScroll(butt, idx, out num, out num2);
            if (alwaysScroll || (result != ScrollTestResult.Ok))
            {
                float num3;
                if (forceToTop || (result == ScrollTestResult.Lower))
                {
                    num3 = num;
                }
                else
                {
                    if (result != ScrollTestResult.Higher)
                    {
                        return;
                    }
                    num3 = num2 - 1f;
                }
                this.m_BPList.SetScrollBarPage(num3);
            }
        }

        public void SelectNewBlueprint(string name, MyBlueprintTypeEnum type)
        {
            int idx = -1;
            MyGuiControlContentButton butt = null;
            foreach (MyGuiControlRadioButton button2 in this.m_BPTypesGroup)
            {
                idx++;
                MyBlueprintItemInfo userData = button2.UserData as MyBlueprintItemInfo;
                if ((userData != null) && ((userData.Type == type) && userData.Data.Name.Equals(name)))
                {
                    butt = button2 as MyGuiControlContentButton;
                    break;
                }
            }
            if (butt != null)
            {
                this.SelectButton(butt, idx, true, true);
            }
        }

        private void SetBlueprintInitData(MyGridClipboard clipboard, bool allowCopyToClipboard, MyBlueprintAccessType accessType)
        {
            this.m_content = Content.Blueprint;
            this.m_accessType = accessType;
            this.m_clipboard = clipboard;
            this.m_allowCopyToClipboard = allowCopyToClipboard;
            CheckCurrentLocalDirectory_Blueprint();
            this.GetLocalNames_Blueprints(m_downloadFromSteam);
            this.ApplyFiltering();
        }

        public void SetCurrentLocalDirectory(string path)
        {
            SetCurrentLocalDirectory(this.m_content, path);
        }

        public static void SetCurrentLocalDirectory(Content content, string path)
        {
            if (m_currentLocalDirectoryDict.ContainsKey(content))
            {
                m_currentLocalDirectoryDict[content] = path;
            }
            else
            {
                m_currentLocalDirectoryDict.Add(content, path);
            }
        }

        private void SetDetailPageTexts()
        {
            this.m_button_Refresh.Text = null;
            this.m_button_Refresh.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButRefresh);
            this.m_button_GroupSelection.Text = null;
            this.m_button_GroupSelection.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButGrouping);
            this.m_button_Sorting.Text = null;
            this.m_button_Sorting.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButSort);
            this.m_button_OpenWorkshop.Text = null;
            this.m_button_OpenWorkshop.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButOpenWorkshop);
            this.m_button_DirectorySelection.Text = null;
            this.m_button_DirectorySelection.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButFolders);
            this.m_button_HideThumbnails.Text = null;
            this.m_button_HideThumbnails.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButVisibility);
            this.m_button_TabInfo.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButInfo);
            this.m_button_TabInfo.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButInfo);
            this.m_button_TabEdit.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButEdit);
            this.m_button_TabEdit.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButEdit);
            this.m_button_Rename.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButRename);
            this.m_button_Rename.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButRename);
            this.m_button_Replace.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButReplace);
            this.m_button_Replace.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButReplace);
            this.m_button_Delete.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButDelete);
            this.m_button_Delete.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButDelete);
            this.m_button_TakeScreenshot.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButScreenshot);
            this.m_button_TakeScreenshot.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButScreenshot);
            this.m_button_Publish.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButPublish);
            this.m_button_Publish.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButPublish);
            this.m_button_OpenInWorkshop.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButOpenInWorkshop);
            this.m_button_OpenInWorkshop.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButOpenInWorkshop);
            Content content = this.m_content;
            if (content == Content.Blueprint)
            {
                this.m_button_NewBlueprint.Text = null;
                this.m_button_NewBlueprint.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButNewBlueprint);
                this.m_button_CopyToClipboard.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButToClipboard);
                this.m_button_CopyToClipboard.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButToClipboard);
            }
            else if (content == Content.Script)
            {
                this.m_button_NewBlueprint.Text = null;
                this.m_button_NewBlueprint.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButNewScript);
                this.m_button_CopyToClipboard.Text = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_ButToEditor);
                this.m_button_CopyToClipboard.SetToolTip(MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButToEditor);
            }
        }

        private void SetGroupSelection(MyBlueprintTypeEnum option)
        {
            this.SetSelectedBlueprintType(option);
            this.SetIconForGroupSelection();
            this.ApplyFiltering();
            this.TrySelectFirstBlueprint();
        }

        private void SetIconForGroupSelection()
        {
            switch (this.GetSelectedBlueprintType())
            {
                case MyBlueprintTypeEnum.STEAM:
                    this.m_icon_GroupSelection.SetTexture(@"Textures\GUI\Icons\Blueprints\BP_Steam.png");
                    return;

                case MyBlueprintTypeEnum.LOCAL:
                    this.m_icon_GroupSelection.SetTexture(@"Textures\GUI\Icons\Blueprints\BP_Local.png");
                    return;

                case MyBlueprintTypeEnum.CLOUD:
                    this.m_icon_GroupSelection.SetTexture(@"Textures\GUI\Icons\Blueprints\BP_Cloud.png");
                    return;

                case MyBlueprintTypeEnum.MIXED:
                    this.m_icon_GroupSelection.SetTexture(@"Textures\GUI\Icons\Blueprints\BP_Mixed.png");
                    return;
            }
            this.m_icon_GroupSelection.SetTexture(@"Textures\GUI\Icons\Blueprints\BP_Mixed.png");
        }

        private void SetIconForHideThubnails()
        {
            if (this.GetThumbnailVisibility())
            {
                this.m_icon_HideThumbnails.SetTexture(@"Textures\GUI\Icons\Blueprints\ThumbnailsON.png");
            }
            else
            {
                this.m_icon_HideThumbnails.SetTexture(@"Textures\GUI\Icons\Blueprints\ThumbnailsOFF.png");
            }
        }

        private void SetIconForSorting()
        {
            switch (this.GetSelectedSort())
            {
                case SortOption.None:
                    this.m_icon_Sorting.SetTexture(@"Textures\GUI\Icons\Blueprints\NoSorting.png");
                    return;

                case SortOption.Alphabetical:
                    this.m_icon_Sorting.SetTexture(@"Textures\GUI\Icons\Blueprints\Alphabetical.png");
                    return;

                case SortOption.CreationDate:
                    this.m_icon_Sorting.SetTexture(@"Textures\GUI\Icons\Blueprints\ByCreationDate.png");
                    return;

                case SortOption.UpdateDate:
                    this.m_icon_Sorting.SetTexture(@"Textures\GUI\Icons\Blueprints\ByUpdateDate.png");
                    return;
            }
            this.m_icon_Sorting.SetTexture(@"Textures\GUI\Icons\Blueprints\NoSorting.png");
        }

        private void SetScriptInitData(Action<string> onScriptOpened, Func<string> getCodeFromEditor, Action onCloseAction)
        {
            this.m_content = Content.Script;
            this.m_onScriptOpened = onScriptOpened;
            this.m_getCodeFromEditor = getCodeFromEditor;
            this.m_onCloseAction = onCloseAction;
            CheckCurrentLocalDirectory_Blueprint();
            using (SubscribedItemsLock.AcquireSharedUsing())
            {
                this.GetLocalNames_Scripts(GetSubscribedItemsList(this.m_content).Count == 0);
            }
        }

        public void SetSelectedBlueprintType(MyBlueprintTypeEnum option)
        {
            SetSelectedBlueprintType(this.m_content, option);
        }

        public static void SetSelectedBlueprintType(Content content, MyBlueprintTypeEnum option)
        {
            if (m_selectedBlueprintTypeDict.ContainsKey(content))
            {
                m_selectedBlueprintTypeDict[content] = option;
            }
            else
            {
                m_selectedBlueprintTypeDict.Add(content, option);
            }
        }

        private void SetSelectedSort(SortOption option)
        {
            SetSelectedSort(this.m_content, option);
        }

        private static void SetSelectedSort(Content content, SortOption option)
        {
            if (m_selectedSortDict.ContainsKey(content))
            {
                m_selectedSortDict[content] = option;
            }
            else
            {
                m_selectedSortDict.Add(content, option);
            }
        }

        public void SetSubscriveItemList(ref List<MyWorkshopItem> list)
        {
            SetSubscriveItemList(ref list, this.m_content);
        }

        public static void SetSubscriveItemList(ref List<MyWorkshopItem> list, Content content)
        {
            if (m_subscribedItemsListDict.ContainsKey(content))
            {
                m_subscribedItemsListDict[content] = list;
            }
            else
            {
                m_subscribedItemsListDict.Add(content, list);
            }
        }

        public void SetThumbnailVisibility(bool option)
        {
            SetThumbnailVisibility(this.m_content, option);
        }

        public static void SetThumbnailVisibility(Content content, bool option)
        {
            if (m_thumbnailsVisibleDict.ContainsKey(content))
            {
                m_thumbnailsVisibleDict[content] = option;
            }
            else
            {
                m_thumbnailsVisibleDict.Add(content, option);
            }
        }

        [Event(null, 0xe7b), Reliable, Server]
        public static void ShareBlueprintRequest(ulong workshopId, string name, ulong sendToId, string senderName)
        {
            if (!Sync.IsServer || (sendToId == Sync.MyId))
            {
                ShareBlueprintRequestClient(workshopId, name, sendToId, senderName);
            }
            else
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, string, ulong, string>(x => new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen_Reworked.ShareBlueprintRequestClient), workshopId, name, sendToId, senderName, new EndpointId(sendToId), position);
            }
        }

        [Event(null, 0xe88), Reliable, Client]
        private static void ShareBlueprintRequestClient(ulong workshopId, string name, ulong sendToId, string senderName)
        {
            MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.SHARED, new ulong?(workshopId));
            info1.BlueprintName = name;
            MyBlueprintItemInfo info = info1;
            info.SetAdditionalBlueprintInformation(name, null, null);
            if (!m_recievedBlueprints.Any<MyBlueprintItemInfo>(delegate (MyBlueprintItemInfo item2) {
                ulong? publishedItemId = item2.PublishedItemId;
                ulong? nullable2 = info.PublishedItemId;
                return ((publishedItemId.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((publishedItemId != null) == (nullable2 != null)));
            }))
            {
                m_recievedBlueprints.Add(info);
                MyHudNotificationDebug notification = new MyHudNotificationDebug(string.Format(MyTexts.Get(MySpaceTexts.SharedBlueprintNotify).ToString(), senderName), 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug);
                MyHud.Notifications.Add(notification);
            }
        }

        private ScrollTestResult ShouldScroll(MyGuiControlContentButton butt, int idx, out float min, out float max)
        {
            float num = !this.GetThumbnailVisibility() ? MAGIC_SPACING_SMALL : MAGIC_SPACING_BIG;
            int num2 = 0;
            for (int i = 0; i < idx; i++)
            {
                if (!this.m_BPList.Controls[i].Visible)
                {
                    num2++;
                }
            }
            float num3 = (butt.Size.Y + this.m_BPList.GetItemMargins().SizeChange.Y) - num;
            float y = this.m_BPList.Size.Y;
            float num5 = ((idx - num2) * num3) / y;
            float num6 = (((idx - num2) + 1f) * num3) / y;
            float page = this.m_BPList.GetScrollBar().GetPage();
            min = num5;
            max = num6;
            return ((num5 >= page) ? ((num6 <= (page + 1f)) ? ScrollTestResult.Ok : ScrollTestResult.Higher) : ScrollTestResult.Lower);
        }

        private void SortBlueprints(List<MyBlueprintItemInfo> list, MyBlueprintTypeEnum type)
        {
            MyItemComparer_Rew comparer = null;
            switch (type)
            {
                case MyBlueprintTypeEnum.STEAM:
                    switch (this.GetSelectedSort())
                    {
                        case SortOption.Alphabetical:
                            comparer = new MyItemComparer_Rew((x, y) => x.BlueprintName.CompareTo(y.BlueprintName));
                            break;

                        case SortOption.CreationDate:
                            comparer = new MyItemComparer_Rew(delegate (MyBlueprintItemInfo x, MyBlueprintItemInfo y) {
                                DateTime timeCreated = x.Item.TimeCreated;
                                DateTime time2 = y.Item.TimeCreated;
                                return (timeCreated >= time2) ? ((timeCreated <= time2) ? 0 : -1) : 1;
                            });
                            break;

                        case SortOption.UpdateDate:
                            comparer = new MyItemComparer_Rew(delegate (MyBlueprintItemInfo x, MyBlueprintItemInfo y) {
                                DateTime timeUpdated = x.Item.TimeUpdated;
                                DateTime time2 = y.Item.TimeUpdated;
                                return (timeUpdated >= time2) ? ((timeUpdated <= time2) ? 0 : -1) : 1;
                            });
                            break;

                        default:
                            break;
                    }
                    break;

                case MyBlueprintTypeEnum.LOCAL:
                case MyBlueprintTypeEnum.CLOUD:
                    switch (this.GetSelectedSort())
                    {
                        case SortOption.Alphabetical:
                            comparer = new MyItemComparer_Rew((x, y) => x.BlueprintName.CompareTo(y.BlueprintName));
                            break;

                        case SortOption.CreationDate:
                            comparer = new MyItemComparer_Rew(delegate (MyBlueprintItemInfo x, MyBlueprintItemInfo y) {
                                DateTime? timeCreated = x.TimeCreated;
                                DateTime? nullable2 = y.TimeCreated;
                                if ((timeCreated == null) || (nullable2 == null))
                                {
                                    return 0;
                                }
                                return -1 * DateTime.Compare(timeCreated.Value, nullable2.Value);
                            });
                            break;

                        case SortOption.UpdateDate:
                            comparer = new MyItemComparer_Rew(delegate (MyBlueprintItemInfo x, MyBlueprintItemInfo y) {
                                DateTime? timeUpdated = x.TimeUpdated;
                                DateTime? nullable2 = y.TimeUpdated;
                                if ((timeUpdated == null) || (nullable2 == null))
                                {
                                    return 0;
                                }
                                return -1 * DateTime.Compare(timeUpdated.Value, nullable2.Value);
                            });
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
            if (comparer != null)
            {
                list.Sort(comparer);
            }
        }

        public void TakeScreenshotCloud(string pathRel, string pathFull, MyGuiControlContentButton button)
        {
            if (!m_waitingForScreenshot.IsWaiting())
            {
                m_waitingForScreenshot.SetData_TakeScreenshotCloud(pathRel, pathFull, button);
                MyRenderProxy.TakeScreenshot(new Vector2(0.5f, 0.5f), pathFull, false, true, false);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_ScreenBeingTaken_Caption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_ScreenBeingTaken), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public void TakeScreenshotLocalBP(string name, MyGuiControlContentButton button)
        {
            if (!m_waitingForScreenshot.IsWaiting())
            {
                m_waitingForScreenshot.SetData_TakeScreenshotLocal(button);
                string pathToSave = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.GetCurrentLocalDirectory(), name, MyBlueprintUtils.THUMB_IMAGE_NAME);
                MyRenderProxy.TakeScreenshot(new Vector2(0.5f, 0.5f), pathToSave, false, true, false);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_ScreenBeingTaken_Caption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_ScreenBeingTaken), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void TogglePreviewVisibility()
        {
            foreach (MyGuiControlContentButton button in this.m_BPList.Controls)
            {
                if (button != null)
                {
                    button.SetPreviewVisibility(this.GetThumbnailVisibility());
                }
            }
            this.m_BPList.Recalculate();
        }

        private void TrySelectFirstBlueprint()
        {
            if (this.m_BPTypesGroup.Count > 0)
            {
                if (this.m_BPTypesGroup.SelectedIndex == null)
                {
                    this.m_BPTypesGroup.SelectByIndex(0);
                }
            }
            else
            {
                this.m_multiline.Clear();
                MyBlueprintTypeEnum selectedBlueprintType = this.GetSelectedBlueprintType();
                if ((selectedBlueprintType != MyBlueprintTypeEnum.STEAM) && (selectedBlueprintType != MyBlueprintTypeEnum.MIXED))
                {
                    this.m_multiline.AppendText(MyTexts.Get(MySpaceTexts.ScreenBlueprintsRew_NoBlueprints), "Blue", this.m_multiline.TextScale, VRageMath.Vector4.One);
                }
                else
                {
                    this.m_multiline.AppendText(MyTexts.Get((this.m_content == Content.Blueprint) ? MySpaceTexts.ScreenBlueprintsRew_NoWorkshopBlueprints : MySpaceTexts.ScreenBlueprintsRew_NoWorkshopScripts), "Blue", this.m_multiline.TextScale, VRageMath.Vector4.One);
                    this.m_multiline.AppendLine();
                    this.m_multiline.AppendLink((this.m_content == Content.Blueprint) ? MySteamConstants.URL_BROWSE_WORKSHOP_BLUEPRINTS : MySteamConstants.URL_BROWSE_WORKSHOP_INGAMESCRIPTS, "Space Engineers Steam Workshop");
                    this.m_multiline.AppendLine();
                    this.m_multiline.OnLinkClicked += new LinkClicked(this.OnLinkClicked);
                }
                this.m_multiline.ScrollbarOffsetV = 1f;
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_thumbnailImage.Visible)
            {
                this.UpdateThumbnailPosition();
            }
            if (Task.IsComplete && m_needsExtract)
            {
                this.GetWorkshopItemsSteam();
                m_needsExtract = false;
                this.RefreshBlueprintList(false);
            }
            if (this.m_wasPublished)
            {
                this.m_wasPublished = false;
                this.RefreshBlueprintList(true);
            }
            return base.Update(hasFocus);
        }

        public void UpdateDetailKeyEnable()
        {
            if (this.m_selectedBlueprint == null)
            {
                this.m_button_OpenInWorkshop.Enabled = false;
                this.m_button_CopyToClipboard.Enabled = false;
                this.m_button_Rename.Enabled = false;
                this.m_button_Replace.Enabled = false;
                this.m_button_Delete.Enabled = false;
                this.m_button_TakeScreenshot.Enabled = false;
                this.m_button_Publish.Enabled = false;
                this.m_sendToCombo.Enabled = false;
            }
            else
            {
                switch (this.m_selectedBlueprint.Type)
                {
                    case MyBlueprintTypeEnum.STEAM:
                        this.m_button_OpenInWorkshop.Enabled = true;
                        this.m_button_CopyToClipboard.Enabled = true;
                        this.m_button_Rename.Enabled = false;
                        this.m_button_Replace.Enabled = false;
                        this.m_button_Delete.Enabled = false;
                        this.m_button_TakeScreenshot.Enabled = false;
                        this.m_button_Publish.Enabled = false;
                        this.m_sendToCombo.Enabled = true;
                        return;

                    case MyBlueprintTypeEnum.LOCAL:
                        this.m_button_OpenInWorkshop.Enabled = false;
                        this.m_button_CopyToClipboard.Enabled = true;
                        this.m_button_Rename.Enabled = true;
                        this.m_button_Replace.Enabled = true;
                        this.m_button_Delete.Enabled = true;
                        this.m_button_TakeScreenshot.Enabled = true;
                        this.m_button_Publish.Enabled = MyFakes.ENABLE_WORKSHOP_PUBLISH;
                        this.m_sendToCombo.Enabled = false;
                        return;

                    case MyBlueprintTypeEnum.SHARED:
                        this.m_button_OpenInWorkshop.Enabled = false;
                        this.m_button_CopyToClipboard.Enabled = true;
                        this.m_button_Rename.Enabled = false;
                        this.m_button_Replace.Enabled = false;
                        this.m_button_Delete.Enabled = false;
                        this.m_button_TakeScreenshot.Enabled = false;
                        this.m_button_Publish.Enabled = false;
                        this.m_sendToCombo.Enabled = false;
                        return;

                    case MyBlueprintTypeEnum.DEFAULT:
                        this.m_button_OpenInWorkshop.Enabled = false;
                        this.m_button_CopyToClipboard.Enabled = true;
                        this.m_button_Rename.Enabled = false;
                        this.m_button_Replace.Enabled = false;
                        this.m_button_Delete.Enabled = false;
                        this.m_button_TakeScreenshot.Enabled = false;
                        this.m_button_Publish.Enabled = false;
                        this.m_sendToCombo.Enabled = false;
                        return;

                    case MyBlueprintTypeEnum.CLOUD:
                        this.m_button_OpenInWorkshop.Enabled = false;
                        this.m_button_CopyToClipboard.Enabled = true;
                        this.m_button_Rename.Enabled = false;
                        this.m_button_Replace.Enabled = true;
                        this.m_button_Delete.Enabled = true;
                        this.m_button_TakeScreenshot.Enabled = true;
                        this.m_button_Publish.Enabled = false;
                        this.m_sendToCombo.Enabled = false;
                        return;
                }
                this.m_button_OpenInWorkshop.Enabled = false;
                this.m_button_CopyToClipboard.Enabled = false;
                this.m_button_Rename.Enabled = false;
                this.m_button_Replace.Enabled = false;
                this.m_button_Delete.Enabled = false;
                this.m_button_TakeScreenshot.Enabled = false;
                this.m_button_Publish.Enabled = false;
                this.m_sendToCombo.Enabled = false;
            }
        }

        private void UpdateInfo(Stream sbcStream, MyBlueprintItemInfo data)
        {
            int num = 0;
            string str = string.Empty;
            string str2 = MyTexts.GetString(MySpaceTexts.ScreenBlueprintsRew_NotAvailable);
            MyBlueprintItemInfo selectedBlueprint = this.m_selectedBlueprint;
            MyGuiControlContentButton selectedButton = this.m_selectedButton;
            if (((data != null) && (this.m_selectedBlueprint != null)) && data.Equals(this.m_selectedBlueprint))
            {
                Content content = this.m_content;
                if (content != Content.Blueprint)
                {
                    if (content == Content.Script)
                    {
                        string text1;
                        if (data.Item == null)
                        {
                            text1 = "N/A";
                        }
                        else
                        {
                            text1 = data.Item.OwnerId.ToString();
                        }
                        str = text1;
                    }
                }
                else
                {
                    string text2;
                    string text3;
                    XDocument document1 = XDocument.Load(sbcStream);
                    IEnumerable<XElement> source = document1.Descendants("GridSizeEnum");
                    IEnumerable<XElement> enumerable2 = document1.Descendants("DisplayName");
                    IEnumerable<XElement> enumerable3 = document1.Descendants("CubeBlocks");
                    IEnumerable<XElement> enumerable4 = document1.Descendants("DLC");
                    if ((source == null) || (source.Count<XElement>() <= 0))
                    {
                        text2 = "N/A";
                    }
                    else
                    {
                        text2 = (string) source.First<XElement>();
                    }
                    str2 = text2;
                    if ((enumerable2 == null) || (enumerable2.Count<XElement>() <= 0))
                    {
                        text3 = "N/A";
                    }
                    else
                    {
                        text3 = (string) enumerable2.First<XElement>();
                    }
                    str = text3;
                    num = 0;
                    if ((enumerable3 != null) && (enumerable3.Count<XElement>() > 0))
                    {
                        foreach (XElement element in enumerable3)
                        {
                            num += element.Elements().Count<XElement>();
                        }
                    }
                    if (enumerable4 != null)
                    {
                        HashSet<uint> set = new HashSet<uint>();
                        foreach (XElement element2 in enumerable4)
                        {
                            MyDLCs.MyDLC ydlc;
                            if (string.IsNullOrEmpty(element2.Value))
                            {
                                continue;
                            }
                            if (MyDLCs.TryGetDLC(element2.Value, out ydlc))
                            {
                                set.Add(ydlc.AppId);
                            }
                        }
                        if (set.Count > 0)
                        {
                            selectedBlueprint.Data.DLCs = set.ToArray<uint>();
                        }
                    }
                }
            }
            if (ReferenceEquals(selectedBlueprint, this.m_selectedBlueprint) && ReferenceEquals(selectedButton, this.m_selectedButton))
            {
                this.m_detailDLC.Visible = false;
                foreach (MyGuiControlImage image in this.m_dlcIcons)
                {
                    this.Controls.Remove(image);
                }
                this.m_dlcIcons.Clear();
                if ((selectedBlueprint != null) && !selectedBlueprint.Data.DLCs.IsNullOrEmpty<uint>())
                {
                    this.m_detailDLC.Visible = true;
                    Vector2 vector = new Vector2(this.m_sendToCombo.Position.X, this.m_detailDLC.Position.Y);
                    foreach (uint num5 in selectedBlueprint.Data.DLCs)
                    {
                        MyDLCs.MyDLC ydlc2;
                        if (MyDLCs.TryGetDLC(num5, out ydlc2))
                        {
                            Vector2? position = null;
                            position = null;
                            VRageMath.Vector4? backgroundColor = null;
                            string[] textures = new string[] { ydlc2.Icon };
                            MyGuiControlImage image1 = new MyGuiControlImage(position, position, backgroundColor, null, textures, MyDLCs.GetRequiredDLCTooltip(num5), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                            image1.Size = new Vector2(32f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
                            MyGuiControlImage item = image1;
                            item.Position = vector;
                            vector += item.Size.X + 0.002f;
                            this.m_dlcIcons.Add(item);
                            this.Controls.Add(item);
                        }
                    }
                }
                this.m_detailBlockCountValue.Text = num.ToString();
                this.m_detailSizeValue.Text = str2;
                this.m_detailAuthorName.Text = str;
                this.m_detailSendTo.Text = MyTexts.GetString(MySpaceTexts.BlueprintInfo_SendTo);
                float x = (this.m_detailBlockCount.Position.X + Math.Max(Math.Max(this.m_detailBlockCount.Size.X, this.m_detailSize.Size.X), this.m_detailAuthor.Size.X)) + 0.001f;
                this.m_detailBlockCountValue.Position = new Vector2(x, this.m_detailBlockCount.Position.Y);
                this.m_detailSizeValue.Position = new Vector2(x, this.m_detailSize.Position.Y);
                this.m_detailAuthorName.Position = new Vector2(x, this.m_detailAuthor.Position.Y);
            }
            if (this.m_loadedPrefab != null)
            {
                this.UpdateDetailKeyEnable();
            }
        }

        private void UpdateNameAndDescription()
        {
            if (this.m_selectedBlueprint.Item == null)
            {
                this.m_detailName.Text = this.m_selectedBlueprint.Data.Name;
                StringBuilder builder = new StringBuilder(this.m_selectedBlueprint.Data.Description);
                if ((this.m_selectedBlueprint.Data.DLCs == null) || (this.m_selectedBlueprint.Data.DLCs.Length == 0))
                {
                    this.m_multiline.Text = builder;
                }
                else
                {
                    this.m_multiline.Text = builder;
                }
            }
            else
            {
                StringBuilder builder2 = new StringBuilder(this.m_selectedBlueprint.Item.Description);
                this.m_multiline.Text = builder2;
                string title = this.m_selectedBlueprint.Item.Title;
                if (title.Length > 80)
                {
                    title = title.Substring(0, 80);
                }
                this.m_detailName.Text = title;
            }
        }

        private bool UpdatePrefab(MyBlueprintItemInfo data, bool loadPrefab)
        {
            bool flag = true;
            this.m_loadedPrefab = null;
            if (data != null)
            {
                string str;
                switch (data.Type)
                {
                    case MyBlueprintTypeEnum.STEAM:
                        if ((data.PublishedItemId != null) && (data.Item != null))
                        {
                            flag = false;
                            MyWorkshopItem workshopData = data.Item;
                            Task = Parallel.Start(delegate {
                                if (!MyWorkshop.IsUpToDate(workshopData))
                                {
                                    this.DownloadBlueprintFromSteam(workshopData);
                                }
                                this.OnBlueprintDownloadedDetails(workshopData, loadPrefab);
                            }, delegate {
                            });
                        }
                        return flag;

                    case MyBlueprintTypeEnum.LOCAL:
                        str = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, GetCurrentLocalDirectory(Content.Blueprint), data.Data.Name, "bp.sbc");
                        if (File.Exists(str))
                        {
                            if (loadPrefab)
                            {
                                this.m_loadedPrefab = MyBlueprintUtils.LoadPrefab(str);
                            }
                            try
                            {
                                using (FileStream stream = new FileStream(str, FileMode.Open))
                                {
                                    this.UpdateNameAndDescription();
                                    this.UpdateInfo(stream, data);
                                    this.UpdateDetailKeyEnable();
                                }
                            }
                            catch (Exception exception1)
                            {
                                MyLog.Default.WriteLine($"Failed to open {str}.
Exception: {exception1.Message}");
                            }
                        }
                        return flag;

                    case MyBlueprintTypeEnum.DEFAULT:
                        break;

                    case MyBlueprintTypeEnum.CLOUD:
                    {
                        if (loadPrefab)
                        {
                            this.m_loadedPrefab = MyBlueprintUtils.LoadPrefabFromCloud(data);
                        }
                        byte[] buffer = MyGameService.LoadFromCloud(data.CloudPathXML);
                        if (buffer == null)
                        {
                            return flag;
                        }
                        else
                        {
                            using (MemoryStream stream2 = new MemoryStream(buffer))
                            {
                                this.UpdateNameAndDescription();
                                this.UpdateInfo(stream2.UnwrapGZip(), data);
                                this.UpdateDetailKeyEnable();
                                return flag;
                            }
                        }
                        break;
                    }
                    default:
                        return flag;
                }
                str = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, data.Data.Name, "bp.sbc");
                if (File.Exists(str))
                {
                    if (loadPrefab)
                    {
                        this.m_loadedPrefab = MyBlueprintUtils.LoadPrefab(str);
                    }
                    using (FileStream stream3 = new FileStream(str, FileMode.Open))
                    {
                        this.UpdateNameAndDescription();
                        this.UpdateInfo(stream3, data);
                        this.UpdateDetailKeyEnable();
                    }
                }
            }
            return flag;
        }

        private void UpdateThumbnailPosition()
        {
            Vector2 vector = (MyGuiManager.MouseCursorPosition + new Vector2(0.02f, 0.055f)) + this.m_thumbnailImage.Size;
            if ((vector.X > 1f) || (vector.Y > 1f))
            {
                this.m_thumbnailImage.Position = MyGuiManager.MouseCursorPosition + new Vector2((0.5f * this.m_thumbnailImage.Size.X) - 0.48f, (-0.5f * this.m_thumbnailImage.Size.Y) - 0.47f);
            }
            else
            {
                this.m_thumbnailImage.Position = (MyGuiManager.MouseCursorPosition + (0.5f * this.m_thumbnailImage.Size)) + new Vector2(-0.48f, -0.445f);
            }
        }

        private bool ValidateModInfo(MyObjectBuilder_ModInfo info) => 
            ((info != null) && (info.SubtypeName != null));

        private bool ValidateSelecteditem() => 
            ((this.m_selectedBlueprint != null) ? (this.m_selectedBlueprint.Data.Name != null) : false);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiBlueprintScreen_Reworked.<>c <>9 = new MyGuiBlueprintScreen_Reworked.<>c();
            public static Func<IMyEventOwner, Action<ulong, string, ulong, string>> <>9__144_0;
            public static Action <>9__159_2;
            public static Action <>9__159_0;
            public static Action <>9__177_1;
            public static Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> <>9__194_0;
            public static Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> <>9__194_1;
            public static Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> <>9__194_2;
            public static Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> <>9__194_3;
            public static Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> <>9__194_4;
            public static Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> <>9__194_5;
            public static Func<IMyEventOwner, Action<ulong, string, ulong, string>> <>9__195_0;

            internal void <OnPrefabLoaded>b__159_0()
            {
                MyClipboardComponent.Static.HandlePasteInput(true);
            }

            internal void <OnPrefabLoaded>b__159_2()
            {
                MyClipboardComponent.Static.HandlePasteInput(true);
            }

            internal Action<ulong, string, ulong, string> <OnSendToPlayer>b__144_0(IMyEventOwner x) => 
                new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen_Reworked.ShareBlueprintRequest);

            internal Action<ulong, string, ulong, string> <ShareBlueprintRequest>b__195_0(IMyEventOwner x) => 
                new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen_Reworked.ShareBlueprintRequestClient);

            internal int <SortBlueprints>b__194_0(MyBlueprintItemInfo x, MyBlueprintItemInfo y) => 
                x.BlueprintName.CompareTo(y.BlueprintName);

            internal int <SortBlueprints>b__194_1(MyBlueprintItemInfo x, MyBlueprintItemInfo y)
            {
                DateTime? timeUpdated = x.TimeUpdated;
                DateTime? nullable2 = y.TimeUpdated;
                if ((timeUpdated == null) || (nullable2 == null))
                {
                    return 0;
                }
                return (-1 * DateTime.Compare(timeUpdated.Value, nullable2.Value));
            }

            internal int <SortBlueprints>b__194_2(MyBlueprintItemInfo x, MyBlueprintItemInfo y)
            {
                DateTime? timeCreated = x.TimeCreated;
                DateTime? nullable2 = y.TimeCreated;
                if ((timeCreated == null) || (nullable2 == null))
                {
                    return 0;
                }
                return (-1 * DateTime.Compare(timeCreated.Value, nullable2.Value));
            }

            internal int <SortBlueprints>b__194_3(MyBlueprintItemInfo x, MyBlueprintItemInfo y) => 
                x.BlueprintName.CompareTo(y.BlueprintName);

            internal int <SortBlueprints>b__194_4(MyBlueprintItemInfo x, MyBlueprintItemInfo y)
            {
                DateTime timeCreated = x.Item.TimeCreated;
                DateTime time2 = y.Item.TimeCreated;
                return ((timeCreated >= time2) ? ((timeCreated <= time2) ? 0 : -1) : 1);
            }

            internal int <SortBlueprints>b__194_5(MyBlueprintItemInfo x, MyBlueprintItemInfo y)
            {
                DateTime timeUpdated = x.Item.TimeUpdated;
                DateTime time2 = y.Item.TimeUpdated;
                return ((timeUpdated >= time2) ? ((timeUpdated <= time2) ? 0 : -1) : 1);
            }

            internal void <UpdatePrefab>b__177_1()
            {
            }
        }

        private class MyWaitForScreenshotData
        {
            private bool m_isSet;

            public MyWaitForScreenshotData()
            {
                this.Clear();
            }

            public void Clear()
            {
                this.m_isSet = false;
                this.Option = MyGuiBlueprintScreen_Reworked.WaitForScreenshotOptions.None;
                this.UsedButton = null;
                this.PathRel = string.Empty;
                this.PathFull = string.Empty;
                this.UsedButton = null;
            }

            public bool IsWaiting() => 
                this.m_isSet;

            public bool SetData_CreateNewBlueprintCloud(string pathRel, string pathFull)
            {
                if (this.m_isSet)
                {
                    return false;
                }
                this.m_isSet = true;
                this.Option = MyGuiBlueprintScreen_Reworked.WaitForScreenshotOptions.CreateNewBlueprintCloud;
                this.PathRel = pathRel;
                this.PathFull = pathFull;
                return true;
            }

            public bool SetData_TakeScreenshotCloud(string pathRel, string pathFull, MyGuiControlContentButton button)
            {
                if (this.m_isSet)
                {
                    return false;
                }
                this.m_isSet = true;
                this.Option = MyGuiBlueprintScreen_Reworked.WaitForScreenshotOptions.TakeScreenshotCloud;
                this.PathRel = pathRel;
                this.PathFull = pathFull;
                this.UsedButton = button;
                return true;
            }

            public bool SetData_TakeScreenshotLocal(MyGuiControlContentButton button)
            {
                if (this.m_isSet)
                {
                    return false;
                }
                this.m_isSet = true;
                this.Option = MyGuiBlueprintScreen_Reworked.WaitForScreenshotOptions.TakeScreenshotLocal;
                this.UsedButton = button;
                return true;
            }

            public MyGuiBlueprintScreen_Reworked.WaitForScreenshotOptions Option { get; private set; }

            public MyGuiControlContentButton UsedButton { get; private set; }

            public MyObjectBuilder_Definitions Prefab { get; private set; }

            public string PathRel { get; private set; }

            public string PathFull { get; private set; }
        }

        private enum ScrollTestResult
        {
            Ok,
            Higher,
            Lower
        }

        public enum SortOption
        {
            None,
            Alphabetical,
            CreationDate,
            UpdateDate
        }

        private enum Tab
        {
            Info,
            Edit
        }

        private enum WaitForScreenshotOptions
        {
            None,
            TakeScreenshotLocal,
            CreateNewBlueprintCloud,
            TakeScreenshotCloud
        }
    }
}

