namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public abstract class MyTextSurfaceScriptBase : IMyTextSurfaceScript, IDisposable
    {
        public static readonly Color DEFAULT_BACKGROUND_COLOR = new Color(0, 0x58, 0x97);
        public static readonly Color DEFAULT_FONT_COLOR = new Color(0xb3, 0xed, 0xff);
        protected IMyTextSurface m_surface;
        protected IMyCubeBlock m_block;
        protected Vector2 m_size;
        protected Vector2 m_halfSize;
        protected Vector2 m_scale;
        protected Color m_backgroundColor = DEFAULT_BACKGROUND_COLOR;
        protected Color m_foregroundColor = DEFAULT_FONT_COLOR;

        protected MyTextSurfaceScriptBase(IMyTextSurface surface, IMyCubeBlock block, Vector2 size)
        {
            this.m_surface = surface;
            this.m_block = block;
            this.m_size = size;
            this.m_halfSize = size / 2f;
            this.m_scale = size / 512f;
        }

        public virtual void Dispose()
        {
            this.m_surface = null;
            this.m_block = null;
        }

        public static void FitRect(Vector2 texture, ref Vector2 rect)
        {
            float num = Math.Min((float) (texture.X / rect.X), (float) (texture.Y / rect.Y));
            rect *= num;
        }

        public virtual void Run()
        {
        }

        public IMyTextSurface Surface =>
            this.m_surface;

        public IMyCubeBlock Block =>
            this.m_block;

        public Color ForegroundColor =>
            this.m_foregroundColor;

        public Color BackgroundColor =>
            this.m_backgroundColor;

        public abstract ScriptUpdate NeedsUpdate { get; }
    }
}

