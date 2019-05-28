namespace Sandbox.Game.GUI
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
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
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyBlueprintUtils
    {
        public static readonly string THUMB_IMAGE_NAME = "thumb.png";
        public static readonly string DEFAULT_SCRIPT_NAME = "Script";
        public static readonly string SCRIPT_EXTENSION = ".cs";
        public static readonly string BLUEPRINT_WORKSHOP_EXTENSION = ".sbb";
        public static readonly string BLUEPRINT_LOCAL_NAME = "bp.sbc";
        public static readonly string STEAM_THUMBNAIL_NAME = @"Textures\GUI\Icons\IngameProgrammingIcon.png";
        public static readonly string BLUEPRINT_CLOUD_DIRECTORY = "Blueprints/cloud";
        public static readonly string SCRIPTS_DIRECTORY = "IngameScripts";
        public static readonly string BLUEPRINT_DIRECTORY = "Blueprints";
        public static readonly string BLUEPRINT_DEFAULT_DIRECTORY = Path.Combine(MyFileSystem.ContentPath, "Data", "Blueprints");
        public static readonly string SCRIPT_FOLDER_LOCAL = Path.Combine(MyFileSystem.UserDataPath, SCRIPTS_DIRECTORY, "local");
        public static readonly string SCRIPT_FOLDER_WORKSHOP = Path.Combine(MyFileSystem.UserDataPath, SCRIPTS_DIRECTORY, "workshop");
        public static readonly string BLUEPRINT_FOLDER_LOCAL = Path.Combine(MyFileSystem.UserDataPath, BLUEPRINT_DIRECTORY, "local");
        public static readonly string BLUEPRINT_FOLDER_WORKSHOP = Path.Combine(MyFileSystem.UserDataPath, BLUEPRINT_DIRECTORY, "workshop");
        public static readonly string BLUEPRINT_WORKSHOP_TEMP = Path.Combine(BLUEPRINT_FOLDER_WORKSHOP, "temp");

        private MyBlueprintUtils()
        {
        }

        public static bool CopyFileFromCloud(string pathFull, string pathRel)
        {
            byte[] buffer = MyGameService.LoadFromCloud(pathRel);
            if (buffer == null)
            {
                return false;
            }
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                stream.Seek(0L, SeekOrigin.Begin);
                MyFileSystem.CreateDirectoryRecursive(Path.GetDirectoryName(pathFull));
                using (FileStream stream2 = new FileStream(pathFull, FileMode.OpenOrCreate))
                {
                    stream.CopyTo(stream2);
                    stream2.Flush();
                }
            }
            return true;
        }

        public static MyGuiControlButton CreateButton(MyGuiScreenDebugBase screen, float usableWidth, StringBuilder text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?(), float textScale = 1f)
        {
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlButton button = screen.AddButton(text, onClick, null, textColor, size, true, true);
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

        public static int GetNumberOfBlocks(ref MyObjectBuilder_Definitions prefab)
        {
            int num = 0;
            foreach (MyObjectBuilder_CubeGrid grid in prefab.ShipBlueprints[0].CubeGrids)
            {
                num += grid.CubeBlocks.Count;
            }
            return num;
        }

        public static bool IsItem_Blueprint(string path) => 
            File.Exists(path + @"\bp.sbc");

        public static bool IsItem_Script(string path) => 
            File.Exists(path + @"\Script.cs");

        public static MyObjectBuilder_Definitions LoadPrefab(string filePath)
        {
            MyObjectBuilder_Definitions objectBuilder = null;
            bool flag = false;
            string path = filePath + MyObjectBuilderSerializer.ProtobufferExtension;
            if (!MyFileSystem.FileExists(path))
            {
                if (MyFileSystem.FileExists(filePath))
                {
                    flag = MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(filePath, out objectBuilder);
                    if (flag)
                    {
                        MyObjectBuilderSerializer.SerializePB(path, false, objectBuilder);
                    }
                }
            }
            else
            {
                flag = MyObjectBuilderSerializer.DeserializePB<MyObjectBuilder_Definitions>(path, out objectBuilder);
                if ((objectBuilder == null) || (objectBuilder.ShipBlueprints == null))
                {
                    flag = MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(filePath, out objectBuilder);
                    if (objectBuilder != null)
                    {
                        MyObjectBuilderSerializer.SerializePB(path, false, objectBuilder);
                    }
                }
            }
            return (flag ? objectBuilder : null);
        }

        public static MyObjectBuilder_Definitions LoadPrefabFromCloud(MyBlueprintItemInfo info)
        {
            MyObjectBuilder_Definitions objectBuilder = null;
            if (!string.IsNullOrEmpty(info.CloudPathPB))
            {
                byte[] buffer = MyGameService.LoadFromCloud(info.CloudPathPB);
                if (buffer == null)
                {
                    return objectBuilder;
                }
                else
                {
                    using (MemoryStream stream = new MemoryStream(buffer))
                    {
                        MyObjectBuilderSerializer.DeserializePB<MyObjectBuilder_Definitions>(stream, out objectBuilder);
                        return objectBuilder;
                    }
                }
            }
            if (!string.IsNullOrEmpty(info.CloudPathXML))
            {
                byte[] buffer = MyGameService.LoadFromCloud(info.CloudPathXML);
                if (buffer != null)
                {
                    using (MemoryStream stream2 = new MemoryStream(buffer))
                    {
                        using (Stream stream3 = stream2.UnwrapGZip())
                        {
                            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(stream3, out objectBuilder);
                        }
                    }
                }
            }
            return objectBuilder;
        }

        public static MyObjectBuilder_Definitions LoadWorkshopPrefab(string archive, ulong? publishedItemId, bool isOldBlueprintScreen)
        {
            MyWorkshopItem item;
            if ((!File.Exists(archive) && !MyFileSystem.DirectoryExists(archive)) || (publishedItemId == null))
            {
                return null;
            }
            if (isOldBlueprintScreen)
            {
                item = MyGuiBlueprintScreen.m_subscribedItemsList.Find(delegate (MyWorkshopItem item) {
                    ulong? nullable = publishedItemId;
                    return (item.Id == nullable.GetValueOrDefault()) & (nullable != null);
                });
            }
            else
            {
                using (MyGuiBlueprintScreen_Reworked.SubscribedItemsLock.AcquireSharedUsing())
                {
                    item = MyGuiBlueprintScreen_Reworked.GetSubscribedItemsList(Content.Blueprint).Find(delegate (MyWorkshopItem item) {
                        ulong? nullable = publishedItemId;
                        return (item.Id == nullable.GetValueOrDefault()) & (nullable != null);
                    });
                }
            }
            if (item == null)
            {
                return null;
            }
            string path = Path.Combine(archive, BLUEPRINT_LOCAL_NAME);
            string str2 = path + MyObjectBuilderSerializer.ProtobufferExtension;
            if (!MyFileSystem.FileExists(str2) && (publishedItemId != null))
            {
                string text1 = Path.Combine(BLUEPRINT_WORKSHOP_TEMP, publishedItemId.Value.ToString());
                MyFileSystem.EnsureDirectoryExists(text1);
                str2 = Path.Combine(text1, BLUEPRINT_LOCAL_NAME) + MyObjectBuilderSerializer.ProtobufferExtension;
            }
            bool flag = false;
            MyObjectBuilder_Definitions objectBuilder = null;
            bool flag2 = MyFileSystem.FileExists(path);
            bool flag3 = false;
            if (MyFileSystem.FileExists(str2) & flag2)
            {
                FileInfo info = new FileInfo(path);
                if (new FileInfo(str2).LastWriteTimeUtc >= info.LastWriteTimeUtc)
                {
                    flag3 = true;
                }
            }
            if (flag3)
            {
                flag = MyObjectBuilderSerializer.DeserializePB<MyObjectBuilder_Definitions>(str2, out objectBuilder);
                if ((objectBuilder == null) || (objectBuilder.ShipBlueprints == null))
                {
                    flag = MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(path, out objectBuilder);
                }
            }
            else if (flag2)
            {
                flag = MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(path, out objectBuilder);
                if (flag && (publishedItemId != null))
                {
                    MyObjectBuilderSerializer.SerializePB(str2, false, objectBuilder);
                }
            }
            if (!flag)
            {
                return null;
            }
            objectBuilder.ShipBlueprints[0].Description = item.Description;
            objectBuilder.ShipBlueprints[0].CubeGrids[0].DisplayName = item.Title;
            objectBuilder.ShipBlueprints[0].DLCs = new string[item.DLCs.Count];
            int index = 0;
            while (true)
            {
                MyDLCs.MyDLC ydlc;
                ListReader<uint> dLCs = item.DLCs;
                if (index >= dLCs.Count)
                {
                    return objectBuilder;
                }
                if (MyDLCs.TryGetDLC(item.DLCs[index], out ydlc))
                {
                    objectBuilder.ShipBlueprints[0].DLCs[index] = ydlc.Name;
                }
                index++;
            }
        }

        public static void PublishBlueprint(MyObjectBuilder_Definitions prefab, string blueprintName, string currentLocalDirectory, Action<ulong> publishCallback = null)
        {
            string file = Path.Combine(BLUEPRINT_FOLDER_LOCAL, currentLocalDirectory, blueprintName);
            string title = prefab.ShipBlueprints[0].CubeGrids[0].DisplayName;
            string description = prefab.ShipBlueprints[0].Description;
            ulong publishId = prefab.ShipBlueprints[0].WorkshopId;
            StringBuilder messageCaption = new StringBuilder("Publish");
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder("Do you want to publish this blueprint?"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum val) {
                if (val == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    Action<MyGuiScreenMessageBox.ResultEnum, string[]> callback = delegate (MyGuiScreenMessageBox.ResultEnum tagsResult, string[] outTags) {
                        if (tagsResult == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            HashSet<uint> source = new HashSet<uint>();
                            MyObjectBuilder_ShipBlueprintDefinition[] shipBlueprints = prefab.ShipBlueprints;
                            int index = 0;
                            while (true)
                            {
                                if (index >= shipBlueprints.Length)
                                {
                                    MyWorkshop.PublishBlueprintAsync(file, title, description, new ulong?(publishId), outTags, source.ToArray<uint>(), MyPublishedFileVisibility.Public, delegate (bool success, MyGameServiceCallResult result, ulong publishedFileId) {
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
                                            if (publishCallback != null)
                                            {
                                                publishCallback(publishedFileId);
                                            }
                                            prefab.ShipBlueprints[0].WorkshopId = publishedFileId;
                                            SavePrefabToFile(prefab, blueprintName, currentLocalDirectory, true, MyBlueprintTypeEnum.LOCAL);
                                            nullable = null;
                                            nullable = null;
                                            nullable = null;
                                            nullable = null;
                                            nullable2 = null;
                                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublished), MySession.Platform), new StringBuilder("BLUEPRINT PUBLISHED"), nullable, nullable, nullable, nullable, a => MyGameService.OpenOverlayUrl($"http://steamcommunity.com/sharedfiles/filedetails/?id={publishedFileId}"), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                                        }
                                    });
                                    break;
                                }
                                MyObjectBuilder_ShipBlueprintDefinition definition = shipBlueprints[index];
                                if (definition.DLCs != null)
                                {
                                    foreach (string str in definition.DLCs)
                                    {
                                        MyDLCs.MyDLC ydlc;
                                        if (MyDLCs.TryGetDLC(str, out ydlc))
                                        {
                                            source.Add(ydlc.AppId);
                                        }
                                        else
                                        {
                                            uint num3;
                                            if (uint.TryParse(str, out num3))
                                            {
                                                source.Add(num3);
                                            }
                                        }
                                    }
                                }
                                index++;
                            }
                        }
                    };
                    if (MyWorkshop.BlueprintCategories.Length != 0)
                    {
                        MyGuiSandbox.AddScreen(new MyGuiScreenWorkshopTags("blueprint", MyWorkshop.BlueprintCategories, null, callback));
                    }
                    else
                    {
                        string[] textArray1 = new string[] { "blueprint" };
                        callback(MyGuiScreenMessageBox.ResultEnum.YES, textArray1);
                    }
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public static void PublishScript(MyGuiControlButton button, string directory, MyBlueprintItemInfo script, Action OnPublished)
        {
            MyObjectBuilder_ModInfo info;
            string path = Path.Combine(SCRIPT_FOLDER_LOCAL, directory, script.Data.Name, "modinfo.sbmi");
            if (File.Exists(path) && MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_ModInfo>(path, out info))
            {
                script.PublishedItemId = new ulong?(info.WorkshopId);
            }
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.LoadScreenButtonPublish);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ProgrammableBlock_PublishScriptDialogText), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum val) {
                if (val == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    string fullPath = Path.Combine(SCRIPT_FOLDER_LOCAL, directory, script.Data.Name);
                    MyWorkshop.PublishIngameScriptAsync(fullPath, script.Data.Name, script.Data.Description ?? "", script.PublishedItemId, MyPublishedFileVisibility.Public, delegate (bool success, MyGameServiceCallResult result, ulong publishedFileId) {
                        MyStringId? nullable;
                        Vector2? nullable2;
                        if (success)
                        {
                            MyWorkshop.GenerateModInfo(fullPath, publishedFileId, Sync.MyId);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublished), MySession.Platform), MyTexts.Get(MySpaceTexts.ProgrammableBlock_PublishScriptPublished), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum a) {
                                OnPublished();
                                MyGameService.OpenOverlayUrl(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, publishedFileId));
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
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionWorldPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    });
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public static void SavePrefabToFile(MyObjectBuilder_Definitions prefab, string name, string currentDirectory, bool replace = false, MyBlueprintTypeEnum type = 1)
        {
            if ((type == MyBlueprintTypeEnum.LOCAL) && MySandboxGame.Config.EnableSteamCloud)
            {
                type = MyBlueprintTypeEnum.CLOUD;
            }
            string file = string.Empty;
            switch (type)
            {
                case MyBlueprintTypeEnum.STEAM:
                case MyBlueprintTypeEnum.SHARED:
                case MyBlueprintTypeEnum.DEFAULT:
                    file = Path.Combine(BLUEPRINT_FOLDER_WORKSHOP, "temp", name);
                    break;

                case MyBlueprintTypeEnum.LOCAL:
                    file = Path.Combine(BLUEPRINT_FOLDER_LOCAL, currentDirectory, name);
                    break;

                case MyBlueprintTypeEnum.CLOUD:
                    file = Path.Combine(BLUEPRINT_CLOUD_DIRECTORY, name);
                    break;

                default:
                    break;
            }
            string filePath = string.Empty;
            try
            {
                if (type != MyBlueprintTypeEnum.CLOUD)
                {
                    SaveToDisk(prefab, name, replace, type, file, currentDirectory, ref filePath);
                }
                else
                {
                    filePath = Path.Combine(file, BLUEPRINT_LOCAL_NAME);
                    SaveToCloud(prefab, filePath, replace);
                }
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine($"Failed to write prefab at file {filePath}, message: {exception.Message}, stack:{exception.StackTrace}");
            }
        }

        public static void SaveToCloud(MyObjectBuilder_Definitions prefab, string filePath, bool replace)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bool flag1 = MyObjectBuilderSerializer.SerializeXML(stream, prefab, MyObjectBuilderSerializer.XmlCompression.Gzip, null);
                if (flag1)
                {
                    byte[] buffer = stream.ToArray();
                    MyGameService.SaveToCloudAsync(filePath, buffer, delegate (bool result) {
                        if (result)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                if (MyObjectBuilderSerializer.SerializePB(stream, prefab))
                                {
                                    byte[] buffer = stream.ToArray();
                                    filePath = filePath + MyObjectBuilderSerializer.ProtobufferExtension;
                                    MyGameService.SaveToCloud(filePath, buffer);
                                }
                            }
                        }
                    });
                }
                if (!flag1)
                {
                    ShowBlueprintSaveError();
                }
            }
        }

        public static void SaveToCloudFile(string pathFull, string pathRel)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (FileStream stream2 = new FileStream(pathFull, FileMode.Open, FileAccess.Read))
                {
                    stream2.CopyTo(stream);
                    byte[] buffer = stream.ToArray();
                    MyGameService.SaveToCloud(pathRel, buffer);
                }
            }
        }

        private static void SaveToDisk(MyObjectBuilder_Definitions prefab, string name, bool replace, MyBlueprintTypeEnum type, string file, string currentDirectory, ref string filePath)
        {
            if (!replace)
            {
                int num = 1;
                while (true)
                {
                    if (!MyFileSystem.DirectoryExists(file))
                    {
                        if (num > 1)
                        {
                            string text1 = name + new StringBuilder("_" + (num - 1));
                            name = text1;
                        }
                        break;
                    }
                    file = Path.Combine(BLUEPRINT_FOLDER_LOCAL, currentDirectory, name + "_" + num);
                    num++;
                }
            }
            filePath = Path.Combine(file, BLUEPRINT_LOCAL_NAME);
            bool flag1 = MyObjectBuilderSerializer.SerializeXML(filePath, false, prefab, null);
            if (flag1 && (type == MyBlueprintTypeEnum.LOCAL))
            {
                MyObjectBuilderSerializer.SerializePB(filePath + MyObjectBuilderSerializer.ProtobufferExtension, false, prefab);
            }
            if (!flag1)
            {
                StringBuilder messageCaption = new StringBuilder("Error");
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("There was a problem with saving blueprint"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                if (Directory.Exists(file))
                {
                    Directory.Delete(file, true);
                }
            }
        }

        private static void ShowBlueprintSaveError()
        {
            StringBuilder messageCaption = new StringBuilder("Error");
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("There was a problem with saving blueprint/script"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }
    }
}

