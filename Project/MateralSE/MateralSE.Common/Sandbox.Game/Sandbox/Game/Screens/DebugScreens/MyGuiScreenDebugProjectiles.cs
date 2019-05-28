namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.Weapons;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("Game", "Projectiles")]
    internal class MyGuiScreenDebugProjectiles : MyGuiScreenDebugBase
    {
        private static MyGuiScreenDebugProjectiles m_instance;

        public MyGuiScreenDebugProjectiles() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugProjectiles";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            m_instance = this;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector4? color = null;
            this.AddSlider("Impact snd timeout", (float) MyProjectile.CollisionSoundsTimedCache.EventTimeoutMs, 0f, 1000f, (Action<MyGuiControlSlider>) (x => (MyProjectile.CollisionSoundsTimedCache.EventTimeoutMs = (int) x.Value)), color);
            color = null;
            this.AddSlider("Impact part. timeout", (float) MyProjectile.CollisionParticlesTimedCache.EventTimeoutMs, 0f, 1000f, (Action<MyGuiControlSlider>) (x => (MyProjectile.CollisionParticlesTimedCache.EventTimeoutMs = (int) x.Value)), color);
            color = null;
            this.AddSlider("Impact part. cube size", 1f / ((float) MyProjectile.CollisionParticlesSpaceMapping), 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyProjectile.CollisionParticlesSpaceMapping = 1f / x.Value)), color);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugProjectiles.<>c <>9 = new MyGuiScreenDebugProjectiles.<>c();
            public static Action<MyGuiControlSlider> <>9__3_0;
            public static Action<MyGuiControlSlider> <>9__3_1;
            public static Action<MyGuiControlSlider> <>9__3_2;

            internal void <RecreateControls>b__3_0(MyGuiControlSlider x)
            {
                MyProjectile.CollisionSoundsTimedCache.EventTimeoutMs = (int) x.Value;
            }

            internal void <RecreateControls>b__3_1(MyGuiControlSlider x)
            {
                MyProjectile.CollisionParticlesTimedCache.EventTimeoutMs = (int) x.Value;
            }

            internal void <RecreateControls>b__3_2(MyGuiControlSlider x)
            {
                MyProjectile.CollisionParticlesSpaceMapping = 1f / x.Value;
            }
        }
    }
}

