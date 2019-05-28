namespace Sandbox.Engine.Utils
{
    using Sandbox.Graphics;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Utils;
    using VRageMath;

    internal class MyTextureAtlasUtils
    {
        private static MyTextureAtlas LoadTextureAtlas(string textureDir, string atlasFile)
        {
            MyTextureAtlas atlas = new MyTextureAtlas(0x40);
            using (Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.ContentPath, atlasFile)))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string str = reader.ReadLine();
                        if (!str.StartsWith("#"))
                        {
                            char[] trimChars = new char[] { ' ' };
                            if (str.Trim(trimChars).Length != 0)
                            {
                                string[] strArray = str.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                string str3 = strArray[1];
                                Vector4 uvOffsets = new Vector4(Convert.ToSingle(strArray[4], CultureInfo.InvariantCulture), Convert.ToSingle(strArray[5], CultureInfo.InvariantCulture), Convert.ToSingle(strArray[7], CultureInfo.InvariantCulture), Convert.ToSingle(strArray[8], CultureInfo.InvariantCulture));
                                MyTextureAtlasItem item = new MyTextureAtlasItem(textureDir + str3, uvOffsets);
                                atlas.Add(strArray[0], item);
                            }
                        }
                    }
                }
            }
            return atlas;
        }

        public static void LoadTextureAtlas(string[] enumsToStrings, string textureDir, string atlasFile, out string texture, out MyAtlasTextureCoordinate[] textureCoords)
        {
            MyTextureAtlas atlas = LoadTextureAtlas(textureDir, atlasFile);
            textureCoords = new MyAtlasTextureCoordinate[enumsToStrings.Length];
            texture = null;
            for (int i = 0; i < enumsToStrings.Length; i++)
            {
                MyTextureAtlasItem item = atlas[enumsToStrings[i]];
                textureCoords[i] = new MyAtlasTextureCoordinate(new Vector2(item.UVOffsets.X, item.UVOffsets.Y), new Vector2(item.UVOffsets.Z, item.UVOffsets.W));
                if (texture == null)
                {
                    texture = item.AtlasTexture;
                }
            }
        }
    }
}

