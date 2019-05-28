namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Graphics;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI;
    using VRageMath;

    [MyTextSurfaceScript("TSS_EnergyHydrogen", "DisplayName_TSS_EnergyHydrogen")]
    public class MyTSSEnergyHydrogen : MyTSSCommon
    {
        public static float ASPECT_RATIO = 3f;
        public static float DECORATION_RATIO = 0.25f;
        public static float TEXT_RATIO = 0.25f;
        public static string ENERGY_ICON = "IconEnergy";
        public static string HYDROGEN_ICON = "IconHydrogen";
        private Vector2 m_innerSize;
        private Vector2 m_decorationSize;
        private float m_firstLine;
        private float m_secondLine;
        private StringBuilder m_sb;
        private MyResourceDistributorComponent m_resourceDistributor;
        private MyCubeGrid m_grid;
        private float m_maxHydrogen;
        private List<Sandbox.Game.Entities.Interfaces.IMyGasTank> m_tankBlocks;

        public MyTSSEnergyHydrogen(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.m_sb = new StringBuilder();
            this.m_tankBlocks = new List<Sandbox.Game.Entities.Interfaces.IMyGasTank>();
            this.m_innerSize = new Vector2(ASPECT_RATIO, 1f);
            FitRect(size, ref this.m_innerSize);
            this.m_decorationSize = new Vector2(0.012f * this.m_innerSize.X, DECORATION_RATIO * this.m_innerSize.Y);
            this.m_sb.Clear();
            this.m_sb.Append("Power Usage: 00.000");
            Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, 1f);
            float num = (TEXT_RATIO * this.m_innerSize.Y) / vector.Y;
            base.m_fontScale = Math.Min((this.m_innerSize.X * 0.72f) / vector.X, num);
            this.m_firstLine = base.m_halfSize.Y - (this.m_decorationSize.Y * 0.55f);
            this.m_secondLine = base.m_halfSize.Y + (this.m_decorationSize.Y * 0.55f);
            if (base.m_block != null)
            {
                this.m_grid = base.m_block.CubeGrid as MyCubeGrid;
                if (this.m_grid != null)
                {
                    this.m_resourceDistributor = this.m_grid.GridSystems.ResourceDistributor;
                    this.m_grid.GridSystems.ConveyorSystem.BlockAdded += new Action<MyCubeBlock>(this.ConveyorSystemOnBlockAdded);
                    this.m_grid.GridSystems.ConveyorSystem.BlockRemoved += new Action<MyCubeBlock>(this.ConveyorSystemOnBlockRemoved);
                    this.Recalculate();
                }
            }
        }

        private void ConveyorSystemOnBlockAdded(MyCubeBlock myCubeBlock)
        {
            Sandbox.Game.Entities.Interfaces.IMyGasTank item = myCubeBlock as Sandbox.Game.Entities.Interfaces.IMyGasTank;
            if ((item != null) && item.IsResourceStorage(MyResourceDistributorComponent.HydrogenId))
            {
                this.m_maxHydrogen += item.GasCapacity;
                this.m_tankBlocks.Add(item);
            }
        }

        private void ConveyorSystemOnBlockRemoved(MyCubeBlock myCubeBlock)
        {
            Sandbox.Game.Entities.Interfaces.IMyGasTank item = myCubeBlock as Sandbox.Game.Entities.Interfaces.IMyGasTank;
            if ((item != null) && item.IsResourceStorage(MyResourceDistributorComponent.HydrogenId))
            {
                this.m_maxHydrogen -= item.GasCapacity;
                this.m_tankBlocks.Remove(item);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private void Recalculate()
        {
            this.m_maxHydrogen = 0f;
            if (this.m_grid != null)
            {
                foreach (Sandbox.Game.Entities.Interfaces.IMyGasTank tank in this.m_grid.GridSystems.ConveyorSystem.ConveyorEndpointBlocks)
                {
                    if (tank == null)
                    {
                        continue;
                    }
                    if (tank.IsResourceStorage(MyResourceDistributorComponent.HydrogenId))
                    {
                        this.m_maxHydrogen += tank.GasCapacity;
                        this.m_tankBlocks.Add(tank);
                    }
                }
            }
        }

        public override void Run()
        {
            base.Run();
            using (MySpriteDrawFrame frame = base.m_surface.DrawFrame())
            {
                base.AddBackground(frame, new Color(base.m_foregroundColor, 0.66f));
                if ((this.m_resourceDistributor == null) && (this.m_grid != null))
                {
                    this.m_resourceDistributor = this.m_grid.GridSystems.ResourceDistributor;
                }
                if (this.m_resourceDistributor != null)
                {
                    Color barBgColor = new Color(base.m_foregroundColor, 0.1f);
                    float x = this.m_innerSize.X * 0.5f;
                    float num2 = x * 0.06f;
                    float max = this.m_resourceDistributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId);
                    float num4 = MyMath.Clamp(this.m_resourceDistributor.TotalRequiredInputByType(MyResourceDistributorComponent.ElectricityId), 0f, max);
                    float ratio = (max > 0f) ? (num4 / max) : 0f;
                    this.m_sb.Clear();
                    this.m_sb.Append("[");
                    Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, 1f);
                    float scale = this.m_decorationSize.Y / vector.Y;
                    vector = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, scale);
                    this.m_sb.Clear();
                    this.m_sb.Append($"{ratio * 100f:0}");
                    Vector2 vector2 = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, base.m_fontScale);
                    MySprite sprite = new MySprite {
                        Position = new Vector2((base.m_halfSize.X + (x * 0.6f)) - num2, this.m_firstLine - (vector2.Y * 0.5f)),
                        Size = new Vector2(this.m_innerSize.X, this.m_innerSize.Y),
                        Type = SpriteType.TEXT,
                        FontId = base.m_fontId,
                        Alignment = TextAlignment.LEFT,
                        Color = new Color?(base.m_foregroundColor),
                        RotationOrScale = base.m_fontScale,
                        Data = this.m_sb.ToString()
                    };
                    frame.Add(sprite);
                    Vector2? position = null;
                    position = null;
                    sprite = new MySprite(SpriteType.TEXTURE, ENERGY_ICON, position, position, new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, 0f) {
                        Position = new Vector2((base.m_halfSize.X - (x * 0.6f)) - num2, this.m_firstLine),
                        Size = new Vector2(vector.Y * 0.6f)
                    };
                    frame.Add(sprite);
                    base.AddProgressBar(frame, new Vector2(base.m_halfSize.X - num2, this.m_firstLine), new Vector2(x, vector.Y * 0.4f), ratio, barBgColor, base.m_foregroundColor, null, null);
                    float num7 = 0f;
                    foreach (Sandbox.Game.Entities.Interfaces.IMyGasTank tank in this.m_tankBlocks)
                    {
                        num7 += (float) (tank.FilledRatio * tank.GasCapacity);
                    }
                    ratio = (this.m_maxHydrogen > 0f) ? (num7 / this.m_maxHydrogen) : 0f;
                    this.m_sb.Clear();
                    this.m_sb.Append($"{ratio * 100f:0}");
                    vector2 = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, base.m_fontScale);
                    sprite = new MySprite {
                        Position = new Vector2((base.m_halfSize.X + (x * 0.6f)) - num2, this.m_secondLine - (vector2.Y * 0.5f)),
                        Size = new Vector2(this.m_innerSize.X, this.m_innerSize.Y),
                        Type = SpriteType.TEXT,
                        FontId = base.m_fontId,
                        Alignment = TextAlignment.LEFT,
                        Color = new Color?(base.m_foregroundColor),
                        RotationOrScale = base.m_fontScale,
                        Data = this.m_sb.ToString()
                    };
                    frame.Add(sprite);
                    position = null;
                    position = null;
                    sprite = new MySprite(SpriteType.TEXTURE, HYDROGEN_ICON, position, position, new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, 0f) {
                        Position = new Vector2((base.m_halfSize.X - (x * 0.6f)) - num2, this.m_secondLine),
                        Size = new Vector2(vector.Y * 0.6f)
                    };
                    frame.Add(sprite);
                    base.AddProgressBar(frame, new Vector2(base.m_halfSize.X - num2, this.m_secondLine), new Vector2(x, vector.Y * 0.4f), ratio, barBgColor, base.m_foregroundColor, null, null);
                    float num8 = (this.m_innerSize.Y / 256f) * 0.9f;
                    base.AddBrackets(frame, new Vector2(64f, 256f), num8);
                }
            }
        }

        public override ScriptUpdate NeedsUpdate =>
            ScriptUpdate.Update10;
    }
}

