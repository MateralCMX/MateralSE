namespace VRage.ObjectBuilders
{
    using System;
    using System.Collections.Generic;

    public class MyRuntimeObjectBuilderIdComparer : IComparer<MyRuntimeObjectBuilderId>, IEqualityComparer<MyRuntimeObjectBuilderId>
    {
        public int Compare(MyRuntimeObjectBuilderId x, MyRuntimeObjectBuilderId y) => 
            (x.Value - y.Value);

        public bool Equals(MyRuntimeObjectBuilderId x, MyRuntimeObjectBuilderId y) => 
            (x.Value == y.Value);

        public int GetHashCode(MyRuntimeObjectBuilderId obj) => 
            obj.Value.GetHashCode();
    }
}

