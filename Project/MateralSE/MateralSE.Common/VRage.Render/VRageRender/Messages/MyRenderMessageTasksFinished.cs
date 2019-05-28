namespace VRageRender.Messages
{
    internal class MyRenderMessageTasksFinished : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.TasksFinished;
    }
}

