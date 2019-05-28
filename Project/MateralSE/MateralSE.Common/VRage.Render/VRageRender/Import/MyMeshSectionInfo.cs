namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class MyMeshSectionInfo
    {
        public MyMeshSectionInfo()
        {
            this.Meshes = new List<MyMeshSectionMeshInfo>();
        }

        public bool Export(BinaryWriter writer)
        {
            writer.Write(this.Name);
            writer.Write(this.Meshes.Count);
            bool flag = true;
            foreach (MyMeshSectionMeshInfo info in this.Meshes)
            {
                flag &= info.Export(writer);
            }
            return flag;
        }

        public bool Import(BinaryReader reader, int version)
        {
            this.Name = reader.ReadString();
            int num = reader.ReadInt32();
            bool flag = true;
            for (int i = 0; i < num; i++)
            {
                MyMeshSectionMeshInfo item = new MyMeshSectionMeshInfo();
                flag &= item.Import(reader, version);
                this.Meshes.Add(item);
            }
            return flag;
        }

        public string Name { get; set; }

        public List<MyMeshSectionMeshInfo> Meshes { get; private set; }
    }
}

