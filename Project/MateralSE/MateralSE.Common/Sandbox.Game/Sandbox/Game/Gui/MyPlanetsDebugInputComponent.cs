namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyPlanetsDebugInputComponent : MyMultiDebugInputComponent
    {
        private static uint[] AdjacentFaceTransforms = new uint[] { 
            0, 0, 0, 0x10, 10, 0x1a, 0, 0, 0x10, 0, 6, 0x16, 0x10, 0, 0, 0,
            3, 0x1f, 0, 0x10, 0, 0, 15, 0x13, 0x19, 5, 0x13, 15, 0, 0, 9, 0x15,
            0x1f, 3, 0, 0
        };
        private MyDebugComponent[] m_components;
        private List<MyVoxelBase> m_voxels = new List<MyVoxelBase>();
        public MyPlanet CameraPlanet;
        public MyPlanet CharacterPlanet;

        public MyPlanetsDebugInputComponent()
        {
            this.m_components = new MyDebugComponent[] { new ShapeComponent(this), new InfoComponent(this), new SectorsComponent(this), new SectorTreeComponent(this), new MiscComponent(this) };
        }

        public override void Draw()
        {
            if (MySession.Static != null)
            {
                base.Draw();
            }
        }

        public override void DrawInternal()
        {
            if (this.CameraPlanet != null)
            {
                object[] arguments = new object[] { this.CameraPlanet.StorageName };
                base.Text(Color.DarkOrange, "Current Planet: {0}", arguments);
            }
        }

        public MyPlanet GetClosestContainingPlanet(Vector3D point)
        {
            this.m_voxels.Clear();
            BoundingBoxD box = new BoundingBoxD(point, point);
            MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, this.m_voxels);
            double positiveInfinity = double.PositiveInfinity;
            MyPlanet planet = null;
            foreach (MyVoxelBase base2 in this.m_voxels)
            {
                if (!(base2 is MyPlanet))
                {
                    continue;
                }
                float num2 = Vector3.Distance((Vector3) base2.WorldMatrix.Translation, (Vector3) point);
                if (num2 < positiveInfinity)
                {
                    positiveInfinity = num2;
                    planet = (MyPlanet) base2;
                }
            }
            return planet;
        }

        public override string GetName() => 
            "Planets";

        private static void ProjectToCube(ref Vector3D localPos, out int direction, out Vector2D texcoords)
        {
            Vector3D vectord;
            Vector3D.Abs(ref localPos, out vectord);
            if (vectord.X > vectord.Y)
            {
                if (vectord.X > vectord.Z)
                {
                    localPos /= vectord.X;
                    texcoords.Y = localPos.Y;
                    if (localPos.X > 0.0)
                    {
                        texcoords.X = -localPos.Z;
                        direction = 3;
                    }
                    else
                    {
                        texcoords.X = localPos.Z;
                        direction = 2;
                    }
                }
                else
                {
                    localPos /= vectord.Z;
                    texcoords.Y = localPos.Y;
                    if (localPos.Z > 0.0)
                    {
                        texcoords.X = localPos.X;
                        direction = 1;
                    }
                    else
                    {
                        texcoords.X = -localPos.X;
                        direction = 0;
                    }
                }
            }
            else if (vectord.Y > vectord.Z)
            {
                localPos /= vectord.Y;
                texcoords.Y = localPos.X;
                if (localPos.Y > 0.0)
                {
                    texcoords.X = localPos.Z;
                    direction = 4;
                }
                else
                {
                    texcoords.X = -localPos.Z;
                    direction = 5;
                }
            }
            else
            {
                localPos /= vectord.Z;
                texcoords.Y = localPos.Y;
                if (localPos.Z > 0.0)
                {
                    texcoords.X = localPos.X;
                    direction = 1;
                }
                else
                {
                    texcoords.X = -localPos.X;
                    direction = 0;
                }
            }
        }

        public override void Update100()
        {
            this.CameraPlanet = this.GetClosestContainingPlanet(MySector.MainCamera.Position);
            if (MySession.Static.LocalCharacter != null)
            {
                this.CharacterPlanet = this.GetClosestContainingPlanet(MySession.Static.LocalCharacter.PositionComp.GetPosition());
            }
            base.Update100();
        }

        public override MyDebugComponent[] Components =>
            this.m_components;

        private class InfoComponent : MyDebugComponent
        {
            private MyPlanetsDebugInputComponent m_comp;
            private Vector3 m_lastCameraPosition = Vector3.Invalid;
            private Queue<float> m_speeds = new Queue<float>(60);

            public InfoComponent(MyPlanetsDebugInputComponent comp)
            {
                this.m_comp = comp;
            }

            public override void Draw()
            {
                base.Draw();
                if ((MySession.Static != null) && (this.m_comp.CameraPlanet != null))
                {
                    MyPlanetStorageProvider provider = this.m_comp.CameraPlanet.Provider;
                    if (provider != null)
                    {
                        MyPlanetStorageProvider.SurfacePropertiesExtended extended;
                        Vector3 position = (Vector3) MySector.MainCamera.Position;
                        float item = 0f;
                        float num2 = 0f;
                        if (this.m_lastCameraPosition.IsValid())
                        {
                            item = (position - this.m_lastCameraPosition).Length() * 60f;
                            if (this.m_speeds.Count == 60)
                            {
                                this.m_speeds.Dequeue();
                            }
                            this.m_speeds.Enqueue(item);
                            foreach (float num3 in this.m_speeds)
                            {
                                num2 += num3;
                            }
                            num2 /= (float) this.m_speeds.Count;
                        }
                        this.m_lastCameraPosition = position;
                        provider.ComputeCombinedMaterialAndSurfaceExtended(position - this.m_comp.CameraPlanet.PositionLeftBottomCorner, out extended);
                        base.Section("Position", Array.Empty<object>());
                        object[] arguments = new object[] { extended.Position };
                        base.Text("Position: {0}", arguments);
                        object[] objArray2 = new object[] { item, num2 };
                        base.Text("Speed: {0:F2}ms -- {1:F2}m/s", objArray2);
                        object[] objArray3 = new object[] { MathHelper.ToDegrees((float) Math.Asin((double) extended.Latitude)) };
                        base.Text("Latitude: {0}", objArray3);
                        object[] objArray4 = new object[] { MathHelper.ToDegrees(MathHelper.MonotonicAcos(extended.Longitude)) };
                        base.Text("Longitude: {0}", objArray4);
                        object[] objArray5 = new object[] { extended.Altitude };
                        base.Text("Altitude: {0}", objArray5);
                        base.VSpace(5f);
                        object[] objArray6 = new object[] { extended.Depth };
                        base.Text("Height: {0}", objArray6);
                        object[] objArray7 = new object[] { extended.HeightRatio };
                        base.Text("HeightRatio: {0}", objArray7);
                        object[] objArray8 = new object[] { MathHelper.ToDegrees((float) Math.Acos((double) extended.Slope)) };
                        base.Text("Slope: {0}", objArray8);
                        object[] objArray9 = new object[] { this.m_comp.CameraPlanet.GetAirDensity(position) };
                        base.Text("Air Density: {0}", objArray9);
                        object[] objArray10 = new object[] { this.m_comp.CameraPlanet.GetOxygenForPosition(position) };
                        base.Text("Oxygen: {0}", objArray10);
                        base.Section("Cube Position", Array.Empty<object>());
                        object[] objArray11 = new object[] { MyCubemapHelpers.GetNameForFace(extended.Face) };
                        base.Text("Face: {0}", objArray11);
                        object[] objArray12 = new object[] { extended.Texcoord };
                        base.Text("Texcoord: {0}", objArray12);
                        object[] objArray13 = new object[] { (Vector2I) (extended.Texcoord * 2048f) };
                        base.Text("Texcoord Position: {0}", objArray13);
                        base.Section("Material", Array.Empty<object>());
                        object[] objArray22 = new object[] { (extended.Material != null) ? extended.Material.Id.SubtypeName : "null" };
                        object[] objArray23 = new object[] { (extended.Material != null) ? extended.Material.Id.SubtypeName : "null" };
                        this.Text("Material: {0}", objArray23);
                        object[] objArray14 = new object[] { extended.Origin };
                        base.Text("Material Origin: {0}", objArray14);
                        object[] objArray20 = new object[] { (extended.Biome != null) ? extended.Biome.Name : "" };
                        object[] objArray21 = new object[] { (extended.Biome != null) ? extended.Biome.Name : "" };
                        this.Text("Biome: {0}", objArray21);
                        object[] objArray15 = new object[] { extended.EffectiveRule };
                        base.MultilineText("EffectiveRule: {0}", objArray15);
                        object[] objArray16 = new object[] { extended.Ore };
                        base.Text("Ore: {0}", objArray16);
                        base.Section("Map values", Array.Empty<object>());
                        object[] objArray17 = new object[] { extended.BiomeValue };
                        base.Text("BiomeValue: {0}", objArray17);
                        object[] objArray18 = new object[] { extended.MaterialValue };
                        base.Text("MaterialValue: {0}", objArray18);
                        object[] objArray19 = new object[] { extended.OreValue };
                        base.Text("OreValue: {0}", objArray19);
                    }
                }
            }

            public override string GetName() => 
                "Info";
        }

        private class MiscComponent : MyDebugComponent
        {
            private MyPlanetsDebugInputComponent m_comp;
            private Vector3 m_lastCameraPosition = Vector3.Invalid;
            private Queue<float> m_speeds = new Queue<float>(60);

            public MiscComponent(MyPlanetsDebugInputComponent comp)
            {
                this.m_comp = comp;
            }

            public override void Draw()
            {
                base.Draw();
                if (MySession.Static != null)
                {
                    object[] arguments = new object[] { MySession.Static.ElapsedGameTime };
                    base.Text("Game time: {0}", arguments);
                    Vector3 position = (Vector3) MySector.MainCamera.Position;
                    float item = 0f;
                    float num2 = 0f;
                    if (this.m_lastCameraPosition.IsValid())
                    {
                        item = (position - this.m_lastCameraPosition).Length() * 60f;
                        if (this.m_speeds.Count == 60)
                        {
                            this.m_speeds.Dequeue();
                        }
                        this.m_speeds.Enqueue(item);
                        foreach (float num3 in this.m_speeds)
                        {
                            num2 += num3;
                        }
                        num2 /= (float) this.m_speeds.Count;
                    }
                    this.m_lastCameraPosition = position;
                    base.Section("Controlled Entity/Camera", Array.Empty<object>());
                    object[] objArray2 = new object[] { item, num2 };
                    base.Text("Speed: {0:F2}ms -- {1:F2}m/s", objArray2);
                    if ((MySession.Static.LocalHumanPlayer != null) && (MySession.Static.LocalHumanPlayer.Controller.ControlledEntity != null))
                    {
                        MyEntityThrustComponent component;
                        MyEntity controlledEntity = (MyEntity) MySession.Static.LocalHumanPlayer.Controller.ControlledEntity;
                        if (controlledEntity is MyCubeBlock)
                        {
                            controlledEntity = ((MyCubeBlock) controlledEntity).CubeGrid;
                        }
                        StringBuilder output = new StringBuilder();
                        if (controlledEntity.Physics != null)
                        {
                            output.Clear();
                            output.Append("Mass: ");
                            MyValueFormatter.AppendWeightInBestUnit(controlledEntity.Physics.Mass, output);
                            base.Text(output.ToString(), Array.Empty<object>());
                        }
                        if (controlledEntity.Components.TryGet<MyEntityThrustComponent>(out component))
                        {
                            output.Clear();
                            output.Append("Current Thrust: ");
                            MyValueFormatter.AppendForceInBestUnit(component.FinalThrust.Length(), output);
                            output.AppendFormat(" : {0}", component.FinalThrust);
                            base.Text(output.ToString(), Array.Empty<object>());
                        }
                    }
                }
            }

            public override string GetName() => 
                "Misc";
        }

        private class SectorsComponent : MyDebugComponent
        {
            private MyPlanetsDebugInputComponent m_comp;
            private bool m_updateRange = true;
            private Vector3D m_center;
            private double m_radius;
            private double m_height;
            private QuaternionD m_orientation;

            public SectorsComponent(MyPlanetsDebugInputComponent comp)
            {
                this.m_comp = comp;
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Toggle update range", delegate {
                    bool flag;
                    this.m_updateRange = flag = !this.m_updateRange;
                    return flag;
                });
            }

            public override void Draw()
            {
                base.Draw();
                MyPlanet cameraPlanet = this.m_comp.CameraPlanet;
                if (cameraPlanet != null)
                {
                    MyPlanetEnvironmentComponent component = cameraPlanet.Components.Get<MyPlanetEnvironmentComponent>();
                    if (component != null)
                    {
                        bool flag = false;
                        MyEnvironmentSector activeSector = MyPlanetEnvironmentSessionComponent.ActiveSector;
                        if ((activeSector != null) && (activeSector.DataView != null))
                        {
                            List<MyLogicalEnvironmentSectorBase> logicalSectors = activeSector.DataView.LogicalSectors;
                            object[] objArray1 = new object[] { activeSector.ToString() };
                            base.Text(Color.White, 1.5f, "Current sector: {0}", objArray1);
                            base.Text("Storage sectors:", Array.Empty<object>());
                            foreach (MyLogicalEnvironmentSectorBase base2 in logicalSectors)
                            {
                                object[] objArray2 = new object[] { base2.DebugData };
                                base.Text("   {0}", objArray2);
                            }
                        }
                        object[] arguments = new object[] { this.m_radius };
                        base.Text("Horizon Distance: {0}", arguments);
                        if (this.m_updateRange)
                        {
                            this.UpdateViewRange(cameraPlanet);
                        }
                        IMyEnvironmentDataProvider[] providers = component.Providers;
                        int index = 0;
                        while (index < providers.Length)
                        {
                            MyLogicalEnvironmentSectorBase[] localArray1 = providers[index].LogicalSectors.ToArray<MyLogicalEnvironmentSectorBase>();
                            if ((localArray1.Length != 0) && !flag)
                            {
                                flag = true;
                                base.Text(Color.Yellow, 1.5f, "Synchronized:", Array.Empty<object>());
                            }
                            MyLogicalEnvironmentSectorBase[] baseArray = localArray1;
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= baseArray.Length)
                                {
                                    index++;
                                    break;
                                }
                                MyLogicalEnvironmentSectorBase base3 = baseArray[num2];
                                if ((base3 != null) && base3.ServerOwned)
                                {
                                    object[] objArray4 = new object[] { base3.ToString() };
                                    base.Text("Sector {0}", objArray4);
                                }
                                num2++;
                            }
                        }
                        base.Text("Physics", Array.Empty<object>());
                        foreach (MyEnvironmentSector sector2 in cameraPlanet.Components.Get<MyPlanetEnvironmentComponent>().PhysicsSectors.Values)
                        {
                            object[] objArray5 = new object[] { sector2.ToString() };
                            base.Text(Color.White, 0.8f, "Sector {0}", objArray5);
                        }
                        base.Text("Graphics", Array.Empty<object>());
                        foreach (MyPlanetEnvironmentClipmapProxy proxy in cameraPlanet.Components.Get<MyPlanetEnvironmentComponent>().Proxies.Values)
                        {
                            if (proxy.EnvironmentSector != null)
                            {
                                object[] objArray6 = new object[] { proxy.EnvironmentSector.ToString() };
                                base.Text(Color.White, 0.8f, "Sector {0}", objArray6);
                            }
                        }
                        MyRenderProxy.DebugDrawCylinder(this.m_center, this.m_orientation, (double) ((float) this.m_radius), this.m_height, Color.Orange, 1f, true, false, false);
                    }
                }
            }

            private string FormatWorkTracked(Vector4I workStats) => 
                $"{workStats.X:D3}/{workStats.Y:D3}/{workStats.Z:D3}/{workStats.W:D3}";

            public override string GetName() => 
                "Sectors";

            private bool ToggleSectors()
            {
                MyPlanet.RUN_SECTORS = !MyPlanet.RUN_SECTORS;
                return true;
            }

            private void UpdateViewRange(MyPlanet planet)
            {
                double num3;
                Vector3D position = MySector.MainCamera.Position;
                double maxValue = double.MaxValue;
                foreach (MyPlanet planet2 in MyPlanets.GetPlanets())
                {
                    MatrixD worldMatrix = planet2.WorldMatrix;
                    double num4 = Vector3D.DistanceSquared(position, worldMatrix.Translation);
                    if (num4 < maxValue)
                    {
                        planet = planet2;
                        maxValue = num4;
                    }
                }
                float minimumRadius = planet.MinimumRadius;
                this.m_height = planet.MaximumRadius - minimumRadius;
                Vector3D translation = planet.WorldMatrix.Translation;
                this.m_radius = HyperSphereHelpers.DistanceToTangentProjected(ref translation, ref position, (double) minimumRadius, out num3);
                Vector3D v = translation - position;
                v.Normalize();
                this.m_center = position + (v * num3);
                Vector3D forward = Vector3D.CalculatePerpendicularVector(v);
                this.m_orientation = QuaternionD.CreateFromForwardUp(forward, v);
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyPlanetsDebugInputComponent.SectorsComponent.<>c <>9 = new MyPlanetsDebugInputComponent.SectorsComponent.<>c();
                public static Func<string> <>9__1_0;

                internal string <.ctor>b__1_0() => 
                    "Toggle update range";
            }
        }

        private class SectorTreeComponent : MyDebugComponent, IMy2DClipmapManager
        {
            private MyPlanetsDebugInputComponent m_comp;
            private readonly HashSet<DebugDrawHandler> m_handlers = new HashSet<DebugDrawHandler>();
            private List<DebugDrawHandler> m_sortedHandlers = new List<DebugDrawHandler>();
            private My2DClipmap<DebugDrawHandler>[] m_tree;
            private int m_allocs;
            private int m_activeClipmap;
            private Vector3D Origin = Vector3D.Zero;
            private double Radius = 60000.0;
            private double Size;
            private bool m_update = true;
            private Vector3D m_lastUpdate;
            private int m_activeFace;

            public SectorTreeComponent(MyPlanetsDebugInputComponent comp)
            {
                this.Size = this.Radius * Math.Sqrt(2.0);
                double sectorSize = 64.0;
                this.m_comp = comp;
                this.m_tree = new My2DClipmap<DebugDrawHandler>[6];
                this.m_activeClipmap = 0;
                while (this.m_activeClipmap < this.m_tree.Length)
                {
                    Vector3 vector = Base6Directions.Directions[this.m_activeClipmap];
                    Vector3 suggestedUp = Base6Directions.Directions[(int) Base6Directions.GetPerpendicular((Base6Directions.Direction) ((byte) this.m_activeClipmap))];
                    MatrixD worldMatrix = MatrixD.CreateFromDir(-vector, suggestedUp);
                    worldMatrix.Translation = (vector * this.Size) / 2.0;
                    this.m_tree[this.m_activeClipmap] = new My2DClipmap<DebugDrawHandler>();
                    this.m_tree[this.m_activeClipmap].Init(this, ref worldMatrix, sectorSize, this.Size);
                    this.m_activeClipmap++;
                }
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Toggle clipmap update", delegate {
                    bool flag;
                    this.m_update = flag = !this.m_update;
                    return flag;
                });
            }

            public override void Draw()
            {
                base.Draw();
                object[] arguments = new object[] { this.m_allocs };
                base.Text("Node Allocs/Deallocs from last update: {0}", arguments);
                foreach (DebugDrawHandler handler in this.m_sortedHandlers)
                {
                    MyRenderProxy.DebugDraw6FaceConvex(handler.FrustumBounds, new Color(My2DClipmapHelpers.LodColors[handler.Lod], 1f), 0.2f, true, true, false);
                }
                this.m_activeClipmap = 0;
                while (this.m_activeClipmap < this.m_tree.Length)
                {
                    My2DClipmap<DebugDrawHandler> clipmap = this.m_tree[this.m_activeClipmap];
                    Vector3D position = Vector3.Transform((Vector3) this.m_tree[this.m_activeClipmap].LastPosition, this.m_tree[this.m_activeClipmap].WorldMatrix);
                    MyRenderProxy.DebugDrawSphere(position, 500f, Color.Red, 1f, true, false, true, false);
                    MyRenderProxy.DebugDrawText3D(position, ((byte) this.m_activeClipmap).ToString(), Color.Blue, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    MatrixD worldMatrix = clipmap.WorldMatrix;
                    Vector3D pointTo = Vector3D.Transform(Vector3D.Right * 10000.0, clipmap.WorldMatrix);
                    Vector3D vectord2 = Vector3D.Transform(Vector3D.Up * 10000.0, clipmap.WorldMatrix);
                    Base6Directions.Direction activeClipmap = (Base6Directions.Direction) ((byte) this.m_activeClipmap);
                    Vector3D translation = worldMatrix.Translation;
                    MyRenderProxy.DebugDrawText3D(translation, activeClipmap.ToString(), Color.Blue, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    MyRenderProxy.DebugDrawLine3D(translation, vectord2, Color.Green, Color.Green, true, false);
                    MyRenderProxy.DebugDrawLine3D(translation, pointTo, Color.Red, Color.Red, true, false);
                    this.m_activeClipmap++;
                }
            }

            public override string GetName() => 
                "Sector Tree";

            public override unsafe void Update10()
            {
                base.Update10();
                if (MySession.Static != null)
                {
                    int num;
                    Vector2D vectord2;
                    if (this.m_update)
                    {
                        this.m_lastUpdate = MySector.MainCamera.Position;
                    }
                    MyPlanetsDebugInputComponent.ProjectToCube(ref this.m_lastUpdate, out num, out vectord2);
                    this.m_activeFace = num;
                    Vector3D vectord3 = Base6Directions.Directions[num];
                    double* numPtr1 = (double*) ref vectord3.X;
                    numPtr1[0] *= this.m_lastUpdate.X;
                    double* numPtr2 = (double*) ref vectord3.Y;
                    numPtr2[0] *= this.m_lastUpdate.Y;
                    double* numPtr3 = (double*) ref vectord3.Z;
                    numPtr3[0] *= this.m_lastUpdate.Z;
                    double num2 = Math.Abs((double) (this.m_lastUpdate.Length() - this.Radius));
                    this.m_allocs = 0;
                    this.m_activeClipmap = 0;
                    while (this.m_activeClipmap < this.m_tree.Length)
                    {
                        Vector3D vectord5;
                        My2DClipmap<DebugDrawHandler> clipmap = this.m_tree[this.m_activeClipmap];
                        Vector2D newCoords = vectord2;
                        MyPlanetCubemapHelper.TranslateTexcoordsToFace(ref vectord2, num, this.m_activeClipmap, out newCoords);
                        vectord5.X = newCoords.X * clipmap.FaceHalf;
                        vectord5.Y = newCoords.Y * clipmap.FaceHalf;
                        vectord5.Z = ((this.m_activeClipmap ^ num) != 1) ? num2 : (num2 + (this.Radius * 2.0));
                        this.m_tree[this.m_activeClipmap].NodeAllocDeallocs = 0;
                        this.m_tree[this.m_activeClipmap].Update(vectord5);
                        this.m_allocs += this.m_tree[this.m_activeClipmap].NodeAllocDeallocs;
                        this.m_activeClipmap++;
                    }
                    this.m_sortedHandlers = this.m_handlers.ToList<DebugDrawHandler>();
                    DebugDrawSorter comparer = new DebugDrawSorter();
                    this.m_sortedHandlers.Sort(comparer);
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyPlanetsDebugInputComponent.SectorTreeComponent.<>c <>9 = new MyPlanetsDebugInputComponent.SectorTreeComponent.<>c();
                public static Func<string> <>9__11_0;

                internal string <.ctor>b__11_0() => 
                    "Toggle clipmap update";
            }

            private class DebugDrawHandler : IMy2DClipmapNodeHandler
            {
                private MyPlanetsDebugInputComponent.SectorTreeComponent m_parent;
                public Vector2I Coords;
                public BoundingBoxD Bounds;
                public int Lod;
                public Vector3D[] FrustumBounds;

                public void Close()
                {
                    this.m_parent.m_handlers.Remove(this);
                }

                public unsafe void Init(IMy2DClipmapManager parent, int x, int y, int lod, ref BoundingBox2D bounds)
                {
                    this.m_parent = (MyPlanetsDebugInputComponent.SectorTreeComponent) parent;
                    this.Bounds = new BoundingBoxD(new Vector3D(bounds.Min, 0.0), new Vector3D(bounds.Max, 50.0));
                    this.Lod = lod;
                    MatrixD worldMatrix = this.m_parent.m_tree[this.m_parent.m_activeClipmap].WorldMatrix;
                    this.Bounds = this.Bounds.TransformFast(worldMatrix);
                    this.Coords = new Vector2I(x, y);
                    this.m_parent.m_handlers.Add(this);
                    Vector3D center = this.Bounds.Center;
                    Vector3D[] vectordArray = new Vector3D[] { Vector3D.Transform(new Vector3D(bounds.Min.X, bounds.Min.Y, 0.0), worldMatrix), Vector3D.Transform(new Vector3D(bounds.Max.X, bounds.Min.Y, 0.0), worldMatrix), Vector3D.Transform(new Vector3D(bounds.Min.X, bounds.Max.Y, 0.0), worldMatrix), Vector3D.Transform(new Vector3D(bounds.Max.X, bounds.Max.Y, 0.0), worldMatrix) };
                    for (int i = 0; i < 4; i++)
                    {
                        vectordArray[i].Normalize();
                        vectordArray[i + 4] = vectordArray[i] * this.m_parent.Radius;
                        Vector3D* vectordPtr1 = (Vector3D*) ref vectordArray[i];
                        vectordPtr1[0] *= this.m_parent.Radius + 50.0;
                    }
                    this.FrustumBounds = vectordArray;
                }

                public void InitJoin(IMy2DClipmapNodeHandler[] children)
                {
                    MyPlanetsDebugInputComponent.SectorTreeComponent.DebugDrawHandler handler = (MyPlanetsDebugInputComponent.SectorTreeComponent.DebugDrawHandler) children[0];
                    this.Lod = handler.Lod + 1;
                    this.Coords = new Vector2I(handler.Coords.X >> 1, handler.Coords.Y >> 1);
                    this.m_parent.m_handlers.Add(this);
                }

                public unsafe void Split(BoundingBox2D* childBoxes, ref IMy2DClipmapNodeHandler[] children)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        children[i].Init(this.m_parent, (this.Coords.X << 1) + (i & 1), (this.Coords.Y << 1) + ((i >> 1) & 1), this.Lod - 1, ref (BoundingBox2D) ref (childBoxes + i));
                    }
                }
            }

            [StructLayout(LayoutKind.Sequential, Size=1)]
            private struct DebugDrawSorter : IComparer<MyPlanetsDebugInputComponent.SectorTreeComponent.DebugDrawHandler>
            {
                public int Compare(MyPlanetsDebugInputComponent.SectorTreeComponent.DebugDrawHandler x, MyPlanetsDebugInputComponent.SectorTreeComponent.DebugDrawHandler y) => 
                    (x.Lod - y.Lod);
            }
        }

        private class ShapeComponent : MyDebugComponent
        {
            private MyPlanetsDebugInputComponent m_comp;

            public ShapeComponent(MyPlanetsDebugInputComponent comp)
            {
                this.m_comp = comp;
            }

            public override void Draw()
            {
                object[] arguments = new object[] { MyPlanetShapeProvider.CullStats.History };
                base.Text("Planet Shape request culls: {0}", arguments);
                object[] objArray2 = new object[] { MyPlanetShapeProvider.CacheStats.History };
                base.Text("Planet Shape coefficient cache hits: {0}", objArray2);
                object[] objArray3 = new object[] { MyPlanetShapeProvider.PruningStats.History };
                base.Text("Planet Shape pruning tree hits: {0}", objArray3);
                base.Draw();
            }

            public override string GetName() => 
                "Shape";

            public override void Update100()
            {
                base.Update100();
                MyPlanetShapeProvider.PruningStats.CycleWork();
                MyPlanetShapeProvider.CacheStats.CycleWork();
                MyPlanetShapeProvider.CullStats.CycleWork();
            }
        }
    }
}

