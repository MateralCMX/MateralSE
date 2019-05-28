namespace VRage.Voxels
{
    using System;

    public class MyStorageDataProviderAttribute : Attribute
    {
        public readonly int ProviderTypeId;
        public Type ProviderType;

        public MyStorageDataProviderAttribute(int typeId)
        {
            this.ProviderTypeId = typeId;
        }
    }
}

