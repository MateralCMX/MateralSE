namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyMaterialDescriptor
    {
        public Dictionary<string, string> Textures;
        public Dictionary<string, string> UserData;

        public MyMaterialDescriptor()
        {
            this.Textures = new Dictionary<string, string>();
            this.UserData = new Dictionary<string, string>();
        }

        public MyMaterialDescriptor(string materialName)
        {
            this.Textures = new Dictionary<string, string>();
            this.UserData = new Dictionary<string, string>();
            this.MaterialName = materialName;
            this.Technique = "MESH";
            this.GlassCCW = string.Empty;
            this.GlassCW = string.Empty;
            this.GlassSmoothNormals = true;
        }

        public bool Read(BinaryReader reader, int version)
        {
            this.Textures.Clear();
            this.UserData.Clear();
            this.MaterialName = reader.ReadString();
            if (string.IsNullOrEmpty(this.MaterialName))
            {
                this.MaterialName = null;
            }
            if (version >= 0x100d62)
            {
                int num = reader.ReadInt32();
                for (int i = 0; i < num; i++)
                {
                    string key = reader.ReadString();
                    string str4 = reader.ReadString();
                    this.Textures.Add(key, str4);
                }
            }
            else
            {
                string str = reader.ReadString();
                if (!string.IsNullOrEmpty(str))
                {
                    this.Textures.Add("DiffuseTexture", str);
                }
                string str2 = reader.ReadString();
                if (!string.IsNullOrEmpty(str2))
                {
                    this.Textures.Add("NormalTexture", str2);
                }
            }
            if (version >= 0x104be1)
            {
                int num3 = reader.ReadInt32();
                for (int i = 0; i < num3; i++)
                {
                    string key = reader.ReadString();
                    string str6 = reader.ReadString();
                    this.UserData.Add(key, str6);
                }
            }
            if (version < 0x11a789)
            {
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
            }
            if (version >= 0x100d61)
            {
                this.Technique = reader.ReadString();
            }
            else
            {
                string str7 = ((byte) reader.ReadInt32()).ToString();
                this.Technique = str7;
            }
            if (this.Technique == "GLASS")
            {
                if (version >= 0xfea39)
                {
                    this.GlassCW = reader.ReadString();
                    this.GlassCCW = reader.ReadString();
                    this.GlassSmoothNormals = reader.ReadBoolean();
                    if ((!string.IsNullOrEmpty(this.GlassCCW) && !MyTransparentMaterials.ContainsMaterial(MyStringId.GetOrCompute(this.MaterialName))) && MyTransparentMaterials.ContainsMaterial(MyStringId.GetOrCompute(this.GlassCCW)))
                    {
                        this.MaterialName = this.GlassCCW;
                    }
                }
                else
                {
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    this.GlassCW = "GlassCW";
                    this.GlassCCW = "GlassCCW";
                    this.GlassSmoothNormals = false;
                }
            }
            return true;
        }

        public bool Write(BinaryWriter writer)
        {
            writer.Write((this.MaterialName != null) ? this.MaterialName : "");
            writer.Write(this.Textures.Count);
            foreach (KeyValuePair<string, string> pair in this.Textures)
            {
                writer.Write(pair.Key);
                writer.Write((pair.Value == null) ? "" : pair.Value);
            }
            writer.Write(this.UserData.Count);
            foreach (KeyValuePair<string, string> pair2 in this.UserData)
            {
                writer.Write(pair2.Key);
                writer.Write((pair2.Value == null) ? "" : pair2.Value);
            }
            writer.Write(this.Technique);
            if (this.Technique == "GLASS")
            {
                writer.Write(this.GlassCW);
                writer.Write(this.GlassCCW);
                writer.Write(this.GlassSmoothNormals);
            }
            return true;
        }

        public string MaterialName { get; private set; }

        public string Technique { get; set; }

        public MyMeshDrawTechnique TechniqueEnum
        {
            get
            {
                MyMeshDrawTechnique technique;
                Enum.TryParse<MyMeshDrawTechnique>(this.Technique, out technique);
                return technique;
            }
            set => 
                (this.Technique = value.ToString());
        }

        public string GlassCW { get; set; }

        public string GlassCCW { get; set; }

        public bool GlassSmoothNormals { get; set; }

        public MyFacingEnum Facing
        {
            get
            {
                string str;
                MyFacingEnum enum2;
                return (!this.UserData.TryGetValue("Facing", out str) ? MyFacingEnum.None : (Enum.TryParse<MyFacingEnum>(str, out enum2) ? enum2 : MyFacingEnum.None));
            }
        }

        public Vector2 WindScaleAndFreq
        {
            get
            {
                string str;
                Vector2 zero = Vector2.Zero;
                if (this.UserData.TryGetValue("WindScale", out str))
                {
                    float num;
                    if (!float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
                    {
                        return zero;
                    }
                    zero.X = num;
                    if (this.UserData.TryGetValue("WindFrequency", out str) && !float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
                    {
                        return zero;
                    }
                    zero.Y = num;
                }
                return zero;
            }
        }
    }
}

