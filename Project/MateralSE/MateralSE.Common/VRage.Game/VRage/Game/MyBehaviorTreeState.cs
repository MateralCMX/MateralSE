namespace VRage.Game
{
    using System;

    public enum MyBehaviorTreeState : sbyte
    {
        ERROR = -1,
        NOT_TICKED = 0,
        SUCCESS = 1,
        FAILURE = 2,
        RUNNING = 3
    }
}

