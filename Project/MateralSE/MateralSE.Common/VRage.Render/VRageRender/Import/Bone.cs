namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public class Bone
    {
        public Bone()
        {
            this.Children = new List<Bone>();
        }

        public override string ToString() => 
            (this.Name + ": " + base.ToString());

        public string Name { get; set; }

        public Matrix LocalTransform { get; set; }

        public Bone Parent { get; set; }

        public List<Bone> Children { get; private set; }
    }
}

