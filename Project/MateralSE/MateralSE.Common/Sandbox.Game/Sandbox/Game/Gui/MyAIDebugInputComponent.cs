namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    [StaticEventOwner]
    public class MyAIDebugInputComponent : MyDebugComponent
    {
        private bool m_drawSphere;
        private BoundingSphere m_sphere;
        private Matrix m_sphereMatrix;
        private string m_string;
        private Vector3D m_point1;
        private Vector3D m_point2;
        private MySmartPath m_smartPath;
        private Vector3D m_currentTarget;
        private List<Vector3D> m_pastTargets = new List<Vector3D>();
        public static int FaceToRemove;
        public static int BinIndex = -1;
        private static List<DebugDrawPoint> DebugDrawPoints = new List<DebugDrawPoint>();
        private static List<DebugDrawSphere> DebugDrawSpheres = new List<DebugDrawSphere>();
        private static List<DebugDrawBox> DebugDrawBoxes = new List<DebugDrawBox>();
        private static MyWingedEdgeMesh DebugDrawMesh = null;
        private static List<MyPolygon> DebugDrawPolys = new List<MyPolygon>();
        public static List<BoundingBoxD> Boxes = null;
        private static bool m_drawDebug = false;
        private static bool m_drawNavesh = false;
        private static bool m_drawPhysicalMesh = false;

        public MyAIDebugInputComponent()
        {
            if (MyPerGameSettings.EnableAi)
            {
                this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Toggle Draw Grid Physical Mesh", new Func<bool>(this.ToggleDrawPhysicalMesh));
                this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Add bot", new Func<bool>(this.AddBot));
                this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Remove bot", new Func<bool>(this.RemoveBot));
                this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Toggle Draw Debug", new Func<bool>(this.ToggleDrawDebug));
                this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Toggle Wireframe", new Func<bool>(this.ToggleWireframe));
                this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Set PF target", new Func<bool>(this.SetPathfindingDebugTarget));
                this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Toggle Draw Navmesh", new Func<bool>(this.ToggleDrawNavmesh));
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Generate Navmesh Tile", new Func<bool>(this.GenerateNavmeshTile));
                this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Invalidate Navmesh Position", new Func<bool>(this.InvalidateNavmeshPosition));
            }
        }

        private bool AddBot()
        {
            MyAgentDefinition agentDefinition = (MyPerGameSettings.Game != GameEnum.SE_GAME) ? (MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_HumanoidBot), "NormalBarbarian")) as MyAgentDefinition) : (MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_AnimalBot), "Wolf")) as MyAgentDefinition);
            MyAIComponent.Static.TrySpawnBot(agentDefinition);
            return true;
        }

        public static void AddDebugBox(BoundingBoxD box, Color color)
        {
            DebugDrawBox item = new DebugDrawBox {
                Box = box,
                Color = color
            };
            DebugDrawBoxes.Add(item);
        }

        public static void AddDebugPoint(Vector3D point, Color color)
        {
            DebugDrawPoint item = new DebugDrawPoint {
                Position = point,
                Color = color
            };
            DebugDrawPoints.Add(item);
        }

        public static void AddDebugSphere(Vector3D position, float radius, Color color)
        {
            DebugDrawSphere item = new DebugDrawSphere {
                Position = position,
                Radius = radius,
                Color = color
            };
            DebugDrawSpheres.Add(item);
        }

        public static void ClearDebugBoxes()
        {
            DebugDrawBoxes.Clear();
        }

        public static void ClearDebugPoints()
        {
            DebugDrawPoints.Clear();
        }

        public static void ClearDebugSpheres()
        {
            DebugDrawSpheres.Clear();
        }

        public override void Draw()
        {
            base.Draw();
            if (MySector.MainCamera != null)
            {
                Vector3 forwardVector = MySector.MainCamera.ForwardVector;
                Vector3D position = MySector.MainCamera.Position;
                MyPhysics.HitInfo? nullable = MyPhysics.CastRay(position, MySector.MainCamera.Position + (500f * forwardVector), 0);
                if (nullable != null)
                {
                    IMyEntity hitEntity = nullable.Value.HkHitInfo.GetHitEntity();
                    if (hitEntity != null)
                    {
                        MyVoxelPhysics topMostParent = hitEntity.GetTopMostParent(null) as MyVoxelPhysics;
                        if (topMostParent != null)
                        {
                            MyPlanet parent = topMostParent.Parent;
                            IMyGravityProvider provider = parent as IMyGravityProvider;
                            if (provider != null)
                            {
                                Vector3 worldGravity = provider.GetWorldGravity(nullable.Value.Position);
                                worldGravity.Normalize();
                                Vector3D vectord2 = parent.PositionComp.GetPosition() - (worldGravity * 9503f);
                                MyRenderProxy.DebugDrawSphere(vectord2, 0.5f, Color.Red, 1f, false, false, true, false);
                                MyRenderProxy.DebugDrawSphere(vectord2, 5.5f, Color.Yellow, 1f, false, false, true, false);
                                nullable = MyPhysics.CastRay(vectord2, vectord2 + (worldGravity * 500f), 0);
                                if (nullable != null)
                                {
                                    MyRenderProxy.DebugDrawText2D(new Vector2(10f, 10f), (nullable.Value.HkHitInfo.HitFraction * 500f).ToString(), Color.White, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                }
                            }
                        }
                    }
                }
            }
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && (MyCubeBuilder.Static != null))
            {
                if (this.m_smartPath != null)
                {
                    this.m_smartPath.DebugDraw();
                    MyRenderProxy.DebugDrawSphere(this.m_currentTarget, 2f, Color.HotPink, 1f, false, false, true, false);
                    for (int i = 1; i < this.m_pastTargets.Count; i++)
                    {
                        MyRenderProxy.DebugDrawLine3D(this.m_pastTargets[i], this.m_pastTargets[i - 1], Color.Blue, Color.Blue, false, false);
                    }
                }
                MyRenderProxy.DebugDrawOBB(MyCubeBuilder.Static.GetBuildBoundingBox(0f), Color.Red, 0.25f, false, false, false);
                MyScreenManager.GetScreenWithFocus();
                if ((MyScreenManager.GetScreenWithFocus() != null) && (MyScreenManager.GetScreenWithFocus().DebugNamePath == "MyGuiScreenGamePlay"))
                {
                    if (this.m_drawSphere)
                    {
                        MyRenderProxy.DebugDrawSphere(this.m_sphere.Center, this.m_sphere.Radius, Color.Red, 1f, false, false, true, false);
                        MyRenderProxy.DebugDrawAxis(this.m_sphereMatrix, 50f, false, false, false);
                        MyRenderProxy.DebugDrawText2D(new Vector2(200f, 0f), this.m_string, Color.Red, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    }
                    MyRenderProxy.DebugDrawSphere(this.m_point1, 0.5f, Color.Orange.ToVector3(), 1f, true, false, true, false);
                    MyRenderProxy.DebugDrawSphere(this.m_point2, 0.5f, Color.Orange.ToVector3(), 1f, true, false, true, false);
                    foreach (DebugDrawPoint point in DebugDrawPoints)
                    {
                        MyRenderProxy.DebugDrawSphere(point.Position, 0.03f, point.Color, 1f, false, false, true, false);
                    }
                    foreach (DebugDrawSphere sphere in DebugDrawSpheres)
                    {
                        MyRenderProxy.DebugDrawSphere(sphere.Position, sphere.Radius, sphere.Color, 1f, false, false, true, false);
                    }
                    foreach (DebugDrawBox box in DebugDrawBoxes)
                    {
                        MyRenderProxy.DebugDrawAABB(box.Box, box.Color, 1f, 1f, false, false, false);
                    }
                    if (DebugDrawMesh != null)
                    {
                        Matrix identity = Matrix.Identity;
                        DebugDrawMesh.DebugDraw(ref identity, MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES);
                    }
                    using (List<MyPolygon>.Enumerator enumerator4 = DebugDrawPolys.GetEnumerator())
                    {
                        while (enumerator4.MoveNext())
                        {
                            MatrixD identity = MatrixD.Identity;
                            enumerator4.Current.DebugDraw(ref identity);
                        }
                    }
                    MyPolygonBoolOps.Static.DebugDraw(MatrixD.Identity);
                    if (Boxes != null)
                    {
                        using (List<BoundingBoxD>.Enumerator enumerator5 = Boxes.GetEnumerator())
                        {
                            while (enumerator5.MoveNext())
                            {
                                MyRenderProxy.DebugDrawAABB(enumerator5.Current, Color.Red, 1f, 1f, true, false, false);
                            }
                        }
                    }
                }
            }
        }

        private bool GenerateNavmeshTile()
        {
            Vector3D? targetPosition = this.GetTargetPosition();
            MyAIComponent.Static.GenerateNavmeshTile(targetPosition);
            return true;
        }

        public override string GetName() => 
            "A.I.";

        private Vector3D? GetTargetPosition()
        {
            LineD ed = new LineD(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 1000f));
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(ed.From, ed.To, toList, 15);
            toList.RemoveAll(hit => ReferenceEquals(hit.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity));
            if (toList.Count != 0)
            {
                return new Vector3D?(toList[0].Position);
            }
            return null;
        }

        public override bool HandleInput() => 
            ((MySession.Static != null) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogPrefabCheat) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogRemoveTriangle) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogViewEdge) ? base.HandleInput() : false) : false) : false) : false);

        private bool InvalidateNavmeshPosition()
        {
            Vector3D? targetPosition = this.GetTargetPosition();
            MyAIComponent.Static.InvalidateNavmeshPosition(targetPosition);
            return true;
        }

        private bool RemoveBot()
        {
            int num = -1;
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                if (player.Id.SteamId == Sync.MyId)
                {
                    num = Math.Max(num, player.Id.SerialId);
                }
            }
            if (num > 0)
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(Sync.MyId, num));
                Sync.Players.RemovePlayer(playerById, true);
            }
            return true;
        }

        private bool SetPathfindingDebugTarget()
        {
            Vector3D? targetPosition = this.GetTargetPosition();
            MyAIComponent.Static.SetPathfindingDebugTarget(targetPosition);
            return true;
        }

        private bool ToggleDrawDebug()
        {
            m_drawDebug = !m_drawDebug;
            MyAIComponent.Static.PathfindingSetDrawDebug(m_drawDebug);
            return true;
        }

        private bool ToggleDrawNavmesh()
        {
            m_drawNavesh = !m_drawNavesh;
            MyAIComponent.Static.PathfindingSetDrawNavmesh(m_drawNavesh);
            return true;
        }

        private bool ToggleDrawPhysicalMesh()
        {
            m_drawPhysicalMesh = !m_drawPhysicalMesh;
            return true;
        }

        private bool ToggleWireframe()
        {
            MyRenderProxy.Settings.Wireframe = !MyRenderProxy.Settings.Wireframe;
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAIDebugInputComponent.<>c <>9 = new MyAIDebugInputComponent.<>c();
            public static Func<string> <>9__20_0;
            public static Func<string> <>9__20_1;
            public static Func<string> <>9__20_2;
            public static Func<string> <>9__20_3;
            public static Func<string> <>9__20_4;
            public static Func<string> <>9__20_5;
            public static Func<string> <>9__20_6;
            public static Func<string> <>9__20_7;
            public static Func<string> <>9__20_8;
            public static Predicate<MyPhysics.HitInfo> <>9__31_0;

            internal string <.ctor>b__20_0() => 
                "Toggle Draw Grid Physical Mesh";

            internal string <.ctor>b__20_1() => 
                "Add bot";

            internal string <.ctor>b__20_2() => 
                "Remove bot";

            internal string <.ctor>b__20_3() => 
                "Toggle Draw Debug";

            internal string <.ctor>b__20_4() => 
                "Toggle Wireframe";

            internal string <.ctor>b__20_5() => 
                "Set PF target";

            internal string <.ctor>b__20_6() => 
                "Toggle Draw Navmesh";

            internal string <.ctor>b__20_7() => 
                "Generate Navmesh Tile";

            internal string <.ctor>b__20_8() => 
                "Invalidate Navmesh Position";

            internal bool <GetTargetPosition>b__31_0(MyPhysics.HitInfo hit) => 
                ReferenceEquals(hit.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DebugDrawBox
        {
            public BoundingBoxD Box;
            public VRageMath.Color Color;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DebugDrawPoint
        {
            public Vector3D Position;
            public VRageMath.Color Color;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DebugDrawSphere
        {
            public Vector3D Position;
            public float Radius;
            public VRageMath.Color Color;
        }
    }
}

