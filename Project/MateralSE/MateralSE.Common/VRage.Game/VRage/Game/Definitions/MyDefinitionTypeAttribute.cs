namespace VRage.Game.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyDefinitionTypeAttribute : MyFactoryTagAttribute
    {
        public readonly Type PostProcessor;

        public MyDefinitionTypeAttribute(Type objectBuilderType, Type postProcessor = null) : base(objectBuilderType, true)
        {
            if (postProcessor == null)
            {
                postProcessor = typeof(NullDefinitionPostprocessor);
            }
            else if (!typeof(MyDefinitionPostprocessor).IsAssignableFrom(postProcessor))
            {
                throw new ArgumentException("postProcessor processor must be a subclass of MyDefinitionPostprocessor.", "postProcessor");
            }
            this.PostProcessor = postProcessor;
        }
    }
}

