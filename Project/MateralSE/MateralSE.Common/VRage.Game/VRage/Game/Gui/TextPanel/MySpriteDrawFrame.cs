namespace VRage.Game.GUI.TextPanel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    [StructLayout(LayoutKind.Sequential)]
    public struct MySpriteDrawFrame : IDisposable
    {
        private List<MySprite> m_sprites;
        private Action<MySpriteDrawFrame> m_submitFrameCallback;
        private bool m_isValid;
        public MySpriteDrawFrame(Action<MySpriteDrawFrame> submitFrameCallback)
        {
            this.m_sprites = PoolManager.Get<List<MySprite>>();
            this.m_submitFrameCallback = submitFrameCallback;
            this.m_isValid = this.m_submitFrameCallback != null;
        }

        public void Add(MySprite sprite)
        {
            this.m_sprites.Add(sprite);
        }

        public void AddRange(IEnumerable<MySprite> sprites)
        {
            this.m_sprites.AddRange(sprites);
        }

        public MySpriteCollection ToCollection()
        {
            if (this.m_sprites.Count == 0)
            {
                return new MySpriteCollection();
            }
            MySprite[] sprites = new MySprite[this.m_sprites.Count];
            for (int i = 0; i < this.m_sprites.Count; i++)
            {
                sprites[i] = this.m_sprites[i];
            }
            return new MySpriteCollection(sprites);
        }

        public void AddToList(List<MySprite> list)
        {
            if (list != null)
            {
                list.AddRange(this.m_sprites);
            }
        }

        public void Dispose()
        {
            if (this.m_isValid)
            {
                this.m_isValid = false;
                if (this.m_submitFrameCallback != null)
                {
                    this.m_submitFrameCallback(this);
                }
                this.m_sprites.SetSize<MySprite>(0);
                PoolManager.Return<List<MySprite>>(ref this.m_sprites);
            }
        }
    }
}

