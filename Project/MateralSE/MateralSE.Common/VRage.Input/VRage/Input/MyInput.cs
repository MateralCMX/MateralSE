namespace VRage.Input
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyInput
    {
        public static void Initialize(IMyInput implementation)
        {
            if (Static != null)
            {
                throw new InvalidOperationException("Input already initialized.");
            }
            Static = implementation;
        }

        public static void UnloadData()
        {
            if (Static != null)
            {
                Static.UnloadData();
                Static = null;
            }
        }

        public static IMyInput Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            set => 
                (<Static>k__BackingField = value);
        }
    }
}

