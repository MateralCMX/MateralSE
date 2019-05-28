namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_ConveyorConnector))]
    public class MyConveyorConnector : MyCubeBlock, IMyConveyorSegmentBlock, Sandbox.ModAPI.IMyConveyorTube, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyConveyorTube
    {
        private readonly MyConveyorSegment m_segment = new MyConveyorSegment();
        private bool m_working;
        private bool m_emissivitySet;
        private MyResourceStateEnum m_state;

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.Init(builder, cubeGrid);
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.m_emissivitySet = false;
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorSegment(this.m_segment));
        }

        public void InitializeConveyorSegment()
        {
            MyConveyorLine.BlockLinePositionInformation[] blockLinePositions = MyConveyorLine.GetBlockLinePositions(this);
            if (blockLinePositions.Length != 0)
            {
                ConveyorLinePosition connectingPosition = this.PositionToGridCoords(blockLinePositions[0].Position).GetConnectingPosition();
                this.m_segment.Init(this, connectingPosition, this.PositionToGridCoords(blockLinePositions[1].Position).GetConnectingPosition(), blockLinePositions[0].LineType, MyObjectBuilder_ConveyorLine.LineConductivity.FULL);
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            this.m_emissivitySet = false;
        }

        private ConveyorLinePosition PositionToGridCoords(ConveyorLinePosition position)
        {
            ConveyorLinePosition position2 = new ConveyorLinePosition();
            Matrix result = new Matrix();
            base.Orientation.GetMatrix(out result);
            position2.LocalGridPosition = (Vector3I) (Vector3I.Round(Vector3.Transform(new Vector3(position.LocalGridPosition), result)) + base.Position);
            position2.Direction = base.Orientation.TransformDirection(position.Direction);
            return position2;
        }

        public override bool SetEmissiveStateDisabled()
        {
            if (!base.IsFunctional || this.m_working)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);
            }
            return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Warning, base.Render.RenderObjectIDs[0], null);
        }

        public override bool SetEmissiveStateWorking()
        {
            if (!base.IsWorking)
            {
                return false;
            }
            if (this.m_state == MyResourceStateEnum.OverloadBlackout)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);
            }
            this.m_emissivitySet = true;
            return (!this.m_working ? base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Warning, base.Render.RenderObjectIDs[0], null) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null));
        }

        public override void UpdateBeforeSimulation100()
        {
            if (this.m_segment.ConveyorLine != null)
            {
                MyResourceStateEnum enum2 = this.m_segment.ConveyorLine.UpdateIsWorking();
                if ((!this.m_emissivitySet || (this.m_working != this.m_segment.ConveyorLine.IsWorking)) || (this.m_state != enum2))
                {
                    this.m_working = this.m_segment.ConveyorLine.IsWorking;
                    this.m_state = enum2;
                    this.SetEmissiveStateWorking();
                }
            }
        }

        public override float MaxGlassDistSq =>
            22500f;

        public MyConveyorSegment ConveyorSegment =>
            this.m_segment;
    }
}

