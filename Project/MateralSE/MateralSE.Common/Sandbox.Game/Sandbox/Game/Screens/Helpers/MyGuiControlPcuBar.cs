namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlPcuBar : MyGuiControlParent
    {
        private MyGuiControlLabel m_PCULabel;
        private MyGuiControlLabel m_PCUCost;
        private MyGuiControlLabel m_PCUCountLabel;
        private MyGuiControlImage m_PCUIcon;
        private MyGuiControlImage m_PCULineBG;
        private MyGuiControlImage m_PCULine;
        private int m_maxPCU;
        private int m_currentPCU;
        private int m_currentDisplayedPCU;
        private int m_frameCounterPCU;
        private readonly Vector2 PCU_BAR_WIDTH;

        public MyGuiControlPcuBar(Vector2? position = new Vector2?()) : base(position, nullable, backgroundColor, null)
        {
            this.PCU_BAR_WIDTH = new Vector2(0.32f, 0.007f);
            VRageMath.Vector4? backgroundColor = null;
            base.Size = new Vector2(this.PCU_BAR_WIDTH.X, 0.039f);
            Vector2 vector = -base.Size / 2f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = vector + new Vector2(0.03f, 0.002f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = "PCU:";
            this.m_PCULabel = label1;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = vector + new Vector2(0.07f, 0.002f);
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_PCUCost = label2;
            base.Controls.Add(this.m_PCUCost);
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = vector + new Vector2(base.Size.X - 0.005f, 0.002f);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_PCUCountLabel = label3;
            backgroundColor = null;
            string[] textures = new string[] { @"Textures\GUI\PCU.png" };
            this.m_PCUIcon = new MyGuiControlImage(new Vector2?(vector), new Vector2(0.022f, 0.029f), backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_PCUIcon.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            base.Controls.Add(this.m_PCUIcon);
            backgroundColor = null;
            string[] textArray2 = new string[] { @"Textures\GUI\Icons\HUD 2017\DrillBarBackground.png" };
            this.m_PCULineBG = new MyGuiControlImage(new Vector2?(vector + new Vector2(0f, base.Size.Y)), new Vector2?(this.PCU_BAR_WIDTH), backgroundColor, null, textArray2, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_PCULineBG.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            base.Controls.Add(this.m_PCULineBG);
            backgroundColor = null;
            string[] textArray3 = new string[] { @"Textures\GUI\Icons\HUD 2017\DrillBarProgress.png" };
            this.m_PCULine = new MyGuiControlImage(new Vector2?(vector + new Vector2(0f, base.Size.Y)), new Vector2?(this.PCU_BAR_WIDTH), backgroundColor, null, textArray3, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_PCULine.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            base.Controls.Add(this.m_PCULine);
            base.Controls.Add(this.m_PCULabel);
            base.Controls.Add(this.m_PCUCountLabel);
        }

        public void InitPCU(MyIdentity identity)
        {
            this.m_maxPCU = 0;
            this.m_currentPCU = 0;
            if (identity != null)
            {
                this.m_maxPCU = identity.GetInitialPCU();
                this.m_currentPCU = identity.BlockLimits.PCU;
                this.m_currentDisplayedPCU = this.m_currentPCU;
                this.m_PCUCountLabel.Text = this.m_currentDisplayedPCU.ToString() + " / " + this.m_maxPCU.ToString();
                this.m_PCULine.Size = new Vector2((this.m_maxPCU != 0) ? Math.Min((this.PCU_BAR_WIDTH.X / ((float) this.m_maxPCU)) * this.m_currentDisplayedPCU, this.PCU_BAR_WIDTH.X) : 0f, this.PCU_BAR_WIDTH.Y);
            }
        }

        public void ShowPcuCost(MyCubeBlockDefinition definition)
        {
            if (definition != null)
            {
                this.m_PCUCost.Text = definition.PCU.ToString();
            }
        }

        public void UpdatePCU(MyIdentity identity)
        {
            this.m_maxPCU = 0;
            this.m_currentPCU = 0;
            if (identity != null)
            {
                this.m_maxPCU = identity.GetInitialPCU();
                this.m_currentPCU = identity.BlockLimits.PCU;
            }
            if ((MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE) || (MySession.Static.TotalPCU == 0))
            {
                this.m_PCUCountLabel.Text = MyTexts.Get(MyCommonTexts.Unlimited).ToString();
            }
            else if (this.m_currentDisplayedPCU != this.m_currentPCU)
            {
                int num = Math.Max(1, Math.Abs((int) ((this.m_currentPCU - this.m_currentDisplayedPCU) / 20)));
                this.m_currentDisplayedPCU = (this.m_currentPCU < this.m_currentDisplayedPCU) ? (this.m_currentDisplayedPCU - num) : (this.m_currentDisplayedPCU + num);
                this.m_PCUCountLabel.Text = this.m_currentPCU.ToString() + " / " + this.m_maxPCU.ToString();
                this.m_PCULine.Size = new Vector2(Math.Min((this.PCU_BAR_WIDTH.X / ((float) this.m_maxPCU)) * this.m_currentDisplayedPCU, this.PCU_BAR_WIDTH.X), this.PCU_BAR_WIDTH.Y);
            }
        }
    }
}

