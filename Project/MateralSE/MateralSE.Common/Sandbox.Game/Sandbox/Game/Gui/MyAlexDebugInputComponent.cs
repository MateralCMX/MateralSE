namespace Sandbox.Game.Gui
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyAlexDebugInputComponent : MyDebugComponent
    {
        private static bool ShowDebugDrawTests;
        private List<LineInfo> m_lines = new List<LineInfo>();

        public MyAlexDebugInputComponent()
        {
            Static = this;
            this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Clear lines", delegate {
                this.Clear();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "SuitOxygenLevel = 0.35f", delegate {
                MySession.Static.LocalCharacter.OxygenComponent.SuitOxygenLevel = 0.35f;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "SuitOxygenLevel = 0f", delegate {
                MySession.Static.LocalCharacter.OxygenComponent.SuitOxygenLevel = 0f;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "SuitOxygenLevel -= 0.05f", delegate {
                MyCharacterOxygenComponent oxygenComponent = MySession.Static.LocalCharacter.OxygenComponent;
                oxygenComponent.SuitOxygenLevel -= 0.05f;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Deplete battery", delegate {
                MySession.Static.LocalCharacter.SuitBattery.DebugDepleteBattery();
                return true;
            });
            this.AddShortcut(MyKeys.Add, true, true, false, false, () => "SunRotationIntervalMinutes = 1", delegate {
                MySession.Static.Settings.SunRotationIntervalMinutes = 1f;
                return true;
            });
            this.AddShortcut(MyKeys.Subtract, true, true, false, false, () => "SunRotationIntervalMinutes = 1", delegate {
                MySession.Static.Settings.SunRotationIntervalMinutes = -1f;
                return true;
            });
            this.AddShortcut(MyKeys.Space, true, true, false, false, () => "Enable sun rotation: " + ((MySession.Static != null) && MySession.Static.Settings.EnableSunRotation).ToString(), delegate {
                if (MySession.Static == null)
                {
                    return false;
                }
                MySession.Static.Settings.EnableSunRotation = !MySession.Static.Settings.EnableSunRotation;
                return true;
            });
            this.AddShortcut(MyKeys.D, true, true, false, false, () => "Show debug draw tests: " + ShowDebugDrawTests.ToString(), delegate {
                ShowDebugDrawTests = !ShowDebugDrawTests;
                return true;
            });
        }

        public void AddDebugLine(LineInfo line)
        {
            this.m_lines.Add(line);
        }

        public void Clear()
        {
            this.m_lines.Clear();
        }

        public override void Draw()
        {
            base.Draw();
            foreach (LineInfo info in this.m_lines)
            {
                MyRenderProxy.DebugDrawLine3D(info.From, info.To, info.ColorFrom, info.ColorTo, info.DepthRead, false);
            }
            if (ShowDebugDrawTests)
            {
                Vector3D pointFrom = new Vector3D(1000000000.0, 1000000000.0, 1000000000.0);
                MyRenderProxy.DebugDrawLine3D(pointFrom, pointFrom + Vector3D.Up, Color.Red, Color.Blue, true, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawLine3D(pointFrom, pointFrom + Vector3D.Up, Color.Red, Color.Blue, false, false);
                Matrix? projection = null;
                MyRenderProxy.DebugDrawLine2D(new Vector2(10f, 10f), new Vector2(50f, 50f), Color.Red, Color.Blue, projection, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawPoint(pointFrom, Color.White, true, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawPoint(pointFrom, Color.White, false, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawSphere(pointFrom, 0.5f, Color.White, 1f, true, false, true, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawAABB(new BoundingBoxD(pointFrom - (Vector3D.One * 0.5), pointFrom + (Vector3D.One * 0.5)), Color.White, 1f, 1f, true, false, false);
                pointFrom = (pointFrom + Vector3D.Left) + Vector3D.Left;
                MyRenderProxy.DebugDrawAxis(MatrixD.CreateFromTransformScale(Quaternion.Identity, pointFrom, Vector3D.One * 0.5), 1f, true, false, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(pointFrom, Vector3D.One * 0.5, Quaternion.Identity), Color.White, 1f, true, false, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawCylinder(MatrixD.CreateFromTransformScale(Quaternion.Identity, pointFrom, Vector3D.One * 0.5), Color.White, 1f, true, true, false);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawTriangle(pointFrom, pointFrom + Vector3D.Up, pointFrom + Vector3D.Left, Color.White, true, true, false);
                pointFrom += Vector3D.Left;
                MyRenderMessageDebugDrawTriangles triangles1 = MyRenderProxy.PrepareDebugDrawTriangles();
                triangles1.AddTriangle(pointFrom, pointFrom + Vector3D.Up, pointFrom + Vector3D.Left);
                triangles1.AddTriangle(pointFrom, pointFrom + Vector3D.Left, pointFrom - Vector3D.Up);
                pointFrom += Vector3D.Left;
                MyRenderProxy.DebugDrawCapsule(pointFrom, pointFrom + Vector3D.Up, 0.5f, Color.White, true, false, false);
                MyRenderProxy.DebugDrawText2D(new Vector2(100f, 100f), "text", Color.Green, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                MyRenderProxy.DebugDrawText3D(pointFrom + Vector3D.Left, "3D Text", Color.Blue, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        public override string GetName() => 
            "Alex";

        private void ModifyOxygenBottleAmount(float amount)
        {
            using (List<MyPhysicalInventoryItem>.Enumerator enumerator = MySession.Static.LocalCharacter.GetInventory(0).GetItems().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyObjectBuilder_GasContainerObject content = enumerator.Current.Content as MyObjectBuilder_GasContainerObject;
                    if (content != null)
                    {
                        MyOxygenContainerDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(content) as MyOxygenContainerDefinition;
                        if (((amount <= 0f) || (content.GasLevel != 1f)) && ((amount >= 0f) || (content.GasLevel != 0f)))
                        {
                            content.GasLevel += amount / physicalItemDefinition.Capacity;
                            if (content.GasLevel < 0f)
                            {
                                content.GasLevel = 0f;
                            }
                            if (content.GasLevel > 1f)
                            {
                                content.GasLevel = 1f;
                            }
                        }
                    }
                }
            }
        }

        public static MyAlexDebugInputComponent Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAlexDebugInputComponent.<>c <>9 = new MyAlexDebugInputComponent.<>c();
            public static Func<string> <>9__6_0;
            public static Func<string> <>9__6_2;
            public static Func<bool> <>9__6_3;
            public static Func<string> <>9__6_4;
            public static Func<bool> <>9__6_5;
            public static Func<string> <>9__6_6;
            public static Func<bool> <>9__6_7;
            public static Func<string> <>9__6_8;
            public static Func<bool> <>9__6_9;
            public static Func<string> <>9__6_10;
            public static Func<bool> <>9__6_11;
            public static Func<string> <>9__6_12;
            public static Func<bool> <>9__6_13;
            public static Func<string> <>9__6_14;
            public static Func<bool> <>9__6_15;
            public static Func<string> <>9__6_16;
            public static Func<bool> <>9__6_17;

            internal string <.ctor>b__6_0() => 
                "Clear lines";

            internal string <.ctor>b__6_10() => 
                "SunRotationIntervalMinutes = 1";

            internal bool <.ctor>b__6_11()
            {
                MySession.Static.Settings.SunRotationIntervalMinutes = 1f;
                return true;
            }

            internal string <.ctor>b__6_12() => 
                "SunRotationIntervalMinutes = 1";

            internal bool <.ctor>b__6_13()
            {
                MySession.Static.Settings.SunRotationIntervalMinutes = -1f;
                return true;
            }

            internal string <.ctor>b__6_14() => 
                ("Enable sun rotation: " + ((MySession.Static != null) && MySession.Static.Settings.EnableSunRotation).ToString());

            internal bool <.ctor>b__6_15()
            {
                if (MySession.Static == null)
                {
                    return false;
                }
                MySession.Static.Settings.EnableSunRotation = !MySession.Static.Settings.EnableSunRotation;
                return true;
            }

            internal string <.ctor>b__6_16() => 
                ("Show debug draw tests: " + MyAlexDebugInputComponent.ShowDebugDrawTests.ToString());

            internal bool <.ctor>b__6_17()
            {
                MyAlexDebugInputComponent.ShowDebugDrawTests = !MyAlexDebugInputComponent.ShowDebugDrawTests;
                return true;
            }

            internal string <.ctor>b__6_2() => 
                "SuitOxygenLevel = 0.35f";

            internal bool <.ctor>b__6_3()
            {
                MySession.Static.LocalCharacter.OxygenComponent.SuitOxygenLevel = 0.35f;
                return true;
            }

            internal string <.ctor>b__6_4() => 
                "SuitOxygenLevel = 0f";

            internal bool <.ctor>b__6_5()
            {
                MySession.Static.LocalCharacter.OxygenComponent.SuitOxygenLevel = 0f;
                return true;
            }

            internal string <.ctor>b__6_6() => 
                "SuitOxygenLevel -= 0.05f";

            internal bool <.ctor>b__6_7()
            {
                MyCharacterOxygenComponent oxygenComponent = MySession.Static.LocalCharacter.OxygenComponent;
                oxygenComponent.SuitOxygenLevel -= 0.05f;
                return true;
            }

            internal string <.ctor>b__6_8() => 
                "Deplete battery";

            internal bool <.ctor>b__6_9()
            {
                MySession.Static.LocalCharacter.SuitBattery.DebugDepleteBattery();
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LineInfo
        {
            public Vector3 From;
            public Vector3 To;
            public Color ColorFrom;
            public Color ColorTo;
            public bool DepthRead;
            public LineInfo(Vector3 from, Vector3 to, Color colorFrom, Color colorTo, bool depthRead)
            {
                this.From = from;
                this.To = to;
                this.ColorFrom = colorFrom;
                this.ColorTo = colorTo;
                this.DepthRead = depthRead;
            }

            public LineInfo(Vector3 from, Vector3 to, Color colorFrom, bool depthRead) : this(from, to, colorFrom, colorFrom, depthRead)
            {
            }
        }
    }
}

