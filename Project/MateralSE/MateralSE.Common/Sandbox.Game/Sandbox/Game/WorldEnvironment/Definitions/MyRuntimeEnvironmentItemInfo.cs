namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using VRage.Utils;

    public class MyRuntimeEnvironmentItemInfo
    {
        public MyItemTypeDefinition Type;
        public MyStringHash Subtype;
        public float Offset;
        public float Density;
        public short Index;

        public MyRuntimeEnvironmentItemInfo(MyProceduralEnvironmentDefinition def, MyEnvironmentItemInfo info, int id)
        {
            this.Index = (short) id;
            this.Type = def.ItemTypes[info.Type];
            this.Subtype = info.Subtype;
            this.Offset = info.Offset;
            this.Density = info.Density;
        }
    }
}

