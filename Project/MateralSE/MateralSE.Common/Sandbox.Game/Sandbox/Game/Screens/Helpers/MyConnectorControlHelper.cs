namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;

    public class MyConnectorControlHelper : MyControllableEntityControlHelper
    {
        public MyConnectorControlHelper() : this(MyControlsSpace.LANDING_GEAR, x => x.SwitchLandingGears(), x => GetConnectorStatus(x), MySpaceTexts.ControlMenuItemLabel_Connectors, MySupportKeysEnum.NONE)
        {
        }

        private static bool GetConnectorStatus(IMyControllableEntity shipController) => 
            (shipController as MyShipController).CubeGrid.GridSystems.ConveyorSystem.Connected;

        public void SetEntity(IMyControllableEntity entity)
        {
            base.m_entity = entity as MyShipController;
        }

        private MyShipController ShipController =>
            (base.m_entity as MyShipController);

        public override bool Enabled =>
            this.ShipController.CubeGrid.GridSystems.ConveyorSystem.IsInteractionPossible;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyConnectorControlHelper.<>c <>9 = new MyConnectorControlHelper.<>c();
            public static Action<IMyControllableEntity> <>9__4_0;
            public static Func<IMyControllableEntity, bool> <>9__4_1;

            internal void <.ctor>b__4_0(IMyControllableEntity x)
            {
                x.SwitchLandingGears();
            }

            internal bool <.ctor>b__4_1(IMyControllableEntity x) => 
                MyConnectorControlHelper.GetConnectorStatus(x);
        }
    }
}

