namespace VRage.Game.AI
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited=true)]
    public class MyBehaviorTreeActionAttribute : Attribute
    {
        public readonly string ActionName;
        public readonly MyBehaviorTreeActionType ActionType;
        public bool ReturnsRunning;

        public MyBehaviorTreeActionAttribute(string actionName) : this(actionName, MyBehaviorTreeActionType.BODY)
        {
        }

        public MyBehaviorTreeActionAttribute(string actionName, MyBehaviorTreeActionType type)
        {
            this.ActionName = actionName;
            this.ActionType = type;
            this.ReturnsRunning = true;
        }
    }
}

