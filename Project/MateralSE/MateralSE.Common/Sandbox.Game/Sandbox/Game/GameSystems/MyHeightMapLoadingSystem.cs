namespace Sandbox.Game.GameSystems
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Engine.Voxels.Planet;
    using SharpDX.DXGI;
    using SharpDX.Toolkit.Graphics;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyHeightMapLoadingSystem : MySessionComponentBase
    {
        private ConcurrentDictionary<string, MyHeightCubemap> m_heightMaps;
        private ConcurrentDictionary<string, MyCubemap[]> m_planetMaps;
        private ConcurrentDictionary<string, MyTileTexture<byte>> m_ditherTilesets;
        private ConcurrentDictionary<string, MyHeightDetailTexture> m_detailTextures;
        public static MyHeightMapLoadingSystem Static;

        public void Cache(string path, ref MyTileTexture<byte> tilemap)
        {
            tilemap = this.m_ditherTilesets.GetOrAdd(path, tilemap);
        }

        public void Cache(string path, ref MyHeightCubemap heightmap)
        {
            MyHeightCubemap orAdd = this.m_heightMaps.GetOrAdd(path, heightmap);
            if (orAdd != heightmap)
            {
                heightmap.Dispose();
                heightmap = orAdd;
            }
        }

        public void Cache(string path, ref MyCubemap[] materialMaps)
        {
            materialMaps = this.m_planetMaps.GetOrAdd(path, materialMaps);
        }

        public MyTileTexture<byte> GetTerrainBlendTexture(MyPlanetMaterialBlendSettings settings)
        {
            MyTileTexture<byte> texture;
            string path = settings.Texture;
            int cellSize = settings.CellSize;
            if (!this.TryGet(path, out texture))
            {
                string str2 = Path.Combine(MyFileSystem.ContentPath, path) + ".png";
                if (!File.Exists(str2))
                {
                    str2 = Path.Combine(MyFileSystem.ContentPath, path) + ".dds";
                }
                Image image = null;
                try
                {
                    image = Image.Load(str2);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception.Message);
                }
                if (image == null)
                {
                    return MyTileTexture<byte>.Default;
                }
                PixelBuffer buffer = image.GetPixelBuffer(0, 0, 0);
                if (buffer.Format != Format.R8_UNorm)
                {
                    return MyTileTexture<byte>.Default;
                }
                texture = new MyTileTexture<byte>(buffer, cellSize);
                image.Dispose();
                this.Cache(path, ref texture);
            }
            return texture;
        }

        public override void LoadData()
        {
            base.LoadData();
            this.m_heightMaps = new ConcurrentDictionary<string, MyHeightCubemap>();
            this.m_planetMaps = new ConcurrentDictionary<string, MyCubemap[]>();
            this.m_ditherTilesets = new ConcurrentDictionary<string, MyTileTexture<byte>>();
            this.m_detailTextures = new ConcurrentDictionary<string, MyHeightDetailTexture>();
            Static = this;
        }

        public bool TryGet(string path, out MyTileTexture<byte> tilemap) => 
            this.m_ditherTilesets.TryGetValue(path, out tilemap);

        public bool TryGet(string path, out MyHeightCubemap heightmap) => 
            this.m_heightMaps.TryGetValue(path, out heightmap);

        public bool TryGet(string path, out MyCubemap[] materialMaps) => 
            this.m_planetMaps.TryGetValue(path, out materialMaps);

        protected override void UnloadData()
        {
            base.UnloadData();
            using (IEnumerator<MyHeightCubemap> enumerator = this.m_heightMaps.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Dispose();
                }
            }
            this.m_heightMaps.Clear();
            this.m_heightMaps = null;
            this.m_planetMaps.Clear();
            this.m_planetMaps = null;
            this.m_ditherTilesets.Clear();
            this.m_ditherTilesets = null;
            this.m_detailTextures.Clear();
            this.m_detailTextures = null;
        }
    }
}

