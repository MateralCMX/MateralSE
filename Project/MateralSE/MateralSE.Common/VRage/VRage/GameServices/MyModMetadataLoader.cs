namespace VRage.GameServices
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using VRage.FileSystem;
    using VRage.Utils;

    public class MyModMetadataLoader
    {
        public static ModMetadataFile Load(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }
            ModMetadataFile file = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ModMetadataFile));
                Stream stream = MyFileSystem.OpenRead(filename);
                if (stream != null)
                {
                    file = (ModMetadataFile) serializer.Deserialize(stream);
                    stream.Close();
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { filename, exception.Message };
                MyLog.Default.Warning("Failed loading mod metadata file: {0} with exception: {1}", args);
            }
            return file;
        }

        public static ModMetadataFile Parse(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            ModMetadataFile file = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ModMetadataFile));
                using (TextReader reader = new StringReader(xml))
                {
                    file = (ModMetadataFile) serializer.Deserialize(reader);
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.Message };
                MyLog.Default.Warning("Failed parsing mod metadata: {0}", args);
            }
            return file;
        }

        public static bool Save(string filename, ModMetadataFile file)
        {
            if (string.IsNullOrEmpty(filename) || (file == null))
            {
                return false;
            }
            try
            {
                TextWriter textWriter = new StreamWriter(filename);
                new XmlSerializer(typeof(ModMetadataFile)).Serialize(textWriter, file);
                textWriter.Close();
            }
            catch (Exception exception)
            {
                object[] args = new object[] { filename, exception.Message };
                MyLog.Default.Warning("Failed saving mod metadata file: {0} with exception: {1}", args);
                return false;
            }
            return true;
        }

        public static string Serialize(ModMetadataFile data)
        {
            string str;
            if (data == null)
            {
                return null;
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ModMetadataFile));
                using (TextWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, data);
                    str = writer.ToString();
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.Message };
                MyLog.Default.Warning("Failed serializing mod metadata: {0}", args);
                str = null;
            }
            return str;
        }
    }
}

