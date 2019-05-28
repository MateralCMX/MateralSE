namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyAsteroidsDebugInputComponent : MyDebugComponent
    {
        private bool m_drawSeeds;
        private bool m_drawTrackedEntities;
        private bool m_drawAroundCamera;
        private bool m_drawRadius;
        private bool m_drawDistance;
        private bool m_drawCells;
        private List<MyCharacter> m_plys = new List<MyCharacter>();
        private float m_originalFarPlaneDisatance = -1f;
        private float m_debugFarPlaneDistance = 1000000f;
        private bool m_fakeFarPlaneDistance;
        private List<MyObjectSeed> m_tmpSeedsList = new List<MyObjectSeed>();
        private List<MyProceduralCell> m_tmpCellsList = new List<MyProceduralCell>();

        public MyAsteroidsDebugInputComponent()
        {
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Enable Meteor Debug Draw", delegate {
                MyDebugDrawSettings.DEBUG_DRAW_METEORITS_DIRECTIONS = true;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Spawn meteor shower", delegate {
                MyMeteorShower.StartDebugWave((Vector3) MySession.Static.LocalCharacter.WorldMatrix.Translation);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Spawn small asteroid", delegate {
                Vector3 forward = (Vector3) MySession.Static.LocalCharacter.WorldMatrix.Forward;
                MyMeteor.SpawnRandom(MySession.Static.LocalCharacter.WorldMatrix.Translation + (forward * 2f), forward);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Spawn crater", delegate {
                this.SpawnCrater();
                return true;
            });
        }

        public override void Draw()
        {
            base.Draw();
            if (((MySession.Static != null) && (MySector.MainCamera != null)) && (MyProceduralWorldGenerator.Static != null))
            {
                if (this.m_drawAroundCamera)
                {
                    MyProceduralWorldGenerator.Static.OverlapAllPlanetSeedsInSphere(new BoundingSphereD(MySector.MainCamera.Position, (double) (MySector.MainCamera.FarPlaneDistance * 2f)), this.m_tmpSeedsList);
                }
                MyProceduralWorldGenerator.Static.GetAllExisting(this.m_tmpSeedsList);
                double num = 720000.0;
                foreach (MyObjectSeed seed in this.m_tmpSeedsList)
                {
                    if (this.m_drawSeeds)
                    {
                        Vector3D center = seed.BoundingVolume.Center;
                        MyRenderProxy.DebugDrawSphere(center, seed.Size / 2f, (seed.Params.Type == MyObjectSeedType.Asteroid) ? Color.Green : Color.Red, 1f, true, false, true, false);
                        if (this.m_drawRadius)
                        {
                            MyRenderProxy.DebugDrawText3D(center, $"{seed.Size:0}m", Color.Yellow, 0.8f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                        }
                        if (this.m_drawDistance)
                        {
                            double num2 = (center - MySector.MainCamera.Position).Length();
                            MyRenderProxy.DebugDrawText3D(center, $"{num2 / 1000.0:0.0}km", Color.Lerp(Color.Green, Color.Red, (float) (num2 / num)), 0.8f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, -1, false);
                        }
                    }
                }
                this.m_tmpSeedsList.Clear();
                if (this.m_drawTrackedEntities)
                {
                    foreach (KeyValuePair<MyEntity, MyEntityTracker> pair in MyProceduralWorldGenerator.Static.GetTrackedEntities())
                    {
                        MyRenderProxy.DebugDrawSphere(pair.Value.CurrentPosition, (float) pair.Value.BoundingVolume.Radius, Color.White, 1f, true, false, true, false);
                    }
                }
                if (this.m_drawCells)
                {
                    MyProceduralWorldGenerator.Static.GetAllExistingCells(this.m_tmpCellsList);
                    using (List<MyProceduralCell>.Enumerator enumerator3 = this.m_tmpCellsList.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            MyRenderProxy.DebugDrawAABB(enumerator3.Current.BoundingVolume, Color.Blue, 1f, 1f, true, false, false);
                        }
                    }
                }
                this.m_tmpCellsList.Clear();
                MyRenderProxy.DebugDrawSphere(Vector3D.Zero, 0f, Color.White, 0f, false, false, true, false);
            }
        }

        public override string GetName() => 
            "Asteroids";

        public override bool HandleInput() => 
            ((MySession.Static != null) ? base.HandleInput() : false);

        private void SpawnCrater()
        {
            Vector3 translation = (Vector3) MySession.Static.LocalCharacter.WorldMatrix.Translation;
            MyPhysics.CastRay(translation, translation + (MySession.Static.LocalCharacter.WorldMatrix.Forward * 100f), 0);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAsteroidsDebugInputComponent.<>c <>9 = new MyAsteroidsDebugInputComponent.<>c();
            public static Func<string> <>9__10_0;
            public static Func<bool> <>9__10_1;
            public static Func<string> <>9__10_2;
            public static Func<bool> <>9__10_3;
            public static Func<string> <>9__10_4;
            public static Func<bool> <>9__10_5;
            public static Func<string> <>9__10_6;

            internal string <.ctor>b__10_0() => 
                "Enable Meteor Debug Draw";

            internal bool <.ctor>b__10_1()
            {
                MyDebugDrawSettings.DEBUG_DRAW_METEORITS_DIRECTIONS = true;
                return true;
            }

            internal string <.ctor>b__10_2() => 
                "Spawn meteor shower";

            internal bool <.ctor>b__10_3()
            {
                MyMeteorShower.StartDebugWave((Vector3) MySession.Static.LocalCharacter.WorldMatrix.Translation);
                return true;
            }

            internal string <.ctor>b__10_4() => 
                "Spawn small asteroid";

            internal bool <.ctor>b__10_5()
            {
                Vector3 forward = (Vector3) MySession.Static.LocalCharacter.WorldMatrix.Forward;
                MyMeteor.SpawnRandom(MySession.Static.LocalCharacter.WorldMatrix.Translation + (forward * 2f), forward);
                return true;
            }

            internal string <.ctor>b__10_6() => 
                "Spawn crater";
        }
    }
}

