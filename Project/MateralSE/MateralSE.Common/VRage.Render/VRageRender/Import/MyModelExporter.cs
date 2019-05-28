namespace VRageRender.Import
{
    using BulletXNA.BulletCollision;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using VRage.Import;
    using VRage.Security;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender.Animations;
    using VRageRender.Fractures;

    public class MyModelExporter : IDisposable
    {
        private BinaryWriter m_writer;
        private BinaryWriter m_originalWriter;
        private MemoryStream m_cacheStream;

        public MyModelExporter()
        {
        }

        public MyModelExporter(string filePath)
        {
            FileStream output = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            this.m_writer = new BinaryWriter(output);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        private int CalculateIndexSize(Dictionary<string, int> dict)
        {
            int num = 4;
            foreach (KeyValuePair<string, int> pair in dict)
            {
                num = (num + (Encoding.ASCII.GetByteCount(pair.Key) + 1)) + 4;
            }
            return num;
        }

        public void Dispose()
        {
            if (this.m_writer != null)
            {
                this.m_writer.Close();
                this.m_writer = null;
            }
            if (this.m_originalWriter != null)
            {
                this.m_originalWriter.Close();
                this.m_originalWriter = null;
            }
        }

        public bool ExportBool(string tagName, bool value)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(value);
            return true;
        }

        public bool ExportData(string tagName, Dictionary<string, Matrix> dict)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(dict.Count);
            foreach (KeyValuePair<string, Matrix> pair in dict)
            {
                this.m_writer.Write(pair.Key);
                this.WriteMatrix(pair.Value);
            }
            return true;
        }

        public bool ExportData(string tagName, Dictionary<string, MyModelDummy> dict)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(dict.Count);
            foreach (KeyValuePair<string, MyModelDummy> pair in dict)
            {
                this.m_writer.Write(pair.Key);
                this.WriteMatrix(pair.Value.Matrix);
                this.m_writer.Write(pair.Value.CustomData.Count);
                foreach (KeyValuePair<string, object> pair2 in pair.Value.CustomData)
                {
                    this.m_writer.Write(pair2.Key);
                    this.m_writer.Write(pair2.Value.ToString());
                }
            }
            return true;
        }

        public bool ExportData(string tagName, List<MyMeshPartInfo> list)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(list.Count);
            using (List<MyMeshPartInfo>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Export(this.m_writer);
                }
            }
            return true;
        }

        public bool ExportData(string tagName, List<MyMeshSectionInfo> list)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(list.Count);
            using (List<MyMeshSectionInfo>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Export(this.m_writer);
                }
            }
            return true;
        }

        public bool ExportData(string tagName, byte[] byteArray)
        {
            this.WriteTag(tagName);
            if (byteArray == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(byteArray.Length);
            this.m_writer.Write(byteArray);
            return true;
        }

        public bool ExportData(string tagName, int[] intArr)
        {
            this.WriteTag(tagName);
            if (intArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(intArr.Length);
            foreach (int num2 in intArr)
            {
                this.m_writer.Write(num2);
            }
            return true;
        }

        public bool ExportData(string tagName, string[] strArr)
        {
            this.WriteTag(tagName);
            if (strArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(strArr.Length);
            foreach (string str in strArr)
            {
                this.m_writer.Write(str);
            }
            return true;
        }

        public bool ExportData(string tagName, Matrix[] matArr)
        {
            if (matArr != null)
            {
                this.WriteTag(tagName);
                this.m_writer.Write(matArr.Length);
                foreach (Matrix matrix in matArr)
                {
                    this.WriteMatrix(matrix);
                }
            }
            return true;
        }

        public bool ExportData(string tagName, Byte4[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (Byte4 num2 in vctArr)
            {
                this.WriteByte4(num2);
            }
            return true;
        }

        public bool ExportData(string tagName, HalfVector2[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (HalfVector2 vector in vctArr)
            {
                this.WriteVector(vector);
            }
            return true;
        }

        public bool ExportData(string tagName, HalfVector4[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (HalfVector4 vector in vctArr)
            {
                this.WriteVector(vector);
            }
            return true;
        }

        public bool ExportData(string tagName, Vector2[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (Vector2 vector in vctArr)
            {
                this.WriteVector(vector);
            }
            return true;
        }

        public bool ExportData(string tagName, Vector3[] vctArr)
        {
            if (vctArr != null)
            {
                this.WriteTag(tagName);
                this.m_writer.Write(vctArr.Length);
                foreach (Vector3 vector in vctArr)
                {
                    this.WriteVector(vector);
                }
            }
            return true;
        }

        public bool ExportData(string tagName, Vector3I[] vctArr)
        {
            if (vctArr != null)
            {
                this.WriteTag(tagName);
                this.m_writer.Write(vctArr.Length);
                foreach (Vector3I vectori in vctArr)
                {
                    this.WriteVector(vectori);
                }
            }
            return true;
        }

        public bool ExportData(string tagName, Vector4[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (Vector4 vector in vctArr)
            {
                this.WriteVector(vector);
            }
            return true;
        }

        public bool ExportData(string tagName, Vector4I[] vctArr)
        {
            if (vctArr != null)
            {
                this.WriteTag(tagName);
                this.m_writer.Write(vctArr.Length);
                foreach (Vector4I vectori in vctArr)
                {
                    this.WriteVector(vectori);
                }
            }
            return true;
        }

        public bool ExportData(string tagName, BoundingBox boundingBox)
        {
            this.WriteTag(tagName);
            this.WriteVector(boundingBox.Min);
            this.WriteVector(boundingBox.Max);
            return true;
        }

        public bool ExportData(string tagName, BoundingSphere boundingSphere)
        {
            this.WriteTag(tagName);
            this.WriteVector(boundingSphere.Center);
            this.m_writer.Write(boundingSphere.Radius);
            return true;
        }

        public bool ExportData(string tagName, ModelAnimations modelAnimations)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(modelAnimations.Clips.Count);
            foreach (MyAnimationClip clip in modelAnimations.Clips)
            {
                this.Write(clip);
            }
            this.m_writer.Write(modelAnimations.Skeleton.Count);
            foreach (int num in modelAnimations.Skeleton)
            {
                this.m_writer.Write(num);
            }
            return true;
        }

        public bool ExportData(string tagName, MyModelInfo modelInfo)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(modelInfo.TrianglesCount);
            this.m_writer.Write(modelInfo.VerticesCount);
            this.WriteVector(modelInfo.BoundingBoxSize);
            return true;
        }

        public bool ExportData(string tagName, MyLODDescriptor[] lodDescriptions)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(lodDescriptions.Length);
            MyLODDescriptor[] descriptorArray = lodDescriptions;
            for (int i = 0; i < descriptorArray.Length; i++)
            {
                descriptorArray[i].Write(this.m_writer);
            }
            return true;
        }

        public bool ExportData(string tagName, MyModelBone[] bones)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(bones.Length);
            foreach (MyModelBone bone in bones)
            {
                this.m_writer.Write(bone.Name);
                this.m_writer.Write(bone.Parent);
                this.WriteMatrix(bone.Transform);
            }
            return true;
        }

        public void ExportData(string tagName, Md5.Hash hash)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(hash.A);
            this.m_writer.Write(hash.B);
            this.m_writer.Write(hash.C);
            this.m_writer.Write(hash.D);
        }

        public void ExportData(string tagName, MyModelFractures modelFractures)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(modelFractures.Version);
            this.m_writer.Write((modelFractures.Fractures != null) ? modelFractures.Fractures.Length : 0);
            foreach (MyFractureSettings settings in modelFractures.Fractures)
            {
                if (settings is RandomSplitFractureSettings)
                {
                    RandomSplitFractureSettings settings2 = (RandomSplitFractureSettings) settings;
                    this.m_writer.Write("RandomSplit");
                    this.m_writer.Write(settings2.NumObjectsOnLevel1);
                    this.m_writer.Write(settings2.NumObjectsOnLevel2);
                    this.m_writer.Write(settings2.RandomRange);
                    this.m_writer.Write(settings2.RandomSeed1);
                    this.m_writer.Write(settings2.RandomSeed2);
                    this.m_writer.Write(settings2.SplitPlane);
                }
                else if (settings is VoronoiFractureSettings)
                {
                    VoronoiFractureSettings settings3 = (VoronoiFractureSettings) settings;
                    this.m_writer.Write("Voronoi");
                    this.m_writer.Write(settings3.Seed);
                    this.m_writer.Write(settings3.NumSitesToGenerate);
                    this.m_writer.Write(settings3.NumIterations);
                    this.m_writer.Write(settings3.SplitPlane);
                }
                else if (settings is WoodFractureSettings)
                {
                    WoodFractureSettings settings4 = (WoodFractureSettings) settings;
                    this.m_writer.Write("WoodFracture");
                    this.m_writer.Write(settings4.BoardCustomSplittingPlaneAxis);
                    this.m_writer.Write(settings4.BoardFractureLineShearingRange);
                    this.m_writer.Write(settings4.BoardFractureNormalShearingRange);
                    this.m_writer.Write(settings4.BoardNumSubparts);
                    this.m_writer.Write((int) settings4.BoardRotateSplitGeom);
                    this.WriteVector(settings4.BoardScale);
                    this.WriteVector(settings4.BoardScaleRange);
                    this.m_writer.Write(settings4.BoardSplitGeomShiftRangeY);
                    this.m_writer.Write(settings4.BoardSplitGeomShiftRangeZ);
                    this.WriteVector(settings4.BoardSplittingAxis);
                    this.m_writer.Write(settings4.BoardSplittingPlane);
                    this.m_writer.Write(settings4.BoardSurfaceNormalShearingRange);
                    this.m_writer.Write(settings4.BoardWidthRange);
                    this.m_writer.Write(settings4.SplinterCustomSplittingPlaneAxis);
                    this.m_writer.Write(settings4.SplinterFractureLineShearingRange);
                    this.m_writer.Write(settings4.SplinterFractureNormalShearingRange);
                    this.m_writer.Write(settings4.SplinterNumSubparts);
                    this.m_writer.Write((int) settings4.SplinterRotateSplitGeom);
                    this.WriteVector(settings4.SplinterScale);
                    this.WriteVector(settings4.SplinterScaleRange);
                    this.m_writer.Write(settings4.SplinterSplitGeomShiftRangeY);
                    this.m_writer.Write(settings4.SplinterSplitGeomShiftRangeZ);
                    this.WriteVector(settings4.SplinterSplittingAxis);
                    this.m_writer.Write(settings4.SplinterSplittingPlane);
                    this.m_writer.Write(settings4.SplinterSurfaceNormalShearingRange);
                    this.m_writer.Write(settings4.SplinterWidthRange);
                }
            }
        }

        public bool ExportDataPackedAsB4(string tagName, Vector3[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (Vector3 vector in vctArr)
            {
                Byte4 val = new Byte4 {
                    PackedValue = VF_Packer.PackNormal(ref vector)
                };
                this.WriteByte4(val);
            }
            return true;
        }

        public bool ExportDataPackedAsHV2(string tagName, Vector2[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (Vector2 vector in vctArr)
            {
                HalfVector2 val = new HalfVector2(vector);
                this.WriteVector(val);
            }
            return true;
        }

        public bool ExportDataPackedAsHV4(string tagName, Vector3[] vctArr)
        {
            this.WriteTag(tagName);
            if (vctArr == null)
            {
                this.m_writer.Write(0);
                return true;
            }
            this.m_writer.Write(vctArr.Length);
            foreach (Vector3 vector in vctArr)
            {
                this.WriteVector(VF_Packer.PackPosition(ref vector));
            }
            return true;
        }

        public bool ExportFloat(string tagName, float value)
        {
            this.WriteTag(tagName);
            this.m_writer.Write(value);
            return true;
        }

        public static void ExportModelData(string filename, Dictionary<string, object> tagData)
        {
            using (MyModelExporter exporter = new MyModelExporter(filename))
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                List<string> list = new List<string>((string[]) tagData["Debug"]);
                list.RemoveAll(x => x.Contains("Version:"));
                list.Add("Version:01157001");
                exporter.ExportData("Debug", list.ToArray());
                exporter.StartCacheWrite();
                dict.Add("Dummies", exporter.GetCachePosition());
                exporter.ExportData("Dummies", (Dictionary<string, MyModelDummy>) tagData["Dummies"]);
                dict.Add("Vertices", exporter.GetCachePosition());
                exporter.ExportData("Vertices", (HalfVector4[]) tagData["Vertices"]);
                dict.Add("Normals", exporter.GetCachePosition());
                exporter.ExportData("Normals", (Byte4[]) tagData["Normals"]);
                dict.Add("TexCoords0", exporter.GetCachePosition());
                exporter.ExportData("TexCoords0", (HalfVector2[]) tagData["TexCoords0"]);
                dict.Add("Binormals", exporter.GetCachePosition());
                exporter.ExportData("Binormals", (Byte4[]) tagData["Binormals"]);
                dict.Add("Tangents", exporter.GetCachePosition());
                exporter.ExportData("Tangents", (Byte4[]) tagData["Tangents"]);
                dict.Add("TexCoords1", exporter.GetCachePosition());
                exporter.ExportData("TexCoords1", (HalfVector2[]) tagData["TexCoords1"]);
                dict.Add("RescaleFactor", exporter.GetCachePosition());
                exporter.ExportFloat("RescaleFactor", (float) tagData["RescaleFactor"]);
                dict.Add("UseChannelTextures", exporter.GetCachePosition());
                exporter.ExportBool("UseChannelTextures", (bool) tagData["UseChannelTextures"]);
                dict.Add("BoundingBox", exporter.GetCachePosition());
                exporter.ExportData("BoundingBox", (BoundingBox) tagData["BoundingBox"]);
                dict.Add("BoundingSphere", exporter.GetCachePosition());
                exporter.ExportData("BoundingSphere", (BoundingSphere) tagData["BoundingSphere"]);
                dict.Add("SwapWindingOrder", exporter.GetCachePosition());
                exporter.ExportBool("SwapWindingOrder", (bool) tagData["SwapWindingOrder"]);
                dict.Add("MeshParts", exporter.GetCachePosition());
                exporter.ExportData("MeshParts", (List<MyMeshPartInfo>) tagData["MeshParts"]);
                dict.Add("Sections", exporter.GetCachePosition());
                exporter.ExportData("Sections", (List<MyMeshSectionInfo>) tagData["Sections"]);
                dict.Add("ModelBvh", exporter.GetCachePosition());
                exporter.ExportData("ModelBvh", ((GImpactQuantizedBvh) tagData["ModelBvh"]).Save());
                dict.Add("ModelInfo", exporter.GetCachePosition());
                exporter.ExportData("ModelInfo", (MyModelInfo) tagData["ModelInfo"]);
                dict.Add("BlendIndices", exporter.GetCachePosition());
                exporter.ExportData("BlendIndices", (Vector4I[]) tagData["BlendIndices"]);
                dict.Add("BlendWeights", exporter.GetCachePosition());
                exporter.ExportData("BlendWeights", (Vector4[]) tagData["BlendWeights"]);
                dict.Add("Animations", exporter.GetCachePosition());
                exporter.ExportData("Animations", (ModelAnimations) tagData["Animations"]);
                dict.Add("Bones", exporter.GetCachePosition());
                exporter.ExportData("Bones", (MyModelBone[]) tagData["Bones"]);
                dict.Add("BoneMapping", exporter.GetCachePosition());
                exporter.ExportData("BoneMapping", (Vector3I[]) tagData["BoneMapping"]);
                dict.Add("HavokCollisionGeometry", exporter.GetCachePosition());
                exporter.ExportData("HavokCollisionGeometry", (byte[]) tagData["HavokCollisionGeometry"]);
                dict.Add("PatternScale", exporter.GetCachePosition());
                exporter.ExportFloat("PatternScale", (float) tagData["PatternScale"]);
                dict.Add("LODs", exporter.GetCachePosition());
                exporter.ExportData("LODs", (MyLODDescriptor[]) tagData["LODs"]);
                if (tagData.ContainsKey("FBXHash"))
                {
                    dict.Add("FBXHash", exporter.GetCachePosition());
                    exporter.ExportData("FBXHash", (Md5.Hash) tagData["FBXHash"]);
                }
                if (tagData.ContainsKey("HKTHash"))
                {
                    dict.Add("HKTHash", exporter.GetCachePosition());
                    exporter.ExportData("HKTHash", (Md5.Hash) tagData["HKTHash"]);
                }
                if (tagData.ContainsKey("XMLHash"))
                {
                    dict.Add("XMLHash", exporter.GetCachePosition());
                    exporter.ExportData("XMLHash", (Md5.Hash) tagData["XMLHash"]);
                }
                if (tagData.ContainsKey("ModelFractures"))
                {
                    dict.Add("ModelFractures", exporter.GetCachePosition());
                    exporter.ExportData("ModelFractures", (MyModelFractures) tagData["ModelFractures"]);
                }
                exporter.StopCacheWrite();
                exporter.WriteIndexDictionary(dict);
                exporter.FlushCache();
            }
        }

        public void FlushCache()
        {
            this.m_writer.Write(this.m_cacheStream.GetBuffer());
        }

        public int GetCachePosition() => 
            ((int) this.m_writer.BaseStream.Position);

        public void StartCacheWrite()
        {
            this.m_originalWriter = this.m_writer;
            this.m_cacheStream = new MemoryStream();
            this.m_writer = new BinaryWriter(this.m_cacheStream);
        }

        public void StopCacheWrite()
        {
            this.m_writer.Close();
            this.m_writer = this.m_originalWriter;
        }

        protected void Write(MyAnimationClip clip)
        {
            this.m_writer.Write(clip.Name);
            this.m_writer.Write(clip.Duration);
            this.m_writer.Write(clip.Bones.Count);
            foreach (MyAnimationClip.Bone bone in clip.Bones)
            {
                this.m_writer.Write(bone.Name);
                this.m_writer.Write(bone.Keyframes.Count);
                foreach (MyAnimationClip.Keyframe keyframe in bone.Keyframes)
                {
                    this.m_writer.Write(keyframe.Time);
                    this.WriteQuaternion(keyframe.Rotation);
                    this.WriteVector(keyframe.Translation);
                }
            }
        }

        private void WriteByte4(Byte4 val)
        {
            this.m_writer.Write(val.PackedValue);
        }

        public void WriteIndexDictionary(Dictionary<string, int> dict)
        {
            int position = (int) this.m_writer.BaseStream.Position;
            int num2 = this.CalculateIndexSize(dict);
            this.m_writer.Write(dict.Count);
            foreach (KeyValuePair<string, int> pair in dict)
            {
                this.m_writer.Write(pair.Key);
                this.m_writer.Write((int) ((pair.Value + num2) + position));
            }
        }

        private void WriteMatrix(Matrix matrix)
        {
            this.m_writer.Write(matrix.M11);
            this.m_writer.Write(matrix.M12);
            this.m_writer.Write(matrix.M13);
            this.m_writer.Write(matrix.M14);
            this.m_writer.Write(matrix.M21);
            this.m_writer.Write(matrix.M22);
            this.m_writer.Write(matrix.M23);
            this.m_writer.Write(matrix.M24);
            this.m_writer.Write(matrix.M31);
            this.m_writer.Write(matrix.M32);
            this.m_writer.Write(matrix.M33);
            this.m_writer.Write(matrix.M34);
            this.m_writer.Write(matrix.M41);
            this.m_writer.Write(matrix.M42);
            this.m_writer.Write(matrix.M43);
            this.m_writer.Write(matrix.M44);
        }

        private void WriteQuaternion(Quaternion q)
        {
            this.m_writer.Write(q.X);
            this.m_writer.Write(q.Y);
            this.m_writer.Write(q.Z);
            this.m_writer.Write(q.W);
        }

        private void WriteTag(string tagName)
        {
            this.m_writer.Write(tagName);
        }

        private void WriteVector(HalfVector2 val)
        {
            this.m_writer.Write(val.PackedValue);
        }

        private void WriteVector(HalfVector4 val)
        {
            this.m_writer.Write(val.PackedValue);
        }

        private void WriteVector(Vector2 vct)
        {
            this.m_writer.Write(vct.X);
            this.m_writer.Write(vct.Y);
        }

        private void WriteVector(Vector2I vct)
        {
            this.m_writer.Write(vct.X);
            this.m_writer.Write(vct.Y);
        }

        private void WriteVector(Vector3 vct)
        {
            this.m_writer.Write(vct.X);
            this.m_writer.Write(vct.Y);
            this.m_writer.Write(vct.Z);
        }

        private void WriteVector(Vector3I vct)
        {
            this.m_writer.Write(vct.X);
            this.m_writer.Write(vct.Y);
            this.m_writer.Write(vct.Z);
        }

        private void WriteVector(Vector4 vct)
        {
            this.m_writer.Write(vct.X);
            this.m_writer.Write(vct.Y);
            this.m_writer.Write(vct.Z);
            this.m_writer.Write(vct.W);
        }

        private void WriteVector(Vector4I vct)
        {
            this.m_writer.Write(vct.X);
            this.m_writer.Write(vct.Y);
            this.m_writer.Write(vct.Z);
            this.m_writer.Write(vct.W);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyModelExporter.<>c <>9 = new MyModelExporter.<>c();
            public static Predicate<string> <>9__54_0;

            internal bool <ExportModelData>b__54_0(string x) => 
                x.Contains("Version:");
        }
    }
}

