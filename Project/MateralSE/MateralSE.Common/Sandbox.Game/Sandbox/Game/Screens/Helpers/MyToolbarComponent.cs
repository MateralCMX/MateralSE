namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Input;
    using VRage.Utils;
    using VRageRender.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyToolbarComponent : MySessionComponentBase
    {
        private static readonly MyStringId[] m_slotControls;
        private static MyToolbarComponent m_instance;
        private MyToolbar m_currentToolbar;
        private MyToolbar m_universalCharacterToolbar = new MyToolbar(MyToolbarType.Character, 9, 9);
        private bool m_toolbarControlIsShown;
        [CompilerGenerated]
        private static Action CurrentToolbarChanged;
        private static StringBuilder m_slotControlTextCache;

        public static  event Action CurrentToolbarChanged
        {
            [CompilerGenerated] add
            {
                Action currentToolbarChanged = CurrentToolbarChanged;
                while (true)
                {
                    Action a = currentToolbarChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    currentToolbarChanged = Interlocked.CompareExchange<Action>(ref CurrentToolbarChanged, action3, a);
                    if (ReferenceEquals(currentToolbarChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action currentToolbarChanged = CurrentToolbarChanged;
                while (true)
                {
                    Action source = currentToolbarChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    currentToolbarChanged = Interlocked.CompareExchange<Action>(ref CurrentToolbarChanged, action3, source);
                    if (ReferenceEquals(currentToolbarChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyToolbarComponent()
        {
            MyStringId[] idArray1 = new MyStringId[10];
            idArray1[0] = MyControlsSpace.SLOT1;
            idArray1[1] = MyControlsSpace.SLOT2;
            idArray1[2] = MyControlsSpace.SLOT3;
            idArray1[3] = MyControlsSpace.SLOT4;
            idArray1[4] = MyControlsSpace.SLOT5;
            idArray1[5] = MyControlsSpace.SLOT6;
            idArray1[6] = MyControlsSpace.SLOT7;
            idArray1[7] = MyControlsSpace.SLOT8;
            idArray1[8] = MyControlsSpace.SLOT9;
            idArray1[9] = MyControlsSpace.SLOT0;
            m_slotControls = idArray1;
            m_slotControlTextCache = new StringBuilder();
        }

        public MyToolbarComponent()
        {
            this.m_currentToolbar = this.m_universalCharacterToolbar;
            AutoUpdate = true;
        }

        private MyToolbarType GetCurrentToolbarType() => 
            (!MyBlockBuilderBase.SpectatorIsBuilding ? ((MySession.Static.ControlledEntity != null) ? MySession.Static.ControlledEntity.ToolbarType : MyToolbarType.Spectator) : MyToolbarType.Spectator);

        public static MyObjectBuilder_Toolbar GetObjectBuilder(MyToolbarType type)
        {
            MyObjectBuilder_Toolbar objectBuilder = m_instance.m_currentToolbar.GetObjectBuilder();
            objectBuilder.ToolbarType = type;
            return objectBuilder;
        }

        public static StringBuilder GetSlotControlText(int slotIndex)
        {
            if (!m_slotControls.IsValidIndex<MyStringId>(slotIndex))
            {
                return null;
            }
            m_slotControlTextCache.Clear();
            MyInput.Static.GetGameControl(m_slotControls[slotIndex]).AppendBoundKeyJustOne(ref m_slotControlTextCache);
            return m_slotControlTextCache;
        }

        private static MyToolbar GetToolbar() => 
            m_instance.m_currentToolbar;

        public override void HandleInput()
        {
            try
            {
                int num;
                MyStringId context = (MySession.Static.ControlledEntity != null) ? MySession.Static.ControlledEntity.ControlContext : MyStringId.NullOrEmpty;
                MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
                if (!ReferenceEquals(screenWithFocus, MyGuiScreenGamePlay.Static) && !IsToolbarControlShown)
                {
                    goto TR_0000;
                }
                if (CurrentToolbar == null)
                {
                    goto TR_0000;
                }
                else if (MyGuiScreenGamePlay.DisableInput)
                {
                    goto TR_0000;
                }
                else
                {
                    num = 0;
                }
                goto TR_0022;
            TR_0013:
                num++;
            TR_0022:
                while (true)
                {
                    if (num >= m_slotControls.Length)
                    {
                        if (!ReferenceEquals(screenWithFocus, MyGuiScreenGamePlay.Static))
                        {
                            if (!(screenWithFocus is MyGuiScreenCubeBuilder) && !(screenWithFocus is MyGuiScreenToolbarConfigBase))
                            {
                                break;
                            }
                            if (!((MyGuiScreenToolbarConfigBase) screenWithFocus).AllowToolbarKeys())
                            {
                                break;
                            }
                        }
                        if (CurrentToolbar != null)
                        {
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.TOOLBAR_NEXT_ITEM, MyControlStateType.NEW_PRESSED, false))
                            {
                                CurrentToolbar.SelectNextSlot();
                            }
                            else if (MyControllerHelper.IsControl(context, MyControlsSpace.TOOLBAR_PREV_ITEM, MyControlStateType.NEW_PRESSED, false))
                            {
                                CurrentToolbar.SelectPreviousSlot();
                            }
                            if (MySpectator.Static.SpectatorCameraMovement != MySpectatorCameraMovementEnum.ConstantDelta)
                            {
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.TOOLBAR_UP, MyControlStateType.NEW_PRESSED, false))
                                {
                                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                    CurrentToolbar.PageUp();
                                    MySession @static = MySession.Static;
                                    @static.ToolbarPageSwitches++;
                                    if (MySpaceAnalytics.Instance != null)
                                    {
                                        MySpaceAnalytics.Instance.ReportToolbarSwitch(CurrentToolbar.CurrentPage);
                                    }
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.TOOLBAR_DOWN, MyControlStateType.NEW_PRESSED, false))
                                {
                                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                    CurrentToolbar.PageDown();
                                    MySession @static = MySession.Static;
                                    @static.ToolbarPageSwitches++;
                                    if (MySpaceAnalytics.Instance != null)
                                    {
                                        MySpaceAnalytics.Instance.ReportToolbarSwitch(CurrentToolbar.CurrentPage);
                                    }
                                }
                            }
                        }
                        break;
                    }
                    if (MyControllerHelper.IsControl(context, m_slotControls[num], MyControlStateType.NEW_PRESSED, false))
                    {
                        if (MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            if (num < CurrentToolbar.PageCount)
                            {
                                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                CurrentToolbar.SwitchToPage(num);
                                MySession @static = MySession.Static;
                                @static.ToolbarPageSwitches++;
                                if (MySpaceAnalytics.Instance != null)
                                {
                                    MySpaceAnalytics.Instance.ReportToolbarSwitch(CurrentToolbar.CurrentPage);
                                }
                            }
                        }
                        else if ((((screenWithFocus is MyGuiScreenScriptingTools) || ReferenceEquals(screenWithFocus, MyGuiScreenGamePlay.Static)) || (((screenWithFocus is MyGuiScreenCubeBuilder) || (screenWithFocus is MyGuiScreenToolbarConfigBase)) && ((MyGuiScreenToolbarConfigBase) screenWithFocus).AllowToolbarKeys())) && (CurrentToolbar != null))
                        {
                            CurrentToolbar.ActivateItemAtSlot(num, false, true, true);
                        }
                    }
                    goto TR_0013;
                }
            }
            finally
            {
            }
        TR_0000:
            base.HandleInput();
        }

        public static void InitCharacterToolbar(MyObjectBuilder_Toolbar characterToolbar)
        {
            m_instance.m_universalCharacterToolbar.Init(characterToolbar, null, true);
        }

        public static void InitToolbar(MyToolbarType type, MyObjectBuilder_Toolbar builder)
        {
            if ((builder != null) && (builder.ToolbarType != type))
            {
                builder.ToolbarType = type;
            }
            m_instance.m_currentToolbar.Init(builder, null, true);
        }

        public override void LoadData()
        {
            m_instance = this;
            base.LoadData();
        }

        protected override void UnloadData()
        {
            m_instance = null;
            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                using (Stats.Generic.Measure("Toolbar.Update()"))
                {
                    UpdateCurrentToolbar();
                    if (CurrentToolbar != null)
                    {
                        CurrentToolbar.Update();
                    }
                }
            }
            finally
            {
            }
            base.UpdateBeforeSimulation();
        }

        public static void UpdateCurrentToolbar()
        {
            if (AutoUpdate && (((MySession.Static.ControlledEntity != null) && (MySession.Static.ControlledEntity.Toolbar != null)) && !ReferenceEquals(m_instance.m_currentToolbar, MySession.Static.ControlledEntity.Toolbar)))
            {
                m_instance.m_currentToolbar = MySession.Static.ControlledEntity.Toolbar;
                if (CurrentToolbarChanged != null)
                {
                    CurrentToolbarChanged();
                }
            }
        }

        public static bool IsToolbarControlShown
        {
            get => 
                ((m_instance != null) ? m_instance.m_toolbarControlIsShown : false);
            set
            {
                if (m_instance != null)
                {
                    m_instance.m_toolbarControlIsShown = value;
                }
            }
        }

        public static MyToolbar CurrentToolbar
        {
            get => 
                ((m_instance != null) ? m_instance.m_currentToolbar : null);
            set
            {
                if (!ReferenceEquals(m_instance.m_currentToolbar, value))
                {
                    m_instance.m_currentToolbar = value;
                    if (CurrentToolbarChanged != null)
                    {
                        CurrentToolbarChanged();
                    }
                }
            }
        }

        public static MyToolbar CharacterToolbar =>
            ((m_instance != null) ? m_instance.m_universalCharacterToolbar : null);

        public static bool GlobalBuilding
        {
            get
            {
                bool isDedicated = Sandbox.Engine.Platform.Game.IsDedicated;
                return (MySession.Static.IsCameraUserControlledSpectator() && MyInput.Static.ENABLE_DEVELOPER_KEYS);
            }
        }

        public static bool CreativeModeEnabled =>
            (MyFakes.UNLIMITED_CHARACTER_BUILDING || MySession.Static.CreativeMode);

        public static bool AutoUpdate
        {
            [CompilerGenerated]
            get => 
                <AutoUpdate>k__BackingField;
            [CompilerGenerated]
            set => 
                (<AutoUpdate>k__BackingField = value);
        }
    }
}

