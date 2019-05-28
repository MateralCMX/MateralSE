namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class MyLayers
    {
        public static readonly List<UpdateLayerDesc> UpdateLayerDescriptors = new List<UpdateLayerDesc>();

        public static int GetSyncDistance() => 
            ((UpdateLayerDescriptors.Count != 0) ? UpdateLayerDescriptors[UpdateLayerDescriptors.Count - 1].Radius : 0);

        public static void SetSyncDistance(int distance)
        {
            UpdateLayerDescriptors.Clear();
            UpdateLayerDesc item = new UpdateLayerDesc {
                Radius = 20,
                UpdateInterval = 60,
                SendInterval = 4
            };
            UpdateLayerDescriptors.Add(item);
            for (UpdateLayerDesc desc = UpdateLayerDescriptors[UpdateLayerDescriptors.Count - 1]; desc.Radius < distance; desc = UpdateLayerDescriptors[UpdateLayerDescriptors.Count - 1])
            {
                item = new UpdateLayerDesc {
                    Radius = Math.Min(desc.Radius * 4, distance),
                    UpdateInterval = desc.UpdateInterval * 2,
                    SendInterval = desc.SendInterval * 2
                };
                UpdateLayerDescriptors.Add(item);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UpdateLayerDesc
        {
            public int Radius;
            public int UpdateInterval;
            public int SendInterval;
        }
    }
}

