namespace VRageRender.Import
{
    using BulletXNA.BulletCollision;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Security;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender.Animations;
    using VRageRender.Fractures;

    public class MyModelImporter
    {
        private static Dictionary<string, ITagReader> TagReaders;
        private Dictionary<string, object> m_retTagData = new Dictionary<string, object>();
        private int m_version;
        private static string m_debugAssetName;
        public static bool USE_LINEAR_KEYFRAME_REDUCTION;
        public static bool LINEAR_KEYFRAME_REDUCTION_STATS;
        public static Dictionary<string, List<ReductionInfo>> ReductionStats;
        private const float TinyLength = 1E-08f;
        private const float TinyCosAngle = 0.9999999f;

        static MyModelImporter()
        {
            Dictionary<string, ITagReader> dictionary1 = new Dictionary<string, ITagReader>();
            dictionary1.Add("Vertices", new TagReader<HalfVector4[]>(new Func<BinaryReader, HalfVector4[]>(MyModelImporter.ReadArrayOfHalfVector4)));
            dictionary1.Add("Normals", new TagReader<Byte4[]>(new Func<BinaryReader, Byte4[]>(MyModelImporter.ReadArrayOfByte4)));
            dictionary1.Add("TexCoords0", new TagReader<HalfVector2[]>(new Func<BinaryReader, HalfVector2[]>(MyModelImporter.ReadArrayOfHalfVector2)));
            dictionary1.Add("Binormals", new TagReader<Byte4[]>(new Func<BinaryReader, Byte4[]>(MyModelImporter.ReadArrayOfByte4)));
            dictionary1.Add("Tangents", new TagReader<Byte4[]>(new Func<BinaryReader, Byte4[]>(MyModelImporter.ReadArrayOfByte4)));
            dictionary1.Add("TexCoords1", new TagReader<HalfVector2[]>(new Func<BinaryReader, HalfVector2[]>(MyModelImporter.ReadArrayOfHalfVector2)));
            dictionary1.Add("UseChannelTextures", new TagReader<bool>(x => x.ReadBoolean()));
            dictionary1.Add("BoundingBox", new TagReader<BoundingBox>(new Func<BinaryReader, BoundingBox>(MyModelImporter.ReadBoundingBox)));
            dictionary1.Add("BoundingSphere", new TagReader<BoundingSphere>(new Func<BinaryReader, BoundingSphere>(MyModelImporter.ReadBoundingSphere)));
            dictionary1.Add("RescaleFactor", new TagReader<float>(x => x.ReadSingle()));
            dictionary1.Add("SwapWindingOrder", new TagReader<bool>(x => x.ReadBoolean()));
            dictionary1.Add("Dummies", new TagReader<Dictionary<string, MyModelDummy>>(new Func<BinaryReader, Dictionary<string, MyModelDummy>>(MyModelImporter.ReadDummies)));
            dictionary1.Add("MeshParts", new TagReader<List<MyMeshPartInfo>>(new Func<BinaryReader, int, List<MyMeshPartInfo>>(MyModelImporter.ReadMeshParts)));
            dictionary1.Add("Sections", new TagReader<List<MyMeshSectionInfo>>(new Func<BinaryReader, int, List<MyMeshSectionInfo>>(MyModelImporter.ReadMeshSections)));
            dictionary1.Add("ModelBvh", new TagReader<GImpactQuantizedBvh>(delegate (BinaryReader reader) {
                GImpactQuantizedBvh bvh1 = new GImpactQuantizedBvh();
                bvh1.Load(ReadArrayOfBytes(reader));
                return bvh1;
            }));
            dictionary1.Add("ModelInfo", new TagReader<MyModelInfo>(reader => new MyModelInfo(reader.ReadInt32(), reader.ReadInt32(), ImportVector3(reader))));
            dictionary1.Add("BlendIndices", new TagReader<Vector4I[]>(new Func<BinaryReader, Vector4I[]>(MyModelImporter.ReadArrayOfVector4Int)));
            dictionary1.Add("BlendWeights", new TagReader<Vector4[]>(new Func<BinaryReader, Vector4[]>(MyModelImporter.ReadArrayOfVector4)));
            dictionary1.Add("Animations", new TagReader<ModelAnimations>(new Func<BinaryReader, ModelAnimations>(MyModelImporter.ReadAnimations)));
            dictionary1.Add("Bones", new TagReader<MyModelBone[]>(new Func<BinaryReader, MyModelBone[]>(MyModelImporter.ReadBones)));
            dictionary1.Add("BoneMapping", new TagReader<Vector3I[]>(new Func<BinaryReader, Vector3I[]>(MyModelImporter.ReadArrayOfVector3Int)));
            dictionary1.Add("HavokCollisionGeometry", new TagReader<byte[]>(new Func<BinaryReader, byte[]>(MyModelImporter.ReadArrayOfBytes)));
            dictionary1.Add("PatternScale", new TagReader<float>(x => x.ReadSingle()));
            dictionary1.Add("LODs", new TagReader<MyLODDescriptor[]>(new Func<BinaryReader, int, MyLODDescriptor[]>(MyModelImporter.ReadLODs)));
            dictionary1.Add("HavokDestructionGeometry", new TagReader<byte[]>(new Func<BinaryReader, byte[]>(MyModelImporter.ReadArrayOfBytes)));
            dictionary1.Add("HavokDestruction", new TagReader<byte[]>(new Func<BinaryReader, byte[]>(MyModelImporter.ReadArrayOfBytes)));
            dictionary1.Add("FBXHash", new TagReader<Md5.Hash>(new Func<BinaryReader, Md5.Hash>(MyModelImporter.ReadHash)));
            dictionary1.Add("HKTHash", new TagReader<Md5.Hash>(new Func<BinaryReader, Md5.Hash>(MyModelImporter.ReadHash)));
            dictionary1.Add("XMLHash", new TagReader<Md5.Hash>(new Func<BinaryReader, Md5.Hash>(MyModelImporter.ReadHash)));
            dictionary1.Add("ModelFractures", new TagReader<MyModelFractures>(new Func<BinaryReader, MyModelFractures>(MyModelImporter.ReadModelFractures)));
            TagReaders = dictionary1;
            USE_LINEAR_KEYFRAME_REDUCTION = true;
            LINEAR_KEYFRAME_REDUCTION_STATS = false;
            ReductionStats = new Dictionary<string, List<ReductionInfo>>();
        }

        private static void CalculateKeyframeDeltas(List<MyAnimationClip.Keyframe> keyframes)
        {
            for (int i = 1; i < keyframes.Count; i++)
            {
                MyAnimationClip.Keyframe keyframe = keyframes[i - 1];
                MyAnimationClip.Keyframe keyframe2 = keyframes[i];
                keyframe2.InvTimeDiff = 1.0 / (keyframe2.Time - keyframe.Time);
            }
        }

        public void Clear()
        {
            this.m_retTagData.Clear();
            this.m_version = 0;
        }

        public Dictionary<string, object> GetTagData() => 
            this.m_retTagData;

        public void ImportData(string assetFileName, string[] tags = null)
        {
            this.Clear();
            m_debugAssetName = assetFileName;
            using (Stream stream = MyFileSystem.OpenRead(Path.IsPathRooted(assetFileName) ? assetFileName : Path.Combine(MyFileSystem.ContentPath, assetFileName)))
            {
                if (stream != null)
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        this.LoadTagData(reader, tags);
                    }
                    stream.Close();
                }
            }
        }

        private static Quaternion ImportQuaternion(BinaryReader reader)
        {
            Quaternion quaternion;
            quaternion.X = reader.ReadSingle();
            quaternion.Y = reader.ReadSingle();
            quaternion.Z = reader.ReadSingle();
            quaternion.W = reader.ReadSingle();
            return quaternion;
        }

        private static Vector2 ImportVector2(BinaryReader reader)
        {
            Vector2 vector;
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            return vector;
        }

        private static Vector3 ImportVector3(BinaryReader reader)
        {
            Vector3 vector;
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            vector.Z = reader.ReadSingle();
            return vector;
        }

        private static Vector3I ImportVector3Int(BinaryReader reader)
        {
            Vector3I vectori;
            vectori.X = reader.ReadInt32();
            vectori.Y = reader.ReadInt32();
            vectori.Z = reader.ReadInt32();
            return vectori;
        }

        private static Vector4 ImportVector4(BinaryReader reader)
        {
            Vector4 vector;
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            vector.Z = reader.ReadSingle();
            vector.W = reader.ReadSingle();
            return vector;
        }

        private static Vector4I ImportVector4Int(BinaryReader reader)
        {
            Vector4I vectori;
            vectori.X = reader.ReadInt32();
            vectori.Y = reader.ReadInt32();
            vectori.Z = reader.ReadInt32();
            vectori.W = reader.ReadInt32();
            return vectori;
        }

        private static void LinearKeyframeReduction(LinkedList<MyAnimationClip.Keyframe> keyframes, float translationThreshold, float rotationThreshold)
        {
            if (keyframes.Count >= 3)
            {
                LinkedListNode<MyAnimationClip.Keyframe> next = keyframes.First.Next;
                while (true)
                {
                    LinkedListNode<MyAnimationClip.Keyframe> node2 = next.Next;
                    if (node2 == null)
                    {
                        return;
                    }
                    MyAnimationClip.Keyframe keyframe = next.Value;
                    MyAnimationClip.Keyframe keyframe2 = node2.Value;
                    float amount = (float) ((next.Value.Time - next.Previous.Value.Time) / (node2.Value.Time - next.Previous.Value.Time));
                    MyAnimationClip.Keyframe local1 = next.Previous.Value;
                    Vector3 vector = Vector3.Lerp(local1.Translation, keyframe2.Translation, amount);
                    Quaternion quaternion = Quaternion.Slerp(local1.Rotation, keyframe2.Rotation, amount);
                    Vector3 vector2 = vector - keyframe.Translation;
                    if ((vector2.LengthSquared() < translationThreshold) && (Quaternion.Dot(quaternion, keyframe.Rotation) > rotationThreshold))
                    {
                        keyframes.Remove(next);
                    }
                    next = node2;
                }
            }
        }

        private void LoadOldVersion(BinaryReader reader)
        {
            string key = reader.ReadString();
            this.m_retTagData.Add(key, ReadDummies(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfHalfVector4(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfByte4(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfHalfVector2(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfByte4(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfByte4(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfHalfVector2(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadBoolean());
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadSingle());
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadSingle());
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadBoolean());
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadBoolean());
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadSingle());
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadSingle());
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadBoundingBox(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadBoundingSphere(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, reader.ReadBoolean());
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadMeshParts(reader, this.m_version));
            key = reader.ReadString();
            GImpactQuantizedBvh bvh = new GImpactQuantizedBvh();
            bvh.Load(ReadArrayOfBytes(reader));
            this.m_retTagData.Add(key, bvh);
            key = reader.ReadString();
            this.m_retTagData.Add(key, new MyModelInfo(reader.ReadInt32(), reader.ReadInt32(), ImportVector3(reader)));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfVector4Int(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfVector4(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadAnimations(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadBones(reader));
            key = reader.ReadString();
            this.m_retTagData.Add(key, ReadArrayOfVector3Int(reader));
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                key = reader.ReadString();
                this.m_retTagData.Add(key, ReadArrayOfBytes(reader));
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                key = reader.ReadString();
                this.m_retTagData.Add(key, reader.ReadSingle());
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                key = reader.ReadString();
                this.m_retTagData.Add(key, ReadLODs(reader, 0x104412));
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                key = reader.ReadString();
                this.m_retTagData.Add(key, ReadArrayOfBytes(reader));
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                key = reader.ReadString();
                this.m_retTagData.Add(key, ReadArrayOfBytes(reader));
            }
        }

        private void LoadTagData(BinaryReader reader, string[] tags)
        {
            string key = reader.ReadString();
            string[] strArray = ReadArrayOfString(reader);
            this.m_retTagData.Add(key, strArray);
            string str2 = "Version:";
            if ((strArray.Length != 0) && strArray[0].Contains(str2))
            {
                string str3 = strArray[0].Replace(str2, "");
                this.m_version = Convert.ToInt32(str3);
            }
            if (this.m_version < 0x104412)
            {
                this.LoadOldVersion(reader);
            }
            else
            {
                Dictionary<string, int> dictionary = this.ReadIndexDictionary(reader);
                if (tags == null)
                {
                    tags = dictionary.Keys.ToArray<string>();
                }
                foreach (string str4 in tags)
                {
                    if (dictionary.ContainsKey(str4))
                    {
                        int num2 = dictionary[str4];
                        reader.BaseStream.Seek((long) num2, SeekOrigin.Begin);
                        reader.ReadString();
                        if (TagReaders.ContainsKey(str4))
                        {
                            this.m_retTagData.Add(str4, TagReaders[str4].Read(reader, this.m_version));
                        }
                    }
                }
            }
        }

        private static void PercentageKeyframeReduction(LinkedList<MyAnimationClip.Keyframe> keyframes, float ratio)
        {
            if (keyframes.Count >= 3)
            {
                float num = 0f;
                int num2 = (int) (keyframes.Count * ratio);
                if (num2 != 0)
                {
                    float num3 = ((float) num2) / ((float) keyframes.Count);
                    LinkedListNode<MyAnimationClip.Keyframe> next = keyframes.First.Next;
                    while (true)
                    {
                        LinkedListNode<MyAnimationClip.Keyframe> node2 = next.Next;
                        if (node2 == null)
                        {
                            return;
                        }
                        if (num < 1f)
                        {
                            next = node2;
                        }
                        else
                        {
                            while (num >= 1f)
                            {
                                keyframes.Remove(next);
                                next = node2;
                                node2 = next.Next;
                                num--;
                            }
                        }
                        num += num3;
                    }
                }
            }
        }

        private static ModelAnimations ReadAnimations(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            ModelAnimations animations = new ModelAnimations();
            while (true)
            {
                num--;
                if (num <= 0)
                {
                    int num2 = reader.ReadInt32();
                    while (true)
                    {
                        num2--;
                        if (num2 <= 0)
                        {
                            return animations;
                        }
                        int num3 = reader.ReadInt32();
                        animations.Skeleton.Add(num3);
                    }
                }
                MyAnimationClip item = ReadClip(reader);
                animations.Clips.Add(item);
            }
        }

        private static Byte4[] ReadArrayOfByte4(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            Byte4[] numArray = new Byte4[num];
            for (int i = 0; i < num; i++)
            {
                numArray[i] = ReadByte4(reader);
            }
            return numArray;
        }

        private static byte[] ReadArrayOfBytes(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            return reader.ReadBytes(count);
        }

        private static HalfVector2[] ReadArrayOfHalfVector2(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            HalfVector2[] vectorArray = new HalfVector2[num];
            for (int i = 0; i < num; i++)
            {
                vectorArray[i] = ReadHalfVector2(reader);
            }
            return vectorArray;
        }

        private static HalfVector4[] ReadArrayOfHalfVector4(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            HalfVector4[] vectorArray = new HalfVector4[num];
            for (int i = 0; i < num; i++)
            {
                vectorArray[i] = ReadHalfVector4(reader);
            }
            return vectorArray;
        }

        private static int[] ReadArrayOfInt(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            int[] numArray = new int[num];
            for (int i = 0; i < num; i++)
            {
                numArray[i] = reader.ReadInt32();
            }
            return numArray;
        }

        private static string[] ReadArrayOfString(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            string[] strArray = new string[num];
            for (int i = 0; i < num; i++)
            {
                strArray[i] = reader.ReadString();
            }
            return strArray;
        }

        private static Vector2[] ReadArrayOfVector2(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            Vector2[] vectorArray = new Vector2[num];
            for (int i = 0; i < num; i++)
            {
                vectorArray[i] = ImportVector2(reader);
            }
            return vectorArray;
        }

        private static Vector3[] ReadArrayOfVector3(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            Vector3[] vectorArray = new Vector3[num];
            for (int i = 0; i < num; i++)
            {
                vectorArray[i] = ImportVector3(reader);
            }
            return vectorArray;
        }

        private static Vector3I[] ReadArrayOfVector3Int(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            Vector3I[] vectoriArray = new Vector3I[num];
            for (int i = 0; i < num; i++)
            {
                vectoriArray[i] = ImportVector3Int(reader);
            }
            return vectoriArray;
        }

        private static Vector4[] ReadArrayOfVector4(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            Vector4[] vectorArray = new Vector4[num];
            for (int i = 0; i < num; i++)
            {
                vectorArray[i] = ImportVector4(reader);
            }
            return vectorArray;
        }

        private static Vector4I[] ReadArrayOfVector4Int(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            Vector4I[] vectoriArray = new Vector4I[num];
            for (int i = 0; i < num; i++)
            {
                vectoriArray[i] = ImportVector4Int(reader);
            }
            return vectoriArray;
        }

        private static MyModelBone[] ReadBones(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            MyModelBone[] boneArray = new MyModelBone[num];
            int index = 0;
            while (true)
            {
                num--;
                if (num <= 0)
                {
                    return boneArray;
                }
                MyModelBone bone = new MyModelBone();
                boneArray[index] = bone;
                bone.Name = reader.ReadString();
                index++;
                bone.Index = index;
                bone.Parent = reader.ReadInt32();
                bone.Transform = ReadMatrix(reader);
            }
        }

        private static BoundingBox ReadBoundingBox(BinaryReader reader)
        {
            BoundingBox box;
            box.Min = ImportVector3(reader);
            box.Max = ImportVector3(reader);
            return box;
        }

        private static BoundingSphere ReadBoundingSphere(BinaryReader reader)
        {
            BoundingSphere sphere;
            sphere.Center = ImportVector3(reader);
            sphere.Radius = reader.ReadSingle();
            return sphere;
        }

        private static Byte4 ReadByte4(BinaryReader reader) => 
            new Byte4 { PackedValue = reader.ReadUInt32() };

        private static MyAnimationClip ReadClip(BinaryReader reader)
        {
            MyAnimationClip clip = new MyAnimationClip {
                Name = reader.ReadString(),
                Duration = reader.ReadDouble()
            };
            int num = reader.ReadInt32();
            while (true)
            {
                num--;
                if (num <= 0)
                {
                    return clip;
                }
                MyAnimationClip.Bone item = new MyAnimationClip.Bone {
                    Name = reader.ReadString()
                };
                int num2 = reader.ReadInt32();
                while (true)
                {
                    num2--;
                    if (num2 <= 0)
                    {
                        clip.Bones.Add(item);
                        int count = item.Keyframes.Count;
                        int num4 = 0;
                        if (count > 3)
                        {
                            if (USE_LINEAR_KEYFRAME_REDUCTION)
                            {
                                LinkedList<MyAnimationClip.Keyframe> keyframes = new LinkedList<MyAnimationClip.Keyframe>();
                                foreach (MyAnimationClip.Keyframe keyframe2 in item.Keyframes)
                                {
                                    keyframes.AddLast(keyframe2);
                                }
                                LinearKeyframeReduction(keyframes, 1E-08f, 0.9999999f);
                                item.Keyframes.Clear();
                                item.Keyframes.AddArray<MyAnimationClip.Keyframe>(keyframes.ToArray<MyAnimationClip.Keyframe>());
                                num4 = item.Keyframes.Count;
                            }
                            if (LINEAR_KEYFRAME_REDUCTION_STATS)
                            {
                                List<ReductionInfo> list2;
                                ReductionInfo info = new ReductionInfo {
                                    BoneName = item.Name,
                                    OriginalKeys = count,
                                    OptimizedKeys = num4
                                };
                                if (!ReductionStats.TryGetValue(m_debugAssetName, out list2))
                                {
                                    list2 = new List<ReductionInfo>();
                                    ReductionStats.Add(m_debugAssetName, list2);
                                }
                                list2.Add(info);
                            }
                        }
                        CalculateKeyframeDeltas(item.Keyframes);
                        break;
                    }
                    MyAnimationClip.Keyframe keyframe = new MyAnimationClip.Keyframe {
                        Time = reader.ReadDouble(),
                        Rotation = ImportQuaternion(reader),
                        Translation = ImportVector3(reader)
                    };
                    item.Keyframes.Add(keyframe);
                }
            }
        }

        private static Dictionary<string, MyModelDummy> ReadDummies(BinaryReader reader)
        {
            Dictionary<string, MyModelDummy> dictionary = new Dictionary<string, MyModelDummy>();
            int num = reader.ReadInt32();
            int num2 = 0;
            while (num2 < num)
            {
                string key = reader.ReadString();
                Matrix matrix = ReadMatrix(reader);
                MyModelDummy dummy = new MyModelDummy {
                    Name = key,
                    Matrix = matrix,
                    CustomData = new Dictionary<string, object>()
                };
                int num3 = reader.ReadInt32();
                int num4 = 0;
                while (true)
                {
                    if (num4 >= num3)
                    {
                        dictionary.Add(key, dummy);
                        num2++;
                        break;
                    }
                    string str2 = reader.ReadString();
                    string str3 = reader.ReadString();
                    dummy.CustomData.Add(str2, str3);
                    num4++;
                }
            }
            return dictionary;
        }

        private static HalfVector2 ReadHalfVector2(BinaryReader reader) => 
            new HalfVector2 { PackedValue = reader.ReadUInt32() };

        private static HalfVector4 ReadHalfVector4(BinaryReader reader) => 
            new HalfVector4 { PackedValue = reader.ReadUInt64() };

        private static Md5.Hash ReadHash(BinaryReader reader)
        {
            Md5.Hash hash1 = new Md5.Hash();
            hash1.A = reader.ReadUInt32();
            hash1.B = reader.ReadUInt32();
            hash1.C = reader.ReadUInt32();
            hash1.D = reader.ReadUInt32();
            return hash1;
        }

        private Dictionary<string, int> ReadIndexDictionary(BinaryReader reader)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                string key = reader.ReadString();
                int num3 = reader.ReadInt32();
                dictionary.Add(key, num3);
            }
            return dictionary;
        }

        private static MyLODDescriptor[] ReadLODs(BinaryReader reader, int version)
        {
            int num = reader.ReadInt32();
            MyLODDescriptor[] descriptorArray = new MyLODDescriptor[num];
            int index = 0;
            while (true)
            {
                num--;
                if (num <= 0)
                {
                    return descriptorArray;
                }
                MyLODDescriptor descriptor = new MyLODDescriptor();
                index++;
                descriptorArray[index] = descriptor;
                descriptor.Read(reader);
            }
        }

        private static Matrix ReadMatrix(BinaryReader reader)
        {
            Matrix matrix;
            matrix.M11 = reader.ReadSingle();
            matrix.M12 = reader.ReadSingle();
            matrix.M13 = reader.ReadSingle();
            matrix.M14 = reader.ReadSingle();
            matrix.M21 = reader.ReadSingle();
            matrix.M22 = reader.ReadSingle();
            matrix.M23 = reader.ReadSingle();
            matrix.M24 = reader.ReadSingle();
            matrix.M31 = reader.ReadSingle();
            matrix.M32 = reader.ReadSingle();
            matrix.M33 = reader.ReadSingle();
            matrix.M34 = reader.ReadSingle();
            matrix.M41 = reader.ReadSingle();
            matrix.M42 = reader.ReadSingle();
            matrix.M43 = reader.ReadSingle();
            matrix.M44 = reader.ReadSingle();
            return matrix;
        }

        private static List<MyMeshPartInfo> ReadMeshParts(BinaryReader reader, int version)
        {
            List<MyMeshPartInfo> list = new List<MyMeshPartInfo>();
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                MyMeshPartInfo item = new MyMeshPartInfo();
                item.Import(reader, version);
                list.Add(item);
            }
            return list;
        }

        private static List<MyMeshSectionInfo> ReadMeshSections(BinaryReader reader, int version)
        {
            List<MyMeshSectionInfo> list = new List<MyMeshSectionInfo>();
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                MyMeshSectionInfo item = new MyMeshSectionInfo();
                item.Import(reader, version);
                list.Add(item);
            }
            return list;
        }

        private static MyModelFractures ReadModelFractures(BinaryReader reader)
        {
            MyModelFractures fractures = new MyModelFractures {
                Version = reader.ReadInt32()
            };
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                string str = reader.ReadString();
                if (str == "RandomSplit")
                {
                    RandomSplitFractureSettings settings = new RandomSplitFractureSettings {
                        NumObjectsOnLevel1 = reader.ReadInt32(),
                        NumObjectsOnLevel2 = reader.ReadInt32(),
                        RandomRange = reader.ReadInt32(),
                        RandomSeed1 = reader.ReadInt32(),
                        RandomSeed2 = reader.ReadInt32(),
                        SplitPlane = reader.ReadString()
                    };
                    fractures.Fractures = new MyFractureSettings[] { settings };
                }
                else if (str == "Voronoi")
                {
                    VoronoiFractureSettings settings2 = new VoronoiFractureSettings {
                        Seed = reader.ReadInt32(),
                        NumSitesToGenerate = reader.ReadInt32(),
                        NumIterations = reader.ReadInt32(),
                        SplitPlane = reader.ReadString()
                    };
                    fractures.Fractures = new MyFractureSettings[] { settings2 };
                }
                else if (str == "WoodFracture")
                {
                    WoodFractureSettings settings3 = new WoodFractureSettings {
                        BoardCustomSplittingPlaneAxis = reader.ReadBoolean(),
                        BoardFractureLineShearingRange = reader.ReadSingle(),
                        BoardFractureNormalShearingRange = reader.ReadSingle(),
                        BoardNumSubparts = reader.ReadInt32(),
                        BoardRotateSplitGeom = (WoodFractureSettings.Rotation) reader.ReadInt32(),
                        BoardScale = ReadVector3(reader),
                        BoardScaleRange = ReadVector3(reader),
                        BoardSplitGeomShiftRangeY = reader.ReadSingle(),
                        BoardSplitGeomShiftRangeZ = reader.ReadSingle(),
                        BoardSplittingAxis = ReadVector3(reader),
                        BoardSplittingPlane = reader.ReadString(),
                        BoardSurfaceNormalShearingRange = reader.ReadSingle(),
                        BoardWidthRange = reader.ReadSingle(),
                        SplinterCustomSplittingPlaneAxis = reader.ReadBoolean(),
                        SplinterFractureLineShearingRange = reader.ReadSingle(),
                        SplinterFractureNormalShearingRange = reader.ReadSingle(),
                        SplinterNumSubparts = reader.ReadInt32(),
                        SplinterRotateSplitGeom = (WoodFractureSettings.Rotation) reader.ReadInt32(),
                        SplinterScale = ReadVector3(reader),
                        SplinterScaleRange = ReadVector3(reader),
                        SplinterSplitGeomShiftRangeY = reader.ReadSingle(),
                        SplinterSplitGeomShiftRangeZ = reader.ReadSingle(),
                        SplinterSplittingAxis = ReadVector3(reader),
                        SplinterSplittingPlane = reader.ReadString(),
                        SplinterSurfaceNormalShearingRange = reader.ReadSingle(),
                        SplinterWidthRange = reader.ReadSingle()
                    };
                    fractures.Fractures = new MyFractureSettings[] { settings3 };
                }
            }
            return fractures;
        }

        private static Vector3 ReadVector3(BinaryReader reader) => 
            new Vector3 { 
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };

        public int DataVersion =>
            this.m_version;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyModelImporter.<>c <>9 = new MyModelImporter.<>c();

            internal bool <.cctor>b__57_0(BinaryReader x) => 
                x.ReadBoolean();

            internal float <.cctor>b__57_1(BinaryReader x) => 
                x.ReadSingle();

            internal bool <.cctor>b__57_2(BinaryReader x) => 
                x.ReadBoolean();

            internal GImpactQuantizedBvh <.cctor>b__57_3(BinaryReader reader)
            {
                GImpactQuantizedBvh bvh1 = new GImpactQuantizedBvh();
                bvh1.Load(MyModelImporter.ReadArrayOfBytes(reader));
                return bvh1;
            }

            internal MyModelInfo <.cctor>b__57_4(BinaryReader reader) => 
                new MyModelInfo(reader.ReadInt32(), reader.ReadInt32(), MyModelImporter.ImportVector3(reader));

            internal float <.cctor>b__57_5(BinaryReader x) => 
                x.ReadSingle();
        }

        private interface ITagReader
        {
            object Read(BinaryReader reader, int version);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ReductionInfo
        {
            public string BoneName;
            public int OriginalKeys;
            public int OptimizedKeys;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TagReader<T> : MyModelImporter.ITagReader
        {
            private Func<BinaryReader, int, T> m_tagReader;
            public TagReader(Func<BinaryReader, T> tagReader)
            {
                this.m_tagReader = (x, y) => tagReader(x);
            }

            public TagReader(Func<BinaryReader, int, T> tagReader)
            {
                this.m_tagReader = tagReader;
            }

            private T ReadTag(BinaryReader reader, int version) => 
                this.m_tagReader(reader, version);

            public object Read(BinaryReader reader, int version) => 
                this.ReadTag(reader, version);
        }
    }
}

