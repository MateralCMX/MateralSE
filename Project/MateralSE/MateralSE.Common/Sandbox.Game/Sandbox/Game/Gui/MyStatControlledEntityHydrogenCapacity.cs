namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;

    public class MyStatControlledEntityHydrogenCapacity : MyStatBase
    {
        private float m_maxValue;
        private MyCubeGrid m_lastConnected;
        private List<IMyGasTank> m_tankBlocks = new List<IMyGasTank>();

        public MyStatControlledEntityHydrogenCapacity()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_hydrogen_capacity");
        }

        private void ConveyorSystemOnBlockAdded(MyCubeBlock myCubeBlock)
        {
            IMyGasTank item = myCubeBlock as IMyGasTank;
            if ((item != null) && item.IsResourceStorage(MyResourceDistributorComponent.HydrogenId))
            {
                this.m_maxValue += item.GasCapacity;
                this.m_tankBlocks.Add(item);
            }
        }

        private void ConveyorSystemOnBlockRemoved(MyCubeBlock myCubeBlock)
        {
            IMyGasTank item = myCubeBlock as IMyGasTank;
            if ((item != null) && item.IsResourceStorage(MyResourceDistributorComponent.HydrogenId))
            {
                this.m_maxValue -= item.GasCapacity;
                this.m_tankBlocks.Remove(item);
            }
        }

        private void Recalculate()
        {
            this.m_maxValue = 0f;
            foreach (IMyGasTank tank in this.m_lastConnected.GridSystems.ConveyorSystem.ConveyorEndpointBlocks)
            {
                if (tank == null)
                {
                    continue;
                }
                if (tank.IsResourceStorage(MyResourceDistributorComponent.HydrogenId))
                {
                    this.m_maxValue += tank.GasCapacity;
                    this.m_tankBlocks.Add(tank);
                }
            }
        }

        public override string ToString()
        {
            float num = (this.m_maxValue > 0f) ? (base.CurrentValue / this.m_maxValue) : 0f;
            return $"{(num * 100f):0}";
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity == null)
            {
                base.CurrentValue = 0f;
                this.m_maxValue = 0f;
            }
            else
            {
                MyResourceDistributorComponent resourceDistributor = null;
                MyCubeGrid objA = null;
                MyCockpit entity = controlledEntity.Entity as MyCockpit;
                if (entity != null)
                {
                    objA = entity.CubeGrid;
                    resourceDistributor = entity.CubeGrid.GridSystems.ResourceDistributor;
                }
                else
                {
                    MyRemoteControl control = controlledEntity as MyRemoteControl;
                    if (control != null)
                    {
                        objA = control.CubeGrid;
                        resourceDistributor = control.CubeGrid.GridSystems.ResourceDistributor;
                    }
                    else
                    {
                        MyLargeTurretBase base2 = controlledEntity as MyLargeTurretBase;
                        if (base2 != null)
                        {
                            objA = base2.CubeGrid;
                            resourceDistributor = base2.CubeGrid.GridSystems.ResourceDistributor;
                        }
                    }
                }
                if (!ReferenceEquals(objA, this.m_lastConnected))
                {
                    if (this.m_lastConnected != null)
                    {
                        this.m_lastConnected.GridSystems.ConveyorSystem.BlockAdded -= new Action<MyCubeBlock>(this.ConveyorSystemOnBlockAdded);
                        this.m_lastConnected.GridSystems.ConveyorSystem.BlockRemoved -= new Action<MyCubeBlock>(this.ConveyorSystemOnBlockRemoved);
                    }
                    this.m_lastConnected = objA;
                    this.m_tankBlocks.Clear();
                    if (objA != null)
                    {
                        objA.GridSystems.ConveyorSystem.BlockAdded += new Action<MyCubeBlock>(this.ConveyorSystemOnBlockAdded);
                        objA.GridSystems.ConveyorSystem.BlockRemoved += new Action<MyCubeBlock>(this.ConveyorSystemOnBlockRemoved);
                        this.Recalculate();
                    }
                }
                if (resourceDistributor == null)
                {
                    base.CurrentValue = 0f;
                    this.m_maxValue = 0f;
                }
                else
                {
                    base.CurrentValue = 0f;
                    foreach (IMyGasTank tank in this.m_tankBlocks)
                    {
                        base.CurrentValue += (float) (tank.FilledRatio * tank.GasCapacity);
                    }
                }
            }
        }

        public override float MaxValue =>
            this.m_maxValue;
    }
}

