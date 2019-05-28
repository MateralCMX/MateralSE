namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Generics;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyHudNotifications
    {
        public const int MAX_PRIORITY = 5;
        private Predicate<MyHudNotificationBase> m_disappearedPredicate;
        private Dictionary<int, List<MyHudNotificationBase>> m_notificationsByPriority;
        private List<StringBuilder> m_texts;
        private readonly List<NotificationDrawData> m_drawData;
        private MyObjectsPool<StringBuilder> m_textsPool;
        private object m_lockObject;
        private MyHudNotificationBase[] m_singletons;
        [CompilerGenerated]
        private Action<MyNotificationSingletons> OnNotificationAdded;
        public Vector2 Position;

        public event Action<MyNotificationSingletons> OnNotificationAdded
        {
            [CompilerGenerated] add
            {
                Action<MyNotificationSingletons> onNotificationAdded = this.OnNotificationAdded;
                while (true)
                {
                    Action<MyNotificationSingletons> a = onNotificationAdded;
                    Action<MyNotificationSingletons> action3 = (Action<MyNotificationSingletons>) Delegate.Combine(a, value);
                    onNotificationAdded = Interlocked.CompareExchange<Action<MyNotificationSingletons>>(ref this.OnNotificationAdded, action3, a);
                    if (ReferenceEquals(onNotificationAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyNotificationSingletons> onNotificationAdded = this.OnNotificationAdded;
                while (true)
                {
                    Action<MyNotificationSingletons> source = onNotificationAdded;
                    Action<MyNotificationSingletons> action3 = (Action<MyNotificationSingletons>) Delegate.Remove(source, value);
                    onNotificationAdded = Interlocked.CompareExchange<Action<MyNotificationSingletons>>(ref this.OnNotificationAdded, action3, source);
                    if (ReferenceEquals(onNotificationAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyHudNotifications()
        {
            int num1;
            this.m_drawData = new List<NotificationDrawData>(9);
            this.m_lockObject = new object();
            this.Position = MyNotificationConstants.DEFAULT_NOTIFICATION_MESSAGE_NORMALIZED_POSITION;
            this.m_disappearedPredicate = x => !x.Alive;
            this.m_notificationsByPriority = new Dictionary<int, List<MyHudNotificationBase>>();
            this.m_texts = new List<StringBuilder>(9);
            this.m_textsPool = new MyObjectsPool<StringBuilder>(20, null);
            this.m_singletons = new MyHudNotificationBase[System.Enum.GetValues(typeof(MyNotificationSingletons)).Length];
            this.Register(MyNotificationSingletons.GameOverload, new MyHudNotification(MyCommonTexts.NotificationMemoryOverload, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.SuitEnergyLow, new MyHudNotification(MySpaceTexts.NotificationSuitEnergyLow, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.SuitEnergyCritical, new MyHudNotification(MySpaceTexts.NotificationSuitEnergyCritical, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.IncompleteGrid, new MyHudNotification(MyCommonTexts.NotificationIncompleteGrid, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.DisabledWeaponsAndTools, new MyHudNotification(MyCommonTexts.NotificationToolDisabled, 0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.WeaponDisabledInWorldSettings, new MyHudNotification(MyCommonTexts.NotificationWeaponDisabledInSettings, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.MultiplayerDisabled, new MyHudNotification(MyCommonTexts.NotificationMultiplayerDisabled, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 5, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.MissingComponent, new MyHudMissingComponentNotification(MyCommonTexts.NotificationMissingComponentToPlaceBlockFormat, 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 1, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.WorldLoaded, new MyHudNotification(MyCommonTexts.WorldLoaded, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.ObstructingBlockDuringMerge, new MyHudNotification(MySpaceTexts.NotificationObstructingBlockDuringMerge, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.HideHints, new MyHudNotification(MyCommonTexts.NotificationHideHintsInGameOptions, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Control));
            this.Register(MyNotificationSingletons.HelpHint, new MyHudNotification(MyCommonTexts.NotificationNeedShowHelpScreen, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 1, MyNotificationLevel.Control));
            this.Register(MyNotificationSingletons.ScreenHint, new MyHudNotification(MyCommonTexts.NotificationScreenFormat, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control));
            this.Register(MyNotificationSingletons.RespawnShipWarning, new MyHudNotification(MySpaceTexts.NotificationRespawnShipDelete, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.BuildingOnRespawnShipWarning, new MyHudNotification(MySpaceTexts.NotificationBuildingOnRespawnShip, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerDemotedNone, new MyHudNotification(MySpaceTexts.NotificationPlayerDemoted_None, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerDemotedScripter, new MyHudNotification(MySpaceTexts.NotificationPlayerDemoted_Scripter, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerDemotedModerator, new MyHudNotification(MySpaceTexts.NotificationPlayerDemoted_Moderator, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerDemotedSpaceMaster, new MyHudNotification(MySpaceTexts.NotificationPlayerDemoted_SpaceMaster, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerPromotedScripter, new MyHudNotification(MySpaceTexts.NotificationPlayerPromoted_Scripter, 0x2710, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerPromotedModerator, new MyHudNotification(MySpaceTexts.NotificationPlayerPromoted_Moderator, 0x2710, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerPromotedSpaceMaster, new MyHudNotification(MySpaceTexts.NotificationPlayerPromoted_SpaceMaster, 0x2710, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PlayerPromotedAdmin, new MyHudNotification(MySpaceTexts.NotificationPlayerPromoted_Admin, 0x2710, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.CopySucceeded, new MyHudNotification(MyCommonTexts.NotificationCopySucceeded, 0x514, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.CopyFailed, new MyHudNotification(MyCommonTexts.NotificationCopyFailed, 0x514, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.PasteFailed, new MyHudNotification(MyCommonTexts.NotificationPasteFailed, 0x514, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.CutPermissionFailed, new MyHudNotification(MyCommonTexts.NotificationCutPermissionFailed, 0x514, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.DeletePermissionFailed, new MyHudNotification(MyCommonTexts.NotificationDeletePermissionFailed, 0x514, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.ClientCannotSave, new MyHudNotification(MyCommonTexts.NotificationClientCannotSave, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.CannotSave, new MyHudNotification(MyCommonTexts.NotificationSavingDisabled, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.WheelNotPlaced, new MyHudNotification(MySpaceTexts.NotificationWheelNotPlaced, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.CopyPasteBlockNotAvailable, new MyHudNotification(MyCommonTexts.NotificationCopyPasteBlockNotAvailable, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.CopyPasteFloatingObjectNotAvailable, new MyHudNotification(MyCommonTexts.NotificationCopyPasteFloatingObjectNotAvailable, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.CopyPasteAsteoridObstructed, new MyHudNotification(MySpaceTexts.NotificationCopyPasteAsteroidObstructed, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.TextPanelReadOnly, new MyHudNotification(MyCommonTexts.NotificationTextPanelReadOnly, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.AccessDenied, new MyHudNotification(MyCommonTexts.AccessDenied, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.AdminMenuNotAvailable, new MyHudNotification(MySpaceTexts.AdminMenuNotAvailable, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.HeadNotPlaced, new MyHudNotification(MySpaceTexts.Notification_PistonHeadNotPlaced, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.HeadAlreadyExists, new MyHudNotification(MySpaceTexts.Notification_PistonHeadAlreadyExists, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationLimitsGridSize, 0x1388, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] arguments = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.HELP_SCREEN).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) };
            notification.SetTextFormatArguments(arguments);
            this.Register(MyNotificationSingletons.LimitsGridSize, notification);
            MyHudNotification notification2 = new MyHudNotification(MySpaceTexts.NotificationLimitsPerBlockType, 0x1388, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] objArray2 = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.HELP_SCREEN).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) };
            notification2.SetTextFormatArguments(objArray2);
            this.Register(MyNotificationSingletons.LimitsPerBlockType, notification2);
            MyHudNotification notification3 = new MyHudNotification(MySpaceTexts.NotificationLimitsPlayer, 0x1388, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] objArray3 = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.HELP_SCREEN).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) };
            notification3.SetTextFormatArguments(objArray3);
            this.Register(MyNotificationSingletons.LimitsPlayer, notification3);
            MyHudNotification notification4 = new MyHudNotification(MySpaceTexts.NotificationLimitsPCU, 0x1388, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] objArray4 = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.HELP_SCREEN).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) };
            notification4.SetTextFormatArguments(objArray4);
            this.Register(MyNotificationSingletons.LimitsPCU, notification4);
            MyHudNotification notification5 = new MyHudNotification(MySpaceTexts.NotificationLimitsNoFaction, 0x1388, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            object[] objArray5 = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) };
            notification5.SetTextFormatArguments(objArray5);
            this.Register(MyNotificationSingletons.LimitsNoFaction, notification5);
            this.Register(MyNotificationSingletons.GridReachedPhysicalLimit, new MyHudNotification(MySpaceTexts.NotificationGridReachedPhysicalLimit, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.BlockNotResearched, new MyHudNotification(MySpaceTexts.NotificationBlockNotResearched, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            if (MyPerGameSettings.Game == GameEnum.ME_GAME)
            {
                this.Register(MyNotificationSingletons.GameplayOptions, new MyHudNotification(MyCommonTexts.Notification_GameplayOptions, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control));
                this.Add(MyNotificationSingletons.GameplayOptions);
            }
            this.Register(MyNotificationSingletons.ManipulatingDoorFailed, new MyHudNotification(MyCommonTexts.Notification_CannotManipulateDoor, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            this.Register(MyNotificationSingletons.BlueprintScriptsRemoved, new MyHudNotification(MySpaceTexts.Notification_BlueprintScriptRemoved, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.ConnectionProblem, new MyHudNotification(MyCommonTexts.PerformanceWarningHeading_Connection, 0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            this.Register(MyNotificationSingletons.MissingDLC, new MyHudNotification(MyCommonTexts.RequiresDlc, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) MyFakes.ENABLE_CONTROLLER_HINTS;
            }
            this.FormatNotifications((bool) num1);
            MyInput.Static.JoystickConnected += new Action<bool>(this.Static_JoystickConnected);
        }

        public void Add(MyHudNotificationBase notification)
        {
            object lockObject = this.m_lockObject;
            lock (lockObject)
            {
                List<MyHudNotificationBase> notificationGroup = this.GetNotificationGroup(notification.Priority);
                if (!notificationGroup.Contains(notification))
                {
                    notification.BeforeAdd();
                    notificationGroup.Add(notification);
                }
                notification.ResetAliveTime();
            }
        }

        public void Add(MyNotificationSingletons singleNotification)
        {
            this.Add(this.m_singletons[(int) singleNotification]);
            if (this.OnNotificationAdded != null)
            {
                this.OnNotificationAdded(singleNotification);
            }
        }

        public void Clear()
        {
            MyInput.Static.JoystickConnected -= new Action<bool>(this.Static_JoystickConnected);
            object lockObject = this.m_lockObject;
            lock (lockObject)
            {
                foreach (KeyValuePair<int, List<MyHudNotificationBase>> pair in this.m_notificationsByPriority)
                {
                    pair.Value.Clear();
                }
            }
        }

        private void ClearTexts()
        {
            this.m_drawData.Clear();
            using (List<StringBuilder>.Enumerator enumerator = this.m_texts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Clear();
                }
            }
            this.m_textsPool.DeallocateAll();
            this.m_texts.Clear();
        }

        public static MyHudNotification CreateControlNotification(MyStringId textId, params object[] args)
        {
            MyHudNotification notification1 = new MyHudNotification(textId, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control);
            notification1.SetTextFormatArguments(args);
            return notification1;
        }

        public void Draw()
        {
            int num;
            this.ProcessBeforeDraw(out num);
            this.DrawFog();
            this.DrawNotifications(num);
        }

        private unsafe void DrawFog()
        {
            Vector2 position = this.Position;
            for (int i = 0; i < this.m_drawData.Count; i++)
            {
                if (this.m_drawData[i].HasFog)
                {
                    Vector2 textSize = this.m_drawData[i].TextSize;
                    MyGuiTextShadows.DrawShadow(ref position, ref textSize, null, 1f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                    float* singlePtr1 = (float*) ref position.Y;
                    singlePtr1[0] += textSize.Y;
                }
            }
        }

        private unsafe void DrawNotifications(int visibleCount)
        {
            object lockObject = this.m_lockObject;
            lock (lockObject)
            {
                Vector2 position = this.Position;
                int num = 0;
                StringBuilder text = this.m_textsPool.Allocate(false);
                int key = 5;
                goto TR_001D;
            TR_0003:
                key--;
            TR_001D:
                while (true)
                {
                    List<MyHudNotificationBase> list;
                    if (key < 0)
                    {
                        break;
                    }
                    this.m_notificationsByPriority.TryGetValue(key, out list);
                    if (list != null)
                    {
                        using (List<MyHudNotificationBase>.Enumerator enumerator = list.GetEnumerator())
                        {
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    MyHudNotificationBase current = enumerator.Current;
                                    if (!this.IsDrawn(current))
                                    {
                                        continue;
                                    }
                                    char[] separator = new char[] { '[', ']' };
                                    string[] strArray = this.m_texts[num].ToString().Split(separator);
                                    text.Clear();
                                    text.Append(this.m_texts[num].ToString().UpdateControlsFromNotificationFriendly());
                                    if (strArray.Length <= 1)
                                    {
                                        MyGuiManager.DrawString(current.Font, text, position, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f, new Color?(Color.White), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyVideoSettingsManager.IsTripleHead(), float.PositiveInfinity);
                                    }
                                    else
                                    {
                                        bool flag2 = false;
                                        Vector2 zero = Vector2.Zero;
                                        zero.X = -MyGuiManager.MeasureString(current.Font, text, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f).X / 2f;
                                        StringBuilder builder2 = this.m_textsPool.Allocate(false);
                                        if (builder2 == null)
                                        {
                                            continue;
                                        }
                                        foreach (string str in strArray)
                                        {
                                            builder2.Clear().Append(str.UpdateControlsFromNotificationFriendly());
                                            Vector2 vector4 = MyGuiManager.MeasureString(current.Font, builder2, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f);
                                            if (flag2)
                                            {
                                                MyGuiManager.DrawString(current.Font, builder2, position + zero, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f, new Color?(Color.Yellow), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, MyVideoSettingsManager.IsTripleHead(), float.PositiveInfinity);
                                            }
                                            else
                                            {
                                                MyGuiManager.DrawString(current.Font, builder2, position + zero, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f, new Color?(Color.White), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, MyVideoSettingsManager.IsTripleHead(), float.PositiveInfinity);
                                            }
                                            float* singlePtr1 = (float*) ref zero.X;
                                            singlePtr1[0] += vector4.X;
                                            flag2 = !flag2;
                                        }
                                    }
                                    float* singlePtr2 = (float*) ref position.Y;
                                    singlePtr2[0] += this.m_drawData[num].TextSize.Y;
                                    num++;
                                    visibleCount--;
                                    if (visibleCount != 0)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    goto TR_0003;
                                }
                                break;
                            }
                            break;
                        }
                    }
                    goto TR_0003;
                }
            }
        }

        private void FormatNotifications(bool forJoystick)
        {
            if (forJoystick)
            {
                MyStringId context = MySpaceBindingCreator.CX_CHARACTER;
                MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BASE, MyControlsSpace.CONTROL_MENU);
                MyControllerHelper.GetCodeForControl(context, MyControlsSpace.TOOLBAR_NEXT_ITEM);
                MyControllerHelper.GetCodeForControl(context, MyControlsSpace.TOOLBAR_PREV_ITEM);
                if (MyPerGameSettings.Game == GameEnum.ME_GAME)
                {
                    this.Remove(MyNotificationSingletons.GameplayOptions);
                }
            }
            else
            {
                MyInput.Static.GetGameControl(MyControlsSpace.TOGGLE_HUD);
                MyInput.Static.GetGameControl(MyControlsSpace.SLOT1);
                MyInput.Static.GetGameControl(MyControlsSpace.SLOT2);
                MyInput.Static.GetGameControl(MyControlsSpace.SLOT3);
                MyInput.Static.GetGameControl(MyControlsSpace.BUILD_SCREEN);
                MyInput.Static.GetGameControl(MyControlsSpace.HELP_SCREEN);
                MyInput.Static.GetGameControl(MyControlsSpace.SWITCH_COMPOUND);
                if (MyPerGameSettings.Game == GameEnum.ME_GAME)
                {
                    this.Add(MyNotificationSingletons.GameplayOptions);
                    object[] args = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) };
                    this.SetNotificationTextAndArgs(MyNotificationSingletons.GameplayOptions, MyCommonTexts.Notification_GameplayOptions, args);
                }
            }
        }

        public MyHudNotificationBase Get(MyNotificationSingletons singleNotification) => 
            this.m_singletons[(int) singleNotification];

        private List<MyHudNotificationBase> GetNotificationGroup(int priority)
        {
            List<MyHudNotificationBase> list;
            if (!this.m_notificationsByPriority.TryGetValue(priority, out list))
            {
                list = new List<MyHudNotificationBase>();
                this.m_notificationsByPriority[priority] = list;
            }
            return list;
        }

        private bool IsDrawn(MyHudNotificationBase notification)
        {
            bool alive = notification.Alive;
            if (notification.IsControlsHint)
            {
                alive = alive && MySandboxGame.Config.ControlsHints;
            }
            if ((MyHud.MinimalHud && !MyHud.CutsceneHud) && (notification.Level != MyNotificationLevel.Important))
            {
                alive = false;
            }
            if (MyHud.CutsceneHud && (notification.Level == MyNotificationLevel.Control))
            {
                alive = false;
            }
            return alive;
        }

        private void ProcessBeforeDraw(out int visibleCount)
        {
            this.ClearTexts();
            visibleCount = 0;
            object lockObject = this.m_lockObject;
            lock (lockObject)
            {
                StringBuilder text = this.m_textsPool.Allocate(false);
                int key = 5;
                goto TR_0010;
            TR_0003:
                key--;
            TR_0010:
                while (true)
                {
                    List<MyHudNotificationBase> list;
                    if (key < 0)
                    {
                        break;
                    }
                    this.m_notificationsByPriority.TryGetValue(key, out list);
                    if (list != null)
                    {
                        using (List<MyHudNotificationBase>.Enumerator enumerator = list.GetEnumerator())
                        {
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    MyHudNotificationBase current = enumerator.Current;
                                    if (!this.IsDrawn(current))
                                    {
                                        continue;
                                    }
                                    text.Clear();
                                    text.Append(current.GetText().UpdateControlsFromNotificationFriendly());
                                    NotificationDrawData item = new NotificationDrawData {
                                        HasFog = true,
                                        TextSize = MyGuiManager.MeasureString(current.Font, text, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f)
                                    };
                                    this.m_drawData.Add(item);
                                    StringBuilder builder2 = this.m_textsPool.Allocate(false).Clear();
                                    builder2.Append(current.GetText());
                                    this.m_texts.Add(builder2);
                                    visibleCount++;
                                    if (visibleCount != 9)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    goto TR_0003;
                                }
                                break;
                            }
                            break;
                        }
                    }
                    goto TR_0003;
                }
            }
        }

        public void Register(MyNotificationSingletons singleton, MyHudNotificationBase notification)
        {
            this.m_singletons[(int) singleton] = notification;
        }

        public void ReloadTexts()
        {
            int num1;
            if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) MyFakes.ENABLE_CONTROLLER_HINTS;
            }
            this.FormatNotifications((bool) num1);
            object lockObject = this.m_lockObject;
            lock (lockObject)
            {
                foreach (KeyValuePair<int, List<MyHudNotificationBase>> pair in this.m_notificationsByPriority)
                {
                    using (List<MyHudNotificationBase>.Enumerator enumerator2 = pair.Value.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.SetTextDirty();
                        }
                    }
                }
            }
        }

        public void Remove(MyHudNotificationBase notification)
        {
            if (notification != null)
            {
                object lockObject = this.m_lockObject;
                lock (lockObject)
                {
                    this.GetNotificationGroup(notification.Priority).Remove(notification);
                }
            }
        }

        public void Remove(MyNotificationSingletons singleNotification)
        {
            this.Remove(this.m_singletons[(int) singleNotification]);
        }

        private void SetNotificationTextAndArgs(MyNotificationSingletons type, MyStringId textId, params object[] args)
        {
            MyHudNotification notification = this.Get(type) as MyHudNotification;
            notification.Text = textId;
            notification.SetTextFormatArguments(args);
            this.Add(notification);
        }

        private void Static_JoystickConnected(bool value)
        {
            this.FormatNotifications(value && MyFakes.ENABLE_CONTROLLER_HINTS);
        }

        public void UpdateBeforeSimulation()
        {
            object lockObject = this.m_lockObject;
            lock (lockObject)
            {
                foreach (KeyValuePair<int, List<MyHudNotificationBase>> pair in this.m_notificationsByPriority)
                {
                    using (List<MyHudNotificationBase>.Enumerator enumerator2 = pair.Value.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.AddAliveTime(0x10);
                        }
                    }
                }
                foreach (KeyValuePair<int, List<MyHudNotificationBase>> pair2 in this.m_notificationsByPriority)
                {
                    foreach (MyHudNotificationBase base2 in pair2.Value)
                    {
                        if (this.m_disappearedPredicate(base2))
                        {
                            base2.BeforeRemove();
                        }
                    }
                    pair2.Value.RemoveAll(this.m_disappearedPredicate);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyHudNotifications.<>c <>9 = new MyHudNotifications.<>c();
            public static Predicate<MyHudNotificationBase> <>9__17_0;

            internal bool <.ctor>b__17_0(MyHudNotificationBase x) => 
                !x.Alive;
        }

        public class ControlsHelper
        {
            private MyControl[] m_controls;

            public ControlsHelper(params MyControl[] controls)
            {
                this.m_controls = controls;
            }

            public override string ToString() => 
                string.Join(", ", (IEnumerable<string>) (from s in this.m_controls
                    select s.ButtonNamesIgnoreSecondary into s
                    where !string.IsNullOrEmpty(s)
                    select s));

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyHudNotifications.ControlsHelper.<>c <>9 = new MyHudNotifications.ControlsHelper.<>c();
                public static Func<MyControl, string> <>9__2_0;
                public static Func<string, bool> <>9__2_1;

                internal string <ToString>b__2_0(MyControl s) => 
                    s.ButtonNamesIgnoreSecondary;

                internal bool <ToString>b__2_1(string s) => 
                    !string.IsNullOrEmpty(s);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NotificationDrawData
        {
            public bool HasFog;
            public Vector2 TextSize;
        }
    }
}

