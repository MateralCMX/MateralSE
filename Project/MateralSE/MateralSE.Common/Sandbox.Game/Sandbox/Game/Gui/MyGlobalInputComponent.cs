namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.Components;
    using VRage.Input;
    using VRageMath;

    internal class MyGlobalInputComponent : MyDebugComponent
    {
        public MyGlobalInputComponent()
        {
            this.AddShortcut(MyKeys.Space, true, true, false, false, () => "Teleport controlled object to camera position", delegate {
                if (ReferenceEquals(MySession.Static.CameraController, MySpectator.Static))
                {
                    MyMultiplayer.TeleportControlledEntity(MySpectator.Static.Position);
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Apply backward linear impulse x100", delegate {
                MyPhysicsComponentBase physics = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).Physics;
                if ((physics != null) && (physics.RigidBody != null))
                {
                    physics.RigidBody.ApplyLinearImpulse((Vector3) ((MySession.Static.ControlledEntity.Entity.WorldMatrix.Forward * physics.Mass) * -100.0));
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Apply linear impulse x100", delegate {
                MyPhysicsComponentBase physics = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).Physics;
                if ((physics != null) && (physics.RigidBody != null))
                {
                    physics.RigidBody.ApplyLinearImpulse((Vector3) ((MySession.Static.ControlledEntity.Entity.WorldMatrix.Forward * physics.Mass) * 100.0));
                }
                return true;
            });
            this.AddShortcut(MyKeys.Z, true, true, true, false, () => "Save clipboard as prefab", delegate {
                MyClipboardComponent.Static.Clipboard.SaveClipboardAsPrefab(null, null);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, delegate {
                if ((MySessionComponentReplay.Static == null) || !MySessionComponentReplay.Static.IsReplaying)
                {
                    return "Replay";
                }
                return "Stop replaying";
            }, delegate {
                if (MySessionComponentReplay.Static != null)
                {
                    if (!MySessionComponentReplay.Static.IsReplaying)
                    {
                        MySessionComponentReplay.Static.StartReplay();
                    }
                    else
                    {
                        MySessionComponentReplay.Static.StopReplay();
                    }
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad6, true, false, false, false, delegate {
                if ((MySessionComponentReplay.Static == null) || !MySessionComponentReplay.Static.IsRecording)
                {
                    return "Record + Replay";
                }
                return "Stop recording ";
            }, delegate {
                if (MySessionComponentReplay.Static != null)
                {
                    if (!MySessionComponentReplay.Static.IsRecording)
                    {
                        MySessionComponentReplay.Static.StartRecording();
                        MySessionComponentReplay.Static.StartReplay();
                    }
                    else
                    {
                        MySessionComponentReplay.Static.StopRecording();
                        MySessionComponentReplay.Static.StopReplay();
                    }
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Delete recordings", delegate {
                MySessionComponentReplay.Static.DeleteRecordings();
                return true;
            });
            this.AddShortcut(MyKeys.U, true, false, false, false, () => "Add character", delegate {
                MyCharacterInputComponent.SpawnCharacter(null);
                return true;
            });
        }

        public override string GetName() => 
            "Global";

        public override bool HandleInput() => 
            ((MySession.Static != null) ? base.HandleInput() : false);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGlobalInputComponent.<>c <>9 = new MyGlobalInputComponent.<>c();
            public static Func<string> <>9__1_0;
            public static Func<bool> <>9__1_1;
            public static Func<string> <>9__1_2;
            public static Func<bool> <>9__1_3;
            public static Func<string> <>9__1_4;
            public static Func<bool> <>9__1_5;
            public static Func<string> <>9__1_6;
            public static Func<bool> <>9__1_7;
            public static Func<string> <>9__1_8;
            public static Func<bool> <>9__1_9;
            public static Func<string> <>9__1_10;
            public static Func<bool> <>9__1_11;
            public static Func<string> <>9__1_12;
            public static Func<bool> <>9__1_13;
            public static Func<string> <>9__1_14;
            public static Func<bool> <>9__1_15;

            internal string <.ctor>b__1_0() => 
                "Teleport controlled object to camera position";

            internal bool <.ctor>b__1_1()
            {
                if (ReferenceEquals(MySession.Static.CameraController, MySpectator.Static))
                {
                    MyMultiplayer.TeleportControlledEntity(MySpectator.Static.Position);
                }
                return true;
            }

            internal string <.ctor>b__1_10()
            {
                if ((MySessionComponentReplay.Static == null) || !MySessionComponentReplay.Static.IsRecording)
                {
                    return "Record + Replay";
                }
                return "Stop recording ";
            }

            internal bool <.ctor>b__1_11()
            {
                if (MySessionComponentReplay.Static != null)
                {
                    if (!MySessionComponentReplay.Static.IsRecording)
                    {
                        MySessionComponentReplay.Static.StartRecording();
                        MySessionComponentReplay.Static.StartReplay();
                    }
                    else
                    {
                        MySessionComponentReplay.Static.StopRecording();
                        MySessionComponentReplay.Static.StopReplay();
                    }
                }
                return true;
            }

            internal string <.ctor>b__1_12() => 
                "Delete recordings";

            internal bool <.ctor>b__1_13()
            {
                MySessionComponentReplay.Static.DeleteRecordings();
                return true;
            }

            internal string <.ctor>b__1_14() => 
                "Add character";

            internal bool <.ctor>b__1_15()
            {
                MyCharacterInputComponent.SpawnCharacter(null);
                return true;
            }

            internal string <.ctor>b__1_2() => 
                "Apply backward linear impulse x100";

            internal bool <.ctor>b__1_3()
            {
                MyPhysicsComponentBase physics = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).Physics;
                if ((physics != null) && (physics.RigidBody != null))
                {
                    physics.RigidBody.ApplyLinearImpulse((Vector3) ((MySession.Static.ControlledEntity.Entity.WorldMatrix.Forward * physics.Mass) * -100.0));
                }
                return true;
            }

            internal string <.ctor>b__1_4() => 
                "Apply linear impulse x100";

            internal bool <.ctor>b__1_5()
            {
                MyPhysicsComponentBase physics = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).Physics;
                if ((physics != null) && (physics.RigidBody != null))
                {
                    physics.RigidBody.ApplyLinearImpulse((Vector3) ((MySession.Static.ControlledEntity.Entity.WorldMatrix.Forward * physics.Mass) * 100.0));
                }
                return true;
            }

            internal string <.ctor>b__1_6() => 
                "Save clipboard as prefab";

            internal bool <.ctor>b__1_7()
            {
                MyClipboardComponent.Static.Clipboard.SaveClipboardAsPrefab(null, null);
                return true;
            }

            internal string <.ctor>b__1_8()
            {
                if ((MySessionComponentReplay.Static == null) || !MySessionComponentReplay.Static.IsReplaying)
                {
                    return "Replay";
                }
                return "Stop replaying";
            }

            internal bool <.ctor>b__1_9()
            {
                if (MySessionComponentReplay.Static != null)
                {
                    if (!MySessionComponentReplay.Static.IsReplaying)
                    {
                        MySessionComponentReplay.Static.StartReplay();
                    }
                    else
                    {
                        MySessionComponentReplay.Static.StopReplay();
                    }
                }
                return true;
            }
        }
    }
}

