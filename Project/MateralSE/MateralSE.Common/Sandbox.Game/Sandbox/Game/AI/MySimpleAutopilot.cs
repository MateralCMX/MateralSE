namespace Sandbox.Game.AI
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Groups;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyAutopilotType(typeof(MyObjectBuilder_SimpleAutopilot))]
    internal class MySimpleAutopilot : MyAutopilotBase
    {
        private static readonly int SHIP_LIFESPAN_MILLISECONDS = 0x1b7740;
        private int m_spawnTime;
        private long[] m_gridIds;
        private Vector3 m_direction;
        private Vector3D m_destination;
        private int m_subgridLookupCounter;

        public MySimpleAutopilot()
        {
            this.m_subgridLookupCounter = -1;
        }

        public MySimpleAutopilot(Vector3D destination, Vector3 direction, long[] gridsIds)
        {
            this.m_subgridLookupCounter = -1;
            this.m_gridIds = gridsIds;
            this.m_direction = direction;
            this.m_destination = destination;
            this.m_spawnTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_NEUTRAL_SHIPS && (base.ShipController != null))
            {
                Vector3D position = MySector.MainCamera.Position;
                Vector3D vectord2 = Vector3D.Normalize(this.m_destination - position);
                Vector3D pointTo = Vector3D.Normalize((Vector3D.Normalize(base.ShipController.PositionComp.GetPosition() - position) + vectord2) * 0.5) + position;
                Vector3D vectord4 = Vector3D.Normalize(base.ShipController.WorldMatrix.Translation - position) + position;
                Vector3D pointFrom = Vector3D.Normalize(base.ShipController.PositionComp.GetPosition() - position) + position;
                MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, Color.Red, Color.Red, false, false);
                MyRenderProxy.DebugDrawLine3D(pointTo, vectord2 + position, Color.Red, Color.Red, false, false);
                MyRenderProxy.DebugDrawSphere(vectord4, 0.01f, Color.Orange.ToVector3(), 1f, false, false, true, false);
                MyRenderProxy.DebugDrawSphere(vectord4 + (this.m_direction * 0.015f), 0.005f, Color.Yellow.ToVector3(), 1f, false, false, true, false);
                MyRenderProxy.DebugDrawText3D(pointFrom, "Remaining time: " + ((SHIP_LIFESPAN_MILLISECONDS - MySandboxGame.TotalGamePlayTimeInMilliseconds) + this.m_spawnTime), Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        private void ForEachGrid(Action<MyCubeGrid> action)
        {
            if ((this.m_gridIds != null) && (this.m_gridIds.Length != 0))
            {
                long[] gridIds = this.m_gridIds;
                for (int i = 0; i < gridIds.Length; i++)
                {
                    MyCubeGrid entityById = (MyCubeGrid) Sandbox.Game.Entities.MyEntities.GetEntityById(gridIds[i], false);
                    if (entityById != null)
                    {
                        action(entityById);
                    }
                }
            }
        }

        public override MyObjectBuilder_AutopilotBase GetObjectBuilder()
        {
            MyObjectBuilder_SimpleAutopilot local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SimpleAutopilot>();
            local1.Destination = this.m_destination;
            local1.Direction = this.m_direction;
            local1.SpawnTime = new int?(this.m_spawnTime);
            local1.GridIds = this.m_gridIds;
            return local1;
        }

        public override void Init(MyObjectBuilder_AutopilotBase objectBuilder)
        {
            MyObjectBuilder_SimpleAutopilot autopilot = (MyObjectBuilder_SimpleAutopilot) objectBuilder;
            this.m_gridIds = autopilot.GridIds;
            this.m_direction = autopilot.Direction;
            this.m_destination = autopilot.Destination;
            int? spawnTime = autopilot.SpawnTime;
            this.m_spawnTime = (spawnTime != null) ? spawnTime.GetValueOrDefault() : MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.m_gridIds == null)
            {
                this.m_subgridLookupCounter = 100;
            }
        }

        private bool IsPlayerNearby()
        {
            BoundingBoxD playerBox = BoundingBoxD.CreateInvalid();
            Sandbox.Game.Entities.MyEntities.GetInflatedPlayerBoundingBox(ref playerBox, 2000f);
            return (playerBox.Contains(base.ShipController.PositionComp.GetPosition()) == ContainmentType.Contains);
        }

        public override void OnAttachedToShipController(MyCockpit newShipController)
        {
            base.OnAttachedToShipController(newShipController);
            if (this.m_subgridLookupCounter <= 0)
            {
                this.RegisterGridCallbacks();
            }
        }

        private void OnBlockAddedRemovedOrChanged(MySlimBlock obj)
        {
            this.PersistShip();
        }

        private void OnGridChanged(MyCubeGrid grid)
        {
            this.PersistShip();
        }

        public override void OnRemovedFromCockpit()
        {
            if (Sync.IsServer)
            {
                this.ForEachGrid(delegate (MyCubeGrid grid) {
                    grid.OnGridChanged -= new Action<MyCubeGrid>(this.OnGridChanged);
                    grid.OnBlockAdded -= new Action<MySlimBlock>(this.OnBlockAddedRemovedOrChanged);
                    grid.OnBlockRemoved -= new Action<MySlimBlock>(this.OnBlockAddedRemovedOrChanged);
                    grid.OnBlockIntegrityChanged -= new Action<MySlimBlock>(this.OnBlockAddedRemovedOrChanged);
                });
            }
            base.OnRemovedFromCockpit();
        }

        private void PersistShip()
        {
            base.ShipController.RemoveAutopilot(true);
        }

        private void RegisterGridCallbacks()
        {
            if (Sync.IsServer)
            {
                this.ForEachGrid(delegate (MyCubeGrid grid) {
                    grid.OnGridChanged += new Action<MyCubeGrid>(this.OnGridChanged);
                    grid.OnBlockAdded += new Action<MySlimBlock>(this.OnBlockAddedRemovedOrChanged);
                    grid.OnBlockRemoved += new Action<MySlimBlock>(this.OnBlockAddedRemovedOrChanged);
                    grid.OnBlockIntegrityChanged += new Action<MySlimBlock>(this.OnBlockAddedRemovedOrChanged);
                });
            }
        }

        public override void Update()
        {
            if (Sync.IsServer)
            {
                if (this.m_subgridLookupCounter > 0)
                {
                    int num = this.m_subgridLookupCounter - 1;
                    this.m_subgridLookupCounter = num;
                    if (num == 0)
                    {
                        MyCubeGrid cubeGrid = base.ShipController.CubeGrid;
                        MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(cubeGrid);
                        this.m_gridIds = (from x in group.Nodes select x.NodeData.EntityId).ToArray<long>();
                        this.RegisterGridCallbacks();
                    }
                }
                MyCockpit shipController = base.ShipController;
                if (shipController != null)
                {
                    int num1;
                    if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_spawnTime) <= SHIP_LIFESPAN_MILLISECONDS)
                    {
                        num1 = (int) ((shipController.PositionComp.GetPosition() - this.m_destination).Dot(this.m_direction) > 0.0);
                    }
                    else
                    {
                        num1 = 1;
                    }
                    if ((num1 != 0) && !this.IsPlayerNearby())
                    {
                        base.ShipController.RemoveAutopilot(true);
                        this.ForEachGrid(grid => grid.Close());
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySimpleAutopilot.<>c <>9 = new MySimpleAutopilot.<>c();
            public static Func<MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node, long> <>9__16_1;
            public static Action<MyCubeGrid> <>9__16_0;

            internal void <Update>b__16_0(MyCubeGrid grid)
            {
                grid.Close();
            }

            internal long <Update>b__16_1(MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node x) => 
                x.NodeData.EntityId;
        }
    }
}

