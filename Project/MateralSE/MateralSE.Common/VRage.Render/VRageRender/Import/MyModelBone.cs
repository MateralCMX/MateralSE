namespace VRageRender.Import
{
    using System;
    using VRageMath;

    public sealed class MyModelBone
    {
        public int Index;
        public string Name;
        public int Parent;
        public Matrix Transform;

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.Name, " (", this.Index, ")" };
            return string.Concat(objArray1);
        }
    }
}

