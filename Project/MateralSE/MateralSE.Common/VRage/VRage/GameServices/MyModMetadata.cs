namespace VRage.GameServices
{
    using System;
    using System.Linq;

    public class MyModMetadata
    {
        public Version ModVersion;
        public Version MinGameVersion;
        public Version MaxGameVersion;

        public static implicit operator MyModMetadata(ModMetadataFile file)
        {
            if (file == null)
            {
                return null;
            }
            MyModMetadata metadata = new MyModMetadata();
            Version.TryParse(file.ModVersion, out metadata.ModVersion);
            if (file.MinGameVersion == null)
            {
                metadata.MinGameVersion = null;
            }
            else
            {
                char[] separator = new char[] { '.' };
                string[] source = file.MinGameVersion.Split(separator);
                file.MinGameVersion = string.Join(".", source.Take<string>(3));
                Version.TryParse(file.MinGameVersion, out metadata.MinGameVersion);
            }
            if (file.MaxGameVersion == null)
            {
                metadata.MaxGameVersion = null;
            }
            else
            {
                char[] separator = new char[] { '.' };
                string[] source = file.MaxGameVersion.Split(separator);
                file.MaxGameVersion = string.Join(".", source.Take<string>(3));
                Version.TryParse(file.MaxGameVersion, out metadata.MaxGameVersion);
            }
            return metadata;
        }

        public static implicit operator ModMetadataFile(MyModMetadata metadata)
        {
            if (metadata == null)
            {
                return null;
            }
            ModMetadataFile file = new ModMetadataFile();
            if (metadata.ModVersion != null)
            {
                file.ModVersion = metadata.ModVersion.ToString();
            }
            if (metadata.MinGameVersion != null)
            {
                file.MinGameVersion = metadata.MinGameVersion.ToString();
            }
            if (metadata.MaxGameVersion != null)
            {
                file.MaxGameVersion = metadata.MaxGameVersion.ToString();
            }
            return file;
        }

        public override string ToString() => 
            $"ModVersion: {((this.ModVersion != null) ? this.ModVersion.ToString() : "N/A")}, MinGameVersion: {((this.MinGameVersion != null) ? this.MinGameVersion.ToString() : "N/A")}, MaxGameVersion: {((this.MaxGameVersion != null) ? this.MaxGameVersion.ToString() : "N/A")}";
    }
}

