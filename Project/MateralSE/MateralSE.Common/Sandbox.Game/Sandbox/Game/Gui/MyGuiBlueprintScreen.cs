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
    using Sandbox.Game.SessionComponents.Clipboard;
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
    using VRage.Compression;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [StaticEventOwner]
    public class MyGuiBlueprintScreen : MyGuiBlueprintScreenBase
    {
        private readonly string m_thumbImageName;
        public static ParallelTasks.Task Task;
        private static bool m_downloadFromSteam = true;
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private static List<MyGuiControlListbox.Item> m_recievedBlueprints = new List<MyGuiControlListbox.Item>();
        private static bool m_needsExtract = false;
        public static List<MyWorkshopItem> m_subscribedItemsList = new List<MyWorkshopItem>();
        private Vector2 m_controlPadding;
        private float m_textScale;
        private MyGuiControlButton m_detailsButton;
        private MyGuiControlButton m_screenshotButton;
        private MyGuiControlButton m_replaceButton;
        private MyGuiControlButton m_deleteButton;
        private MyGuiControlButton m_okButton;
        private MyGuiControlCombobox m_sortCombobox;
        private MyGuiControlTextbox m_searchBox;
        private MyGuiControlButton m_searchClear;
        private static MyBlueprintSortingOptions m_sortBy = MyBlueprintSortingOptions.SortBy_None;
        private static MyGuiControlListbox m_blueprintList;
        private MyGuiDetailScreenBase m_detailScreen;
        private MyGuiControlImage m_thumbnailImage;
        private bool m_activeDetail;
        private MyGuiControlListbox.Item m_selectedItem;
        private MyGuiControlRotatingWheel m_wheel;
        private MyGridClipboard m_clipboard;
        private bool m_allowCopyToClipboard;
        private string m_selectedThumbnailPath;
        private bool m_blueprintBeingLoaded;
        private MyBlueprintAccessType m_accessType;
        private static string m_currentLocalDirectory;
        private static HashSet<ulong> m_downloadQueued;
        private static MyConcurrentHashSet<ulong> m_downloadFinished;
        private static string TEMP_PATH;
        private string[] filenames;
        private static LoadPrefabData m_LoadPrefabData;
        private List<string> m_preloadedTextures;
        private MyGuiControlListbox.Item m_previousItem;

        static MyGuiBlueprintScreen()
        {
            Vector2? position = null;
            m_blueprintList = new MyGuiControlListbox(position, MyGuiControlListboxStyleEnum.Blueprints);
            m_currentLocalDirectory = string.Empty;
            m_downloadQueued = new HashSet<ulong>();
            m_downloadFinished = new MyConcurrentHashSet<ulong>();
            TEMP_PATH = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, "temp");
        }

        public MyGuiBlueprintScreen(MyGridClipboard clipboard, bool allowCopyToClipboard, MyBlueprintAccessType accessType) : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), SCREEN_SIZE, MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity, false)
        {
            this.m_thumbImageName = "thumb.png";
            this.m_controlPadding = new Vector2(0.02f, 0.02f);
            this.m_textScale = 0.8f;
            this.m_preloadedTextures = new List<string>();
            this.m_accessType = accessType;
            this.m_clipboard = clipboard;
            this.m_allowCopyToClipboard = allowCopyToClipboard;
            if (!Directory.Exists(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL))
            {
                Directory.CreateDirectory(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL);
            }
            if (!Directory.Exists(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP))
            {
                Directory.CreateDirectory(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP);
            }
            m_blueprintList.Items.Clear();
            CheckCurrentLocalDirectory();
            this.GetLocalBlueprintNames(m_downloadFromSteam);
            if (m_downloadFromSteam)
            {
                m_downloadFromSteam = false;
            }
            this.CreateTempDirectory();
            this.RecreateControls(true);
            m_blueprintList.ItemsSelected += new Action<MyGuiControlListbox>(this.OnSelectItem);
            m_blueprintList.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnItemDoubleClick);
            m_blueprintList.ItemMouseOver += new Action<MyGuiControlListbox>(this.OnMouseOverItem);
            base.OnEnterCallback = (Action) Delegate.Combine(base.OnEnterCallback, new Action(this.Ok));
            this.m_searchBox.TextChanged += new Action<MyGuiControlTextbox>(this.OnSearchTextChange);
            if (MyGuiScreenHudSpace.Static != null)
            {
                MyGuiScreenHudSpace.Static.HideScreen();
            }
        }

        private static bool CheckBlueprintForMods(MyObjectBuilder_Definitions prefab)
        {
            bool flag;
            if (prefab.ShipBlueprints == null)
            {
                return true;
            }
            MyObjectBuilder_CubeGrid[] cubeGrids = prefab.ShipBlueprints[0].CubeGrids;
            if (cubeGrids == null)
            {
                goto TR_0001;
            }
            else if (cubeGrids.Length != 0)
            {
                MyObjectBuilder_CubeGrid[] gridArray2 = cubeGrids;
                for (int i = 0; i < gridArray2.Length; i++)
                {
                    using (List<MyObjectBuilder_CubeBlock>.Enumerator enumerator = gridArray2[i].CubeBlocks.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyDefinitionId defId = enumerator.Current.GetId();
                            MyCubeBlockDefinition blockDefinition = null;
                            if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out blockDefinition))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                goto TR_0001;
            }
            return flag;
        TR_0001:
            return true;
        }

        private static void CheckCurrentLocalDirectory()
        {
            if (!Directory.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory)))
            {
                m_currentLocalDirectory = string.Empty;
            }
        }

        public override bool CloseScreen()
        {
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
            if (setOwner)
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
                MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                switch (userData.Type)
                {
                    case MyBlueprintTypeEnum.STEAM:
                    {
                        ulong? publishedItemId = userData.PublishedItemId;
                        path = userData.Item.Folder;
                        if (File.Exists(path) || MyFileSystem.IsDirectory(path))
                        {
                            m_LoadPrefabData = new LoadPrefabData(prefab, path, this, publishedItemId);
                            Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadWorkshopPrefab), null, m_LoadPrefabData);
                        }
                        break;
                    }
                    case MyBlueprintTypeEnum.LOCAL:
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, this.m_selectedItem.Text.ToString(), "bp.sbc");
                        if (File.Exists(path))
                        {
                            nullable2 = null;
                            m_LoadPrefabData = new LoadPrefabData(prefab, path, this, nullable2);
                            Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefab), null, m_LoadPrefabData);
                        }
                        break;

                    case MyBlueprintTypeEnum.SHARED:
                        return false;

                    case MyBlueprintTypeEnum.DEFAULT:
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, this.m_selectedItem.Text.ToString(), "bp.sbc");
                        if (File.Exists(path))
                        {
                            nullable2 = null;
                            m_LoadPrefabData = new LoadPrefabData(prefab, path, this, nullable2);
                            Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefab), null, m_LoadPrefabData);
                        }
                        break;

                    case MyBlueprintTypeEnum.CLOUD:
                        m_LoadPrefabData = new LoadPrefabData(prefab, userData, this);
                        Task = Parallel.Start(new Action<WorkData>(m_LoadPrefabData.CallLoadPrefabFromCloud), null, m_LoadPrefabData);
                        break;

                    default:
                        break;
                }
            }
            return false;
        }

        private void CreateButtons()
        {
            // Unresolved stack state at '000002FD'
        }

        public void CreateFromClipboard(bool withScreenshot = false, bool replace = false)
        {
            if (this.m_clipboard.CopiedGridsName != null)
            {
                string str = MyUtils.StripInvalidChars(this.m_clipboard.CopiedGridsName);
                string str2 = str;
                string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, str);
                for (int i = 1; MyFileSystem.DirectoryExists(path); i++)
                {
                    str2 = str + "_" + i;
                    path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, str2);
                }
                Path.Combine(path, this.m_thumbImageName);
                if (withScreenshot && !MySandboxGame.Config.EnableSteamCloud)
                {
                    this.TakeScreenshot(str2);
                }
                MyObjectBuilder_ShipBlueprintDefinition definition = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ShipBlueprintDefinition>();
                definition.Id = (SerializableDefinitionId) new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_ShipBlueprintDefinition)), MyUtils.StripInvalidChars(str));
                definition.CubeGrids = this.m_clipboard.CopiedGrids.ToArray();
                definition.RespawnShip = false;
                definition.DisplayName = MyGameService.UserName;
                definition.OwnerSteamId = Sync.MyId;
                definition.CubeGrids[0].DisplayName = str;
                MyObjectBuilder_Definitions prefab = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
                prefab.ShipBlueprints = new MyObjectBuilder_ShipBlueprintDefinition[] { definition };
                MyBlueprintUtils.SavePrefabToFile(prefab, this.m_clipboard.CopiedGridsName, m_currentLocalDirectory, replace, MyBlueprintTypeEnum.LOCAL);
                this.RefreshBlueprintList(false);
            }
        }

        private DirectoryInfo CreateTempDirectory() => 
            Directory.CreateDirectory(TEMP_PATH);

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
            m_subscribedItemsList.Clear();
            bool subscribedBlueprintsBlocking = MyWorkshop.GetSubscribedBlueprintsBlocking(m_subscribedItemsList);
            if (subscribedBlueprintsBlocking)
            {
                Directory.CreateDirectory(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP);
                foreach (MyWorkshopItem item in m_subscribedItemsList)
                {
                    ulong id = item.Id;
                    if (File.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, id.ToString() + MyBlueprintUtils.BLUEPRINT_WORKSHOP_EXTENSION)))
                    {
                        m_downloadFinished.Add(item.Id);
                        continue;
                    }
                    this.DownloadBlueprintFromSteam(item);
                    m_downloadFinished.Add(item.Id);
                }
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

        private void ExtractWorkshopItem(MyWorkshopItem subItem)
        {
            if (MyFileSystem.IsDirectory(subItem.Folder))
            {
                MyObjectBuilder_ModInfo info1 = new MyObjectBuilder_ModInfo();
                info1.SubtypeName = subItem.Title;
                info1.WorkshopId = subItem.Id;
                info1.SteamIDOwner = subItem.OwnerId;
            }
            else
            {
                try
                {
                    string path = Path.Combine(TEMP_PATH, subItem.Id.ToString());
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    Directory.CreateDirectory(path);
                    MyZipArchive archive = MyZipArchive.OpenOnFile(subItem.Folder, FileMode.Open, FileAccess.Read, FileShare.Read, false);
                    MyObjectBuilder_ModInfo objectBuilder = new MyObjectBuilder_ModInfo {
                        SubtypeName = subItem.Title,
                        WorkshopId = subItem.Id,
                        SteamIDOwner = subItem.OwnerId
                    };
                    string str2 = Path.Combine(TEMP_PATH, subItem.Id.ToString(), "info.temp");
                    if (File.Exists(str2))
                    {
                        File.Delete(str2);
                    }
                    MyObjectBuilderSerializer.SerializeXML(str2, false, objectBuilder, null);
                    if (archive.FileExists(this.m_thumbImageName))
                    {
                        Stream stream = archive.GetFile(this.m_thumbImageName).GetStream(FileMode.Open, FileAccess.Read);
                        if (stream != null)
                        {
                            using (FileStream stream2 = File.Create(Path.Combine(path, this.m_thumbImageName)))
                            {
                                stream.CopyTo(stream2);
                            }
                        }
                        stream.Close();
                    }
                    archive.Dispose();
                }
                catch (IOException exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
            MyBlueprintItemInfo userData = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(subItem.Id));
            MyGuiControlListbox.Item listItem = new MyGuiControlListbox.Item(new StringBuilder(subItem.Title), subItem.Title, MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Normal, userData, null);
            if (m_blueprintList.Items.FindIndex(delegate (MyGuiControlListbox.Item item) {
                ulong? publishedItemId = (item.UserData as MyBlueprintItemInfo).PublishedItemId;
                ulong? nullable2 = (listItem.UserData as MyBlueprintItemInfo).PublishedItemId;
                return ((publishedItemId.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((publishedItemId != null) == (nullable2 != null))) && ((item.UserData as MyBlueprintItemInfo).Type == MyBlueprintTypeEnum.STEAM);
            }) == -1)
            {
                int? position = null;
                m_blueprintList.Add(listItem, position);
            }
        }

        private void GetBlueprints(string directory, MyBlueprintTypeEnum type)
        {
            List<MyGuiControlListbox.Item> list = new List<MyGuiControlListbox.Item>();
            List<MyGuiControlListbox.Item> list2 = new List<MyGuiControlListbox.Item>();
            if (Directory.Exists(directory))
            {
                ulong? nullable;
                int? nullable2;
                List<string> list3 = new List<string>();
                List<string> list4 = new List<string>();
                foreach (string str in Directory.GetDirectories(directory))
                {
                    list3.Add(str + @"\bp.sbc");
                    char[] separator = new char[] { '\\' };
                    string[] strArray2 = str.Split(separator);
                    list4.Add(strArray2[strArray2.Length - 1]);
                }
                for (int i = 0; i < list4.Count; i++)
                {
                    string toolTip = list4[i];
                    string path = list3[i];
                    nullable = null;
                    MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(type, nullable);
                    info1.TimeCreated = new DateTime?(File.GetCreationTimeUtc(path));
                    info1.TimeUpdated = new DateTime?(File.GetLastWriteTimeUtc(path));
                    info1.BlueprintName = toolTip;
                    MyBlueprintItemInfo userData = info1;
                    string icon = string.Empty;
                    if (File.Exists(path))
                    {
                        icon = MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL.Normal;
                    }
                    else
                    {
                        userData.IsDirectory = true;
                        icon = MyGuiConstants.TEXTURE_ICON_MODS_LOCAL.Normal;
                    }
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(toolTip), toolTip, icon, userData, null);
                    if (userData.IsDirectory)
                    {
                        list2.Add(item);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
                if (!string.IsNullOrEmpty(m_currentLocalDirectory))
                {
                    nullable = null;
                    MyBlueprintItemInfo info3 = new MyBlueprintItemInfo(type, nullable);
                    info3.TimeCreated = new DateTime?(DateTime.Now);
                    info3.TimeUpdated = new DateTime?(DateTime.Now);
                    info3.BlueprintName = string.Empty;
                    info3.IsDirectory = true;
                    MyBlueprintItemInfo info2 = info3;
                    object userData = info2;
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder("[..]"), m_currentLocalDirectory, MyGuiConstants.TEXTURE_ICON_MODS_LOCAL.Normal, userData, null);
                    nullable2 = null;
                    m_blueprintList.Add(item, nullable2);
                }
                this.SortBlueprints(list2, MyBlueprintTypeEnum.LOCAL);
                foreach (MyGuiControlListbox.Item item3 in list2)
                {
                    nullable2 = null;
                    m_blueprintList.Add(item3, nullable2);
                }
                this.SortBlueprints(list, MyBlueprintTypeEnum.LOCAL);
                foreach (MyGuiControlListbox.Item item4 in list)
                {
                    nullable2 = null;
                    m_blueprintList.Add(item4, nullable2);
                }
            }
        }

        private void GetBlueprintsFromCloud()
        {
            List<MyCloudFileInfo> cloudFiles = MyGameService.GetCloudFiles(MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY);
            if (cloudFiles != null)
            {
                List<MyGuiControlListbox.Item> list = new List<MyGuiControlListbox.Item>();
                Dictionary<string, MyBlueprintItemInfo> dictionary = new Dictionary<string, MyBlueprintItemInfo>();
                foreach (MyCloudFileInfo info in cloudFiles)
                {
                    char[] separator = new char[] { '/' };
                    string[] textArray1 = info.Name.Split(separator);
                    string key = textArray1[textArray1.Length - 2];
                    MyBlueprintItemInfo info2 = null;
                    if (!dictionary.TryGetValue(key, out info2))
                    {
                        ulong? id = null;
                        MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.CLOUD, id);
                        info1.TimeCreated = new DateTime?(DateTime.FromFileTimeUtc(info.Timestamp));
                        info1.TimeUpdated = new DateTime?(DateTime.FromFileTimeUtc(info.Timestamp));
                        info1.BlueprintName = key;
                        info1.CloudInfo = info;
                        info2 = info1;
                        object userData = info2;
                        dictionary.Add(key, info2);
                        list.Add(new MyGuiControlListbox.Item(new StringBuilder(key), info.Name, MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_CLOUD.Normal, userData, null));
                    }
                    if (info.Name.EndsWith(MyObjectBuilderSerializer.ProtobufferExtension))
                    {
                        info2.CloudPathPB = info.Name;
                    }
                    else if (info.Name.EndsWith(MyBlueprintUtils.BLUEPRINT_LOCAL_NAME))
                    {
                        info2.CloudPathXML = info.Name;
                    }
                }
                this.SortBlueprints(list, MyBlueprintTypeEnum.CLOUD);
                foreach (MyGuiControlListbox.Item item2 in list)
                {
                    int? position = null;
                    m_blueprintList.Add(item2, position);
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyBlueprintScreen";

        private void GetLocalBlueprintNames(bool reload = false)
        {
            string directory = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory);
            this.GetBlueprints(directory, MyBlueprintTypeEnum.LOCAL);
            if (MySandboxGame.Config.EnableSteamCloud)
            {
                this.GetBlueprintsFromCloud();
            }
            if (Task.IsComplete)
            {
                if (reload)
                {
                    this.GetWorkshopBlueprints();
                }
                else
                {
                    this.GetWorkshopItemsSteam();
                }
            }
            foreach (MyGuiControlListbox.Item item in m_recievedBlueprints)
            {
                int? position = null;
                m_blueprintList.Add(item, position);
            }
            if (MyFakes.ENABLE_DEFAULT_BLUEPRINTS)
            {
                this.GetBlueprints(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, MyBlueprintTypeEnum.DEFAULT);
            }
        }

        private void GetWorkshopBlueprints()
        {
            Task = Parallel.Start(new Action(this.DownloadBlueprints));
        }

        private void GetWorkshopItemsLocal()
        {
            string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, "temp");
            if (Directory.Exists(path))
            {
                List<string> list = new List<string>();
                List<string> list1 = new List<string>();
                string[] directories = Directory.GetDirectories(path);
                int index = 0;
                while (true)
                {
                    if (index >= directories.Length)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string str2 = Path.Combine(path, list[i], "info.temp");
                            MyObjectBuilder_ModInfo objectBuilder = null;
                            if (File.Exists(str2))
                            {
                                MyAnalyticsHelper.ReportActivityStart(null, "show_blueprints", string.Empty, "gui", string.Empty, true);
                                bool flag = MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_ModInfo>(str2, out objectBuilder);
                                if (this.ValidateModInfo(objectBuilder) && flag)
                                {
                                    string subtypeName = objectBuilder.SubtypeName;
                                    object userData = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(objectBuilder.WorkshopId));
                                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(subtypeName), subtypeName, MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Normal, userData, null);
                                    int? position = null;
                                    m_blueprintList.Add(item, position);
                                }
                            }
                        }
                        break;
                    }
                    char[] separator = new char[] { '\\' };
                    string[] strArray2 = directories[index].Split(separator);
                    list.Add(strArray2[strArray2.Length - 1]);
                    index++;
                }
            }
        }

        private void GetWorkshopItemsSteam()
        {
            List<MyGuiControlListbox.Item> list = new List<MyGuiControlListbox.Item>();
            for (int i = 0; i < m_subscribedItemsList.Count; i++)
            {
                MyWorkshopItem item = m_subscribedItemsList[i];
                MyAnalyticsHelper.ReportActivityStart(null, "show_blueprints", string.Empty, "gui", string.Empty, true);
                string title = item.Title;
                MyBlueprintItemInfo info1 = new MyBlueprintItemInfo(MyBlueprintTypeEnum.STEAM, new ulong?(item.Id));
                info1.Item = item;
                info1.BlueprintName = title;
                MyBlueprintItemInfo info = info1;
                object userData = info;
                MyGuiControlListbox.Item item2 = new MyGuiControlListbox.Item(new StringBuilder(title), title, MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Normal, userData, null);
                list.Add(item2);
            }
            this.SortBlueprints(list, MyBlueprintTypeEnum.STEAM);
            foreach (MyGuiControlListbox.Item item3 in list)
            {
                int? position = null;
                m_blueprintList.Add(item3, position);
            }
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((MyInput.Static.IsNewKeyPressed(MyKeys.F12) || MyInput.Static.IsNewKeyPressed(MyKeys.F11)) || MyInput.Static.IsNewKeyPressed(MyKeys.F10))
            {
                this.CloseScreen();
            }
        }

        private bool IsExtracted(MyWorkshopItem subItem) => 
            Directory.Exists(Path.Combine(TEMP_PATH, subItem.Id.ToString()));

        private void Ok()
        {
            if (this.m_selectedItem == null)
            {
                this.CloseScreen();
            }
            else
            {
                MyBlueprintItemInfo itemInfo = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                if (itemInfo.IsDirectory)
                {
                    if (!string.IsNullOrEmpty(itemInfo.BlueprintName))
                    {
                        m_currentLocalDirectory = Path.Combine(m_currentLocalDirectory, itemInfo.BlueprintName);
                    }
                    else
                    {
                        char[] separator = new char[] { Path.DirectorySeparatorChar };
                        string[] paths = m_currentLocalDirectory.Split(separator);
                        if (paths.Length <= 1)
                        {
                            m_currentLocalDirectory = string.Empty;
                        }
                        else
                        {
                            paths[paths.Length - 1] = string.Empty;
                            m_currentLocalDirectory = Path.Combine(paths);
                        }
                    }
                    CheckCurrentLocalDirectory();
                    this.RefreshAndReloadBlueprintList();
                }
                else
                {
                    this.m_blueprintBeingLoaded = true;
                    switch (itemInfo.Type)
                    {
                        case MyBlueprintTypeEnum.STEAM:
                            Task = Parallel.Start(delegate {
                                if (!MyWorkshop.IsUpToDate(itemInfo.Item))
                                {
                                    this.DownloadBlueprintFromSteam(itemInfo.Item);
                                }
                            }, () => this.CopyBlueprintAndClose());
                            return;

                        case MyBlueprintTypeEnum.LOCAL:
                        case MyBlueprintTypeEnum.DEFAULT:
                        case MyBlueprintTypeEnum.CLOUD:
                            this.CopyBlueprintAndClose();
                            return;

                        case MyBlueprintTypeEnum.SHARED:
                            this.OpenSharedBlueprint(itemInfo);
                            return;
                    }
                }
            }
        }

        private void OnBlueprintDownloadedDetails(MyWorkshopItem workshopDetails)
        {
            if (File.Exists(workshopDetails.Folder))
            {
                this.m_thumbnailImage.Visible = false;
                this.m_detailScreen = new MyGuiDetailScreenSteam(delegate (MyGuiControlListbox.Item item) {
                    this.m_selectedItem = item;
                    this.m_activeDetail = false;
                    this.m_detailScreen = null;
                    if (Task.IsComplete)
                    {
                        this.RefreshBlueprintList(false);
                    }
                }, this.m_selectedItem, this, this.m_selectedThumbnailPath, this.m_textScale);
                this.m_activeDetail = true;
                MyScreenManager.InputToNonFocusedScreens = true;
                MyScreenManager.AddScreen(this.m_detailScreen);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Error);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.CannotFindBlueprint), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnBlueprintDownloadedThumbnail(MyWorkshopItem item)
        {
            this.m_okButton.Enabled = true;
            this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltip);
            m_blueprintList.Enabled = true;
            string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, "temp", item.Id.ToString(), this.m_thumbImageName);
            if (!File.Exists(path))
            {
                this.m_thumbnailImage.Visible = false;
                this.m_thumbnailImage.SetTexture(null);
            }
            else
            {
                this.m_thumbnailImage.SetTexture(path);
                if (!this.m_activeDetail && this.m_thumbnailImage.IsAnyTextureValid())
                {
                    this.m_thumbnailImage.Visible = true;
                }
            }
            m_downloadQueued.Remove(item.Id);
            m_downloadFinished.Add(item.Id);
        }

        private void OnCancel(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyAnalyticsHelper.ReportActivityEnd(null, "show_blueprints");
            if (this.m_activeDetail)
            {
                this.m_detailScreen.CloseScreen();
            }
        }

        private void OnCreate(MyGuiControlButton button)
        {
            this.CreateFromClipboard(false, false);
        }

        private void OnDelete(MyGuiControlButton button)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Delete);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.DeleteBlueprintQuestion), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if ((callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES) && (this.m_selectedItem != null))
                {
                    MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                    if (userData != null)
                    {
                        switch (userData.Type)
                        {
                            case MyBlueprintTypeEnum.LOCAL:
                            case MyBlueprintTypeEnum.DEFAULT:
                            {
                                string file = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, m_currentLocalDirectory, userData.BlueprintName);
                                if (base.DeleteItem(file))
                                {
                                    this.ResetBlueprintUI();
                                }
                                break;
                            }
                            case MyBlueprintTypeEnum.CLOUD:
                            {
                                string[] textArray1 = new string[] { MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY, "/", userData.BlueprintName, "/", MyBlueprintUtils.BLUEPRINT_LOCAL_NAME };
                                string fileName = string.Concat(textArray1);
                                if (MyGameService.DeleteFromCloud(fileName))
                                {
                                    this.ResetBlueprintUI();
                                    MyGameService.DeleteFromCloud(fileName + MyObjectBuilderSerializer.ProtobufferExtension);
                                }
                                break;
                            }
                            default:
                                break;
                        }
                        this.RefreshBlueprintList(false);
                    }
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
                MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                if (userData != null)
                {
                    switch (userData.Type)
                    {
                        case MyBlueprintTypeEnum.STEAM:
                            this.OpenSteamWorkshopDetail(userData);
                            return;

                        case MyBlueprintTypeEnum.LOCAL:
                            this.OpenLocalBlueprintDetail();
                            return;

                        case MyBlueprintTypeEnum.SHARED:
                            break;

                        case MyBlueprintTypeEnum.DEFAULT:
                            this.OpenDefaultBlueprintDetail();
                            return;

                        case MyBlueprintTypeEnum.CLOUD:
                            this.OpenCloudBlueprintDetail();
                            break;

                        default:
                            return;
                    }
                }
            }
        }

        private void OnItemDoubleClick(MyGuiControlListbox list)
        {
            this.m_selectedItem = list.SelectedItems[0];
            this.Ok();
        }

        private void OnMouseOverItem(MyGuiControlListbox listBox)
        {
            MyGuiControlListbox.Item mouseOverItem = listBox.MouseOverItem;
            if (!ReferenceEquals(this.m_previousItem, mouseOverItem))
            {
                this.m_previousItem = mouseOverItem;
                if (mouseOverItem == null)
                {
                    this.m_thumbnailImage.Visible = false;
                }
                else
                {
                    string path = string.Empty;
                    MyBlueprintItemInfo userData = mouseOverItem.UserData as MyBlueprintItemInfo;
                    if (userData.Type == MyBlueprintTypeEnum.LOCAL)
                    {
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, mouseOverItem.Text.ToString(), this.m_thumbImageName);
                    }
                    else if (userData.Type != MyBlueprintTypeEnum.STEAM)
                    {
                        if (userData.Type == MyBlueprintTypeEnum.DEFAULT)
                        {
                            path = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, mouseOverItem.Text.ToString(), this.m_thumbImageName);
                        }
                    }
                    else
                    {
                        ulong? publishedItemId = userData.PublishedItemId;
                        if ((publishedItemId != null) && (userData.Item != null))
                        {
                            bool flag = false;
                            if (!MyFileSystem.IsDirectory(userData.Item.Folder))
                            {
                                path = Path.Combine(TEMP_PATH, publishedItemId.ToString(), this.m_thumbImageName);
                            }
                            else
                            {
                                path = Path.Combine(userData.Item.Folder, this.m_thumbImageName);
                                flag = true;
                            }
                            bool flag2 = m_downloadFinished.Contains(userData.Item.Id);
                            MyWorkshopItem worshopData = userData.Item;
                            if ((flag2 && !this.IsExtracted(worshopData)) && !flag)
                            {
                                m_blueprintList.Enabled = false;
                                this.m_okButton.Enabled = false;
                                this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
                                this.ExtractWorkshopItem(worshopData);
                                m_blueprintList.Enabled = true;
                                this.m_okButton.Enabled = true;
                                this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltip);
                            }
                            if (!m_downloadQueued.Contains(userData.Item.Id) && !flag2)
                            {
                                m_blueprintList.Enabled = false;
                                this.m_okButton.Enabled = false;
                                m_downloadQueued.Add(userData.Item.Id);
                                Task = Parallel.Start((Action) (() => this.DownloadBlueprintFromSteam(worshopData)), (Action) (() => this.OnBlueprintDownloadedThumbnail(worshopData)));
                                path = string.Empty;
                            }
                        }
                    }
                    if (!File.Exists(path))
                    {
                        this.m_thumbnailImage.Visible = false;
                        this.m_thumbnailImage.SetTexture(null);
                    }
                    else
                    {
                        this.m_preloadedTextures.Clear();
                        this.m_preloadedTextures.Add(path);
                        MyRenderProxy.PreloadTextures(this.m_preloadedTextures, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
                        this.m_thumbnailImage.SetTexture(path);
                        if (!this.m_activeDetail && this.m_thumbnailImage.IsAnyTextureValid())
                        {
                            this.m_thumbnailImage.Visible = true;
                        }
                    }
                }
            }
        }

        private void OnOk(MyGuiControlButton button)
        {
            button.Enabled = false;
            this.Ok();
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
                if (!CheckBlueprintForMods(prefab))
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
                        if (result == MyGuiScreenMessageBox.ResultEnum.NO)
                        {
                            this.m_selectedItem = m_blueprintList.SelectedItems[0];
                            this.m_okButton.Enabled = true;
                            this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltip);
                        }
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
            this.m_selectedItem = null;
            this.m_detailsButton.Enabled = false;
            this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            this.m_screenshotButton.Enabled = false;
            this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
            m_downloadFinished.Clear();
            m_downloadQueued.Clear();
            this.RefreshAndReloadBlueprintList();
        }

        private void OnReplace(MyGuiControlButton button)
        {
            if (this.m_selectedItem != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.BlueprintsMessageBoxTitle_Replace);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.BlueprintsMessageBoxDesc_Replace), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                    if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        string str = this.m_selectedItem.Text.ToString();
                        string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, str, "bp.sbc");
                        if (File.Exists(path))
                        {
                            this.m_clipboard.CopiedGrids[0].DisplayName = str;
                            MyObjectBuilder_Definitions prefab = MyBlueprintUtils.LoadPrefab(path);
                            prefab.ShipBlueprints[0].CubeGrids = this.m_clipboard.CopiedGrids.ToArray();
                            MyBlueprintUtils.SavePrefabToFile(prefab, this.m_clipboard.CopiedGridsName, m_currentLocalDirectory, true, MyBlueprintTypeEnum.LOCAL);
                            this.RefreshBlueprintList(false);
                        }
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnScreenshot(MyGuiControlButton button)
        {
            if (this.m_selectedItem != null)
            {
                this.TakeScreenshot(this.m_selectedItem.Text.ToString());
            }
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
                foreach (MyGuiControlListbox.Item item in m_blueprintList.Items)
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
                using (ObservableCollection<MyGuiControlListbox.Item>.Enumerator enumerator = m_blueprintList.Items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Visible = true;
                    }
                }
            }
            m_blueprintList.ScrollToolbarToTop();
        }

        private void OnSelectItem(MyGuiControlListbox list)
        {
            if (list.SelectedItems.Count != 0)
            {
                this.m_selectedItem = list.SelectedItems[0];
                this.m_okButton.Enabled = true;
                this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltip);
                this.m_detailsButton.Enabled = true;
                this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_DetailsTooltip);
                this.m_screenshotButton.Enabled = true;
                this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltip);
                this.m_replaceButton.Enabled = this.m_clipboard.HasCopiedGrids();
                this.m_replaceButton.SetToolTip(this.m_replaceButton.Enabled ? MyCommonTexts.Blueprints_ReplaceBlueprintTooltip : MyCommonTexts.Blueprints_CreateTooltipDisabled);
                MyBlueprintItemInfo userData = this.m_selectedItem.UserData as MyBlueprintItemInfo;
                ulong? publishedItemId = userData.PublishedItemId;
                string path = "";
                switch (userData.Type)
                {
                    case MyBlueprintTypeEnum.STEAM:
                        if (userData.Item == null)
                        {
                            return;
                        }
                        path = !MyFileSystem.IsDirectory(userData.Item.Folder) ? Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, "temp", publishedItemId.ToString(), this.m_thumbImageName) : Path.Combine(userData.Item.Folder, this.m_thumbImageName);
                        this.m_screenshotButton.Enabled = false;
                        this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
                        this.m_replaceButton.Enabled = false;
                        this.m_deleteButton.Enabled = false;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
                        break;

                    case MyBlueprintTypeEnum.LOCAL:
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, this.m_selectedItem.Text.ToString(), this.m_thumbImageName);
                        this.m_deleteButton.Enabled = true;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltip);
                        if (userData.IsDirectory)
                        {
                            this.m_detailsButton.Enabled = false;
                            this.m_screenshotButton.Enabled = false;
                            this.m_replaceButton.Enabled = false;
                        }
                        break;

                    case MyBlueprintTypeEnum.SHARED:
                        this.m_replaceButton.Enabled = false;
                        this.m_screenshotButton.Enabled = false;
                        this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
                        this.m_detailsButton.Enabled = false;
                        this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
                        this.m_deleteButton.Enabled = false;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
                        break;

                    case MyBlueprintTypeEnum.DEFAULT:
                        path = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, this.m_selectedItem.Text.ToString(), this.m_thumbImageName);
                        this.m_replaceButton.Enabled = false;
                        this.m_screenshotButton.Enabled = false;
                        this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
                        this.m_deleteButton.Enabled = false;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
                        break;

                    case MyBlueprintTypeEnum.CLOUD:
                        this.m_deleteButton.Enabled = true;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltip);
                        this.m_detailsButton.Enabled = true;
                        this.m_screenshotButton.Enabled = false;
                        break;

                    default:
                        break;
                }
                if (File.Exists(path))
                {
                    this.m_selectedThumbnailPath = path;
                }
                else
                {
                    this.m_selectedThumbnailPath = null;
                }
            }
        }

        private void OpenCloudBlueprintDetail()
        {
            this.m_thumbnailImage.Visible = false;
            this.m_detailScreen = new MyGuiDetailScreenCloud(delegate (MyGuiControlListbox.Item item) {
                if (item == null)
                {
                    this.m_screenshotButton.Enabled = false;
                    this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
                    this.m_detailsButton.Enabled = false;
                    this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
                    this.m_replaceButton.Enabled = false;
                    this.m_deleteButton.Enabled = false;
                    this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
                }
                this.m_selectedItem = item;
                this.m_activeDetail = false;
                this.m_detailScreen = null;
                if (Task.IsComplete)
                {
                    this.RefreshBlueprintList(false);
                }
            }, this.m_selectedItem, this, this.m_selectedThumbnailPath, this.m_textScale);
            this.m_activeDetail = true;
            MyScreenManager.InputToNonFocusedScreens = true;
            MyScreenManager.AddScreen(this.m_detailScreen);
        }

        private void OpenDefaultBlueprintDetail()
        {
            if (File.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, this.m_selectedItem.Text.ToString(), "bp.sbc")))
            {
                this.m_thumbnailImage.Visible = false;
                this.m_detailScreen = new MyGuiDetailScreenDefault(delegate (MyGuiControlListbox.Item item) {
                    if (item == null)
                    {
                        this.m_screenshotButton.Enabled = false;
                        this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
                        this.m_detailsButton.Enabled = false;
                        this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
                        this.m_replaceButton.Enabled = false;
                        this.m_deleteButton.Enabled = false;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
                    }
                    this.m_selectedItem = item;
                    this.m_activeDetail = false;
                    this.m_detailScreen = null;
                    if (Task.IsComplete)
                    {
                        this.RefreshBlueprintList(false);
                    }
                }, this.m_selectedItem, this, this.m_selectedThumbnailPath, this.m_textScale);
                this.m_activeDetail = true;
                MyScreenManager.InputToNonFocusedScreens = true;
                MyScreenManager.AddScreen(this.m_detailScreen);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Error);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.CannotFindBlueprint), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OpenLocalBlueprintDetail()
        {
            if (File.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, this.m_selectedItem.Text.ToString(), "bp.sbc")))
            {
                this.m_thumbnailImage.Visible = false;
                this.m_detailScreen = new MyGuiDetailScreenLocal(delegate (MyGuiControlListbox.Item item) {
                    if (item == null)
                    {
                        this.m_screenshotButton.Enabled = false;
                        this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
                        this.m_detailsButton.Enabled = false;
                        this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
                        this.m_replaceButton.Enabled = false;
                        this.m_deleteButton.Enabled = false;
                        this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
                    }
                    this.m_selectedItem = item;
                    this.m_activeDetail = false;
                    this.m_detailScreen = null;
                    if (Task.IsComplete)
                    {
                        this.RefreshBlueprintList(false);
                    }
                }, this.m_selectedItem, this, this.m_selectedThumbnailPath, this.m_textScale, m_currentLocalDirectory);
                this.m_activeDetail = true;
                MyScreenManager.InputToNonFocusedScreens = true;
                MyScreenManager.AddScreen(this.m_detailScreen);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.Error);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.CannotFindBlueprint), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
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
                    m_recievedBlueprints.Remove(this.m_selectedItem);
                    this.m_selectedItem = null;
                    this.RefreshBlueprintList(false);
                }
                else if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.NO)
                {
                    m_recievedBlueprints.Remove(this.m_selectedItem);
                    this.m_selectedItem = null;
                    this.RefreshBlueprintList(false);
                }
                else if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.CANCEL)
                {
                    this.m_okButton.Enabled = true;
                    this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltip);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OpenSteamWorkshopDetail(MyBlueprintItemInfo blueprintInfo)
        {
            MyWorkshopItem workshopData = blueprintInfo.Item;
            Task = Parallel.Start(delegate {
                if (!MyWorkshop.IsUpToDate(workshopData))
                {
                    this.DownloadBlueprintFromSteam(workshopData);
                }
            }, () => this.OnBlueprintDownloadedDetails(workshopData));
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            MyAnalyticsHelper.ReportActivityStart(null, "show_blueprints", string.Empty, "gui", string.Empty, true);
            Vector2 vector = new Vector2(0.02f, SCREEN_SIZE.Y - 1.076f);
            float num = (SCREEN_SIZE.Y - 1f) / 2f;
            MyGuiControlLabel control = base.MakeLabel(MyTexts.Get(MyCommonTexts.Search).ToString() + ":", vector + new Vector2(-0.175f, -0.015f), this.m_textScale);
            control.Position = new Vector2(-0.164f, -0.406f);
            this.m_searchBox = new MyGuiControlTextbox();
            this.m_searchBox.Position = new Vector2(0.123f, -0.401f);
            this.m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_searchBox.Size = new Vector2(0.279f - control.Size.X, 0.2f);
            this.m_searchBox.SetToolTip(MyCommonTexts.Blueprints_SearchTooltip);
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = vector + new Vector2(0.076f, -0.521f);
            button1.Size = new Vector2(0.045f, 0.05666667f);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Close;
            button1.ActivateOnMouseRelease = true;
            this.m_searchClear = button1;
            this.m_searchClear.ButtonClicked += new Action<MyGuiControlButton>(this.OnSearchClear);
            this.m_sortCombobox = new MyGuiControlCombobox();
            foreach (object obj2 in System.Enum.GetValues(typeof(MyBlueprintSortingOptions)))
            {
                int? sortOrder = null;
                this.m_sortCombobox.AddItem((long) ((int) obj2), new StringBuilder(MyTexts.TrySubstitute(obj2.ToString())), sortOrder, null);
            }
            this.m_sortCombobox.SelectItemByIndex((int) m_sortBy);
            this.m_sortCombobox.ItemSelected += () => this.SortOptionChanged((MyBlueprintSortingOptions) ((int) this.m_sortCombobox.GetSelectedKey()));
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.Blueprint_Sort_Label), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                Position = new Vector2(-0.164f, -0.348f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            this.m_sortCombobox.Position = new Vector2(0.123f, -0.348f);
            this.m_sortCombobox.Size = new Vector2(0.28f - label2.Size.X, 0.04f);
            this.m_sortCombobox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_sortCombobox.SetToolTip(MyCommonTexts.Blueprints_SortByTooltip);
            base.AddCaption(MyTexts.Get(MySpaceTexts.BlueprintsScreen).ToString(), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(this.m_controlPadding + new Vector2(-HIDDEN_PART_RIGHT, num - 0.03f)), 0.8f);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            colorMask = null;
            list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.44f), base.m_size.Value.X * 0.73f, 0f, colorMask);
            colorMask = null;
            list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.17f), base.m_size.Value.X * 0.73f, 0f, colorMask);
            colorMask = null;
            list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.318f), base.m_size.Value.X * 0.73f, 0f, colorMask);
            this.Controls.Add(list);
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.155f, -0.307f);
            label1.Name = "ControlLabel";
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MyCommonTexts.Blueprints_ListOfBlueprints);
            MyGuiControlLabel label3 = label1;
            colorMask = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.1635f, -0.312f), new Vector2(0.2865f, 0.035f), colorMask, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            m_blueprintList.Position = new Vector2(-0.02f, -0.066f);
            m_blueprintList.VisibleRowsCount = 12;
            m_blueprintList.MultiSelect = false;
            this.Controls.Add(panel);
            this.Controls.Add(label3);
            this.Controls.Add(control);
            this.Controls.Add(this.m_searchBox);
            this.Controls.Add(this.m_searchClear);
            this.Controls.Add(m_blueprintList);
            this.Controls.Add(this.m_sortCombobox);
            this.Controls.Add(label2);
            this.RefreshThumbnail();
            this.Controls.Add(this.m_thumbnailImage);
            this.CreateButtons();
            string texture = @"Textures\GUI\screens\screen_loading_wheel.dds";
            position = null;
            this.m_wheel = new MyGuiControlRotatingWheel(new Vector2(-0.02f, -0.12f), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.28f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, texture, true, MyPerGameSettings.GUI.MultipleSpinningWheels, position, 1.5f);
            this.Controls.Add(this.m_wheel);
            this.m_wheel.Visible = false;
        }

        public void RefreshAndReloadBlueprintList()
        {
            m_blueprintList.StoreSituation();
            m_blueprintList.Items.Clear();
            this.GetLocalBlueprintNames(true);
            this.ReloadTextures();
            m_blueprintList.RestoreSituation(false, true);
            this.OnSearchTextChange(this.m_searchBox);
        }

        public override void RefreshBlueprintList(bool fromTask = false)
        {
            m_blueprintList.StoreSituation();
            m_blueprintList.Items.Clear();
            this.GetLocalBlueprintNames(fromTask);
            this.m_selectedItem = null;
            this.m_screenshotButton.Enabled = false;
            this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            this.m_detailsButton.Enabled = false;
            this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            this.m_replaceButton.Enabled = false;
            this.m_replaceButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            this.m_deleteButton.Enabled = false;
            this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            this.m_okButton.Enabled = false;
            this.m_okButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            m_blueprintList.RestoreSituation(false, true);
            this.OnSearchTextChange(this.m_searchBox);
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

        private void ReloadTextures()
        {
            string path = "";
            string[] directories = Directory.GetDirectories(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL);
            int index = 0;
            while (index < directories.Length)
            {
                path = Path.Combine(directories[index], this.m_thumbImageName);
                if (File.Exists(path))
                {
                    MyRenderProxy.UnloadTexture(path);
                }
                index++;
            }
            string str = MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY;
            if (Directory.Exists(str))
            {
                directories = Directory.GetDirectories(str);
                index = 0;
                while (index < directories.Length)
                {
                    path = Path.Combine(directories[index], this.m_thumbImageName);
                    if (File.Exists(path))
                    {
                        MyRenderProxy.UnloadTexture(path);
                    }
                    index++;
                }
            }
            str = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, "temp");
            if (Directory.Exists(str))
            {
                directories = Directory.GetDirectories(str);
                for (index = 0; index < directories.Length; index++)
                {
                    path = Path.Combine(directories[index], this.m_thumbImageName);
                    if (File.Exists(path))
                    {
                        MyRenderProxy.UnloadTexture(path);
                    }
                }
            }
        }

        private void ResetBlueprintUI()
        {
            this.m_deleteButton.Enabled = false;
            this.m_deleteButton.SetToolTip(MyCommonTexts.Blueprints_DeleteTooltipDisabled);
            this.m_detailsButton.Enabled = false;
            this.m_detailsButton.SetToolTip(MyCommonTexts.Blueprints_OkTooltipDisabled);
            this.m_screenshotButton.Enabled = false;
            this.m_screenshotButton.SetToolTip(MyCommonTexts.Blueprints_TakeScreenshotTooltipDisabled);
            this.m_replaceButton.Enabled = false;
            this.m_selectedItem = null;
        }

        [Event(null, 210), Reliable, Server]
        public static void ShareBlueprintRequest(ulong workshopId, string name, ulong sendToId, string senderName)
        {
            if (!Sync.IsServer || (sendToId == Sync.MyId))
            {
                ShareBlueprintRequestClient(workshopId, name, sendToId, senderName);
            }
            else
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, string, ulong, string>(x => new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen.ShareBlueprintRequestClient), workshopId, name, sendToId, senderName, new EndpointId(sendToId), position);
            }
        }

        [Event(null, 0xdf), Reliable, Client]
        private static void ShareBlueprintRequestClient(ulong workshopId, string name, ulong sendToId, string senderName)
        {
            MyBlueprintItemInfo info = new MyBlueprintItemInfo(MyBlueprintTypeEnum.SHARED, new ulong?(workshopId));
            object userData = info;
            MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(name.ToString()), null, MyGuiConstants.TEXTURE_BLUEPRINTS_ARROW.Normal, userData, null) {
                ColorMask = new VRageMath.Vector4(0.7f)
            };
            if (!m_recievedBlueprints.Any<MyGuiControlListbox.Item>(delegate (MyGuiControlListbox.Item item2) {
                ulong? publishedItemId = (item2.UserData as MyBlueprintItemInfo).PublishedItemId;
                ulong? nullable2 = (item.UserData as MyBlueprintItemInfo).PublishedItemId;
                return ((publishedItemId.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((publishedItemId != null) == (nullable2 != null)));
            }))
            {
                m_recievedBlueprints.Add(item);
                int? position = null;
                m_blueprintList.Add(item, position);
                MyHudNotificationDebug notification = new MyHudNotificationDebug(string.Format(MyTexts.Get(MySpaceTexts.SharedBlueprintNotify).ToString(), senderName), 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug);
                MyHud.Notifications.Add(notification);
            }
        }

        private void SortBlueprints(List<MyGuiControlListbox.Item> list, MyBlueprintTypeEnum type)
        {
            MyItemComparer comparer = null;
            switch (type)
            {
                case MyBlueprintTypeEnum.STEAM:
                    switch (m_sortBy)
                    {
                        case MyBlueprintSortingOptions.SortBy_Name:
                            comparer = new MyItemComparer((x, y) => ((MyBlueprintItemInfo) x.UserData).BlueprintName.CompareTo(((MyBlueprintItemInfo) y.UserData).BlueprintName));
                            break;

                        case MyBlueprintSortingOptions.SortBy_CreationDate:
                            comparer = new MyItemComparer(delegate (MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) {
                                DateTime timeCreated = ((MyBlueprintItemInfo) x.UserData).Item.TimeCreated;
                                DateTime time2 = ((MyBlueprintItemInfo) y.UserData).Item.TimeCreated;
                                return (timeCreated >= time2) ? ((timeCreated <= time2) ? 0 : -1) : 1;
                            });
                            break;

                        case MyBlueprintSortingOptions.SortBy_UpdateDate:
                            comparer = new MyItemComparer(delegate (MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) {
                                DateTime timeUpdated = ((MyBlueprintItemInfo) x.UserData).Item.TimeUpdated;
                                DateTime time2 = ((MyBlueprintItemInfo) y.UserData).Item.TimeUpdated;
                                return (timeUpdated >= time2) ? ((timeUpdated <= time2) ? 0 : -1) : 1;
                            });
                            break;

                        default:
                            break;
                    }
                    break;

                case MyBlueprintTypeEnum.LOCAL:
                case MyBlueprintTypeEnum.CLOUD:
                    switch (m_sortBy)
                    {
                        case MyBlueprintSortingOptions.SortBy_Name:
                            comparer = new MyItemComparer((x, y) => ((MyBlueprintItemInfo) x.UserData).BlueprintName.CompareTo(((MyBlueprintItemInfo) y.UserData).BlueprintName));
                            break;

                        case MyBlueprintSortingOptions.SortBy_CreationDate:
                            comparer = new MyItemComparer(delegate (MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) {
                                DateTime? timeCreated = ((MyBlueprintItemInfo) x.UserData).TimeCreated;
                                DateTime? nullable2 = ((MyBlueprintItemInfo) y.UserData).TimeCreated;
                                if ((timeCreated == null) || (nullable2 == null))
                                {
                                    return 0;
                                }
                                return -1 * DateTime.Compare(timeCreated.Value, nullable2.Value);
                            });
                            break;

                        case MyBlueprintSortingOptions.SortBy_UpdateDate:
                            comparer = new MyItemComparer(delegate (MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) {
                                DateTime? timeUpdated = ((MyBlueprintItemInfo) x.UserData).TimeUpdated;
                                DateTime? nullable2 = ((MyBlueprintItemInfo) y.UserData).TimeUpdated;
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

        public void SortOptionChanged(MyBlueprintSortingOptions option)
        {
            m_sortBy = option;
            this.OnReload(null);
        }

        public void TakeScreenshot(string name)
        {
            string pathToSave = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, m_currentLocalDirectory, name, this.m_thumbImageName);
            MyRenderProxy.TakeScreenshot(new Vector2(0.5f, 0.5f), pathToSave, false, true, false);
            MyRenderProxy.UnloadTexture(pathToSave);
            this.m_thumbnailImage.Visible = true;
        }

        public override bool Update(bool hasFocus)
        {
            if (!m_blueprintList.IsMouseOver)
            {
                this.m_thumbnailImage.Visible = false;
            }
            if (!Task.IsComplete)
            {
                this.m_wheel.Visible = true;
            }
            if (Task.IsComplete)
            {
                this.m_wheel.Visible = false;
                if (m_needsExtract)
                {
                    this.GetWorkshopItemsSteam();
                    m_needsExtract = false;
                    this.RefreshBlueprintList(false);
                }
            }
            return base.Update(hasFocus);
        }

        private bool ValidateModInfo(MyObjectBuilder_ModInfo info) => 
            ((info != null) && (info.SubtypeName != null));

        private bool ValidateSelecteditem() => 
            ((this.m_selectedItem != null) ? ((this.m_selectedItem.UserData != null) ? (this.m_selectedItem.Text != null) : false) : false);

        public static bool FirstTime
        {
            get => 
                m_downloadFromSteam;
            set => 
                (m_downloadFromSteam = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiBlueprintScreen.<>c <>9 = new MyGuiBlueprintScreen.<>c();
            public static Func<IMyEventOwner, Action<ulong, string, ulong, string>> <>9__41_0;
            public static Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> <>9__50_0;
            public static Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> <>9__50_1;
            public static Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> <>9__50_2;
            public static Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> <>9__50_3;
            public static Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> <>9__50_4;
            public static Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> <>9__50_5;
            public static Action <>9__71_2;
            public static Action <>9__71_0;

            internal void <OnPrefabLoaded>b__71_0()
            {
                MyClipboardComponent.Static.HandlePasteInput(true);
            }

            internal void <OnPrefabLoaded>b__71_2()
            {
                MyClipboardComponent.Static.HandlePasteInput(true);
            }

            internal Action<ulong, string, ulong, string> <ShareBlueprintRequest>b__41_0(IMyEventOwner x) => 
                new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen.ShareBlueprintRequestClient);

            internal int <SortBlueprints>b__50_0(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) => 
                ((MyBlueprintItemInfo) x.UserData).BlueprintName.CompareTo(((MyBlueprintItemInfo) y.UserData).BlueprintName);

            internal int <SortBlueprints>b__50_1(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y)
            {
                DateTime? timeUpdated = ((MyBlueprintItemInfo) x.UserData).TimeUpdated;
                DateTime? nullable2 = ((MyBlueprintItemInfo) y.UserData).TimeUpdated;
                if ((timeUpdated == null) || (nullable2 == null))
                {
                    return 0;
                }
                return (-1 * DateTime.Compare(timeUpdated.Value, nullable2.Value));
            }

            internal int <SortBlueprints>b__50_2(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y)
            {
                DateTime? timeCreated = ((MyBlueprintItemInfo) x.UserData).TimeCreated;
                DateTime? nullable2 = ((MyBlueprintItemInfo) y.UserData).TimeCreated;
                if ((timeCreated == null) || (nullable2 == null))
                {
                    return 0;
                }
                return (-1 * DateTime.Compare(timeCreated.Value, nullable2.Value));
            }

            internal int <SortBlueprints>b__50_3(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) => 
                ((MyBlueprintItemInfo) x.UserData).BlueprintName.CompareTo(((MyBlueprintItemInfo) y.UserData).BlueprintName);

            internal int <SortBlueprints>b__50_4(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y)
            {
                DateTime timeCreated = ((MyBlueprintItemInfo) x.UserData).Item.TimeCreated;
                DateTime time2 = ((MyBlueprintItemInfo) y.UserData).Item.TimeCreated;
                return ((timeCreated >= time2) ? ((timeCreated <= time2) ? 0 : -1) : 1);
            }

            internal int <SortBlueprints>b__50_5(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y)
            {
                DateTime timeUpdated = ((MyBlueprintItemInfo) x.UserData).Item.TimeUpdated;
                DateTime time2 = ((MyBlueprintItemInfo) y.UserData).Item.TimeUpdated;
                return ((timeUpdated >= time2) ? ((timeUpdated <= time2) ? 0 : -1) : 1);
            }
        }

        private class LoadPrefabData : WorkData
        {
            private MyObjectBuilder_Definitions m_prefab;
            private string m_path;
            private MyGuiBlueprintScreen m_blueprintScreen;
            private ulong? m_id;
            private MyBlueprintItemInfo m_info;

            public LoadPrefabData(MyObjectBuilder_Definitions prefab, MyBlueprintItemInfo info, MyGuiBlueprintScreen blueprintScreen)
            {
                this.m_prefab = prefab;
                this.m_blueprintScreen = blueprintScreen;
                this.m_info = info;
            }

            public LoadPrefabData(MyObjectBuilder_Definitions prefab, string path, MyGuiBlueprintScreen blueprintScreen, ulong? id = new ulong?())
            {
                this.m_prefab = prefab;
                this.m_path = path;
                this.m_blueprintScreen = blueprintScreen;
                this.m_id = id;
            }

            public void CallLoadPrefab(WorkData workData)
            {
                this.m_prefab = MyBlueprintUtils.LoadPrefab(this.m_path);
                this.CallOnPrefabLoaded();
            }

            public void CallLoadPrefabFromCloud(WorkData workData)
            {
                this.m_prefab = MyBlueprintUtils.LoadPrefabFromCloud(this.m_info);
                this.CallOnPrefabLoaded();
            }

            public void CallLoadWorkshopPrefab(WorkData workData)
            {
                this.m_prefab = MyBlueprintUtils.LoadWorkshopPrefab(this.m_path, this.m_id, true);
                this.CallOnPrefabLoaded();
            }

            public void CallOnPrefabLoaded()
            {
                if (this.m_blueprintScreen.State == MyGuiScreenState.OPENED)
                {
                    this.m_blueprintScreen.OnPrefabLoaded(this.m_prefab);
                }
            }
        }
    }
}

