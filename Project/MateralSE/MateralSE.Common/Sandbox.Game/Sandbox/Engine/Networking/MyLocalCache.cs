namespace Sandbox.Engine.Networking
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRage.GameServices;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyLocalCache
    {
        private const string CHECKPOINT_FILE = "Sandbox.sbc";
        private const string LAST_LOADED_TIMES_FILE = "LastLoaded.sbl";
        private const string LAST_SESSION_FILE = "LastSession.sbl";
        private static readonly string activeInventoryFile = "ActiveInventory.sbl";
        private static bool m_initialized;
        public static MyObjectBuilder_LastSession LastSessionOverride;

        private static string CheckLastSession(MyObjectBuilder_LastSession lastSession)
        {
            if ((lastSession != null) && !string.IsNullOrEmpty(lastSession.Path))
            {
                string path = Path.Combine(lastSession.IsContentWorlds ? MyFileSystem.ContentPath : MyFileSystem.SavesPath, lastSession.Path);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        public static void ClearLastSessionInfo()
        {
            string path = Path.Combine(MyFileSystem.SavesPath, "LastSession.sbl");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static List<Tuple<string, MyWorldInfo>> GetAvailableAISchoolInfos() => 
            GetAvailableInfosFromDirectory("AI school scenarios", AISchoolSessionsPath);

        private static List<Tuple<string, MyWorldInfo>> GetAvailableInfosFromDirectory(string worldCategory, string worldDirectoryPath)
        {
            string str = "Loading available " + worldCategory;
            MySandboxGame.Log.WriteLine(str + " - START");
            List<Tuple<string, MyWorldInfo>> result = new List<Tuple<string, MyWorldInfo>>();
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.ALL))
            {
                GetWorldInfoFromDirectory(Path.Combine(MyFileSystem.ContentPath, worldDirectoryPath), result);
            }
            MySandboxGame.Log.WriteLine(str + " - END");
            return result;
        }

        public static List<Tuple<string, MyWorldInfo>> GetAvailableMissionInfos() => 
            GetAvailableInfosFromDirectory("mission", MissionSessionsPath);

        public static List<Tuple<string, MyWorldInfo>> GetAvailableTutorialInfos()
        {
            MySandboxGame.Log.WriteLine("Loading available tutorials - START");
            List<Tuple<string, MyWorldInfo>> result = new List<Tuple<string, MyWorldInfo>>();
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.ALL))
            {
                string str = Path.Combine("Tutorials", "Basic");
                string str2 = Path.Combine("Tutorials", "Intermediate");
                string str3 = Path.Combine("Tutorials", "Advanced");
                string str4 = Path.Combine("Tutorials", "Planetary");
                GetWorldInfoFromDirectory(Path.Combine(MyFileSystem.ContentPath, str), result);
                GetWorldInfoFromDirectory(Path.Combine(MyFileSystem.ContentPath, str2), result);
                GetWorldInfoFromDirectory(Path.Combine(MyFileSystem.ContentPath, str3), result);
                GetWorldInfoFromDirectory(Path.Combine(MyFileSystem.ContentPath, str4), result);
            }
            MySandboxGame.Log.WriteLine("Loading available tutorials - END");
            return result;
        }

        public static List<Tuple<string, MyWorldInfo>> GetAvailableWorldInfos(string customPath = null)
        {
            MySandboxGame.Log.WriteLine("Loading available saves - START");
            List<Tuple<string, MyWorldInfo>> result = new List<Tuple<string, MyWorldInfo>>();
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.ALL))
            {
                GetWorldInfoFromDirectory(customPath ?? MyFileSystem.SavesPath, result);
            }
            MySandboxGame.Log.WriteLine("Loading available saves - END");
            return result;
        }

        public static bool GetCharacterInfoFromInventoryConfig(ref string model, ref Color color)
        {
            MyObjectBuilder_SkinInventory inventory;
            if (!MyGameService.IsActive)
            {
                return false;
            }
            string path = Path.Combine(MyFileSystem.SavesPath, activeInventoryFile);
            if (!MyFileSystem.FileExists(path))
            {
                return false;
            }
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_SkinInventory>(path, out inventory))
            {
                return false;
            }
            model = inventory.Model;
            color = new Color(inventory.Color.X, inventory.Color.Y, inventory.Color.Z);
            return true;
        }

        public static MyObjectBuilder_LastSession GetLastSession()
        {
            if ((LastSessionOverride != null) && (CheckLastSession(LastSessionOverride) != null))
            {
                return LastSessionOverride;
            }
            if (!File.Exists(LastSessionPath))
            {
                return null;
            }
            MyObjectBuilder_LastSession objectBuilder = null;
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_LastSession>(LastSessionPath, out objectBuilder);
            return objectBuilder;
        }

        public static string GetLastSessionPath() => 
            CheckLastSession(GetLastSession());

        private static string GetSectorName(Vector3I sectorPosition) => 
            $"{"SANDBOX"}_{sectorPosition.X}_{sectorPosition.Y}_{sectorPosition.Z}_";

        private static string GetSectorPath(string sessionPath, Vector3I sectorPosition) => 
            Path.Combine(sessionPath, GetSectorName(sectorPosition) + ".sbs");

        public static string GetSessionSavesPath(string sessionUniqueName, bool contentFolder, bool createIfNotExists = true)
        {
            string path = !contentFolder ? Path.Combine(MyFileSystem.SavesPath, sessionUniqueName) : Path.Combine(MyFileSystem.ContentPath, ContentSessionsPath, sessionUniqueName);
            if (createIfNotExists)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static void GetWorldInfoFromDirectory(string path, List<Tuple<string, MyWorldInfo>> result)
        {
            bool flag = Directory.Exists(path);
            MySandboxGame.Log.WriteLine($"GetWorldInfoFromDirectory (Exists: {flag}) '{path}'");
            if (flag)
            {
                foreach (string str in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
                {
                    MyWorldInfo info = LoadWorldInfo(str);
                    if ((info != null) && string.IsNullOrEmpty(info.SessionName))
                    {
                        info.SessionName = Path.GetFileName(str);
                    }
                    result.Add(Tuple.Create<string, MyWorldInfo>(str, info));
                }
            }
        }

        public static MyObjectBuilder_Checkpoint LoadCheckpoint(string sessionPath, out ulong sizeInBytes)
        {
            sizeInBytes = 0L;
            string path = Path.Combine(sessionPath, "Sandbox.sbc");
            if (!File.Exists(path))
            {
                return null;
            }
            MyObjectBuilder_Checkpoint objectBuilder = null;
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Checkpoint>(path, out objectBuilder, out sizeInBytes);
            if ((objectBuilder != null) && string.IsNullOrEmpty(objectBuilder.SessionName))
            {
                objectBuilder.SessionName = Path.GetFileNameWithoutExtension(path);
            }
            if (((objectBuilder != null) && ((objectBuilder.Settings != null) && !objectBuilder.Settings.ExperimentalMode)) && ((objectBuilder.Settings.IsSettingsExperimental() || (((MySandboxGame.ConfigDedicated != null) && (MySandboxGame.ConfigDedicated.Plugins != null)) && (MySandboxGame.ConfigDedicated.Plugins.Count != 0))) || (MySandboxGame.Config.ExperimentalMode && (MySandboxGame.ConfigDedicated == null))))
            {
                objectBuilder.Settings.ExperimentalMode = true;
            }
            return objectBuilder;
        }

        public static MyObjectBuilder_CubeGrid LoadCubeGrid(string sessionPath, string fileName, out ulong sizeInBytes)
        {
            MyObjectBuilder_CubeGrid grid;
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_CubeGrid>(Path.Combine(sessionPath, fileName), out grid, out sizeInBytes);
            if (grid != null)
            {
                return grid;
            }
            MySandboxGame.Log.WriteLine("Incorrect save data");
            return null;
        }

        public static void LoadInventoryConfig(MyCharacter character, bool setModel = true)
        {
            MyObjectBuilder_SkinInventory inventory;
            if (character == null)
            {
                throw new ArgumentNullException("character");
            }
            if (!MyGameService.IsActive)
            {
                return;
            }
            string path = Path.Combine(MyFileSystem.SavesPath, activeInventoryFile);
            if (!MyFileSystem.FileExists(path))
            {
                ResetAllInventorySlots(character);
                return;
            }
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_SkinInventory>(path, out inventory))
            {
                ResetAllInventorySlots(character);
                return;
            }
            if ((inventory.Character != null) && (MyGameService.InventoryItems != null))
            {
                MyAssetModifierComponent comp;
                List<MyGameInventoryItem> items = new List<MyGameInventoryItem>();
                List<MyGameInventoryItemSlot> list2 = Enum.GetValues(typeof(MyGameInventoryItemSlot)).Cast<MyGameInventoryItemSlot>().ToList<MyGameInventoryItemSlot>();
                list2.Remove(MyGameInventoryItemSlot.None);
                using (List<ulong>.Enumerator enumerator = inventory.Character.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyGameInventoryItem item = MyGameService.InventoryItems.FirstOrDefault<MyGameInventoryItem>(delegate (MyGameInventoryItem i) {
                            ulong itemId;
                            return i.ID == itemId;
                        });
                        if (item != null)
                        {
                            item.IsInUse = true;
                            items.Add(item);
                            list2.Remove(item.ItemDefinition.ItemSlot);
                        }
                    }
                }
                if (!character.Components.TryGet<MyAssetModifierComponent>(out comp))
                {
                    goto TR_0007;
                }
                else
                {
                    MyGameService.GetItemsCheckData(items, checkDataResult => comp.TryAddAssetModifier(checkDataResult));
                    foreach (MyGameInventoryItemSlot slot in list2)
                    {
                        comp.ResetSlot(slot);
                    }
                    goto TR_0007;
                }
            }
            ResetAllInventorySlots(character);
        TR_0007:
            if (setModel && !string.IsNullOrEmpty(inventory.Model))
            {
                character.ModelName = inventory.Model;
            }
            character.ColorMask = (Vector3) inventory.Color;
        }

        public static void LoadInventoryConfig(MyEntity toolEntity, MyAssetModifierComponent skinComponent)
        {
            if (toolEntity == null)
            {
                throw new ArgumentNullException("toolEntity");
            }
            if (skinComponent == null)
            {
                throw new ArgumentNullException("skinComponent");
            }
            if (MyGameService.IsActive)
            {
                MyObjectBuilder_SkinInventory inventory;
                string path = Path.Combine(MyFileSystem.SavesPath, activeInventoryFile);
                switch (inventory.Tools)
                {
                    case ((!MyFileSystem.FileExists(path) || !MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_SkinInventory>(path, out inventory)) || ((null) || (null))):
                        break;

                    default:
                    {
                        IMyHandheldGunObject<MyDeviceBase> obj2 = toolEntity as IMyHandheldGunObject<MyDeviceBase>;
                        MyPhysicalItemDefinition physicalItemDefinition = obj2.PhysicalItemDefinition;
                        MyGameInventoryItemSlot none = MyGameInventoryItemSlot.None;
                        if (obj2 is MyHandDrill)
                        {
                            none = MyGameInventoryItemSlot.Drill;
                        }
                        else if (obj2 is MyAutomaticRifleGun)
                        {
                            none = MyGameInventoryItemSlot.Rifle;
                        }
                        else if (obj2 is MyWelder)
                        {
                            none = MyGameInventoryItemSlot.Welder;
                        }
                        else if (obj2 is MyAngleGrinder)
                        {
                            none = MyGameInventoryItemSlot.Grinder;
                        }
                        if (none != MyGameInventoryItemSlot.None)
                        {
                            List<MyGameInventoryItem> items = new List<MyGameInventoryItem>();
                            using (List<ulong>.Enumerator enumerator = inventory.Tools.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    MyGameInventoryItem item = MyGameService.InventoryItems.FirstOrDefault<MyGameInventoryItem>(delegate (MyGameInventoryItem i) {
                                        ulong itemId;
                                        return i.ID == itemId;
                                    });
                                    if ((item != null) && ((physicalItemDefinition != null) && ((physicalItemDefinition == null) || (item.ItemDefinition.ItemSlot == none))))
                                    {
                                        item.IsInUse = true;
                                        items.Add(item);
                                    }
                                }
                            }
                            MyGameService.GetItemsCheckData(items, checkDataResult => skinComponent.TryAddAssetModifier(checkDataResult));
                        }
                        break;
                    }
                }
            }
        }

        private static MyObjectBuilder_Sector LoadSector(string path, bool allowXml, out ulong sizeInBytes, out bool needsXml)
        {
            MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Sector>();
            sizeInBytes = 0L;
            needsXml = false;
            MyObjectBuilder_Sector objectBuilder = null;
            string str = path + MyObjectBuilderSerializer.ProtobufferExtension;
            if (!MyFileSystem.FileExists(str))
            {
                if (!allowXml)
                {
                    needsXml = true;
                }
                else
                {
                    MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Sector>(path, out objectBuilder, out sizeInBytes);
                    if (!MyFileSystem.FileExists(str))
                    {
                        MyObjectBuilderSerializer.SerializePB(path + MyObjectBuilderSerializer.ProtobufferExtension, false, objectBuilder);
                    }
                }
            }
            else
            {
                MyObjectBuilderSerializer.DeserializePB<MyObjectBuilder_Sector>(str, out objectBuilder, out sizeInBytes);
                if ((objectBuilder == null) || (objectBuilder.SectorObjects == null))
                {
                    if (!allowXml)
                    {
                        needsXml = true;
                    }
                    else
                    {
                        MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Sector>(path, out objectBuilder, out sizeInBytes);
                        if (objectBuilder != null)
                        {
                            MyObjectBuilderSerializer.SerializePB(str, false, objectBuilder);
                        }
                    }
                }
            }
            if (objectBuilder != null)
            {
                return objectBuilder;
            }
            MySandboxGame.Log.WriteLine("Incorrect save data");
            return null;
        }

        public static MyObjectBuilder_Sector LoadSector(string sessionPath, Vector3I sectorPosition, bool allowXml, out ulong sizeInBytes, out bool needsXml) => 
            LoadSector(GetSectorPath(sessionPath, sectorPosition), allowXml, out sizeInBytes, out needsXml);

        private static MyWorldInfo LoadWorldInfo(string sessionPath)
        {
            MyWorldInfo info = null;
            try
            {
                XDocument document = null;
                string path = Path.Combine(sessionPath, "Sandbox.sbc");
                if (File.Exists(path))
                {
                    ulong num;
                    info = new MyWorldInfo();
                    using (Stream stream = MyFileSystem.OpenRead(path).UnwrapGZip())
                    {
                        document = XDocument.Load(stream);
                    }
                    XElement root = document.Root;
                    XElement element2 = root.Element("SessionName");
                    XElement element3 = root.Element("Description");
                    XElement element4 = root.Element("LastSaveTime");
                    root.Element("WorldID");
                    XElement element5 = root.Element("WorkshopId");
                    XElement element6 = root.Element("Briefing");
                    XElement element1 = root.Element("Settings");
                    XElement element7 = (element1 != null) ? root.Element("Settings").Element("ScenarioEditMode") : null;
                    XElement element8 = (element1 != null) ? root.Element("Settings").Element("ExperimentalMode") : null;
                    if (element8 != null)
                    {
                        bool.TryParse(element8.Value, out info.IsExperimental);
                    }
                    if (element2 != null)
                    {
                        info.SessionName = MyStatControlText.SubstituteTexts(element2.Value, null);
                    }
                    if (element3 != null)
                    {
                        info.Description = element3.Value;
                    }
                    if (element4 != null)
                    {
                        DateTime.TryParse(element4.Value, out info.LastSaveTime);
                    }
                    if ((element5 != null) && ulong.TryParse(element5.Value, out num))
                    {
                        info.WorkshopId = new ulong?(num);
                    }
                    if (element6 != null)
                    {
                        info.Briefing = element6.Value;
                    }
                    if (element7 != null)
                    {
                        bool.TryParse(element7.Value, out info.ScenarioEditMode);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine(exception);
                info.IsCorrupted = true;
            }
            return info;
        }

        public static void ResetAllInventorySlots(MyCharacter character)
        {
            MyAssetModifierComponent component;
            if (character.Components.TryGet<MyAssetModifierComponent>(out component))
            {
                foreach (MyGameInventoryItemSlot slot in Enum.GetValues(typeof(MyGameInventoryItemSlot)))
                {
                    if (slot != MyGameInventoryItemSlot.None)
                    {
                        component.ResetSlot(slot);
                    }
                }
            }
        }

        public static bool SaveCheckpoint(MyObjectBuilder_Checkpoint checkpoint, string sessionPath)
        {
            ulong num;
            return SaveCheckpoint(checkpoint, sessionPath, out num);
        }

        public static bool SaveCheckpoint(MyObjectBuilder_Checkpoint checkpoint, string sessionPath, out ulong sizeInBytes) => 
            MyObjectBuilderSerializer.SerializeXML(Path.Combine(sessionPath, "Sandbox.sbc"), MySandboxGame.Config.CompressSaveGames, checkpoint, out sizeInBytes, null);

        public static void SaveInventoryConfig(MyCharacter character)
        {
            if ((character != null) && MyGameService.IsActive)
            {
                ulong num;
                MyObjectBuilder_SkinInventory objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SkinInventory>();
                objectBuilder.Character = new List<ulong>();
                objectBuilder.Color = character.ColorMask;
                objectBuilder.Model = character.ModelName;
                objectBuilder.Tools = new List<ulong>();
                if (MyGameService.InventoryItems != null)
                {
                    foreach (MyGameInventoryItem item in MyGameService.InventoryItems)
                    {
                        if (!item.IsInUse)
                        {
                            continue;
                        }
                        MyGameInventoryItemSlot itemSlot = item.ItemDefinition.ItemSlot;
                        if (itemSlot <= MyGameInventoryItemSlot.Suit)
                        {
                            objectBuilder.Character.Add(item.ID);
                            continue;
                        }
                        if ((itemSlot - 6) <= MyGameInventoryItemSlot.Gloves)
                        {
                            objectBuilder.Tools.Add(item.ID);
                        }
                    }
                }
                MyObjectBuilderSerializer.SerializeXML(Path.Combine(MyFileSystem.SavesPath, activeInventoryFile), false, objectBuilder, out num, null);
            }
        }

        public static bool SaveLastSessionInfo(string sessionPath, bool isOnline, bool isLobby, string gameName, string serverIP, int serverPort)
        {
            ulong num;
            MyObjectBuilder_LastSession objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_LastSession>();
            objectBuilder.IsOnline = isOnline;
            objectBuilder.IsLobby = isLobby;
            if (!isOnline)
            {
                if (sessionPath != null)
                {
                    objectBuilder.Path = sessionPath;
                    objectBuilder.GameName = gameName;
                    objectBuilder.IsContentWorlds = sessionPath.StartsWith(MyFileSystem.ContentPath, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            else if (isLobby)
            {
                objectBuilder.GameName = gameName;
                objectBuilder.ServerIP = serverIP;
            }
            else
            {
                objectBuilder.GameName = gameName;
                objectBuilder.ServerIP = serverIP;
                objectBuilder.ServerPort = serverPort;
            }
            return MyObjectBuilderSerializer.SerializeXML(LastSessionPath, false, objectBuilder, out num, null);
        }

        public static bool SaveRespawnShip(MyObjectBuilder_CubeGrid cubegrid, string sessionPath, string fileName, out ulong sizeInBytes) => 
            MyObjectBuilderSerializer.SerializeXML(Path.Combine(sessionPath, fileName), MySandboxGame.Config.CompressSaveGames, cubegrid, out sizeInBytes, null);

        public static bool SaveSector(MyObjectBuilder_Sector sector, string sessionPath, Vector3I sectorPosition, out ulong sizeInBytes)
        {
            string sectorPath = GetSectorPath(sessionPath, sectorPosition);
            bool flag = MyObjectBuilderSerializer.SerializeXML(sectorPath, MySandboxGame.Config.CompressSaveGames, sector, out sizeInBytes, null);
            MyObjectBuilderSerializer.SerializePB(sectorPath + MyObjectBuilderSerializer.ProtobufferExtension, MySandboxGame.Config.CompressSaveGames, sector, out sizeInBytes);
            return flag;
        }

        public static string LastLoadedTimesPath =>
            Path.Combine(MyFileSystem.SavesPath, "LastLoaded.sbl");

        public static string LastSessionPath =>
            Path.Combine(MyFileSystem.SavesPath, "LastSession.sbl");

        public static string ContentSessionsPath =>
            "Worlds";

        public static string MissionSessionsPath =>
            "Missions";

        public static string AISchoolSessionsPath =>
            "AISchool";
    }
}

