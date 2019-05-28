namespace Sandbox.Game.Gui
{
    using Havok;
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GUI.DebugInputComponents.HonzaDebugInputComponent;
    using Sandbox.Game.World;
    using SharpDX.Windows;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using VRage.Audio;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyHonzaInputComponent : MyMultiDebugInputComponent
    {
        private static IMyEntity m_selectedEntity;
        [CompilerGenerated]
        private static Action OnSelectedEntityChanged;
        private static long m_counter;
        public static long dbgPosCounter;
        private MyDebugComponent[] m_components = new MyDebugComponent[] { new DefaultComponent(), new PhysicsComponent(), new LiveWatchComponent() };

        public static  event Action OnSelectedEntityChanged
        {
            [CompilerGenerated] add
            {
                Action onSelectedEntityChanged = OnSelectedEntityChanged;
                while (true)
                {
                    Action a = onSelectedEntityChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onSelectedEntityChanged = Interlocked.CompareExchange<Action>(ref OnSelectedEntityChanged, action3, a);
                    if (ReferenceEquals(onSelectedEntityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onSelectedEntityChanged = OnSelectedEntityChanged;
                while (true)
                {
                    Action source = onSelectedEntityChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onSelectedEntityChanged = Interlocked.CompareExchange<Action>(ref OnSelectedEntityChanged, action3, source);
                    if (ReferenceEquals(onSelectedEntityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public override string GetName() => 
            "Honza";

        private void HandleEntitySelect()
        {
            if (MyInput.Static.IsAnyShiftKeyPressed() && MyInput.Static.IsNewLeftMousePressed())
            {
                if ((SelectedEntity != null) && (SelectedEntity.Physics != null))
                {
                    if (SelectedEntity is MyCubeGrid)
                    {
                        ((HkGridShape) ((MyPhysicsBody) SelectedEntity.Physics).GetShape()).DebugDraw = false;
                    }
                    ((MyEntity) SelectedEntity).ClearDebugRenderComponents();
                    SelectedEntity = null;
                }
                else if (MySector.MainCamera != null)
                {
                    List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
                    MyPhysics.CastRay(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 100f), toList, 0);
                    foreach (MyPhysics.HitInfo info in toList)
                    {
                        HkRigidBody body = info.HkHitInfo.Body;
                        if ((body != null) && (body.Layer != 0x13))
                        {
                            SelectedEntity = info.HkHitInfo.GetHitEntity();
                            if (SelectedEntity is MyCubeGrid)
                            {
                                HkGridShape shape = (HkGridShape) ((MyPhysicsBody) SelectedEntity.Physics).GetShape();
                                shape.DebugRigidBody = body;
                                shape.DebugDraw = true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        public override bool HandleInput()
        {
            this.HandleEntitySelect();
            return base.HandleInput();
        }

        public static IMyEntity SelectedEntity
        {
            get => 
                m_selectedEntity;
            set
            {
                if (!ReferenceEquals(m_selectedEntity, value))
                {
                    m_selectedEntity = value;
                    m_counter = dbgPosCounter = 0L;
                    if (OnSelectedEntityChanged != null)
                    {
                        OnSelectedEntityChanged();
                    }
                }
            }
        }

        public override MyDebugComponent[] Components =>
            this.m_components;

        public class DefaultComponent : MyDebugComponent
        {
            public static float MassMultiplier = 100f;
            private static long m_lastMemory;
            private static HkMemorySnapshot? m_snapA;
            public static bool ApplyMassMultiplier;
            public static ShownMassEnum ShowRealBlockMass = ShownMassEnum.None;
            private int m_memoryB;
            private int m_memoryA;
            private static bool HammerForce;
            private float RADIUS = 0.005f;
            private bool m_drawBodyInfo = true;
            private bool m_drawUpdateInfo;
            private List<System.Type> m_dbgComponents = new List<System.Type>();

            public DefaultComponent()
            {
                this.AddShortcut(MyKeys.S, true, true, false, false, () => "Insert safe zone", delegate {
                    this.TestParallelBatch();
                    return true;
                });
                this.AddShortcut(MyKeys.None, false, false, false, false, () => "Hammer (CTRL + Mouse left)", null);
                this.AddShortcut(MyKeys.H, true, true, true, false, () => "Hammer force: " + (HammerForce ? "ON" : "OFF"), delegate {
                    HammerForce = !HammerForce;
                    return true;
                });
                base.AddShortcut(MyKeys.OemPlus, true, true, false, false, () => "Radius+: " + this.RADIUS, delegate {
                    this.RADIUS += 0.5f;
                    return true;
                });
                this.AddShortcut(MyKeys.OemMinus, true, true, false, false, () => "", delegate {
                    this.RADIUS -= 0.5f;
                    return true;
                });
                this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Shown mass: " + ShowRealBlockMass.ToString(), delegate {
                    ShowRealBlockMass += 1;
                    ShowRealBlockMass = ShowRealBlockMass % ShownMassEnum.MaxVal;
                    return true;
                });
                base.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => string.Concat(new object[] { "MemA: ", this.m_memoryA, " MemB: ", this.m_memoryB, " Diff:", this.m_memoryB - this.m_memoryA }), new Func<bool>(this.Diff));
                this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "", delegate {
                    this.m_drawBodyInfo = !this.m_drawBodyInfo;
                    this.m_drawUpdateInfo = !this.m_drawUpdateInfo;
                    return true;
                });
                this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Prioritize: " + (MyFakes.PRIORITIZE_PRECALC_JOBS ? "On" : "Off"), delegate {
                    MyFakes.PRIORITIZE_PRECALC_JOBS = !MyFakes.PRIORITIZE_PRECALC_JOBS;
                    return true;
                });
                this.m_dbgComponents.Clear();
                foreach (System.Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if ((!type.IsSubclassOf(typeof(MyRenderComponentBase)) && !type.IsSubclassOf(typeof(MySyncComponentBase))) && type.IsSubclassOf(typeof(MyEntityComponentBase)))
                    {
                        this.m_dbgComponents.Add(type);
                    }
                }
            }

            private bool Diff()
            {
                foreach (MyEntity entity in MyEntities.GetEntities())
                {
                    Vector3D vectord = entity.PositionComp.GetPosition() - MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition();
                    if (vectord.Length() > 100.0)
                    {
                        entity.Close();
                    }
                }
                return true;
            }

            public override unsafe void Draw()
            {
                base.Draw();
                Vector2 vector = new Vector2(600f, 100f);
                foreach (System.Type type in this.m_dbgComponents)
                {
                    bool local1 = (MyHonzaInputComponent.SelectedEntity != null) && MyHonzaInputComponent.SelectedEntity.Components.Contains(type);
                    float* singlePtr1 = (float*) ref vector.Y;
                    singlePtr1[0] += 10f;
                }
                vector = new Vector2(580f, (float) (100 + (10 * this.m_memoryA)));
                if (MyHonzaInputComponent.SelectedEntity != null)
                {
                    BoundingBoxD box = new BoundingBoxD(MyHonzaInputComponent.SelectedEntity.PositionComp.LocalAABB.Min, MyHonzaInputComponent.SelectedEntity.PositionComp.LocalAABB.Max);
                    MyOrientedBoundingBoxD xd1 = new MyOrientedBoundingBoxD(box, MyHonzaInputComponent.SelectedEntity.WorldMatrix);
                    MyRenderProxy.DebugDrawAABB(MyHonzaInputComponent.SelectedEntity.PositionComp.WorldAABB, Color.White, 1f, 1f, false, false, false);
                }
                this.DrawBodyInfo();
            }

            private unsafe void DrawBodyInfo()
            {
                Vector2 screenCoord = new Vector2(400f, 10f);
                MyEntity thisEntity = null;
                HkRigidBody hkEntity = null;
                if ((MyHonzaInputComponent.SelectedEntity != null) && (MyHonzaInputComponent.SelectedEntity.Physics != null))
                {
                    hkEntity = ((MyEntity) MyHonzaInputComponent.SelectedEntity).Physics.RigidBody;
                }
                if ((MySector.MainCamera != null) && (hkEntity == null))
                {
                    List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
                    MyPhysics.CastRay(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 100f), toList, 0);
                    foreach (MyPhysics.HitInfo info in toList)
                    {
                        hkEntity = info.HkHitInfo.Body;
                        if ((hkEntity != null) && (hkEntity.Layer != 0x13))
                        {
                            thisEntity = info.HkHitInfo.GetHitEntity() as MyEntity;
                            StringBuilder builder = new StringBuilder("ShapeKeys: ");
                            int index = 0;
                            while (true)
                            {
                                if (index < HkWorld.HitInfo.ShapeKeyCount)
                                {
                                    uint shapeKey = info.HkHitInfo.GetShapeKey(index);
                                    if (shapeKey != uint.MaxValue)
                                    {
                                        builder.Append($"{shapeKey} ");
                                        index++;
                                        continue;
                                    }
                                }
                                MyRenderProxy.DebugDrawText2D(screenCoord, builder.ToString(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                float* singlePtr1 = (float*) ref screenCoord.Y;
                                singlePtr1[0] += 20f;
                                if ((thisEntity != null) && (thisEntity.GetPhysicsBody() != null))
                                {
                                    MyRenderProxy.DebugDrawText2D(screenCoord, $"Weld: {thisEntity.GetPhysicsBody().WeldInfo.Children.Count}", Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                }
                                float* singlePtr2 = (float*) ref screenCoord.Y;
                                singlePtr2[0] += 20f;
                                break;
                            }
                            break;
                        }
                    }
                }
                if ((hkEntity != null) && this.m_drawBodyInfo)
                {
                    MyEntity entity = null;
                    MyPhysicsBody userObject = (MyPhysicsBody) hkEntity.UserObject;
                    if (userObject != null)
                    {
                        entity = (MyEntity) userObject.Entity;
                    }
                    if (hkEntity.GetBody() != null)
                    {
                        MyRenderProxy.DebugDrawText2D(screenCoord, $"Name: {hkEntity.GetBody().Entity.DisplayName}", Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    }
                    float* singlePtr3 = (float*) ref screenCoord.Y;
                    singlePtr3[0] += 20f;
                    uint collisionFilterInfo = hkEntity.GetCollisionFilterInfo();
                    int layerFromFilterInfo = HkGroupFilter.GetLayerFromFilterInfo(collisionFilterInfo);
                    int systemGroupFromFilterInfo = HkGroupFilter.GetSystemGroupFromFilterInfo(collisionFilterInfo);
                    int subSystemIdFromFilterInfo = HkGroupFilter.GetSubSystemIdFromFilterInfo(collisionFilterInfo);
                    int num6 = HkGroupFilter.getSubSystemDontCollideWithFromFilterInfo(collisionFilterInfo);
                    MyRenderProxy.DebugDrawText2D(screenCoord, $"Layer: {layerFromFilterInfo}  Group: {systemGroupFromFilterInfo} SubSys: {subSystemIdFromFilterInfo} SubSysDont: {num6} ", (layerFromFilterInfo == 0) ? Color.Red : Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr4 = (float*) ref screenCoord.Y;
                    singlePtr4[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "ShapeType: " + hkEntity.GetShape().ShapeType, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr5 = (float*) ref screenCoord.Y;
                    singlePtr5[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Mass: " + hkEntity.Mass, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr6 = (float*) ref screenCoord.Y;
                    singlePtr6[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Friction: " + hkEntity.Friction, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr7 = (float*) ref screenCoord.Y;
                    singlePtr7[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Restitution: " + hkEntity.Restitution, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr8 = (float*) ref screenCoord.Y;
                    singlePtr8[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "LinDamping: " + hkEntity.LinearDamping, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr9 = (float*) ref screenCoord.Y;
                    singlePtr9[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "AngDamping: " + hkEntity.AngularDamping, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr10 = (float*) ref screenCoord.Y;
                    singlePtr10[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "PenetrationDepth: " + hkEntity.AllowedPenetrationDepth, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr11 = (float*) ref screenCoord.Y;
                    singlePtr11[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Lin: " + hkEntity.LinearVelocity.Length(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr12 = (float*) ref screenCoord.Y;
                    singlePtr12[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Ang: " + hkEntity.AngularVelocity.Length(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr13 = (float*) ref screenCoord.Y;
                    singlePtr13[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Act: " + (hkEntity.IsActive ? "true" : "false"), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr14 = (float*) ref screenCoord.Y;
                    singlePtr14[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Stat: " + (hkEntity.IsFixedOrKeyframed ? "true" : "false"), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr15 = (float*) ref screenCoord.Y;
                    singlePtr15[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Solver: " + hkEntity.Motion.GetDeactivationClass(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr16 = (float*) ref screenCoord.Y;
                    singlePtr16[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Mass: " + hkEntity.Mass, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr17 = (float*) ref screenCoord.Y;
                    singlePtr17[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "MotionType: " + hkEntity.GetMotionType(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr18 = (float*) ref screenCoord.Y;
                    singlePtr18[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "QualityType: " + hkEntity.Quality, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr19 = (float*) ref screenCoord.Y;
                    singlePtr19[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "DeactCtr0: " + hkEntity.DeactivationCounter0, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr20 = (float*) ref screenCoord.Y;
                    singlePtr20[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "DeactCtr1: " + hkEntity.DeactivationCounter1, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr21 = (float*) ref screenCoord.Y;
                    singlePtr21[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "EntityId: " + entity.EntityId, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr22 = (float*) ref screenCoord.Y;
                    singlePtr22[0] += 20f;
                }
                if ((MyHonzaInputComponent.SelectedEntity != null) && this.m_drawUpdateInfo)
                {
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Updates: " + MyHonzaInputComponent.m_counter, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr23 = (float*) ref screenCoord.Y;
                    singlePtr23[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "PositionUpd: " + MyHonzaInputComponent.dbgPosCounter, Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr24 = (float*) ref screenCoord.Y;
                    singlePtr24[0] += 20f;
                    MyRenderProxy.DebugDrawText2D(screenCoord, "Frames per update: " + (((float) MyHonzaInputComponent.m_counter) / ((float) MyHonzaInputComponent.dbgPosCounter)), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    float* singlePtr25 = (float*) ref screenCoord.Y;
                    singlePtr25[0] += 20f;
                }
            }

            public override string GetName() => 
                "Honza";

            private unsafe void Hammer()
            {
                Vector3D position = MySector.MainCamera.Position;
                LineD ed = new LineD(position, position + (MySector.MainCamera.ForwardVector * 200f));
                List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
                MyPhysics.CastRay(ed.From, ed.To, toList, 0x18);
                toList.RemoveAll(hit => ReferenceEquals(hit.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity));
                if (toList.Count != 0)
                {
                    MyEntity thisEntity = null;
                    MyPhysics.HitInfo info = new MyPhysics.HitInfo();
                    foreach (MyPhysics.HitInfo info2 in toList)
                    {
                        if (info2.HkHitInfo.Body != null)
                        {
                            thisEntity = info2.HkHitInfo.GetHitEntity() as MyEntity;
                            info = info2;
                            break;
                        }
                    }
                    if (thisEntity != null)
                    {
                        HkdFractureImpactDetails details = HkdFractureImpactDetails.Create();
                        details.SetBreakingBody(thisEntity.Physics.RigidBody);
                        details.SetContactPoint((Vector3) thisEntity.Physics.WorldToCluster(info.Position));
                        details.SetDestructionRadius(this.RADIUS);
                        details.SetBreakingImpulse(MyDestructionConstants.STRENGTH * 10f);
                        if (HammerForce)
                        {
                            details.SetParticleVelocity((Vector3) (-ed.Direction * 20.0));
                        }
                        details.SetParticlePosition((Vector3) thisEntity.Physics.WorldToCluster(info.Position));
                        details.SetParticleMass(1000000f);
                        HkdFractureImpactDetails* detailsPtr1 = (HkdFractureImpactDetails*) ref details;
                        detailsPtr1.Flag = details.Flag | HkdFractureImpactDetails.Flags.FLAG_DONT_RECURSE;
                        if (thisEntity.GetPhysicsBody().HavokWorld.DestructionWorld != null)
                        {
                            MyPhysics.FractureImpactDetails details2 = new MyPhysics.FractureImpactDetails {
                                Details = details,
                                World = thisEntity.GetPhysicsBody().HavokWorld,
                                Entity = thisEntity
                            };
                            MyPhysics.EnqueueDestruction(details2);
                        }
                    }
                }
            }

            public override bool HandleInput()
            {
                MyHonzaInputComponent.m_counter += 1L;
                if (base.HandleInput())
                {
                    return true;
                }
                bool handled = false;
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewLeftMouseReleased())
                {
                    this.Hammer();
                }
                handled = HandleMassMultiplier(handled);
                handled = this.HandleMemoryDiff(handled);
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad9))
                {
                    MyScriptManager.Static.LoadData();
                }
                if ((MyHonzaInputComponent.SelectedEntity != null) && MyInput.Static.IsNewKeyPressed(MyKeys.NumPad3))
                {
                    object obj2 = Activator.CreateInstance(this.m_dbgComponents[this.m_memoryA]);
                    MyHonzaInputComponent.SelectedEntity.Components.Add(this.m_dbgComponents[this.m_memoryA], obj2 as MyComponentBase);
                }
                if (MyAudio.Static != null)
                {
                    using (List<IMy3DSoundEmitter>.Enumerator enumerator = MyAudio.Static.Get3DSounds().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyRenderProxy.DebugDrawSphere(enumerator.Current.SourcePosition, 0.1f, Color.Red, 1f, false, false, true, false);
                        }
                    }
                }
                return handled;
            }

            private static bool HandleMassMultiplier(bool handled)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad1))
                {
                    ApplyMassMultiplier = !ApplyMassMultiplier;
                    handled = true;
                }
                int num = 1;
                if (MyInput.Static.IsKeyPress(MyKeys.N))
                {
                    num = 10;
                }
                if (MyInput.Static.IsKeyPress(MyKeys.B))
                {
                    num = 100;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.OemQuotes))
                {
                    MassMultiplier = (MassMultiplier <= 1f) ? (MassMultiplier * num) : (MassMultiplier + num);
                    handled = true;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.OemSemicolon))
                {
                    MassMultiplier = (MassMultiplier <= 1f) ? (MassMultiplier / ((float) num)) : (MassMultiplier - num);
                    handled = true;
                }
                return handled;
            }

            private bool HandleMemoryDiff(bool handled)
            {
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.PageUp))
                {
                    this.m_memoryA--;
                    handled = true;
                }
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.PageDown))
                {
                    this.m_memoryA++;
                    handled = true;
                }
                this.m_memoryA = (this.m_memoryA + this.m_dbgComponents.Count) % this.m_dbgComponents.Count;
                return handled;
            }

            private static bool SpawnBreakable(bool handled) => 
                handled;

            private void TestParallelBatch()
            {
                WorkOptions? options = null;
                Parallel.For(0, 10, delegate (int _) {
                    int[] RunJournal = new int[0x3e8];
                    DependencyBatch batch = new DependencyBatch(WorkPriority.VeryHigh);
                    batch.Preallocate(0x5dc);
                    for (int i = 0; i < 0x3e8; i++)
                    {
                        int id = i;
                        batch.Add(delegate {
                            Thread.Sleep(TimeSpan.FromMilliseconds((double) (5 + MyRandom.Instance.Next(10))));
                            if ((id > 0) && (id != 0x3e7))
                            {
                                int index = ((id - 1) / 2) * 2;
                                Interlocked.Exchange(ref RunJournal[index], 1);
                            }
                            Interlocked.Exchange(ref RunJournal[id], 1);
                        });
                    }
                    for (int j = 0; j < 0x3e5; j += 2)
                    {
                        using (DependencyBatch.StartToken token = batch.Job(j))
                        {
                            token.Starts(j + 1);
                            token.Starts(j + 2);
                        }
                    }
                    batch.Execute();
                    int[] numArray = RunJournal;
                    for (int k = 0; k < numArray.Length; k++)
                    {
                        int num1 = numArray[k];
                    }
                }, WorkPriority.Normal, options);
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyHonzaInputComponent.DefaultComponent.<>c <>9 = new MyHonzaInputComponent.DefaultComponent.<>c();
                public static Func<string> <>9__14_0;
                public static Func<string> <>9__14_2;
                public static Func<string> <>9__14_3;
                public static Func<bool> <>9__14_4;
                public static Func<string> <>9__14_7;
                public static Func<string> <>9__14_9;
                public static Func<bool> <>9__14_10;
                public static Func<string> <>9__14_12;
                public static Func<string> <>9__14_14;
                public static Func<bool> <>9__14_15;
                public static Predicate<MyPhysics.HitInfo> <>9__20_0;
                public static Action<int> <>9__23_0;

                internal string <.ctor>b__14_0() => 
                    "Insert safe zone";

                internal bool <.ctor>b__14_10()
                {
                    MyHonzaInputComponent.DefaultComponent.ShowRealBlockMass += 1;
                    MyHonzaInputComponent.DefaultComponent.ShowRealBlockMass = MyHonzaInputComponent.DefaultComponent.ShowRealBlockMass % MyHonzaInputComponent.DefaultComponent.ShownMassEnum.MaxVal;
                    return true;
                }

                internal string <.ctor>b__14_12() => 
                    "";

                internal string <.ctor>b__14_14() => 
                    ("Prioritize: " + (MyFakes.PRIORITIZE_PRECALC_JOBS ? "On" : "Off"));

                internal bool <.ctor>b__14_15()
                {
                    MyFakes.PRIORITIZE_PRECALC_JOBS = !MyFakes.PRIORITIZE_PRECALC_JOBS;
                    return true;
                }

                internal string <.ctor>b__14_2() => 
                    "Hammer (CTRL + Mouse left)";

                internal string <.ctor>b__14_3() => 
                    ("Hammer force: " + (MyHonzaInputComponent.DefaultComponent.HammerForce ? "ON" : "OFF"));

                internal bool <.ctor>b__14_4()
                {
                    MyHonzaInputComponent.DefaultComponent.HammerForce = !MyHonzaInputComponent.DefaultComponent.HammerForce;
                    return true;
                }

                internal string <.ctor>b__14_7() => 
                    "";

                internal string <.ctor>b__14_9() => 
                    ("Shown mass: " + MyHonzaInputComponent.DefaultComponent.ShowRealBlockMass.ToString());

                internal bool <Hammer>b__20_0(MyPhysics.HitInfo hit) => 
                    ReferenceEquals(hit.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity);

                internal void <TestParallelBatch>b__23_0(int _)
                {
                    int[] RunJournal = new int[0x3e8];
                    DependencyBatch batch = new DependencyBatch(WorkPriority.VeryHigh);
                    batch.Preallocate(0x5dc);
                    for (int i = 0; i < 0x3e8; i++)
                    {
                        int id = i;
                        batch.Add(delegate {
                            Thread.Sleep(TimeSpan.FromMilliseconds((double) (5 + MyRandom.Instance.Next(10))));
                            if ((id > 0) && (id != 0x3e7))
                            {
                                int index = ((id - 1) / 2) * 2;
                                Interlocked.Exchange(ref RunJournal[index], 1);
                            }
                            Interlocked.Exchange(ref RunJournal[id], 1);
                        });
                    }
                    for (int j = 0; j < 0x3e5; j += 2)
                    {
                        using (DependencyBatch.StartToken token = batch.Job(j))
                        {
                            token.Starts(j + 1);
                            token.Starts(j + 2);
                        }
                    }
                    batch.Execute();
                    int[] numArray = RunJournal;
                    for (int k = 0; k < numArray.Length; k++)
                    {
                        int num1 = numArray[k];
                    }
                }
            }

            public enum ShownMassEnum
            {
                Havok,
                Real,
                SI,
                None,
                MaxVal
            }
        }

        public class LiveWatchComponent : MyDebugComponent
        {
            private int MAX_HISTORY = 0x2710;
            private object m_currentInstance;
            private System.Type m_selectedType;
            private System.Type m_lastType;
            private readonly List<MemberInfo> m_members = new List<MemberInfo>();
            private readonly List<MemberInfo> m_currentPath = new List<MemberInfo>();
            private readonly Dictionary<System.Type, MyListDictionary<MemberInfo, MemberInfo>> m_watch = new Dictionary<System.Type, MyListDictionary<MemberInfo, MemberInfo>>();
            private List<List<object>> m_history = new List<List<object>>();
            private bool m_showWatch;
            private bool m_showOnScreenWatch;
            private LiveWatch m_form;
            private float m_scale = 2f;
            private HashSet<int> m_toPlot = new HashSet<int>();
            private int m_frame;
            protected static Color[] m_colors;

            static LiveWatchComponent()
            {
                Color[] colorArray1 = new Color[0x13];
                colorArray1[0] = new Color(0, 0xc0, 0xc0);
                colorArray1[1] = Color.Orange;
                colorArray1[2] = Color.BlueViolet * 1.5f;
                colorArray1[3] = Color.BurlyWood;
                colorArray1[4] = Color.Chartreuse;
                colorArray1[5] = Color.CornflowerBlue;
                colorArray1[6] = Color.Cyan;
                colorArray1[7] = Color.ForestGreen;
                colorArray1[8] = Color.Fuchsia;
                colorArray1[9] = Color.Gold;
                colorArray1[10] = Color.GreenYellow;
                colorArray1[11] = Color.LightBlue;
                colorArray1[12] = Color.LightGreen;
                colorArray1[13] = Color.LimeGreen;
                colorArray1[14] = Color.Magenta;
                colorArray1[15] = Color.MintCream;
                colorArray1[0x10] = Color.Orchid;
                colorArray1[0x11] = Color.PeachPuff;
                colorArray1[0x12] = Color.Purple;
                m_colors = colorArray1;
            }

            public LiveWatchComponent()
            {
                MyHonzaInputComponent.OnSelectedEntityChanged += new Action(this.MyHonzaInputComponent_OnSelectedEntityChanged);
                base.AddSwitch(MyKeys.NumPad9, delegate (MyKeys key) {
                    if (this.m_form == null)
                    {
                        new Thread(delegate {
                            this.m_form = new LiveWatch();
                            this.m_form.Show();
                            RenderLoop.Run(this.m_form, new SharpDX.Windows.RenderLoop.RenderCallback(this.RenderCallback), false);
                        }).Start();
                    }
                    else if (this.m_form.IsDisposed)
                    {
                        this.m_form = null;
                    }
                    else
                    {
                        Action method = delegate {
                            this.m_form.Close();
                            this.m_form = null;
                        };
                        this.m_form.Invoke(method);
                    }
                    return true;
                }, new MyDebugComponent.MyRef<bool>(() => this.m_form != null, null), "External viewer");
                base.AddSwitch(MyKeys.NumPad8, delegate (MyKeys key) {
                    this.m_showOnScreenWatch = !this.m_showOnScreenWatch;
                    return true;
                }, new MyDebugComponent.MyRef<bool>(() => this.m_showOnScreenWatch, null), "External viewer");
            }

            private static float ConvertToFloat(object o)
            {
                float naN = float.NaN;
                int? nullable = o as int?;
                if (nullable != null)
                {
                    naN = (float) nullable.Value;
                }
                float? nullable2 = o as float?;
                if (nullable2 != null)
                {
                    naN = nullable2.Value;
                }
                double? nullable3 = o as double?;
                if (nullable3 != null)
                {
                    naN = (float) nullable3.Value;
                }
                return naN;
            }

            public override unsafe void Draw()
            {
                base.Draw();
                if ((MyHonzaInputComponent.SelectedEntity != null) && this.m_showOnScreenWatch)
                {
                    MyListDictionary<MemberInfo, MemberInfo> dictionary = null;
                    this.m_watch.TryGetValue(this.m_selectedType, out dictionary);
                    if (this.m_showWatch)
                    {
                        this.DrawWatch(dictionary);
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder(MyHonzaInputComponent.SelectedEntity.GetType().Name);
                        System.Type selectedType = this.m_selectedType;
                        this.m_currentInstance = MyHonzaInputComponent.SelectedEntity;
                        foreach (MemberInfo info in this.m_currentPath)
                        {
                            builder.Append(".");
                            builder.Append(info.Name);
                            this.m_currentInstance = info.GetValue(this.m_currentInstance);
                            selectedType = this.m_currentInstance.GetType();
                        }
                        if (selectedType != this.m_lastType)
                        {
                            this.m_lastType = selectedType;
                            this.m_members.Clear();
                            MemberInfo[] fields = selectedType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                            int index = 0;
                            while (true)
                            {
                                if (index >= fields.Length)
                                {
                                    fields = selectedType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                    index = 0;
                                    while (true)
                                    {
                                        if (index >= fields.Length)
                                        {
                                            this.m_members.Sort((x, y) => string.Compare(x.Name, y.Name));
                                            break;
                                        }
                                        MemberInfo info3 = fields[index];
                                        if (info3.DeclaringType == selectedType)
                                        {
                                            this.m_members.Add(info3);
                                        }
                                        index++;
                                    }
                                    break;
                                }
                                MemberInfo item = fields[index];
                                if (item.DeclaringType == selectedType)
                                {
                                    this.m_members.Add(item);
                                }
                                index++;
                            }
                        }
                        Vector2 screenCoord = new Vector2(100f, 50f);
                        MyRenderProxy.DebugDrawText2D(screenCoord, builder.ToString(), Color.White, 0.65f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        float* singlePtr1 = (float*) ref screenCoord.Y;
                        singlePtr1[0] += 20f;
                        for (int i = this.SelectedMember; i < this.m_members.Count; i++)
                        {
                            object obj2 = this.m_members[i].GetValue(this.m_currentInstance);
                            ((obj2 != null) ? obj2.ToString() : "null").Replace("\n", "");
                            float* singlePtr2 = (float*) ref screenCoord.Y;
                            singlePtr2[0] += 12f;
                        }
                    }
                }
            }

            private unsafe void DrawWatch(MyListDictionary<MemberInfo, MemberInfo> watch)
            {
                this.PlotHistory();
                if (watch != null)
                {
                    List<object> item = new CacheList<object>(watch.Values.Count);
                    StringBuilder builder = new StringBuilder();
                    Vector2 screenCoord = new Vector2(100f, 50f);
                    int num = -1;
                    foreach (List<MemberInfo> list2 in watch.Values)
                    {
                        num++;
                        if (num >= this.SelectedMember)
                        {
                            object selectedEntity = MyHonzaInputComponent.SelectedEntity;
                            foreach (MemberInfo info in list2)
                            {
                                builder.Append(".");
                                builder.Append(info.Name);
                                selectedEntity = info.GetValue(selectedEntity);
                            }
                            builder.Append(":");
                            builder.Append(selectedEntity.ToString());
                            MyRenderProxy.DebugDrawText2D(screenCoord, builder.ToString(), this.m_toPlot.Contains(num) ? m_colors[num] : Color.White, 0.55f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            float* singlePtr1 = (float*) ref screenCoord.Y;
                            singlePtr1[0] += 12f;
                            builder.Clear();
                            item.Add(selectedEntity);
                        }
                    }
                    screenCoord.X = 90f;
                    foreach (int num2 in this.m_toPlot)
                    {
                        int num3 = num2 - this.SelectedMember;
                        if (num3 >= 0)
                        {
                            screenCoord.Y = 50 + (num3 * 12);
                            MyRenderProxy.DebugDrawText2D(screenCoord, "*", m_colors[num2], 0.55f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        }
                    }
                    this.m_history.Add(item);
                    if (this.m_history.Count >= this.MAX_HISTORY)
                    {
                        this.m_history.RemoveAtFast<List<object>>(this.m_frame);
                    }
                    this.m_frame++;
                    this.m_frame = this.m_frame % this.MAX_HISTORY;
                }
            }

            public override string GetName() => 
                "LiveWatch";

            public override bool HandleInput() => 
                base.HandleInput();

            private void MyHonzaInputComponent_OnSelectedEntityChanged()
            {
                if ((MyHonzaInputComponent.SelectedEntity != null) && (this.m_selectedType != MyHonzaInputComponent.SelectedEntity.GetType()))
                {
                    this.m_selectedType = MyHonzaInputComponent.SelectedEntity.GetType();
                    this.m_members.Clear();
                    this.m_currentPath.Clear();
                }
            }

            private unsafe void PlotHistory()
            {
                int num = 0;
                Vector2 pointFrom = new Vector2(100f, 400f);
                Vector2 pointTo = pointFrom;
                float* singlePtr1 = (float*) ref pointTo.X;
                singlePtr1[0]++;
                Matrix? projection = null;
                MyRenderProxy.DebugDrawLine2D(new Vector2(pointFrom.X, pointFrom.Y - 200f), new Vector2(pointFrom.X + 1000f, pointFrom.Y - 200f), Color.Gray, Color.Gray, projection, false);
                projection = null;
                MyRenderProxy.DebugDrawLine2D(new Vector2(pointFrom.X, pointFrom.Y + 200f), new Vector2(pointFrom.X + 1000f, pointFrom.Y + 200f), Color.Gray, Color.Gray, projection, false);
                projection = null;
                MyRenderProxy.DebugDrawLine2D(new Vector2(pointFrom.X, pointFrom.Y), new Vector2(pointFrom.X + 1000f, pointFrom.Y), Color.Gray, Color.Gray, projection, false);
                MyRenderProxy.DebugDrawText2D(new Vector2(90f, 200f), (200f / this.m_scale).ToString(), Color.White, 0.55f, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, false);
                MyRenderProxy.DebugDrawText2D(new Vector2(90f, 600f), (-200f / this.m_scale).ToString(), Color.White, 0.55f, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, false);
                for (int i = Math.Min(0x3e8, this.m_history.Count); i > 0; i--)
                {
                    int num4 = ((this.m_frame + this.m_history.Count) - i) % this.m_history.Count;
                    List<object> list = this.m_history[num4];
                    List<object> list2 = this.m_history[(num4 + 1) % this.m_history.Count];
                    num++;
                    foreach (int num5 in this.m_toPlot)
                    {
                        if (list.Count <= num5)
                        {
                            continue;
                        }
                        if (list2.Count > num5)
                        {
                            object o = list[num5];
                            if (o.GetType().IsPrimitive)
                            {
                                pointFrom.Y = 400f - (ConvertToFloat(o) * this.m_scale);
                                pointTo.Y = 400f - (ConvertToFloat(list2[num5]) * this.m_scale);
                                if (num == 1)
                                {
                                    pointFrom.Y = pointTo.Y;
                                }
                                if (i < 3)
                                {
                                    pointTo.Y = pointFrom.Y;
                                }
                                projection = null;
                                MyRenderProxy.DebugDrawLine2D(pointFrom, pointTo, m_colors[num5], m_colors[num5], projection, false);
                            }
                        }
                    }
                    float* singlePtr2 = (float*) ref pointFrom.X;
                    singlePtr2[0]++;
                    float* singlePtr3 = (float*) ref pointTo.X;
                    singlePtr3[0]++;
                }
            }

            private void RenderCallback()
            {
                if (this.m_form.propertyGrid1.SelectedObject != MyHonzaInputComponent.SelectedEntity)
                {
                    this.m_form.propertyGrid1.SelectedObject = MyHonzaInputComponent.SelectedEntity;
                }
                if (this.m_showWatch)
                {
                    MyListDictionary<MemberInfo, MemberInfo> dictionary = null;
                    this.m_watch.TryGetValue(this.m_selectedType, out dictionary);
                    if (dictionary != null)
                    {
                        int count = Math.Max(0, dictionary.Values.Count - this.m_form.Watch.Rows.Count);
                        if (count != 0)
                        {
                            this.m_form.Watch.Rows.Add(count);
                        }
                        int num2 = 0;
                        using (Dictionary<MemberInfo, List<MemberInfo>>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                object selectedEntity = MyHonzaInputComponent.SelectedEntity;
                                MemberInfo info = null;
                                foreach (MemberInfo local1 in enumerator.Current)
                                {
                                    selectedEntity = local1.GetValue(selectedEntity);
                                    info = local1;
                                }
                                (this.m_form.Watch.Rows[num2].Cells[0] as DataGridViewTextBoxCell).Value = info.Name;
                                (this.m_form.Watch.Rows[num2].Cells[1] as DataGridViewTextBoxCell).Value = selectedEntity.ToString();
                                num2++;
                            }
                        }
                    }
                }
            }

            private int SelectedMember
            {
                get
                {
                    int num = (int) (MyHonzaInputComponent.m_counter * 0.005f);
                    return (!this.m_showWatch ? Math.Min(Math.Max(num, 0), this.m_members.Count - 1) : (!this.m_watch.ContainsKey(this.m_selectedType) ? 0 : Math.Min(Math.Max(num, 0), this.m_watch[this.m_selectedType].Values.Count - 1)));
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyHonzaInputComponent.LiveWatchComponent.<>c <>9 = new MyHonzaInputComponent.LiveWatchComponent.<>c();
                public static Comparison<MemberInfo> <>9__20_0;

                internal int <Draw>b__20_0(MemberInfo x, MemberInfo y) => 
                    string.Compare(x.Name, y.Name);
            }
        }

        public class PhysicsComponent : MyDebugComponent
        {
            public PhysicsComponent()
            {
                this.AddShortcut(MyKeys.W, true, true, false, false, () => "Debug Draw", delegate {
                    MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                    return true;
                });
                this.AddShortcut(MyKeys.Q, true, true, false, false, () => "Draw Physics Shapes", delegate {
                    MyDebugDrawSettings.DEBUG_DRAW_PHYSICS = true;
                    MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES = !MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES;
                    return true;
                });
                this.AddShortcut(MyKeys.C, true, true, false, false, () => "Draw Physics Constraints", delegate {
                    MyDebugDrawSettings.DEBUG_DRAW_PHYSICS = true;
                    MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS = !MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS;
                    return false;
                });
                this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Wheel multiplier x1.5x: " + MyPhysicsConfig.WheelSlipCutAwayRatio.ToString("F2"), delegate {
                    MyPhysicsConfig.WheelSlipCutAwayRatio *= 1.5f;
                    return true;
                });
                this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Wheel multiplier /1.5x: " + MyPhysicsConfig.WheelSlipCutAwayRatio.ToString("F2"), delegate {
                    MyPhysicsConfig.WheelSlipCutAwayRatio /= 1.5f;
                    return true;
                });
            }

            public override string GetName() => 
                "Physics";

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyHonzaInputComponent.PhysicsComponent.<>c <>9 = new MyHonzaInputComponent.PhysicsComponent.<>c();
                public static Func<string> <>9__0_0;
                public static Func<bool> <>9__0_1;
                public static Func<string> <>9__0_2;
                public static Func<bool> <>9__0_3;
                public static Func<string> <>9__0_4;
                public static Func<bool> <>9__0_5;
                public static Func<string> <>9__0_6;
                public static Func<bool> <>9__0_7;
                public static Func<string> <>9__0_8;
                public static Func<bool> <>9__0_9;

                internal string <.ctor>b__0_0() => 
                    "Debug Draw";

                internal bool <.ctor>b__0_1()
                {
                    MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                    return true;
                }

                internal string <.ctor>b__0_2() => 
                    "Draw Physics Shapes";

                internal bool <.ctor>b__0_3()
                {
                    MyDebugDrawSettings.DEBUG_DRAW_PHYSICS = true;
                    MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES = !MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES;
                    return true;
                }

                internal string <.ctor>b__0_4() => 
                    "Draw Physics Constraints";

                internal bool <.ctor>b__0_5()
                {
                    MyDebugDrawSettings.DEBUG_DRAW_PHYSICS = true;
                    MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS = !MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS;
                    return false;
                }

                internal string <.ctor>b__0_6() => 
                    ("Wheel multiplier x1.5x: " + MyPhysicsConfig.WheelSlipCutAwayRatio.ToString("F2"));

                internal bool <.ctor>b__0_7()
                {
                    MyPhysicsConfig.WheelSlipCutAwayRatio *= 1.5f;
                    return true;
                }

                internal string <.ctor>b__0_8() => 
                    ("Wheel multiplier /1.5x: " + MyPhysicsConfig.WheelSlipCutAwayRatio.ToString("F2"));

                internal bool <.ctor>b__0_9()
                {
                    MyPhysicsConfig.WheelSlipCutAwayRatio /= 1.5f;
                    return true;
                }
            }
        }
    }
}

