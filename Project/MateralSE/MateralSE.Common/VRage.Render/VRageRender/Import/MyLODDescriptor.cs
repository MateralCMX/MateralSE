namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.FileSystem;

    public class MyLODDescriptor
    {
        public float Distance;
        public string Model;
        public string RenderQuality;
        public List<int> RenderQualityList;

        public string GetModelAbsoluteFilePath(string parentAssetFilePath)
        {
            if (this.Model == null)
            {
                return null;
            }
            string str = parentAssetFilePath.ToLower();
            string model = this.Model;
            if (!model.Contains(".mwm"))
            {
                model = model + ".mwm";
            }
            if (!Path.IsPathRooted(parentAssetFilePath) || !str.Contains("models"))
            {
                return Path.Combine(MyFileSystem.ContentPath, model);
            }
            string path = Path.Combine(parentAssetFilePath.Substring(0, str.IndexOf("models")), model);
            if (MyFileSystem.FileExists(path))
            {
                return path;
            }
            path = Path.Combine(MyFileSystem.ContentPath, model);
            return (MyFileSystem.FileExists(path) ? path : null);
        }

        public bool Read(BinaryReader reader)
        {
            this.Distance = reader.ReadSingle();
            this.Model = reader.ReadString();
            if (string.IsNullOrEmpty(this.Model))
            {
                this.Model = null;
            }
            this.RenderQuality = reader.ReadString();
            if (string.IsNullOrEmpty(this.RenderQuality))
            {
                this.RenderQuality = null;
            }
            return true;
        }

        public bool Write(BinaryWriter writer)
        {
            writer.Write(this.Distance);
            writer.Write((this.Model != null) ? this.Model : "");
            writer.Write((this.RenderQuality != null) ? this.RenderQuality : "");
            return true;
        }
    }
}

