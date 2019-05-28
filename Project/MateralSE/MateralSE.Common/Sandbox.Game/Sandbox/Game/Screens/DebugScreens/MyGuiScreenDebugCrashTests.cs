namespace Sandbox.Game.Screens.DebugScreens
{
    using Havok;
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("VRage", "Crash tests")]
    internal class MyGuiScreenDebugCrashTests : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugCrashTests() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void Allocate(object state = null)
        {
            List<byte[]> list = new List<byte[]>();
            int num = 0;
            while (num < 0x989680)
            {
                byte[] item = new byte[0xfa000];
                int index = 0;
                while (true)
                {
                    if (index >= item.Length)
                    {
                        list.Add(item);
                        num++;
                        break;
                    }
                    item[index] = (byte) (index ^ list.Count);
                    index++;
                }
            }
            Console.WriteLine(list.Count);
        }

        private void DivideByZero(MyGuiControlButton sender)
        {
            int num = 7;
            Console.WriteLine((int) (14 / (14 - (2 * num))));
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugCrashTests";

        private void HavokAccessViolation(object state = null)
        {
            Console.WriteLine(new HkRigidBodyCinfo().LinearVelocity);
        }

        private void HavokAccessViolationException(MyGuiControlButton sender)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.HavokAccessViolation));
        }

        private void MainThreadCrasher()
        {
            throw new InvalidOperationException("Forced exception");
        }

        private void MainThreadInvokedException(MyGuiControlButton sender)
        {
            MySandboxGame.Static.Invoke(new Action(this.MainThreadCrasher), "DebugCrashTest");
        }

        private void OutOfMemoryUpdateException(MyGuiControlButton sender)
        {
            this.Allocate(null);
        }

        private void OutOfMemoryWorkerException(MyGuiControlButton sender)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.Allocate));
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Crash tests", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Exception in update thread."), new Action<MyGuiControlButton>(this.UpdateThreadException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Exception in render thread."), new Action<MyGuiControlButton>(this.RenderThreadException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Exception in worker thread."), new Action<MyGuiControlButton>(this.WorkerThreadException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Main thread invoked exception."), new Action<MyGuiControlButton>(this.MainThreadInvokedException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Update thread out of memory."), new Action<MyGuiControlButton>(this.OutOfMemoryUpdateException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Worker thread out of memory."), new Action<MyGuiControlButton>(this.OutOfMemoryWorkerException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Havok access violation."), new Action<MyGuiControlButton>(this.HavokAccessViolationException), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Divide by zero."), new Action<MyGuiControlButton>(this.DivideByZero), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Unsupported GPU."), new Action<MyGuiControlButton>(this.UnsupportedGPU), null, textColor, captionOffset, true, true);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }

        private void RenderThreadException(MyGuiControlButton sender)
        {
            MyRenderProxy.DebugCrashRenderThread();
        }

        private void UnsupportedGPU(MyGuiControlButton sender)
        {
            new Result(0x887a0004).CheckError();
        }

        private void UpdateThreadException(MyGuiControlButton sender)
        {
            throw new InvalidOperationException("Forced exception");
        }

        private void WorkerThreadCrasher(object state)
        {
            Thread.Sleep(0x7d0);
            throw new InvalidOperationException("Forced exception");
        }

        private void WorkerThreadException(MyGuiControlButton sender)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.WorkerThreadCrasher));
        }
    }
}

