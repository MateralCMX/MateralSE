namespace Sandbox.Game.Gui
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Valve.VR;
    using VRage.Input;
    using VRage.OpenVRWrapper;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyVRDebugInputComponent : MyDebugComponent
    {
        private Vector2 m_c1touch = new Vector2(0f, 0f);
        private Vector2 m_c2touch = new Vector2(0f, 0f);
        private StringBuilder sb = new StringBuilder();
        private Compositor_FrameTiming m_timing;
        private bool m_freezeTiming;
        private bool m_logTiming;

        public MyVRDebugInputComponent()
        {
            Static = this;
        }

        public override void Draw()
        {
            base.Draw();
            MyRenderProxy.DebugDrawText2D(new Vector2(50f, 50f), "LMU (ctrl *):" + MyOpenVR.LmuDebugOnOff.ToString(), Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(250f, 50f), "2D (ctrl /):" + MyOpenVR.Debug2DImage.ToString(), Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(450f, 50f), "SYNC (ctrl ,):" + MyOpenVR.SyncWait.ToString(), Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(100f, 80f), "Missed frames: " + MyOpenVR.MissedFramesCount, Color.Purple, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(400f, 80f), "Wait ms: " + MyOpenVR.WaitTimeMs.ToString("00.00"), Color.MediumPurple, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(100f, 100f), "IPD (hmd wheel) =" + (MyOpenVR.Ipd_2 * 2f).ToString(), Color.Green, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(100f, 150f), "C1 angles:" + Vector3.Multiply(MyMath.QuaternionToEuler(Quaternion.CreateFromRotationMatrix(MyOpenVR.Controller1Matrix)), (float) 57.29578f).ToString(), Color.Gray, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            this.sb.Clear();
            for (int i = 0; i < 0x40; i++)
            {
                if (MyOpenVR.Controller1State.WasButtonPressed((EVRButtonId) i))
                {
                    this.sb.Append("+" + i.ToString() + " ");
                }
                if (MyOpenVR.Controller1State.WasButtonReleased((EVRButtonId) i))
                {
                    this.sb.Append("-" + i.ToString() + " ");
                }
                if (MyOpenVR.Controller1State.IsButtonPressed((EVRButtonId) i))
                {
                    this.sb.Append(MyOpenVR.GetButtonName(i));
                    this.sb.Append(" ");
                }
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(100f, 170f), this.sb.ToString(), Color.Yellow, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            if (MyOpenVR.Controller1State.GetTouchpadXY(ref this.m_c1touch))
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(100f, 190f), "C1 touchpad:" + this.m_c1touch.X.ToString("0.00") + ", " + this.m_c1touch.Y.ToString("0.00"), Color.RosyBrown, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            if (MyOpenVR.Controller2State.GetTouchpadXY(ref this.m_c2touch))
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(100f, 210f), "C2 touchpad:" + this.m_c2touch.X.ToString("0.00") + ", " + this.m_c2touch.Y.ToString("0.00"), Color.RosyBrown, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(100f, 230f), "C1 analog trigger: " + MyOpenVR.Controller1State.GetAnalogTrigger().ToString("0.00"), Color.Yellow, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            this.DrawFrameTiming();
        }

        private void DrawFrameTiming()
        {
            int num = 250;
            if (!this.m_freezeTiming)
            {
                MyOpenVR.GetFrameTiming(ref this.m_timing, 0);
            }
            foreach (FieldInfo info in this.m_timing.GetType().GetFields())
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(50f, (float) (num += 10)), info.Name + ": " + info.GetValue(this.m_timing), Color.NavajoWhite, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                if (this.m_logTiming)
                {
                    MyLog.Default.WriteLine(info.Name + ": " + info.GetValue(this.m_timing));
                }
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(50f, (float) (num += 10)), "freeze (ctrl -): " + this.m_freezeTiming.ToString() + "  to console (ctrl +): " + this.m_logTiming.ToString(), Color.White, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
        }

        public override string GetName() => 
            "VR";

        public override bool HandleInput()
        {
            if (MyInput.Static.IsKeyPress(MyKeys.Control))
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                {
                    this.m_logTiming = !this.m_logTiming;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                {
                    this.m_freezeTiming = !this.m_freezeTiming;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply))
                {
                    MyOpenVR.LmuDebugOnOff = !MyOpenVR.LmuDebugOnOff;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Divide))
                {
                    MyOpenVR.Debug2DImage = !MyOpenVR.Debug2DImage;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Decimal))
                {
                    MyOpenVR.SyncWait = !MyOpenVR.SyncWait;
                }
            }
            return false;
        }

        public static MyVRDebugInputComponent Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }
    }
}

