namespace Sandbox.Engine.Platform
{
    using System;
    using System.Collections.Generic;
    using VRageRender;

    internal static class MyGpuIds
    {
        private static readonly int[] UnsupportedIntels = new int[] { 
            0x2582, 0x2782, 0x2592, 0x2792, 0x2772, 0x2776, 0x27a2, 0x27a6, 0x27ae, 0x29d2, 0x29d3, 0x29b2, 0x29b3, 0x29c2, 0x29c3, 0xa001,
            0xa002, 0xa011, 0xa012, 0x2972, 0x2973, 0x2992, 0x2993
        };
        private static readonly int[] UnderMinimumIntels = new int[] { 
            0x29a2, 0x29a3, 0x2982, 0x2983, 0x2a02, 0x2a03, 0x2a12, 0x2a13, 0x2e42, 0x2e43, 0x2e92, 0x2e93, 0x2e12, 0x2e13, 0x2e32, 0x2e33,
            0x2e22, 0x2e23, 0x2a42, 0x2a43, 0x42, 70, 0x102, 0x106, 0x112, 0x116, 290, 0x126, 0x10a, 0x152, 0x162, 0x166,
            0x402
        };
        private static readonly int[] UnsupportedRadeons = new int[] { 0x791e, 0x791f, 0x7145 };
        private static readonly Dictionary<VendorIds, int[]> Unsupported;
        private static readonly Dictionary<VendorIds, int[]> UnderMinimum;

        static MyGpuIds()
        {
            Dictionary<VendorIds, int[]> dictionary1 = new Dictionary<VendorIds, int[]>();
            dictionary1.Add(VendorIds.Amd, UnsupportedRadeons);
            dictionary1.Add(VendorIds.Intel, UnsupportedIntels);
            Unsupported = dictionary1;
            Dictionary<VendorIds, int[]> dictionary = new Dictionary<VendorIds, int[]>();
            int[] numArray1 = new int[] { 0x405 };
            dictionary.Add(VendorIds.VMWare, numArray1);
            int[] numArray2 = new int[] { 0x4005 };
            dictionary.Add(VendorIds.Parallels, numArray2);
            dictionary.Add(VendorIds.Intel, UnderMinimumIntels);
            UnderMinimum = dictionary;
        }

        public static bool IsUnderMinimum(VendorIds vendorId, int deviceId)
        {
            int[] numArray;
            return (IsUnsupported(vendorId, deviceId) || (UnderMinimum.TryGetValue(vendorId, out numArray) && numArray.Contains<int>(deviceId)));
        }

        public static bool IsUnsupported(VendorIds vendorId, int deviceId)
        {
            int[] numArray;
            return (Unsupported.TryGetValue(vendorId, out numArray) && numArray.Contains<int>(deviceId));
        }
    }
}

