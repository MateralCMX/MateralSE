namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("Game", "Environment")]
    public class MyGuiScreenDebugEnvironment : MyGuiScreenDebugBase
    {
        public static Action DeleteEnvironmentItems;

        public MyGuiScreenDebugEnvironment() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderEnvironment";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            base.AddShareFocusHint();
            base.Spacing = 0.01f;
            Vector2? captionOffset = null;
            base.AddCaption("World Environment", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            captionOffset = null;
            base.AddCaption("Debug Tools:", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Update Environment Sectors", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.EnableUpdate), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.EnableUpdate = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddButton("Refresh Sectors", x => this.RefreshSectors(), null, color, captionOffset);
            base.AddLabel("Debug Draw Options:", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Sectors", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawSectors), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawSectors = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Clipmap Proxies", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawProxies), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawProxies = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Dynamic Clusters", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawDynamicObjectClusters), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawDynamicObjectClusters = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Collision Boxes", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawCollisionCheckers), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawCollisionCheckers = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Providers", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawEnvironmentProviders), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawEnvironmentProviders = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Active Sector Items", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorItems), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorItems = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Draw Active Sector Provider", (Func<bool>) (() => MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorProvider), (Action<bool>) (x => (MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorProvider = x)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Sector Name Draw Distance:", new MyGuiSliderPropertiesExponential(1f, 1000f, 10f, false), () => MyPlanetEnvironmentSessionComponent.DebugDrawDistance, x => MyPlanetEnvironmentSessionComponent.DebugDrawDistance = x, color);
        }

        private void RefreshSectors()
        {
            using (List<MyPlanet>.Enumerator enumerator = MyPlanets.GetPlanets().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Components.Get<MyPlanetEnvironmentComponent>().CloseAll();
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugEnvironment.<>c <>9 = new MyGuiScreenDebugEnvironment.<>c();
            public static Func<bool> <>9__3_0;
            public static Action<bool> <>9__3_1;
            public static Func<bool> <>9__3_3;
            public static Action<bool> <>9__3_4;
            public static Func<bool> <>9__3_5;
            public static Action<bool> <>9__3_6;
            public static Func<bool> <>9__3_7;
            public static Action<bool> <>9__3_8;
            public static Func<bool> <>9__3_9;
            public static Action<bool> <>9__3_10;
            public static Func<bool> <>9__3_11;
            public static Action<bool> <>9__3_12;
            public static Func<bool> <>9__3_13;
            public static Action<bool> <>9__3_14;
            public static Func<bool> <>9__3_15;
            public static Action<bool> <>9__3_16;
            public static Func<float> <>9__3_17;
            public static Action<float> <>9__3_18;

            internal bool <RecreateControls>b__3_0() => 
                MyPlanetEnvironmentSessionComponent.EnableUpdate;

            internal void <RecreateControls>b__3_1(bool x)
            {
                MyPlanetEnvironmentSessionComponent.EnableUpdate = x;
            }

            internal void <RecreateControls>b__3_10(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawCollisionCheckers = x;
            }

            internal bool <RecreateControls>b__3_11() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawEnvironmentProviders;

            internal void <RecreateControls>b__3_12(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawEnvironmentProviders = x;
            }

            internal bool <RecreateControls>b__3_13() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorItems;

            internal void <RecreateControls>b__3_14(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorItems = x;
            }

            internal bool <RecreateControls>b__3_15() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorProvider;

            internal void <RecreateControls>b__3_16(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorProvider = x;
            }

            internal float <RecreateControls>b__3_17() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawDistance;

            internal void <RecreateControls>b__3_18(float x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawDistance = x;
            }

            internal bool <RecreateControls>b__3_3() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawSectors;

            internal void <RecreateControls>b__3_4(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawSectors = x;
            }

            internal bool <RecreateControls>b__3_5() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawProxies;

            internal void <RecreateControls>b__3_6(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawProxies = x;
            }

            internal bool <RecreateControls>b__3_7() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawDynamicObjectClusters;

            internal void <RecreateControls>b__3_8(bool x)
            {
                MyPlanetEnvironmentSessionComponent.DebugDrawDynamicObjectClusters = x;
            }

            internal bool <RecreateControls>b__3_9() => 
                MyPlanetEnvironmentSessionComponent.DebugDrawCollisionCheckers;
        }
    }
}

