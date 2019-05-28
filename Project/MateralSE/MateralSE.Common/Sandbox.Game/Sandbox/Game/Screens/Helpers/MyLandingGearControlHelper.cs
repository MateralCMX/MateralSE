namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;

    public class MyLandingGearControlHelper : MyControllableEntityControlHelper
    {
        public MyLandingGearControlHelper() : this(MyControlsSpace.LANDING_GEAR, x => x.SwitchLandingGears(), x => x.EnabledLeadingGears, MySpaceTexts.ControlMenuItemLabel_LandingGear, MySupportKeysEnum.NONE)
        {
        }

        public void SetEntity(IMyControllableEntity entity)
        {
            base.m_entity = entity as MyShipController;
        }

        private MyShipController ShipController =>
            (base.m_entity as MyShipController);

        public override bool Enabled =>
            (this.ShipController.CubeGrid.GridSystems.LandingSystem.Locked != MyMultipleEnabledEnum.NoObjects);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyLandingGearControlHelper.<>c <>9 = new MyLandingGearControlHelper.<>c();
            public static Action<IMyControllableEntity> <>9__4_0;
            public static Func<IMyControllableEntity, bool> <>9__4_1;

            internal void <.ctor>b__4_0(IMyControllableEntity x)
            {
                x.SwitchLandingGears();
            }

            internal bool <.ctor>b__4_1(IMyControllableEntity x) => 
                x.EnabledLeadingGears;
        }
    }
}

