namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    [StaticEventOwner]
    public class MyCestmirDebugInputComponent : MyDebugComponent
    {
        private bool m_drawSphere;
        private BoundingSphere m_sphere;
        private Matrix m_sphereMatrix;
        private string m_string;
        private Vector3D m_point1;
        private Vector3D m_point2;
        private IMyPath m_smartPath;
        private Vector3D m_currentTarget;
        private List<Vector3D> m_pastTargets = new List<Vector3D>();
        public static int FaceToRemove;
        public static int BinIndex = -1;
        [CompilerGenerated]
        private static Action TestAction;
        [CompilerGenerated]
        private static Action<Vector3D, MyEntity> PlacedAction;
        private static List<DebugDrawPoint> DebugDrawPoints = new List<DebugDrawPoint>();
        private static List<DebugDrawSphere> DebugDrawSpheres = new List<DebugDrawSphere>();
        private static List<DebugDrawBox> DebugDrawBoxes = new List<DebugDrawBox>();
        private static MyWingedEdgeMesh DebugDrawMesh = null;
        private static List<MyPolygon> DebugDrawPolys = new List<MyPolygon>();
        public static List<BoundingBoxD> Boxes = null;
        private static List<Tuple<Vector2[], Vector2[]>> m_testList = null;
        private static int m_testIndex = 0;
        private static int m_testOperation = 0;
        private static int m_prevTestIndex = 0;
        private static int m_prevTestOperation = 0;

        public static  event Action<Vector3D, MyEntity> PlacedAction
        {
            [CompilerGenerated] add
            {
                Action<Vector3D, MyEntity> placedAction = PlacedAction;
                while (true)
                {
                    Action<Vector3D, MyEntity> a = placedAction;
                    Action<Vector3D, MyEntity> action3 = (Action<Vector3D, MyEntity>) Delegate.Combine(a, value);
                    placedAction = Interlocked.CompareExchange<Action<Vector3D, MyEntity>>(ref PlacedAction, action3, a);
                    if (ReferenceEquals(placedAction, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Vector3D, MyEntity> placedAction = PlacedAction;
                while (true)
                {
                    Action<Vector3D, MyEntity> source = placedAction;
                    Action<Vector3D, MyEntity> action3 = (Action<Vector3D, MyEntity>) Delegate.Remove(source, value);
                    placedAction = Interlocked.CompareExchange<Action<Vector3D, MyEntity>>(ref PlacedAction, action3, source);
                    if (ReferenceEquals(placedAction, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action TestAction
        {
            [CompilerGenerated] add
            {
                Action testAction = TestAction;
                while (true)
                {
                    Action a = testAction;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    testAction = Interlocked.CompareExchange<Action>(ref TestAction, action3, a);
                    if (ReferenceEquals(testAction, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action testAction = TestAction;
                while (true)
                {
                    Action source = testAction;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    testAction = Interlocked.CompareExchange<Action>(ref TestAction, action3, source);
                    if (ReferenceEquals(testAction, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyCestmirDebugInputComponent()
        {
            this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Add prefab...", new Func<bool>(this.AddPrefab));
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Copy target grid position to clipboard", new Func<bool>(this.CaptureGridPosition));
            if (!MyPerGameSettings.EnableAi)
            {
                this.AddShortcut(MyKeys.I, true, true, false, false, () => "Place an environment item in front of the player", new Func<bool>(this.AddEnvironmentItem));
            }
            else
            {
                this.AddShortcut(MyKeys.Multiply, true, false, false, false, () => "Next navmesh connection helper bin", new Func<bool>(this.NextBin));
                this.AddShortcut(MyKeys.Divide, true, false, false, false, () => "Prev navmesh connection helper bin", new Func<bool>(this.PrevBin));
                this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Add bot", new Func<bool>(this.AddBot));
                this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Remove bot", new Func<bool>(this.RemoveBot));
                this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Find path for first bot", new Func<bool>(this.FindBotPath));
                this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Find path between points", new Func<bool>(this.FindPath));
                this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Find smart path between points", new Func<bool>(this.FindSmartPath));
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Get next smart path target", new Func<bool>(this.GetNextTarget));
                this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Test", new Func<bool>(this.EmitTestAction));
                this.AddShortcut(MyKeys.Add, true, false, false, false, () => "Next funnel segment", delegate {
                    MyNavigationMesh.m_debugFunnelIdx++;
                    return true;
                });
                this.AddShortcut(MyKeys.Subtract, true, false, false, false, () => "Previous funnel segment", delegate {
                    if (MyNavigationMesh.m_debugFunnelIdx > 0)
                    {
                        MyNavigationMesh.m_debugFunnelIdx--;
                    }
                    return true;
                });
                this.AddShortcut(MyKeys.O, true, false, false, false, () => "Remove navmesh tri...", delegate {
                    MyGuiSandbox.AddScreen(new MyGuiScreenDialogRemoveTriangle());
                    return true;
                });
                this.AddShortcut(MyKeys.M, true, false, false, false, () => "View all navmesh edges", delegate {
                    MyWingedEdgeMesh.DebugDrawEdgesReset();
                    return true;
                });
                this.AddShortcut(MyKeys.L, true, false, false, false, () => "View single navmesh edge...", delegate {
                    MyGuiSandbox.AddScreen(new MyGuiScreenDialogViewEdge());
                    return true;
                });
            }
        }

        private bool AddBot()
        {
            MyAgentDefinition botDefinition = MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_AnimalBot), "Wolf")) as MyAgentDefinition;
            MyAIComponent.Static.SpawnNewBot(botDefinition);
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

        private bool AddEnvironmentItem() => 
            true;

        private bool AddPrefab()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenDialogPrefabCheat());
            return true;
        }

        [Event(null, 0x150), Reliable, Server]
        public static void AddPrefabServer(string prefabId, MatrixD worldMatrix)
        {
            int num1;
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                num1 = (int) MySandboxGame.ConfigDedicated.Administrators.Contains(MyEventContext.Current.Sender.ToString());
            }
            else
            {
                num1 = 0;
            }
            if ((num1 != 0) || MyInput.Static.ENABLE_DEVELOPER_KEYS)
            {
                MyPrefabManager.Static.SpawnPrefab(prefabId, worldMatrix.Translation, (Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up, Vector3.Zero, Vector3.Zero, prefabId, prefabId, SpawningOptions.None, 0L, true, null);
            }
        }

        private bool CaptureGridPosition()
        {
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 1000f), toList, 0);
            bool flag = false;
            for (int i = 0; i < toList.Count; i++)
            {
                MyCubeGrid hitEntity = toList[i].HkHitInfo.GetHitEntity() as MyCubeGrid;
                if (hitEntity != null)
                {
                    MyObjectBuilder_CubeGrid objectBuilder = hitEntity.GetObjectBuilder(false) as MyObjectBuilder_CubeGrid;
                    if (objectBuilder != null)
                    {
                        this.m_sphere = objectBuilder.CalculateBoundingSphere();
                        this.m_sphere = this.m_sphere.Transform((Matrix) hitEntity.WorldMatrix);
                        this.m_sphereMatrix = (Matrix) hitEntity.WorldMatrix;
                        this.m_sphereMatrix.Translation = this.m_sphere.Center;
                        StringBuilder builder = new StringBuilder();
                        object[] args = new object[9];
                        args[0] = this.m_sphereMatrix.Translation.X;
                        args[1] = this.m_sphereMatrix.Translation.Y;
                        args[2] = this.m_sphereMatrix.Translation.Z;
                        args[3] = this.m_sphereMatrix.Forward.X;
                        args[4] = this.m_sphereMatrix.Forward.Y;
                        args[5] = this.m_sphereMatrix.Forward.Z;
                        args[6] = this.m_sphereMatrix.Up.X;
                        args[7] = this.m_sphereMatrix.Up.Y;
                        args[8] = this.m_sphereMatrix.Up.Z;
                        builder.AppendFormat("<Position x=\"{0}\" y=\"{1}\" z=\"{2}\" />\n<Forward x=\"{3}\" y=\"{4}\" z=\"{5}\" />\n<Up x=\"{6}\" y=\"{7}\" z=\"{8}\" />", args);
                        this.m_string = builder.ToString();
                        MyClipboardHelper.SetClipboard(this.m_string);
                        flag = true;
                        break;
                    }
                }
            }
            this.m_drawSphere = flag;
            return flag;
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
                    string text1;
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
                    string[] textArray1 = new string[6];
                    textArray1[0] = "Test index: ";
                    textArray1[1] = m_prevTestIndex.ToString();
                    textArray1[2] = "/";
                    string[] textArray2 = textArray1;
                    if (m_testList != null)
                    {
                        text1 = m_testList.Count.ToString();
                    }
                    else
                    {
                        text1 = "-";
                    }
                    textArray2[3] = text1;
                    string[] local1 = textArray2;
                    local1[4] = ", Test operation: ";
                    local1[5] = m_prevTestOperation.ToString();
                    MyRenderProxy.DebugDrawText2D(new Vector2(300f, 0f), string.Concat(local1), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    if ((m_prevTestOperation % 3) == 0)
                    {
                        MyRenderProxy.DebugDrawText2D(new Vector2(300f, 20f), "Intersection", Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    }
                    else if ((m_prevTestOperation % 3) == 1)
                    {
                        MyRenderProxy.DebugDrawText2D(new Vector2(300f, 20f), "Union", Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    }
                    else if ((m_prevTestOperation % 3) == 2)
                    {
                        MyRenderProxy.DebugDrawText2D(new Vector2(300f, 20f), "Difference", Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
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

        private bool EmitPlacedAction(Vector3D position, IMyEntity entity)
        {
            if (PlacedAction != null)
            {
                PlacedAction(position, entity as MyEntity);
            }
            return true;
        }

        private bool EmitTestAction()
        {
            if (TestAction != null)
            {
                TestAction();
            }
            return true;
        }

        private bool FindBotPath()
        {
            Vector3D? nullable;
            IMyEntity entity;
            Raycast(out nullable, out entity);
            if (nullable != null)
            {
                this.EmitPlacedAction(nullable.Value, entity);
            }
            return true;
        }

        private bool FindPath()
        {
            Vector3D? nullable;
            IMyEntity entity;
            Raycast(out nullable, out entity);
            if (nullable != null)
            {
                this.m_point1 = this.m_point2;
                this.m_point2 = nullable.Value;
                MyCestmirPathfindingShorts.Pathfinding.FindPathLowlevel(this.m_point1, this.m_point2);
            }
            return true;
        }

        private bool FindSmartPath()
        {
            Vector3D? nullable;
            IMyEntity entity;
            if (MyAIComponent.Static.Pathfinding == null)
            {
                return false;
            }
            Raycast(out nullable, out entity);
            if (nullable != null)
            {
                this.m_point1 = this.m_point2;
                this.m_point2 = nullable.Value;
                MyDestinationSphere end = new MyDestinationSphere(ref this.m_point2, 3f);
                if (this.m_smartPath != null)
                {
                    this.m_smartPath.Invalidate();
                }
                this.m_smartPath = MyAIComponent.Static.Pathfinding.FindPathGlobal(this.m_point1, end, null);
                this.m_pastTargets.Clear();
                this.m_currentTarget = this.m_point1;
                this.m_pastTargets.Add(this.m_currentTarget);
            }
            return true;
        }

        public override string GetName() => 
            "Cestmir";

        private bool GetNextTarget()
        {
            float num;
            IMyEntity entity;
            if (this.m_smartPath == null)
            {
                return false;
            }
            this.m_smartPath.GetNextTarget(this.m_currentTarget, out this.m_currentTarget, out num, out entity);
            this.m_pastTargets.Add(this.m_currentTarget);
            return true;
        }

        public override bool HandleInput() => 
            ((MySession.Static != null) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogPrefabCheat) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogRemoveTriangle) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogViewEdge) ? base.HandleInput() : false) : false) : false) : false);

        private bool NextBin()
        {
            BinIndex++;
            return true;
        }

        private bool PrevBin()
        {
            BinIndex--;
            if (BinIndex < -1)
            {
                BinIndex = -1;
            }
            return true;
        }

        private static void Raycast(out Vector3D? firstHit, out IMyEntity entity)
        {
            MyCamera mainCamera = MySector.MainCamera;
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(mainCamera.Position, mainCamera.Position + (mainCamera.ForwardVector * 1000f), toList, 0);
            if (toList.Count > 0)
            {
                firstHit = new Vector3D?(toList[0].Position);
                entity = toList[0].HkHitInfo.GetHitEntity();
            }
            else
            {
                firstHit = 0;
                entity = null;
            }
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

        private unsafe bool Test()
        {
            Vector3D forwardVector;
            Vector3D vectord2;
            Vector3D right;
            Vector3D up;
            Matrix worldMatrix;
            Plane plane;
            bool flag;
            bool flag2;
            List<Vector3> list;
            float num3;
            int num = 0;
            goto TR_0039;
        TR_001C:
            num3 = 0f;
            int num13 = 0;
            while (true)
            {
                if (num13 >= list.Count)
                {
                    if (num3 < 0f)
                    {
                        flag2 = false;
                    }
                    break;
                }
                Vector3 vector6 = list[num13];
                Vector3 vector7 = list[(num13 + 1) % list.Count];
                num3 += (vector7.X - vector6.X) * (vector7.Y + vector6.Y);
                num13++;
            }
        TR_0036:
            while (true)
            {
                if (flag | flag2)
                {
                    int num5;
                    flag = false;
                    flag2 = true;
                    list.Clear();
                    int num4 = 0;
                    while (true)
                    {
                        if (num4 >= 6)
                        {
                            num5 = 0;
                            break;
                        }
                        Vector3 item = (Vector3) MyUtils.GetRandomDiscPosition(ref vectord2, 4.5, ref right, ref up);
                        list.Add(item);
                        num4++;
                    }
                    while (true)
                    {
                        while (true)
                        {
                            if (num5 < list.Count)
                            {
                                Line line = new Line(list[num5], list[(num5 + 1) % list.Count], true);
                                Vector3 vector2 = Vector3.Normalize(line.Direction);
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (((Math.Abs((int) (i - num5)) > 1) && ((i != 0) || (num5 != (list.Count - 1)))) && ((num5 != 0) || (i != (list.Count - 1))))
                                    {
                                        Vector3 vector3 = list[i] - list[num5];
                                        Vector3 vector4 = list[(i + 1) % list.Count] - list[num5];
                                        if (Vector3.Dot(Vector3.Cross(vector3, vector2), Vector3.Cross(vector4, vector2)) < 0f)
                                        {
                                            float num8 = Vector3.Dot(vector4, vector2);
                                            float num9 = Vector3.Reject(vector3, vector2).Length();
                                            float num10 = Vector3.Reject(vector4, vector2).Length();
                                            float num11 = num9 + num10;
                                            float num12 = (Vector3.Dot(vector3, vector2) * (num10 / num11)) + (num8 * (num9 / num11));
                                            if ((num12 <= line.Length) && (num12 >= 0f))
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                goto TR_001C;
                            }
                            break;
                        }
                        if (flag)
                        {
                            goto TR_001C;
                        }
                        else
                        {
                            num5++;
                        }
                    }
                }
                else
                {
                    using (List<Vector3>.Enumerator enumerator2 = list.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            AddDebugPoint(enumerator2.Current, Color.Yellow);
                        }
                    }
                    MyWingedEdgeMesh mesh = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "StoneCube")).NavigationDefinition.Mesh.Mesh.Copy();
                    mesh.Transform(worldMatrix);
                    HashSet<int> set = new HashSet<int>();
                    MyWingedEdgeMesh.EdgeEnumerator edges = mesh.GetEdges(null);
                    List<Vector3> loop = new List<Vector3>();
                    while (true)
                    {
                        if (edges.MoveNext())
                        {
                            Vector3 vector8;
                            int currentIndex = edges.CurrentIndex;
                            if (set.Contains(currentIndex))
                            {
                                continue;
                            }
                            MyWingedEdgeMesh.Edge edge = mesh.GetEdge(currentIndex);
                            loop.Clear();
                            if (!mesh.IntersectEdge(ref edge, ref plane, out vector8))
                            {
                                continue;
                            }
                            loop.Add(vector8);
                            int num15 = currentIndex;
                            int leftFace = edge.LeftFace;
                            currentIndex = edge.GetNextFaceEdge(leftFace);
                            for (edge = mesh.GetEdge(currentIndex); currentIndex != num15; edge = mesh.GetEdge(currentIndex))
                            {
                                if (mesh.IntersectEdge(ref edge, ref plane, out vector8))
                                {
                                    leftFace = edge.OtherFace(leftFace);
                                    if (Vector3.DistanceSquared(loop[loop.Count - 1], vector8) > 1E-06f)
                                    {
                                        loop.Add(vector8);
                                    }
                                }
                                currentIndex = edge.GetNextFaceEdge(leftFace);
                            }
                        }
                        edges.Dispose();
                        DebugDrawMesh = mesh;
                        new List<int>().Clear();
                        MyPolygon item = new MyPolygon(plane);
                        item.AddLoop(loop);
                        DebugDrawPolys.Add(item);
                        MyPolygon polygon2 = new MyPolygon(plane);
                        polygon2.AddLoop(list);
                        DebugDrawPolys.Add(polygon2);
                        MyPolygon polygon3 = MyPolygonBoolOps.Static.Difference(item, polygon2);
                        Matrix transformationMatrix = Matrix.CreateTranslation((Vector3) (forwardVector * -1.0));
                        polygon3.Transform(ref transformationMatrix);
                        DebugDrawPolys.Add(polygon3);
                        num++;
                        break;
                    }
                }
                break;
            }
        TR_0039:
            while (true)
            {
                if (num >= 1)
                {
                    return true;
                }
                ClearDebugPoints();
                DebugDrawPolys.Clear();
                float num2 = 8f;
                forwardVector = MySector.MainCamera.ForwardVector;
                vectord2 = MySector.MainCamera.Position + (forwardVector * num2);
                right = MySector.MainCamera.WorldMatrix.Right;
                up = MySector.MainCamera.WorldMatrix.Up;
                worldMatrix = (Matrix) MySector.MainCamera.WorldMatrix;
                Matrix* matrixPtr1 = (Matrix*) ref worldMatrix;
                matrixPtr1.Translation += forwardVector * num2;
                plane = new Plane((Vector3) vectord2, (Vector3) forwardVector);
                DebugDrawPoint item = new DebugDrawPoint {
                    Position = vectord2,
                    Color = Color.Pink
                };
                DebugDrawPoints.Add(item);
                item = new DebugDrawPoint {
                    Position = vectord2 + forwardVector,
                    Color = Color.Pink
                };
                DebugDrawPoints.Add(item);
                flag = true;
                flag2 = true;
                list = new List<Vector3>();
                break;
            }
            goto TR_0036;
        }

        private bool Test2()
        {
            Plane polygonPlane = new Plane(Vector3.Forward, 0f);
            if (m_testList == null)
            {
                m_testList = new List<Tuple<Vector2[], Vector2[]>>();
                Vector2[] vectorArray1 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray4 = new Vector2[] { new Vector2(1f, 1f), new Vector2(1f, 3f), new Vector2(3f, 3f), new Vector2(3f, 1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray1, vectorArray4));
                Vector2[] vectorArray5 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray6 = new Vector2[] { new Vector2(-1f, 1f), new Vector2(-1f, 3f), new Vector2(1f, 3f), new Vector2(1f, 1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray5, vectorArray6));
                Vector2[] vectorArray7 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray8 = new Vector2[] { new Vector2(-1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(1f, -1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray7, vectorArray8));
                Vector2[] vectorArray9 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray10 = new Vector2[] { new Vector2(1f, -1f), new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(3f, -1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray9, vectorArray10));
                Vector2[] vectorArray11 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray12 = new Vector2[] { new Vector2(-1f, 0f), new Vector2(-1f, 2f), new Vector2(1f, 2f), new Vector2(1f, 0f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray11, vectorArray12));
                Vector2[] vectorArray13 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray14 = new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 2f), new Vector2(3f, 2f), new Vector2(3f, 0f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray13, vectorArray14));
                Vector2[] vectorArray15 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray16 = new Vector2[] { new Vector2(0f, 1f), new Vector2(0f, 3f), new Vector2(2f, 3f), new Vector2(2f, 1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray15, vectorArray16));
                Vector2[] vectorArray17 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray18 = new Vector2[] { new Vector2(0f, -1f), new Vector2(0f, 1f), new Vector2(2f, 1f), new Vector2(2f, -1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray17, vectorArray18));
                Vector2[] vectorArray19 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray20 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(2f, 2f), new Vector2(2f, 0f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray19, vectorArray20));
                Vector2[] vectorArray21 = new Vector2[] { new Vector2(0f, 0f), new Vector2(-1f, 1f), new Vector2(0f, 2f), new Vector2(1f, 1f) };
                Vector2[] vectorArray22 = new Vector2[] { new Vector2(-2f, 1.3f), new Vector2(-2f, 2.3f), new Vector2(2f, 2.7f), new Vector2(2f, 1.7f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray21, vectorArray22));
                Vector2[] vectorArray23 = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 5f), new Vector2(3f, 2f), new Vector2(4f, 4f), new Vector2(5f, 1f) };
                Vector2[] vectorArray24 = new Vector2[] { new Vector2(-1f, 4f), new Vector2(1f, 7f), new Vector2(6f, 4f), new Vector2(5f, 3f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray23, vectorArray24));
                Vector2[] vectorArray25 = new Vector2[] { new Vector2(0f, 3f), new Vector2(4f, 7f), new Vector2(9f, 8f), new Vector2(5f, 2f), new Vector2(2f, 0f) };
                Vector2[] vectorArray26 = new Vector2[] { new Vector2(0f, 9f), new Vector2(4f, 12f), new Vector2(7f, 9f), new Vector2(9f, 1f), new Vector2(4f, 9f), new Vector2(2f, 4f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray25, vectorArray26));
                Vector2[] vectorArray27 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 4.1f), new Vector2(4f, 4f), new Vector2(4f, 0.1f) };
                Vector2[] vectorArray28 = new Vector2[] { new Vector2(2f, 1f), new Vector2(1f, 2f), new Vector2(2f, 3f), new Vector2(3f, 2f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray27, vectorArray28));
                Vector2[] vectorArray29 = new Vector2[] { new Vector2(3f, 0f), new Vector2(0f, 3f), new Vector2(3f, 6f), new Vector2(6f, 3f) };
                Vector2[] vectorArray30 = new Vector2[] { new Vector2(6f, 7f), new Vector2(8f, 5f), new Vector2(5f, 2f), new Vector2(3f, 4f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray29, vectorArray30));
                Vector2[] vectorArray31 = new Vector2[] { new Vector2(3f, 0f), new Vector2(0f, 3f), new Vector2(3f, 6f), new Vector2(6f, 3f) };
                Vector2[] vectorArray32 = new Vector2[] { new Vector2(6f, 3f), new Vector2(3f, 6f), new Vector2(6f, 7f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray31, vectorArray32));
                Vector2[] vectorArray33 = new Vector2[] { new Vector2(0f, 0f), new Vector2(-2f, 2f), new Vector2(0f, 4f), new Vector2(2f, 2f) };
                Vector2[] vectorArray34 = new Vector2[] { new Vector2(0f, 0f), new Vector2(-1f, 1f), new Vector2(0f, 2f), new Vector2(1f, 1f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray33, vectorArray34));
                Vector2[] vectorArray35 = new Vector2[] { new Vector2(0f, 0f), new Vector2(-2f, 2f), new Vector2(0f, 4f), new Vector2(2f, 2f) };
                Vector2[] vectorArray36 = new Vector2[] { new Vector2(1f, 1f), new Vector2(0f, 2f), new Vector2(1f, 3f), new Vector2(2f, 2f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray35, vectorArray36));
                Vector2[] vectorArray37 = new Vector2[] { new Vector2(0f, 0f), new Vector2(-2f, 2f), new Vector2(0f, 4f), new Vector2(2f, 2f) };
                Vector2[] vectorArray38 = new Vector2[] { new Vector2(0f, 2f), new Vector2(-1f, 3f), new Vector2(0f, 4f), new Vector2(1f, 3f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray37, vectorArray38));
                Vector2[] vectorArray39 = new Vector2[] { new Vector2(0f, 0f), new Vector2(-2f, 2f), new Vector2(0f, 4f), new Vector2(2f, 2f) };
                Vector2[] vectorArray40 = new Vector2[] { new Vector2(-1f, 1f), new Vector2(-2f, 2f), new Vector2(-1f, 3f), new Vector2(0f, 2f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray39, vectorArray40));
                Vector2[] vectorArray41 = new Vector2[] { new Vector2(2f, 0f), new Vector2(0f, 2f), new Vector2(4f, 6f), new Vector2(0f, 10f), new Vector2(2f, 12f), new Vector2(4f, 10f), new Vector2(0f, 6f), new Vector2(4f, 2f) };
                Vector2[] vectorArray42 = new Vector2[] { new Vector2(1f, 2f), new Vector2(1f, 8f), new Vector2(3f, 10f), new Vector2(3f, 4f) };
                m_testList.Add(new Tuple<Vector2[], Vector2[]>(vectorArray41, vectorArray42));
            }
            DebugDrawPolys.Clear();
            m_prevTestIndex = m_testIndex;
            m_prevTestOperation = m_testOperation;
            Vector2[] vectorArray43 = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 4f), new Vector2(4f, 4f), new Vector2(4f, 0f) };
            Vector2[] vectorArray2 = new Vector2[] { new Vector2(1f, 2f), new Vector2(2f, 1f), new Vector2(3f, 2f), new Vector2(2f, 3f) };
            Vector2[] vectorArray3 = new Vector2[] { new Vector2(-1f, 2f), new Vector2(-1f, 5f), new Vector2(5f, 5f), new Vector2(5f, 2f) };
            Tuple<Vector2[], Vector2[]> local1 = m_testList[m_testIndex];
            MyPolygon item = new MyPolygon(polygonPlane);
            MyPolygon polygon2 = new MyPolygon(polygonPlane);
            item.AddLoop(new List<Vector3>(from i in vectorArray43 select new Vector3(i.X, i.Y, 0f)));
            item.AddLoop(new List<Vector3>(from i in vectorArray2 select new Vector3(i.X, i.Y, 0f)));
            polygon2.AddLoop(new List<Vector3>(from i in vectorArray3 select new Vector3(i.X, i.Y, 0f)));
            DebugDrawPolys.Add(item);
            DebugDrawPolys.Add(polygon2);
            TimeSpan span2 = new TimeSpan();
            MyPolygon polygon3 = null;
            polygon3 = (m_testOperation != 0) ? ((m_testOperation != 1) ? ((m_testOperation != 2) ? ((m_testOperation != 3) ? ((m_testOperation != 4) ? MyPolygonBoolOps.Static.Difference(polygon2, item) : MyPolygonBoolOps.Static.Union(polygon2, item)) : MyPolygonBoolOps.Static.Intersection(polygon2, item)) : MyPolygonBoolOps.Static.Difference(item, polygon2)) : MyPolygonBoolOps.Static.Union(item, polygon2)) : MyPolygonBoolOps.Static.Intersection(item, polygon2);
            TimeSpan local5 = span2 + Stopwatch.StartNew().Elapsed;
            Matrix transformationMatrix = Matrix.CreateTranslation(Vector3.Right * 12f);
            polygon3.Transform(ref transformationMatrix);
            DebugDrawPolys.Add(polygon3);
            m_testIndex++;
            m_testIndex = 0;
            m_testOperation = (m_testOperation + 1) % 6;
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCestmirDebugInputComponent.<>c <>9 = new MyCestmirDebugInputComponent.<>c();
            public static Func<string> <>9__26_0;
            public static Func<string> <>9__26_1;
            public static Func<string> <>9__26_2;
            public static Func<string> <>9__26_3;
            public static Func<string> <>9__26_4;
            public static Func<string> <>9__26_5;
            public static Func<string> <>9__26_6;
            public static Func<string> <>9__26_7;
            public static Func<string> <>9__26_8;
            public static Func<string> <>9__26_9;
            public static Func<string> <>9__26_10;
            public static Func<string> <>9__26_11;
            public static Func<bool> <>9__26_12;
            public static Func<string> <>9__26_13;
            public static Func<bool> <>9__26_14;
            public static Func<string> <>9__26_15;
            public static Func<bool> <>9__26_16;
            public static Func<string> <>9__26_17;
            public static Func<bool> <>9__26_18;
            public static Func<string> <>9__26_19;
            public static Func<bool> <>9__26_20;
            public static Func<string> <>9__26_21;
            public static Func<Vector2, Vector3> <>9__39_0;
            public static Func<Vector2, Vector3> <>9__39_1;
            public static Func<Vector2, Vector3> <>9__39_2;

            internal string <.ctor>b__26_0() => 
                "Add prefab...";

            internal string <.ctor>b__26_1() => 
                "Copy target grid position to clipboard";

            internal string <.ctor>b__26_10() => 
                "Test";

            internal string <.ctor>b__26_11() => 
                "Next funnel segment";

            internal bool <.ctor>b__26_12()
            {
                MyNavigationMesh.m_debugFunnelIdx++;
                return true;
            }

            internal string <.ctor>b__26_13() => 
                "Previous funnel segment";

            internal bool <.ctor>b__26_14()
            {
                if (MyNavigationMesh.m_debugFunnelIdx > 0)
                {
                    MyNavigationMesh.m_debugFunnelIdx--;
                }
                return true;
            }

            internal string <.ctor>b__26_15() => 
                "Remove navmesh tri...";

            internal bool <.ctor>b__26_16()
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenDialogRemoveTriangle());
                return true;
            }

            internal string <.ctor>b__26_17() => 
                "View all navmesh edges";

            internal bool <.ctor>b__26_18()
            {
                MyWingedEdgeMesh.DebugDrawEdgesReset();
                return true;
            }

            internal string <.ctor>b__26_19() => 
                "View single navmesh edge...";

            internal string <.ctor>b__26_2() => 
                "Next navmesh connection helper bin";

            internal bool <.ctor>b__26_20()
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenDialogViewEdge());
                return true;
            }

            internal string <.ctor>b__26_21() => 
                "Place an environment item in front of the player";

            internal string <.ctor>b__26_3() => 
                "Prev navmesh connection helper bin";

            internal string <.ctor>b__26_4() => 
                "Add bot";

            internal string <.ctor>b__26_5() => 
                "Remove bot";

            internal string <.ctor>b__26_6() => 
                "Find path for first bot";

            internal string <.ctor>b__26_7() => 
                "Find path between points";

            internal string <.ctor>b__26_8() => 
                "Find smart path between points";

            internal string <.ctor>b__26_9() => 
                "Get next smart path target";

            internal Vector3 <Test2>b__39_0(Vector2 i) => 
                new Vector3(i.X, i.Y, 0f);

            internal Vector3 <Test2>b__39_1(Vector2 i) => 
                new Vector3(i.X, i.Y, 0f);

            internal Vector3 <Test2>b__39_2(Vector2 i) => 
                new Vector3(i.X, i.Y, 0f);
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

        private class Vector3Comparer : IComparer<Vector3>
        {
            private Vector3 m_right;
            private Vector3 m_up;

            public Vector3Comparer(Vector3 right, Vector3 up)
            {
                this.m_right = right;
                this.m_up = up;
            }

            public int Compare(Vector3 x, Vector3 y)
            {
                float num;
                float num2;
                Vector3.Dot(ref x, ref this.m_right, out num2);
                Vector3.Dot(ref y, ref this.m_right, out num);
                float num3 = num2 - num;
                if (num3 < 0f)
                {
                    return -1;
                }
                if (num3 > 0f)
                {
                    return 1;
                }
                Vector3.Dot(ref x, ref this.m_up, out num2);
                Vector3.Dot(ref y, ref this.m_up, out num);
                num3 = num2 - num;
                return ((num3 >= 0f) ? ((num3 <= 0f) ? 0 : 1) : -1);
            }
        }
    }
}

