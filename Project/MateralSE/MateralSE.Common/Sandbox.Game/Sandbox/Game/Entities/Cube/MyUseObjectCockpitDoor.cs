namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("cockpit")]
    internal class MyUseObjectCockpitDoor : MyUseObjectBase
    {
        public readonly IMyEntity Cockpit;
        public readonly Matrix LocalMatrix;

        public MyUseObjectCockpitDoor(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.Cockpit = owner;
            this.LocalMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToEnterCockpit
            };
            description.FormatParams = new object[] { "[" + MyGuiSandbox.GetKeyName(MyControlsSpace.USE) + "]", ((VRage.Game.Entity.MyEntity) this.Cockpit).DisplayNameText };
            description.IsTextControlHint = true;
            description.JoystickFormatParams = new object[] { "]" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]", ((VRage.Game.Entity.MyEntity) this.Cockpit).DisplayNameText };
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
            if (this.Cockpit is MyCockpit)
            {
                (this.Cockpit as MyCockpit).RequestUse(actionEnum, user);
            }
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.LocalMatrix * this.Cockpit.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.Cockpit.WorldMatrix;

        public override uint RenderObjectID =>
            this.Cockpit.Render.GetRenderObjectID();

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions =>
            (this.PrimaryAction | this.SecondaryAction);

        public override UseActionEnum PrimaryAction =>
            UseActionEnum.Manipulate;

        public override UseActionEnum SecondaryAction =>
            UseActionEnum.None;

        public override bool ContinuousUsage =>
            false;

        public override bool PlayIndicatorSound =>
            (!(this.Cockpit is MyShipController) || (this.Cockpit as MyShipController).PlayDefaultUseSound);
    }
}

