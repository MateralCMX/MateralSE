namespace Sandbox.Game.GameSystems
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGroupControlSystem
    {
        private MyShipController m_currentShipController;
        private readonly CachingHashSet<MyShipController> m_groupControllers = new CachingHashSet<MyShipController>();
        private readonly HashSet<MyCubeGrid> m_cubeGrids = new HashSet<MyCubeGrid>();
        private bool m_controlDirty;
        private bool m_firstControlRecalculation;
        private MyEntity m_relativeDampeningEntity;

        public MyGroupControlSystem()
        {
            this.CurrentShipController = null;
            this.m_controlDirty = false;
            this.m_firstControlRecalculation = true;
        }

        public void AddControllerBlock(MyShipController controllerBlock)
        {
            this.m_groupControllers.Add(controllerBlock);
            bool flag = false;
            if ((this.CurrentShipController != null) && !ReferenceEquals(this.CurrentShipController.CubeGrid, controllerBlock.CubeGrid))
            {
                MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(controllerBlock.CubeGrid);
                if (group != null)
                {
                    using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.NodeData == this.CurrentShipController.CubeGrid)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
            }
            if ((!flag && (this.CurrentShipController != null)) && !ReferenceEquals(this.CurrentShipController.CubeGrid, controllerBlock.CubeGrid))
            {
                this.RemoveControllerBlock(this.CurrentShipController);
                this.CurrentShipController = null;
            }
            bool flag2 = (this.CurrentShipController == null) || MyShipController.HasPriorityOver(controllerBlock, this.CurrentShipController);
            if (flag2)
            {
                this.m_controlDirty = true;
                this.m_cubeGrids.ForEach<MyCubeGrid>(x => x.MarkForUpdate());
            }
            if (Sync.IsServer && ((this.CurrentShipController != null) & flag2))
            {
                Sync.Players.ReduceAllControl(this.CurrentShipController);
            }
        }

        public void AddGrid(MyCubeGrid CubeGrid)
        {
            this.m_cubeGrids.Add(CubeGrid);
            if ((Sync.IsServer && (!this.m_controlDirty && (this.CurrentShipController != null))) && (this.CurrentShipController.ControllerInfo.Controller != null))
            {
                Sync.Players.ExtendControl(this.CurrentShipController, CubeGrid);
            }
        }

        public void DebugDraw(float startYCoord)
        {
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, startYCoord), "Controlled group controllers:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            startYCoord += 13f;
            foreach (MyShipController controller in this.m_groupControllers)
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, startYCoord), "  " + controller.ToString(), Color.LightYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                startYCoord += 13f;
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, startYCoord), "Controlled group grids:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            startYCoord += 13f;
            foreach (MyCubeGrid grid in this.m_cubeGrids)
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, startYCoord), "  " + grid.ToString(), Color.LightYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                startYCoord += 13f;
            }
        }

        public MyEntityController GetController() => 
            this.CurrentShipController?.ControllerInfo.Controller;

        public MyShipController GetShipController() => 
            this.CurrentShipController;

        private void relativeDampeningEntityClosed(MyEntity entity)
        {
            this.m_relativeDampeningEntity = null;
        }

        public void RemoveControllerBlock(MyShipController controllerBlock)
        {
            this.m_groupControllers.ApplyAdditions();
            if (this.m_groupControllers.Contains(controllerBlock))
            {
                this.m_groupControllers.Remove(controllerBlock, false);
            }
            if (ReferenceEquals(controllerBlock, this.CurrentShipController))
            {
                this.m_controlDirty = true;
                this.m_cubeGrids.ForEach<MyCubeGrid>(x => x.MarkForUpdate());
            }
            if (Sync.IsServer && ReferenceEquals(controllerBlock, this.CurrentShipController))
            {
                Sync.Players.ReduceAllControl(this.CurrentShipController);
                this.CurrentShipController = null;
            }
        }

        public void RemoveGrid(MyCubeGrid CubeGrid)
        {
            if ((Sync.IsServer && (this.CurrentShipController != null)) && (this.CurrentShipController.ControllerInfo.Controller != null))
            {
                Sync.Players.ReduceControl(this.CurrentShipController, CubeGrid);
            }
            this.m_cubeGrids.Remove(CubeGrid);
        }

        public void UpdateBeforeSimulation()
        {
            this.m_groupControllers.ApplyChanges();
            if (this.m_controlDirty)
            {
                this.UpdateControl();
                this.m_controlDirty = false;
                this.m_firstControlRecalculation = false;
            }
            this.UpdateControls();
        }

        public void UpdateBeforeSimulation100()
        {
            if ((this.RelativeDampeningEntity != null) && (this.CurrentShipController != null))
            {
                MyEntityThrustComponent.UpdateRelativeDampeningEntity(this.CurrentShipController, this.RelativeDampeningEntity);
            }
        }

        private void UpdateControl()
        {
            MyShipController second = null;
            foreach (MyShipController controller2 in this.m_groupControllers)
            {
                if (second == null)
                {
                    second = controller2;
                    continue;
                }
                if (MyShipController.HasPriorityOver(controller2, second))
                {
                    second = controller2;
                }
            }
            this.CurrentShipController = second;
            if (Sync.IsServer && (this.CurrentShipController != null))
            {
                MyEntityController controller = this.CurrentShipController.ControllerInfo.Controller;
                foreach (MyCubeGrid grid in this.m_cubeGrids)
                {
                    if (this.CurrentShipController.ControllerInfo.Controller != null)
                    {
                        Sync.Players.TryExtendControl(this.CurrentShipController, grid);
                    }
                }
                if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
                {
                    this.CurrentShipController.GridWheels.InitControl(this.CurrentShipController.Entity);
                }
            }
        }

        public void UpdateControls()
        {
            using (HashSet<MyShipController>.Enumerator enumerator = this.m_groupControllers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateControls();
                }
            }
        }

        public MyEntity RelativeDampeningEntity
        {
            get => 
                this.m_relativeDampeningEntity;
            set
            {
                if (!ReferenceEquals(this.m_relativeDampeningEntity, value))
                {
                    if (this.m_relativeDampeningEntity != null)
                    {
                        this.m_relativeDampeningEntity.OnClose -= new Action<MyEntity>(this.relativeDampeningEntityClosed);
                    }
                    this.m_relativeDampeningEntity = value;
                    if (this.m_relativeDampeningEntity != null)
                    {
                        this.m_relativeDampeningEntity.OnClose += new Action<MyEntity>(this.relativeDampeningEntityClosed);
                    }
                }
            }
        }

        private MyShipController CurrentShipController
        {
            get => 
                this.m_currentShipController;
            set
            {
                if (!ReferenceEquals(value, this.m_currentShipController))
                {
                    if (value != null)
                    {
                        this.m_currentShipController = value;
                        MyGridPhysicalHierarchy.Static.UpdateRoot(this.m_currentShipController.CubeGrid);
                    }
                    else
                    {
                        MyShipController currentShipController = this.m_currentShipController;
                        this.m_currentShipController = value;
                        MyGridPhysicalHierarchy.Static.UpdateRoot(currentShipController.CubeGrid);
                    }
                }
            }
        }

        public bool NeedsPerFrameUpdate =>
            (this.m_controlDirty || ((this.CurrentShipController != null) && (this.CurrentShipController.ControllerInfo.Controller != null)));

        public bool IsLocallyControlled
        {
            get
            {
                MyEntityController controller = this.GetController();
                return ((controller != null) ? controller.Player.IsLocalPlayer : false);
            }
        }

        public bool IsControlled =>
            (this.GetController() != null);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGroupControlSystem.<>c <>9 = new MyGroupControlSystem.<>c();
            public static Action<MyCubeGrid> <>9__19_0;
            public static Action<MyCubeGrid> <>9__20_0;

            internal void <AddControllerBlock>b__20_0(MyCubeGrid x)
            {
                x.MarkForUpdate();
            }

            internal void <RemoveControllerBlock>b__19_0(MyCubeGrid x)
            {
                x.MarkForUpdate();
            }
        }
    }
}

