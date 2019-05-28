namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Weapons;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRageMath;

    internal static class MyControllableEntityExtensions
    {
        public static void GetLinearVelocity(this IMyControllableEntity controlledEntity, ref Vector3 velocityVector, bool useRemoteControlVelocity = true)
        {
            if (controlledEntity.Entity.Physics != null)
            {
                velocityVector = (controlledEntity.Entity.Physics != null) ? controlledEntity.Entity.Physics.LinearVelocity : Vector3.Zero;
            }
            else
            {
                MyCockpit cockpit = controlledEntity as MyCockpit;
                if (cockpit != null)
                {
                    velocityVector = (cockpit.CubeGrid.Physics != null) ? cockpit.CubeGrid.Physics.LinearVelocity : Vector3.Zero;
                }
                else
                {
                    MyRemoteControl control = controlledEntity as MyRemoteControl;
                    if ((control != null) & useRemoteControlVelocity)
                    {
                        velocityVector = (control.CubeGrid.Physics != null) ? control.CubeGrid.Physics.LinearVelocity : Vector3.Zero;
                    }
                    else
                    {
                        MyLargeTurretBase base2 = controlledEntity as MyLargeTurretBase;
                        if (base2 != null)
                        {
                            velocityVector = (base2.CubeGrid.Physics != null) ? base2.CubeGrid.Physics.LinearVelocity : Vector3.Zero;
                        }
                    }
                }
            }
        }

        public static MyPhysicsComponentBase Physics(this IMyControllableEntity entity)
        {
            if (entity.Entity == null)
            {
                return null;
            }
            if (entity.Entity.Physics != null)
            {
                return entity.Entity.Physics;
            }
            MyCockpit cockpit = entity.Entity as MyCockpit;
            if (((cockpit == null) || (cockpit.CubeGrid == null)) || (cockpit.CubeGrid.Physics == null))
            {
                return null;
            }
            return cockpit.CubeGrid.Physics;
        }

        public static void SwitchControl(this IMyControllableEntity entity, IMyControllableEntity newControlledEntity)
        {
            if (entity.ControllerInfo.Controller != null)
            {
                entity.ControllerInfo.Controller.TakeControl(newControlledEntity);
            }
        }
    }
}

