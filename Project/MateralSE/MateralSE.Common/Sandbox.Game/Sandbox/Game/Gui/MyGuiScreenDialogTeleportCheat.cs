namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenDialogTeleportCheat : MyGuiScreenBase
    {
        private List<IMyGps> m_prefabDefinitions;
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlCombobox m_prefabs;

        public MyGuiScreenDialogTeleportCheat() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_prefabDefinitions = new List<IMyGps>();
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void cancelButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void confirmButton_OnButtonClick(MyGuiControlButton sender)
        {
            int selectedKey = (int) this.m_prefabs.GetSelectedKey();
            IMyGps gps = this.m_prefabDefinitions[(selectedKey == -1) ? 0 : selectedKey];
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<Vector3D>(s => new Action<Vector3D>(MyAlesDebugInputComponent.TravelToWaypoint), gps.Coords, targetEndpoint, position);
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDialogTravelToCheat";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
        }

        public override void RecreateControls(bool contructor)
        {
            base.RecreateControls(contructor);
            Vector2? size = null;
            Vector4? colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.1f), size, "Select gps you want to reach. (Dont use for grids with subgrids.)", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            colorMask = null;
            size = null;
            size = null;
            colorMask = null;
            this.m_prefabs = new MyGuiControlCombobox(new Vector2(0.2f, 0f), new Vector2(0.3f, 0.05f), colorMask, size, 10, size, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, colorMask);
            colorMask = null;
            int? buttonIndex = null;
            this.m_confirmButton = new MyGuiControlButton(new Vector2(0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Confirm"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            colorMask = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(-0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Cancel"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            List<IMyGps> list = new List<IMyGps>();
            MySession.Static.Gpss.GetGpsList(MySession.Static.LocalPlayerId, list);
            foreach (IMyGps gps in list)
            {
                int count = this.m_prefabDefinitions.Count;
                this.m_prefabDefinitions.Add(gps);
                buttonIndex = null;
                this.m_prefabs.AddItem((long) count, gps.Name, buttonIndex, null);
            }
            this.Controls.Add(this.m_prefabs);
            this.Controls.Add(this.m_confirmButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDialogTeleportCheat.<>c <>9 = new MyGuiScreenDialogTeleportCheat.<>c();
            public static Func<IMyEventOwner, Action<Vector3D>> <>9__8_0;

            internal Action<Vector3D> <confirmButton_OnButtonClick>b__8_0(IMyEventOwner s) => 
                new Action<Vector3D>(MyAlesDebugInputComponent.TravelToWaypoint);
        }
    }
}

