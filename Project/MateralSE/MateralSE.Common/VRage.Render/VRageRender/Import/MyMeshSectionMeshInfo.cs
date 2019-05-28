namespace VRageRender.Import
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class MyMeshSectionMeshInfo
    {
        public MyMeshSectionMeshInfo()
        {
            this.StartIndex = -1;
        }

        public bool Export(BinaryWriter writer)
        {
            writer.Write(this.MaterialName);
            writer.Write(this.StartIndex);
            writer.Write(this.IndexCount);
            return true;
        }

        public bool Import(BinaryReader reader, int version)
        {
            this.MaterialName = reader.ReadString();
            this.StartIndex = reader.ReadInt32();
            this.IndexCount = reader.ReadInt32();
            return true;
        }

        public string MaterialName { get; set; }

        public int StartIndex { get; set; }

        public int IndexCount { get; set; }
    }
}

