namespace Sandbox.Engine.Voxels.Planet
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using SharpDX;
    using SharpDX.DXGI;
    using SharpDX.Toolkit.Graphics;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.Compression;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ObjectBuilders;
    using VRage.Utils;

    [MyPlanetMapProvider(typeof(MyObjectBuilder_PlanetTextureMapProvider), true)]
    public class MyPlanetTextureMapProvider : MyPlanetMapProviderBase
    {
        public static string PlanetDataFilesPath = "Data/PlanetDataFiles";
        private string m_path;
        private MyModContext m_mod;

        private void ClearMatValues(MyCubemapData<byte>[] maps)
        {
            for (int i = 0; i < 6; i++)
            {
                maps[i * 4] = null;
                maps[(i * 4) + 1] = null;
                maps[(i * 4) + 2] = null;
            }
        }

        public MyHeightDetailTexture GetDetailMap(string path)
        {
            MyHeightDetailTexture texture;
            string str = Path.Combine(MyFileSystem.ContentPath, path);
            str = !MyFileSystem.FileExists(str + ".png") ? (str + ".dds") : (str + ".png");
            using (Image image = this.LoadTexture(str))
            {
                if (image != null)
                {
                    PixelBuffer buffer = image.GetPixelBuffer(0, 0, 0);
                    if (buffer.Format == Format.R8_UNorm)
                    {
                        texture = new MyHeightDetailTexture(buffer.GetPixels<byte>(0), (uint) buffer.Height);
                        image.Dispose();
                    }
                    else
                    {
                        string msg = $"Detail map '{str}' could not be loaded, expected format R8_UNorm, got {buffer.Format} instead.";
                        MyLog.Default.WriteLine(msg);
                        return null;
                    }
                }
                else
                {
                    texture = new MyHeightDetailTexture(new byte[1], 1);
                }
            }
            return texture;
        }

        public override MyHeightCubemap GetHeightmap()
        {
            MyHeightCubemap heightMap;
            MyHeightMapLoadingSystem component = MySession.Static.GetComponent<MyHeightMapLoadingSystem>();
            if (component == null)
            {
                return this.GetHeightMap(this.m_path, this.m_mod);
            }
            if (!component.TryGet(this.m_path, out heightMap))
            {
                heightMap = this.GetHeightMap(this.m_path, this.m_mod);
                component.Cache(this.m_path, ref heightMap);
            }
            return heightMap;
        }

        public MyHeightCubemap GetHeightMap(string folderName, MyModContext context)
        {
            string modId = context.ModId;
            string name = $"{modId ?? "BaseGame"}:{folderName}";
            bool flag = false;
            MyHeightmapFace[] faces = new MyHeightmapFace[6];
            int resolution = 0;
            for (int i = 0; i < 6; i++)
            {
                faces[i] = this.GetHeightMap(folderName, MyCubemapHelpers.GetNameForFace(i), context);
                if (faces[i] == null)
                {
                    flag = true;
                }
                else if ((faces[i].Resolution == resolution) || (resolution == 0))
                {
                    resolution = faces[i].Resolution;
                }
                else
                {
                    flag = true;
                    MyLog.Default.Error("Cubemap faces must be all the same size!", Array.Empty<object>());
                }
                if (flag)
                {
                    break;
                }
            }
            if (flag)
            {
                MyLog.Default.WriteLine($"Error loading heightmap {folderName}, using fallback instead. See rest of log for details.");
                for (int j = 0; j < 6; j++)
                {
                    faces[j] = MyHeightmapFace.Default;
                    resolution = faces[j].Resolution;
                }
            }
            return new MyHeightCubemap(name, faces, resolution);
        }

        private MyHeightmapFace GetHeightMap(string folderName, string faceName, MyModContext context)
        {
            string path = this.GetPath(folderName, faceName, context);
            MyHeightmapFace map = null;
            try
            {
                using (Image image = this.LoadTexture(path))
                {
                    if (image == null)
                    {
                        object[] args = new object[] { path };
                        MyLog.Default.Error("Could not load texture {0}, no suitable format found. ", args);
                    }
                    else
                    {
                        PixelBuffer imageData = image.GetPixelBuffer(0, 0, 0);
                        map = new MyHeightmapFace(imageData.Height);
                        if (imageData.Format == Format.R16_UNorm)
                        {
                            PrepareHeightMap(map, imageData);
                        }
                        else if (imageData.Format == Format.R8_UNorm)
                        {
                            PrepareHeightMap8Bit(map, imageData);
                        }
                        else
                        {
                            MyLog.Default.Error($"Heighmap texture {path}: Invalid format {imageData.Format} (expecting R16_UNorm or R8_UNorm).", Array.Empty<object>());
                        }
                        image.Dispose();
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception.Message);
            }
            return map;
        }

        public override MyCubemap[] GetMaps(MyPlanetMapTypeSet types)
        {
            MyCubemap[] cubemapArray;
            MyHeightMapLoadingSystem component = MySession.Static.GetComponent<MyHeightMapLoadingSystem>();
            if (component == null)
            {
                this.GetPlanetMaps(this.m_path, this.m_mod, types, out cubemapArray);
            }
            else if (!component.TryGet(this.m_path, out cubemapArray))
            {
                this.GetPlanetMaps(this.m_path, this.m_mod, types, out cubemapArray);
                component.Cache(this.m_path, ref cubemapArray);
            }
            return cubemapArray;
        }

        private string GetPath(string folder, string name, MyModContext context)
        {
            string str;
            if (!context.IsBaseGame)
            {
                str = Path.Combine(Path.Combine(context.ModPath, PlanetDataFilesPath), folder, name);
                if (!MyFileSystem.FileExists(str + ".png"))
                {
                    if (MyFileSystem.FileExists(str + ".dds"))
                    {
                        return (str + ".dds");
                    }
                }
                else
                {
                    return (str + ".png");
                }
            }
            str = Path.Combine(MyFileSystem.ContentPath, PlanetDataFilesPath, folder, name);
            if (MyFileSystem.FileExists(str + ".png"))
            {
                str = str + ".png";
            }
            else if (MyFileSystem.FileExists(str + ".dds"))
            {
                str = str + ".dds";
            }
            return str;
        }

        public void GetPlanetMaps(string folder, MyModContext context, MyPlanetMapTypeSet mapsToUse, out MyCubemap[] maps)
        {
            int num;
            int num2;
            maps = new MyCubemap[4];
            MyCubemapData<byte>[] dataArray = new MyCubemapData<byte>[0x18];
            byte[][] streams = new byte[4][];
            if (mapsToUse == 0)
            {
                goto TR_000A;
            }
            else
            {
                num = 0;
            }
            goto TR_0026;
        TR_000A:
            num2 = 0;
            while (num2 < 4)
            {
                if (dataArray[num2] != null)
                {
                    MyCubemapData<byte>[] faces = new MyCubemapData<byte>[6];
                    int index = 0;
                    while (true)
                    {
                        if (index >= 6)
                        {
                            maps[num2] = new MyCubemap(faces);
                            break;
                        }
                        faces[index] = dataArray[num2 + (index * 4)];
                        index++;
                    }
                }
                num2++;
            }
            return;
        TR_000D:
            num++;
        TR_0026:
            while (true)
            {
                if (num >= 6)
                {
                    break;
                }
                string name = Path.Combine(folder, MyCubemapHelpers.GetNameForFace(num));
                try
                {
                    string str;
                    Image image = this.TryGetPlanetTexture(name, context, "_mat", out str);
                    if (image == null)
                    {
                        this.ClearMatValues(dataArray);
                        break;
                    }
                    using (image)
                    {
                        PixelBuffer buffer = image.GetPixelBuffer(0, 0, 0);
                        if ((buffer.Format != Format.B8G8R8A8_UNorm) && (buffer.Format != Format.R8G8B8A8_UNorm))
                        {
                            object[] args = new object[] { buffer.Format, str };
                            MyLog.Default.Error("While loading maps from {1}: Unsupported planet map format: {0}.", args);
                            break;
                        }
                        if (buffer.Width == buffer.Height)
                        {
                            if (mapsToUse.HasFlag(MyPlanetMapTypeSet.Material))
                            {
                                dataArray[num * 4] = new MyCubemapData<byte>(buffer.Width, null);
                                streams[0] = dataArray[num * 4].Data;
                            }
                            if (mapsToUse.HasFlag(MyPlanetMapTypeSet.Biome))
                            {
                                dataArray[(num * 4) + 1] = new MyCubemapData<byte>(buffer.Width, null);
                                streams[1] = dataArray[(num * 4) + 1].Data;
                            }
                            if (mapsToUse.HasFlag(MyPlanetMapTypeSet.Ore))
                            {
                                dataArray[(num * 4) + 2] = new MyCubemapData<byte>(buffer.Width, null);
                                streams[2] = dataArray[(num * 4) + 2].Data;
                            }
                            if (buffer.Format == Format.B8G8R8A8_UNorm)
                            {
                                streams[2] = streams[0];
                                streams[0] = streams[2];
                            }
                            this.ReadChannelsFromImage(streams, buffer);
                            image.Dispose();
                            goto TR_000D;
                        }
                        else
                        {
                            object[] args = new object[] { str };
                            MyLog.Default.Error("While loading maps from {0}: Width and height must be the same.", args);
                        }
                        break;
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.Error(exception.ToString(), Array.Empty<object>());
                    break;
                }
                goto TR_000D;
            }
            goto TR_000A;
        }

        public override void Init(long seed, MyPlanetGeneratorDefinition generator, MyObjectBuilder_PlanetMapProvider builder)
        {
            MyObjectBuilder_PlanetTextureMapProvider provider = (MyObjectBuilder_PlanetTextureMapProvider) builder;
            this.m_path = provider.Path;
            this.m_mod = generator.Context;
        }

        private Image LoadTexture(string path)
        {
            if (!MyFileSystem.FileExists(path))
            {
                return null;
            }
            using (Stream stream = MyFileSystem.OpenRead(path))
            {
                return ((stream != null) ? Image.Load(stream) : null);
            }
        }

        private static void PrepareHeightMap(MyHeightmapFace map, PixelBuffer imageData)
        {
            IntPtr dataPointer = imageData.DataPointer;
            int rowStride = imageData.RowStride;
            for (int i = 0; i < map.Resolution; i++)
            {
                Utilities.Read<ushort>(dataPointer, map.Data, map.GetRowStart(i), imageData.Width);
                dataPointer += rowStride;
            }
        }

        private static void PrepareHeightMap8Bit(MyHeightmapFace map, PixelBuffer imageData)
        {
            int y = 0;
            while (y < map.Resolution)
            {
                int x = 0;
                while (true)
                {
                    if (x >= map.Resolution)
                    {
                        y++;
                        break;
                    }
                    map.SetValue(x, y, (ushort) (imageData.GetPixel<byte>(x, y) * 0x100));
                    x++;
                }
            }
        }

        private unsafe void ReadChannelsFromImage(byte[][] streams, PixelBuffer buffer)
        {
            byte* numPtr = (byte*) buffer.DataPointer.ToPointer();
            int width = buffer.Width;
            for (int i = 0; i < 4; i++)
            {
                if (streams[i] != null)
                {
                    int num3 = 0;
                    int index = width + 3;
                    int num5 = 0;
                    while (num5 < width)
                    {
                        int num6 = 0;
                        while (true)
                        {
                            if (num6 >= width)
                            {
                                index += 2;
                                num5++;
                                break;
                            }
                            streams[i][index] = numPtr[(num3 * 4) + i];
                            num3++;
                            index++;
                            num6++;
                        }
                    }
                }
            }
        }

        private Image TryGetPlanetTexture(string name, MyModContext context, string p, out string fullPath)
        {
            bool flag = false;
            string text1 = name + p;
            name = text1;
            fullPath = Path.Combine(context.ModPathData, "PlanetDataFiles", name) + ".png";
            if (!context.IsBaseGame)
            {
                if (MyFileSystem.FileExists(fullPath))
                {
                    flag = true;
                }
                else
                {
                    fullPath = Path.Combine(context.ModPathData, "PlanetDataFiles", name) + ".dds";
                    if (MyFileSystem.FileExists(fullPath))
                    {
                        flag = true;
                    }
                }
            }
            if (!flag)
            {
                string str = Path.Combine(MyFileSystem.ContentPath, PlanetDataFilesPath);
                fullPath = Path.Combine(str, name) + ".png";
                if (!MyFileSystem.FileExists(fullPath))
                {
                    fullPath = Path.Combine(str, name) + ".dds";
                    if (!MyFileSystem.FileExists(fullPath))
                    {
                        return null;
                    }
                }
            }
            if (fullPath.Contains(MyWorkshop.WorkshopModSuffix))
            {
                string path = fullPath.Substring(0, fullPath.IndexOf(MyWorkshop.WorkshopModSuffix) + MyWorkshop.WorkshopModSuffix.Length);
                string str3 = fullPath.Replace(path + @"\", "");
                MyZipArchive archive = MyZipArchive.OpenOnFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, false);
                try
                {
                    return Image.Load(archive.GetFile(str3).GetStream(FileMode.Open, FileAccess.Read));
                }
                catch (Exception)
                {
                    MyLog.Default.Error("Failed to load existing " + p + " file mod archive. " + fullPath, Array.Empty<object>());
                    return null;
                }
                finally
                {
                    if (archive != null)
                    {
                        archive.Dispose();
                    }
                }
            }
            return Image.Load(fullPath);
        }
    }
}

