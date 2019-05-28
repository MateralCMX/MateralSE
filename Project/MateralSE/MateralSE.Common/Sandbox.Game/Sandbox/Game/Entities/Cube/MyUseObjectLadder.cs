namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("ladder")]
    public class MyUseObjectLadder : MyUseObjectBase
    {
        private MyLadder m_ladder;
        private Matrix m_localMatrix;

        public MyUseObjectLadder(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.m_ladder = (MyLadder) owner;
            this.m_localMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToGetOnLadder
            };
            description.FormatParams = new object[] { "[" + MyGuiSandbox.GetKeyName(MyControlsSpace.USE) + "]" };
            description.IsTextControlHint = true;
            description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]" };
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity user)
        {
            this.m_ladder.Use(actionEnum, user);
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.m_localMatrix * this.m_ladder.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.m_ladder.WorldMatrix;

        public override uint RenderObjectID
        {
            get
            {
                if (this.m_ladder.Render == null)
                {
                    return uint.MaxValue;
                }
                uint[] renderObjectIDs = this.m_ladder.Render.RenderObjectIDs;
                return ((renderObjectIDs.Length == 0) ? uint.MaxValue : renderObjectIDs[0]);
            }
        }

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

