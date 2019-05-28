namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_Ladder2))]
    public class MyLadder : MyCubeBlock
    {
        [CompilerGenerated]
        private Action<MyCubeGrid> CubeGridChanged;
        private Matrix m_detectorBox = Matrix.Identity;

        public event Action<MyCubeGrid> CubeGridChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> cubeGridChanged = this.CubeGridChanged;
                while (true)
                {
                    Action<MyCubeGrid> a = cubeGridChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    cubeGridChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.CubeGridChanged, action3, a);
                    if (ReferenceEquals(cubeGridChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> cubeGridChanged = this.CubeGridChanged;
                while (true)
                {
                    Action<MyCubeGrid> source = cubeGridChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    cubeGridChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.CubeGridChanged, action3, source);
                    if (ReferenceEquals(cubeGridChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public override unsafe bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? t, IntersectionFlags flags = 3)
        {
            MyOrientedBoundingBoxD xd2 = new MyOrientedBoundingBoxD(this.m_detectorBox * base.PositionComp.WorldMatrix);
            t = 0;
            double? nullable = xd2.Intersects(ref line);
            if (nullable != null)
            {
                MyIntersectionResultLineTriangleEx ex = new MyIntersectionResultLineTriangleEx {
                    Entity = this,
                    IntersectionPointInWorldSpace = line.From + ((nullable.Value + 0.2) * line.Direction)
                };
                MyIntersectionResultLineTriangleEx* exPtr1 = (MyIntersectionResultLineTriangleEx*) ref ex;
                exPtr1->IntersectionPointInObjectSpace = (Vector3) Vector3D.Transform(ex.IntersectionPointInWorldSpace, base.PositionComp.WorldMatrixInvScaled);
                ex.NormalInWorldSpace = (Vector3) -line.Direction;
                MyIntersectionResultLineTriangleEx* exPtr2 = (MyIntersectionResultLineTriangleEx*) ref ex;
                exPtr2->NormalInObjectSpace = (Vector3) Vector3D.TransformNormal(ex.NormalInWorldSpace, base.PositionComp.WorldMatrixInvScaled);
                t = new MyIntersectionResultLineTriangleEx?(ex);
            }
            return (t != 0);
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            this.StartMatrix = Matrix.Identity;
            this.UpdateVisual();
            base.AddDebugRenderComponent(new MyDebugRenderComponentLadder(this));
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            base.OnCubeGridChanged(oldGrid);
            if (this.CubeGridChanged != null)
            {
                this.CubeGridChanged(oldGrid);
            }
        }

        public override void UpdateVisual()
        {
            MyModelDummy dummy;
            MyModelDummy dummy2;
            base.UpdateVisual();
            if (base.Model.Dummies.TryGetValue("astronaut", out dummy))
            {
                this.StartMatrix = dummy.Matrix;
            }
            if (base.Model.Dummies.TryGetValue("detector_ladder_01", out dummy))
            {
                this.m_detectorBox = dummy.Matrix;
            }
            if (base.Model.Dummies.TryGetValue("TopLadder", out dummy))
            {
                this.StopMatrix = dummy.Matrix;
            }
            if (base.Model.Dummies.TryGetValue("pole_1", out dummy) && base.Model.Dummies.TryGetValue("pole_2", out dummy2))
            {
                this.DistanceBetweenPoles = Math.Abs((float) (dummy.Matrix.Translation.Y - dummy2.Matrix.Translation.Y));
            }
        }

        public void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyCharacter character = entity as MyCharacter;
            if ((character != null) && (Sandbox.Game.Entities.MyEntities.IsInsideWorld(character.PositionComp.GetPosition()) && Sandbox.Game.Entities.MyEntities.IsInsideWorld(base.PositionComp.GetPosition())))
            {
                character.GetOnLadder(this);
            }
        }

        public Matrix StartMatrix { get; private set; }

        public Matrix StopMatrix { get; private set; }

        public float DistanceBetweenPoles { get; private set; }
    }
}

