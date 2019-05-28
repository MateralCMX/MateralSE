namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Gui;

    public class MyHudWorldBorderChecker
    {
        private static readonly float WARNING_DISTANCE = 600f;
        private MyHudNotification m_notification = new MyHudNotification(MyCommonTexts.NotificationLeavingWorld, MyHudNotificationBase.INFINITE, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        private MyHudNotification m_notificationCreative = new MyHudNotification(MyCommonTexts.NotificationLeavingWorld_Creative, MyHudNotificationBase.INFINITE, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        internal static MyHudEntityParams HudEntityParams = new MyHudEntityParams(MyTexts.Get(MyCommonTexts.HudMarker_ReturnToWorld), 0L, MyHudIndicatorFlagsEnum.SHOW_BORDER_INDICATORS | MyHudIndicatorFlagsEnum.SHOW_TEXT);

        public void Update()
        {
            if (MySession.Static.ControlledEntity != null)
            {
                double num1;
                float num = MyEntities.WorldHalfExtent();
                if (MySession.Static.ControlledEntity.Entity == null)
                {
                    num1 = 0.0;
                }
                else
                {
                    num1 = MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition().AbsMax();
                }
                double num2 = num1;
                if (((num != 0f) && (MySession.Static.ControlledEntity.Entity != null)) && ((num - num2) < WARNING_DISTANCE))
                {
                    double num3 = ((num - num2) > 0.0) ? (num - num2) : 0.0;
                    if (MySession.Static.SurvivalMode)
                    {
                        object[] arguments = new object[] { num3 };
                        this.m_notification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(this.m_notification);
                    }
                    else
                    {
                        object[] arguments = new object[] { num3 };
                        this.m_notificationCreative.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(this.m_notificationCreative);
                    }
                    this.WorldCenterHintVisible = true;
                }
                else
                {
                    MyHud.Notifications.Remove(this.m_notification);
                    MyHud.Notifications.Remove(this.m_notificationCreative);
                    this.WorldCenterHintVisible = false;
                }
            }
        }

        public bool WorldCenterHintVisible { get; private set; }
    }
}

