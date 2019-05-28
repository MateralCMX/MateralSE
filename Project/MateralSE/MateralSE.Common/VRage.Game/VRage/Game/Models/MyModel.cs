namespace VRage.Game.Models
{
    using BulletXNA.BulletCollision;
    using Havok;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Import;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender.Animations;
    using VRageRender.Fractures;
    using VRageRender.Import;
    using VRageRender.Models;
    using VRageRender.Utils;

    public class MyModel : IDisposable, IPrimitiveManagerBase, IMyModel
    {
        private static int m_nextUniqueId = 0;
        private static Dictionary<int, string> m_uniqueModelNames = new Dictionary<int, string>();
        private static Dictionary<string, int> m_uniqueModelIds = new Dictionary<string, int>();
        [ThreadStatic]
        private static MyModelImporter m_perThreadImporter;
        public readonly int UniqueId;
        private int m_verticesCount;
        private int m_trianglesCount;
        private MyCompressedVertexNormal[] m_vertices;
        private MyCompressedBoneIndicesWeights[] m_bonesIndicesWeights;
        private int[] m_Indices;
        private ushort[] m_Indices_16bit;
        public MyTriangleVertexIndices[] Triangles;
        public Dictionary<string, MyModelDummy> Dummies;
        public MyModelInfo ModelInfo;
        public ModelAnimations Animations;
        public MyModelBone[] Bones;
        public byte[] HavokData;
        public MyModelFractures ModelFractures;
        private bool m_hasUV;
        public bool m_loadUV;
        public bool ExportedWrong;
        public HkShape[] HavokCollisionShapes;
        public HkdBreakableShape[] HavokBreakableShapes;
        public byte[] HavokDestructionData;
        private HalfVector2[] m_texCoords;
        private Byte4[] m_tangents;
        private VRageMath.BoundingSphere m_boundingSphere;
        private VRageMath.BoundingBox m_boundingBox;
        private Vector3 m_boundingBoxSize;
        private Vector3 m_boundingBoxSizeHalf;
        public Vector3I[] BoneMapping;
        public float PatternScale;
        private MyLODDescriptor[] m_lods;
        private IMyTriangePruningStructure m_bvh;
        private readonly string m_assetName;
        private bool m_loadedData;
        private List<MyMesh> m_meshContainer;
        private Dictionary<string, MyMeshSection> m_meshSections;
        private bool m_loadingErrorProcessed;
        private float m_scaleFactor;

        public MyModel(string assetName) : this(assetName, false)
        {
            this.UniqueId = GetId(assetName);
        }

        public MyModel(string assetName, bool keepInMemory)
        {
            this.PatternScale = 1f;
            this.m_meshContainer = new List<MyMesh>();
            this.m_meshSections = new Dictionary<string, MyMeshSection>();
            this.m_scaleFactor = 1f;
            this.m_assetName = assetName;
            this.m_loadedData = false;
            this.KeepInMemory = keepInMemory;
            if (!Path.IsPathRooted(this.AssetName))
            {
                Path.Combine(MyFileSystem.ContentPath, this.AssetName);
            }
            else
            {
                string text1 = this.AssetName;
            }
        }

        void IPrimitiveManagerBase.Cleanup()
        {
        }

        void IPrimitiveManagerBase.GetPrimitiveBox(int prim_index, out AABB primbox)
        {
            VRageMath.BoundingBox box = VRageMath.BoundingBox.CreateInvalid();
            Vector3 vertex = this.GetVertex(this.Triangles[prim_index].I0);
            Vector3 vector2 = this.GetVertex(this.Triangles[prim_index].I1);
            Vector3 vector3 = this.GetVertex(this.Triangles[prim_index].I2);
            box.Include(ref vertex, ref vector2, ref vector3);
            AABB aabb = new AABB {
                m_min = box.Min.ToBullet(),
                m_max = box.Max.ToBullet()
            };
            primbox = aabb;
        }

        int IPrimitiveManagerBase.GetPrimitiveCount() => 
            this.m_trianglesCount;

        void IPrimitiveManagerBase.GetPrimitiveTriangle(int prim_index, PrimitiveTriangle triangle)
        {
            triangle.m_vertices[0] = this.GetVertex(this.Triangles[prim_index].I0).ToBullet();
            triangle.m_vertices[1] = this.GetVertex(this.Triangles[prim_index].I1).ToBullet();
            triangle.m_vertices[2] = this.GetVertex(this.Triangles[prim_index].I2).ToBullet();
        }

        bool IPrimitiveManagerBase.IsTrimesh() => 
            true;

        public void CheckLoadingErrors(MyModContext context, out bool errorFound)
        {
            if (!this.ExportedWrong || this.m_loadingErrorProcessed)
            {
                errorFound = false;
            }
            else
            {
                errorFound = true;
                this.m_loadingErrorProcessed = true;
            }
        }

        [Conditional("DEBUG")]
        private void CheckTriangles(int triangleCount)
        {
            bool flag = true;
            foreach (MyTriangleVertexIndices indices in this.Triangles)
            {
                flag = (((((flag & (indices.I0 != indices.I1)) & (indices.I1 != indices.I2)) & (indices.I2 != indices.I0)) & ((indices.I0 >= 0) & (indices.I0 < this.m_verticesCount))) & ((indices.I1 >= 0) & (indices.I1 < this.m_verticesCount))) & ((indices.I2 >= 0) & (indices.I2 < this.m_verticesCount));
            }
        }

        private void CopyTriangleIndices()
        {
            this.Triangles = new MyTriangleVertexIndices[this.GetNumberOfTrianglesForColDet()];
            int index = 0;
            foreach (MyMesh mesh in this.m_meshContainer)
            {
                mesh.TriStart = index;
                if (this.m_Indices != null)
                {
                    for (int j = 0; j < mesh.TriCount; j++)
                    {
                        this.Triangles[index] = new MyTriangleVertexIndices(this.m_Indices[mesh.IndexStart + (j * 3)], this.m_Indices[(mesh.IndexStart + (j * 3)) + 2], this.m_Indices[(mesh.IndexStart + (j * 3)) + 1]);
                        index++;
                    }
                    continue;
                }
                if (this.m_Indices_16bit == null)
                {
                    throw new InvalidBranchException();
                }
                for (int i = 0; i < mesh.TriCount; i++)
                {
                    this.Triangles[index] = new MyTriangleVertexIndices(this.m_Indices_16bit[mesh.IndexStart + (i * 3)], this.m_Indices_16bit[(mesh.IndexStart + (i * 3)) + 2], this.m_Indices_16bit[(mesh.IndexStart + (i * 3)) + 1]);
                    index++;
                }
            }
        }

        public void Dispose()
        {
            this.m_meshContainer.Clear();
        }

        public MyTriangle_BoneIndicesWeigths? GetBoneIndicesWeights(int triangleIndex)
        {
            if (this.m_bonesIndicesWeights == null)
            {
                return null;
            }
            MyTriangleVertexIndices indices = this.Triangles[triangleIndex];
            MyCompressedBoneIndicesWeights weights = this.m_bonesIndicesWeights[indices.I0];
            MyCompressedBoneIndicesWeights weights2 = this.m_bonesIndicesWeights[indices.I1];
            MyCompressedBoneIndicesWeights weights3 = this.m_bonesIndicesWeights[indices.I2];
            MyTriangle_BoneIndicesWeigths weigths = new MyTriangle_BoneIndicesWeigths();
            MyVertex_BoneIndicesWeights weights4 = new MyVertex_BoneIndicesWeights {
                Indices = weights.Indices.ToVector4UByte(),
                Weights = weights.Weights.ToVector4()
            };
            weigths.Vertex0 = weights4;
            weights4 = new MyVertex_BoneIndicesWeights {
                Indices = weights2.Indices.ToVector4UByte(),
                Weights = weights2.Weights.ToVector4()
            };
            weigths.Vertex1 = weights4;
            weights4 = new MyVertex_BoneIndicesWeights {
                Indices = weights3.Indices.ToVector4UByte(),
                Weights = weights3.Weights.ToVector4()
            };
            weigths.Vertex2 = weights4;
            return new MyTriangle_BoneIndicesWeigths?(weigths);
        }

        public int GetBVHSize() => 
            ((this.m_bvh != null) ? this.m_bvh.Size : 0);

        public static string GetById(int id) => 
            m_uniqueModelNames[id];

        public MyMeshDrawTechnique GetDrawTechnique(int triangleIndex)
        {
            MyMeshDrawTechnique mESH = MyMeshDrawTechnique.MESH;
            for (int i = 0; i < this.m_meshContainer.Count; i++)
            {
                if ((triangleIndex >= this.m_meshContainer[i].TriStart) && (triangleIndex < (this.m_meshContainer[i].TriStart + this.m_meshContainer[i].TriCount)))
                {
                    mESH = this.m_meshContainer[i].Material.DrawTechnique;
                }
            }
            return mESH;
        }

        public static int GetId(string assetName)
        {
            int nextUniqueId;
            Dictionary<string, int> uniqueModelIds = m_uniqueModelIds;
            lock (uniqueModelIds)
            {
                if (!m_uniqueModelIds.TryGetValue(assetName, out nextUniqueId))
                {
                    m_nextUniqueId++;
                    nextUniqueId = m_nextUniqueId;
                    m_uniqueModelIds.Add(assetName, nextUniqueId);
                    m_uniqueModelNames.Add(nextUniqueId, assetName);
                }
            }
            return nextUniqueId;
        }

        public List<MyMesh> GetMeshList() => 
            this.m_meshContainer;

        public MyMeshSection GetMeshSection(string name) => 
            this.m_meshSections[name];

        private int GetNumberOfTrianglesForColDet()
        {
            int num = 0;
            foreach (MyMesh mesh in this.m_meshContainer)
            {
                num += mesh.TriCount;
            }
            return num;
        }

        public MyTriangleVertexIndices GetTriangle(int triangleIndex) => 
            this.Triangles[triangleIndex];

        public void GetTriangleBoundingBox(int triangleIndex, ref VRageMath.BoundingBox boundingBox)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            boundingBox = VRageMath.BoundingBox.CreateInvalid();
            this.GetVertex(this.Triangles[triangleIndex].I0, this.Triangles[triangleIndex].I1, this.Triangles[triangleIndex].I2, out vector, out vector2, out vector3);
            boundingBox.Include(vector, vector2, vector3);
        }

        public IMyTriangePruningStructure GetTrianglePruningStructure() => 
            this.m_bvh;

        public int GetTrianglesCount() => 
            this.m_trianglesCount;

        public Vector3 GetVertex(int vertexIndex) => 
            this.GetVertexInt(vertexIndex);

        public void GetVertex(int vertexIndex1, int vertexIndex2, int vertexIndex3, out Vector3 v1, out Vector3 v2, out Vector3 v3)
        {
            v1 = this.GetVertex(vertexIndex1);
            v2 = this.GetVertex(vertexIndex2);
            v3 = this.GetVertex(vertexIndex3);
        }

        public Vector3 GetVertexInt(int vertexIndex) => 
            VF_Packer.UnpackPosition(ref this.m_vertices[vertexIndex].Position);

        public Vector3 GetVertexNormal(int vertexIndex) => 
            VF_Packer.UnpackNormal(ref this.m_vertices[vertexIndex].Normal);

        public Vector3 GetVertexTangent(int vertexIndex)
        {
            if (this.m_tangents == null)
            {
                string[] tags = new string[] { "Tangents" };
                m_importer.ImportData(this.AssetName, tags);
                Dictionary<string, object> tagData = m_importer.GetTagData();
                if (tagData.ContainsKey("Tangents"))
                {
                    this.m_tangents = (Byte4[]) tagData["Tangents"];
                }
            }
            return ((this.m_tangents == null) ? Vector3.Zero : VF_Packer.UnpackNormal(this.m_tangents[vertexIndex]));
        }

        public int GetVerticesCount() => 
            this.m_verticesCount;

        public void LoadAnimationData()
        {
            if (!this.m_loadedData)
            {
                MyModel model = this;
                lock (model)
                {
                    MyLog.Default.WriteLine("MyModel.LoadData -> START", LoggingOptions.LOADING_MODELS);
                    MyLog.Default.IncreaseIndent(LoggingOptions.LOADING_MODELS);
                    MyLog.Default.WriteLine("m_assetName: " + this.m_assetName, LoggingOptions.LOADING_MODELS);
                    MyLog.Default.WriteLine($"Importing asset {this.m_assetName}, path: {this.AssetName}", LoggingOptions.LOADING_MODELS);
                    try
                    {
                        m_importer.ImportData(this.AssetName, null);
                    }
                    catch
                    {
                        MyLog.Default.WriteLine($"Importing asset failed {this.m_assetName}");
                        throw;
                    }
                    Dictionary<string, object> tagData = m_importer.GetTagData();
                    if (tagData.Count == 0)
                    {
                        this.DataVersion = 0;
                        this.Animations = null;
                        this.Bones = null;
                        this.m_boundingBox = new VRageMath.BoundingBox();
                        this.m_boundingSphere = new VRageMath.BoundingSphere();
                        this.m_boundingBoxSize = new Vector3();
                        this.m_boundingBoxSizeHalf = new Vector3();
                        this.Dummies = null;
                        this.BoneMapping = null;
                    }
                    else
                    {
                        this.DataVersion = m_importer.DataVersion;
                        this.Animations = (ModelAnimations) tagData["Animations"];
                        this.Bones = (MyModelBone[]) tagData["Bones"];
                        this.m_boundingBox = (VRageMath.BoundingBox) tagData["BoundingBox"];
                        this.m_boundingSphere = (VRageMath.BoundingSphere) tagData["BoundingSphere"];
                        this.m_boundingBoxSize = this.BoundingBox.Max - this.BoundingBox.Min;
                        this.m_boundingBoxSizeHalf = this.BoundingBoxSize / 2f;
                        this.Dummies = tagData["Dummies"] as Dictionary<string, MyModelDummy>;
                        this.BoneMapping = tagData["BoneMapping"] as Vector3I[];
                        if (this.BoneMapping.Length == 0)
                        {
                            this.BoneMapping = null;
                        }
                    }
                    this.ModelInfo = new MyModelInfo(this.GetTrianglesCount(), this.GetVerticesCount(), this.BoundingBoxSize);
                    if (tagData.Count != 0)
                    {
                        this.m_loadedData = true;
                    }
                    MyLog.Default.DecreaseIndent(LoggingOptions.LOADING_MODELS);
                    MyLog.Default.WriteLine("MyModel.LoadAnimationData -> END", LoggingOptions.LOADING_MODELS);
                }
            }
        }

        public unsafe void LoadData()
        {
            MyModel model = this;
            lock (model)
            {
                Dictionary<string, object> tagData;
                object obj2;
                List<int> list;
                int num3;
                if (!this.m_loadedData)
                {
                    MyLog.Default.WriteLine("MyModel.LoadData -> START", LoggingOptions.LOADING_MODELS);
                    MyLog.Default.IncreaseIndent(LoggingOptions.LOADING_MODELS);
                    MyLog.Default.WriteLine("m_assetName: " + this.m_assetName, LoggingOptions.LOADING_MODELS);
                    MyLog.Default.WriteLine($"Importing asset {this.m_assetName}, path: {this.AssetName}", LoggingOptions.LOADING_MODELS);
                    string assetName = this.AssetName;
                    if (!MyFileSystem.FileExists(Path.IsPathRooted(this.AssetName) ? this.AssetName : Path.Combine(MyFileSystem.ContentPath, this.AssetName)))
                    {
                        assetName = @"Models\Debug\Error.mwm";
                    }
                    try
                    {
                        m_importer.ImportData(assetName, null);
                    }
                    catch
                    {
                        MyLog.Default.WriteLine($"Importing asset failed {this.m_assetName}");
                        throw;
                    }
                    this.DataVersion = m_importer.DataVersion;
                    tagData = m_importer.GetTagData();
                    if (tagData.Count == 0)
                    {
                        throw new Exception($"Uncompleted tagData for asset: {this.m_assetName}, path: {this.AssetName}");
                    }
                    HalfVector4[] vectorArray = (HalfVector4[]) tagData["Vertices"];
                    Byte4[] numArray = (Byte4[]) tagData["Normals"];
                    this.m_vertices = new MyCompressedVertexNormal[vectorArray.Length];
                    if (numArray.Length == 0)
                    {
                        for (int i = 0; i < vectorArray.Length; i++)
                        {
                            MyCompressedVertexNormal normal = new MyCompressedVertexNormal {
                                Position = vectorArray[i]
                            };
                            this.m_vertices[i] = normal;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vectorArray.Length; i++)
                        {
                            this.m_vertices[i] = new MyCompressedVertexNormal { 
                                Position = vectorArray[i],
                                Normal = numArray[i]
                            };
                        }
                    }
                    this.m_verticesCount = vectorArray.Length;
                    this.m_meshContainer.Clear();
                    if (tagData.ContainsKey("MeshParts"))
                    {
                        list = new List<int>(this.GetVerticesCount());
                        num3 = 0;
                        using (List<MyMeshPartInfo>.Enumerator enumerator = (tagData["MeshParts"] as List<MyMeshPartInfo>).GetEnumerator())
                        {
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    MyMeshPartInfo current = enumerator.Current;
                                    MyMesh item = new MyMesh(current, this.m_assetName) {
                                        IndexStart = list.Count,
                                        TriCount = current.m_indices.Count / 3
                                    };
                                    if (this.m_loadUV && !this.m_hasUV)
                                    {
                                        this.m_texCoords = (HalfVector2[]) tagData["TexCoords0"];
                                        this.m_hasUV = true;
                                        this.m_loadUV = false;
                                    }
                                    if (item.TriCount != 0)
                                    {
                                        foreach (int num4 in current.m_indices)
                                        {
                                            list.Add(num4);
                                            if (num4 > num3)
                                            {
                                                num3 = num4;
                                            }
                                        }
                                        this.m_meshContainer.Add(item);
                                        continue;
                                    }
                                }
                                else
                                {
                                    goto TR_003B;
                                }
                                break;
                            }
                            goto TR_0000;
                        }
                    }
                }
                else
                {
                    goto TR_0000;
                }
            TR_0034:
                this.m_meshSections.Clear();
                if (tagData.ContainsKey("Sections"))
                {
                    int num6 = 0;
                    foreach (MyMeshSectionInfo info2 in tagData["Sections"] as List<MyMeshSectionInfo>)
                    {
                        MyMeshSection section1 = new MyMeshSection();
                        section1.Name = info2.Name;
                        section1.Index = num6;
                        MyMeshSection section = section1;
                        this.m_meshSections[section.Name] = section;
                        num6++;
                    }
                }
                if (tagData.ContainsKey("LODs"))
                {
                    this.m_lods = tagData["LODs"] as MyLODDescriptor[];
                }
                if (tagData.ContainsKey("ModelBvh"))
                {
                    this.m_bvh = new MyQuantizedBvhAdapter(tagData["ModelBvh"] as GImpactQuantizedBvh, this);
                }
                this.Animations = (ModelAnimations) tagData["Animations"];
                this.Bones = (MyModelBone[]) tagData["Bones"];
                Vector4I[] vectoriArray = (Vector4I[]) tagData["BlendIndices"];
                Vector4[] vectorArray2 = (Vector4[]) tagData["BlendWeights"];
                if (((vectoriArray != null) && ((vectoriArray.Length != 0) && ((vectorArray2 != null) && (vectoriArray.Length == vectorArray2.Length)))) && (vectoriArray.Length == this.m_vertices.Length))
                {
                    this.m_bonesIndicesWeights = new MyCompressedBoneIndicesWeights[vectoriArray.Length];
                    for (int i = 0; i < vectoriArray.Length; i++)
                    {
                        this.m_bonesIndicesWeights[i].Indices = new Byte4((float) vectoriArray[i].X, (float) vectoriArray[i].Y, (float) vectoriArray[i].Z, (float) vectoriArray[i].W);
                        this.m_bonesIndicesWeights[i].Weights = new HalfVector4(vectorArray2[i]);
                    }
                }
                this.m_boundingBox = (VRageMath.BoundingBox) tagData["BoundingBox"];
                this.m_boundingSphere = (VRageMath.BoundingSphere) tagData["BoundingSphere"];
                this.m_boundingBoxSize = this.BoundingBox.Max - this.BoundingBox.Min;
                this.m_boundingBoxSizeHalf = this.BoundingBoxSize / 2f;
                this.Dummies = tagData["Dummies"] as Dictionary<string, MyModelDummy>;
                this.BoneMapping = tagData["BoneMapping"] as Vector3I[];
                if (tagData.ContainsKey("ModelFractures"))
                {
                    this.ModelFractures = (MyModelFractures) tagData["ModelFractures"];
                }
                if (tagData.TryGetValue("PatternScale", out obj2))
                {
                    this.PatternScale = (float) obj2;
                }
                if (this.BoneMapping.Length == 0)
                {
                    this.BoneMapping = null;
                }
                if (tagData.ContainsKey("HavokCollisionGeometry"))
                {
                    this.HavokData = (byte[]) tagData["HavokCollisionGeometry"];
                    byte[] buffer = (byte[]) tagData["HavokCollisionGeometry"];
                    if ((buffer.Length != 0) && HkBaseSystem.IsThreadInitialized)
                    {
                        bool flag2;
                        bool flag3;
                        List<HkShape> shapes = new List<HkShape>();
                        if (!HkShapeLoader.LoadShapesListFromBuffer(buffer, shapes, out flag2, out flag3))
                        {
                            MyLog.Default.WriteLine($"Model {this.AssetName} - Unable to load collision geometry", LoggingOptions.LOADING_MODELS);
                        }
                        if (shapes.Count > 10)
                        {
                            MyLog.Default.WriteLine($"Model {this.AssetName} - Found too many collision shapes, only the first 10 will be used", LoggingOptions.LOADING_MODELS);
                        }
                        HkShape[] havokCollisionShapes = this.HavokCollisionShapes;
                        if (shapes.Count > 0)
                        {
                            this.HavokCollisionShapes = shapes.ToArray();
                        }
                        else
                        {
                            MyLog.Default.WriteLine($"Model {this.AssetName} - Unable to load collision geometry from file, default collision will be used !");
                        }
                        if (flag3)
                        {
                            this.HavokDestructionData = buffer;
                        }
                        this.ExportedWrong = !flag2;
                    }
                }
                if (tagData.ContainsKey("HavokDestruction") && (((byte[]) tagData["HavokDestruction"]).Length != 0))
                {
                    this.HavokDestructionData = (byte[]) tagData["HavokDestruction"];
                }
                this.CopyTriangleIndices();
                this.m_trianglesCount = this.Triangles.Length;
                MyLog.Default.WriteLine("Triangles.Length: " + this.Triangles.Length, LoggingOptions.LOADING_MODELS);
                MyLog.Default.WriteLine("Vertexes.Length: " + this.GetVerticesCount(), LoggingOptions.LOADING_MODELS);
                MyLog.Default.WriteLine("UseChannelTextures: " + ((bool) tagData["UseChannelTextures"]).ToString(), LoggingOptions.LOADING_MODELS);
                MyLog.Default.WriteLine("BoundingBox: " + this.BoundingBox, LoggingOptions.LOADING_MODELS);
                MyLog.Default.WriteLine("BoundingSphere: " + this.BoundingSphere, LoggingOptions.LOADING_MODELS);
                int* numPtr1 = (int*) ref Stats.PerAppLifetime.MyModelsCount;
                numPtr1[0]++;
                int* numPtr2 = (int*) ref Stats.PerAppLifetime.MyModelsMeshesCount;
                numPtr2[0] += this.m_meshContainer.Count;
                int* numPtr3 = (int*) ref Stats.PerAppLifetime.MyModelsVertexesCount;
                numPtr3[0] += this.GetVerticesCount();
                int* numPtr4 = (int*) ref Stats.PerAppLifetime.MyModelsTrianglesCount;
                numPtr4[0] += this.Triangles.Length;
                this.ModelInfo = new MyModelInfo(this.GetTrianglesCount(), this.GetVerticesCount(), this.BoundingBoxSize);
                this.m_loadedData = true;
                this.m_loadingErrorProcessed = false;
                MyLog.Default.DecreaseIndent(LoggingOptions.LOADING_MODELS);
                MyLog.Default.WriteLine("MyModel.LoadData -> END", LoggingOptions.LOADING_MODELS);
                goto TR_0000;
            TR_003B:
                if (num3 > 0xffff)
                {
                    this.m_Indices = list.ToArray();
                }
                else
                {
                    this.m_Indices_16bit = new ushort[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.m_Indices_16bit[i] = (ushort) list[i];
                    }
                }
                goto TR_0034;
            TR_0000:;
            }
        }

        public void LoadOnlyDummies()
        {
            if (!this.m_loadedData)
            {
                MyModel model = this;
                lock (model)
                {
                    MyLog.Default.WriteLine("MyModel.LoadSnapPoints -> START", LoggingOptions.LOADING_MODELS);
                    using (MyLog.Default.IndentUsing(LoggingOptions.LOADING_MODELS))
                    {
                        MyLog.Default.WriteLine("m_assetName: " + this.m_assetName, LoggingOptions.LOADING_MODELS);
                        MyModelImporter importer = new MyModelImporter();
                        MyLog.Default.WriteLine($"Importing asset {this.m_assetName}, path: {this.AssetName}", LoggingOptions.LOADING_MODELS);
                        try
                        {
                            string[] tags = new string[] { "Dummies" };
                            importer.ImportData(this.AssetName, tags);
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine($"Importing asset failed {this.m_assetName}, message: {exception.Message}, stack:{exception.StackTrace}");
                        }
                        Dictionary<string, object> tagData = importer.GetTagData();
                        this.Dummies = (tagData.Count <= 0) ? new Dictionary<string, MyModelDummy>() : (tagData["Dummies"] as Dictionary<string, MyModelDummy>);
                    }
                }
            }
        }

        public void LoadOnlyModelInfo()
        {
            if (!this.m_loadedData)
            {
                MyModel model = this;
                lock (model)
                {
                    MyLog.Default.WriteLine("MyModel.LoadModelData -> START", LoggingOptions.LOADING_MODELS);
                    using (MyLog.Default.IndentUsing(LoggingOptions.LOADING_MODELS))
                    {
                        MyLog.Default.WriteLine("m_assetName: " + this.m_assetName, LoggingOptions.LOADING_MODELS);
                        MyModelImporter importer = new MyModelImporter();
                        MyLog.Default.WriteLine($"Importing asset {this.m_assetName}, path: {this.AssetName}", LoggingOptions.LOADING_MODELS);
                        try
                        {
                            string[] tags = new string[] { "ModelInfo" };
                            importer.ImportData(this.AssetName, tags);
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine($"Importing asset failed {this.m_assetName}, message: {exception.Message}, stack:{exception.StackTrace}");
                        }
                        Dictionary<string, object> tagData = importer.GetTagData();
                        this.ModelInfo = (tagData.Count <= 0) ? new MyModelInfo(0, 0, Vector3.Zero) : (tagData["ModelInfo"] as MyModelInfo);
                    }
                }
            }
        }

        public bool LoadTexCoordData()
        {
            if (!this.m_hasUV)
            {
                MyModel model = this;
                lock (model)
                {
                    try
                    {
                        string[] tags = new string[] { "TexCoords0" };
                        m_importer.ImportData(this.AssetName, tags);
                    }
                    catch
                    {
                        MyLog.Default.WriteLine($"Importing asset failed {this.m_assetName}");
                        return false;
                    }
                    Dictionary<string, object> tagData = m_importer.GetTagData();
                    this.m_texCoords = (HalfVector2[]) tagData["TexCoords0"];
                    this.m_hasUV = true;
                    this.m_loadUV = false;
                }
            }
            return this.m_hasUV;
        }

        public unsafe void Rescale(float scaleFactor)
        {
            if (this.m_scaleFactor != scaleFactor)
            {
                float num = scaleFactor / this.m_scaleFactor;
                this.m_scaleFactor = scaleFactor;
                int vertexIndex = 0;
                while (true)
                {
                    if (vertexIndex >= this.m_verticesCount)
                    {
                        if (this.Dummies != null)
                        {
                            foreach (KeyValuePair<string, MyModelDummy> pair in this.Dummies)
                            {
                                Matrix matrix = pair.Value.Matrix;
                                Matrix* matrixPtr1 = (Matrix*) ref matrix;
                                matrixPtr1.Translation *= num;
                                pair.Value.Matrix = matrix;
                            }
                        }
                        Vector3* vectorPtr1 = (Vector3*) ref this.m_boundingBox.Min;
                        vectorPtr1[0] *= num;
                        Vector3* vectorPtr2 = (Vector3*) ref this.m_boundingBox.Max;
                        vectorPtr2[0] *= num;
                        this.m_boundingBoxSize = this.BoundingBox.Max - this.BoundingBox.Min;
                        this.m_boundingBoxSizeHalf = this.BoundingBoxSize / 2f;
                        float* singlePtr1 = (float*) ref this.m_boundingSphere.Radius;
                        singlePtr1[0] *= num;
                        break;
                    }
                    Vector3 newPosition = this.GetVertex(vertexIndex) * num;
                    this.SetVertexPosition(vertexIndex, ref newPosition);
                    vertexIndex++;
                }
            }
        }

        public void SetVertexPosition(int vertexIndex, ref Vector3 newPosition)
        {
            this.m_vertices[vertexIndex].Position = VF_Packer.PackPosition((Vector3) newPosition);
        }

        public bool TryGetMeshSection(string name, out MyMeshSection section) => 
            this.m_meshSections.TryGetValue(name, out section);

        public unsafe bool UnloadData()
        {
            bool loadedData = this.m_loadedData;
            this.m_loadedData = false;
            if (this.m_bvh != null)
            {
                this.m_bvh.Close();
                this.m_bvh = null;
            }
            int* numPtr1 = (int*) ref Stats.PerAppLifetime.MyModelsMeshesCount;
            numPtr1[0] -= this.m_meshContainer.Count;
            if (this.m_vertices != null)
            {
                int* numPtr2 = (int*) ref Stats.PerAppLifetime.MyModelsVertexesCount;
                numPtr2[0] -= this.GetVerticesCount();
            }
            if (this.Triangles != null)
            {
                int* numPtr3 = (int*) ref Stats.PerAppLifetime.MyModelsTrianglesCount;
                numPtr3[0] -= this.Triangles.Length;
            }
            if (loadedData)
            {
                int* numPtr4 = (int*) ref Stats.PerAppLifetime.MyModelsCount;
                numPtr4[0]--;
            }
            if (this.HavokCollisionShapes != null)
            {
                int index = 0;
                while (true)
                {
                    if (index >= this.HavokCollisionShapes.Length)
                    {
                        this.HavokCollisionShapes = null;
                        break;
                    }
                    this.HavokCollisionShapes[index].RemoveReference();
                    index++;
                }
            }
            if (this.HavokBreakableShapes != null)
            {
                this.HavokBreakableShapes = null;
            }
            this.m_vertices = null;
            this.Triangles = null;
            this.m_meshContainer.Clear();
            this.m_Indices_16bit = null;
            this.m_Indices = null;
            this.Dummies = null;
            this.HavokData = null;
            this.HavokDestructionData = null;
            this.m_scaleFactor = 1f;
            this.Animations = null;
            return loadedData;
        }

        int IMyModel.GetDummies(IDictionary<string, IMyModelDummy> dummies)
        {
            if (this.Dummies == null)
            {
                return 0;
            }
            if (dummies != null)
            {
                foreach (KeyValuePair<string, MyModelDummy> pair in this.Dummies)
                {
                    dummies.Add(pair.Key, pair.Value);
                }
            }
            return this.Dummies.Count;
        }

        public bool KeepInMemory { get; private set; }

        public int DataVersion { get; private set; }

        public MyCompressedVertexNormal[] Vertices =>
            this.m_vertices;

        private static MyModelImporter m_importer
        {
            get
            {
                if (m_perThreadImporter == null)
                {
                    m_perThreadImporter = new MyModelImporter();
                }
                return m_perThreadImporter;
            }
        }

        public int[] Indices =>
            this.m_Indices;

        public ushort[] Indices16 =>
            this.m_Indices_16bit;

        public bool HasUV =>
            this.m_hasUV;

        public bool LoadUV
        {
            set => 
                (this.m_loadUV = value);
        }

        public HalfVector2[] TexCoords =>
            this.m_texCoords;

        public bool LoadedData =>
            this.m_loadedData;

        public MyLODDescriptor[] LODs
        {
            get => 
                this.m_lods;
            private set => 
                (this.m_lods = value);
        }

        public float ScaleFactor =>
            this.m_scaleFactor;

        public string AssetName =>
            this.m_assetName;

        public VRageMath.BoundingSphere BoundingSphere =>
            this.m_boundingSphere;

        public VRageMath.BoundingBox BoundingBox =>
            this.m_boundingBox;

        public Vector3 BoundingBoxSize =>
            this.m_boundingBoxSize;

        public Vector3 BoundingBoxSizeHalf =>
            this.m_boundingBoxSizeHalf;

        int IMyModel.UniqueId =>
            this.UniqueId;

        int IMyModel.DataVersion =>
            this.DataVersion;

        Vector3I[] IMyModel.BoneMapping =>
            (this.BoneMapping.Clone() as Vector3I[]);

        float IMyModel.PatternScale =>
            this.PatternScale;
    }
}

