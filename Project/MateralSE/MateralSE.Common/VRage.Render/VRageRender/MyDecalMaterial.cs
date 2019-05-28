namespace VRageRender
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Utils;

    public class MyDecalMaterial
    {
        public bool Transparent;

        public MyDecalMaterial(MyDecalMaterialDesc materialDef, bool transparent, MyStringHash target, MyStringHash source, float minSize, float maxSize, float depth, float rotation)
        {
            this.StringId = MyDecalMaterials.GetStringId(source, target);
            this.Material = materialDef;
            this.Target = target;
            this.Source = source;
            this.MinSize = minSize;
            this.MaxSize = maxSize;
            this.Depth = depth;
            this.Rotation = rotation;
            this.Transparent = transparent;
        }

        public string StringId { get; private set; }

        public MyDecalMaterialDesc Material { get; private set; }

        public MyStringHash Target { get; private set; }

        public MyStringHash Source { get; private set; }

        public float MinSize { get; private set; }

        public float MaxSize { get; private set; }

        public float Depth { get; private set; }

        public float Rotation { get; private set; }
    }
}

