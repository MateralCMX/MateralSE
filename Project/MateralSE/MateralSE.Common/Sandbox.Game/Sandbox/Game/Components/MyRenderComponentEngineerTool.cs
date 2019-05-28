namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentEngineerTool : MyRenderComponent
    {
        private MyEngineerToolBase m_tool;

        public override void Draw()
        {
            base.Draw();
            if (this.m_tool.CanBeDrawn())
            {
                this.DrawHighlight();
            }
        }

        public void DrawHighlight()
        {
            if (((this.m_tool.GetTargetGrid() != null) && this.m_tool.HasHitBlock) && MySandboxGame.Config.ShowCrosshair)
            {
                MySlimBlock cubeBlock = this.m_tool.GetTargetGrid().GetCubeBlock(this.m_tool.TargetCube);
                if (cubeBlock != null)
                {
                    Matrix matrix;
                    cubeBlock.Orientation.GetMatrix(out matrix);
                    MatrixD worldMatrix = this.m_tool.GetTargetGrid().Physics.GetWorldMatrix();
                    MatrixD xd = ((matrix * Matrix.CreateTranslation((Vector3) cubeBlock.Position)) * Matrix.CreateScale(this.m_tool.GetTargetGrid().GridSize)) * worldMatrix;
                    Vector3 vector = new Vector3(0.5f, 0.5f, 0.5f);
                    TimeSpan elapsedPlayTime = MySession.Static.ElapsedPlayTime;
                    Vector3 vector2 = new Vector3(0.05f);
                    BoundingBoxD localbox = new BoundingBoxD((Vector3D) ((-cubeBlock.BlockDefinition.Center - vector) - vector2), ((cubeBlock.BlockDefinition.Size - cubeBlock.BlockDefinition.Center) - vector) + vector2);
                    Color highlightColor = this.m_tool.HighlightColor;
                    MyStringId? faceMaterial = null;
                    MySimpleObjectDraw.DrawTransparentBox(ref xd, ref localbox, ref highlightColor, MySimpleObjectRasterizer.Wireframe, 1, (this.m_tool.GetTargetGrid().GridSizeEnum == MyCubeSize.Large) ? 0.06f : 0.03f, faceMaterial, new MyStringId?(this.m_tool.HighlightMaterial), false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_tool = base.Container.Entity as MyEngineerToolBase;
        }
    }
}

