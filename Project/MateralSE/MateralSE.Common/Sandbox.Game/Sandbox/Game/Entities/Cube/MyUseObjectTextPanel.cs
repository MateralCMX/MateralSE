namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("textpanel")]
    public class MyUseObjectTextPanel : MyUseObjectBase
    {
        private MyTextPanel m_textPanel;
        private Matrix m_localMatrix;

        public MyUseObjectTextPanel(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.m_textPanel = (MyTextPanel) owner;
            this.m_localMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            if (actionEnum == UseActionEnum.Manipulate)
            {
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToShowScreen
                };
                description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]" };
                description.IsTextControlHint = true;
                description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]" };
                return description;
            }
            if (actionEnum != UseActionEnum.OpenTerminal)
            {
                description = new MyActionDescription {
                    Text = MySpaceTexts.NotificationHintPressToOpenTerminal
                };
                description.FormatParams = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) };
                description.IsTextControlHint = true;
                return description;
            }
            description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToOpenTerminal
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]" };
            description.IsTextControlHint = true;
            description.JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenTerminal);
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity user)
        {
            this.m_textPanel.Use(actionEnum, user);
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.m_localMatrix * this.m_textPanel.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.m_textPanel.WorldMatrix;

        public override uint RenderObjectID
        {
            get
            {
                if (this.m_textPanel.Render == null)
                {
                    return uint.MaxValue;
                }
                uint[] renderObjectIDs = this.m_textPanel.Render.RenderObjectIDs;
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
                UseActionEnum none = UseActionEnum.None;
                if (this.m_textPanel.GetPlayerRelationToOwner() != MyRelationsBetweenPlayerAndBlock.Enemies)
                {
                    none = this.PrimaryAction | this.SecondaryAction;
                }
                return none;
            }
        }

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

