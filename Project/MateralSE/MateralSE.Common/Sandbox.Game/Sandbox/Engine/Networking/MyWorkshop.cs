namespace Sandbox.Engine.Networking
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Compression;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyWorkshop
    {
        private const int MOD_NAME_LIMIT = 0x19;
        private static MyGuiScreenMessageBox m_downloadScreen;
        private static DownloadModsResult m_downloadResult;
        private static readonly int m_dependenciesRequestTimeout = 0x7530;
        private static readonly string m_workshopWorldsDir = "WorkshopWorlds";
        private static readonly string m_workshopWorldsPath = Path.Combine(MyFileSystem.UserDataPath, m_workshopWorldsDir);
        private static readonly string m_workshopWorldSuffix = ".sbw";
        private static readonly string m_workshopBlueprintsPath = Path.Combine(MyFileSystem.UserDataPath, "Blueprints", "workshop");
        private static readonly string m_workshopBlueprintSuffix = ".sbb";
        private static readonly string m_workshopScriptPath = Path.Combine(MyFileSystem.UserDataPath, "IngameScripts", "workshop");
        private static readonly string m_workshopModsPath = MyFileSystem.ModsPath;
        public static readonly string WorkshopModSuffix = "_legacy.bin";
        private static readonly string m_workshopScenariosPath = Path.Combine(MyFileSystem.UserDataPath, "Scenarios", "workshop");
        private static readonly string m_workshopScenariosSuffix = ".sbs";
        private static readonly string[] m_previewFileNames = new string[] { "thumb.png", MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION };
        private const string ModMetadataFileName = "metadata.mod";
        private static readonly HashSet<string> m_ignoredExecutableExtensions;
        private static readonly int m_bufferSize;
        private static byte[] buffer;
        private static Category[] m_modCategories;
        private static Category[] m_worldCategories;
        private static Category[] m_blueprintCategories;
        private static Category[] m_scenarioCategories;
        public const string WORKSHOP_DEVELOPMENT_TAG = "development";
        public const string WORKSHOP_WORLD_TAG = "world";
        public const string WORKSHOP_CAMPAIGN_TAG = "campaign";
        public const string WORKSHOP_MOD_TAG = "mod";
        public const string WORKSHOP_BLUEPRINT_TAG = "blueprint";
        public const string WORKSHOP_SCENARIO_TAG = "scenario";
        private const string WORKSHOP_INGAMESCRIPT_TAG = "ingameScript";
        private static FastResourceLock m_modLock;
        private static Action<bool, MyGameServiceCallResult, ulong> m_onPublishingFinished;
        private static bool m_publishSuccess;
        private static ulong m_publishedFileId;
        private static MyGameServiceCallResult m_publishResult;
        private static MyGuiScreenProgressAsync m_asyncPublishScreen;

        static MyWorkshop()
        {
            string[] collection = new string[0x30];
            collection[0] = ".action";
            collection[1] = ".apk";
            collection[2] = ".app";
            collection[3] = ".bat";
            collection[4] = ".bin";
            collection[5] = ".cmd";
            collection[6] = ".com";
            collection[7] = ".command";
            collection[8] = ".cpl";
            collection[9] = ".csh";
            collection[10] = ".dll";
            collection[11] = ".exe";
            collection[12] = ".gadget";
            collection[13] = ".inf1";
            collection[14] = ".ins";
            collection[15] = ".inx";
            collection[0x10] = ".ipa";
            collection[0x11] = ".isu";
            collection[0x12] = ".job";
            collection[0x13] = ".jse";
            collection[20] = ".ksh";
            collection[0x15] = ".lnk";
            collection[0x16] = ".msc";
            collection[0x17] = ".msi";
            collection[0x18] = ".msp";
            collection[0x19] = ".mst";
            collection[0x1a] = ".osx";
            collection[0x1b] = ".out";
            collection[0x1c] = ".pif";
            collection[0x1d] = ".paf";
            collection[30] = ".prg";
            collection[0x1f] = ".ps1";
            collection[0x20] = ".reg";
            collection[0x21] = ".rgs";
            collection[0x22] = ".run";
            collection[0x23] = ".sct";
            collection[0x24] = ".shb";
            collection[0x25] = ".shs";
            collection[0x26] = ".so";
            collection[0x27] = ".u3p";
            collection[40] = ".vb";
            collection[0x29] = ".vbe";
            collection[0x2a] = ".vbs";
            collection[0x2b] = ".vbscript";
            collection[0x2c] = ".workflow";
            collection[0x2d] = ".ws";
            collection[0x2e] = ".wsf";
            collection[0x2f] = ".suo";
            m_ignoredExecutableExtensions = new HashSet<string>(collection);
            m_bufferSize = 0x100000;
            buffer = new byte[m_bufferSize];
            m_modLock = new FastResourceLock();
        }

        public static bool CanRunOffline(List<MyObjectBuilder_Checkpoint.ModItem> mods)
        {
            using (List<MyObjectBuilder_Checkpoint.ModItem>.Enumerator enumerator = mods.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyObjectBuilder_Checkpoint.ModItem current = enumerator.Current;
                    if (current.PublishedFileId != 0)
                    {
                        string path = Path.Combine(MyFileSystem.ModsPath, current.Name);
                        if (!Directory.Exists(path) && !File.Exists(path))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void CheckAndFixModMetadata(ref MyModMetadata mod)
        {
            if (mod == null)
            {
                mod = new MyModMetadata();
            }
            if (mod.ModVersion == null)
            {
                mod.ModVersion = new Version(1, 0);
            }
        }

        public static bool CheckLocalModsAllowed(List<MyObjectBuilder_Checkpoint.ModItem> mods, bool allowLocalMods)
        {
            using (List<MyObjectBuilder_Checkpoint.ModItem>.Enumerator enumerator = mods.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if ((enumerator.Current.PublishedFileId == 0) && !allowLocalMods)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static MyModCompatibility CheckModCompatibility(string localFullPath)
        {
            if (string.IsNullOrWhiteSpace(localFullPath))
            {
                return MyModCompatibility.Unknown;
            }
            string path = Path.Combine(localFullPath, "metadata.mod");
            return (MyFileSystem.FileExists(path) ? CheckModCompatibility(MyModMetadataLoader.Load(path)) : MyModCompatibility.Unknown);
        }

        public static MyModCompatibility CheckModCompatibility(MyModMetadata mod) => 
            MyModCompatibility.Ok;

        private static void CheckModDependencies(List<MyObjectBuilder_Checkpoint.ModItem> mods, List<ulong> publishedFileIds)
        {
            bool flag;
            List<MyObjectBuilder_Checkpoint.ModItem> collection = new List<MyObjectBuilder_Checkpoint.ModItem>();
            HashSet<ulong> set = new HashSet<ulong>();
            foreach (MyObjectBuilder_Checkpoint.ModItem item in mods)
            {
                if (!item.IsDependency)
                {
                    if (item.PublishedFileId == 0)
                    {
                        collection.Add(item);
                        continue;
                    }
                    set.Add(item.PublishedFileId);
                }
            }
            foreach (MyWorkshopItem item2 in GetModsDependencyHiearchy(set, out flag))
            {
                bool isDependency = !set.Contains(item2.Id);
                MyObjectBuilder_Checkpoint.ModItem item3 = new MyObjectBuilder_Checkpoint.ModItem(item2.Id, isDependency) {
                    FriendlyName = item2.Title
                };
                collection.Add(item3);
                if (!publishedFileIds.Contains(item2.Id))
                {
                    publishedFileIds.Add(item2.Id);
                }
            }
            mods.Clear();
            mods.AddRange(collection);
        }

        private static bool CheckModFolder(ref string localFolder, HashSet<string> ignoredExtensions, HashSet<string> ignoredPaths)
        {
            if (((ignoredExtensions == null) || (ignoredExtensions.Count == 0)) && ((ignoredPaths == null) || (ignoredPaths.Count == 0)))
            {
                return false;
            }
            string path = Path.Combine(Path.GetTempPath(), $"{Process.GetCurrentProcess().Id}-{Path.GetFileName(localFolder)}");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            localFolder = MyFileSystem.TerminatePath(localFolder);
            int sourcePathLength = localFolder.Length;
            MyFileSystem.CopyAll(localFolder, path, delegate (string s) {
                if (ignoredExtensions != null)
                {
                    string extension = Path.GetExtension(s);
                    if ((extension != null) && ignoredExtensions.Contains(extension))
                    {
                        return false;
                    }
                }
                if (ignoredPaths == null)
                {
                    return true;
                }
                string item = s.Substring(sourcePathLength);
                return !ignoredPaths.Contains(item);
            });
            localFolder = path;
            return true;
        }

        public static void CreateWorldInstanceAsync(MyWorkshopItem world, MyWorkshopPathInfo pathInfo, bool overwrite, Action<bool, string> callbackOnFinished = null)
        {
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.ProgressTextCreatingWorld, cancelText, () => new CreateWorldResult(world, pathInfo, callbackOnFinished, overwrite), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionCreateWorldInstance), null));
        }

        public static bool DownloadBlueprintBlockingUGC(MyWorkshopItem item, bool check = true)
        {
            if (!check || !item.IsUpToDate())
            {
                return UpdateMod(item);
            }
            MySandboxGame.Log.WriteLineAndConsole($"Up to date mod: Id = {item.Id}, title = '{item.Title}'");
            return true;
        }

        public static void DownloadModsAsync(List<MyObjectBuilder_Checkpoint.ModItem> mods, Action<bool> onFinishedCallback, Action onCancelledCallback = null)
        {
            if ((mods == null) || (mods.Count == 0))
            {
                onFinishedCallback(true);
            }
            else
            {
                if (!Directory.Exists(m_workshopModsPath))
                {
                    Directory.CreateDirectory(m_workshopModsPath);
                }
                CancelToken cancelToken = new CancelToken();
                MyStringId? cancelButtonText = null;
                cancelButtonText = null;
                cancelButtonText = null;
                Vector2? size = null;
                m_downloadScreen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(MyTexts.GetString(MyCommonTexts.ProgressTextCheckingMods)), new StringBuilder(MyTexts.GetString(MyCommonTexts.DownloadingMods)), new MyStringId?(MyCommonTexts.Cancel), cancelButtonText, cancelButtonText, cancelButtonText, delegate (MyGuiScreenMessageBox.ResultEnum r) {
                    cancelToken.Cancel = true;
                    if (onCancelledCallback != null)
                    {
                        onCancelledCallback();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
                m_downloadResult = new DownloadModsResult(mods, onFinishedCallback, cancelToken);
                MyGuiSandbox.AddScreen(m_downloadScreen);
            }
        }

        private static ResultData DownloadModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, ResultData ret, List<ulong> publishedFileIds, CancelToken cancelToken)
        {
            List<MyWorkshopItem> resultDestination = new List<MyWorkshopItem>(publishedFileIds.Count);
            if (!GetItemsBlockingUGC(publishedFileIds, resultDestination))
            {
                MySandboxGame.Log.WriteLine("Could not obtain workshop item details");
                ret.Success = false;
            }
            else if (publishedFileIds.Count != resultDestination.Count)
            {
                MySandboxGame.Log.WriteLine($"Could not obtain all workshop item details, expected {publishedFileIds.Count}, got {resultDestination.Count}");
                ret.Success = false;
            }
            else
            {
                if (m_downloadScreen != null)
                {
                    m_downloadScreen.MessageText = new StringBuilder(MyTexts.GetString(MyCommonTexts.ProgressTextDownloadingMods) + " 0 of " + resultDestination.Count.ToString());
                }
                ret = DownloadModsBlockingUGC(resultDestination, cancelToken);
                if (!ret.Success)
                {
                    MySandboxGame.Log.WriteLine("Downloading mods failed");
                }
                else
                {
                    MyObjectBuilder_Checkpoint.ModItem[] array = mods.ToArray();
                    int i = 0;
                    while (true)
                    {
                        if (i >= array.Length)
                        {
                            mods.Clear();
                            mods.AddArray<MyObjectBuilder_Checkpoint.ModItem>(array);
                            break;
                        }
                        MyWorkshopItem workshopItem = resultDestination.Find(x => x.Id == array[i].PublishedFileId);
                        if (workshopItem == null)
                        {
                            array[i].FriendlyName = array[i].Name;
                        }
                        else
                        {
                            array[i].FriendlyName = workshopItem.Title;
                            array[i].SetModData(workshopItem);
                        }
                        i++;
                    }
                }
            }
            return ret;
        }

        public static ResultData DownloadModsBlockingUGC(List<MyWorkshopItem> mods, CancelToken cancelToken)
        {
            int counter = 0;
            string numMods = mods.Count.ToString();
            CachingList<MyWorkshopItem> failedMods = new CachingList<MyWorkshopItem>();
            CachingList<MyWorkshopItem> collection = new CachingList<MyWorkshopItem>();
            List<KeyValuePair<MyWorkshopItem, string>> currentMods = new List<KeyValuePair<MyWorkshopItem, string>>();
            bool downloadingFailed = false;
            long timestamp = Stopwatch.GetTimestamp();
            double byteSize = 0.0;
            int num6 = 0;
            while (true)
            {
                if (num6 >= mods.Count)
                {
                    string sizeStr = MyUtils.FormatByteSizePrefix(ref byteSize);
                    sizeStr = byteSize.ToString("N1") + sizeStr + "B";
                    double runningTotal = 0.0;
                    WorkOptions? options = null;
                    Parallel.ForEach<MyWorkshopItem>(mods, delegate (MyWorkshopItem mod) {
                        if (!MyGameService.IsOnline)
                        {
                            downloadingFailed = true;
                        }
                        else if ((cancelToken != null) && cancelToken.Cancel)
                        {
                            downloadingFailed = true;
                        }
                        else
                        {
                            UpdateDownloadScreen(counter, numMods, currentMods, sizeStr, runningTotal, mod);
                            if (!UpdateMod(mod))
                            {
                                MySandboxGame.Log.WriteLineAndConsole($"Mod failed: Id = {mod.Id}, title = '{mod.Title}'");
                                failedMods.Add(mod);
                                downloadingFailed = true;
                                if (cancelToken != null)
                                {
                                    cancelToken.Cancel = true;
                                }
                            }
                            else
                            {
                                MySandboxGame.Log.WriteLineAndConsole($"Up to date mod: Id = {mod.Id}, title = '{mod.Title}'");
                                if (m_downloadScreen != null)
                                {
                                    using (m_modLock.AcquireExclusiveUsing())
                                    {
                                        runningTotal += mod.Size;
                                        int num = counter;
                                        counter = num + 1;
                                        currentMods.RemoveAll(e => e.Key == mod);
                                    }
                                }
                            }
                        }
                    }, WorkPriority.Normal, options, true);
                    long num3 = Stopwatch.GetTimestamp();
                    if (!downloadingFailed)
                    {
                        collection.ApplyChanges();
                        ResultData data = new ResultData {
                            Success = true,
                            MismatchMods = new List<MyWorkshopItem>(collection),
                            Mods = new List<MyWorkshopItem>(mods)
                        };
                        double num4 = ((double) (num3 - timestamp)) / ((double) Stopwatch.Frequency);
                        MySandboxGame.Log.WriteLineAndConsole($"Mod download time: {num4:0.00} seconds");
                        return data;
                    }
                    failedMods.ApplyChanges();
                    if (failedMods.Count > 0)
                    {
                        foreach (MyWorkshopItem item in failedMods)
                        {
                            MySandboxGame.Log.WriteLineAndConsole($"Failed to download mod: Id = {item.Id}, title = '{item.Title}'");
                        }
                        break;
                    }
                    if ((cancelToken == null) || !cancelToken.Cancel)
                    {
                        MySandboxGame.Log.WriteLineAndConsole(string.Format("Failed to download mods because Steam is not in Online Mode.", Array.Empty<object>()));
                    }
                    else
                    {
                        MySandboxGame.Log.WriteLineAndConsole(string.Format("Failed to download mods because download was stopped.", Array.Empty<object>()));
                    }
                    break;
                }
                StartDownloadingMod(mods[num6]);
                byteSize += mods[num6].Size;
                num6++;
            }
            return new ResultData();
        }

        public static bool DownloadScriptBlocking(MyWorkshopItem item)
        {
            if (!MyGameService.IsOnline)
            {
                return false;
            }
            if (!item.IsUpToDate())
            {
                return UpdateMod(item);
            }
            MySandboxGame.Log.WriteLineAndConsole($"Up to date mod: Id = {item.Id}, title = '{item.Title}'");
            return true;
        }

        public static unsafe ResultData DownloadWorldModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, CancelToken cancelToken)
        {
            ResultData ret = new ResultData {
                Success = true
            };
            if (!MyFakes.ENABLE_WORKSHOP_MODS)
            {
                if (cancelToken != null)
                {
                    ret.Cancel = cancelToken.Cancel;
                }
                return ret;
            }
            MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - START");
            MySandboxGame.Log.IncreaseIndent();
            if ((mods != null) && (mods.Count > 0))
            {
                List<ulong> publishedFileIds = new List<ulong>();
                using (List<MyObjectBuilder_Checkpoint.ModItem>.Enumerator enumerator = mods.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyObjectBuilder_Checkpoint.ModItem current = enumerator.Current;
                        if (current.PublishedFileId != 0)
                        {
                            if (publishedFileIds.Contains(current.PublishedFileId))
                            {
                                continue;
                            }
                            publishedFileIds.Add(current.PublishedFileId);
                            continue;
                        }
                        if (Sandbox.Engine.Platform.Game.IsDedicated)
                        {
                            MySandboxGame.Log.WriteLineAndConsole("Local mods are not allowed in multiplayer.");
                            MySandboxGame.Log.DecreaseIndent();
                            return new ResultData();
                        }
                    }
                }
                publishedFileIds.Sort();
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    if (Sync.IsServer)
                    {
                        CheckModDependencies(mods, publishedFileIds);
                    }
                    ret = DownloadModsBlocking(mods, ret, publishedFileIds, cancelToken);
                }
                else
                {
                    if (MySandboxGame.ConfigDedicated.AutodetectDependencies)
                    {
                        CheckModDependencies(mods, publishedFileIds);
                    }
                    MyGameService.SetServerModTemporaryDirectory();
                    ret = DownloadModsBlocking(mods, ret, publishedFileIds, cancelToken);
                }
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - END");
            if (cancelToken != null)
            {
                bool* flagPtr1 = (bool*) ref ret.Cancel;
                *((sbyte*) flagPtr1) = *(((byte*) flagPtr1)) | cancelToken.Cancel;
            }
            return ret;
        }

        private static void endActionCreateWorldInstance(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            screen.CloseScreen();
            CreateWorldResult result2 = (CreateWorldResult) result;
            Action<bool, string> callback = result2.Callback;
            if (callback != null)
            {
                callback(result2.Success, result2.m_createdSessionPath);
            }
        }

        private static void endActionDownloadMods()
        {
            m_downloadScreen.CloseScreen();
            if (!m_downloadResult.Result.Success)
            {
                MySandboxGame.Log.WriteLine(string.Format("Error downloading mods", Array.Empty<object>()));
            }
            m_downloadResult.Callback(m_downloadResult.Result.Success);
        }

        private static void endActionPublish(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            screen.CloseScreenNow();
            if (m_onPublishingFinished != null)
            {
                m_onPublishingFinished(m_publishSuccess, m_publishResult, m_publishedFileId);
            }
            m_publishSuccess = false;
            m_publishResult = MyGameServiceCallResult.Fail;
            m_onPublishingFinished = null;
            m_asyncPublishScreen = null;
        }

        private static void endActionUpdateWorld(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            screen.CloseScreen();
            UpdateWorldsResult result2 = (UpdateWorldsResult) result;
            Action<bool> callback = result2.Callback;
            if (callback != null)
            {
                callback(result2.Success);
            }
        }

        public static bool GenerateModInfo(string modPath, ulong publishedFileId, ulong steamIDOwner)
        {
            MyObjectBuilder_ModInfo objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ModInfo>();
            objectBuilder.WorkshopId = publishedFileId;
            objectBuilder.SteamIDOwner = steamIDOwner;
            if (MyObjectBuilderSerializer.SerializeXML(Path.Combine(modPath, "modinfo.sbmi"), false, objectBuilder, null))
            {
                return true;
            }
            MySandboxGame.Log.WriteLine($"Error creating modinfo: workshopID={publishedFileId}, mod='{modPath}'");
            return false;
        }

        private static string GetErrorString(bool ioFailure, MyGameServiceCallResult result) => 
            (ioFailure ? "IO Failure" : result.ToString());

        public static bool GetItemsBlockingUGC(IEnumerable<ulong> publishedFileIds, List<MyWorkshopItem> resultDestination)
        {
            ulong[] collection = publishedFileIds.Distinct<ulong>().ToArray<ulong>();
            MySandboxGame.Log.WriteLine($"MyWorkshop.GetItemsBlocking: getting {collection.Length} items");
            resultDestination.Clear();
            if (!MyGameService.IsOnline && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                return false;
            }
            if (collection.Length != 0)
            {
                MyWorkshopQuery query = MyGameService.CreateWorkshopQuery();
                query.ItemIds = new List<ulong>(collection);
                using (AutoResetEvent resetEvent = new AutoResetEvent(false))
                {
                    query.QueryCompleted += delegate (MyGameServiceCallResult result) {
                        if (result == MyGameServiceCallResult.OK)
                        {
                            MySandboxGame.Log.WriteLine("Mod query successful");
                        }
                        else
                        {
                            MySandboxGame.Log.WriteLine($"Error during mod query: {result}");
                        }
                        resetEvent.Set();
                    };
                    query.Run();
                    if (!resetEvent.WaitOne())
                    {
                        return false;
                    }
                }
                if (query.Items != null)
                {
                    resultDestination.AddRange(query.Items);
                }
            }
            return true;
        }

        public static List<MyWorkshopItem> GetModsDependencyHiearchy(HashSet<ulong> publishedFileIds, out bool hasReferenceIssue)
        {
            // Invalid method body.
        }

        public static List<MyWorkshopItem> GetModsInfo(List<ulong> publishedFileIds)
        {
            MyWorkshopQuery query = MyGameService.CreateWorkshopQuery();
            query.ItemIds = publishedFileIds;
            using (AutoResetEvent resetEvent = new AutoResetEvent(false))
            {
                query.QueryCompleted += delegate (MyGameServiceCallResult result) {
                    if (result == MyGameServiceCallResult.OK)
                    {
                        MySandboxGame.Log.WriteLine("Mod dependencies query successful");
                    }
                    else
                    {
                        MySandboxGame.Log.WriteLine($"Error during mod dependencies query: {result}");
                    }
                    resetEvent.Set();
                };
                query.Run();
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    if (!resetEvent.WaitOne(m_dependenciesRequestTimeout))
                    {
                        query.Dispose();
                        return null;
                    }
                }
                else
                {
                    int num = 0;
                    while (true)
                    {
                        if (resetEvent.WaitOne(1))
                        {
                            break;
                        }
                        MyGameService.Update();
                        num++;
                        if (num > m_dependenciesRequestTimeout)
                        {
                            query.Dispose();
                            return null;
                        }
                    }
                }
            }
            List<MyWorkshopItem> items = query.Items;
            query.Dispose();
            return items;
        }

        public static bool GetSubscribedBlueprintsBlocking(List<MyWorkshopItem> results)
        {
            bool subscribedItemsBlockingUGC;
            MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedModsBlocking - START");
            try
            {
                string[] tags = new string[] { "blueprint" };
                subscribedItemsBlockingUGC = GetSubscribedItemsBlockingUGC(results, tags);
            }
            finally
            {
                MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedModsBlocking - END");
            }
            return subscribedItemsBlockingUGC;
        }

        public static bool GetSubscribedCampaignsBlocking(List<MyWorkshopItem> results)
        {
            bool subscribedItemsBlockingUGC;
            MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedWorldsBlocking - START");
            try
            {
                string[] tags = new string[] { "campaign" };
                subscribedItemsBlockingUGC = GetSubscribedItemsBlockingUGC(results, tags);
            }
            finally
            {
                MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedWorldsBlocking - END");
            }
            return subscribedItemsBlockingUGC;
        }

        public static bool GetSubscribedIngameScriptsBlocking(List<MyWorkshopItem> results)
        {
            bool subscribedItemsBlockingUGC;
            MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedModsBlocking - START");
            try
            {
                string[] tags = new string[] { "ingameScript" };
                subscribedItemsBlockingUGC = GetSubscribedItemsBlockingUGC(results, tags);
            }
            finally
            {
                MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedModsBlocking - END");
            }
            return subscribedItemsBlockingUGC;
        }

        private static bool GetSubscribedItemsBlockingUGC(List<MyWorkshopItem> results, IEnumerable<string> tags)
        {
            results.Clear();
            if (!MyGameService.IsActive)
            {
                return false;
            }
            MyWorkshopQuery query = MyGameService.CreateWorkshopQuery();
            query.UserId = Sync.MyId;
            if (tags != null)
            {
                if (query.RequiredTags == null)
                {
                    query.RequiredTags = new List<string>();
                }
                query.RequiredTags.AddRange(tags);
            }
            using (AutoResetEvent resetEvent = new AutoResetEvent(false))
            {
                query.QueryCompleted += delegate (MyGameServiceCallResult result) {
                    if (result == MyGameServiceCallResult.OK)
                    {
                        MySandboxGame.Log.WriteLine("Query successful.");
                    }
                    else
                    {
                        MySandboxGame.Log.WriteLine($"Error during publishing: {result}");
                    }
                    resetEvent.Set();
                };
                query.Run();
                if (!resetEvent.WaitOne())
                {
                    return false;
                }
            }
            if (query.Items != null)
            {
                results.AddRange(query.Items);
            }
            return true;
        }

        public static bool GetSubscribedModsBlocking(List<MyWorkshopItem> results)
        {
            bool subscribedItemsBlockingUGC;
            MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedModsBlocking - START");
            try
            {
                string[] tags = new string[] { "mod" };
                subscribedItemsBlockingUGC = GetSubscribedItemsBlockingUGC(results, tags);
            }
            finally
            {
                MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedModsBlocking - END");
            }
            return subscribedItemsBlockingUGC;
        }

        public static bool GetSubscribedScenariosBlocking(List<MyWorkshopItem> results)
        {
            bool subscribedItemsBlockingUGC;
            MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedScenariosBlocking - START");
            try
            {
                string[] tags = new string[] { "scenario" };
                subscribedItemsBlockingUGC = GetSubscribedItemsBlockingUGC(results, tags);
            }
            finally
            {
                MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedScenariosBlocking - END");
            }
            return subscribedItemsBlockingUGC;
        }

        public static bool GetSubscribedWorldsBlocking(List<MyWorkshopItem> results)
        {
            bool subscribedItemsBlockingUGC;
            MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedWorldsBlocking - START");
            try
            {
                string[] tags = new string[] { "world" };
                subscribedItemsBlockingUGC = GetSubscribedItemsBlockingUGC(results, tags);
            }
            finally
            {
                MySandboxGame.Log.WriteLine("MySteamWorkshop.GetSubscribedWorldsBlocking - END");
            }
            return subscribedItemsBlockingUGC;
        }

        public static ulong GetWorkshopIdFromLocalMod(string localModFolder)
        {
            MyObjectBuilder_ModInfo info;
            string path = Path.Combine(MyFileSystem.ModsPath, localModFolder, "modinfo.sbmi");
            if (!File.Exists(path) || !MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_ModInfo>(path, out info))
            {
                return 0UL;
            }
            return info.WorkshopId;
        }

        public static void Init(Category[] modCategories, Category[] worldCategories, Category[] blueprintCategories, Category[] scenarioCategories)
        {
            m_modCategories = modCategories;
            m_worldCategories = worldCategories;
            m_blueprintCategories = blueprintCategories;
            m_scenarioCategories = scenarioCategories;
        }

        public static bool IsUpToDate(MyWorkshopItem item)
        {
            if (!MyGameService.IsOnline)
            {
                return false;
            }
            item.UpdateState();
            return item.IsUpToDate();
        }

        public static void PublishBlueprintAsync(string localWorldFolder, string publishedTitle, string publishedDescription, ulong? publishedFileId, string[] tags, uint[] requiredDLCs, MyPublishedFileVisibility visibility, Action<bool, MyGameServiceCallResult, ulong> callbackOnFinished = null)
        {
            m_onPublishingFinished = callbackOnFinished;
            m_publishSuccess = false;
            m_publishedFileId = 0UL;
            m_publishResult = MyGameServiceCallResult.Fail;
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(m_asyncPublishScreen = new MyGuiScreenProgressAsync(MyCommonTexts.ProgressTextUploadingWorld, cancelText, () => new PublishItemResult(localWorldFolder, publishedTitle, publishedDescription, publishedFileId, visibility, tags, m_ignoredExecutableExtensions, null, requiredDLCs), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionPublish), null));
        }

        public static void PublishIngameScriptAsync(string localWorldFolder, string publishedTitle, string publishedDescription, ulong? publishedFileId, MyPublishedFileVisibility visibility, Action<bool, MyGameServiceCallResult, ulong> callbackOnFinished = null)
        {
            m_onPublishingFinished = callbackOnFinished;
            m_publishSuccess = false;
            m_publishedFileId = 0UL;
            m_publishResult = MyGameServiceCallResult.Fail;
            string[] tags = new string[] { "ingameScript" };
            HashSet<string> ignoredExtensions = new HashSet<string>(m_ignoredExecutableExtensions) { 
                ".sbmi",
                ".png",
                ".jpg"
            };
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(m_asyncPublishScreen = new MyGuiScreenProgressAsync(MyCommonTexts.ProgressTextUploadingWorld, cancelText, () => new PublishItemResult(localWorldFolder, publishedTitle, publishedDescription, publishedFileId, visibility, tags, ignoredExtensions, null, null), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionPublish), null));
        }

        private static ulong PublishItemBlocking(string localFolder, string publishedTitle, string publishedDescription, ulong? workshopId, MyPublishedFileVisibility visibility, string[] tags, HashSet<string> ignoredExtensions = null, HashSet<string> ignoredPaths = null, uint[] requiredDLCs = null)
        {
            ulong num3;
            MySandboxGame.Log.WriteLine("PublishItemBlocking - START");
            MySandboxGame.Log.IncreaseIndent();
            if (tags.Length == 0)
            {
                MySandboxGame.Log.WriteLine("Error: Can not publish with no tags!");
                MySandboxGame.Log.DecreaseIndent();
                MySandboxGame.Log.WriteLine("PublishItemBlocking - END");
                return 0UL;
            }
            if (!MyGameService.IsActive && !MyGameService.IsOnline)
            {
                return 0UL;
            }
            MyWorkshopItemPublisher publisher = MyGameService.CreateWorkshopPublisher();
            if (publisher == null)
            {
                return 0UL;
            }
            if (workshopId != null)
            {
                List<MyWorkshopItem> resultDestination = new List<MyWorkshopItem>();
                ulong[] publishedFileIds = new ulong[] { workshopId.Value };
                if (GetItemsBlockingUGC(publishedFileIds, resultDestination))
                {
                    MyWorkshopItem item = resultDestination.FirstOrDefault<MyWorkshopItem>(wi => wi.Id == workshopId.Value);
                    if (item != null)
                    {
                        publishedTitle = item.Title;
                    }
                }
            }
            publisher.Title = publishedTitle;
            publisher.Description = publishedDescription;
            publisher.Visibility = visibility;
            publisher.Tags = new List<string>(tags);
            ulong? nullable = workshopId;
            publisher.Id = (nullable != null) ? nullable.GetValueOrDefault() : ((ulong) 0L);
            string filename = Path.Combine(localFolder, "metadata.mod");
            MyModMetadata mod = MyModMetadataLoader.Load(filename);
            CheckAndFixModMetadata(ref mod);
            MyModMetadataLoader.Save(filename, (ModMetadataFile) mod);
            bool flag = CheckModFolder(ref localFolder, ignoredExtensions, ignoredPaths);
            publisher.Folder = localFolder;
            publisher.Metadata = mod;
            string[] previewFileNames = m_previewFileNames;
            int index = 0;
            while (true)
            {
                if (index < previewFileNames.Length)
                {
                    string str = previewFileNames[index];
                    string path = Path.Combine(localFolder, str);
                    if (!File.Exists(path))
                    {
                        index++;
                        continue;
                    }
                    publisher.Thumbnail = path;
                }
                try
                {
                    using (AutoResetEvent resetEvent = new AutoResetEvent(false))
                    {
                        publisher.ItemPublished += delegate (MyGameServiceCallResult result, ulong id) {
                            m_publishResult = result;
                            if (result == MyGameServiceCallResult.OK)
                            {
                                MySandboxGame.Log.WriteLine("Published file update successful");
                            }
                            else
                            {
                                MySandboxGame.Log.WriteLine($"Error during publishing: {result}");
                            }
                            m_publishSuccess = result == MyGameServiceCallResult.OK;
                            m_publishedFileId = id;
                            resetEvent.Set();
                        };
                        if (requiredDLCs != null)
                        {
                            foreach (uint num2 in requiredDLCs)
                            {
                                publisher.DLCs.Add(num2);
                            }
                        }
                        publisher.Publish();
                        if (!resetEvent.WaitOne())
                        {
                            num3 = 0UL;
                            break;
                        }
                    }
                }
                finally
                {
                    if (flag && localFolder.StartsWith(Path.GetTempPath()))
                    {
                        Directory.Delete(localFolder, true);
                    }
                }
                return publisher.Id;
            }
            return num3;
        }

        public static void PublishModAsync(string localModFolder, string publishedTitle, string publishedDescription, ulong publishedFileId, string[] tags, MyPublishedFileVisibility visibility, Action<bool, MyGameServiceCallResult, ulong> callbackOnFinished = null)
        {
            m_onPublishingFinished = callbackOnFinished;
            m_publishSuccess = false;
            m_publishedFileId = 0UL;
            m_publishResult = MyGameServiceCallResult.Fail;
            HashSet<string> ignoredPaths = new HashSet<string> { "modinfo.sbmi" };
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(m_asyncPublishScreen = new MyGuiScreenProgressAsync(MyCommonTexts.ProgressTextUploadingWorld, cancelText, () => new PublishItemResult(localModFolder, publishedTitle, publishedDescription, new ulong?(publishedFileId), visibility, tags, m_ignoredExecutableExtensions, ignoredPaths, null), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionPublish), null));
        }

        public static void PublishScenarioAsync(string localWorldFolder, string publishedTitle, string publishedDescription, ulong? publishedFileId, MyPublishedFileVisibility visibility, Action<bool, MyGameServiceCallResult, ulong> callbackOnFinished = null)
        {
            m_onPublishingFinished = callbackOnFinished;
            m_publishSuccess = false;
            m_publishedFileId = 0UL;
            m_publishResult = MyGameServiceCallResult.Fail;
            string[] tags = new string[] { "scenario" };
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(m_asyncPublishScreen = new MyGuiScreenProgressAsync(MyCommonTexts.ProgressTextUploadingWorld, cancelText, () => new PublishItemResult(localWorldFolder, publishedTitle, publishedDescription, publishedFileId, visibility, tags, m_ignoredExecutableExtensions, null, null), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionPublish), null));
        }

        public static void PublishWorldAsync(string localWorldFolder, string publishedTitle, string publishedDescription, ulong? publishedFileId, string[] tags, MyPublishedFileVisibility visibility, Action<bool, MyGameServiceCallResult, ulong> callbackOnFinished = null)
        {
            m_onPublishingFinished = callbackOnFinished;
            m_publishSuccess = false;
            m_publishedFileId = 0UL;
            m_publishResult = MyGameServiceCallResult.Fail;
            HashSet<string> ignoredExtensions = new HashSet<string>(m_ignoredExecutableExtensions) { 
                ".xmlcache",
                ".png"
            };
            HashSet<string> ignoredPaths = new HashSet<string> { "Backup" };
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(m_asyncPublishScreen = new MyGuiScreenProgressAsync(MyCommonTexts.ProgressTextUploadingWorld, cancelText, () => new PublishItemResult(localWorldFolder, publishedTitle, publishedDescription, publishedFileId, visibility, tags, ignoredExtensions, ignoredPaths, null), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionPublish), null));
        }

        private static void StartDownloadingMod(MyWorkshopItem mod)
        {
            mod.UpdateState();
            if (!mod.IsUpToDate())
            {
                mod.Download();
            }
        }

        public static bool TryCreateWorldInstanceBlocking(MyWorkshopItem world, MyWorkshopPathInfo pathInfo, out string sessionPath, bool overwrite)
        {
            ulong num;
            if (!Directory.Exists(pathInfo.Path))
            {
                Directory.CreateDirectory(pathInfo.Path);
            }
            string sessionUniqueName = MyUtils.StripInvalidChars(world.Title);
            sessionPath = null;
            Path.Combine(pathInfo.Path, world.Id + pathInfo.Suffix);
            if (!MyGameService.IsOnline)
            {
                return false;
            }
            if (!UpdateMod(world))
            {
                return false;
            }
            sessionPath = MyLocalCache.GetSessionSavesPath(sessionUniqueName, false, false);
            if (overwrite && Directory.Exists(sessionPath))
            {
                Directory.Delete(sessionPath, true);
            }
            while (Directory.Exists(sessionPath))
            {
                sessionPath = MyLocalCache.GetSessionSavesPath(sessionUniqueName + MyUtils.GetRandomInt(0x7fffffff).ToString("########"), false, false);
            }
            if (MyFileSystem.IsDirectory(world.Folder))
            {
                MyFileSystem.CopyAll(world.Folder, sessionPath);
            }
            else
            {
                MyZipArchive.ExtractToDirectory(world.Folder, sessionPath);
            }
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out num);
            if (checkpoint == null)
            {
                return false;
            }
            checkpoint.SessionName = $"({pathInfo.NamePrefix}) {world.Title}";
            checkpoint.LastSaveTime = DateTime.Now;
            checkpoint.WorkshopId = null;
            MyLocalCache.SaveCheckpoint(checkpoint, sessionPath);
            return true;
        }

        public static bool TryUpdateWorldsBlocking(List<MyWorkshopItem> worlds, MyWorkshopPathInfo pathInfo)
        {
            if (!Directory.Exists(pathInfo.Path))
            {
                Directory.CreateDirectory(pathInfo.Path);
            }
            if (!MyGameService.IsOnline)
            {
                return false;
            }
            bool flag = true;
            foreach (MyWorkshopItem item in worlds)
            {
                flag &= UpdateMod(item);
            }
            return flag;
        }

        private static void UpdateDownloadScreen(int counter, string numMods, List<KeyValuePair<MyWorkshopItem, string>> currentMods, string sizeStr, double runningTotal, MyWorkshopItem mod)
        {
            if (m_downloadScreen != null)
            {
                string title;
                StringBuilder loadingText = new StringBuilder();
                if (mod.Title.Length <= 0x19)
                {
                    title = mod.Title;
                }
                else
                {
                    title = mod.Title.Substring(0, 0x19);
                    int length = title.LastIndexOf(' ');
                    if (length != -1)
                    {
                        title = title.Substring(0, length);
                    }
                    title = title + "...";
                }
                using (m_modLock.AcquireExclusiveUsing())
                {
                    double byteSize = runningTotal;
                    string str2 = MyUtils.FormatByteSizePrefix(ref byteSize);
                    double size = mod.Size;
                    string str3 = MyUtils.FormatByteSizePrefix(ref size);
                    string[] textArray1 = new string[] { title, " ", size.ToString("N1"), str3, "B" };
                    currentMods.Add(new KeyValuePair<MyWorkshopItem, string>(mod, string.Concat(textArray1)));
                    loadingText.Clear();
                    loadingText.AppendLine();
                    foreach (KeyValuePair<MyWorkshopItem, string> pair in currentMods)
                    {
                        loadingText.AppendLine(pair.Value);
                    }
                    object[] objArray1 = new object[9];
                    objArray1[0] = MyTexts.GetString(MyCommonTexts.DownloadingMods_Completed);
                    objArray1[1] = counter;
                    objArray1[2] = "/";
                    objArray1[3] = numMods;
                    objArray1[4] = " : ";
                    objArray1[5] = byteSize.ToString("N1");
                    objArray1[6] = str2;
                    objArray1[7] = "B/";
                    objArray1[8] = sizeStr;
                    loadingText.AppendLine(string.Concat(objArray1));
                    MySandboxGame.Static.Invoke(delegate {
                        using (m_modLock.AcquireExclusiveUsing())
                        {
                            m_downloadScreen.MessageText = loadingText;
                        }
                    }, "MySteamWorkshop::set loading text");
                }
            }
        }

        private static bool UpdateMod(MyWorkshopItem mod)
        {
            mod.UpdateState();
            if (!mod.IsUpToDate())
            {
                using (AutoResetEvent resetEvent = new AutoResetEvent(false))
                {
                    MyWorkshopItem.DownloadItemResult result = delegate (MyGameServiceCallResult result, ulong id) {
                        if (result == MyGameServiceCallResult.OK)
                        {
                            MySandboxGame.Log.WriteLine("Mod download successful.");
                        }
                        if (result != MyGameServiceCallResult.Pending)
                        {
                            MySandboxGame.Log.WriteLine($"Error during downloading: {result}");
                            resetEvent.Set();
                        }
                    };
                    mod.ItemDownloaded += result;
                    mod.Download();
                    resetEvent.WaitOne();
                    mod.ItemDownloaded -= result;
                }
            }
            return true;
        }

        public static void UpdateWorldsAsync(List<MyWorkshopItem> worlds, MyWorkshopPathInfo pathInfo, Action<bool> callbackOnFinished = null)
        {
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, () => new UpdateWorldsResult(worlds, pathInfo, callbackOnFinished), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(MyWorkshop.endActionUpdateWorld), null));
        }

        public static Category[] ModCategories =>
            m_modCategories;

        public static Category[] WorldCategories =>
            m_worldCategories;

        public static Category[] BlueprintCategories =>
            m_blueprintCategories;

        public static Category[] ScenarioCategories =>
            m_scenarioCategories;

        public class CancelToken
        {
            public bool Cancel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Category
        {
            public string Id;
            public MyStringId LocalizableName;
            public bool IsVisibleForFilter;
        }

        private class CreateWorldResult : IMyAsyncResult
        {
            public string m_createdSessionPath;

            public CreateWorldResult(MyWorkshopItem world, MyWorkshop.MyWorkshopPathInfo pathInfo, Action<bool, string> callback, bool overwrite)
            {
                this.Callback = callback;
                this.Task = Parallel.Start(() => this.Success = MyWorkshop.TryCreateWorldInstanceBlocking(world, pathInfo, out this.m_createdSessionPath, overwrite));
            }

            public ParallelTasks.Task Task { get; private set; }

            public bool Success { get; private set; }

            public Action<bool, string> Callback { get; private set; }

            public bool IsCompleted =>
                this.Task.IsComplete;
        }

        private class DownloadModsResult : IMyAsyncResult
        {
            public MyWorkshop.ResultData Result;
            public Action<bool> Callback;

            public DownloadModsResult(List<MyObjectBuilder_Checkpoint.ModItem> mods, Action<bool> onFinishedCallback, MyWorkshop.CancelToken cancelToken)
            {
                this.Callback = onFinishedCallback;
                this.Task = Parallel.Start(delegate {
                    this.Result = MyWorkshop.DownloadWorldModsBlocking(mods, cancelToken);
                    if (!this.Result.Cancel)
                    {
                        MySandboxGame.Static.Invoke(new Action(MyWorkshop.endActionDownloadMods), "DownloadModsResult::endActionDownloadMods");
                    }
                });
            }

            public ParallelTasks.Task Task { get; private set; }

            public bool IsCompleted =>
                this.Task.IsComplete;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyWorkshopPathInfo
        {
            public string Path;
            public string Suffix;
            public string NamePrefix;
            public static MyWorkshop.MyWorkshopPathInfo CreateWorldInfo() => 
                new MyWorkshop.MyWorkshopPathInfo { 
                    Path = MyWorkshop.m_workshopWorldsPath,
                    Suffix = MyWorkshop.m_workshopWorldSuffix,
                    NamePrefix = "Workshop"
                };

            public static MyWorkshop.MyWorkshopPathInfo CreateScenarioInfo() => 
                new MyWorkshop.MyWorkshopPathInfo { 
                    Path = MyWorkshop.m_workshopScenariosPath,
                    Suffix = MyWorkshop.m_workshopScenariosSuffix,
                    NamePrefix = "Scenario"
                };
        }

        private class PublishItemResult : IMyAsyncResult
        {
            public PublishItemResult(string localFolder, string publishedTitle, string publishedDescription, ulong? publishedFileId, MyPublishedFileVisibility visibility, string[] tags, HashSet<string> ignoredExtensions, HashSet<string> ignoredPaths = null, uint[] requiredDLCs = null)
            {
                this.Task = Parallel.Start(() => MyWorkshop.m_publishedFileId = MyWorkshop.PublishItemBlocking(localFolder, publishedTitle, publishedDescription, publishedFileId, visibility, tags, ignoredExtensions, ignoredPaths, requiredDLCs));
            }

            public ParallelTasks.Task Task { get; private set; }

            public bool IsCompleted =>
                this.Task.IsComplete;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ResultData
        {
            public bool Success;
            public bool Cancel;
            public List<MyWorkshopItem> Mods;
            public List<MyWorkshopItem> MismatchMods;
            public ResultData(bool success, bool cancel)
            {
                this.Success = success;
                this.Cancel = cancel;
                this.Mods = new List<MyWorkshopItem>();
                this.MismatchMods = new List<MyWorkshopItem>();
            }
        }

        private class UpdateWorldsResult : IMyAsyncResult
        {
            public UpdateWorldsResult(List<MyWorkshopItem> worlds, MyWorkshop.MyWorkshopPathInfo pathInfo, Action<bool> callback)
            {
                this.Callback = callback;
                this.Task = Parallel.Start(() => this.Success = MyWorkshop.TryUpdateWorldsBlocking(worlds, pathInfo));
            }

            public ParallelTasks.Task Task { get; private set; }

            public bool Success { get; private set; }

            public Action<bool> Callback { get; private set; }

            public bool IsCompleted =>
                this.Task.IsComplete;
        }
    }
}

