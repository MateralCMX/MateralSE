namespace Sandbox.ModAPI
{
    using ProtoBuf;
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;
    using VRage.FileSystem;
    using VRage.Game.ModAPI;
    using VRage.Utils;

    public class MyAPIUtilities : IMyUtilities, IMyGamePaths
    {
        private const string STORAGE_FOLDER = "Storage";
        public static readonly MyAPIUtilities Static = new MyAPIUtilities();
        [CompilerGenerated]
        private MessageEnteredDel MessageEntered;
        [CompilerGenerated]
        private Action<ulong, string> MessageRecieved;
        private Dictionary<long, List<Action<object>>> m_registeredListeners = new Dictionary<long, List<Action<object>>>();
        public Dictionary<string, object> Variables = new Dictionary<string, object>();

        public event MessageEnteredDel MessageEntered
        {
            [CompilerGenerated] add
            {
                MessageEnteredDel messageEntered = this.MessageEntered;
                while (true)
                {
                    MessageEnteredDel a = messageEntered;
                    MessageEnteredDel del3 = (MessageEnteredDel) Delegate.Combine(a, value);
                    messageEntered = Interlocked.CompareExchange<MessageEnteredDel>(ref this.MessageEntered, del3, a);
                    if (ReferenceEquals(messageEntered, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MessageEnteredDel messageEntered = this.MessageEntered;
                while (true)
                {
                    MessageEnteredDel source = messageEntered;
                    MessageEnteredDel del3 = (MessageEnteredDel) Delegate.Remove(source, value);
                    messageEntered = Interlocked.CompareExchange<MessageEnteredDel>(ref this.MessageEntered, del3, source);
                    if (ReferenceEquals(messageEntered, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<ulong, string> MessageRecieved
        {
            [CompilerGenerated] add
            {
                Action<ulong, string> messageRecieved = this.MessageRecieved;
                while (true)
                {
                    Action<ulong, string> a = messageRecieved;
                    Action<ulong, string> action3 = (Action<ulong, string>) Delegate.Combine(a, value);
                    messageRecieved = Interlocked.CompareExchange<Action<ulong, string>>(ref this.MessageRecieved, action3, a);
                    if (ReferenceEquals(messageRecieved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong, string> messageRecieved = this.MessageRecieved;
                while (true)
                {
                    Action<ulong, string> source = messageRecieved;
                    Action<ulong, string> action3 = (Action<ulong, string>) Delegate.Remove(source, value);
                    messageRecieved = Interlocked.CompareExchange<Action<ulong, string>>(ref this.MessageRecieved, action3, source);
                    if (ReferenceEquals(messageRecieved, source))
                    {
                        return;
                    }
                }
            }
        }

        event MessageEnteredDel IMyUtilities.MessageEntered
        {
            add
            {
                this.MessageEntered += value;
            }
            remove
            {
                this.MessageEntered -= value;
            }
        }

        event Action<ulong, string> IMyUtilities.MessageRecieved
        {
            add
            {
                this.MessageRecieved += value;
            }
            remove
            {
                this.MessageRecieved -= value;
            }
        }

        public void EnterMessage(string messageText, ref bool sendToOthers)
        {
            MessageEnteredDel messageEntered = this.MessageEntered;
            if (messageEntered != null)
            {
                messageEntered(messageText, ref sendToOthers);
            }
        }

        public void RecieveMessage(ulong senderSteamId, string message)
        {
            Action<ulong, string> messageRecieved = this.MessageRecieved;
            if (messageRecieved != null)
            {
                messageRecieved(senderSteamId, message);
            }
        }

        public void RegisterMessageHandler(long id, Action<object> messageHandler)
        {
            List<Action<object>> list;
            if (this.m_registeredListeners.TryGetValue(id, out list))
            {
                list.Add(messageHandler);
            }
            else
            {
                List<Action<object>> list1 = new List<Action<object>>();
                list1.Add(messageHandler);
                this.m_registeredListeners[id] = list1;
            }
        }

        public void SendModMessage(long id, object payload)
        {
            List<Action<object>> list;
            if (this.m_registeredListeners.TryGetValue(id, out list))
            {
                using (List<Action<object>>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current(payload);
                    }
                }
            }
        }

        private string StripDllExtIfNecessary(string name)
        {
            string str = ".dll";
            return (!name.EndsWith(str, StringComparison.InvariantCultureIgnoreCase) ? name : name.Substring(0, name.Length - str.Length));
        }

        public void UnregisterMessageHandler(long id, Action<object> messageHandler)
        {
            List<Action<object>> list;
            if (this.m_registeredListeners.TryGetValue(id, out list))
            {
                list.Remove(messageHandler);
            }
        }

        IMyHudNotification IMyUtilities.CreateNotification(string message, int disappearTimeMs, string font)
        {
            MyHudNotification notification = new MyHudNotification(MyCommonTexts.CustomText, disappearTimeMs, font, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] arguments = new object[] { message };
            notification.SetTextFormatArguments(arguments);
            return notification;
        }

        void IMyUtilities.DeleteFileInGlobalStorage(string file)
        {
            if (((IMyUtilities) this).FileExistsInGlobalStorage(file))
            {
                File.Delete(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
            }
        }

        void IMyUtilities.DeleteFileInLocalStorage(string file, System.Type callingType)
        {
            if (((IMyUtilities) this).FileExistsInLocalStorage(file, callingType))
            {
                File.Delete(Path.Combine(MyFileSystem.UserDataPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
            }
        }

        void IMyUtilities.DeleteFileInWorldStorage(string file, System.Type callingType)
        {
            if (((IMyUtilities) this).FileExistsInLocalStorage(file, callingType))
            {
                File.Delete(Path.Combine(MySession.Static.CurrentPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
            }
        }

        bool IMyUtilities.FileExistsInGlobalStorage(string file) => 
            ((file.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) ? File.Exists(Path.Combine(MyFileSystem.UserDataPath, "Storage", file)) : false);

        bool IMyUtilities.FileExistsInLocalStorage(string file, System.Type callingType) => 
            ((file.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) ? File.Exists(Path.Combine(MyFileSystem.UserDataPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file)) : false);

        bool IMyUtilities.FileExistsInWorldStorage(string file, System.Type callingType) => 
            ((file.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) ? File.Exists(Path.Combine(MySession.Static.CurrentPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file)) : false);

        IMyHudObjectiveLine IMyUtilities.GetObjectiveLine() => 
            MyHud.ObjectiveLine;

        string IMyUtilities.GetTypeName(System.Type type) => 
            type.Name;

        bool IMyUtilities.GetVariable<T>(string name, out T value)
        {
            object obj2;
            value = default(T);
            if (!this.Variables.TryGetValue(name, out obj2) || !(obj2 is T))
            {
                return false;
            }
            value = (T) obj2;
            return true;
        }

        void IMyUtilities.InvokeOnGameThread(Action action, string invokerName)
        {
            if (MySandboxGame.Static != null)
            {
                MySandboxGame.Static.Invoke(action, invokerName);
            }
        }

        BinaryReader IMyUtilities.ReadBinaryFileInGlobalStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream input = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
            if (input == null)
            {
                throw new FileNotFoundException();
            }
            return new BinaryReader(input);
        }

        BinaryReader IMyUtilities.ReadBinaryFileInLocalStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream input = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
            if (input == null)
            {
                throw new FileNotFoundException();
            }
            return new BinaryReader(input);
        }

        BinaryReader IMyUtilities.ReadBinaryFileInWorldStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream input = MyFileSystem.OpenRead(Path.Combine(MySession.Static.CurrentPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
            if (input == null)
            {
                throw new FileNotFoundException();
            }
            return new BinaryReader(input);
        }

        TextReader IMyUtilities.ReadFileInGlobalStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
            if (stream == null)
            {
                throw new FileNotFoundException();
            }
            return new StreamReader(stream);
        }

        TextReader IMyUtilities.ReadFileInLocalStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
            if (stream == null)
            {
                throw new FileNotFoundException();
            }
            return new StreamReader(stream);
        }

        TextReader IMyUtilities.ReadFileInWorldStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream stream = MyFileSystem.OpenRead(Path.Combine(MySession.Static.CurrentPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
            if (stream == null)
            {
                throw new FileNotFoundException();
            }
            return new StreamReader(stream);
        }

        bool IMyUtilities.RemoveVariable(string name) => 
            this.Variables.Remove(name);

        void IMyUtilities.SendMessage(string messageText)
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.SendChatMessage(messageText, ChatChannel.Global, 0L);
            }
        }

        T IMyUtilities.SerializeFromBinary<T>(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        T IMyUtilities.SerializeFromXML<T>(string xml)
        {
            T local;
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                using (XmlReader reader2 = XmlReader.Create(reader))
                {
                    local = (T) serializer.Deserialize(reader2);
                }
            }
            return local;
        }

        byte[] IMyUtilities.SerializeToBinary<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize<T>(stream, obj);
                return stream.ToArray();
            }
        }

        string IMyUtilities.SerializeToXML<T>(T objToSerialize)
        {
            StringWriter writer = new StringWriter();
            new XmlSerializer(objToSerialize.GetType()).Serialize((TextWriter) writer, objToSerialize);
            return writer.ToString();
        }

        void IMyUtilities.SetVariable<T>(string name, T value)
        {
            this.Variables.Remove(name);
            this.Variables.Add(name, value);
        }

        void IMyUtilities.ShowMessage(string sender, string messageText)
        {
            MyHud.Chat.ShowMessage(sender, messageText, "Blue");
        }

        void IMyUtilities.ShowMissionScreen(string screenTitle, string currentObjectivePrefix, string currentObjective, string screenDescription, Action<VRage.Game.ModAPI.ResultEnum> callback = null, string okButtonCaption = null)
        {
            Vector2? windowSize = null;
            windowSize = null;
            MyScreenManager.AddScreen(new MyGuiScreenMission(screenTitle, currentObjectivePrefix, currentObjective, screenDescription, callback, okButtonCaption, windowSize, windowSize, false, true, false, MyMissionScreenStyleEnum.BLUE));
        }

        void IMyUtilities.ShowNotification(string message, int disappearTimeMs, string font)
        {
            MyHudNotification notification = new MyHudNotification(MyCommonTexts.CustomText, disappearTimeMs, font, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] arguments = new object[] { message };
            notification.SetTextFormatArguments(arguments);
            MyHud.Notifications.Add(notification);
        }

        BinaryWriter IMyUtilities.WriteBinaryFileInGlobalStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream output = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", file), FileMode.Create);
            if (output == null)
            {
                throw new FileNotFoundException();
            }
            return new BinaryWriter(output);
        }

        BinaryWriter IMyUtilities.WriteBinaryFileInLocalStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream output = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file), FileMode.Create);
            if (output == null)
            {
                throw new FileNotFoundException();
            }
            return new BinaryWriter(output);
        }

        BinaryWriter IMyUtilities.WriteBinaryFileInWorldStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream output = MyFileSystem.OpenWrite(Path.Combine(MySession.Static.CurrentPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file), FileMode.Create);
            if (output == null)
            {
                throw new FileNotFoundException();
            }
            return new BinaryWriter(output);
        }

        TextWriter IMyUtilities.WriteFileInGlobalStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", file), FileMode.Create);
            if (stream == null)
            {
                throw new FileNotFoundException();
            }
            return new StreamWriter(stream);
        }

        TextWriter IMyUtilities.WriteFileInLocalStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file), FileMode.Create);
            if (stream == null)
            {
                throw new FileNotFoundException();
            }
            return new StreamWriter(stream);
        }

        TextWriter IMyUtilities.WriteFileInWorldStorage(string file, System.Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            Stream stream = MyFileSystem.OpenWrite(Path.Combine(MySession.Static.CurrentPath, "Storage", this.StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file), FileMode.Create);
            if (stream == null)
            {
                throw new FileNotFoundException();
            }
            return new StreamWriter(stream);
        }

        IMyConfigDedicated IMyUtilities.ConfigDedicated =>
            MySandboxGame.ConfigDedicated;

        string IMyGamePaths.ContentPath =>
            MyFileSystem.ContentPath;

        string IMyGamePaths.ModsPath =>
            MyFileSystem.ModsPath;

        string IMyGamePaths.UserDataPath =>
            MyFileSystem.UserDataPath;

        string IMyGamePaths.SavesPath =>
            MyFileSystem.SavesPath;

        string IMyGamePaths.ModScopeName =>
            this.StripDllExtIfNecessary(Assembly.GetCallingAssembly().ManifestModule.ScopeName);

        IMyGamePaths IMyUtilities.GamePaths =>
            this;

        bool IMyUtilities.IsDedicated =>
            Game.IsDedicated;
    }
}

