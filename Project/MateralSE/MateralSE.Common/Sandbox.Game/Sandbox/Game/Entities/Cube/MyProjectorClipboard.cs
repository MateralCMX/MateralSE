namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.GameSystems.CoordinateSystem;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRageMath;

    public class MyProjectorClipboard : MyGridClipboard
    {
        private MyProjectorBase m_projector;
        private Vector3I m_oldProjectorRotation;
        private Vector3I m_oldProjectorOffset;
        private float m_oldScale;
        private MatrixD m_oldProjectorMatrix;
        private bool m_firstUpdateAfterNewBlueprint;
        private bool m_hasPreviewBBox;
        private bool m_projectionCanBePlaced;

        public MyProjectorClipboard(MyProjectorBase projector, MyPlacementSettings settings) : base(settings, true)
        {
            this.m_projector = projector;
            base.m_calculateVelocity = false;
        }

        public override void Activate(Action callback = null)
        {
            this.ActivateNoAlign(callback);
            this.m_firstUpdateAfterNewBlueprint = true;
        }

        public bool ActuallyTestPlacement()
        {
            this.m_projectionCanBePlaced = base.TestPlacement();
            MyCoordinateSystem.Static.Visible = false;
            return this.m_projectionCanBePlaced;
        }

        public void Clear()
        {
            base.CopiedGrids.Clear();
            base.m_copiedGridOffsets.Clear();
        }

        protected override VRage.Game.Entity.MyEntity GetClipboardBuilder() => 
            null;

        public bool HasGridsLoaded() => 
            ((base.CopiedGrids != null) && (base.CopiedGrids.Count > 0));

        public void ProcessCubeGrid(MyObjectBuilder_CubeGrid gridBuilder)
        {
            gridBuilder.IsStatic = false;
            gridBuilder.DestructibleBlocks = false;
            foreach (MyObjectBuilder_CubeBlock local1 in gridBuilder.CubeBlocks)
            {
                local1.Owner = 0L;
                local1.ShareMode = MyOwnershipShareModeEnum.None;
                local1.EntityId = 0L;
                MyObjectBuilder_FunctionalBlock block = local1 as MyObjectBuilder_FunctionalBlock;
                if (block != null)
                {
                    block.Enabled = false;
                }
            }
        }

        public void ResetGridOrientation()
        {
            base.m_pasteDirForward = Vector3.Forward;
            base.m_pasteDirUp = Vector3.Up;
            base.m_pasteOrientationAngle = 0f;
        }

        protected override void TestBuildingMaterials()
        {
            base.m_characterHasEnoughMaterials = true;
        }

        protected override bool TestPlacement() => 
            Sandbox.Game.Entities.MyEntities.IsInsideWorld(base.m_pastePosition);

        protected override unsafe void UpdateGridTransformations()
        {
            MatrixD worldMatrix = this.m_projector.WorldMatrix;
            if ((this.m_firstUpdateAfterNewBlueprint || ((this.m_oldProjectorRotation != this.m_projector.ProjectionRotation) || ((this.m_oldProjectorOffset != this.m_projector.ProjectionOffset) || !this.m_oldProjectorMatrix.EqualsFast(ref worldMatrix, 0.0001)))) || (this.m_projector.Scale != this.m_oldScale))
            {
                this.m_firstUpdateAfterNewBlueprint = false;
                this.m_oldProjectorRotation = this.m_projector.ProjectionRotation;
                this.m_oldProjectorMatrix = worldMatrix;
                this.m_oldProjectorOffset = this.m_projector.ProjectionOffset;
                this.m_oldScale = this.m_projector.Scale;
                worldMatrix = Matrix.Multiply(Matrix.CreateFromQuaternion(this.m_projector.ProjectionRotationQuaternion), (Matrix) worldMatrix);
                float scale = this.m_projector.Scale;
                for (int i = 0; i < base.PreviewGrids.Count; i++)
                {
                    Vector3D zero = Vector3D.Zero;
                    if (!this.m_projector.AllowScaling)
                    {
                        Vector3D translation;
                        if (base.PreviewGrids[i].CubeBlocks.Count <= 0)
                        {
                            translation = worldMatrix.Translation;
                        }
                        else
                        {
                            MySlimBlock block = base.PreviewGrids[i].CubeBlocks.First<MySlimBlock>();
                            translation = MyCubeGrid.GridIntegerToWorld(base.PreviewGrids[i].GridSize, block.Position, worldMatrix);
                        }
                        zero = (translation - this.m_projector.WorldMatrix.Translation) * scale;
                    }
                    MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                    xdPtr1.Translation -= zero + Vector3D.Transform((Vector3D) this.m_projector.GetProjectionTranslationOffset(), this.m_projector.WorldMatrix.GetOrientation());
                    base.PreviewGrids[i].PositionComp.Scale = new float?(scale);
                    base.PreviewGrids[i].PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, true, false, false);
                }
            }
        }

        protected override void UpdatePastePosition()
        {
            base.m_pastePositionPrevious = base.m_pastePosition;
            base.m_pastePosition = this.m_projector.WorldMatrix.Translation;
        }

        public override bool HasPreviewBBox
        {
            get => 
                this.m_hasPreviewBBox;
            set => 
                (this.m_hasPreviewBBox = value);
        }

        protected override float Transparency =>
            0f;

        protected override bool CanBePlaced =>
            this.m_projectionCanBePlaced;

        public float GridSize
        {
            get
            {
                if ((base.CopiedGrids == null) || (base.CopiedGrids.Count <= 0))
                {
                    return 0f;
                }
                return MyDefinitionManager.Static.GetCubeSize(base.CopiedGrids[0].GridSizeEnum);
            }
        }
    }
}

