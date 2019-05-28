namespace Sandbox.Game.GUI.DebugInputComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using Sandbox.Engine.Voxels.Storage;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Input;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyVoxelDebugInputComponent : MyMultiDebugInputComponent
    {
        private MyDebugComponent[] m_components;

        public MyVoxelDebugInputComponent()
        {
            this.m_components = new MyDebugComponent[] { new IntersectBBComponent(this), new IntersectRayComponent(this), new ToolsComponent(this), new StorageWriteCacheComponent(this), new PhysicsComponent(this) };
        }

        public override string GetName() => 
            "Voxels";

        public override MyDebugComponent[] Components =>
            this.m_components;

        private class IntersectBBComponent : MyDebugComponent
        {
            private MyVoxelDebugInputComponent m_comp;
            private bool m_moveProbe = true;
            private bool m_showVoxelProbe;
            private byte m_valueToSet = 0x80;
            private ProbeMode m_mode = ProbeMode.Intersect;
            private float m_probeSize = 1f;
            private int m_probeLod;
            private List<MyVoxelBase> m_voxels = new List<MyVoxelBase>();
            private MyStorageData m_target = new MyStorageData(MyStorageDataTypeFlags.All);
            private MyVoxelBase m_probedVoxel;
            private Vector3 m_probePosition;

            public IntersectBBComponent(MyVoxelDebugInputComponent comp)
            {
                this.m_comp = comp;
                this.AddShortcut(MyKeys.OemOpenBrackets, true, false, false, false, () => "Toggle voxel probe box.", () => this.ToggleProbeBox());
                this.AddShortcut(MyKeys.OemCloseBrackets, true, false, false, false, () => "Toggle probe mode", () => this.SwitchProbeMode());
                this.AddShortcut(MyKeys.OemBackslash, true, false, false, false, () => "Freeze/Unfreeze probe", () => this.FreezeProbe());
                this.AddShortcut(MyKeys.OemSemicolon, true, false, false, false, () => "Increase Probe Size.", () => this.ResizeProbe(1, 0));
                this.AddShortcut(MyKeys.OemSemicolon, true, true, false, false, () => "Decrease Probe Size.", () => this.ResizeProbe(-1, 0));
                this.AddShortcut(MyKeys.OemSemicolon, true, false, true, false, () => "Increase Probe Size (x128).", () => this.ResizeProbe(0x80, 0));
                this.AddShortcut(MyKeys.OemSemicolon, true, true, true, false, () => "Decrease Probe Size (x128).", () => this.ResizeProbe(-128, 0));
                this.AddShortcut(MyKeys.OemQuotes, true, false, false, false, () => "Increase LOD Size.", () => this.ResizeProbe(0, 1));
                this.AddShortcut(MyKeys.OemQuotes, true, true, false, false, () => "Decrease LOD Size.", () => this.ResizeProbe(0, -1));
            }

            private Color ColorForContainment(ContainmentType cont) => 
                ((cont == ContainmentType.Disjoint) ? Color.Green : ((cont == ContainmentType.Contains) ? Color.Yellow : Color.Red));

            public override void Draw()
            {
                base.Draw();
                if ((MySession.Static != null) && this.m_showVoxelProbe)
                {
                    float num = this.m_probeSize * 0.5f;
                    int probeLod = this.m_probeLod;
                    if (this.m_moveProbe)
                    {
                        this.m_probePosition = ((Vector3) MySector.MainCamera.Position) + ((MySector.MainCamera.ForwardVector * this.m_probeSize) * 3f);
                    }
                    BoundingBox box = new BoundingBox(this.m_probePosition - num, this.m_probePosition + num);
                    BoundingBoxD xd = box;
                    this.m_voxels.Clear();
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref xd, this.m_voxels);
                    MyVoxelBase rootVoxel = null;
                    double positiveInfinity = double.PositiveInfinity;
                    foreach (MyVoxelBase base3 in this.m_voxels)
                    {
                        MatrixD worldMatrix = base3.WorldMatrix;
                        double num3 = Vector3D.Distance(worldMatrix.Translation, this.m_probePosition);
                        if (num3 < positiveInfinity)
                        {
                            positiveInfinity = num3;
                            rootVoxel = base3;
                        }
                    }
                    ContainmentType disjoint = ContainmentType.Disjoint;
                    if (rootVoxel == null)
                    {
                        base.Section("No Voxel Found", Array.Empty<object>());
                        object[] arguments = new object[] { this.m_mode };
                        base.Text("Probe mode: {0}", arguments);
                        object[] objArray7 = new object[] { this.m_probeSize };
                        base.Text("Probe Size: {0}", objArray7);
                    }
                    else
                    {
                        rootVoxel = rootVoxel.RootVoxel;
                        Vector3 vector = ((Vector3) Vector3.Transform(this.m_probePosition, rootVoxel.PositionComp.WorldMatrixInvScaled)) + rootVoxel.SizeInMetresHalf;
                        box = new BoundingBox(vector - num, vector + num);
                        this.m_probedVoxel = rootVoxel;
                        object[] formatArgs = new object[] { rootVoxel.StorageName, rootVoxel.GetType().Name };
                        base.Section("Probing {1}: {0}", formatArgs);
                        object[] arguments = new object[] { this.m_mode };
                        base.Text("Probe mode: {0}", arguments);
                        if (this.m_mode == ProbeMode.Intersect)
                        {
                            object[] objArray3 = new object[] { vector };
                            base.Text("Local Pos: {0}", objArray3);
                            object[] objArray4 = new object[] { this.m_probeSize };
                            base.Text("Probe Size: {0}", objArray4);
                            disjoint = rootVoxel.Storage.Intersect(ref box, false);
                            object[] objArray5 = new object[] { disjoint.ToString() };
                            base.Text("Result: {0}", objArray5);
                            xd = box;
                        }
                    }
                    Color color = this.ColorForContainment(disjoint);
                    if (rootVoxel != null)
                    {
                        MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(xd.Translate(-rootVoxel.SizeInMetresHalf), rootVoxel.WorldMatrix), color, 0.5f, true, false, false);
                    }
                    else
                    {
                        MyRenderProxy.DebugDrawAABB(xd, color, 0.5f, 1f, true, false, false);
                    }
                }
            }

            private void DrawContentsInfo(MyStorageData data)
            {
                uint num3 = 0;
                uint num4 = 0;
                uint num5 = 0;
                byte num = 0xff;
                byte num2 = 0;
                int num6 = data.SizeLinear / data.StepLinear;
                for (int i = 0; i < data.SizeLinear; i += data.StepLinear)
                {
                    byte num8 = data.Content(i);
                    if (num > num8)
                    {
                        num = num8;
                    }
                    if (num2 < num8)
                    {
                        num2 = num8;
                    }
                    num3 += num8;
                    if (num8 != 0)
                    {
                        num4++;
                    }
                    if (num8 != 0xff)
                    {
                        num5++;
                    }
                }
                object[] formatArgs = new object[] { num6, (num6 > 1) ? "voxels" : "voxel" };
                this.Section("Probing Contents ({0} {1})", formatArgs);
                object[] arguments = new object[] { num };
                base.Text("Min: {0}", arguments);
                object[] objArray3 = new object[] { (long) (((ulong) num3) / ((long) num6)) };
                base.Text("Average: {0}", objArray3);
                object[] objArray4 = new object[] { num2 };
                base.Text("Max: {0}", objArray4);
                base.VSpace(5f);
                object[] objArray5 = new object[] { num4 };
                base.Text("Non-Empty: {0}", objArray5);
                object[] objArray6 = new object[] { num5 };
                base.Text("Non-Full: {0}", objArray6);
            }

            private unsafe void DrawMaterialsInfo(MyStorageData data)
            {
                int* numPtr = (int*) stackalloc byte[0x400];
                int num = data.SizeLinear / data.StepLinear;
                for (int i = 0; i < data.SizeLinear; i += data.StepLinear)
                {
                    byte num3 = data.Material(i);
                    int* numPtr1 = numPtr + num3;
                    numPtr1[0]++;
                }
                object[] formatArgs = new object[] { num, (num > 1) ? "voxels" : "voxel" };
                this.Section("Probing Materials ({0} {1})", formatArgs);
                List<MatInfo> list = new List<MatInfo>();
                for (int j = 0; j < 0x100; j++)
                {
                    if (numPtr[j] > 0)
                    {
                        MatInfo item = new MatInfo {
                            Material = (byte) j,
                            Count = numPtr[j]
                        };
                        list.Add(item);
                    }
                }
                list.Sort();
                int voxelMaterialCount = MyDefinitionManager.Static.VoxelMaterialCount;
            }

            private bool FreezeProbe()
            {
                this.m_moveProbe = !this.m_moveProbe;
                return true;
            }

            public override string GetName() => 
                "Intersect BB";

            public override bool HandleInput()
            {
                int num = MyInput.Static.DeltaMouseScrollWheelValue();
                if (((num == 0) || !MyInput.Static.IsAnyCtrlKeyPressed()) || !this.m_showVoxelProbe)
                {
                    return base.HandleInput();
                }
                this.m_valueToSet = (byte) (this.m_valueToSet + ((byte) (num / 120)));
                return true;
            }

            private bool ResizeProbe(int sizeDelta, int lodDelta)
            {
                this.m_probeLod = MathHelper.Clamp(this.m_probeLod + lodDelta, 0, 0x10);
                this.m_probeSize = (this.m_mode == ProbeMode.Intersect) ? MathHelper.Clamp(this.m_probeSize + (sizeDelta << (this.m_probeLod & 0x1f)), 1f, float.PositiveInfinity) : MathHelper.Clamp(this.m_probeSize + (sizeDelta << (this.m_probeLod & 0x1f)), (float) (1 << (this.m_probeLod & 0x1f)), (float) (0x20 * (1 << (this.m_probeLod & 0x1f))));
                return true;
            }

            private bool SwitchProbeMode()
            {
                this.m_mode = (this.m_mode + 1) % (ProbeMode.Intersect | ProbeMode.Material);
                this.ResizeProbe(0, 0);
                return true;
            }

            private bool ToggleProbeBox()
            {
                this.m_showVoxelProbe = !this.m_showVoxelProbe;
                this.ResizeProbe(0, 0);
                return true;
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyVoxelDebugInputComponent.IntersectBBComponent.<>c <>9 = new MyVoxelDebugInputComponent.IntersectBBComponent.<>c();
                public static Func<string> <>9__12_0;
                public static Func<string> <>9__12_2;
                public static Func<string> <>9__12_4;
                public static Func<string> <>9__12_6;
                public static Func<string> <>9__12_8;
                public static Func<string> <>9__12_10;
                public static Func<string> <>9__12_12;
                public static Func<string> <>9__12_14;
                public static Func<string> <>9__12_16;

                internal string <.ctor>b__12_0() => 
                    "Toggle voxel probe box.";

                internal string <.ctor>b__12_10() => 
                    "Increase Probe Size (x128).";

                internal string <.ctor>b__12_12() => 
                    "Decrease Probe Size (x128).";

                internal string <.ctor>b__12_14() => 
                    "Increase LOD Size.";

                internal string <.ctor>b__12_16() => 
                    "Decrease LOD Size.";

                internal string <.ctor>b__12_2() => 
                    "Toggle probe mode";

                internal string <.ctor>b__12_4() => 
                    "Freeze/Unfreeze probe";

                internal string <.ctor>b__12_6() => 
                    "Increase Probe Size.";

                internal string <.ctor>b__12_8() => 
                    "Decrease Probe Size.";
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MatInfo : IComparable<MyVoxelDebugInputComponent.IntersectBBComponent.MatInfo>
            {
                public byte Material;
                public int Count;
                public int CompareTo(MyVoxelDebugInputComponent.IntersectBBComponent.MatInfo other) => 
                    (this.Count - other.Count);
            }

            private enum ProbeMode
            {
                Content,
                Material,
                Intersect
            }
        }

        private class IntersectRayComponent : MyDebugComponent
        {
            private MyVoxelDebugInputComponent m_comp;
            private bool m_moveProbe = true;
            private bool m_showVoxelProbe;
            private float m_rayLength = 25f;
            private MyVoxelBase m_probedVoxel;
            private LineD m_probedLine;
            private Vector3D m_forward;
            private Vector3D m_up;
            private int m_probeCount = 1;
            private float m_probeGap = 1f;

            public IntersectRayComponent(MyVoxelDebugInputComponent comp)
            {
                this.m_comp = comp;
                this.AddShortcut(MyKeys.OemOpenBrackets, true, false, false, false, () => "Toggle voxel probe ray.", () => this.ToggleProbeRay());
                this.AddShortcut(MyKeys.OemBackslash, true, false, false, false, () => "Freeze/Unfreeze probe", () => this.FreezeProbe());
            }

            public override void Draw()
            {
                base.Draw();
                if ((MySession.Static != null) && this.m_showVoxelProbe)
                {
                    base.Text("Probe Controlls:", Array.Empty<object>());
                    base.Text("\tCtrl + Mousewheel: Chage probe size", Array.Empty<object>());
                    base.Text("\tCtrl + Shift+Mousewheel: Chage probe size (x10)", Array.Empty<object>());
                    base.Text("\tN + Mousewheel: Chage probe count", Array.Empty<object>());
                    base.Text("\tG + Mousewheel: Chage probe gap", Array.Empty<object>());
                    object[] arguments = new object[] { this.m_rayLength };
                    base.Text("Probe Size: {0}", arguments);
                    object[] objArray2 = new object[] { this.m_probeCount * this.m_probeCount };
                    base.Text("Probe Count: {0}", objArray2);
                    if (this.m_moveProbe)
                    {
                        this.m_up = MySector.MainCamera.UpVector;
                        this.m_forward = MySector.MainCamera.ForwardVector;
                        Vector3D from = (MySector.MainCamera.Position - (this.m_up * 0.5)) + (this.m_forward * 0.5);
                        this.m_probedLine = new LineD(from, from + (this.m_rayLength * this.m_forward));
                    }
                    Vector3D vectord = Vector3D.Cross(this.m_forward, this.m_up);
                    float num = ((float) this.m_probeCount) / 2f;
                    int num2 = 0;
                    while (num2 < this.m_probeCount)
                    {
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= this.m_probeCount)
                            {
                                num2++;
                                break;
                            }
                            Vector3D pos = (Vector3D) ((((num2 - num) * this.m_probeGap) * vectord) + (((num3 - num) * this.m_probeGap) * this.m_up));
                            this.Probe(pos);
                            num3++;
                        }
                    }
                }
            }

            private bool FreezeProbe()
            {
                this.m_moveProbe = !this.m_moveProbe;
                return true;
            }

            public override string GetName() => 
                "Intersect Ray";

            public override bool HandleInput()
            {
                int num = MyInput.Static.DeltaMouseScrollWheelValue();
                if ((num != 0) && this.m_showVoxelProbe)
                {
                    if (MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        this.m_rayLength = !MyInput.Static.IsAnyShiftKeyPressed() ? (this.m_rayLength + (((float) num) / 120f)) : (this.m_rayLength + (((float) num) / 12f));
                        this.m_probedLine.To = this.m_probedLine.From + (this.m_rayLength * this.m_probedLine.Direction);
                        this.m_probedLine.Length = this.m_rayLength;
                        return true;
                    }
                    if (MyInput.Static.IsKeyPress(MyKeys.G))
                    {
                        this.m_probeGap = MathHelper.Clamp((float) (this.m_probeGap + (((float) num) / 240f)), (float) 0.5f, (float) 32f);
                        return true;
                    }
                    if (MyInput.Static.IsKeyPress(MyKeys.N))
                    {
                        this.m_probeCount = MathHelper.Clamp(this.m_probeCount + (num / 120), 1, 0x21);
                        return true;
                    }
                }
                return base.HandleInput();
            }

            private unsafe void Probe(Vector3D pos)
            {
                LineD probedLine = this.m_probedLine;
                Vector3D* vectordPtr1 = (Vector3D*) ref probedLine.From;
                vectordPtr1[0] += pos;
                Vector3D* vectordPtr2 = (Vector3D*) ref probedLine.To;
                vectordPtr2[0] += pos;
                List<MyLineSegmentOverlapResult<MyEntity>> list = new List<MyLineSegmentOverlapResult<MyEntity>>();
                MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref probedLine, list, MyEntityQueryType.Static);
                double positiveInfinity = double.PositiveInfinity;
                foreach (MyLineSegmentOverlapResult<MyEntity> result in list)
                {
                    MyVoxelBase element = result.Element as MyVoxelBase;
                    if ((element != null) && (result.Distance < positiveInfinity))
                    {
                        this.m_probedVoxel = element;
                    }
                }
                if (this.m_probedVoxel is MyVoxelPhysics)
                {
                    this.m_probedVoxel = ((MyVoxelPhysics) this.m_probedVoxel).Parent;
                }
                if ((this.m_probedVoxel == null) || (this.m_probedVoxel.Storage.DataProvider == null))
                {
                    if (this.m_probeCount == 1)
                    {
                        base.Text(Color.Yellow, 1.5f, "No voxel found", Array.Empty<object>());
                    }
                    MyRenderProxy.DebugDrawLine3D(probedLine.From, probedLine.To, Color.Yellow, Color.Yellow, true, false);
                }
                else
                {
                    double num2;
                    double num3;
                    MyRenderProxy.DebugDrawLine3D(probedLine.From, probedLine.To, Color.Green, Color.Green, true, false);
                    Vector3D from = Vector3D.Transform(probedLine.From, this.m_probedVoxel.PositionComp.WorldMatrixInvScaled) + this.m_probedVoxel.SizeInMetresHalf;
                    LineD line = new LineD(from, Vector3D.Transform(probedLine.To, this.m_probedVoxel.PositionComp.WorldMatrixInvScaled) + this.m_probedVoxel.SizeInMetresHalf);
                    bool flag = this.m_probedVoxel.Storage.DataProvider.Intersect(ref line, out num2, out num3);
                    Vector3D vectord3 = line.From;
                    LineD* edPtr1 = (LineD*) ref line;
                    edPtr1->From = vectord3 + ((line.Direction * line.Length) * num2);
                    LineD* edPtr2 = (LineD*) ref line;
                    edPtr2->To = vectord3 + ((line.Direction * line.Length) * num3);
                    if (this.m_probeCount == 1)
                    {
                        object[] arguments = new object[] { this.m_probedVoxel.StorageName, this.m_probedVoxel.EntityId };
                        base.Text(Color.Yellow, 1.5f, "Probing voxel map {0}:{1}", arguments);
                        object[] objArray2 = new object[] { from };
                        base.Text("Local Pos: {0}", objArray2);
                        object[] objArray3 = new object[] { flag };
                        base.Text("Intersects: {0}", objArray3);
                    }
                    if (flag)
                    {
                        MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(line.From - this.m_probedVoxel.SizeInMetresHalf, this.m_probedVoxel.PositionComp.WorldMatrix), Vector3D.Transform(line.To - this.m_probedVoxel.SizeInMetresHalf, this.m_probedVoxel.PositionComp.WorldMatrix), Color.Red, Color.Red, true, false);
                    }
                }
            }

            private bool ToggleProbeRay()
            {
                this.m_showVoxelProbe = !this.m_showVoxelProbe;
                return true;
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyVoxelDebugInputComponent.IntersectRayComponent.<>c <>9 = new MyVoxelDebugInputComponent.IntersectRayComponent.<>c();
                public static Func<string> <>9__10_0;
                public static Func<string> <>9__10_2;

                internal string <.ctor>b__10_0() => 
                    "Toggle voxel probe ray.";

                internal string <.ctor>b__10_2() => 
                    "Freeze/Unfreeze probe";
            }
        }

        public class PhysicsComponent : MyDebugComponent
        {
            private MyVoxelDebugInputComponent m_comp;
            private bool m_debugDraw;
            private ConcurrentCachingList<PredictionInfo> m_list = new ConcurrentCachingList<PredictionInfo>();
            public static MyVoxelDebugInputComponent.PhysicsComponent Static;

            public PhysicsComponent(MyVoxelDebugInputComponent comp)
            {
                this.m_comp = comp;
                Static = this;
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Clear boxes", delegate {
                    this.m_list.ClearList();
                    return false;
                });
            }

            [Conditional("DEBUG")]
            public void Add(MatrixD worldMatrix, BoundingBox box, Vector4I id, MyVoxelBase voxel)
            {
                if (this.m_list.Count > 0x76c)
                {
                    this.m_list.ClearList();
                }
                voxel = voxel.RootVoxel;
                box.Translate(-voxel.SizeInMetresHalf);
                PredictionInfo entity = new PredictionInfo();
                entity.Id = id;
                entity.Bounds = MyOrientedBoundingBoxD.Create(box, voxel.WorldMatrix);
                entity.Body = voxel;
                this.m_list.Add(entity);
            }

            public override void Draw()
            {
                base.Draw();
                if (MySession.Static == null)
                {
                    this.m_list.ClearList();
                }
                this.m_list.ApplyChanges();
                object[] arguments = new object[] { this.m_list.Count };
                base.Text("Queried Out Areas: {0}", arguments);
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, PredictionInfo, List<PredictionInfo>.Enumerator> enumerator = this.m_list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyRenderProxy.DebugDrawOBB(enumerator.Current.Bounds, Color.Cyan, 0.2f, true, true, false);
                    }
                }
            }

            public override string GetName() => 
                "Physics";

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyVoxelDebugInputComponent.PhysicsComponent.<>c <>9 = new MyVoxelDebugInputComponent.PhysicsComponent.<>c();
                public static Func<string> <>9__6_0;

                internal string <.ctor>b__6_0() => 
                    "Clear boxes";
            }

            private class PredictionInfo
            {
                public MyVoxelBase Body;
                public Vector4I Id;
                public MyOrientedBoundingBoxD Bounds;
            }
        }

        private class StorageWriteCacheComponent : MyDebugComponent
        {
            private MyVoxelDebugInputComponent m_comp;
            private bool DisplayDetails;
            private bool DebugDraw;

            public StorageWriteCacheComponent(MyVoxelDebugInputComponent comp)
            {
                this.m_comp = comp;
                this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Toggle detailed details.", delegate {
                    bool flag;
                    this.DisplayDetails = flag = !this.DisplayDetails;
                    return flag;
                });
                this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Toggle debug draw.", delegate {
                    bool flag;
                    this.DebugDraw = flag = !this.DebugDraw;
                    return flag;
                });
                this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Toggle cache writing.", new Func<bool>(this.ToggleWrite));
                this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Toggle cache flushing.", new Func<bool>(this.ToggleFlush));
                this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Toggle cache.", new Func<bool>(this.ToggleCache));
            }

            public override void Draw()
            {
                base.Draw();
                if (MySession.Static != null)
                {
                    MyVoxelOperationsSessionComponent component = MySession.Static.GetComponent<MyVoxelOperationsSessionComponent>();
                    if (component != null)
                    {
                        object[] arguments = new object[] { MyVoxelOperationsSessionComponent.EnableCache };
                        base.Text("Cache Enabled: {0}", arguments);
                        object[] objArray9 = new object[] { component.ShouldWrite ? "Enabled" : "Disabled" };
                        object[] objArray10 = new object[] { component.ShouldWrite ? "Enabled" : "Disabled" };
                        this.Text("Cache Writing: {0}", objArray10);
                        object[] objArray7 = new object[] { component.ShouldFlush ? "Enabled" : "Disabled" };
                        object[] objArray8 = new object[] { component.ShouldFlush ? "Enabled" : "Disabled" };
                        this.Text("Cache Flushing: {0}", objArray8);
                        MyStorageBase[] baseArray = component.QueuedStorages.ToArray<MyStorageBase>();
                        if (baseArray.Length == 0)
                        {
                            base.Text(Color.Orange, "No queued storages.", Array.Empty<object>());
                        }
                        else
                        {
                            object[] objArray2 = new object[] { baseArray.Length };
                            base.Text(Color.Yellow, 1.2f, "{0} Queued storages:", objArray2);
                            foreach (MyStorageBase storage in baseArray)
                            {
                                MyStorageBase.WriteCacheStats stats;
                                storage.GetStats(out stats);
                                object[] objArray3 = new object[] { storage.ToString() };
                                base.Text("Voxel storage {0}:", objArray3);
                                object[] objArray4 = new object[] { stats.QueuedWrites };
                                base.Text(Color.White, 0.9f, "Pending Writes: {0}", objArray4);
                                object[] objArray5 = new object[] { stats.CachedChunks };
                                base.Text(Color.White, 0.9f, "Cached Chunks: {0}", objArray5);
                                if (this.DisplayDetails)
                                {
                                    foreach (KeyValuePair<Vector3I, MyStorageBase.VoxelChunk> pair in stats.Chunks)
                                    {
                                        MyStorageBase.VoxelChunk chunk = pair.Value;
                                        object[] objArray6 = new object[] { pair.Key, chunk.HitCount, chunk.Dirty };
                                        base.Text(Color.Wheat, 0.9f, "Chunk {0}: {1} hits; pending {2}", objArray6);
                                    }
                                }
                                if (this.DebugDraw)
                                {
                                    MyVoxelBase base2 = MySession.Static.VoxelMaps.Instances.FirstOrDefault<MyVoxelBase>(x => ReferenceEquals(x.Storage, storage));
                                    if (base2 != null)
                                    {
                                        foreach (KeyValuePair<Vector3I, MyStorageBase.VoxelChunk> pair2 in stats.Chunks)
                                        {
                                            BoundingBoxD box = new BoundingBoxD(pair2.Key << 3, (pair2.Key + 1) << 3);
                                            box.Translate((Vector3D) (-(storage.Size * 0.5) - 0.5));
                                            MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(box, base2.WorldMatrix), GetColorForDirty(pair2.Value.Dirty), 0.1f, true, true, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static Color GetColorForDirty(MyStorageDataTypeFlags dirty)
            {
                switch (dirty)
                {
                    case MyStorageDataTypeFlags.None:
                        return Color.Green;

                    case MyStorageDataTypeFlags.Content:
                        return Color.Blue;

                    case MyStorageDataTypeFlags.Material:
                        return Color.Red;

                    case MyStorageDataTypeFlags.All:
                        return Color.Magenta;
                }
                return Color.White;
            }

            public override string GetName() => 
                "Storage Write Cache";

            private bool ToggleCache()
            {
                MyVoxelOperationsSessionComponent.EnableCache = !MyVoxelOperationsSessionComponent.EnableCache;
                return true;
            }

            private bool ToggleFlush()
            {
                MyVoxelOperationsSessionComponent component = MySession.Static.GetComponent<MyVoxelOperationsSessionComponent>();
                component.ShouldFlush = !component.ShouldFlush;
                return true;
            }

            private bool ToggleWrite()
            {
                MyVoxelOperationsSessionComponent component = MySession.Static.GetComponent<MyVoxelOperationsSessionComponent>();
                component.ShouldWrite = !component.ShouldWrite;
                return true;
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyVoxelDebugInputComponent.StorageWriteCacheComponent.<>c <>9 = new MyVoxelDebugInputComponent.StorageWriteCacheComponent.<>c();
                public static Func<string> <>9__3_0;
                public static Func<string> <>9__3_2;
                public static Func<string> <>9__3_4;
                public static Func<string> <>9__3_5;
                public static Func<string> <>9__3_6;

                internal string <.ctor>b__3_0() => 
                    "Toggle detailed details.";

                internal string <.ctor>b__3_2() => 
                    "Toggle debug draw.";

                internal string <.ctor>b__3_4() => 
                    "Toggle cache writing.";

                internal string <.ctor>b__3_5() => 
                    "Toggle cache flushing.";

                internal string <.ctor>b__3_6() => 
                    "Toggle cache.";
            }
        }

        private class ToolsComponent : MyDebugComponent
        {
            private MyVoxelDebugInputComponent m_comp;
            private MyVoxelBase m_selectedVoxel;

            public ToolsComponent(MyVoxelDebugInputComponent comp)
            {
                this.m_comp = comp;
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Shrink selected storage to fit.", () => this.StorageShrinkToFit());
            }

            private static void Confirm(string message, Action successCallback)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder(message), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum x) {
                    if (x == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        successCallback();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }

            public override void Draw()
            {
                base.Draw();
                if (MySession.Static != null)
                {
                    LineD ray = new LineD(MySector.MainCamera.Position, MySector.MainCamera.Position + (200f * MySector.MainCamera.ForwardVector));
                    List<MyLineSegmentOverlapResult<MyEntity>> list = new List<MyLineSegmentOverlapResult<MyEntity>>();
                    MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref ray, list, MyEntityQueryType.Static);
                    double positiveInfinity = double.PositiveInfinity;
                    foreach (MyLineSegmentOverlapResult<MyEntity> result in list)
                    {
                        MyVoxelBase element = result.Element as MyVoxelBase;
                        if ((element != null) && (result.Distance < positiveInfinity))
                        {
                            this.m_selectedVoxel = element;
                        }
                    }
                    if (this.m_selectedVoxel != null)
                    {
                        object[] arguments = new object[] { this.m_selectedVoxel.StorageName, this.m_selectedVoxel.EntityId };
                        base.Text(Color.DarkOrange, 1.5f, "Selected Voxel: {0}:{1}", arguments);
                    }
                }
            }

            public override string GetName() => 
                "Tools";

            private static void ShowAlert(string message, params object[] args)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(message, args)), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }

            private void ShrinkVMap()
            {
                Vector3I vectori;
                Vector3I vectori2;
                this.m_selectedVoxel.GetFilledStorageBounds(out vectori, out vectori2);
                MyVoxelMapStorageDefinition definition = null;
                if (this.m_selectedVoxel.AsteroidName != null)
                {
                    MyDefinitionManager.Static.TryGetVoxelMapStorageDefinition(this.m_selectedVoxel.AsteroidName, out definition);
                }
                Vector3I size = this.m_selectedVoxel.Size;
                Vector3I vectori4 = (Vector3I) ((vectori2 - vectori) + 1);
                MyOctreeStorage storage = new MyOctreeStorage(null, vectori4);
                MyStorageData target = new MyStorageData(MyStorageDataTypeFlags.All);
                target.Resize(vectori4);
                this.m_selectedVoxel.Storage.ReadRange(target, MyStorageDataTypeFlags.All, 0, vectori, vectori2);
                storage.WriteRange(target, MyStorageDataTypeFlags.All, vectori, (Vector3I) (((vectori = (Vector3I) (((storage.Size - vectori4) / 2) + 1)) + vectori4) - 1), true, false);
                MyVoxelMap map = MyWorldGenerator.AddVoxelMap(this.m_selectedVoxel.StorageName, storage, this.m_selectedVoxel.WorldMatrix, 0L, false, true);
                this.m_selectedVoxel.Close();
                map.Save = true;
                if (definition == null)
                {
                    object[] args = new object[] { this.m_selectedVoxel.StorageName };
                    ShowAlert("Voxel map {0} does not have a definition, the shrunk voxel map will be saved with the world instead.", args);
                }
                else
                {
                    byte[] buffer;
                    map.Storage.Save(out buffer);
                    using (Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.ContentPath, definition.StorageFile), FileMode.Open))
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    MyHudNotification notification = new MyHudNotification(MyStringId.GetOrCompute("Voxel prefab {0} updated succesfuly (size changed from {1} to {2})."), 0xfa0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    object[] arguments = new object[] { definition.Id.SubtypeName, size, storage.Size };
                    notification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(notification);
                }
            }

            private bool StorageShrinkToFit()
            {
                if (this.m_selectedVoxel == null)
                {
                    ShowAlert("Please select a voxel map with the voxel probe box.", Array.Empty<object>());
                    return true;
                }
                if (this.m_selectedVoxel is MyPlanet)
                {
                    ShowAlert("Planets cannot be shrunk to fit.", Array.Empty<object>());
                    return true;
                }
                long size = this.m_selectedVoxel.Size.Size;
                Confirm($"Are you sure you want to shrink "{this.m_selectedVoxel.StorageName}" ({size} voxels total)? This will overwrite the original storage.", new Action(this.ShrinkVMap));
                return true;
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyVoxelDebugInputComponent.ToolsComponent.<>c <>9 = new MyVoxelDebugInputComponent.ToolsComponent.<>c();
                public static Func<string> <>9__2_0;

                internal string <.ctor>b__2_0() => 
                    "Shrink selected storage to fit.";
            }
        }
    }
}

