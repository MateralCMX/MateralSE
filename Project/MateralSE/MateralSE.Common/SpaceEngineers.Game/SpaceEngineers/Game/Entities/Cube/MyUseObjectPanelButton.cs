namespace SpaceEngineers.Game.Entities.Cube
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics.GUI;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("panel")]
    public class MyUseObjectPanelButton : MyUseObjectBase
    {
        private readonly MyButtonPanel m_buttonPanel;
        private readonly Matrix m_localMatrix;
        private int m_index;
        private MyGps m_buttonDesc;

        public MyUseObjectPanelButton(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.m_buttonPanel = owner as MyButtonPanel;
            this.m_localMatrix = dummyData.Matrix;
            int result = 0;
            char[] separator = new char[] { '_' };
            string[] textArray1 = dummyName.Split(separator);
            int.TryParse(textArray1[textArray1.Length - 1], out result);
            this.m_index = result - 1;
            if (this.m_index >= this.m_buttonPanel.BlockDefinition.ButtonCount)
            {
                MyLog.Default.WriteLine($"{this.m_buttonPanel.BlockDefinition.Id.SubtypeName} Button index higher than defined count.");
                this.m_index = this.m_buttonPanel.BlockDefinition.ButtonCount - 1;
            }
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            this.m_buttonPanel.Toolbar.UpdateItem(this.m_index);
            MyToolbarItem itemAtIndex = this.m_buttonPanel.Toolbar.GetItemAtIndex(this.m_index);
            if (actionEnum != UseActionEnum.Manipulate)
            {
                if (actionEnum != UseActionEnum.OpenTerminal)
                {
                    description = new MyActionDescription {
                        Text = MySpaceTexts.NotificationHintPressToOpenButtonPanel
                    };
                    description.FormatParams = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) };
                    description.IsTextControlHint = true;
                    return description;
                }
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToOpenButtonPanel
                };
                description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]" };
                description.IsTextControlHint = true;
                description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenButtonPanel);
                return description;
            }
            if (this.m_buttonDesc == null)
            {
                this.m_buttonDesc = new MyGps();
                this.m_buttonDesc.Description = "";
                this.m_buttonDesc.CoordsFunc = () => this.MarkerPosition;
                this.m_buttonDesc.ShowOnHud = true;
                this.m_buttonDesc.DiscardAt = null;
                this.m_buttonDesc.AlwaysVisible = true;
            }
            MyHud.ButtonPanelMarkers.RegisterMarker(this.m_buttonDesc);
            this.SetButtonName(this.m_buttonPanel.GetCustomButtonName(this.m_index));
            if (itemAtIndex == null)
            {
                return new MyActionDescription { Text = MySpaceTexts.Blank };
            }
            description = new MyActionDescription {
                Text = MyCommonTexts.NotificationHintPressToUse
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]", itemAtIndex.DisplayName };
            description.IsTextControlHint = true;
            description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]", itemAtIndex.DisplayName };
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
            this.RemoveButtonMarker();
        }

        public void RemoveButtonMarker()
        {
            if (this.m_buttonDesc != null)
            {
                MyHud.ButtonPanelMarkers.UnregisterMarker(this.m_buttonDesc);
            }
        }

        private void SetButtonName(string name)
        {
            if ((!this.m_buttonPanel.IsFunctional || !this.m_buttonPanel.IsWorking) || (!this.m_buttonPanel.HasLocalPlayerAccess() && !this.m_buttonPanel.AnyoneCanUse))
            {
                this.m_buttonDesc.Name = "";
            }
            else
            {
                this.m_buttonDesc.Name = name;
            }
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyContainerDropComponent component;
            MyCharacter character = entity as MyCharacter;
            if (this.m_buttonPanel.Components.TryGet<MyContainerDropComponent>(out component))
            {
                MyGuiScreenClaimGameItem screen = new MyGuiScreenClaimGameItem(component, character.GetPlayerIdentityId());
                MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                MyGuiSandbox.AddScreen(screen);
            }
            else if (actionEnum != UseActionEnum.Manipulate)
            {
                if ((actionEnum == UseActionEnum.OpenTerminal) && this.m_buttonPanel.HasLocalPlayerAccess())
                {
                    MyToolbarComponent.CurrentToolbar = this.m_buttonPanel.Toolbar;
                    MyGuiScreenBase @static = MyGuiScreenToolbarConfigBase.Static;
                    if (@static == null)
                    {
                        object[] args = new object[] { 0, this.m_buttonPanel };
                        @static = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    }
                    MyToolbarComponent.AutoUpdate = false;
                    @static.Closed += source => (MyToolbarComponent.AutoUpdate = true);
                    MyGuiSandbox.AddScreen(@static);
                }
            }
            else if (this.m_buttonPanel.IsWorking)
            {
                if (!this.m_buttonPanel.AnyoneCanUse && !this.m_buttonPanel.HasLocalPlayerAccess())
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                }
                else
                {
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyButtonPanel, int>(this.m_buttonPanel, x => new Action<int>(x.ActivateButton), this.m_index, targetEndpoint);
                    long playerId = (character != null) ? character.GetPlayerIdentityId() : 0L;
                    if (MyVisualScriptLogicProvider.ButtonPressedTerminalName != null)
                    {
                        MyVisualScriptLogicProvider.ButtonPressedTerminalName(this.m_buttonPanel.CustomName.ToString(), this.m_index, playerId, this.m_buttonPanel.EntityId);
                    }
                    if (MyVisualScriptLogicProvider.ButtonPressedEntityName != null)
                    {
                        MyVisualScriptLogicProvider.ButtonPressedEntityName(this.m_buttonPanel.Name, this.m_index, playerId, this.m_buttonPanel.EntityId);
                    }
                }
            }
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.m_localMatrix * this.m_buttonPanel.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.m_buttonPanel.WorldMatrix;

        public Vector3D MarkerPosition =>
            this.ActivationMatrix.Translation;

        public override uint RenderObjectID =>
            this.m_buttonPanel.Render.GetRenderObjectID();

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions =>
            (this.PrimaryAction | this.SecondaryAction);

        public override UseActionEnum PrimaryAction =>
            ((this.m_buttonPanel.Toolbar.GetItemAtIndex(this.m_index) != null) ? UseActionEnum.Manipulate : UseActionEnum.None);

        public override UseActionEnum SecondaryAction =>
            UseActionEnum.OpenTerminal;

        public override bool ContinuousUsage =>
            false;

        public override bool PlayIndicatorSound =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyUseObjectPanelButton.<>c <>9 = new MyUseObjectPanelButton.<>c();
            public static Func<MyButtonPanel, Action<int>> <>9__27_0;
            public static MyGuiScreenBase.ScreenHandler <>9__27_1;

            internal Action<int> <Use>b__27_0(MyButtonPanel x) => 
                new Action<int>(x.ActivateButton);

            internal void <Use>b__27_1(MyGuiScreenBase source)
            {
                MyToolbarComponent.AutoUpdate = true;
            }
        }
    }
}

