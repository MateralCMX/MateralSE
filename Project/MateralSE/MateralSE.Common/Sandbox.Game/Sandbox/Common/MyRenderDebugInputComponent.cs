namespace Sandbox.Common
{
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Input;
    using VRageMath;
    using VRageRender;

    public class MyRenderDebugInputComponent : MyDebugComponent
    {
        public static List<object> CheckedObjects = new List<object>();
        [CompilerGenerated]
        private static Action OnDraw;
        public static List<Tuple<BoundingBoxD, Color>> AABBsToDraw = new List<Tuple<BoundingBoxD, Color>>();
        public static List<Tuple<Matrix, Color>> MatricesToDraw = new List<Tuple<Matrix, Color>>();
        public static List<Tuple<CapsuleD, Color>> CapsulesToDraw = new List<Tuple<CapsuleD, Color>>();
        public static List<Tuple<Vector3, Vector3, Color>> LinesToDraw = new List<Tuple<Vector3, Vector3, Color>>();

        public static  event Action OnDraw
        {
            [CompilerGenerated] add
            {
                Action onDraw = OnDraw;
                while (true)
                {
                    Action a = onDraw;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onDraw = Interlocked.CompareExchange<Action>(ref OnDraw, action3, a);
                    if (ReferenceEquals(onDraw, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onDraw = OnDraw;
                while (true)
                {
                    Action source = onDraw;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onDraw = Interlocked.CompareExchange<Action>(ref OnDraw, action3, source);
                    if (ReferenceEquals(onDraw, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyRenderDebugInputComponent()
        {
            this.AddShortcut(MyKeys.C, true, true, false, false, () => "Clears the drawed objects", () => this.ClearObjects());
        }

        public static void AddAABB(BoundingBoxD aabb, Color col)
        {
            AABBsToDraw.Add(new Tuple<BoundingBoxD, Color>(aabb, col));
        }

        public static void AddCapsule(CapsuleD capsule, Color col)
        {
            CapsulesToDraw.Add(new Tuple<CapsuleD, Color>(capsule, col));
        }

        public static void AddLine(Vector3 from, Vector3 to, Color color)
        {
            LinesToDraw.Add(new Tuple<Vector3, Vector3, Color>(from, to, color));
        }

        public static void AddMatrix(Matrix mat, Color col)
        {
            MatricesToDraw.Add(new Tuple<Matrix, Color>(mat, col));
        }

        public static void BreakIfChecked(object objectToCheck)
        {
            if (CheckedObjects.Contains(objectToCheck))
            {
                Debugger.Break();
            }
        }

        public static void Clear()
        {
            AABBsToDraw.Clear();
            MatricesToDraw.Clear();
            CapsulesToDraw.Clear();
            LinesToDraw.Clear();
            OnDraw = null;
        }

        private bool ClearObjects()
        {
            Clear();
            return true;
        }

        public override void Draw()
        {
            base.Draw();
            if (OnDraw != null)
            {
                try
                {
                    OnDraw();
                }
                catch (Exception)
                {
                    OnDraw = null;
                }
            }
            foreach (Tuple<BoundingBoxD, Color> tuple in AABBsToDraw)
            {
                MyRenderProxy.DebugDrawAABB(tuple.Item1, tuple.Item2, 1f, 1f, false, false, false);
            }
            foreach (Tuple<Matrix, Color> tuple2 in MatricesToDraw)
            {
                MyRenderProxy.DebugDrawAxis(tuple2.Item1, 1f, false, false, false);
                MyRenderProxy.DebugDrawOBB(tuple2.Item1, tuple2.Item2, 1f, false, false, true, false);
            }
            foreach (Tuple<Vector3, Vector3, Color> tuple3 in LinesToDraw)
            {
                MyRenderProxy.DebugDrawLine3D(tuple3.Item1, tuple3.Item2, tuple3.Item3, tuple3.Item3, false, false);
            }
        }

        public override string GetName() => 
            "Render";

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRenderDebugInputComponent.<>c <>9 = new MyRenderDebugInputComponent.<>c();
            public static Func<string> <>9__8_0;

            internal string <.ctor>b__8_0() => 
                "Clears the drawed objects";
        }
    }
}

