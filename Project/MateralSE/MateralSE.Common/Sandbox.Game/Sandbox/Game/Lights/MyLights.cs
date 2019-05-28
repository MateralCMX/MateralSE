namespace Sandbox.Game.Lights
{
    using Sandbox;
    using Sandbox.Engine.Platform;
    using System;
    using VRage.Game.Components;
    using VRage.Generics;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, Priority=600)]
    public class MyLights : MySessionComponentBase
    {
        private static MyObjectsPool<MyLight> m_preallocatedLights;
        private static int m_lightsCount;
        private static BoundingSphere m_lastBoundingSphere;

        public static MyLight AddLight()
        {
            MyLight light;
            if (Game.IsDedicated)
            {
                return null;
            }
            m_preallocatedLights.AllocateOrCreate(out light);
            return light;
        }

        public override void LoadData()
        {
            MySandboxGame.Log.WriteLine("MyLights.LoadData() - START");
            MySandboxGame.Log.IncreaseIndent();
            if (m_preallocatedLights == null)
            {
                m_preallocatedLights = new MyObjectsPool<MyLight>(0xfa0, null);
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyLights.LoadData() - END");
        }

        public static void RemoveLight(MyLight light)
        {
            if (light != null)
            {
                light.Clear();
                if (m_preallocatedLights != null)
                {
                    m_preallocatedLights.Deallocate(light);
                }
            }
        }

        protected override void UnloadData()
        {
            if (m_preallocatedLights != null)
            {
                m_preallocatedLights.DeallocateAll();
                m_preallocatedLights = null;
            }
        }
    }
}

