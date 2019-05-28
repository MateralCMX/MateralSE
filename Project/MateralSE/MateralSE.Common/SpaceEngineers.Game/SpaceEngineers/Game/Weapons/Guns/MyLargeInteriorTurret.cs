namespace SpaceEngineers.Game.Weapons.Guns
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.EntityComponents.Renders;
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
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_InteriorTurret)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyLargeInteriorTurret), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyLargeInteriorTurret) })]
    public class MyLargeInteriorTurret : MyLargeTurretBase, SpaceEngineers.Game.ModAPI.IMyLargeInteriorTurret, Sandbox.ModAPI.IMyLargeTurretBase, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun, Sandbox.ModAPI.Ingame.IMyLargeTurretBase, IMyCameraController, SpaceEngineers.Game.ModAPI.Ingame.IMyLargeInteriorTurret
    {
        public MyLargeInteriorTurret()
        {
            base.Render = new MyRenderComponentLargeTurret();
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            base.m_randomStandbyChangeConst_ms = MyUtils.GetRandomInt(0xdac, 0x1194);
            if (base.m_gunBase.HasAmmoMagazines)
            {
                base.m_shootingCueEnum = base.m_gunBase.ShootSound;
            }
            base.m_rotatingCueEnum.Init("WepTurretInteriorRotate", true);
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
                base.m_base1 = base.Subparts["InteriorTurretBase1"];
                base.m_base2 = base.m_base1.Subparts["InteriorTurretBase2"];
                base.m_barrel = new MyLargeInteriorBarrel();
                ((MyLargeInteriorBarrel) base.m_barrel).Init(base.m_base2, this);
                base.GetCameraDummy();
            }
            base.ResetRotation();
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (action == MyShootActionEnum.PrimaryAction)
            {
                base.m_gunBase.Shoot(Vector3.Zero, null);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (!MyFakes.ENABLE_INTERIOR_TURRETS || !MySession.Static.WeaponsEnabled)
            {
                this.RotateModels();
            }
            else
            {
                base.UpdateAfterSimulation();
                base.DrawLasers();
            }
        }

        public int Burst { get; private set; }

        protected override float ForwardCameraOffset =>
            0.2f;

        protected override float UpCameraOffset =>
            0.45f;
    }
}

