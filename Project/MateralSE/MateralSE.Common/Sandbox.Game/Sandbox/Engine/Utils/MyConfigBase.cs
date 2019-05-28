namespace Sandbox.Engine.Utils
{
    using Sandbox;
    using Sandbox.Engine.Platform;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using VRage.FileSystem;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public class MyConfigBase
    {
        protected readonly SerializableDictionary<string, object> m_values = new SerializableDictionary<string, object>();
        private string m_path;

        public MyConfigBase(string fileName)
        {
            this.m_path = Path.Combine(MyFileSystem.UserDataPath, fileName);
        }

        protected unsafe T? GetOptionalEnum<T>(string name) where T: struct, IComparable, IFormattable, IConvertible
        {
            int? intFromString = MyUtils.GetIntFromString(this.GetParameterValue(name));
            if ((intFromString == null) || !Enum.IsDefined(typeof(T), intFromString.Value))
            {
                return null;
            }
            T data = default(T);
            Utilities.Read<T>(new IntPtr((void*) &intFromString.Value), ref data);
            return new T?(data);
        }

        protected string GetParameterValue(string parameterName)
        {
            object obj2;
            return (this.m_values.Dictionary.TryGetValue(parameterName, out obj2) ? ((string) obj2) : "");
        }

        protected SerializableDictionary<string, object> GetParameterValueDictionary(string parameterName)
        {
            object obj2;
            return (this.m_values.Dictionary.TryGetValue(parameterName, out obj2) ? ((SerializableDictionary<string, object>) obj2) : null);
        }

        protected T GetParameterValueT<T>(string parameterName)
        {
            object obj2;
            return (this.m_values.Dictionary.TryGetValue(parameterName, out obj2) ? ((T) obj2) : default(T));
        }

        protected Vector3I GetParameterValueVector3I(string parameterName)
        {
            int num;
            int num2;
            int num3;
            char[] separator = new char[] { ',', ' ' };
            string[] strArray = this.GetParameterValue(parameterName).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (((strArray.Length != 3) || (!int.TryParse(strArray[0], out num) || !int.TryParse(strArray[1], out num2))) || !int.TryParse(strArray[2], out num3))
            {
                return new Vector3I(0, 0, 0);
            }
            return new Vector3I(num, num2, num3);
        }

        public void Load()
        {
            if (!Game.IsDedicated)
            {
                MySandboxGame.Log.WriteLine("MyConfig.Load() - START");
                using (MySandboxGame.Log.IndentUsing(LoggingOptions.CONFIG_ACCESS))
                {
                    MySandboxGame.Log.WriteLine("Path: " + this.m_path, LoggingOptions.CONFIG_ACCESS);
                    string msg = "";
                    try
                    {
                        if (!File.Exists(this.m_path))
                        {
                            MySandboxGame.Log.WriteLine("Config file not found! " + this.m_path);
                        }
                        else
                        {
                            using (Stream stream = MyFileSystem.OpenRead(this.m_path))
                            {
                                using (XmlReader reader = XmlReader.Create(stream))
                                {
                                    Type[] extraTypes = new Type[] { typeof(SerializableDictionary<string, string>), typeof(List<string>), typeof(SerializableDictionary<string, MyConfig.MyDebugInputData>), typeof(MyConfig.MyDebugInputData), typeof(MyObjectBuilder_ServerFilterOptions) };
                                    SerializableDictionary<string, object> dictionary = (SerializableDictionary<string, object>) new XmlSerializer(this.m_values.GetType(), extraTypes).Deserialize(reader);
                                    this.m_values.Dictionary = dictionary.Dictionary;
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        MySandboxGame.Log.WriteLine("Exception occured, but application is continuing. Exception: " + exception);
                        MySandboxGame.Log.WriteLine("Config:");
                        MySandboxGame.Log.WriteLine(msg);
                    }
                    foreach (KeyValuePair<string, object> pair in this.m_values.Dictionary)
                    {
                        if (pair.Value == null)
                        {
                            MySandboxGame.Log.WriteLine("ERROR: " + pair.Key + " is null!", LoggingOptions.CONFIG_ACCESS);
                            continue;
                        }
                        MySandboxGame.Log.WriteLine(pair.Key + ": " + pair.Value.ToString(), LoggingOptions.CONFIG_ACCESS);
                    }
                }
                MySandboxGame.Log.WriteLine("MyConfig.Load() - END");
            }
        }

        protected void RemoveParameterValue(string parameterName)
        {
            this.m_values.Dictionary.Remove(parameterName);
        }

        public void Save()
        {
            if (!Game.IsDedicated)
            {
                MySandboxGame.Log.WriteLine("MyConfig.Save() - START");
                MySandboxGame.Log.IncreaseIndent();
                try
                {
                    MySandboxGame.Log.WriteLine("Path: " + this.m_path, LoggingOptions.CONFIG_ACCESS);
                    try
                    {
                        using (Stream stream = MyFileSystem.OpenWrite(this.m_path, FileMode.Create))
                        {
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.Indent = true;
                            settings.NewLineHandling = NewLineHandling.None;
                            using (XmlWriter writer = XmlWriter.Create(stream, settings))
                            {
                                Type[] extraTypes = new Type[] { typeof(SerializableDictionary<string, string>), typeof(List<string>), typeof(SerializableDictionary<string, MyConfig.MyDebugInputData>), typeof(MyConfig.MyDebugInputData), typeof(MyObjectBuilder_ServerFilterOptions) };
                                new XmlSerializer(this.m_values.GetType(), extraTypes).Serialize(writer, this.m_values);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        MySandboxGame.Log.WriteLine("Exception occured, but application is continuing. Exception: " + exception);
                    }
                }
                finally
                {
                    MySandboxGame.Log.DecreaseIndent();
                    MySandboxGame.Log.WriteLine("MyConfig.Save() - END");
                }
            }
        }

        protected unsafe void SetOptionalEnum<T>(string name, T? value) where T: struct, IComparable, IFormattable, IConvertible
        {
            if (value == null)
            {
                this.RemoveParameterValue(name);
            }
            else
            {
                T data = value.Value;
                int num = 0;
                Utilities.Write<T>(new IntPtr((void*) &num), ref data);
                this.SetParameterValue(name, num);
            }
        }

        protected void SetParameterValue(string parameterName, int value)
        {
            this.m_values.Dictionary[parameterName] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        protected void SetParameterValue(string parameterName, bool? value)
        {
            string text1;
            if (value == null)
            {
                text1 = "";
            }
            else
            {
                text1 = value.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }
            this.m_values.Dictionary[parameterName] = text1;
        }

        protected void SetParameterValue(string parameterName, int? value)
        {
            string text1;
            if (value != null)
            {
                text1 = value.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }
            else
            {
                text1 = "";
            }
            this.m_values.Dictionary[parameterName] = text1;
        }

        protected void SetParameterValue(string parameterName, float value)
        {
            this.m_values.Dictionary[parameterName] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        protected void SetParameterValue(string parameterName, string value)
        {
            this.m_values.Dictionary[parameterName] = value;
        }

        protected void SetParameterValue(string parameterName, Vector3I value)
        {
            this.SetParameterValue(parameterName, $"{value.X}, {value.Y}, {value.Z}");
        }
    }
}

