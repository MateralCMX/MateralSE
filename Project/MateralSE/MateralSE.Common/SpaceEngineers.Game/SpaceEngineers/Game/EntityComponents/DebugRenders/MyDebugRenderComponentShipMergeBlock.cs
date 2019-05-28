namespace SpaceEngineers.Game.EntityComponents.DebugRenders
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentShipMergeBlock : MyDebugRenderComponent
    {
        private MyShipMergeBlock m_shipMergeBlock;

        public MyDebugRenderComponentShipMergeBlock(MyShipMergeBlock shipConnector) : base(shipConnector)
        {
            this.m_shipMergeBlock = shipConnector;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CONNECTORS_AND_MERGE_BLOCKS)
            {
                Matrix worldMatrix = (Matrix) this.m_shipMergeBlock.PositionComp.WorldMatrix;
                MyRenderProxy.DebugDrawLine3D(this.m_shipMergeBlock.Physics.RigidBody.Position, this.m_shipMergeBlock.Physics.RigidBody.Position + this.m_shipMergeBlock.WorldMatrix.Right, Color.Green, Color.Green, false, false);
                MyRenderProxy.DebugDrawSphere(Vector3.Transform((Vector3) (this.m_shipMergeBlock.Position * this.m_shipMergeBlock.CubeGrid.GridSize), Matrix.Invert((Matrix) this.m_shipMergeBlock.WorldMatrix)), 1f, Color.Green, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawSphere(this.m_shipMergeBlock.WorldMatrix.Translation, 0.2f, this.m_shipMergeBlock.InConstraint ? Color.Yellow : Color.Orange, 1f, false, false, true, false);
                if (this.m_shipMergeBlock.InConstraint)
                {
                    MyRenderProxy.DebugDrawSphere(this.m_shipMergeBlock.Other.WorldMatrix.Translation, 0.2f, Color.Yellow, 1f, false, false, true, false);
                    MyRenderProxy.DebugDrawLine3D(this.m_shipMergeBlock.WorldMatrix.Translation, this.m_shipMergeBlock.Other.WorldMatrix.Translation, Color.Yellow, Color.Yellow, false, false);
                }
                MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, worldMatrix.Translation + this.m_shipMergeBlock.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.GetDirection(this.m_shipMergeBlock.PositionComp.LocalMatrix.Right)), Color.Red, Color.Red, false, false);
                MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, worldMatrix.Translation + this.m_shipMergeBlock.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.GetDirection(this.m_shipMergeBlock.PositionComp.LocalMatrix.Up)), Color.Green, Color.Green, false, false);
                MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, worldMatrix.Translation + this.m_shipMergeBlock.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.GetDirection(this.m_shipMergeBlock.PositionComp.LocalMatrix.Backward)), Color.Blue, Color.Blue, false, false);
                MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, worldMatrix.Translation + this.m_shipMergeBlock.CubeGrid.WorldMatrix.GetDirectionVector(this.m_shipMergeBlock.OtherRight), Color.Violet, Color.Violet, false, false);
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, "Bodies: " + this.m_shipMergeBlock.GridCount, Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                if (this.m_shipMergeBlock.Other != null)
                {
                    MyRenderProxy.DebugDrawText3D(worldMatrix.Translation + (this.m_shipMergeBlock.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.GetDirection(this.m_shipMergeBlock.PositionComp.LocalMatrix.Up)) * 0.5), ((float) Math.Exp(-((worldMatrix.Translation - this.m_shipMergeBlock.Other.WorldMatrix.Translation).Length() - this.m_shipMergeBlock.CubeGrid.GridSize) * 6.0)).ToString("0.00"), Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
        }
    }
}

