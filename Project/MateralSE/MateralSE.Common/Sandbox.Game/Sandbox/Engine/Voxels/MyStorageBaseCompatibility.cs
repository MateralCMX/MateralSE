namespace Sandbox.Engine.Voxels
{
    using Sandbox;
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using VRage.Game;
    using VRage.Voxels;
    using VRageMath;

    internal class MyStorageBaseCompatibility
    {
        private const int MAX_ENCODED_NAME_LENGTH = 0x100;
        private readonly byte[] m_encodedNameBuffer = new byte[0x100];

        public unsafe MyStorageBase Compatibility_LoadCellStorage(int fileVersion, Stream stream)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I vectori6;
            vectori.X = stream.ReadInt32();
            vectori.Y = stream.ReadInt32();
            vectori.Z = stream.ReadInt32();
            MyOctreeStorage storage = new MyOctreeStorage(null, vectori);
            vectori2.X = stream.ReadInt32();
            vectori2.Y = stream.ReadInt32();
            vectori2.Z = stream.ReadInt32();
            Vector3I vectori3 = (Vector3I) (vectori / vectori2);
            Dictionary<byte, MyVoxelMaterialDefinition> mapping = null;
            if (fileVersion == 2)
            {
                mapping = this.Compatibility_LoadMaterialIndexMapping(stream);
            }
            else
            {
                int num2 = fileVersion;
            }
            Vector3I zero = Vector3I.Zero;
            Vector3I end = new Vector3I(7);
            MyStorageData source = new MyStorageData(MyStorageDataTypeFlags.All);
            source.Resize(Vector3I.Zero, end);
            vectori6.X = 0;
            while (vectori6.X < vectori3.X)
            {
                vectori6.Y = 0;
                while (true)
                {
                    if (vectori6.Y >= vectori3.Y)
                    {
                        int* numPtr6 = (int*) ref vectori6.X;
                        numPtr6[0]++;
                        break;
                    }
                    vectori6.Z = 0;
                    while (true)
                    {
                        if (vectori6.Z >= vectori3.Z)
                        {
                            int* numPtr5 = (int*) ref vectori6.Y;
                            numPtr5[0]++;
                            break;
                        }
                        MyVoxelContentConstitution constitution = (MyVoxelContentConstitution) stream.ReadByteNoAlloc();
                        switch (constitution)
                        {
                            case MyVoxelContentConstitution.Empty:
                                source.ClearContent(0);
                                break;

                            case MyVoxelContentConstitution.Full:
                                source.ClearContent(0xff);
                                break;

                            case MyVoxelContentConstitution.Mixed:
                                Vector3I vectori7;
                                vectori7.X = 0;
                                while (vectori7.X < 8)
                                {
                                    vectori7.Y = 0;
                                    while (true)
                                    {
                                        if (vectori7.Y >= 8)
                                        {
                                            int* numPtr3 = (int*) ref vectori7.X;
                                            numPtr3[0]++;
                                            break;
                                        }
                                        vectori7.Z = 0;
                                        while (true)
                                        {
                                            if (vectori7.Z >= 8)
                                            {
                                                int* numPtr2 = (int*) ref vectori7.Y;
                                                numPtr2[0]++;
                                                break;
                                            }
                                            source.Content(ref vectori7, stream.ReadByteNoAlloc());
                                            int* numPtr1 = (int*) ref vectori7.Z;
                                            numPtr1[0]++;
                                        }
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                        zero = vectori6 * 8;
                        storage.WriteRange(source, MyStorageDataTypeFlags.Content, zero, (Vector3I) (zero + 7), true, false);
                        int* numPtr4 = (int*) ref vectori6.Z;
                        numPtr4[0]++;
                    }
                }
            }
            try
            {
                vectori6.X = 0;
                while (vectori6.X < vectori3.X)
                {
                    vectori6.Y = 0;
                    while (true)
                    {
                        if (vectori6.Y >= vectori3.Y)
                        {
                            int* numPtr12 = (int*) ref vectori6.X;
                            numPtr12[0]++;
                            break;
                        }
                        vectori6.Z = 0;
                        while (true)
                        {
                            if (vectori6.Z >= vectori3.Z)
                            {
                                int* numPtr11 = (int*) ref vectori6.Y;
                                numPtr11[0]++;
                                break;
                            }
                            MyVoxelMaterialDefinition definition = null;
                            if (stream.ReadByteNoAlloc() == 1)
                            {
                                source.ClearMaterials(this.Compatibility_LoadCellVoxelMaterial(stream, mapping).Index);
                            }
                            else
                            {
                                Vector3I vectori8;
                                vectori8.X = 0;
                                while (vectori8.X < 8)
                                {
                                    vectori8.Y = 0;
                                    while (true)
                                    {
                                        if (vectori8.Y >= 8)
                                        {
                                            int* numPtr9 = (int*) ref vectori8.X;
                                            numPtr9[0]++;
                                            break;
                                        }
                                        vectori8.Z = 0;
                                        while (true)
                                        {
                                            if (vectori8.Z >= 8)
                                            {
                                                int* numPtr8 = (int*) ref vectori8.Y;
                                                numPtr8[0]++;
                                                break;
                                            }
                                            definition = this.Compatibility_LoadCellVoxelMaterial(stream, mapping);
                                            stream.ReadByteNoAlloc();
                                            source.Material(ref vectori8, definition.Index);
                                            int* numPtr7 = (int*) ref vectori8.Z;
                                            numPtr7[0]++;
                                        }
                                    }
                                }
                            }
                            zero = vectori6 * 8;
                            storage.WriteRange(source, MyStorageDataTypeFlags.Material, zero, (Vector3I) (zero + 7), true, false);
                            int* numPtr10 = (int*) ref vectori6.Z;
                            numPtr10[0]++;
                        }
                    }
                }
            }
            catch (EndOfStreamException exception)
            {
                MySandboxGame.Log.WriteLine(exception);
            }
            return storage;
        }

        private MyVoxelMaterialDefinition Compatibility_LoadCellVoxelMaterial(Stream stream, Dictionary<byte, MyVoxelMaterialDefinition> mapping)
        {
            MyVoxelMaterialDefinition voxelMaterialDefinition = null;
            byte materialIndex = stream.ReadByteNoAlloc();
            if (materialIndex != 0xff)
            {
                voxelMaterialDefinition = (mapping == null) ? MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIndex) : mapping[materialIndex];
            }
            else
            {
                byte count = stream.ReadByteNoAlloc();
                stream.Read(this.m_encodedNameBuffer, 0, count);
                string name = Encoding.UTF8.GetString(this.m_encodedNameBuffer, 0, count);
                voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(name);
            }
            if (voxelMaterialDefinition == null)
            {
                voxelMaterialDefinition = MyDefinitionManager.Static.GetDefaultVoxelMaterialDefinition();
            }
            return voxelMaterialDefinition;
        }

        private Dictionary<byte, MyVoxelMaterialDefinition> Compatibility_LoadMaterialIndexMapping(Stream stream)
        {
            int capacity = stream.Read7BitEncodedInt();
            Dictionary<byte, MyVoxelMaterialDefinition> dictionary = new Dictionary<byte, MyVoxelMaterialDefinition>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                MyVoxelMaterialDefinition defaultVoxelMaterialDefinition;
                byte key = stream.ReadByteNoAlloc();
                string name = stream.ReadString(null);
                if (!MyDefinitionManager.Static.TryGetVoxelMaterialDefinition(name, out defaultVoxelMaterialDefinition))
                {
                    defaultVoxelMaterialDefinition = MyDefinitionManager.Static.GetDefaultVoxelMaterialDefinition();
                }
                dictionary.Add(key, defaultVoxelMaterialDefinition);
            }
            return dictionary;
        }
    }
}

