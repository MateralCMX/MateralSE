namespace Sandbox.Graphics
{
    using System;
    using System.Collections.Generic;

    public class MyTextureAtlas : Dictionary<string, MyTextureAtlasItem>
    {
        public MyTextureAtlas(int numItems) : base(numItems)
        {
        }
    }
}

