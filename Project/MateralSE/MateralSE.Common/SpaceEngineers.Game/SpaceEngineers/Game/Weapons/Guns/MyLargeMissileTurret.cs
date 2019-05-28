namespace SpaceEngineers.Game.Weapons.Guns
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using SpaceEngineers.Game.Weapons.Guns.Barrels;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.Network;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_LargeMissileTurret))]
    public class MyLargeMissileTurret : MyLargeConveyorTurretBase, SpaceEngineers.Game.ModAPI.IMyLargeMissileTurret, SpaceEngineers.Game.ModAPI.IMyLargeConveyorTurretBase, Sandbox.ModAPI.IMyLargeTurretBase, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun, Sandbox.ModAPI.Ingame.IMyLargeTurretBase, IMyCameraController, SpaceEngineers.Game.ModAPI.Ingame.IMyLargeConveyorTurretBase, SpaceEngineers.Game.ModAPI.Ingame.IMyLargeMissileTurret, IMyMissileGunObject, IMyGunObject<MyGunBase>
    {
        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            base.m_randomStandbyChangeConst_ms = 0xfa0;
            base.m_rotationSpeed = 0.001570796f;
            base.m_elevationSpeed = 0.001570796f;
            if (base.BlockDefinition != null)
            {
                base.m_rotationSpeed = base.BlockDefinition.RotationSpeed;
                base.m_elevationSpeed = base.BlockDefinition.ElevationSpeed;
            }
            if (base.m_gunBase.HasAmmoMagazines)
            {
                base.m_shootingCueEnum = base.m_gunBase.ShootSound;
            }
            base.m_rotatingCueEnum.Init("WepTurretGatlingRotate", true);
        }

        public void MissileShootEffect()
        {
            if (base.m_barrel != null)
            {
                base.m_barrel.ShootEffect();
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            if (!base.IsBuilt)
            {
                base.m_base1 = null;
                base.m_base2 = null;
                base.m_barrel = null;
            }
            else
            {
                base.m_base1 = base.Subparts["MissileTurretBase1"];
                base.m_base2 = base.m_base1.Subparts["MissileTurretBarrels"];
                base.m_barrel = new MyLargeMissileBarrel();
                ((MyLargeMissileBarrel) base.m_barrel).Init(base.m_base2, this);
                base.GetCameraDummy();
            }
            base.ResetRotation();
        }

        [Event(null, 0x6b), Reliable, Broadcast]
        private void OnRemoveMissile(long entityId)
        {
            MyMissiles.Remove(entityId);
        }

        [Event(null, 0x5e), Reliable, Server, Broadcast]
        private void OnShootMissile(MyObjectBuilder_Missile builder)
        {
            MyMissiles.Add(builder);
        }

        public void RemoveMissile(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyLargeMissileTurret, long>(this, x => new Action<long>(x.OnRemoveMissile), entityId, targetEndpoint);
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if ((action == MyShootActionEnum.PrimaryAction) && (base.m_barrel != null))
            {
                base.m_barrel.StartShooting();
            }
        }

        public override void ShootFromTerminal(Vector3 direction)
        {
            if (base.m_barrel != null)
            {
                base.ShootFromTerminal(direction);
                base.m_isControlled = true;
                base.m_barrel.StartShooting();
                base.m_isControlled = false;
            }
        }

        public void ShootMissile(MyObjectBuilder_Missile builder)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyLargeMissileTurret, MyObjectBuilder_Missile>(this, x => new Action<MyObjectBuilder_Missile>(x.OnShootMissile), builder, targetEndpoint);
        }

        public override void UpdateAfterSimulation()
        {
            if (!MyFakes.ENABLE_MISSILE_TURRETS || !MySession.Static.WeaponsEnabled)
            {
                this.RotateModels();
            }
            else
            {
                base.UpdateAfterSimulation();
                base.DrawLasers();
            }
        }

        public override IMyMissileGunObject Launcher =>
            this;

        protected override float ForwardCameraOffset =>
            0.5f;

        protected override float UpCameraOffset =>
            1f;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyLargeMissileTurret.<>c <>9 = new MyLargeMissileTurret.<>c();
            public static Func<MyLargeMissileTurret, Action<MyObjectBuilder_Missile>> <>9__10_0;
            public static Func<MyLargeMissileTurret, Action<long>> <>9__12_0;

            internal Action<long> <RemoveMissile>b__12_0(MyLargeMissileTurret x) => 
                new Action<long>(x.OnRemoveMissile);

            internal Action<MyObjectBuilder_Missile> <ShootMissile>b__10_0(MyLargeMissileTurret x) => 
                new Action<MyObjectBuilder_Missile>(x.OnShootMissile);
        }
    }
}

