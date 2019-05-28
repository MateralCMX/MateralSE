namespace VRageRender.Models
{
    using System;
    using VRageRender.Import;

    public class MyMesh
    {
        public readonly string AssetName;
        public readonly MyMeshMaterial Material;
        public int IndexStart;
        public int TriStart;
        public int TriCount;

        public MyMesh(MyMeshPartInfo meshInfo, string assetName)
        {
            MyMaterialDescriptor materialDesc = meshInfo.m_MaterialDesc;
            if (materialDesc == null)
            {
                this.Material = new MyMeshMaterial();
            }
            else
            {
                string str;
                materialDesc.Textures.TryGetValue("DiffuseTexture", out str);
                MyMeshMaterial material = new MyMeshMaterial {
                    Name = meshInfo.m_MaterialDesc.MaterialName,
                    Textures = materialDesc.Textures,
                    DrawTechnique = meshInfo.Technique,
                    GlassCW = meshInfo.m_MaterialDesc.GlassCW,
                    GlassCCW = meshInfo.m_MaterialDesc.GlassCCW,
                    GlassSmooth = meshInfo.m_MaterialDesc.GlassSmoothNormals
                };
                this.Material = material;
            }
            this.AssetName = assetName;
        }

        public MyMesh(MyMeshMaterial material, string assetName)
        {
            this.Material = material;
            this.AssetName = assetName;
        }
    }
}

