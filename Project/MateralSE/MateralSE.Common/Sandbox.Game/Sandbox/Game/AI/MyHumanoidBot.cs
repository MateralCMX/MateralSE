namespace Sandbox.Game.AI
{
    using Sandbox.Definitions;
    using Sandbox.Game.AI.Actions;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyBotType(typeof(MyObjectBuilder_HumanoidBot))]
    public class MyHumanoidBot : MyAgentBot
    {
        public MyHumanoidBot(MyPlayer player, MyBotDefinition botDefinition) : base(player, botDefinition)
        {
        }

        public override void DebugDraw()
        {
            base.DebugDraw();
            if (this.HumanoidEntity != null)
            {
                this.HumanoidActions.AiTargetBase.DebugDraw();
                MatrixD xd = this.HumanoidEntity.GetHeadMatrix(true, true, false, true, false);
                if (this.HumanoidActions.AiTargetBase.HasTarget())
                {
                    Vector3D vectord;
                    float num;
                    this.HumanoidActions.AiTargetBase.DrawLineToTarget(xd.Translation);
                    this.HumanoidActions.AiTargetBase.GetTargetPosition(xd.Translation, out vectord, out num);
                    if (vectord != Vector3D.Zero)
                    {
                        MyRenderProxy.DebugDrawSphere(vectord, 0.3f, Color.Red, 0.4f, false, false, true, false);
                        MyRenderProxy.DebugDrawText3D(vectord, "GetTargetPosition", Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                }
                MyRenderProxy.DebugDrawAxis(this.HumanoidEntity.PositionComp.WorldMatrix, 1f, false, false, false);
                MatrixD xd2 = xd;
                xd2.Translation = Vector3.Zero;
                Matrix.Transpose((Matrix) xd2).Translation = xd.Translation;
            }
        }

        public MyCharacter HumanoidEntity =>
            base.AgentEntity;

        public MyHumanoidBotActions HumanoidActions =>
            (base.m_actions as MyHumanoidBotActions);

        public MyHumanoidBotDefinition HumanoidDefinition =>
            (base.m_botDefinition as MyHumanoidBotDefinition);

        public MyHumanoidBotLogic HumanoidLogic =>
            (base.AgentLogic as MyHumanoidBotLogic);

        public override bool IsValidForUpdate =>
            base.IsValidForUpdate;

        protected MyDefinitionId StartingWeaponId
        {
            get
            {
                if (this.HumanoidDefinition != null)
                {
                    return this.HumanoidDefinition.StartingWeaponDefinitionId;
                }
                return new MyDefinitionId();
            }
        }
    }
}

