namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class MyMeshPartInfo
    {
        public int m_MaterialHash;
        public MyMaterialDescriptor m_MaterialDesc;
        public List<int> m_indices = new List<int>();
        public MyMeshDrawTechnique Technique;

        public bool Export(BinaryWriter writer)
        {
            writer.Write(this.m_MaterialHash);
            writer.Write(this.m_indices.Count);
            foreach (int num in this.m_indices)
            {
                writer.Write(num);
            }
            bool flag = true;
            if (this.m_MaterialDesc == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                flag = this.m_MaterialDesc.Write(writer);
            }
            return flag;
        }

        public string GetMaterialName()
        {
            string materialName = "";
            if (this.m_MaterialDesc != null)
            {
                materialName = this.m_MaterialDesc.MaterialName;
            }
            return materialName;
        }

        public bool Import(BinaryReader reader, int version)
        {
            this.m_MaterialHash = reader.ReadInt32();
            if (version < 0x100d61)
            {
                reader.ReadInt32();
            }
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                this.m_indices.Add(reader.ReadInt32());
            }
            bool flag = true;
            if (!reader.ReadBoolean())
            {
                this.m_MaterialDesc = null;
            }
            else
            {
                this.m_MaterialDesc = new MyMaterialDescriptor();
                flag = this.m_MaterialDesc.Read(reader, version) & Enum.TryParse<MyMeshDrawTechnique>(this.m_MaterialDesc.Technique, out this.Technique);
            }
            return flag;
        }
    }
}

