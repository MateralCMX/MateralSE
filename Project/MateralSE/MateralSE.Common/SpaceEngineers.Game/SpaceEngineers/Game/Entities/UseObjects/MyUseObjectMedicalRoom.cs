namespace SpaceEngineers.Game.Entities.UseObjects
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("block")]
    internal class MyUseObjectMedicalRoom : MyUseObjectBase
    {
        private MyMedicalRoom m_medicalRoom;
        private Matrix m_localMatrix;

        public MyUseObjectMedicalRoom(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.m_medicalRoom = (MyMedicalRoom) owner;
            this.m_localMatrix = dummyData.Matrix;
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            switch (actionEnum)
            {
                case UseActionEnum.Manipulate:
                    return new MyActionDescription { 
                        Text = MySpaceTexts.NotificationHintPressToRechargeInMedicalRoom,
                        FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]" },
                        IsTextControlHint = true,
                        JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE) + "]" }
                    };

                case UseActionEnum.OpenTerminal:
                    return new MyActionDescription { 
                        Text = MySpaceTexts.NotificationHintPressToOpenTerminal,
                        FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) + "]" },
                        IsTextControlHint = true,
                        JoystickText = new MyStringId?(MySpaceTexts.NotificationHintJoystickPressToOpenTerminal)
                    };
            }
            return new MyActionDescription { 
                Text = MySpaceTexts.NotificationHintPressToOpenTerminal,
                FormatParams = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL) },
                IsTextControlHint = true
            };
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyCharacter user = entity as MyCharacter;
            this.m_medicalRoom.Use(actionEnum, user);
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.m_localMatrix * this.m_medicalRoom.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.m_medicalRoom.WorldMatrix;

        public override uint RenderObjectID
        {
            get
            {
                uint[] renderObjectIDs = this.m_medicalRoom.Render.RenderObjectIDs;
                if (renderObjectIDs.Length > 0)
                {
                    return renderObjectIDs[0];
                }
                return uint.MaxValue;
            }
        }

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions =>
            (UseActionEnum.OpenTerminal | UseActionEnum.Manipulate);

        public override bool ContinuousUsage =>
            true;

        public override bool PlayIndicatorSound =>
            true;
    }
}

