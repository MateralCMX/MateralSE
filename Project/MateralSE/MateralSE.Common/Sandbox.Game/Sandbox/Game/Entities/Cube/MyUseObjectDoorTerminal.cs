namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    public class MyUseObjectDoorTerminal : MyUseObjectBase
    {
        public readonly MyDoor Door;
        public readonly Matrix LocalMatrix;

        public MyUseObjectDoorTerminal(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.Door = (MyDoor) owner;
            this.LocalMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            if (actionEnum == UseActionEnum.Manipulate)
            {
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToOpenDoor
                };
                description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]", this.Door.DefinitionDisplayNameText };
                description.IsTextControlHint = true;
                description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]", this.Door.DefinitionDisplayNameText };
                return description;
            }
            if (actionEnum != UseActionEnum.OpenTerminal)
            {
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToOpenControlPanel
                };
                description.FormatParams = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL), this.Door.DefinitionDisplayNameText };
                description.IsTextControlHint = true;
                return description;
            }
            description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToOpenControlPanel
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]", this.Door.DefinitionDisplayNameText };
            description.IsTextControlHint = true;
            description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenControlPanel);
            description.JoystickFormatParams = new object[] { this.Door.DefinitionDisplayNameText };
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyCharacter user = entity as MyCharacter;
            if (!this.Door.GetUserRelationToOwner(user.ControllerInfo.ControllingIdentityId).IsFriendly() && !MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals))
            {
                if (user.ControllerInfo.IsLocallyHumanControlled())
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                }
            }
            else if (actionEnum == UseActionEnum.Manipulate)
            {
                this.Door.SetOpenRequest(!this.Door.Open, user.ControllerInfo.ControllingIdentityId);
            }
            else if (actionEnum == UseActionEnum.OpenTerminal)
            {
                MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, user, this.Door);
            }
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.LocalMatrix * this.Door.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.Door.WorldMatrix;

        public override uint RenderObjectID =>
            this.Door.Render.GetRenderObjectID();

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions =>
            (this.PrimaryAction | this.SecondaryAction);

        public override UseActionEnum PrimaryAction =>
            UseActionEnum.Manipulate;

        public override UseActionEnum SecondaryAction =>
            UseActionEnum.OpenTerminal;

        public override bool ContinuousUsage =>
            false;

        public override bool PlayIndicatorSound =>
            true;
    }
}

