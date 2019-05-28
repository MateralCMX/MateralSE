namespace Sandbox.Game.Gui
{
    using Havok;
    using ParallelTasks;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems.StructuralIntegrity;
    using Sandbox.Game.Screens;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Models;
    using VRage.Input;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRageMath;
    using VRageRender;

    public class MyPetaInputComponent : MyDebugComponent
    {
        public static bool ENABLE_SI_DESTRUCTIONS = true;
        public static bool OLD_SI = false;
        public static bool DEBUG_DRAW_TENSIONS = false;
        public static bool DEBUG_DRAW_PATHS = false;
        public static float SI_DYNAMICS_MULTIPLIER = 1f;
        public static bool SHOW_HUD_ALWAYS = false;
        public static bool DRAW_WARNINGS = true;
        public static int DEBUG_INDEX = 0;
        public static Vector3D MovementDistanceStart;
        public static float MovementDistance = 1f;
        public static int MovementDistanceCounter = -1;
        private static Matrix[] s_viewVectors;
        private MyConcurrentDictionary<MyCubePart, List<uint>> m_cubeParts = new MyConcurrentDictionary<MyCubePart, List<uint>>(0, null);
        private int pauseCounter;
        private bool xPressed;
        private bool cPressed;
        private bool spacePressed;
        private bool objectiveInited;
        private int OBJECTIVE_PAUSE = 200;
        private bool generalObjective;
        private bool f1Pressed;
        private bool gPressed;
        private bool iPressed;
        private const int N = 9;
        private const int NT = 0xb5;
        private MyVoxelMap m_voxelMap;
        private bool recording;
        private bool recorded;
        private bool introduceObjective;
        private bool keysObjective;
        private bool wPressed;
        private bool sPressed;
        private bool aPressed;
        private bool dPressed;
        private bool jetpackObjective;
        private List<MySkinnedEntity> m_skins = new List<MySkinnedEntity>();

        public MyPetaInputComponent()
        {
            this.AddShortcut(MyKeys.OemBackslash, true, true, false, false, () => "Debug draw physics clusters: " + MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS.ToString(), delegate {
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS = !MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS;
                return true;
            });
            this.AddShortcut(MyKeys.OemBackslash, false, false, false, false, () => "Advance all moving entities", delegate {
                AdvanceEntities();
                return true;
            });
            this.AddShortcut(MyKeys.S, true, true, false, false, () => "Insert safe zone", delegate {
                this.InsertSafeZone();
                return true;
            });
            this.AddShortcut(MyKeys.Back, true, true, false, false, () => "Freeze gizmo: " + MyCubeBuilder.Static.FreezeGizmo.ToString(), delegate {
                MyCubeBuilder.Static.FreezeGizmo = !MyCubeBuilder.Static.FreezeGizmo;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Test movement distance: " + MovementDistance, delegate {
                if (MovementDistance != 0f)
                {
                    MovementDistance = 0f;
                    MovementDistanceStart = ((MyEntity) MySession.Static.ControlledEntity).PositionComp.GetPosition();
                    MovementDistanceCounter = (int) SI_DYNAMICS_MULTIPLIER;
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Show warnings: " + DRAW_WARNINGS.ToString(), delegate {
                DRAW_WARNINGS = !DRAW_WARNINGS;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Show logos", delegate {
                MyGuiSandbox.BackToIntroLogos(new Action(MySandboxGame.AfterLogos));
                return true;
            });
            this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Highlight GScreen", delegate {
                HighlightGScreen();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Reset Ingame Help", delegate {
                MySessionComponentIngameHelp component = MySession.Static.GetComponent<MySessionComponentIngameHelp>();
                if (component != null)
                {
                    component.Reset();
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "SI Debug draw paths", delegate {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
                {
                    MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY = true;
                    DEBUG_DRAW_PATHS = true;
                    DEBUG_DRAW_TENSIONS = false;
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "SI Debug draw tensions", delegate {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
                {
                    MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY = true;
                    DEBUG_DRAW_PATHS = false;
                    DEBUG_DRAW_TENSIONS = true;
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Move VCs to ships and fly at 20m/s speed", delegate {
                this.MoveVCToShips();
                return true;
            });
            this.AddShortcut(MyKeys.Up, true, false, false, false, () => "SI Selected cube up", delegate {
                if (!MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY)
                {
                    return false;
                }
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y + 1, MyAdvancedStaticSimulator.SelectedCube.Z);
                return true;
            });
            this.AddShortcut(MyKeys.Down, true, false, false, false, () => "SI Selected cube down", delegate {
                if (!MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY)
                {
                    return false;
                }
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y - 1, MyAdvancedStaticSimulator.SelectedCube.Z);
                return true;
            });
            this.AddShortcut(MyKeys.Left, true, false, false, false, () => "Debug index--", delegate {
                DEBUG_INDEX--;
                if (DEBUG_INDEX < 0)
                {
                    DEBUG_INDEX = 6;
                }
                MyDebugDrawSettings.DEBUG_DRAW_DISPLACED_BONES = true;
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                return true;
            });
            this.AddShortcut(MyKeys.Right, true, false, false, false, () => "Debug index++", delegate {
                DEBUG_INDEX++;
                if (DEBUG_INDEX > 6)
                {
                    DEBUG_INDEX = 0;
                }
                MyDebugDrawSettings.DEBUG_DRAW_DISPLACED_BONES = true;
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                return true;
            });
            this.AddShortcut(MyKeys.Up, true, true, false, false, () => "SI Selected cube forward", delegate {
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y, MyAdvancedStaticSimulator.SelectedCube.Z - 1);
                return true;
            });
            this.AddShortcut(MyKeys.Down, true, true, false, false, () => "SI Selected cube back", delegate {
                if (!MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY)
                {
                    return false;
                }
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y, MyAdvancedStaticSimulator.SelectedCube.Z + 1);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Spawn simple skinned object", delegate {
                this.SpawnSimpleSkinnedObject();
                return true;
            });
        }

        private static unsafe void AdvanceEntities()
        {
            foreach (MyEntity entity in MyEntities.GetEntities().ToList<MyEntity>())
            {
                if (entity.Physics == null)
                {
                    continue;
                }
                if (entity.Physics.LinearVelocity.Length() > 0.1f)
                {
                    Vector3D vectord = (entity.Physics.LinearVelocity * SI_DYNAMICS_MULTIPLIER) * 100000f;
                    MatrixD worldMatrix = entity.WorldMatrix;
                    MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                    xdPtr1.Translation += vectord;
                    entity.WorldMatrix = worldMatrix;
                }
            }
        }

        public override void Draw()
        {
            if (MySector.MainCamera != null)
            {
                base.Draw();
                if (this.m_voxelMap != null)
                {
                    MyRenderProxy.DebugDrawAxis(this.m_voxelMap.WorldMatrix, 100f, false, false, false);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_FRACTURED_PIECES)
                {
                    foreach (MyFracturedPiece piece in MyEntities.GetEntities())
                    {
                        if (piece != null)
                        {
                            MyPhysicsDebugDraw.DebugDrawBreakable(piece.Physics.BreakableBody, (Vector3) piece.Physics.ClusterToWorld((Vector3) Vector3D.Zero));
                        }
                    }
                }
            }
        }

        private unsafe void findViews(int species, Vector3 cDIR, out Vector3I vv, out Vector3 rr)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3 vector = new Vector3(cDIR.X, Math.Max(-cDIR.Y, 0.01f), cDIR.Z);
            float num = (Math.Abs(vector.X) > Math.Abs(vector.Z)) ? (-vector.Z / vector.X) : (-vector.X / -vector.Z);
            float num2 = ((9f * (1f - num)) * ((float) Math.Acos((double) MathHelper.Clamp(vector.Y, -1f, 1f)))) / 3.141593f;
            int z = (int) Math.Floor((double) num2);
            float single1 = ((9f * (1f + num)) * ((float) Math.Acos((double) MathHelper.Clamp(vector.Y, -1f, 1f)))) / 3.141593f;
            int y = (int) Math.Floor((double) single1);
            float num5 = num2 - z;
            float num6 = single1 - y;
            float num7 = (1f - num5) - num6;
            bool flag = num7 > 0.0;
            Vector3I* vectoriPtr1 = (Vector3I*) new Vector3I(flag ? z : (z + 1), z + 1, z);
            Vector3I* vectoriPtr2 = (Vector3I*) new Vector3I(flag ? y : (y + 1), y, y + 1);
            rr = new Vector3((double) Math.Abs(num7), flag ? ((double) num5) : (1.0 - num6), flag ? ((double) num6) : (1.0 - num5));
            if (Math.Abs(vector.Z) >= Math.Abs(vector.X))
            {
                vectoriPtr1 = (Vector3I*) ref vectori;
                vectoriPtr2 = (Vector3I*) ref vectori2;
                vectori = -vectori2;
                vectori2 = vectori;
            }
            if (Math.Abs((float) (vector.X + -vector.Z)) > 1E-05f)
            {
                vectori *= Math.Sign((float) (vector.X + -vector.Z));
                vectori2 *= Math.Sign((float) (vector.X + -vector.Z));
            }
            vv = (Vector3I) (new Vector3I(species * 0xb5) + new Vector3I(this.viewNumber(vectori.X, vectori2.X), this.viewNumber(vectori.Y, vectori2.Y), this.viewNumber(vectori.Z, vectori2.Z)));
        }

        public override string GetName() => 
            "Peta";

        public override bool HandleInput()
        {
            if (base.HandleInput())
            {
                return true;
            }
            return false;
        }

        private static void HighlightGScreen()
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            MyGuiScreenHighlight.MyHighlightControl control = new MyGuiScreenHighlight.MyHighlightControl {
                Control = screenWithFocus.GetControlByName("ScrollablePanel").Elements[0]
            };
            int[] numArray1 = new int[3];
            numArray1[1] = 1;
            numArray1[2] = 2;
            control.Indices = numArray1;
            MyGuiScreenHighlight.MyHighlightControl[] controlsData = new MyGuiScreenHighlight.MyHighlightControl[3];
            controlsData[0] = control;
            control = new MyGuiScreenHighlight.MyHighlightControl {
                Control = screenWithFocus.GetControlByName("MyGuiControlGridDragAndDrop")
            };
            controlsData[1] = control;
            control = new MyGuiScreenHighlight.MyHighlightControl {
                Control = screenWithFocus.GetControlByName("MyGuiControlToolbar").Elements[2],
                Indices = new int[1]
            };
            controlsData[2] = control;
            MyGuiScreenHighlight.HighlightControls(controlsData);
        }

        private void InsertSafeZone()
        {
            ((MyEntity) MySession.Static.ControlledEntity).PositionComp.SetPosition(((MyEntity) MySession.Static.ControlledEntity).PositionComp.GetPosition() + new Vector3D(double.PositiveInfinity), null, false, true);
        }

        private void InsertTree()
        {
            MyDefinitionId id = new MyDefinitionId(MyObjectBuilderType.Parse("MyObjectBuilder_Tree"), "Tree04_v2");
            MyEnvironmentItemDefinition environmentItemDefinition = MyDefinitionManager.Static.GetEnvironmentItemDefinition(id);
            if (MyModels.GetModelOnlyData(environmentItemDefinition.Model).HavokBreakableShapes != null)
            {
                HkdBreakableShape shape = MyModels.GetModelOnlyData(environmentItemDefinition.Model).HavokBreakableShapes[0].Clone();
                MatrixD worldMatrix = MatrixD.CreateWorld(MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition() + (2.0 * MySession.Static.ControlledEntity.Entity.WorldMatrix.Forward), Vector3.Forward, Vector3.Up);
                List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                shape.GetChildren(list);
                list[0].Shape.SetFlagRecursively(HkdBreakableShape.Flags.IS_FIXED);
                MyDestructionHelper.CreateFracturePiece(shape, ref worldMatrix, false, new MyDefinitionId?(environmentItemDefinition.Id), true);
            }
        }

        private void MoveVCToShips()
        {
            List<MyCharacter> list = new List<MyCharacter>();
            foreach (MyCharacter character in MyEntities.GetEntities())
            {
                if (character == null)
                {
                    continue;
                }
                if (!character.ControllerInfo.IsLocallyHumanControlled() && character.ControllerInfo.IsLocallyControlled())
                {
                    list.Add(character);
                }
            }
            List<MyCubeGrid> list2 = new List<MyCubeGrid>();
            ConcurrentEnumerator<SpinLockRef.Token, MyEntity, HashSet<MyEntity>.Enumerator> enumerator = MyEntities.GetEntities().GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    MyCubeGrid current = enumerator.Current as MyCubeGrid;
                    if ((current != null) && (!current.GridSystems.ControlSystem.IsControlled && ((current.GridSizeEnum == MyCubeSize.Large) && !current.IsStatic)))
                    {
                        list2.Add(current);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
                goto TR_000D;
            }
        TR_0002:
            if ((list.Count <= 0) || (list2.Count <= 0))
            {
                return;
            }
        TR_000D:
            while (true)
            {
                MyCharacter user = list[0];
                list.RemoveAt(0);
                list2.RemoveAt(0);
                List<MyCockpit> list3 = new List<MyCockpit>();
                foreach (MyCockpit cockpit in list2[0].GetFatBlocks())
                {
                    if (cockpit == null)
                    {
                        continue;
                    }
                    if (cockpit.BlockDefinition.EnableShipControl)
                    {
                        list3.Add(cockpit);
                    }
                }
                list3[0].RequestUse(UseActionEnum.Manipulate, user);
                break;
            }
            goto TR_0002;
        }

        private void SpawnSimpleSkinnedObject()
        {
            MySkinnedEntity entity = new MySkinnedEntity();
            MyObjectBuilder_Character objectBuilder = new MyObjectBuilder_Character {
                EntityDefinitionId = new SerializableDefinitionId(typeof(MyObjectBuilder_Character), "Medieval_barbarian"),
                PositionAndOrientation = new MyPositionAndOrientation(MySector.MainCamera.Position + (2f * MySector.MainCamera.ForwardVector), MySector.MainCamera.ForwardVector, MySector.MainCamera.UpVector)
            };
            float? scale = null;
            entity.Init(null, @"Models\Characters\Basic\ME_barbar.mwm", null, scale, null);
            entity.Init(objectBuilder);
            MyEntities.Add(entity, true);
            MyAnimationCommand command = new MyAnimationCommand {
                AnimationSubtypeName = "IdleBarbar",
                FrameOption = MyFrameOption.Loop,
                TimeScale = 1f
            };
            entity.AddCommand(command, false);
            this.m_skins.Add(entity);
        }

        private void TestIngameHelp()
        {
            MyHud.Questlog.Visible = true;
            this.objectiveInited = false;
            this.introduceObjective = true;
            this.keysObjective = false;
            this.wPressed = false;
            this.sPressed = false;
            this.aPressed = false;
            this.dPressed = false;
        }

        private void TestParallelDictionary()
        {
            WorkOptions? options = null;
            Parallel.For(0, 0x3e8, delegate (int x) {
                switch (MyRandom.Instance.Next(5))
                {
                    case 0:
                    {
                        List<uint> list1 = new List<uint>();
                        list1.Add(0);
                        list1.Add(1);
                        list1.Add(2);
                        this.m_cubeParts.TryAdd(new MyCubePart(), list1);
                        return;
                    }
                    case 1:
                        foreach (KeyValuePair<MyCubePart, List<uint>> local1 in this.m_cubeParts)
                        {
                            Thread.Sleep(10);
                        }
                        return;

                    case 2:
                        break;

                    case 3:
                        foreach (KeyValuePair<MyCubePart, List<uint>> local2 in this.m_cubeParts)
                        {
                            Thread.Sleep(1);
                        }
                        return;

                    default:
                        return;
                }
                if (this.m_cubeParts.Count > 0)
                {
                    this.m_cubeParts.Remove(this.m_cubeParts.First<KeyValuePair<MyCubePart, List<uint>>>().Key);
                }
            }, WorkPriority.Normal, options);
        }

        private int viewNumber(int i, int j) => 
            (((i * (0x13 - Math.Abs(i))) + j) + 90);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPetaInputComponent.<>c <>9 = new MyPetaInputComponent.<>c();
            public static Func<string> <>9__13_0;
            public static Func<bool> <>9__13_1;
            public static Func<string> <>9__13_2;
            public static Func<bool> <>9__13_3;
            public static Func<string> <>9__13_4;
            public static Func<string> <>9__13_6;
            public static Func<bool> <>9__13_7;
            public static Func<string> <>9__13_8;
            public static Func<bool> <>9__13_9;
            public static Func<string> <>9__13_10;
            public static Func<bool> <>9__13_11;
            public static Func<string> <>9__13_12;
            public static Func<bool> <>9__13_13;
            public static Func<string> <>9__13_14;
            public static Func<bool> <>9__13_15;
            public static Func<string> <>9__13_16;
            public static Func<bool> <>9__13_17;
            public static Func<string> <>9__13_18;
            public static Func<bool> <>9__13_19;
            public static Func<string> <>9__13_20;
            public static Func<bool> <>9__13_21;
            public static Func<string> <>9__13_22;
            public static Func<string> <>9__13_24;
            public static Func<bool> <>9__13_25;
            public static Func<string> <>9__13_26;
            public static Func<bool> <>9__13_27;
            public static Func<string> <>9__13_28;
            public static Func<bool> <>9__13_29;
            public static Func<string> <>9__13_30;
            public static Func<bool> <>9__13_31;
            public static Func<string> <>9__13_32;
            public static Func<bool> <>9__13_33;
            public static Func<string> <>9__13_34;
            public static Func<bool> <>9__13_35;
            public static Func<string> <>9__13_36;

            internal string <.ctor>b__13_0() => 
                ("Debug draw physics clusters: " + MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS.ToString());

            internal bool <.ctor>b__13_1()
            {
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS = !MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS;
                return true;
            }

            internal string <.ctor>b__13_10() => 
                ("Show warnings: " + MyPetaInputComponent.DRAW_WARNINGS.ToString());

            internal bool <.ctor>b__13_11()
            {
                MyPetaInputComponent.DRAW_WARNINGS = !MyPetaInputComponent.DRAW_WARNINGS;
                return true;
            }

            internal string <.ctor>b__13_12() => 
                "Show logos";

            internal bool <.ctor>b__13_13()
            {
                MyGuiSandbox.BackToIntroLogos(new Action(MySandboxGame.AfterLogos));
                return true;
            }

            internal string <.ctor>b__13_14() => 
                "Highlight GScreen";

            internal bool <.ctor>b__13_15()
            {
                MyPetaInputComponent.HighlightGScreen();
                return true;
            }

            internal string <.ctor>b__13_16() => 
                "Reset Ingame Help";

            internal bool <.ctor>b__13_17()
            {
                MySessionComponentIngameHelp component = MySession.Static.GetComponent<MySessionComponentIngameHelp>();
                if (component != null)
                {
                    component.Reset();
                }
                return true;
            }

            internal string <.ctor>b__13_18() => 
                "SI Debug draw paths";

            internal bool <.ctor>b__13_19()
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
                {
                    MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY = true;
                    MyPetaInputComponent.DEBUG_DRAW_PATHS = true;
                    MyPetaInputComponent.DEBUG_DRAW_TENSIONS = false;
                }
                return true;
            }

            internal string <.ctor>b__13_2() => 
                "Advance all moving entities";

            internal string <.ctor>b__13_20() => 
                "SI Debug draw tensions";

            internal bool <.ctor>b__13_21()
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
                {
                    MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY = true;
                    MyPetaInputComponent.DEBUG_DRAW_PATHS = false;
                    MyPetaInputComponent.DEBUG_DRAW_TENSIONS = true;
                }
                return true;
            }

            internal string <.ctor>b__13_22() => 
                "Move VCs to ships and fly at 20m/s speed";

            internal string <.ctor>b__13_24() => 
                "SI Selected cube up";

            internal bool <.ctor>b__13_25()
            {
                if (!MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY)
                {
                    return false;
                }
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y + 1, MyAdvancedStaticSimulator.SelectedCube.Z);
                return true;
            }

            internal string <.ctor>b__13_26() => 
                "SI Selected cube down";

            internal bool <.ctor>b__13_27()
            {
                if (!MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY)
                {
                    return false;
                }
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y - 1, MyAdvancedStaticSimulator.SelectedCube.Z);
                return true;
            }

            internal string <.ctor>b__13_28() => 
                "Debug index--";

            internal bool <.ctor>b__13_29()
            {
                MyPetaInputComponent.DEBUG_INDEX--;
                if (MyPetaInputComponent.DEBUG_INDEX < 0)
                {
                    MyPetaInputComponent.DEBUG_INDEX = 6;
                }
                MyDebugDrawSettings.DEBUG_DRAW_DISPLACED_BONES = true;
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                return true;
            }

            internal bool <.ctor>b__13_3()
            {
                MyPetaInputComponent.AdvanceEntities();
                return true;
            }

            internal string <.ctor>b__13_30() => 
                "Debug index++";

            internal bool <.ctor>b__13_31()
            {
                MyPetaInputComponent.DEBUG_INDEX++;
                if (MyPetaInputComponent.DEBUG_INDEX > 6)
                {
                    MyPetaInputComponent.DEBUG_INDEX = 0;
                }
                MyDebugDrawSettings.DEBUG_DRAW_DISPLACED_BONES = true;
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                return true;
            }

            internal string <.ctor>b__13_32() => 
                "SI Selected cube forward";

            internal bool <.ctor>b__13_33()
            {
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y, MyAdvancedStaticSimulator.SelectedCube.Z - 1);
                return true;
            }

            internal string <.ctor>b__13_34() => 
                "SI Selected cube back";

            internal bool <.ctor>b__13_35()
            {
                if (!MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY)
                {
                    return false;
                }
                MyAdvancedStaticSimulator.SelectedCube = new Vector3I(MyAdvancedStaticSimulator.SelectedCube.X, MyAdvancedStaticSimulator.SelectedCube.Y, MyAdvancedStaticSimulator.SelectedCube.Z + 1);
                return true;
            }

            internal string <.ctor>b__13_36() => 
                "Spawn simple skinned object";

            internal string <.ctor>b__13_4() => 
                "Insert safe zone";

            internal string <.ctor>b__13_6() => 
                ("Freeze gizmo: " + MyCubeBuilder.Static.FreezeGizmo.ToString());

            internal bool <.ctor>b__13_7()
            {
                MyCubeBuilder.Static.FreezeGizmo = !MyCubeBuilder.Static.FreezeGizmo;
                return true;
            }

            internal string <.ctor>b__13_8() => 
                ("Test movement distance: " + MyPetaInputComponent.MovementDistance);

            internal bool <.ctor>b__13_9()
            {
                if (MyPetaInputComponent.MovementDistance != 0f)
                {
                    MyPetaInputComponent.MovementDistance = 0f;
                    MyPetaInputComponent.MovementDistanceStart = ((MyEntity) MySession.Static.ControlledEntity).PositionComp.GetPosition();
                    MyPetaInputComponent.MovementDistanceCounter = (int) MyPetaInputComponent.SI_DYNAMICS_MULTIPLIER;
                }
                return true;
            }
        }
    }
}

