namespace SpaceEngineers.Game.Entities.UseObjects
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("block")]
    internal class MyUseObjectLifeSupportingBlock : MyUseObjectBase
    {
        private Matrix m_localMatrix;

        public MyUseObjectLifeSupportingBlock(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.m_localMatrix = dummyData.Matrix;
        }

        public override unsafe MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            switch (actionEnum)
            {
                case UseActionEnum.Manipulate:
                    description = new MyActionDescription {
                        Text = MySpaceTexts.NotificationHintPressToRechargeInMedicalRoom
                    };
                    description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]" };
                    description.IsTextControlHint = true;
                    description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]" };
                    return description;

                case UseActionEnum.OpenTerminal:
                    description = new MyActionDescription {
                        Text = MySpaceTexts.NotificationHintPressToOpenTerminal
                    };
                    description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]" };
                    description.IsTextControlHint = true;
                    description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenTerminal);
                    return description;

                case UseActionEnum.OpenInventory:
                {
                    MyActionDescription* descriptionPtr1;
                    description = new MyActionDescription {
                        Text = MySpaceTexts.NotificationHintPressToOpenTerminal
                    };
                    description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]" };
                    description.IsTextControlHint = true;
                    description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenControlPanel);
                    object[] objArray6 = new object[] { (this.Owner is MyCubeBlock) ? ((MyCubeBlock) this.Owner).DefinitionDisplayNameText : this.Owner.DisplayNameText };
                    descriptionPtr1->JoystickFormatParams = new object[] { (this.Owner is MyCubeBlock) ? ((MyCubeBlock) this.Owner).DefinitionDisplayNameText : this.Owner.DisplayNameText };
                    descriptionPtr1 = (MyActionDescription*) ref description;
                    return description;
                }
            }
            description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToOpenTerminal
            };
            description.FormatParams = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) };
            description.IsTextControlHint = true;
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyCharacter character = entity as MyCharacter;
            if (character != null)
            {
                MyPlayer playerFromCharacter = MyPlayer.GetPlayerFromCharacter(character);
                if (!this.Owner.GetUserRelationToOwner(character.ControllerInfo.Controller.Player.Identity.IdentityId).IsFriendly() && ((playerFromCharacter == null) || !MySession.Static.IsUserSpaceMaster(playerFromCharacter.Client.SteamUserId)))
                {
                    if (ReferenceEquals(character.ControllerInfo.Controller.Player, MySession.Static.LocalHumanPlayer))
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                    }
                }
                else if (actionEnum == UseActionEnum.OpenTerminal)
                {
                    this.Owner.ShowTerminal(character);
                }
                else if (actionEnum == UseActionEnum.Manipulate)
                {
                    this.Owner.Components.Get<MyLifeSupportingComponent>().OnSupportRequested(character);
                }
                else if (actionEnum == UseActionEnum.OpenInventory)
                {
                    MyGuiScreenTerminal.Show(MyTerminalPageEnum.Inventory, character, this.Owner as MyEntity);
                }
            }
        }

        public IMyLifeSupportingBlock Owner =>
            ((IMyLifeSupportingBlock) base.Owner);

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.m_localMatrix * this.Owner.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.Owner.WorldMatrix;

        public override uint RenderObjectID
        {
            get
            {
                uint[] renderObjectIDs = this.Owner.Render.RenderObjectIDs;
                return ((renderObjectIDs.Length == 0) ? uint.MaxValue : renderObjectIDs[0]);
            }
        }

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions
        {
            get
            {
                UseActionEnum enum2 = this.PrimaryAction | this.SecondaryAction;
                if (this.Owner.HasInventory)
                {
                    enum2 |= UseActionEnum.OpenInventory;
                }
                return enum2;
            }
        }

        public override UseActionEnum PrimaryAction =>
            UseActionEnum.Manipulate;

        public override UseActionEnum SecondaryAction =>
            UseActionEnum.OpenTerminal;

        public override bool ContinuousUsage =>
            true;

        public override bool PlayIndicatorSound =>
            true;
    }
}

