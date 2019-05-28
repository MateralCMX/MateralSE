namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_SmallMissileLauncherReload)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMySmallMissileLauncherReload), typeof(Sandbox.ModAPI.Ingame.IMySmallMissileLauncherReload) })]
    public class MySmallMissileLauncherReload : MySmallMissileLauncher, Sandbox.ModAPI.IMySmallMissileLauncherReload, Sandbox.ModAPI.IMySmallMissileLauncher, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun, Sandbox.ModAPI.Ingame.IMySmallMissileLauncher, Sandbox.ModAPI.Ingame.IMySmallMissileLauncherReload
    {
        private const int COOLDOWN_TIME_MILISECONDS = 0x1388;
        private int m_numRocketsShot;
        private static readonly MyHudNotification MISSILE_RELOAD_NOTIFICATION = new MyHudNotification(MySpaceTexts.MissileLauncherReloadingNotification, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);

        public MySmallMissileLauncherReload()
        {
            this.CreateTerminalControls();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySmallMissileLauncherReload>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> switch1 = new MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> switch2 = new MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> local7 = switch2;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> local8 = switch2;
                local8.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> local5 = local8;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> local6 = local8;
                local6.Visible = x => true;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncherReload> onOff = local6;
                onOff.EnableToggleAction<MySmallMissileLauncherReload>();
                MyTerminalControlFactory.AddControl<MySmallMissileLauncherReload>(onOff);
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.Init(builder, cubeGrid);
            MyObjectBuilder_SmallMissileLauncherReload reload = (MyObjectBuilder_SmallMissileLauncherReload) builder;
            base.m_useConveyorSystem.SetLocalValue(reload.UseConveyorSystem);
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if ((this.BurstFireRate != this.m_numRocketsShot) || (MySandboxGame.TotalGamePlayTimeInMilliseconds >= base.m_nextShootTime))
            {
                if (this.BurstFireRate == this.m_numRocketsShot)
                {
                    this.m_numRocketsShot = 0;
                }
                this.m_numRocketsShot++;
                base.Shoot(action, direction, overrideWeaponPos, gunAction);
            }
        }

        public int BurstFireRate =>
            base.GunBase.ShotsInBurst;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySmallMissileLauncherReload.<>c <>9 = new MySmallMissileLauncherReload.<>c();
            public static MyTerminalValueControl<MySmallMissileLauncherReload, bool>.GetterDelegate <>9__6_0;
            public static MyTerminalValueControl<MySmallMissileLauncherReload, bool>.SetterDelegate <>9__6_1;
            public static Func<MySmallMissileLauncherReload, bool> <>9__6_2;

            internal bool <CreateTerminalControls>b__6_0(MySmallMissileLauncherReload x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__6_1(MySmallMissileLauncherReload x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal bool <CreateTerminalControls>b__6_2(MySmallMissileLauncherReload x) => 
                true;
        }
    }
}

