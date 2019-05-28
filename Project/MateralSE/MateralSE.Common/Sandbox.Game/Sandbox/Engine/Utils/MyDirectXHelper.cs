namespace Sandbox.Engine.Utils
{
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using System;
    using System.Runtime.InteropServices;

    public static class MyDirectXHelper
    {
        private static Factory m_factory;

        private static Factory GetFactory()
        {
            if (m_factory == null)
            {
                m_factory = new Factory1();
            }
            return m_factory;
        }

        private static unsafe void GetRamSizes(out ulong vram, Adapter adapter, out ulong svram)
        {
            void* voidPtr = ref ((adapter.Description.DedicatedSystemMemory != 0) ? adapter.Description.DedicatedSystemMemory : adapter.Description.DedicatedVideoMemory).ToPointer();
            vram = (ulong) voidPtr;
            void* voidPtr2 = ref adapter.Description.SharedSystemMemory.ToPointer();
            svram = (ulong) voidPtr2;
        }

        public static bool IsDx11Supported()
        {
            Adapter adapter;
            ulong num2;
            ulong num3;
            Factory factory = GetFactory();
            FeatureLevel[] featureLevels = new FeatureLevel[] { FeatureLevel.Level_11_0 };
            int index = 0;
            while (true)
            {
                if (index >= factory.Adapters.Length)
                {
                    return false;
                }
                adapter = factory.Adapters[index];
                try
                {
                    SharpDX.Direct3D11.Device device1 = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.None, featureLevels);
                    break;
                }
                catch (Exception)
                {
                }
                index++;
            }
            GetRamSizes(out num2, adapter, out num3);
            return ((num2 > 0x1dcd6500L) || (num3 > 0x1dcd6500L));
        }
    }
}

