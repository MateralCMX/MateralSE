namespace VRage.Game.AI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class MyBehaviorDescriptorAttribute : Attribute
    {
        public readonly string DescriptorCategory;

        public MyBehaviorDescriptorAttribute(string category)
        {
            this.DescriptorCategory = category;
        }
    }
}

