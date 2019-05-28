namespace Sandbox.Game.Entities.Cube
{
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

    [MyUseObject("terminal")]
    public class MyUseObjectTerminal : MyUseObjectBase
    {
        public readonly MyCubeBlock Block;
        public readonly Matrix LocalMatrix;

        public MyUseObjectTerminal(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.Block = owner as MyCubeBlock;
            this.LocalMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            if ((actionEnum == UseActionEnum.OpenTerminal) || (actionEnum != UseActionEnum.OpenInventory))
            {
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToOpenControlPanel
                };
                description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]", this.Block.DefinitionDisplayNameText };
                description.IsTextControlHint = true;
                description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenControlPanel);
                description.JoystickFormatParams = new object[] { this.Block.DefinitionDisplayNameText };
                return description;
            }
            description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToOpenInventory
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.INVENTORY) + "]", this.Block.DefinitionDisplayNameText };
            description.IsTextControlHint = true;
            description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenInventory);
            description.JoystickFormatParams = new object[] { this.Block.DefinitionDisplayNameText };
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            if (MySession.Static.CheckDLCAndNotify(this.Block.BlockDefinition))
            {
                MyCharacter user = entity as MyCharacter;
                if (!this.Block.GetUserRelationToOwner(user.ControllerInfo.ControllingIdentityId).IsFriendly() && !MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals))
                {
                    if (user.ControllerInfo.IsLocallyHumanControlled())
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                    }
                }
                else if (actionEnum == UseActionEnum.OpenTerminal)
                {
                    MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, user, this.Block);
                }
                else if ((actionEnum == UseActionEnum.OpenInventory) && (this.Block.GetInventory(0) != null))
                {
                    MyGuiScreenTerminal.Show(MyTerminalPageEnum.Inventory, user, this.Block);
                }
            }
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.LocalMatrix * this.Block.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.Block.WorldMatrix;

        public override uint RenderObjectID =>
            this.Block.Render.GetRenderObjectID();

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions
        {
            get
            {
                UseActionEnum openTerminal = UseActionEnum.OpenTerminal;
                if (this.Block.GetInventory(0) != null)
                {
                    openTerminal |= UseActionEnum.OpenInventory;
                }
                return openTerminal;
            }
        }

        public override UseActionEnum PrimaryAction =>
            UseActionEnum.OpenTerminal;

        public override UseActionEnum SecondaryAction =>
            ((this.Block.GetInventory(0) != null) ? UseActionEnum.OpenInventory : UseActionEnum.None);

        public override bool ContinuousUsage =>
            false;

        public override bool PlayIndicatorSound =>
            true;
    }
}

