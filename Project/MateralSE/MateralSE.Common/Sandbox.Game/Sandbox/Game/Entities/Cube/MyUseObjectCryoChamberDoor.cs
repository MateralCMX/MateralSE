namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("cryopod")]
    internal class MyUseObjectCryoChamberDoor : MyUseObjectBase
    {
        public readonly MyCryoChamber CryoChamber;
        public readonly Matrix LocalMatrix;

        public MyUseObjectCryoChamberDoor(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.CryoChamber = owner as MyCryoChamber;
            this.LocalMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToEnterCryochamber
            };
            description.FormatParams = new object[] { "[" + MyGuiSandbox.GetKeyName(MyControlsSpace.USE) + "]", this.CryoChamber.DisplayNameText };
            description.IsTextControlHint = true;
            description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]", this.CryoChamber.DisplayNameText };
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
            this.CryoChamber.RequestUse(actionEnum, user);
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.LocalMatrix * this.CryoChamber.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.CryoChamber.WorldMatrix;

        public override uint RenderObjectID =>
            this.CryoChamber.Render.GetRenderObjectID();

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
            true;
    }
}

