namespace SpaceEngineers.Game.Entities.UseObjects
{
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("inventory"), MyUseObject("conveyor")]
    internal class MyUseObjectInventory : MyUseObjectBase
    {
        public readonly MyEntity Entity;
        public readonly Matrix LocalMatrix;

        public MyUseObjectInventory(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.Entity = owner as MyEntity;
            this.LocalMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            MyCubeBlock entity = this.Entity as MyCubeBlock;
            string str = (entity != null) ? entity.DefinitionDisplayNameText : this.Entity.DisplayNameText;
            if ((actionEnum == UseActionEnum.OpenTerminal) || (actionEnum != UseActionEnum.OpenInventory))
            {
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToOpenTerminal
                };
                description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]", str };
                description.IsTextControlHint = true;
                description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenControlPanel);
                description.JoystickFormatParams = new object[] { str };
                return description;
            }
            description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToOpenInventory
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.INVENTORY) + "]", str };
            description.IsTextControlHint = true;
            description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenInventory);
            description.JoystickFormatParams = new object[] { str };
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyContainerDropComponent component;
            MyCharacter user = entity as MyCharacter;
            MyCubeBlock block = this.Entity as MyCubeBlock;
            if (block != null)
            {
                if (!MySession.Static.CheckDLCAndNotify(block.BlockDefinition))
                {
                    return;
                }
                if (!block.GetUserRelationToOwner(user.ControllerInfo.ControllingIdentityId).IsFriendly() && !MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals))
                {
                    if (user.ControllerInfo.IsLocallyHumanControlled())
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                    }
                    return;
                }
            }
            if (!this.Entity.Components.TryGet<MyContainerDropComponent>(out component))
            {
                MyGuiScreenTerminal.Show((actionEnum == UseActionEnum.OpenTerminal) ? MyTerminalPageEnum.ControlPanel : MyTerminalPageEnum.Inventory, user, this.Entity);
            }
            else
            {
                MyGuiScreenClaimGameItem screen = new MyGuiScreenClaimGameItem(component, user.GetPlayerIdentityId());
                MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                MyGuiSandbox.AddScreen(screen);
            }
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.LocalMatrix * this.Entity.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.Entity.WorldMatrix;

        public override uint RenderObjectID =>
            this.Entity.Render.GetRenderObjectID();

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions =>
            (this.PrimaryAction | this.SecondaryAction);

        public override UseActionEnum PrimaryAction =>
            UseActionEnum.OpenInventory;

        public override UseActionEnum SecondaryAction =>
            UseActionEnum.OpenTerminal;

        public override bool ContinuousUsage =>
            false;

        public override bool PlayIndicatorSound =>
            true;
    }
}

