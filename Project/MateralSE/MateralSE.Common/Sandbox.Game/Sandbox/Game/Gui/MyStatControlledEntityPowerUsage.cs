namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyStatControlledEntityPowerUsage : MyStatBase
    {
        private float m_maxValue;

        public MyStatControlledEntityPowerUsage()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_power_usage");
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
                MyCockpit entity = controlledEntity.Entity as MyCockpit;
                if (entity != null)
                {
                    resourceDistributor = entity.CubeGrid.GridSystems.ResourceDistributor;
                }
                else
                {
                    MyRemoteControl control = controlledEntity as MyRemoteControl;
                    if (control != null)
                    {
                        resourceDistributor = control.CubeGrid.GridSystems.ResourceDistributor;
                    }
                    else
                    {
                        MyLargeTurretBase base2 = controlledEntity as MyLargeTurretBase;
                        if (base2 != null)
                        {
                            resourceDistributor = base2.CubeGrid.GridSystems.ResourceDistributor;
                        }
                    }
                }
                if (resourceDistributor != null)
                {
                    this.m_maxValue = resourceDistributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId);
                    base.CurrentValue = MyMath.Clamp(resourceDistributor.TotalRequiredInputByType(MyResourceDistributorComponent.ElectricityId), 0f, this.m_maxValue);
                }
                else
                {
                    base.CurrentValue = 0f;
                    this.m_maxValue = 0f;
                }
            }
        }

        public override float MaxValue =>
            this.m_maxValue;
    }
}

