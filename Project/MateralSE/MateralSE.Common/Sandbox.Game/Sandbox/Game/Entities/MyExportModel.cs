namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Models;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender;
    using VRageRender.Import;
    using VRageRender.Models;

    public class MyExportModel
    {
        private readonly MyModel m_model;
        private List<Material> m_materials;

        public MyExportModel(MyModel model)
        {
            this.m_model = model;
            this.m_model.LoadData();
            this.ExtractMaterialsFromModel();
        }

        private void ExtractMaterialsFromModel()
        {
            this.m_materials = new List<Material>();
            List<VRageRender.Models.MyMesh> meshList = this.m_model.GetMeshList();
            if (meshList != null)
            {
                foreach (VRageRender.Models.MyMesh mesh in meshList)
                {
                    if (mesh.Material == null)
                    {
                        continue;
                    }
                    Dictionary<string, string> textures = mesh.Material.Textures;
                    if (textures != null)
                    {
                        Material item = new Material {
                            AddMapsTexture = textures.Get<string, string>("AddMapsTexture", null),
                            AlphamaskTexture = textures.Get<string, string>("AlphamaskTexture", null),
                            ColorMetalTexture = textures.Get<string, string>("ColorMetalTexture", null),
                            NormalGlossTexture = textures.Get<string, string>("NormalGlossTexture", null),
                            FirstTri = mesh.IndexStart / 3,
                            LastTri = ((mesh.IndexStart / 3) + mesh.TriCount) - 1,
                            IsGlass = mesh.Material.DrawTechnique == MyMeshDrawTechnique.GLASS
                        };
                        this.m_materials.Add(item);
                    }
                }
            }
        }

        public List<Material> GetMaterials() => 
            this.m_materials;

        public HalfVector2[] GetTexCoords() => 
            this.m_model.TexCoords;

        public MyTriangleVertexIndices GetTriangle(int index) => 
            this.m_model.GetTriangle(index);

        public int GetTrianglesCount() => 
            this.m_model.GetTrianglesCount();

        public int GetVerticesCount() => 
            this.m_model.GetVerticesCount();

        [StructLayout(LayoutKind.Sequential)]
        public struct Material
        {
            public int LastTri;
            public int FirstTri;
            public bool IsGlass;
            public Vector3 ColorMaskHSV;
            public string ExportedMaterialName;
            public string AddMapsTexture;
            public string AlphamaskTexture;
            public string ColorMetalTexture;
            public string NormalGlossTexture;
            public bool EqualsMaterialWise(MyExportModel.Material x) => 
                (string.Equals(this.AddMapsTexture, x.AddMapsTexture, StringComparison.OrdinalIgnoreCase) && (string.Equals(this.AlphamaskTexture, x.AlphamaskTexture, StringComparison.OrdinalIgnoreCase) && (string.Equals(this.ColorMetalTexture, x.ColorMetalTexture, StringComparison.OrdinalIgnoreCase) && string.Equals(this.NormalGlossTexture, x.NormalGlossTexture, StringComparison.OrdinalIgnoreCase))));
        }
    }
}

